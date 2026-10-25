using HRMS.Dtos.Common;

namespace HRMS.IRepositories;

/// <summary>Generic repository contract for basic CRUD + query operations.</summary>
public interface IGenericRepository<T> where T : class
{
    Task<T?> GetByIdAsync(int id, CancellationToken ct = default);
    IQueryable<T> Query();                       // Returns IQueryable for service-level projection
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
    Task<bool> ExistsAsync(int id, CancellationToken ct = default);
}
