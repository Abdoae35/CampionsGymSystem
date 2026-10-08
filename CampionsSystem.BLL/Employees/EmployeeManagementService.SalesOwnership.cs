using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Employees;

public sealed partial class EmployeeManagementService
{
    // Called only by employment status changes, inside the employee row-lock transaction.
    // Branch transfers deliberately do not invoke this workflow.
    private async Task CloseSalesOwnershipAsync(Guid employeeId, Guid actor, string? reason, CancellationToken ct)
    {
        var members = await database.MemberSalesAssignments.FromSqlInterpolated($"""
            SELECT * FROM "MemberSalesAssignments"
            WHERE "SalesEmployeeId" = {employeeId} AND "EndedAt" IS NULL AND "AssignmentType" = 'Sales'
            ORDER BY "Id" FOR UPDATE
            """).ToListAsync(ct);
        var leads = await database.LeadAssignments.FromSqlInterpolated($"""
            SELECT * FROM "LeadAssignments"
            WHERE "SalesEmployeeId" = {employeeId} AND "EndedAt" IS NULL
            ORDER BY "Id" FOR UPDATE
            """).ToListAsync(ct);
        foreach (var assignment in members)
        {
            await database.Entry(assignment).ReloadAsync(ct);
            if (assignment.EndedAt.HasValue) continue;
            if (assignment.AssignedAt > access.Now)
                throw new BusinessRuleException("Reconcile future-dated Sales assignments before ending employment.");
            assignment.EndedAt = access.Now;
            var branch = await database.Members.Where(m => m.Id == assignment.MemberId).Select(m => m.HomeBranchId).SingleAsync(ct);
            access.Audit(actor, branch, "Member.ReturnedToHouse", nameof(Member), assignment.MemberId,
                new { assignment.Id, assignment.SalesEmployeeId },
                new { assignment.EndedAt, Reason = "Employee no longer active", EmploymentReason = reason?.Trim() });
        }
        foreach (var assignment in leads)
        {
            await database.Entry(assignment).ReloadAsync(ct);
            if (assignment.EndedAt.HasValue) continue;
            if (assignment.AssignedAt > access.Now)
                throw new BusinessRuleException("Reconcile future-dated Lead assignments before ending employment.");
            assignment.EndedAt = access.Now;
            var branch = await database.Leads.Where(l => l.Id == assignment.LeadId).Select(l => l.BranchId).SingleAsync(ct);
            access.Audit(actor, branch, "Lead.Unassigned", nameof(Lead), assignment.LeadId,
                new { assignment.Id, assignment.SalesEmployeeId },
                new { assignment.EndedAt, Reason = "Employee no longer active", EmploymentReason = reason?.Trim() });
        }
        // Subject profiles, schedules, interactions, financial rows and assignment start facts stay intact.
        // Open Member tasks automatically appear in House queues because ownership is derived.
    }
}
