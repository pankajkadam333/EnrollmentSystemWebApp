using EnrollmentDashboard.Business;
using EnrollmentDashboard.Controllers;
using EnrollmentDashboard.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace EnrollmentDashboard.Tests.Controllers;

public class EnrollmentsControllerTests
{
    private readonly Mock<IEnrollmentService> _mockService;
    private readonly EnrollmentsController _controller;

    public EnrollmentsControllerTests()
    {
        _mockService = new Mock<IEnrollmentService>();
        _controller = new EnrollmentsController(_mockService.Object, Mock.Of<ILogger<EnrollmentsController>>());
    }

    // ── Index ──

    [Fact]
    public async Task Index_ReturnsViewWithDashboardModel()
    {
        var dashboard = new DashboardViewModel
        {
            Enrollments = [new() { EnrollmentID = 1 }],
            Summary = new() { TotalEnrollments = 1 }
        };
        _mockService.Setup(s => s.GetDashboardAsync(null, null, null, 1))
            .ReturnsAsync(dashboard);

        var result = await _controller.Index(null, null, null, 1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DashboardViewModel>(viewResult.Model);
        Assert.Single(model.Enrollments);
    }

    [Fact]
    public async Task Index_PassesFiltersToService()
    {
        var start = new DateTime(2026, 1, 1);
        var end = new DateTime(2026, 6, 30);

        _mockService.Setup(s => s.GetDashboardAsync(start, end, "Active", 2))
            .ReturnsAsync(new DashboardViewModel());

        await _controller.Index(start, end, "Active", 2);

        _mockService.Verify(s => s.GetDashboardAsync(start, end, "Active", 2), Times.Once);
    }

    [Fact]
    public async Task Index_DefaultPage_IsOne()
    {
        _mockService.Setup(s => s.GetDashboardAsync(null, null, null, 1))
            .ReturnsAsync(new DashboardViewModel());

        await _controller.Index(null, null, null);

        _mockService.Verify(s => s.GetDashboardAsync(null, null, null, 1), Times.Once);
    }

    // ── Details ──

    [Fact]
    public async Task Details_ValidId_ReturnsViewWithModel()
    {
        var detail = new EnrollmentDetailViewModel
        {
            EnrollmentID = 1,
            FirstName = "John",
            LastName = "Doe"
        };
        _mockService.Setup(s => s.GetEnrollmentDetailAsync(1)).ReturnsAsync(detail);

        var result = await _controller.Details(1);

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<EnrollmentDetailViewModel>(viewResult.Model);
        Assert.Equal(1, model.EnrollmentID);
    }

    [Fact]
    public async Task Details_NonExistentId_ReturnsNotFound()
    {
        _mockService.Setup(s => s.GetEnrollmentDetailAsync(9999))
            .ReturnsAsync((EnrollmentDetailViewModel?)null);

        var result = await _controller.Details(9999);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_NullId_ReturnsNotFound()
    {
        var result = await _controller.Details(null);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_ZeroId_ReturnsNotFound()
    {
        var result = await _controller.Details(0);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_NegativeId_ReturnsNotFound()
    {
        var result = await _controller.Details(-5);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Details_NullId_DoesNotCallService()
    {
        await _controller.Details(null);

        _mockService.Verify(s => s.GetEnrollmentDetailAsync(It.IsAny<int>()), Times.Never);
    }
}
