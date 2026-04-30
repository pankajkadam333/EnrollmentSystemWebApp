namespace EnrollmentDashboard.Models.ViewModels;

public class EnrollmentSummaryViewModel
{
    public int TotalEnrollments { get; set; }
    public int ActiveEnrollments { get; set; }
    public int CompletedEnrollments { get; set; }
    public int WithdrawnEnrollments { get; set; }
}
