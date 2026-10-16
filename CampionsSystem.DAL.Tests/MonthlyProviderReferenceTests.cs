using System.IO.Compression;
using System.Text.Json;
using System.Xml.Linq;
using CampionsSystem.BLL.MonthlyFinance;
using CampionsSystem.BLL.Sales;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.DAL.Tests;

public sealed partial class SalesTests
{
    [Fact]
    public async Task ReceptionCanRecordInstaPayPaymentWithoutProviderReference()
    {
        var managerRole = await db.Roles.SingleAsync(x => x.NormalizedName == "BRANCHMANAGER");
        db.UserRoles.Remove(await db.UserRoles.SingleAsync(x => x.UserId == actor.Id && x.RoleId == managerRole.Id));
        await db.SaveChangesAsync();
        await AddRefundRoleAsync("Reception");
        var insta = await Account(FinancialAccountType.InstaPay);

        var sale = await Finance().CreateAsync(Purchase(new PaymentPartInput(insta.Id, 1200)));

        var payment = await db.Payments.SingleAsync(x => x.SaleTransactionId == sale.SaleId);
        Assert.Null((await db.PaymentParts.SingleAsync(x => x.PaymentId == payment.Id)).ProviderReference);
        var movement = await db.FinancialTransactions.SingleAsync();
        Assert.Equal(insta.Id, movement.FinancialAccountId);
        Assert.Equal(1200, movement.Amount);
    }

    [Fact]
    public async Task MonthlyWorkbookShowsExactInstaPayReferencesAndBlankOptionalCellsWithoutExtraMoney()
    {
        var month = await ReportingMonth(today);
        var cash = await Account();
        var insta = await Account(FinancialAccountType.InstaPay);
        const string paymentReference = " =IP-α /001 ";
        const string refundReference = "RF-002";

        var first = await Finance().CreateAsync(Purchase(new(cash.Id, 700), new(insta.Id, 500, paymentReference)));
        await Finance().RefundAsync(first.SaleId, new(Guid.NewGuid(), 300,
            [new(cash.Id, 100), new(insta.Id, 200, refundReference)], "Partial refund"));

        var secondMember = new Member { MemberCode = "M-NO-REF", FullName = "No reference", PrimaryPhone = "no-ref",
            HomeBranch = a, CreatedByUser = actor };
        db.Add(secondMember);
        await db.SaveChangesAsync();
        var second = await Finance().CreateAsync(Purchase(new PaymentPartInput(insta.Id, 1200)) with { MemberId = secondMember.Id });
        await Finance().RefundAsync(second.SaleId, new(Guid.NewGuid(), 100, [new(insta.Id, 100, "")], "No reference"));

        var firstPayment = await db.Payments.SingleAsync(x => x.SaleTransactionId == first.SaleId);
        var firstRefund = await db.Refunds.SingleAsync(x => x.SaleTransactionId == first.SaleId);
        var secondPayment = await db.Payments.SingleAsync(x => x.SaleTransactionId == second.SaleId);
        var secondRefund = await db.Refunds.SingleAsync(x => x.SaleTransactionId == second.SaleId);
        Assert.Null((await db.PaymentParts.SingleAsync(x => x.PaymentId == secondPayment.Id)).ProviderReference);
        Assert.Equal("", (await db.RefundParts.SingleAsync(x => x.RefundId == secondRefund.Id)).ProviderReference);

        var report = await Monthly().ReportAsync(month.Id);
        Assert.Equal(6, report.Transactions.Count);
        Assert.Equal(await db.FinancialTransactions.CountAsync(), report.Transactions.Count);
        Assert.Equal(report.Transactions.Count, report.Transactions.Select(x => x.LedgerId).Distinct().Count());
        Assert.Equal(2400, report.Summary.GrossCollections);
        Assert.Equal(400, report.Summary.Refunds);
        Assert.Equal(2400, report.Summary.LedgerCredits);
        Assert.Equal(400, report.Summary.LedgerDebits);

        var paymentCash = Assert.Single(report.Transactions, x => x.PaymentId == firstPayment.Id && x.AccountId == cash.Id);
        var paymentInsta = Assert.Single(report.Transactions, x => x.PaymentId == firstPayment.Id && x.AccountId == insta.Id);
        var refundCash = Assert.Single(report.Transactions, x => x.RefundId == firstRefund.Id && x.AccountId == cash.Id);
        var refundInsta = Assert.Single(report.Transactions, x => x.RefundId == firstRefund.Id && x.AccountId == insta.Id);
        var noReferencePayment = Assert.Single(report.Transactions, x => x.PaymentId == secondPayment.Id);
        var noReferenceRefund = Assert.Single(report.Transactions, x => x.RefundId == secondRefund.Id);
        Assert.Null(paymentCash.ProviderReference);
        Assert.Equal(paymentReference, paymentInsta.ProviderReference);
        Assert.Null(refundCash.ProviderReference);
        Assert.Equal(refundReference, refundInsta.ProviderReference);
        Assert.Null(noReferencePayment.ProviderReference);
        Assert.Null(noReferenceRefund.ProviderReference);
        using (var paymentEvidence = JsonDocument.Parse(paymentInsta.SourceEvidence))
            Assert.Contains(paymentEvidence.RootElement.GetProperty("Payment").GetProperty("Parts").EnumerateArray(),
                x => x.GetProperty("FinancialAccountId").GetGuid() == insta.Id && x.GetProperty("ProviderReference").GetString() == paymentReference);
        using (var refundEvidence = JsonDocument.Parse(refundInsta.SourceEvidence))
            Assert.Contains(refundEvidence.RootElement.GetProperty("Refund").GetProperty("Parts").EnumerateArray(),
                x => x.GetProperty("FinancialAccountId").GetGuid() == insta.Id && x.GetProperty("ProviderReference").GetString() == refundReference);

        await CloseReportingShifts();
        await Monthly().CloseAsync(month.Id, new(Guid.NewGuid(), 0, "Freeze evidence"));
        var download = await Monthly().ExportAsync(month.Id);
        using var archive = new ZipArchive(new MemoryStream(download.Bytes));
        using var stream = archive.GetEntry("xl/worksheets/sheet2.xml")!.Open();
        var xml = XDocument.Load(stream);
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var rows = xml.Descendants(ns + "row").ToArray();
        var headings = rows[2].Elements(ns + "c").Select(x => x.Value).ToArray();
        var idColumn = Array.IndexOf(headings, nameof(FinancialTransactionReportRow.LedgerId));
        var referenceColumn = Array.IndexOf(headings, nameof(FinancialTransactionReportRow.ProviderReference));
        Assert.True(idColumn >= 0 && referenceColumn >= 0);
        var values = rows.Skip(3).Take(report.Transactions.Count)
            .ToDictionary(x => x.Elements(ns + "c").ElementAt(idColumn).Value,
                x => x.Elements(ns + "c").ElementAt(referenceColumn).Value);
        Assert.Equal(paymentReference, values[paymentInsta.LedgerId.ToString()]);
        Assert.Equal(refundReference, values[refundInsta.LedgerId.ToString()]);
        Assert.Equal("", values[paymentCash.LedgerId.ToString()]);
        Assert.Equal("", values[refundCash.LedgerId.ToString()]);
        Assert.Equal("", values[noReferencePayment.LedgerId.ToString()]);
        Assert.Equal("", values[noReferenceRefund.LedgerId.ToString()]);
        Assert.Empty(xml.Descendants(ns + "f"));
        Assert.Equal(download.Bytes, (await Monthly().ExportAsync(month.Id)).Bytes);
    }

