using EnrollmentDashboard.Business;
using Microsoft.AspNetCore.Mvc;

namespace EnrollmentDashboard.Controllers;

public class EnrollmentsController : Controller
{
    private readonly IEnrollmentService _enrollmentService;
    private readonly ILogger<EnrollmentsController> _logger;

    public EnrollmentsController(IEnrollmentService enrollmentService, ILogger<EnrollmentsController> logger)
    {
        _enrollmentService = enrollmentService;
        _logger = logger;
    }

    // GET: /Enrollments
    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, string? status, int page = 1)
    {
        // Guard against unparseable date values from query string
        if (startDate.HasValue && endDate.HasValue && startDate > endDate)
        {
            _logger.LogWarning("Controller received reversed dates — swapping before service call");
            (startDate, endDate) = (endDate, startDate);
        }

        var model = await _enrollmentService.GetDashboardAsync(startDate, endDate, status, page);
        return View(model);
    }

    // GET: /Enrollments/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (!id.HasValue || id.Value <= 0)
        {
            _logger.LogWarning("Details requested with invalid ID: {Id}", id);
            return NotFound();
        }

        var model = await _enrollmentService.GetEnrollmentDetailAsync(id.Value);

        if (model is null)
        {
            _logger.LogWarning("Details — enrollment not found for ID: {Id}", id);
            return NotFound();
        }

        return View(model);
    }
}
