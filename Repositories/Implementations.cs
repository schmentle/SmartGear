using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartGear.PM0902.Data;
using SmartGear.PM0902.Hubs;
using SmartGear.PM0902.Models;

namespace SmartGear.PM0902.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;
    private readonly IHubContext<ProductHub> _hub;
    public ProductRepository(AppDbContext db, IHubContext<ProductHub> hub)
    {
        _db = db; _hub = hub;
    }

    public async Task<Product?> GetAsync(int id) =>
        await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Id == id);

    public async Task<Product?> GetBySlugAsync(string slug) =>
        await _db.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.Slug == slug);

    public async Task<IReadOnlyList<Product>> ListAsync() =>
        await _db.Products.Include(p => p.Category).OrderBy(p => p.Name).ToListAsync();

    public async Task AddAsync(Product entity)
    {
        _db.Products.Add(entity);
        await _db.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("ProductCreated", new { entity.Id, entity.Name, entity.Slug, entity.BasePrice });
    }

    public async Task UpdateAsync(Product entity)
    {
        _db.Update(entity);
        await _db.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("ProductUpdated", new { entity.Id, entity.Name, entity.Slug, entity.BasePrice });
    }

    public async Task DeleteAsync(int id)
    {
        var e = await _db.Products.FindAsync(id);
        if (e is null) return;
        _db.Products.Remove(e);
        await _db.SaveChangesAsync();
        await _hub.Clients.All.SendAsync("ProductDeleted", new { id });
    }

    public Task<bool> ExistsAsync(int id) => _db.Products.AnyAsync(p => p.Id == id);
}

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;
    public CategoryRepository(AppDbContext db) => _db = db;

    public async Task<Category?> GetAsync(int id) =>
        await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<Category?> GetBySlugAsync(string slug) =>
        await _db.Categories.FirstOrDefaultAsync(c => c.Slug == slug);

    public async Task<IReadOnlyList<Category>> ListAsync() =>
        await _db.Categories.OrderBy(c => c.Name).ToListAsync();

    public async Task AddAsync(Category entity)
    {
        _db.Categories.Add(entity);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category entity)
    {
        _db.Categories.Update(entity);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _db.Categories.FindAsync(id);
        if (entity is null) return;
        _db.Categories.Remove(entity);
        await _db.SaveChangesAsync();
    }

    public Task<bool> ExistsAsync(int id) => _db.Categories.AnyAsync(c => c.Id == id);
}