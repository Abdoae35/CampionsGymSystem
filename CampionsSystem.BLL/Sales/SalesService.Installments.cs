using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Sales;
public sealed partial class SalesService
{
    private static void ValidateInstallmentSchedule(IReadOnlyList<InstallmentLineInput>? lines,decimal remaining,
        decimal initial,decimal total,DateOnly date)
    {
        Amount(initial);
        if(initial>total || remaining<=0 || lines is null || lines.Count is <1 or >100 || lines.Any(x=>x is null || x.DueDate<date))
            throw new BusinessRuleException("Installments require a positive agreed initial payment and valid remaining obligations.");
        foreach(var line in lines)Amount(line.Amount);
        if(lines.Sum(x=>x.Amount)!=remaining)throw new BusinessRuleException("Installment schedule must equal the Sale balance after the actual initial payment.");
    }
    private void CreateInstallmentSchedule(SaleTransaction sale,IReadOnlyList<InstallmentLineInput> lines,DateOnly date,Guid actor)
    {
        var agreement=new InstallmentAgreement{SaleTransaction=sale,TotalAmount=lines.Sum(x=>x.Amount),NumberOfInstallments=lines.Count,
            StartDate=date,Status=InstallmentAgreementStatus.Active,CreatedByUserId=actor,CreatedAt=access.Now};db.Add(agreement);
        var number=0;foreach(var line in lines)db.Add(new Installment{InstallmentAgreement=agreement,InstallmentNumber=++number,
            DueDate=line.DueDate,AmountDue=line.Amount,AmountPaid=0,Status=InstallmentStatus.Pending});
        access.Audit(actor,sale.BranchId,"Installment.ScheduleCreated",nameof(InstallmentAgreement),agreement.Id,after:new{sale.Id,sale.InitialPaymentRequired,Lines=lines});
    }
    private async Task AllocateInstallmentsAsync(SaleTransaction sale,Payment payment,IReadOnlyList<InstallmentAllocationInput>? allocations,Guid actor,CancellationToken ct)
    {
        if(allocations is null || allocations.Count is <1 or >100 || allocations.Any(x=>x is null || x.InstallmentId==Guid.Empty)
            || allocations.Select(x=>x.InstallmentId).Distinct().Count()!=allocations.Count)
            throw new BusinessRuleException("Installment payments require explicit unique obligation allocations.");
        foreach(var allocation in allocations)Amount(allocation.Amount);
        if(allocations.Sum(x=>x.Amount)!=payment.Amount)throw new BusinessRuleException("Installment allocations must equal the Payment amount.");
        var wasOverdue=await subscriptions.IsPaidSaleOverdueAsync(sale.Id,payment.BusinessDate!.Value,ct);
        foreach(var allocation in allocations.OrderBy(x=>x.InstallmentId))
        {
            var line=await db.Installments.Include(x=>x.InstallmentAgreement).SingleOrDefaultAsync(x=>x.Id==allocation.InstallmentId && x.InstallmentAgreement.SaleTransactionId==sale.Id,ct)
                ?? throw new BusinessRuleException("Installment does not belong to this Sale.");
            var paid=await db.InstallmentPaymentAllocations.Where(x=>x.InstallmentId==line.Id && x.Payment.Status==PaymentStatus.Completed).SumAsync(x=>x.Amount,ct);
            if(allocation.Amount>line.AmountDue-paid)throw new BusinessRuleException("Allocation exceeds the outstanding installment obligation.");
            db.Add(new InstallmentPaymentAllocation{Payment=payment,InstallmentId=line.Id,Amount=allocation.Amount});
            line.AmountPaid=paid+allocation.Amount;line.Status=line.AmountPaid==line.AmountDue?InstallmentStatus.Paid:InstallmentStatus.PartiallyPaid;
            if(line.Status==InstallmentStatus.Paid)line.PaidAt=access.Now;
            access.Audit(actor,sale.BranchId,"Installment.Paid",nameof(Installment),line.Id,new{AmountPaid=paid},new{line.AmountPaid,payment.Id});
        }
        await db.SaveChangesAsync(ct);
        var agreement=await db.InstallmentAgreements.SingleAsync(x=>x.SaleTransactionId==sale.Id,ct);
        if(!await db.Installments.AnyAsync(x=>x.InstallmentAgreementId==agreement.Id && x.AmountDue>
            db.InstallmentPaymentAllocations.Where(a=>a.InstallmentId==x.Id && a.Payment.Status==PaymentStatus.Completed).Sum(a=>a.Amount),ct))agreement.Status=InstallmentAgreementStatus.Completed;
        if(wasOverdue && !await subscriptions.IsPaidSaleOverdueAsync(sale.Id,payment.BusinessDate!.Value,ct))
            access.Audit(actor,sale.BranchId,"Installment.OverdueResolved",nameof(SaleTransaction),sale.Id,after:new{sale.MemberId});
    }
}
