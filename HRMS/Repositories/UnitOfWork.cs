using Microsoft.EntityFrameworkCore.Storage;
using HRMS.Data;
using HRMS.IRepositories;

namespace HRMS.Repositories;

/// <summary>
/// Unit of Work implementation wrapping HrmsDbContext.
/// Supports transactions for multi-table operations (payroll run, leave approval, etc.).
/// </summary>
public class UnitOfWork(HrmsDbContext context) : IUnitOfWork
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _transaction;

    public IGenericRepository<T> Repository<T>() where T : class
    {
        return (IGenericRepository<T>)_repositories.GetOrAdd(typeof(T), _ => new GenericRepository<T>(context));
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => context.SaveChangesAsync(ct);

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackAsync(CancellationToken ct = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        context.Dispose();
    }
}
