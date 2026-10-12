using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Entities;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.MonthlyFinance;
public sealed partial class MonthlyFinanceService
{
    public Task<Guid> RecordOccurrenceAsync(Guid ledgerId,RecordOccurrenceRequest r,CancellationToken ct=default)=>coordination.TransactionAsync(async()=>
    {
        FinancialOperations.Operation(r.OperationId);FinancialOperations.Reason(r.Reason);
        if(r.OriginalOccurredAt==default||r.OriginalOccurredAt>access.Now)throw new BusinessRuleException("A known original occurrence date must be in the past or present.");
        var ledger=await db.FinancialTransactions.AsNoTracking().SingleOrDefaultAsync(x=>x.Id==ledgerId,ct)??throw new BranchNotFoundException("Ledger movement not found.");
        var actor=await access.RequireAsync(MonthlyFinancePermissions.Manage,ledger.BranchId,ct);await coordination.LockBranchAsync(ledger.BranchId,ct);
        var hash=FinancialOperations.Hash(new{ledgerId,r});var prior=await db.FinancialOccurrenceEvidence.AsNoTracking().SingleOrDefaultAsync(x=>x.OperationId==r.OperationId,ct);
        if(prior!=null){FinancialOperations.Retry(prior.RequestHash,hash,prior.ActorUserId,actor);return prior.Id;}
        if(await db.FinancialOccurrenceEvidence.AnyAsync(x=>x.FinancialTransactionId==ledgerId,ct))throw new BusinessRuleException("Original occurrence evidence is already recorded and immutable.");
        var evidence=new FinancialOccurrenceEvidence{FinancialTransactionId=ledgerId,OperationId=r.OperationId,RequestHash=hash,OriginalOccurredAt=r.OriginalOccurredAt,RecordedAt=access.Now,ActorUserId=actor,Reason=r.Reason.Trim()};
        db.Add(evidence);access.Audit(actor,ledger.BranchId,"FinancialMonth.OccurrenceRecorded",nameof(FinancialOccurrenceEvidence),evidence.Id,after:new{ledgerId,r.OriginalOccurredAt,r.Reason});
        return evidence.Id;
    },ct);
}
