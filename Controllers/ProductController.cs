using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartGear.PM0902.Data;
using SmartGear.PM0902.Filters;
using SmartGear.PM0902.Models;
using System.Text.RegularExpressions;

namespace SmartGear.PM0902.Controllers;

[Route("products")]                          
public class ProductController : Controller
{
    private readonly ILogger<ProductController> _logger;
    private readonly AppDbContext _db;

    public ProductController(AppDbContext db, ILogger<ProductController> logger)
    {
        _db = db;
        _logger = logger;
    }

    [HttpGet("ui")]
    public IActionResult Ui() => View();

    // GET /products
    [HttpGet]
    [ServiceFilter(typeof(LogActionFilter))]
    public async Task<IActionResult> Index()
    {
        var list = await _db.Products.AsNoTracking().ToListAsync();
        return Ok(list); 
    }

    // GET /products/{id}
    [HttpGet("{id:int}")]
    [ServiceFilter(typeof(LogActionFilter))]
    public async Task<IActionResult> Details(int id)
    {
        var item = await _db.Products.FindAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    // POST /products
    [HttpPost]
    [ServiceFilter(typeof(AdminHeaderAuthorizeFilter))]
    [ServiceFilter(typeof(LogActionFilter))]
    public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var entity = new Product
        {
            Name = dto.Name,
            BasePrice = dto.BasePrice,
            DiscountPercent = dto.DiscountPercent,
            IsActive = dto.IsActive,
            CategoryId = dto.CategoryId
        };

        _db.Products.Add(entity);
        await _db.SaveChangesAsync(); 
        return CreatedAtAction(nameof(Details), new { id = entity.Id }, entity);
    }

    // PUT /products/{id}
    [HttpPut("{id:int}")]
    [ServiceFilter(typeof(AdminHeaderAuthorizeFilter))]
    [ServiceFilter(typeof(LogActionFilter))]
    public async Task<IActionResult> Update(int id, [FromBody] Product dto)
    {
        if (id != dto.Id) return BadRequest("Id mismatch");
        if (!await _db.Products.AnyAsync(p => p.Id == id)) return NotFound();

        _db.Entry(dto).State = EntityState.Modified;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /products/{id}
    [HttpDelete("{id:int}")]
    [ServiceFilter(typeof(AdminHeaderAuthorizeFilter))]
    [ServiceFilter(typeof(LogActionFilter))]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.Products.FindAsync(id);
        if (entity is null) return NotFound();
        _db.Products.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("new")] // GET /products/new (example extra)
    public IActionResult New() => Ok(new { Placeholder = "New Product form" });

}