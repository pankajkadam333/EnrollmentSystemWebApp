using EnrollmentDashboard.Models.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EnrollmentDashboard.DataAccess;

public class EnrollmentRepository : IEnrollmentRepository
{
    private readonly string _connectionString;

    public EnrollmentRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
    }

    /// <summary>
    /// Calls sp_GetEnrollments stored procedure with parameterized inputs.
    /// </summary>
    public async Task<(List<EnrollmentListViewModel> Enrollments, int TotalRecords)> GetEnrollmentsAsync(
        DateTime? startDate, DateTime? endDate, string? status, int page, int pageSize)
    {
        var enrollments = new List<EnrollmentListViewModel>();
        int totalRecords = 0;

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("sp_GetEnrollments", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.Date)
            { Value = startDate.HasValue ? startDate.Value : DBNull.Value });
        command.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.Date)
            { Value = endDate.HasValue ? endDate.Value : DBNull.Value });
        command.Parameters.Add(new SqlParameter("@Status", SqlDbType.NVarChar, 50)
            { Value = !string.IsNullOrEmpty(status) ? status : DBNull.Value });
        command.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = page });
        command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            var enrollment = new EnrollmentListViewModel
            {
                EnrollmentID = reader.GetInt32(reader.GetOrdinal("EnrollmentID")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                ProgramName = reader.GetString(reader.GetOrdinal("ProgramName")),
                EnrollmentDate = reader.GetDateTime(reader.GetOrdinal("EnrollmentDate")),
                CompletionDate = reader.IsDBNull(reader.GetOrdinal("CompletionDate"))
                    ? null : reader.GetDateTime(reader.GetOrdinal("CompletionDate")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                Notes = reader.IsDBNull(reader.GetOrdinal("Notes"))
                    ? null : reader.GetString(reader.GetOrdinal("Notes")),
                TotalRecords = reader.GetInt32(reader.GetOrdinal("TotalRecords"))
            };

            totalRecords = enrollment.TotalRecords;
            enrollments.Add(enrollment);
        }

        return (enrollments, totalRecords);
    }

    /// <summary>
    /// Calls sp_GetEnrollmentSummary stored procedure.
    /// </summary>
    public async Task<EnrollmentSummaryViewModel> GetEnrollmentSummaryAsync(
        DateTime? startDate, DateTime? endDate)
    {
        var summary = new EnrollmentSummaryViewModel();

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand("sp_GetEnrollmentSummary", connection)
        {
            CommandType = CommandType.StoredProcedure
        };

        command.Parameters.Add(new SqlParameter("@StartDate", SqlDbType.Date)
            { Value = startDate.HasValue ? startDate.Value : DBNull.Value });
        command.Parameters.Add(new SqlParameter("@EndDate", SqlDbType.Date)
            { Value = endDate.HasValue ? endDate.Value : DBNull.Value });

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            summary.TotalEnrollments = reader.GetInt32(reader.GetOrdinal("TotalEnrollments"));
            summary.ActiveEnrollments = reader.GetInt32(reader.GetOrdinal("ActiveEnrollments"));
            summary.CompletedEnrollments = reader.GetInt32(reader.GetOrdinal("CompletedEnrollments"));
            summary.WithdrawnEnrollments = reader.GetInt32(reader.GetOrdinal("WithdrawnEnrollments"));
        }

        return summary;
    }

    /// <summary>
    /// Gets enrollment detail using parameterized inline query.
    /// (this inline query can be convert in to store procedure but its been intentionally used here 
    /// to demonstarte the skill to write inline query still avoid SQL injection )
    /// </summary>
    public async Task<EnrollmentDetailViewModel?> GetEnrollmentDetailAsync(int enrollmentId)
    {
        const string sql = """
            SELECT
                e.EnrollmentID, e.EnrollmentDate, e.CompletionDate, e.Status, e.Notes,
                p.ParticipantID, p.FirstName, p.LastName, p.DateOfBirth, p.Active AS ParticipantActive,
                pr.ProgramID, pr.ProgramName, pr.Description AS ProgramDescription
            FROM Enrollments e
            INNER JOIN Participants p ON e.ParticipantID = p.ParticipantID
            INNER JOIN Programs pr ON e.ProgramID = pr.ProgramID
            WHERE e.EnrollmentID = @EnrollmentID
            """;

        await using var connection = new SqlConnection(_connectionString);
        await using var command = new SqlCommand(sql, connection);
        command.Parameters.Add(new SqlParameter("@EnrollmentID", SqlDbType.Int) { Value = enrollmentId });

        await connection.OpenAsync();
        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new EnrollmentDetailViewModel
        {
            EnrollmentID = reader.GetInt32(reader.GetOrdinal("EnrollmentID")),
            EnrollmentDate = reader.GetDateTime(reader.GetOrdinal("EnrollmentDate")),
            CompletionDate = reader.IsDBNull(reader.GetOrdinal("CompletionDate"))
                ? null : reader.GetDateTime(reader.GetOrdinal("CompletionDate")),
            Status = reader.GetString(reader.GetOrdinal("Status")),
            Notes = reader.IsDBNull(reader.GetOrdinal("Notes"))
                ? null : reader.GetString(reader.GetOrdinal("Notes")),
            ParticipantID = reader.GetInt32(reader.GetOrdinal("ParticipantID")),
            FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
            LastName = reader.GetString(reader.GetOrdinal("LastName")),
            DateOfBirth = reader.GetDateTime(reader.GetOrdinal("DateOfBirth")),
            ParticipantActive = reader.GetBoolean(reader.GetOrdinal("ParticipantActive")),
            ProgramID = reader.GetInt32(reader.GetOrdinal("ProgramID")),
            ProgramName = reader.GetString(reader.GetOrdinal("ProgramName")),
            ProgramDescription = reader.IsDBNull(reader.GetOrdinal("ProgramDescription"))
                ? null : reader.GetString(reader.GetOrdinal("ProgramDescription"))
        };
    }
}
