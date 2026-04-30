using EnrollmentDashboard.Models;
using EnrollmentDashboard.Models.ViewModels;

namespace EnrollmentDashboard.DataAccess;

/// <summary>
/// In-memory repository seeded with the same data from sample-data.sql.
/// Drop-in replacement for EnrollmentRepository when SQL Server is unavailable.
/// </summary>
public class InMemoryEnrollmentRepository : IEnrollmentRepository
{
    private static readonly List<Participant> Participants =
    [
        new() { ParticipantID = 1,  FirstName = "John",        LastName = "Doe",                                    DateOfBirth = new(1985,3,15),  Active = true  },
        new() { ParticipantID = 2,  FirstName = "Jane",        LastName = "Smith",                                  DateOfBirth = new(1990,7,22),  Active = true  },
        new() { ParticipantID = 3,  FirstName = "Michael",     LastName = "Johnson",                                DateOfBirth = new(1978,11,30), Active = true  },
        new() { ParticipantID = 4,  FirstName = "Sarah",       LastName = "Williams",                               DateOfBirth = new(1995,5,18),  Active = true  },
        new() { ParticipantID = 5,  FirstName = "David",       LastName = "Brown",                                  DateOfBirth = new(1982,9,8),   Active = false },
        new() { ParticipantID = 6,  FirstName = "Robert",      LastName = "<script>alert(\"XSS\")</script>",        DateOfBirth = new(1988,4,12),  Active = true  },
        new() { ParticipantID = 7,  FirstName = "Emily",       LastName = "Davis",                                  DateOfBirth = new(1992,6,25),  Active = true  },
        new() { ParticipantID = 8,  FirstName = "James",       LastName = "Wilson",                                 DateOfBirth = new(1980,12,10), Active = true  },
        new() { ParticipantID = 9,  FirstName = "Maria",       LastName = "Garcia",                                 DateOfBirth = new(1987,2,28),  Active = true  },
        new() { ParticipantID = 10, FirstName = "Christopher", LastName = "Martinez",                               DateOfBirth = new(1993,9,14),  Active = true  },
        new() { ParticipantID = 11, FirstName = "Jessica",     LastName = "Anderson",                               DateOfBirth = new(1991,4,3),   Active = true  },
        new() { ParticipantID = 12, FirstName = "Daniel",      LastName = "Taylor",                                 DateOfBirth = new(1984,8,19),  Active = true  },
        new() { ParticipantID = 13, FirstName = "Ashley",      LastName = "Thomas",                                 DateOfBirth = new(1996,1,7),   Active = true  },
        new() { ParticipantID = 14, FirstName = "Matthew",     LastName = "Moore",                                  DateOfBirth = new(1979,11,22), Active = true  },
        new() { ParticipantID = 15, FirstName = "Amanda",      LastName = "Jackson",                                DateOfBirth = new(1989,5,30),  Active = true  },
    ];

    private static readonly List<Models.Program> Programs =
    [
        new() { ProgramID = 1, ProgramName = "Leadership Development",        Description = "Management and leadership skills training",                Active = true  },
        new() { ProgramID = 2, ProgramName = "Technical Skills Workshop",      Description = "Hands-on technology training",                             Active = true  },
        new() { ProgramID = 3, ProgramName = "Communication Bootcamp",         Description = "Professional communication and presentation skills",       Active = true  },
        new() { ProgramID = 4, ProgramName = "Data Analytics Course",          Description = "Introduction to data analysis and visualization",          Active = true  },
        new() { ProgramID = 5, ProgramName = "Project Management Fundamentals",Description = "Core project management concepts and tools",               Active = true  },
        new() { ProgramID = 6, ProgramName = "Advanced Excel Training",        Description = "Advanced spreadsheet analysis and automation",             Active = true  },
        new() { ProgramID = 7, ProgramName = "Public Speaking Mastery",        Description = "Overcome fear and deliver impactful presentations",        Active = true  },
        new() { ProgramID = 8, ProgramName = "Customer Service Excellence",    Description = "Building exceptional customer relationships",              Active = false },
    ];

