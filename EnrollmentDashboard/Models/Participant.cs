namespace EnrollmentDashboard.Models;

public class Participant
{
    public int ParticipantID { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }

    public string FullName => $"{FirstName} {LastName}";
}
