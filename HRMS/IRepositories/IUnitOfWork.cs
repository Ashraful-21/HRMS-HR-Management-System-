namespace HRMS.IRepositories;

/// <summary>
/// Unit of Work — wraps HrmsDbContext for cross-repository transactions.
/// Use SaveChangesAsync for a single-repository save; BeginTransactionAsync for multi-table
/// operations (payroll run, leave approval, employee onboarding).
/// </summary>
public interface IUnitOfWork : IDisposable
{
    IGenericRepository<T> Repository<T>() where T : class;
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    Task BeginTransactionAsync(CancellationToken ct = default);
    Task CommitAsync(CancellationToken ct = default);
    Task RollbackAsync(CancellationToken ct = default);
}
