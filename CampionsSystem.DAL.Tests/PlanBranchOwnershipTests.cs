using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.DAL.Tests;
public sealed class PlanBranchOwnershipTests
{
    [Fact] public async Task MigrationPreservesUnmappedPlansAndRestrictsBranchDeletion()
    {
        await using var fixture=await AuthTestDatabase.CreateAsync();await using var db=fixture.CreateContext();
        var city=new City{Name="Access city"};var branch=new GymBranch{Name="Owner",City=city};
        var legacy=new MembershipPlan{Name="Legacy",City=city,DurationDays=30};
        var owned=new MembershipPlan{Name="Owned",City=city,Branch=branch,DurationDays=30};
        db.AddRange(legacy,owned);await db.SaveChangesAsync();
        Assert.Null((await db.MembershipPlans.AsNoTracking().SingleAsync(p=>p.Id==legacy.Id)).BranchId);
        Assert.Equal(branch.Id,owned.BranchId);
        var fk=db.Model.FindEntityType(typeof(MembershipPlan))!.GetForeignKeys().Single(f=>f.Properties.Single().Name=="BranchId");
        Assert.Equal(DeleteBehavior.Restrict,fk.DeleteBehavior);
        Assert.Contains(db.Model.FindEntityType(typeof(MembershipPlan))!.GetIndexes(),i=>i.Properties.Single().Name=="BranchId");
        await Assert.ThrowsAsync<Npgsql.PostgresException>(()=>db.GymBranches.Where(b=>b.Id==branch.Id).ExecuteDeleteAsync());
    }
}
