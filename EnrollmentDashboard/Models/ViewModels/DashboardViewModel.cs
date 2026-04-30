using System.ComponentModel.DataAnnotations;

namespace EnrollmentDashboard.Models.ViewModels;

public class DashboardViewModel
{
    public EnrollmentSummaryViewModel Summary { get; set; } = new();
    public List<EnrollmentListViewModel> Enrollments { get; set; } = [];

    // Filter inputs
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }

    public string? Status { get; set; }

    // Pagination
    public int CurrentPage { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public int TotalRecords { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);

    public static readonly string[] AllowedStatuses = ["Active", "Completed", "Withdrawn"];
    //This allowed statuses can be configured in config or in DB or can be given choice to user on UI 
    // here its hardcoded intentionally
}
