namespace EnrollmentDashboard.Models.ViewModels;

public class EnrollmentListViewModel
{
    public int EnrollmentID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string ProgramName { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int TotalRecords { get; set; }

    public string ParticipantFullName => $"{FirstName} {LastName}";
}
