using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Sales;
public sealed partial class SalesService
{
    public Task<SaleReceipt> CreateAsync(CreateSaleRequest r,CancellationToken ct=default) => TransactionAsync(async () =>
    {
        var result = await RecordSaleAsync(r,ct);
        var sale = await db.SaleTransactions.SingleAsync(x=>x.Id==result.SaleId,ct);
        var actor = await RequireAsync(SalesPermissions.Create,sale.BranchId,ct);
        if (!await db.MembershipSubscriptionTransactions.AnyAsync(x=>x.SaleTransactionId==sale.Id && x.OperationId!=null,ct))
            await subscriptions.ActivatePaidSaleAsync(sale.Id,actor.UserId,false,null,ct);
        return result;
    },ct);
}
