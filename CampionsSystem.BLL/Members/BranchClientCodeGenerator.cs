using CampionsSystem.DAL.Data;
using CampionsSystem.BLL.Operations;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Members;

public sealed class BranchClientCodeGenerator(ApplicationDbContext db)
{
    // The caller's transaction keeps allocation atomic with member/audit/ownership writes.
    public async Task<long> NextAsync(Guid branchId, CancellationToken ct = default)
    {
        if (db.Database.CurrentTransaction is null) throw new InvalidOperationException("ClientCode allocation requires a transaction.");
        var values = await db.Database.SqlQuery<long>($"""
            UPDATE "GymBranches" SET "LastMemberClientCode" = "LastMemberClientCode" + 1
            WHERE "Id" = {branchId} AND "IsActive" AND "LastMemberClientCode" < 9223372036854775807
            RETURNING "LastMemberClientCode" AS "Value"
            """).ToListAsync(ct);
        return values.SingleOrDefault() is > 0 and var code ? code
            : throw new BusinessRuleException("Active registration branch with available ClientCode capacity required.");
    }
}
