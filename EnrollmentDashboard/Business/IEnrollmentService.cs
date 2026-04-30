using EnrollmentDashboard.Models.ViewModels;

namespace EnrollmentDashboard.Business;

public interface IEnrollmentService
{
    Task<DashboardViewModel> GetDashboardAsync(DateTime? startDate, DateTime? endDate, string? status, int page);
    Task<EnrollmentDetailViewModel?> GetEnrollmentDetailAsync(int enrollmentId);
}
