using Microsoft.AspNetCore.Mvc;
using SmartGear.PM0902.Services;

namespace SmartGear.PM0902.Controllers;

public class HomeController : Controller
{
    private readonly IRequestTimeService _timeService;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IRequestTimeService timeService, ILogger<HomeController> logger)
    {
        _timeService = timeService;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var utcNow = _timeService.UtcNow.ToString("u");
        var correlationId = HttpContext.TraceIdentifier;

        ViewBag.UtcNow = utcNow;
        ViewBag.CorrelationId = correlationId;

        _logger.LogInformation($"Home/Index served at {utcNow} (corrId={correlationId})");

        return View();
    }
}