    private static readonly List<Enrollment> Enrollments =
    [
        // Active
        new() { EnrollmentID = 1,  ParticipantID = 1,  ProgramID = 1, EnrollmentDate = new(2026,1,15),  Status = "Active",    Notes = "Started leadership program" },
        new() { EnrollmentID = 2,  ParticipantID = 1,  ProgramID = 2, EnrollmentDate = new(2026,2,1),   Status = "Active",    Notes = "Enrolled in technical workshop" },
        new() { EnrollmentID = 3,  ParticipantID = 2,  ProgramID = 3, EnrollmentDate = new(2026,1,20),  Status = "Active",    Notes = "Attending weekly sessions" },
        new() { EnrollmentID = 4,  ParticipantID = 3,  ProgramID = 4, EnrollmentDate = new(2026,3,1),   Status = "Active",    Notes = "In progress" },
        new() { EnrollmentID = 5,  ParticipantID = 6,  ProgramID = 1, EnrollmentDate = new(2026,3,20),  Status = "Active",    Notes = "<img src=x onerror=\"alert('XSS')\"> Test note" },
        new() { EnrollmentID = 6,  ParticipantID = 7,  ProgramID = 2, EnrollmentDate = new(2026,4,1),   Status = "Active",    Notes = "Recently started" },
        new() { EnrollmentID = 7,  ParticipantID = 8,  ProgramID = 5, EnrollmentDate = new(2026,3,15),  Status = "Active",    Notes = "Good progress so far" },
        new() { EnrollmentID = 8,  ParticipantID = 9,  ProgramID = 6, EnrollmentDate = new(2026,2,20),  Status = "Active",    Notes = "Learning advanced techniques" },
        new() { EnrollmentID = 9,  ParticipantID = 10, ProgramID = 7, EnrollmentDate = new(2026,4,10),  Status = "Active",    Notes = "Building confidence" },
        new() { EnrollmentID = 10, ParticipantID = 11, ProgramID = 1, EnrollmentDate = new(2026,1,25),  Status = "Active",    Notes = "Strong participant" },

        // Completed
        new() { EnrollmentID = 11, ParticipantID = 2,  ProgramID = 1, EnrollmentDate = new(2025,12,10), CompletionDate = new(2026,3,15), Status = "Completed", Notes = "Successfully completed program" },
        new() { EnrollmentID = 12, ParticipantID = 3,  ProgramID = 2, EnrollmentDate = new(2025,11,5),  CompletionDate = new(2026,1,30), Status = "Completed", Notes = "Completed all modules" },
        new() { EnrollmentID = 13, ParticipantID = 7,  ProgramID = 3, EnrollmentDate = new(2025,10,15), CompletionDate = new(2026,2,28), Status = "Completed", Notes = "Excellent performance" },
        new() { EnrollmentID = 14, ParticipantID = 8,  ProgramID = 4, EnrollmentDate = new(2025,9,1),   CompletionDate = new(2025,12,20),Status = "Completed", Notes = "All requirements met" },
        new() { EnrollmentID = 15, ParticipantID = 11, ProgramID = 2, EnrollmentDate = new(2025,8,15),  CompletionDate = new(2025,11,30),Status = "Completed", Notes = "Outstanding work" },
        new() { EnrollmentID = 16, ParticipantID = 12, ProgramID = 5, EnrollmentDate = new(2025,7,10),  CompletionDate = new(2025,10,15),Status = "Completed", Notes = "Passed final exam" },
        new() { EnrollmentID = 17, ParticipantID = 13, ProgramID = 6, EnrollmentDate = new(2025,6,1),   CompletionDate = new(2025,9,30), Status = "Completed", Notes = "Mastered all concepts" },
        new() { EnrollmentID = 18, ParticipantID = 14, ProgramID = 7, EnrollmentDate = new(2025,5,15),  CompletionDate = new(2025,8,20), Status = "Completed", Notes = "Very impressive" },
        new() { EnrollmentID = 19, ParticipantID = 15, ProgramID = 1, EnrollmentDate = new(2025,4,20),  CompletionDate = new(2025,7,25), Status = "Completed", Notes = "Strong leadership skills" },

        // Withdrawn
        new() { EnrollmentID = 20, ParticipantID = 4,  ProgramID = 1, EnrollmentDate = new(2026,2,14),  CompletionDate = new(2026,3,1),  Status = "Withdrawn", Notes = "Left program early" },
        new() { EnrollmentID = 21, ParticipantID = 12, ProgramID = 1, EnrollmentDate = new(2026,1,10),  CompletionDate = new(2026,2,15), Status = "Withdrawn", Notes = "Personal reasons" },
        new() { EnrollmentID = 22, ParticipantID = 13, ProgramID = 3, EnrollmentDate = new(2025,12,1),  CompletionDate = new(2026,1,5),  Status = "Withdrawn", Notes = "Schedule conflict" },

        // Historical
        new() { EnrollmentID = 23, ParticipantID = 5,  ProgramID = 2, EnrollmentDate = new(2025,1,15),  CompletionDate = new(2025,4,20), Status = "Completed", Notes = "Completed last year" },
        new() { EnrollmentID = 24, ParticipantID = 5,  ProgramID = 3, EnrollmentDate = new(2024,9,10),  CompletionDate = new(2024,12,15),Status = "Completed", Notes = "Old enrollment" },
        new() { EnrollmentID = 25, ParticipantID = 6,  ProgramID = 4, EnrollmentDate = new(2024,6,1),   CompletionDate = new(2024,9,30), Status = "Completed", Notes = "Historical data" },
    ];

