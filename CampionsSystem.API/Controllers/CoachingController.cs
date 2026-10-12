using CampionsSystem.BLL.Coaching;
using CampionsSystem.BLL.Sales;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace CampionsSystem.API.Controllers;

[ApiController]
[Route("api/coaching")]
[Authorize]
public sealed class CoachingController(CoachingService coaching, SalesService sales) : BranchControllerBase
{
    [HttpGet("private/packages"), Authorize(Policy = "Permission:Private.View")]
    public Task<IActionResult> PrivatePackages(Guid branchId, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.PackagesAsync(branchId, false, page, pageSize, ct));
    [HttpPost("private/packages"), Authorize(Policy = "Permission:Private.Manage")]
    public Task<IActionResult> CreatePrivatePackage(CoachingPackageRequest r, CancellationToken ct) => Execute(() => coaching.SavePackageAsync(null, r, false, ct));
    [HttpPut("private/packages/{id:guid}"), Authorize(Policy = "Permission:Private.Manage")]
    public Task<IActionResult> UpdatePrivatePackage(Guid id, CoachingPackageRequest r, CancellationToken ct) => Execute(() => coaching.SavePackageAsync(id, r, false, ct));
    [HttpPost("private/purchases"), Authorize(Policy = "Permission:Private.Manage"), Authorize(Policy = "Permission:Payments.Create")]
    public Task<IActionResult> PurchasePrivate(CoachingPurchaseRequest r, CancellationToken ct) => Execute(() => sales.PurchaseCoachingAsync(r, false, ct));
    [HttpGet("private/members"), Authorize(Policy = "Permission:Private.View")]
    public Task<IActionResult> PrivateMembers(Guid branchId, string search, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.SearchAsync(branchId, false, search, page, pageSize, ct));
    [HttpGet("private/subscriptions"), Authorize(Policy = "Permission:Private.View")]
    public Task<IActionResult> PrivateSubscriptions(Guid branchId, Guid memberId, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.PrivateSubscriptionsAsync(branchId, memberId, page, pageSize, ct));
    [HttpGet("private/subscriptions/{id:guid}"), Authorize(Policy = "Permission:Private.View")]
    public Task<IActionResult> PrivateSubscription(Guid id, CancellationToken ct) => Execute(() => coaching.PrivateDetailsAsync(id, ct));
    [HttpPost("private/subscriptions/{id:guid}/sessions"), Authorize(Policy = "Permission:Private.RegisterSession")]
    public Task<IActionResult> RegisterPrivate(Guid id, PrivateAttendanceRequest r, CancellationToken ct) => Execute(() => coaching.RegisterPrivateAsync(id, r, ct));
    [HttpGet("private/subscriptions/{id:guid}/sessions"), Authorize(Policy = "Permission:Private.View")]
    public Task<IActionResult> PrivateSessions(Guid id, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.PrivateHistoryAsync(id, page, pageSize, ct));
    [HttpPost("private/subscriptions/{id:guid}/transfers"), Authorize(Policy = "Permission:Private.Transfer")]
    public Task<IActionResult> Transfer(Guid id, PrivateTransferRequest r, CancellationToken ct) => Execute(() => coaching.TransferAsync(id, r, ct));
    [HttpPost("private/subscriptions/{id:guid}/expiry-settlements"), Authorize(Policy = "Permission:Private.SettleExpiry")]
    public Task<IActionResult> SettleExpiry(Guid id, PrivateExpiryRequest r, CancellationToken ct) => Execute(() => coaching.SettleExpiryAsync(id, r, ct));
    [HttpGet("private/subscriptions/{id:guid}/expiry-settlements"), Authorize(Policy = "Permission:Private.View")]
    public Task<IActionResult> ExpiryHistory(Guid id, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.ExpiryHistoryAsync(id, page, pageSize, ct));

