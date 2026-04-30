using EnrollmentDashboard.DataAccess;
using EnrollmentDashboard.Models.ViewModels;

namespace EnrollmentDashboard.Business;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _repository;
    private readonly ILogger<EnrollmentService> _logger;
    private const int PageSize = 10;

    public EnrollmentService(IEnrollmentRepository repository, ILogger<EnrollmentService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<DashboardViewModel> GetDashboardAsync(
        DateTime? startDate, DateTime? endDate, string? status, int page)
    {
        _logger.LogInformation("Dashboard requested — StartDate={StartDate}, EndDate={EndDate}, Status={Status}, Page={Page}",
            startDate, endDate, status, page);

        // Validate date range
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            _logger.LogWarning("Date range reversed — swapping StartDate={StartDate} and EndDate={EndDate}", startDate, endDate);
            (startDate, endDate) = (endDate, startDate);
        }

        // Validate status against allowed values
        if (!string.IsNullOrEmpty(status) &&
            !DashboardViewModel.AllowedStatuses.Contains(status, StringComparer.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Invalid status value rejected: {Status}", status);
            status = null;
        }

        // Clamp page number
        if (page < 1)
        {
            _logger.LogWarning("Invalid page number {Page}, clamping to 1", page);
            page = 1;
        }

        var (enrollments, totalRecords) = await _repository.GetEnrollmentsAsync(
            startDate, endDate, status, page, PageSize);

        var summary = await _repository.GetEnrollmentSummaryAsync(startDate, endDate);

        _logger.LogInformation("Dashboard loaded — {Count} enrollments on page {Page}, {Total} total records",
            enrollments.Count, page, totalRecords);

        return new DashboardViewModel
        {
            Enrollments = enrollments,
            Summary = summary,
            StartDate = startDate,
            EndDate = endDate,
            Status = status,
            CurrentPage = page,
            PageSize = PageSize,
            TotalRecords = totalRecords
        };
    }

    public async Task<EnrollmentDetailViewModel?> GetEnrollmentDetailAsync(int enrollmentId)
    {
        if (enrollmentId <= 0)
        {
            _logger.LogWarning("Detail requested with invalid ID: {EnrollmentId}", enrollmentId);
            return null;
        }

        var result = await _repository.GetEnrollmentDetailAsync(enrollmentId);

        if (result is null)
            _logger.LogWarning("Enrollment not found for ID: {EnrollmentId}", enrollmentId);
        else
            _logger.LogInformation("Enrollment detail loaded for ID: {EnrollmentId}", enrollmentId);

        return result;
    }
}
