namespace EnrollmentDashboard.Models.ViewModels;

public class EnrollmentDetailViewModel
{
    public int EnrollmentID { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }

    // Participant info
    public int ParticipantID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public bool ParticipantActive { get; set; }

    // Program info
    public int ProgramID { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string? ProgramDescription { get; set; }

    public string ParticipantFullName => $"{FirstName} {LastName}";
}
