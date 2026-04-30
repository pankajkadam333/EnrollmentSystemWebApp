using EnrollmentDashboard.DataAccess;

namespace EnrollmentDashboard.Tests.DataAccess;

public class InMemoryEnrollmentRepositoryTests
{
    private readonly InMemoryEnrollmentRepository _repo = new();

    // ── GetEnrollmentsAsync ──

    [Fact]
    public async Task GetEnrollments_NoFilters_ReturnsAllRecords()
    {
        var (enrollments, total) = await _repo.GetEnrollmentsAsync(null, null, null, 1, 50);

        Assert.Equal(25, total);
        Assert.Equal(25, enrollments.Count);
    }

    [Fact]
    public async Task GetEnrollments_Pagination_ReturnsCorrectPage()
    {
        var (page1, total) = await _repo.GetEnrollmentsAsync(null, null, null, 1, 10);
        var (page2, _) = await _repo.GetEnrollmentsAsync(null, null, null, 2, 10);
        var (page3, _) = await _repo.GetEnrollmentsAsync(null, null, null, 3, 10);

        Assert.Equal(25, total);
        Assert.Equal(10, page1.Count);
        Assert.Equal(10, page2.Count);
        Assert.Equal(5, page3.Count);
    }

    [Fact]
    public async Task GetEnrollments_FilterByActiveStatus_ReturnsOnlyActive()
    {
        var (enrollments, total) = await _repo.GetEnrollmentsAsync(null, null, "Active", 1, 50);

        Assert.Equal(10, total);
        Assert.All(enrollments, e => Assert.Equal("Active", e.Status));
    }

    [Fact]
    public async Task GetEnrollments_FilterByCompletedStatus_ReturnsOnlyCompleted()
    {
        var (enrollments, total) = await _repo.GetEnrollmentsAsync(null, null, "Completed", 1, 50);

        Assert.Equal(12, total);
        Assert.All(enrollments, e => Assert.Equal("Completed", e.Status));
    }

    [Fact]
    public async Task GetEnrollments_FilterByWithdrawnStatus_ReturnsOnlyWithdrawn()
    {
        var (enrollments, total) = await _repo.GetEnrollmentsAsync(null, null, "Withdrawn", 1, 50);

        Assert.Equal(3, total);
        Assert.All(enrollments, e => Assert.Equal("Withdrawn", e.Status));
    }

