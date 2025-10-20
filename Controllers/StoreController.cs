using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SmartGear.PM0902.Models;
using SmartGear.PM0902.Repositories;
using SmartGear.PM0902.ViewModels;

namespace SmartGear.PM0902.Controllers;

[Authorize] 
public class StoreController : Controller
{
    private readonly IProductRepository _products;
    private readonly ICategoryRepository _categories;
    private readonly ILogger<StoreController> _logger;

    public StoreController(IProductRepository products, ICategoryRepository categories, ILogger<StoreController> logger)
    {
        _products = products;
        _categories = categories;
        _logger = logger;
    }

    // GET /store
    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        try
        {
            var items = await _products.ListAsync();
            return View(items);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list products");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    // GET /store/details/{slug}
    [AllowAnonymous]
    [HttpGet("/store/details/{slug}")]
    public async Task<IActionResult> Details(string slug)
    {
        try
        {
            var item = await _products.GetBySlugAsync(slug);
            return item is null ? NotFound() : View(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load product details for {Slug}", slug);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    // GET /store/create
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        try
        {
            var vm = new ProductFormViewModel
            {
                Categories = await BuildCategorySelectAsync()
            };
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load create form");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    // POST /store/create
    [Authorize(Roles = "Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProductFormViewModel vm)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                vm.Categories = await BuildCategorySelectAsync();
                return View(vm);
            }

            var entity = new Product
            {
                Name = vm.Name,
                BasePrice = vm.BasePrice,
                DiscountPercent = vm.DiscountPercent,
                CategoryId = vm.CategoryId!.Value,
                IsActive = vm.IsActive
            };

            await _products.AddAsync(entity);
            TempData["Flash"] = $"Created '{entity.Name}' @ R {entity.DiscountedPrice:0.00} (ex VAT)";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create product {@VM}", vm);
            vm.Categories = await BuildCategorySelectAsync();
            ModelState.AddModelError(string.Empty, "We couldn’t save your product. Please try again.");
            return View(vm);
        }
    }

    // GET /store/edit/{id}
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var item = await _products.GetAsync(id);
            if (item is null) return NotFound();

            var vm = new ProductFormViewModel
            {
                Id = item.Id,
                Name = item.Name,
                BasePrice = item.BasePrice,
                DiscountPercent = item.DiscountPercent,
                CategoryId = item.CategoryId,
                IsActive = item.IsActive,
                Categories = await BuildCategorySelectAsync()
            };
            return View(vm);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load edit form for {Id}", id);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    // POST /store/edit/{id}
    [Authorize(Roles = "Admin")]
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ProductFormViewModel vm)
    {
        try
        {
            if (id != vm.Id) return BadRequest();
            if (!ModelState.IsValid)
            {
                vm.Categories = await BuildCategorySelectAsync();
                return View(vm);
            }

            var exists = await _products.ExistsAsync(id);
            if (!exists) return NotFound();

            var entity = new Product
            {
                Id = vm.Id!.Value,
                Name = vm.Name,
                BasePrice = vm.BasePrice,
                DiscountPercent = vm.DiscountPercent,
                CategoryId = vm.CategoryId!.Value,
                IsActive = vm.IsActive
            };

            await _products.UpdateAsync(entity);
            TempData["Flash"] = $"Updated '{entity.Name}'.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update product {Id} {@VM}", id, vm);
            vm.Categories = await BuildCategorySelectAsync();
            ModelState.AddModelError(string.Empty, "We couldn’t save your changes. Please try again.");
            return View(vm);
        }
    }

    // GET /store/delete/{id}
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var item = await _products.GetAsync(id);
            return item is null ? NotFound() : View(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load delete confirmation for {Id}", id);
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    // POST /store/delete/{id}
    [Authorize(Roles = "Admin")]
    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _products.DeleteAsync(id);
            TempData["Flash"] = "Product deleted.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete product {Id}", id);
            TempData["Flash"] = "Delete failed.";
            return RedirectToAction(nameof(Index));
        }
    }

    private async Task<IEnumerable<SelectListItem>> BuildCategorySelectAsync()
    {
        var list = await _categories.ListAsync();
        return list.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
    }
}