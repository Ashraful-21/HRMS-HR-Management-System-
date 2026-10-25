using HRMS.Data;
using HRMS.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace HRMS.Repositories;

public class GenericRepository<T>(HrmsDbContext db) : IGenericRepository<T> where T : class
{
    protected readonly HrmsDbContext _db = db;
    protected readonly DbSet<T> _dbSet = db.Set<T>();

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _dbSet.FindAsync([id], ct);

    public virtual IQueryable<T> Query()
        => _dbSet.AsQueryable();

    public virtual async Task AddAsync(T entity, CancellationToken ct = default)
        => await _dbSet.AddAsync(entity, ct);

    public virtual void Update(T entity)
        => _dbSet.Update(entity);

    public virtual void Remove(T entity)
        => _dbSet.Remove(entity);

    public virtual async Task<bool> ExistsAsync(int id, CancellationToken ct = default)
    {
        var entity = await GetByIdAsync(id, ct);
        return entity is not null;
    }
}
