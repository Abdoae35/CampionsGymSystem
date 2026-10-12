using System.Text.Json;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.MonthlyFinance;
internal sealed partial class MonthlyReportBuilder
{
    private async Task<IReadOnlyList<FinancialTransactionReportRow>> TransactionsAsync(IReadOnlyList<FinancialTransaction> ledger,FinancialMonth month,string branchName,DateTimeOffset cutoff,CancellationToken ct)
    {
        var paymentIds=ledger.Where(x=>x.PaymentId!=null).Select(x=>x.PaymentId!.Value).ToArray();
        var refundIds=ledger.Where(x=>x.RefundId!=null).Select(x=>x.RefundId!.Value).ToArray();
        var expenseIds=ledger.Where(x=>x.ExpenseId!=null).Select(x=>x.ExpenseId!.Value).ToArray();
        var advanceIds=ledger.Where(x=>x.EmployeeAdvanceId!=null).Select(x=>x.EmployeeAdvanceId!.Value).ToArray();
        var movementIds=ledger.Where(x=>x.CashMovementId!=null).Select(x=>x.CashMovementId!.Value).ToArray();
        var payrollIds=ledger.Where(x=>x.PayrollId!=null).Select(x=>x.PayrollId!.Value).ToArray();
        var payments=await db.Payments.AsNoTracking().Where(x=>paymentIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,ct);
        var refunds=await db.Refunds.AsNoTracking().Where(x=>refundIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,ct);
        var expenses=await db.Expenses.AsNoTracking().Include(x=>x.ExpenseCategory).Where(x=>expenseIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,ct);
        var advances=await db.EmployeeAdvances.AsNoTracking().Where(x=>advanceIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,ct);
        var movements=await db.CashMovements.AsNoTracking().Where(x=>movementIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,ct);
        var payroll=await db.Payrolls.AsNoTracking().Where(x=>payrollIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,ct);
        var payouts=await db.CoachPayouts.AsNoTracking().Where(x=>expenseIds.Contains(x.ExpenseId)).ToDictionaryAsync(x=>x.ExpenseId,ct);
        var payoutIds=payouts.Values.Select(x=>x.Id).ToArray();
        var payoutAllocations=await Bounded(db.CoachPayoutAllocations.AsNoTracking().Where(x=>payoutIds.Contains(x.CoachPayoutId)).OrderBy(x=>x.CoachPayoutId).ThenBy(x=>x.CoachEarningId),ct);
        var saleIds=payments.Values.Select(x=>x.SaleTransactionId).Concat(refunds.Values.Select(x=>x.SaleTransactionId)).Distinct().ToArray();
        var sales=await db.SaleTransactions.AsNoTracking().Include(x=>x.MembershipSnapshot).Where(x=>saleIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,ct);
        var memberIds=sales.Values.Select(x=>x.MemberId).Distinct().ToArray();var members=await db.Members.Where(x=>memberIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,x=>new{x.FullName,x.MemberCode,x.PrimaryPhone},ct);
        var shiftIds=ledger.Where(x=>x.ShiftId!=null).Select(x=>x.ShiftId!.Value).Distinct().ToArray();
        var shiftEmployees=await db.Shifts.AsNoTracking().Where(x=>shiftIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,x=>x.EmployeeId,ct);
        var userIds=ledger.Select(x=>x.CreatedByUserId).Distinct().ToArray();var employeeIds=sales.Values.Where(x=>x.SalesEmployeeId!=null).Select(x=>x.SalesEmployeeId!.Value)
            .Concat(advances.Values.Select(x=>x.ReceivingEmployeeId)).Concat(payouts.Values.Select(x=>x.CoachEmployeeId))
            .Concat(shiftEmployees.Values.Where(x=>x!=null).Select(x=>x!.Value)).Distinct().ToArray();
        var employees=await db.Employees.AsNoTracking().Where(x=>x.UserId!=null&&userIds.Contains(x.UserId.Value)||employeeIds.Contains(x.Id)).ToListAsync(ct);
        var names=employees.ToDictionary(x=>x.Id,x=>x.FullName);var actors=employees.Where(x=>x.UserId!=null).ToDictionary(x=>x.UserId!.Value);var employeesById=employees.ToDictionary(x=>x.Id);
        var accountIds=ledger.Select(x=>x.FinancialAccountId).Distinct().ToArray();var accounts=await db.FinancialAccounts.AsNoTracking().Where(x=>accountIds.Contains(x.Id)).ToDictionaryAsync(x=>x.Id,ct);
        var ledgerIds=ledger.Select(x=>x.Id).ToArray();var occurrences=await db.FinancialOccurrenceEvidence.AsNoTracking().Where(x=>ledgerIds.Contains(x.FinancialTransactionId)&&x.RecordedAt<=cutoff).ToDictionaryAsync(x=>x.FinancialTransactionId,ct);
        var paymentParts=await Bounded(db.PaymentParts.AsNoTracking().Where(x=>paymentIds.Contains(x.PaymentId)).OrderBy(x=>x.Id).Select(x=>new{x.Id,x.PaymentId,x.FinancialAccountId,x.Amount}),ct);
        var refundParts=await Bounded(db.RefundParts.AsNoTracking().Where(x=>refundIds.Contains(x.RefundId)).OrderBy(x=>x.Id).Select(x=>new{x.Id,x.RefundId,x.FinancialAccountId,x.Amount}),ct);
        var allocations=await Bounded(db.CommissionRefundAllocations.AsNoTracking().Where(x=>refundIds.Contains(x.RefundId)).OrderBy(x=>x.RefundId).ThenBy(x=>x.PaymentId).Select(x=>new{x.RefundId,x.PaymentId,x.Amount}),ct);
        var reversals=await Bounded(db.ExpenseReversals.AsNoTracking().Where(x=>expenseIds.Contains(x.ExpenseId)&&x.OccurredAt<=cutoff),ct);
        var refs=ledgerIds.Concat(paymentIds).Concat(refundIds).Concat(expenseIds).Concat(advanceIds).Concat(movementIds).Concat(payrollIds).Concat(saleIds).Concat(reversals.Select(x=>x.Id)).Concat(payouts.Values.Select(x=>x.Id)).Distinct().ToArray();
        var audits=await Bounded(db.AuditLogs.AsNoTracking().Where(x=>x.BranchId==month.BranchId&&x.EntityId!=null&&refs.Contains(x.EntityId.Value)&&x.CreatedAt<=cutoff).OrderBy(x=>x.CreatedAt).ThenBy(x=>x.Id),ct);
        var partsByPayment=paymentParts.ToLookup(x=>x.PaymentId);var partsByRefund=refundParts.ToLookup(x=>x.RefundId);var allocationsByRefund=allocations.ToLookup(x=>x.RefundId);
        var allocationsByPayout=payoutAllocations.ToLookup(x=>x.CoachPayoutId);var auditsByEntity=audits.ToLookup(x=>x.EntityId);var reversalsById=reversals.ToDictionary(x=>x.Id);
        var reversalByLedger=new Dictionary<Guid,ExpenseReversal>();
        foreach(var audit in audits.Where(x=>x.Action=="Expense.ReturnRecorded"&&x.NewValues!=null))
        {
            using var json=JsonDocument.Parse(audit.NewValues!);
            if(json.RootElement.TryGetProperty("LedgerId",out var id)&&id.TryGetGuid(out var ledgerId))
            {var reversal=audit.EntityId==null?null:reversalsById.GetValueOrDefault(audit.EntityId.Value);if(reversal!=null)reversalByLedger[ledgerId]=reversal;}
        }
        var result=new List<FinancialTransactionReportRow>(ledger.Count);
        foreach(var t in ledger)
        {
            var payment=t.PaymentId==null?null:payments.GetValueOrDefault(t.PaymentId.Value);var refund=t.RefundId==null?null:refunds.GetValueOrDefault(t.RefundId.Value);
            var saleId=payment?.SaleTransactionId??refund?.SaleTransactionId;var sale=saleId==null?null:sales.GetValueOrDefault(saleId.Value);
            var expense=t.ExpenseId==null?null:expenses.GetValueOrDefault(t.ExpenseId.Value);var advance=t.EmployeeAdvanceId==null?null:advances.GetValueOrDefault(t.EmployeeAdvanceId.Value);
            var movement=t.CashMovementId==null?null:movements.GetValueOrDefault(t.CashMovementId.Value);var p=t.PayrollId==null?null:payroll.GetValueOrDefault(t.PayrollId.Value);
            var payout=t.ExpenseId==null?null:payouts.GetValueOrDefault(t.ExpenseId.Value);var reversal=reversalByLedger.GetValueOrDefault(t.Id);
            var performingEmployee=t.ShiftId!=null?shiftEmployees.GetValueOrDefault(t.ShiftId.Value):null;
            var customer=sale==null?null:members.GetValueOrDefault(sale.MemberId);var actor=performingEmployee!=null?employeesById.GetValueOrDefault(performingEmployee.Value):actors.GetValueOrDefault(t.CreatedByUserId);var account=accounts[t.FinancialAccountId];
            var evidence=JsonSerializer.Serialize(new{
                Sale=sale==null?null:new{sale.Id,sale.SaleType,sale.SaleCategory,sale.GrossAmount,sale.DiscountAmount,sale.NetAmount,sale.SalesEmployeeId,sale.Status,sale.CancelledAt,sale.CancelledByUserId,sale.CancellationReason,
                    PurchasedPlan=sale.MembershipSnapshot==null?null:new{sale.MembershipSnapshot.PlanName,sale.MembershipSnapshot.MembershipPlanId,sale.MembershipSnapshot.DurationDays,sale.MembershipSnapshot.SessionCount}},
                Payment=payment==null?null:new{payment.Id,payment.OperationId,payment.Amount,payment.Status,payment.ReceivedByUserId,payment.PaidAt,Parts=partsByPayment[payment.Id]},
                Refund=refund==null?null:new{refund.Id,refund.OperationId,refund.Amount,refund.Reason,refund.ProcessedByUserId,refund.ApprovedByUserId,refund.OriginalPaymentId,Parts=partsByRefund[refund.Id],Allocations=allocationsByRefund[refund.Id]},
                Expense=expense==null?null:new{expense.Id,expense.ExpenseCategoryId,expense.Description,expense.Amount,expense.RecordingEmployeeId,expense.PaidAt,expense.ReceiptReference},
                Advance=advance==null?null:new{advance.Id,advance.ReceivingEmployeeId,Recipient=names.GetValueOrDefault(advance.ReceivingEmployeeId),advance.RecordingEmployeeId,advance.BasicEntitlementSnapshot,advance.Reference},
                CoachPayout=payout==null?null:new{payout.Id,payout.CoachEmployeeId,payout.Amount,payout.BalanceBefore,payout.BalanceAfter,payout.AuthorizedByUserId,Allocations=allocationsByPayout[payout.Id].Select(x=>new{x.CoachEarningId,x.Amount})},
                Payroll=p==null?null:new{p.Id,p.EmployeeId,p.BasicSalary,p.CommissionAmount,p.NetAmount},
                Withdrawal=movement==null?null:new{movement.Id,movement.Kind,movement.Reason},
                Reversal=reversal==null?null:new{reversal.Id,reversal.ExpenseId,reversal.OperationId,reversal.Reason,reversal.ReturnReference},
                OriginalOccurrence=occurrences.TryGetValue(t.Id,out var occurrence)?new{occurrence.Id,occurrence.OriginalOccurredAt,occurrence.Reason,occurrence.RecordedAt,occurrence.ActorUserId}:null
            });
            var linked=new[]{(Guid?)t.Id,t.PaymentId,t.RefundId,t.ExpenseId,t.EmployeeAdvanceId,t.CashMovementId,t.PayrollId,saleId,reversal?.Id,payout?.Id}.Where(x=>x!=null).ToHashSet();
            var auditEvidence=JsonSerializer.Serialize(linked.SelectMany(id=>auditsByEntity[id]).DistinctBy(x=>x.Id).OrderBy(x=>x.CreatedAt).ThenBy(x=>x.Id).Select(x=>new{x.Id,x.Action,x.UserId,x.EntityType,x.EntityId,x.CreatedAt,x.OldValues,x.NewValues}));
            result.Add(new(t.Id,t.BranchId,branchName,t.FinancialMonthId,month.Name,t.OccurredAt,t.PostedAt,occurrences.GetValueOrDefault(t.Id)?.OriginalOccurredAt,t.BusinessDate,
                account.Id,account.Name,account.AccountType.ToString(),t.SourceType.ToString(),t.TransactionType.ToString(),t.Amount,t.CreatedByUserId,actor?.Id,actor?.FullName,t.ShiftId,sale?.MemberId,customer?.FullName,
                sale?.SalesEmployeeId,sale?.SalesEmployeeId==null?null:names.GetValueOrDefault(sale.SalesEmployeeId.Value),saleId,t.PaymentId,t.RefundId,t.ExpenseId,t.EmployeeAdvanceId,t.CashMovementId,payout?.Id,t.PayrollId,
                reversal?.OperationId??payment?.OperationId??refund?.OperationId??advance?.OperationId??movement?.OperationId??expense?.OperationId,expense?.ExpenseCategoryId,expense?.ExpenseCategory.Name,t.Notes,
                reversal?.ReturnReference??expense?.ReceiptReference??advance?.Reference,reversal?.Id,refund?.OriginalPaymentId,evidence,auditEvidence));
        }
        return result;
    }
}
