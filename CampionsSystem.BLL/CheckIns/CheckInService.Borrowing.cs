using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.CheckIns;
public sealed partial class CheckInService
{
    // Called only inside the entrance transaction, after the Member lock and authoritative decision.
    private async Task<CheckInResult> BorrowAsync(AccessDecision decision,Member member,CreateCheckInRequest request,Guid actor,CancellationToken ct)
    {
        var source=await LockSubscriptionAsync(decision.SubscriptionId!.Value,ct);
        source.Version++;
        var entry=new CheckIn{OperationId=request.OperationId,MemberId=member.Id,BranchId=decision.BranchId,
            BusinessDate=decision.BusinessDate,BusinessDayId=decision.BusinessDayId,MemberSubscriptionId=source.Id,
            CheckInType=CheckInType.BorrowedGym,CheckInMethod=request.Method,CheckedInAt=access.Now,CreatedByUserId=actor};
        var debt=new BorrowedGymSession{MemberId=member.Id,ExpiredSubscriptionId=source.Id,CheckIn=entry,
            BranchId=decision.BranchId,BorrowedAt=entry.CheckedInAt,Status=BorrowedSessionStatus.Outstanding};
        db.Add(debt);
        access.Audit(actor,decision.BranchId,"CheckIn.Borrowed",nameof(CheckIn),entry.Id,
            after:new{entry.MemberId,entry.MemberSubscriptionId,entry.OperationId,entry.BusinessDate,entry.BusinessDayId,entry.CheckInMethod,DebtId=debt.Id});
        access.Audit(actor,decision.BranchId,"BorrowedSessionDebt.Created",nameof(BorrowedGymSession),debt.Id,
            after:new{debt.MemberId,debt.ExpiredSubscriptionId,CheckInId=entry.Id,BorrowedUsed=decision.BorrowedUsed+1});
        await db.SaveChangesAsync(ct);return await RecordedResultAsync(entry,member,ct);
    }
}
