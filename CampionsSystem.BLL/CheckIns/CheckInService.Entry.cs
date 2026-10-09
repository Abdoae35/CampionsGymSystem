using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.CheckIns;
public sealed partial class CheckInService
{
    private async Task<T> TransactionAsync<T>(Func<Task<T>> change,CancellationToken ct)
    {
        var owned=db.Database.CurrentTransaction is null?await db.Database.BeginTransactionAsync(ct):null;
        var tx=db.Database.CurrentTransaction!;var point="checkin_"+Guid.NewGuid().ToString("N");
        try{await tx.CreateSavepointAsync(point,ct);var result=await change();await tx.ReleaseSavepointAsync(point,ct);if(owned!=null)await owned.CommitAsync(ct);return result;}
        catch{if(owned!=null)await owned.RollbackAsync(CancellationToken.None);else await tx.RollbackToSavepointAsync(point,CancellationToken.None);db.ChangeTracker.Clear();throw;}
        finally{if(owned!=null)await owned.DisposeAsync();}
    }
    private async Task<Member> LockMemberAsync(Guid id,CancellationToken ct)
    {
        var member=await db.Members.FromSqlInterpolated($"SELECT * FROM \"Members\" WHERE \"Id\"={id} FOR UPDATE").SingleAsync(ct);
        await db.Entry(member).ReloadAsync(ct);return member;
    }
    private async Task<MemberSubscription> LockSubscriptionAsync(Guid id,CancellationToken ct)
    {
        var sub=await db.MemberSubscriptions.FromSqlInterpolated($"SELECT * FROM \"MemberSubscriptions\" WHERE \"Id\"={id} FOR UPDATE").SingleAsync(ct);
        await db.Entry(sub).ReloadAsync(ct);return sub;
    }
    private static CheckInDetails Details(CheckIn row,Member? member)=>new(row.Id,row.OperationId,row.MemberId,member?.MemberCode,
        member?.FullName,row.BranchId,row.BusinessDate,row.BusinessDayId,row.CheckedInAt,row.CheckInType.ToString(),row.CheckInMethod.ToString(),
        row.MemberSubscriptionId,row.SessionConsumed,row.SessionsBefore,row.SessionsAfter,row.CreatedByUserId);
    private async Task<CheckInResult> RecordedResultAsync(CheckIn row,Member member,CancellationToken ct)
    {
        var sub=await db.MemberSubscriptions.AsNoTracking().SingleAsync(x=>x.Id==row.MemberSubscriptionId,ct);
        var used=row.CheckInType==CheckInType.BorrowedGym?await db.BorrowedGymSessions.CountAsync(x=>x.ExpiredSubscriptionId==sub.Id,ct):0;
        return new(new(true,AccessReasons.Allowed,member.Id,member.MemberCode,member.FullName,row.BranchId,row.BusinessDate!.Value,row.BusinessDayId,
            sub.Id,sub.AccessMode.ToString(),row.CheckInType==CheckInType.BorrowedGym?sub.RemainingSessions:row.SessionsAfter,false,used,0),Details(row,member));
    }
    public async Task<CheckInResult> CreateAsync(CreateCheckInRequest r,CancellationToken ct=default)
    {
        if(r.OperationId==Guid.Empty || r.Method is not (CheckInMethod.Manual or CheckInMethod.Scanner)
            || r.Method==CheckInMethod.Scanner && string.IsNullOrWhiteSpace(r.MemberCode))
            throw new BusinessRuleException("A nonempty operation identity and Manual/Scanner method with matching identification are required.");
        var context=await ContextAsync(CheckInPermissions.Create,r.BranchId,ct);
        return await TransactionAsync(async ()=>
        {
            var branch=await db.GymBranches.FromSqlInterpolated($"SELECT * FROM \"GymBranches\" WHERE \"Id\"={context.Branch.Id} FOR SHARE")
                .AsNoTracking().Include(x=>x.City).SingleAsync(ct);
            var day=await db.BusinessDays.FromSqlInterpolated($"SELECT * FROM \"BusinessDays\" WHERE \"BranchId\"={branch.Id} AND \"Status\"='Open' FOR SHARE")
                .AsNoTracking().SingleOrDefaultAsync(ct);
            if(day!=null && (day.OpenedAt>access.Now || day.BusinessDate>access.Today))throw new BusinessRuleException("Invalid open BusinessDay.");
            var member=await IdentifyAsync(r.MemberId,r.MemberCode,ct);
            if(member==null)return new CheckInResult(await DecideAsync(null,branch,day?.BusinessDate??access.Today,day?.Id,ct),null);
            member=await LockMemberAsync(member.Id,ct);
            var retry=await db.CheckIns.AsNoTracking().SingleOrDefaultAsync(x=>x.OperationId==r.OperationId,ct);
            if(retry!=null)
            {
                if(retry.MemberId!=member.Id || retry.BranchId!=branch.Id || retry.CheckInMethod!=r.Method
                    || retry.CreatedByUserId!=context.Actor.UserId || (retry.CheckInType==CheckInType.BorrowedGym)!=r.ConfirmBorrow)
                    throw new BusinessRuleException("Operation identity belongs to a different Check-In request.");
                return await RecordedResultAsync(retry,member,ct);
            }
            var decision=await DecideAsync(member,branch,day?.BusinessDate??access.Today,day?.Id,ct);
            if(r.ConfirmBorrow && decision.BorrowAvailable)
                return await BorrowAsync(decision,member,r,context.Actor.UserId,ct);
            if(!decision.Allowed)return new CheckInResult(decision,null);
            if(r.ConfirmBorrow)throw new BusinessRuleException("Borrow confirmation cannot override a current normal entitlement.");
            var sub=await LockSubscriptionAsync(decision.SubscriptionId!.Value,ct);
            var before=sub.RemainingSessions;
            if(sub.AccessMode==AccessMode.SessionBased){sub.RemainingSessions--;sub.Version++;}
            var row=new CheckIn{OperationId=r.OperationId,MemberId=member.Id,BranchId=branch.Id,BusinessDate=decision.BusinessDate,BusinessDayId=day?.Id,
                MemberSubscriptionId=sub.Id,CheckInType=CheckInType.Gym,CheckInMethod=r.Method,SessionConsumed=sub.AccessMode==AccessMode.SessionBased,
                SessionsBefore=before,SessionsAfter=sub.RemainingSessions,CheckedInAt=access.Now,CreatedByUserId=context.Actor.UserId};
            db.Add(row);access.Audit(context.Actor.UserId,branch.Id,"CheckIn.Created",nameof(CheckIn),row.Id,
                new{RemainingSessions=before},new{row.MemberId,row.MemberSubscriptionId,row.BusinessDate,row.BusinessDayId,row.CheckInType,row.CheckInMethod,row.OperationId,RemainingSessions=sub.RemainingSessions});
            await db.SaveChangesAsync(ct);return await RecordedResultAsync(row,member,ct);
        },ct);
    }
}