    public Task<(List<EnrollmentListViewModel> Enrollments, int TotalRecords)> GetEnrollmentsAsync(
        DateTime? startDate, DateTime? endDate, string? status, int page, int pageSize)
    {
        var query = from e in Enrollments
                    join p in Participants on e.ParticipantID equals p.ParticipantID
                    join pr in Programs on e.ProgramID equals pr.ProgramID
                    select new { e, p, pr };

        if (startDate.HasValue)
            query = query.Where(x => x.e.EnrollmentDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(x => x.e.EnrollmentDate <= endDate.Value);
        if (!string.IsNullOrEmpty(status))
            query = query.Where(x => x.e.Status.Equals(status, StringComparison.OrdinalIgnoreCase));

        var ordered = query.OrderByDescending(x => x.e.EnrollmentDate).ToList();
        int totalRecords = ordered.Count;

        var paged = ordered
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new EnrollmentListViewModel
            {
                EnrollmentID = x.e.EnrollmentID,
                FirstName = x.p.FirstName,
                LastName = x.p.LastName,
                ProgramName = x.pr.ProgramName,
                EnrollmentDate = x.e.EnrollmentDate,
                CompletionDate = x.e.CompletionDate,
                Status = x.e.Status,
                Notes = x.e.Notes,
                TotalRecords = totalRecords
            })
            .ToList();

        return Task.FromResult((paged, totalRecords));
    }

    public Task<EnrollmentSummaryViewModel> GetEnrollmentSummaryAsync(DateTime? startDate, DateTime? endDate)
    {
        var query = Enrollments.AsEnumerable();

        if (startDate.HasValue)
            query = query.Where(e => e.EnrollmentDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(e => e.EnrollmentDate <= endDate.Value);

        var list = query.ToList();

        var summary = new EnrollmentSummaryViewModel
        {
            TotalEnrollments = list.Count,
            ActiveEnrollments = list.Count(e => e.Status == "Active"),
            CompletedEnrollments = list.Count(e => e.Status == "Completed"),
            WithdrawnEnrollments = list.Count(e => e.Status == "Withdrawn")
        };

        return Task.FromResult(summary);
    }

    public Task<EnrollmentDetailViewModel?> GetEnrollmentDetailAsync(int enrollmentId)
    {
        var result = (from e in Enrollments
                      join p in Participants on e.ParticipantID equals p.ParticipantID
                      join pr in Programs on e.ProgramID equals pr.ProgramID
                      where e.EnrollmentID == enrollmentId
                      select new EnrollmentDetailViewModel
                      {
                          EnrollmentID = e.EnrollmentID,
                          EnrollmentDate = e.EnrollmentDate,
                          CompletionDate = e.CompletionDate,
                          Status = e.Status,
                          Notes = e.Notes,
                          ParticipantID = p.ParticipantID,
                          FirstName = p.FirstName,
                          LastName = p.LastName,
                          DateOfBirth = p.DateOfBirth,
                          ParticipantActive = p.Active,
                          ProgramID = pr.ProgramID,
                          ProgramName = pr.ProgramName,
                          ProgramDescription = pr.Description
                      }).FirstOrDefault();

        return Task.FromResult(result);
    }
}
