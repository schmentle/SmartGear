using Microsoft.Extensions.Caching.Memory;
using SmartGear.PM0902.Models;

namespace SmartGear.PM0902.Repositories;

public class ProductRepositoryCached : IProductRepository
{
    private readonly IProductRepository _inner;
    private readonly IMemoryCache _cache;

    private const string ProductsAllKey = "products:list:all";

    public ProductRepositoryCached(IProductRepository inner, IMemoryCache cache)
    {
        _inner = inner;
        _cache = cache;
    }

    public async Task<IReadOnlyList<Product>> ListAsync()
    {
        if (_cache.TryGetValue(ProductsAllKey, out IReadOnlyList<Product>? cached) && cached is not null)
            return cached;

        var data = await _inner.ListAsync(); 
        _cache.Set(
            ProductsAllKey,
            data,
            new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2))
        );
        return data;
    }

    public Task<Product?> GetAsync(int id) => _inner.GetAsync(id);

    public Task<Product?> GetBySlugAsync(string slug) => _inner.GetBySlugAsync(slug);

    public async Task AddAsync(Product entity)
    {
        await _inner.AddAsync(entity);
        Invalidate();
    }

    public async Task UpdateAsync(Product entity)
    {
        await _inner.UpdateAsync(entity);
        Invalidate();
    }

    public async Task DeleteAsync(int id)
    {
        await _inner.DeleteAsync(id);
        Invalidate();
    }

    public Task<bool> ExistsAsync(int id) => _inner.ExistsAsync(id);

    private void Invalidate() => _cache.Remove(ProductsAllKey);
}