namespace EnrollmentDashboard.Models;

public class Enrollment
{
    public int EnrollmentID { get; set; }
    public int ParticipantID { get; set; }
    public int ProgramID { get; set; }
    public DateTime EnrollmentDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
