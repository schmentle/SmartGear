using SmartGear.PM0902.Models;

namespace SmartGear.PM0902.Repositories;

public interface IRepository<T> where T : class
{
    Task<T?> GetAsync(int id);
    Task<IReadOnlyList<T>> ListAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(int id);
    Task<bool> ExistsAsync(int id);
}

public interface IProductRepository : IRepository<Product>
{
    Task<Product?> GetBySlugAsync(string slug);
}

public interface ICategoryRepository : IRepository<Category>
{
    Task<Category?> GetBySlugAsync(string slug);
}