    [Fact]
    public async Task GetEnrollments_FilterByDateRange_ReturnsMatchingRecords()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 1, 31);

        var (enrollments, total) = await _repo.GetEnrollmentsAsync(start, end, null, 1, 50);

        Assert.True(total > 0);
        Assert.All(enrollments, e =>
        {
            Assert.True(e.EnrollmentDate >= start);
            Assert.True(e.EnrollmentDate <= end);
        });
    }

    [Fact]
    public async Task GetEnrollments_FilterByStartDateOnly_ReturnsFromThatDate()
    {
        var start = new DateTime(2026, 4, 1);

        var (enrollments, _) = await _repo.GetEnrollmentsAsync(start, null, null, 1, 50);

        Assert.All(enrollments, e => Assert.True(e.EnrollmentDate >= start));
    }

    [Fact]
    public async Task GetEnrollments_FilterByEndDateOnly_ReturnsUpToThatDate()
    {
        var end = new DateTime(2025, 1, 31);

        var (enrollments, _) = await _repo.GetEnrollmentsAsync(null, end, null, 1, 50);

        Assert.All(enrollments, e => Assert.True(e.EnrollmentDate <= end));
    }

    [Fact]
    public async Task GetEnrollments_CombinedFilters_DateAndStatus()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 12, 31);

        var (enrollments, _) = await _repo.GetEnrollmentsAsync(start, end, "Active", 1, 50);

        Assert.All(enrollments, e =>
        {
            Assert.Equal("Active", e.Status);
            Assert.True(e.EnrollmentDate >= start);
            Assert.True(e.EnrollmentDate <= end);
        });
    }

    [Fact]
    public async Task GetEnrollments_OrderedByDateDescending()
    {
        var (enrollments, _) = await _repo.GetEnrollmentsAsync(null, null, null, 1, 50);

        for (int i = 1; i < enrollments.Count; i++)
        {
            Assert.True(enrollments[i - 1].EnrollmentDate >= enrollments[i].EnrollmentDate);
        }
    }

    [Fact]
    public async Task GetEnrollments_NoMatchingFilters_ReturnsEmpty()
    {
        var start = new DateTime(2030, 1, 1);
        var end = new DateTime(2030, 12, 31);

        var (enrollments, total) = await _repo.GetEnrollmentsAsync(start, end, null, 1, 50);

        Assert.Empty(enrollments);
        Assert.Equal(0, total);
    }

    [Fact]
    public async Task GetEnrollments_XssParticipant_ReturnsRawData()
    {
        // Enrollment #5 belongs to participant #6 who has <script> in last name
        var (enrollments, _) = await _repo.GetEnrollmentsAsync(null, null, null, 1, 50);
        var xssEnrollment = enrollments.FirstOrDefault(e => e.EnrollmentID == 5);

        Assert.NotNull(xssEnrollment);
        Assert.Contains("<script>", xssEnrollment.LastName);
        Assert.Contains("<img", xssEnrollment.Notes);
    }

    // ── GetEnrollmentSummaryAsync ──

    [Fact]
    public async Task GetSummary_NoFilters_ReturnsTotals()
    {
        var summary = await _repo.GetEnrollmentSummaryAsync(null, null);

        Assert.Equal(25, summary.TotalEnrollments);
        Assert.Equal(10, summary.ActiveEnrollments);
        Assert.Equal(12, summary.CompletedEnrollments);
        Assert.Equal(3, summary.WithdrawnEnrollments);
    }

    [Fact]
    public async Task GetSummary_WithDateFilter_ReturnsFilteredTotals()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 12, 31);

        var summary = await _repo.GetEnrollmentSummaryAsync(start, end);

        Assert.True(summary.TotalEnrollments > 0);
        Assert.Equal(
            summary.TotalEnrollments,
            summary.ActiveEnrollments + summary.CompletedEnrollments + summary.WithdrawnEnrollments);
    }

    [Fact]
    public async Task GetSummary_FutureDateRange_ReturnsZeros()
    {
        var summary = await _repo.GetEnrollmentSummaryAsync(new DateTime(2030, 1, 1), new DateTime(2030, 12, 31));

        Assert.Equal(0, summary.TotalEnrollments);
        Assert.Equal(0, summary.ActiveEnrollments);
        Assert.Equal(0, summary.CompletedEnrollments);
        Assert.Equal(0, summary.WithdrawnEnrollments);
    }

    // ── GetEnrollmentDetailAsync ──

    [Fact]
    public async Task GetDetail_ValidId_ReturnsFullDetail()
    {
        var detail = await _repo.GetEnrollmentDetailAsync(1);

        Assert.NotNull(detail);
        Assert.Equal(1, detail.EnrollmentID);
        Assert.Equal("John", detail.FirstName);
        Assert.Equal("Doe", detail.LastName);
        Assert.Equal("Leadership Development", detail.ProgramName);
        Assert.Equal("Active", detail.Status);
    }

    [Fact]
    public async Task GetDetail_NonExistentId_ReturnsNull()
    {
        var detail = await _repo.GetEnrollmentDetailAsync(9999);

        Assert.Null(detail);
    }

    [Fact]
    public async Task GetDetail_XssEnrollment_ContainsRawScriptData()
    {
        var detail = await _repo.GetEnrollmentDetailAsync(5);

        Assert.NotNull(detail);
        Assert.Contains("<script>", detail.LastName);
        Assert.Contains("<img", detail.Notes);
    }

    [Fact]
    public async Task GetDetail_CompletedEnrollment_HasCompletionDate()
    {
        var detail = await _repo.GetEnrollmentDetailAsync(11);

        Assert.NotNull(detail);
        Assert.Equal("Completed", detail.Status);
        Assert.NotNull(detail.CompletionDate);
    }

    [Fact]
    public async Task GetDetail_ActiveEnrollment_HasNullCompletionDate()
    {
        var detail = await _repo.GetEnrollmentDetailAsync(1);

        Assert.NotNull(detail);
        Assert.Equal("Active", detail.Status);
        Assert.Null(detail.CompletionDate);
    }
}
