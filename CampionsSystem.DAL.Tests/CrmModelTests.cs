using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed class CrmModelTests
{
    [Fact] public void HistoryAndOpenWorkHaveDatabaseProtection()
    {
        using var db = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql("Host=localhost;Database=unused").Options);
        var assignment = db.Model.FindEntityType(typeof(LeadAssignment))!;
        Assert.Contains(assignment.GetIndexes(), i => i.IsUnique && i.GetFilter() == "\"EndedAt\" IS NULL");
        var task = db.Model.FindEntityType(typeof(MemberFollowUpTask))!;
        Assert.Contains(task.GetIndexes(), i => i.IsUnique && i.GetFilter() == "\"CompletedAt\" IS NULL");
        Assert.All(task.GetForeignKeys(), fk => Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior));
        Assert.Equal(typeof(Guid), db.Model.FindEntityType(typeof(Lead))!.FindPrimaryKey()!.Properties.Single().ClrType);
    }
    [Fact] public async Task CrmMigrationAppliesInIsolatedSchema()
    {
        await using var fixture = await AuthTestDatabase.CreateAsync(); await using var db = fixture.CreateContext();
        Assert.Empty(await db.MemberFollowUpTasks.ToListAsync());
        Assert.Empty(await db.Leads.ToListAsync());
    }
}
