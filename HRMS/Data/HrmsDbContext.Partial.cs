using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace HRMS.Data;

/// <summary>
/// Partial extension of the scaffolded HrmsDbContext.
/// Adds: audit auto-stamping in SaveChangesAsync, and a hook for global query filters.
/// </summary>
public partial class HrmsDbContext
{
    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Models.Branch>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.Department>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.Designation>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.Grade>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.Shift>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.Holiday>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.Employee>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.User>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.Role>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.JobOpening>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.Candidate>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.LeaveType>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.Asset>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.AssetCategory>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.SalaryComponent>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.Announcement>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.AttendanceLog>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.AttendanceRegularization>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.LeaveApplication>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.SalaryStructure>().HasQueryFilter(e => !e.IsDeleted);
        modelBuilder.Entity<Models.PayrollRun>().HasQueryFilter(e => !e.IsDeleted);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        StampAuditFields();
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void StampAuditFields()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added)
            {
                SetIfExists(entry, "CreatedAt", now);
                SetIfExists(entry, "IsDeleted", false);
            }
            else if (entry.State == EntityState.Modified)
            {
                SetIfExists(entry, "UpdatedAt", now);
            }
        }
    }

    private static void SetIfExists(EntityEntry entry, string prop, object? value)
    {
        var p = entry.Properties.FirstOrDefault(x => x.Metadata.Name == prop);
        if (p is not null)
            p.CurrentValue = value;
    }
}
