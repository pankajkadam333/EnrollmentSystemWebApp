namespace EnrollmentDashboard.Models;

public class Program
{
    public int ProgramID { get; set; }
    public string ProgramName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool Active { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
}