    [Fact]
    public async Task MonthlyReportKeepsMultipleReferencesOnOneAccountMovementWithoutDuplicatingLedgerAmount()
    {
        var month = await ReportingMonth(today);
        var insta = await Account(FinancialAccountType.InstaPay);
        var sale = await Finance().CreateAsync(Purchase(new(insta.Id, 400, "IP-A"), new(insta.Id, 800, "IP-B")));
        await Finance().RefundAsync(sale.SaleId, new(Guid.NewGuid(), 300,
            [new(insta.Id, 100, "RF-A"), new(insta.Id, 200, "RF-B")], "Partial refund"));

        var report = await Monthly().ReportAsync(month.Id);
        Assert.Equal(2, report.Transactions.Count);
        Assert.Equal(2, await db.FinancialTransactions.CountAsync());
        Assert.Equal(1200, report.Summary.LedgerCredits);
        Assert.Equal(300, report.Summary.LedgerDebits);
        var payment = Assert.Single(report.Transactions, x => x.PaymentId != null);
        var refund = Assert.Single(report.Transactions, x => x.RefundId != null);
        Assert.Equal(1200, payment.Amount);
        Assert.Equal(300, refund.Amount);
        using var paymentRefs = JsonDocument.Parse(payment.ProviderReference!);
        using var refundRefs = JsonDocument.Parse(refund.ProviderReference!);
        Assert.Equal(new[] { "IP-A", "IP-B" }, paymentRefs.RootElement.EnumerateArray().Select(x => x.GetString()).Order());
        Assert.Equal(new[] { "RF-A", "RF-B" }, refundRefs.RootElement.EnumerateArray().Select(x => x.GetString()).Order());
        using var paymentEvidence = JsonDocument.Parse(payment.SourceEvidence);
        using var refundEvidence = JsonDocument.Parse(refund.SourceEvidence);
        Assert.Equal(2, paymentEvidence.RootElement.GetProperty("Payment").GetProperty("Parts").GetArrayLength());
        Assert.Equal(2, refundEvidence.RootElement.GetProperty("Refund").GetProperty("Parts").GetArrayLength());

        var bytes = MonthlyWorkbook.Create(report);
        using var archive = new ZipArchive(new MemoryStream(bytes));
        using var stream = archive.GetEntry("xl/worksheets/sheet2.xml")!.Open();
        var xml = XDocument.Load(stream);
        XNamespace ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        var rows = xml.Descendants(ns + "row").ToArray();
        var headings = rows[2].Elements(ns + "c").Select(x => x.Value).ToArray();
        var idColumn = Array.IndexOf(headings, nameof(FinancialTransactionReportRow.LedgerId));
        var referenceColumn = Array.IndexOf(headings, nameof(FinancialTransactionReportRow.ProviderReference));
        var values = rows.Skip(3).Take(report.Transactions.Count)
            .ToDictionary(x => x.Elements(ns + "c").ElementAt(idColumn).Value,
                x => x.Elements(ns + "c").ElementAt(referenceColumn).Value);
        Assert.Equal(payment.ProviderReference, values[payment.LedgerId.ToString()]);
        Assert.Equal(refund.ProviderReference, values[refund.LedgerId.ToString()]);
    }
}
