using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGear.PM0902.Data;

namespace SmartGear.PM0902.Controllers;

[ApiController]
public class ShopController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ILogger<ShopController> _logger;
    public ShopController(AppDbContext db, ILogger<ShopController> logger) { _db = db; _logger = logger; }

    // GET /shop/{slug}
    [HttpGet("~/shop/{slug}", Name = "ShopDetails")]
    public async Task<IActionResult> Details(int id, string slug)
    {
        _logger.LogInformation("Shop/Details slug={Slug}", slug);

        var item = await _db.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Slug == slug);
        if (item is null) return NotFound();

        if (!string.Equals(slug, item.Slug, StringComparison.OrdinalIgnoreCase))
            return RedirectToRoutePermanent("ShopDetails", new { id, slug = item.Slug });

        Response.Headers["Link"] = $"</shop/{id}-{item.Slug}>; rel=\"canonical\"";
        return Ok(item);
    }
}