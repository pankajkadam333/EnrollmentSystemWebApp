using EnrollmentDashboard.Business;
using EnrollmentDashboard.DataAccess;
using EnrollmentDashboard.Models.ViewModels;
using Microsoft.Extensions.Logging;
using Moq;

namespace EnrollmentDashboard.Tests.Business;

public class EnrollmentServiceTests
{
    private readonly Mock<IEnrollmentRepository> _mockRepo;
    private readonly EnrollmentService _service;

    public EnrollmentServiceTests()
    {
        _mockRepo = new Mock<IEnrollmentRepository>();
        _service = new EnrollmentService(_mockRepo.Object, Mock.Of<ILogger<EnrollmentService>>());
    }

    // ── GetDashboardAsync — Input Validation ──

    [Fact]
    public async Task GetDashboard_SwapsReversedDates()
    {
        var start = new DateTime(2026, 12, 31);
        var end = new DateTime(2026, 1, 1);

        _mockRepo.Setup(r => r.GetEnrollmentsAsync(
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((new List<EnrollmentListViewModel>(), 0));
        _mockRepo.Setup(r => r.GetEnrollmentSummaryAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new EnrollmentSummaryViewModel());

        var result = await _service.GetDashboardAsync(start, end, null, 1);

        // After swap, StartDate should be the earlier date
        Assert.Equal(end, result.StartDate);   // Jan 1 (originally end)
        Assert.Equal(start, result.EndDate);    // Dec 31 (originally start)
    }

    [Fact]
    public async Task GetDashboard_InvalidStatus_SetsToNull()
    {
        string? capturedStatus = "not-cleared";

        _mockRepo.Setup(r => r.GetEnrollmentsAsync(
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>()))
            .Callback<DateTime?, DateTime?, string?, int, int>((_, _, s, _, _) => capturedStatus = s)
            .ReturnsAsync((new List<EnrollmentListViewModel>(), 0));
        _mockRepo.Setup(r => r.GetEnrollmentSummaryAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new EnrollmentSummaryViewModel());

        await _service.GetDashboardAsync(null, null, "InvalidStatus", 1);

        Assert.Null(capturedStatus);
    }

    [Theory]
    [InlineData("Active")]
    [InlineData("Completed")]
    [InlineData("Withdrawn")]
    public async Task GetDashboard_ValidStatus_PassedThrough(string status)
    {
        string? capturedStatus = null;

        _mockRepo.Setup(r => r.GetEnrollmentsAsync(
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>()))
            .Callback<DateTime?, DateTime?, string?, int, int>((_, _, s, _, _) => capturedStatus = s)
            .ReturnsAsync((new List<EnrollmentListViewModel>(), 0));
        _mockRepo.Setup(r => r.GetEnrollmentSummaryAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new EnrollmentSummaryViewModel());

        await _service.GetDashboardAsync(null, null, status, 1);

        Assert.Equal(status, capturedStatus);
    }

    [Theory]
    [InlineData("'; DROP TABLE Enrollments--")]
    [InlineData("<script>alert('xss')</script>")]
    [InlineData("Robert' OR '1'='1")]
    public async Task GetDashboard_MaliciousStatus_SetsToNull(string maliciousStatus)
    {
        string? capturedStatus = "not-cleared";

        _mockRepo.Setup(r => r.GetEnrollmentsAsync(
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>()))
            .Callback<DateTime?, DateTime?, string?, int, int>((_, _, s, _, _) => capturedStatus = s)
            .ReturnsAsync((new List<EnrollmentListViewModel>(), 0));
        _mockRepo.Setup(r => r.GetEnrollmentSummaryAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new EnrollmentSummaryViewModel());

        await _service.GetDashboardAsync(null, null, maliciousStatus, 1);

        Assert.Null(capturedStatus);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task GetDashboard_InvalidPage_ClampsToOne(int invalidPage)
    {
        int capturedPage = 0;

        _mockRepo.Setup(r => r.GetEnrollmentsAsync(
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>()))
            .Callback<DateTime?, DateTime?, string?, int, int>((_, _, _, p, _) => capturedPage = p)
            .ReturnsAsync((new List<EnrollmentListViewModel>(), 0));
        _mockRepo.Setup(r => r.GetEnrollmentSummaryAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new EnrollmentSummaryViewModel());

        await _service.GetDashboardAsync(null, null, null, invalidPage);

        Assert.Equal(1, capturedPage);
    }

    [Fact]
    public async Task GetDashboard_NullDates_PassedAsNull()
    {
        DateTime? capturedStart = DateTime.MinValue;
        DateTime? capturedEnd = DateTime.MinValue;

        _mockRepo.Setup(r => r.GetEnrollmentsAsync(
                It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<string?>(), It.IsAny<int>(), It.IsAny<int>()))
            .Callback<DateTime?, DateTime?, string?, int, int>((s, e, _, _, _) => { capturedStart = s; capturedEnd = e; })
            .ReturnsAsync((new List<EnrollmentListViewModel>(), 0));
        _mockRepo.Setup(r => r.GetEnrollmentSummaryAsync(It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new EnrollmentSummaryViewModel());

        await _service.GetDashboardAsync(null, null, null, 1);

        Assert.Null(capturedStart);
        Assert.Null(capturedEnd);
    }

    // ── GetDashboardAsync — ViewModel Construction ──

    [Fact]
    public async Task GetDashboard_ReturnsPopulatedViewModel()
    {
        var enrollments = new List<EnrollmentListViewModel>
        {
            new() { EnrollmentID = 1, FirstName = "John", LastName = "Doe", Status = "Active", TotalRecords = 1 }
        };
        var summary = new EnrollmentSummaryViewModel
        {
            TotalEnrollments = 1, ActiveEnrollments = 1
        };

        _mockRepo.Setup(r => r.GetEnrollmentsAsync(null, null, null, 1, 10))
            .ReturnsAsync((enrollments, 1));
        _mockRepo.Setup(r => r.GetEnrollmentSummaryAsync(null, null))
            .ReturnsAsync(summary);

        var result = await _service.GetDashboardAsync(null, null, null, 1);

        Assert.Single(result.Enrollments);
        Assert.Equal(1, result.Summary.TotalEnrollments);
        Assert.Equal(1, result.TotalRecords);
        Assert.Equal(1, result.CurrentPage);
    }

    // ── GetEnrollmentDetailAsync ──

    [Fact]
    public async Task GetDetail_ValidId_ReturnsDetail()
    {
        var detail = new EnrollmentDetailViewModel { EnrollmentID = 1, FirstName = "John" };
        _mockRepo.Setup(r => r.GetEnrollmentDetailAsync(1)).ReturnsAsync(detail);

        var result = await _service.GetEnrollmentDetailAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result.EnrollmentID);
    }

    [Fact]
    public async Task GetDetail_NonExistentId_ReturnsNull()
    {
        _mockRepo.Setup(r => r.GetEnrollmentDetailAsync(9999)).ReturnsAsync((EnrollmentDetailViewModel?)null);

        var result = await _service.GetEnrollmentDetailAsync(9999);

        Assert.Null(result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-999)]
    public async Task GetDetail_InvalidId_ReturnsNullWithoutCallingRepo(int invalidId)
    {
        var result = await _service.GetEnrollmentDetailAsync(invalidId);

        Assert.Null(result);
        _mockRepo.Verify(r => r.GetEnrollmentDetailAsync(It.IsAny<int>()), Times.Never);
    }
}