    [HttpGet("classes/packages"), Authorize(Policy = "Permission:Classes.View")]
    public Task<IActionResult> ClassesPackages(Guid branchId, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.PackagesAsync(branchId, true, page, pageSize, ct));
    [HttpPost("classes/packages"), Authorize(Policy = "Permission:Classes.Manage")]
    public Task<IActionResult> CreateClassesPackage(CoachingPackageRequest r, CancellationToken ct) => Execute(() => coaching.SavePackageAsync(null, r, true, ct));
    [HttpPut("classes/packages/{id:guid}"), Authorize(Policy = "Permission:Classes.Manage")]
    public Task<IActionResult> UpdateClassesPackage(Guid id, CoachingPackageRequest r, CancellationToken ct) => Execute(() => coaching.SavePackageAsync(id, r, true, ct));
    [HttpPost("classes/purchases"), Authorize(Policy = "Permission:Classes.Manage"), Authorize(Policy = "Permission:Payments.Create")]
    public Task<IActionResult> PurchaseClasses(CoachingPurchaseRequest r, CancellationToken ct) => Execute(() => sales.PurchaseCoachingAsync(r, true, ct));
    [HttpGet("classes/members"), Authorize(Policy = "Permission:Classes.View")]
    public Task<IActionResult> ClassMembers(Guid branchId, string search, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.SearchAsync(branchId, true, search, page, pageSize, ct));
    [HttpGet("classes/subscriptions"), Authorize(Policy = "Permission:Classes.View")]
    public Task<IActionResult> ClassSubscriptions(Guid branchId, Guid memberId, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.ClassesSubscriptionsAsync(branchId, memberId, page, pageSize, ct));
    [HttpGet("classes/members/{member:guid}/attendance"), Authorize(Policy = "Permission:Classes.View")]
    public Task<IActionResult> ClassMemberAttendance(Guid member, Guid branchId, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.ClassMemberHistoryAsync(branchId, member, page, pageSize, ct));
    [HttpGet("classes/types"), Authorize(Policy = "Permission:Classes.View")]
    public Task<IActionResult> ClassTypes(Guid branchId, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.ClassTypesAsync(branchId, page, pageSize, ct));
    [HttpPost("classes/types"), Authorize(Policy = "Permission:Classes.Manage")]
    public Task<IActionResult> CreateClassType(ClassTypeRequest r, CancellationToken ct) => Execute(() => coaching.SaveClassTypeAsync(null, r, ct));
    [HttpPut("classes/types/{id:guid}"), Authorize(Policy = "Permission:Classes.Manage")]
    public Task<IActionResult> UpdateClassType(Guid id, ClassTypeRequest r, CancellationToken ct) => Execute(() => coaching.SaveClassTypeAsync(id, r, ct));
    [HttpGet("classes/occurrences"), Authorize(Policy = "Permission:Classes.View")]
    public Task<IActionResult> Occurrences(Guid branchId, DateOnly from, DateOnly to, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.OccurrencesAsync(branchId, from, to, page, pageSize, ct));
    [HttpPost("classes/occurrences"), Authorize(Policy = "Permission:Classes.Manage")]
    public Task<IActionResult> CreateOccurrence(ClassOccurrenceRequest r, CancellationToken ct) => Execute(() => coaching.CreateOccurrenceAsync(r, ct));
    [HttpPost("classes/occurrences/{id:guid}/complete"), Authorize(Policy = "Permission:Classes.Manage")]
    public Task<IActionResult> CompleteOccurrence(Guid id, CompleteClassRequest r, CancellationToken ct) => Execute(() => coaching.CompleteOccurrenceAsync(id, r, ct));
    [HttpPost("classes/occurrences/{id:guid}/attendance"), Authorize(Policy = "Permission:Classes.RegisterAttendance")]
    public Task<IActionResult> RegisterClass(Guid id, ClassAttendanceRequest r, CancellationToken ct) => Execute(() => coaching.RegisterClassAsync(id, r, ct));
    [HttpGet("classes/occurrences/{id:guid}/attendance"), Authorize(Policy = "Permission:Classes.View")]
    public Task<IActionResult> ClassAttendance(Guid id, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.ClassAttendanceHistoryAsync(id, page, pageSize, ct));
    [HttpGet("classes/coaches/{coach:guid}/statistics"), Authorize(Policy = "Permission:Classes.ViewAttendanceReport")]
    public Task<IActionResult> ClassStatistics(Guid coach, Guid branchId, DateOnly from, DateOnly to, Guid? periodId = null, CancellationToken ct = default) => Execute(() => coaching.ClassStatisticsAsync(branchId, coach, from, to, periodId, ct));

    [HttpPost("earnings/targets"), Authorize(Policy = "Permission:CoachEarnings.ManageTargets")]
    public Task<IActionResult> Targets(CoachTargetRequest r, CancellationToken ct) => Execute(() => coaching.ConfigureTargetsAsync(r, ct));
    [HttpGet("earnings/periods/{period:guid}/targets"), Authorize(Policy = "Permission:CoachEarnings.View")]
    public Task<IActionResult> TargetHistory(Guid period, CancellationToken ct) => Execute(() => coaching.TargetsAsync(period, ct));
    [HttpGet("earnings/periods/{period:guid}/progress"), Authorize(Policy = "Permission:CoachEarnings.View")]
    public Task<IActionResult> Progress(Guid period, CancellationToken ct) => Execute(() => coaching.ProgressAsync(period, ct));
    [HttpPost("earnings/recognitions"), Authorize(Policy = "Permission:CoachEarnings.Recognize")]
    public Task<IActionResult> Recognize(RecognizeCoachEarningsRequest r, CancellationToken ct) => Execute(() => coaching.RecognizeAsync(r, ct));
    [HttpGet("earnings/coaches/{coach:guid}/balance"), Authorize(Policy = "Permission:CoachEarnings.View")]
    public Task<IActionResult> Balance(Guid coach, Guid branchId, CancellationToken ct) => Execute(() => coaching.BalanceAsync(branchId, coach, ct));
    [HttpGet("earnings/coaches/{coach:guid}/history"), Authorize(Policy = "Permission:CoachEarnings.View")]
    public Task<IActionResult> EarningsHistory(Guid coach, Guid branchId, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.EarningsHistoryAsync(branchId, coach, page, pageSize, ct));
    [HttpPost("earnings/payouts"), Authorize(Policy = "Permission:CoachEarnings.Pay")]
    public Task<IActionResult> Payout(CoachPayoutRequest r, CancellationToken ct) => Execute(() => coaching.PayAsync(r, ct));
    [HttpGet("earnings/coaches/{coach:guid}/payouts"), Authorize(Policy = "Permission:CoachEarnings.View")]
    public Task<IActionResult> PayoutHistory(Guid coach, Guid branchId, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.PayoutHistoryAsync(branchId, coach, page, pageSize, ct));
}
