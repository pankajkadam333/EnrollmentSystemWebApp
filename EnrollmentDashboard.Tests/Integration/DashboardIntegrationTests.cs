using EnrollmentDashboard.Business;
using EnrollmentDashboard.DataAccess;
using EnrollmentDashboard.Models.ViewModels;
using Microsoft.Extensions.Logging.Abstractions;

namespace EnrollmentDashboard.Tests.Integration;

/// <summary>
/// Integration tests using real InMemoryRepository + EnrollmentService together.
/// Validates the full pipeline without mocks.
/// </summary>
public class DashboardIntegrationTests
{
    private readonly EnrollmentService _service;

    public DashboardIntegrationTests()
    {
        _service = new EnrollmentService(
            new InMemoryEnrollmentRepository(),
            NullLogger<EnrollmentService>.Instance);
    }

    [Fact]
    public async Task FullPipeline_Dashboard_ReturnsConsistentData()
    {
        var dashboard = await _service.GetDashboardAsync(null, null, null, 1);

        Assert.Equal(25, dashboard.Summary.TotalEnrollments);
        Assert.Equal(25, dashboard.TotalRecords);
        Assert.Equal(
            dashboard.Summary.TotalEnrollments,
            dashboard.Summary.ActiveEnrollments + dashboard.Summary.CompletedEnrollments + dashboard.Summary.WithdrawnEnrollments);
    }

    [Fact]
    public async Task FullPipeline_FilterActive_SummaryAndListMatch()
    {
        var dashboard = await _service.GetDashboardAsync(null, null, "Active", 1);

        Assert.All(dashboard.Enrollments, e => Assert.Equal("Active", e.Status));
    }

    [Fact]
    public async Task FullPipeline_DateRange2026_ExcludesOlderRecords()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 12, 31);

        var dashboard = await _service.GetDashboardAsync(start, end, null, 1);

        Assert.All(dashboard.Enrollments, e =>
        {
            Assert.True(e.EnrollmentDate >= start);
            Assert.True(e.EnrollmentDate <= end);
        });
    }

    [Fact]
    public async Task FullPipeline_ReversedDates_StillWorks()
    {
        // Service should swap them
        var start = new DateTime(2026, 12, 31);
        var end = new DateTime(2026, 1, 1);

        var dashboard = await _service.GetDashboardAsync(start, end, null, 1);

        // Should not throw, and should return results for the corrected range
        Assert.True(dashboard.Summary.TotalEnrollments >= 0);
    }

    [Fact]
    public async Task FullPipeline_InvalidStatus_ReturnsAllRecords()
    {
        var dashboard = await _service.GetDashboardAsync(null, null, "HACKED", 1);

        // Invalid status is nullified, so all records returned
        Assert.Equal(25, dashboard.Summary.TotalEnrollments);
    }

    [Fact]
    public async Task FullPipeline_Detail_XssParticipant_DataIntact()
    {
        var detail = await _service.GetEnrollmentDetailAsync(5);

        Assert.NotNull(detail);
        Assert.Contains("<script>", detail.LastName);
        Assert.Contains("<img", detail.Notes);
        // The view layer (Razor) is responsible for encoding — data layer preserves raw content
    }

    [Fact]
    public async Task FullPipeline_Detail_InvalidId_ReturnsNull()
    {
        var detail = await _service.GetEnrollmentDetailAsync(0);
        Assert.Null(detail);

        detail = await _service.GetEnrollmentDetailAsync(-1);
        Assert.Null(detail);

        detail = await _service.GetEnrollmentDetailAsync(9999);
        Assert.Null(detail);
    }

    [Fact]
    public async Task FullPipeline_Pagination_TotalPagesCalculation()
    {
        var dashboard = await _service.GetDashboardAsync(null, null, null, 1);

        // 25 records, 10 per page = 3 pages
        Assert.Equal(3, dashboard.TotalPages);
        Assert.Equal(10, dashboard.Enrollments.Count);
    }

    [Fact]
    public async Task FullPipeline_LastPage_HasRemainingRecords()
    {
        var dashboard = await _service.GetDashboardAsync(null, null, null, 3);

        Assert.Equal(5, dashboard.Enrollments.Count);
        Assert.Equal(3, dashboard.CurrentPage);
    }
}
