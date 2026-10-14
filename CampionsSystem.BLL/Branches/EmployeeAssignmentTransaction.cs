using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Branches;

// Serialize all assignment writers for one employee. Ambient transactions support tests/callers.
internal static class EmployeeAssignmentTransaction
{
    public static async Task<T> RunAsync<T>(ApplicationDbContext database, Guid employeeId,
        Func<Employee, Task<T>> change, CancellationToken ct, Func<Task>? beforeLock = null)
    {
        var owned = database.Database.CurrentTransaction is null
            ? await database.Database.BeginTransactionAsync(ct) : null;
        var transaction = database.Database.CurrentTransaction!;
        var savepoint = "branch_assignment_" + Guid.NewGuid().ToString("N");
        try
        {
            await transaction.CreateSavepointAsync(savepoint, ct);
            if (beforeLock is not null) await beforeLock();
            var employees = await database.Employees.FromSqlInterpolated(
                $"SELECT * FROM \"Employees\" WHERE \"Id\" = {employeeId} FOR UPDATE").AsNoTracking().ToListAsync(ct);
            var employee = employees.SingleOrDefault() ?? throw new BranchNotFoundException("Employee not found.");
            var result = await change(employee);
            await transaction.ReleaseSavepointAsync(savepoint, ct);
            if (owned is not null) await owned.CommitAsync(ct);
            return result;
        }
        catch
        {
            // Rollback must still run when the request's cancellation token is cancelled.
            if (owned is not null) await owned.RollbackAsync(CancellationToken.None);
            else await transaction.RollbackToSavepointAsync(savepoint, CancellationToken.None);
            database.ChangeTracker.Clear();
            throw;
        }
        finally { if (owned is not null) await owned.DisposeAsync(); }
    }
}
