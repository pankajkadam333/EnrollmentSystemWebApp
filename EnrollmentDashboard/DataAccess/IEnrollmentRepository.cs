using EnrollmentDashboard.Models.ViewModels;

namespace EnrollmentDashboard.DataAccess;

public interface IEnrollmentRepository
{
    Task<(List<EnrollmentListViewModel> Enrollments, int TotalRecords)> GetEnrollmentsAsync(
        DateTime? startDate, DateTime? endDate, string? status, int page, int pageSize);

    Task<EnrollmentSummaryViewModel> GetEnrollmentSummaryAsync(DateTime? startDate, DateTime? endDate);

    Task<EnrollmentDetailViewModel?> GetEnrollmentDetailAsync(int enrollmentId);
}
