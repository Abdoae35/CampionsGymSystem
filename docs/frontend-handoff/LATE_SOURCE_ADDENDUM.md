# Late source verification addendum

Reviewed **2026-10-12T10:55:15.206924+00:00**, HEAD `eb4b538e625c8bcf535e848167835e10ff484c1a`. Concurrent work added one controller action after the frozen OpenAPI capture: **191 current controller actions / 36 Coaching**, versus 190/35 in captured branch snapshot. All 191 now have handoff entries; stable baseline remains 155. Current total including health/document registrations = **193**. Captured branch OpenAPI still truthfully contains 191 operations (190 earlier actions plus health); it was not fabricated/edited to insert the late route. Use the source-derived addition below for this pending route.

DTO contract files were hash-compared with the frozen branch snapshot: **no exposed DTO contract file drift**. Changed controller/business files: CoachingController, CoachingService.Classes and Packages. A later source check confirms package ordering before projection (query translation fix), a new Member attendance read, and the occurrence timestamp guard below. Other captured controller files unchanged. No new runtime business tests were run by this audit.

## Added current route

**GET `/api/coaching/classes/members/{member:guid}/attendance`** — action ClassMemberAttendance, source `CampionsSystem.API/Controllers/CoachingController.cs:49`; BLL ClassMemberHistoryAsync, `BLL/Coaching/CoachingService.Classes.cs:128`.

Status: **VERIFIED FROM CODE / branch-only uncommitted at read; not stable master**. Bearer authentication and exact `Permission:Classes.View`. Read is branch-scoped, not coach-owner-only; Branches.AccessAll enables broader target scope alongside Classes.View. No own Shift needed.

Path: member UUID. Query: branchId UUID (explicit target; send authorized operational branch), page int default1, pageSize int default25 (1..100); page >=1 and overflow-safe Skip. No body/content-type, operation key, version or ETag. Authorization header only. No arbitrary date/sort filter.

Response **200**: [CoachingPage<ClassAttendanceDetails>](branch-only/DTO_AND_ENUM_REFERENCE.md#campionssystem-bll-coaching-coachingpage-campionssystem-bll-coaching-classattendancedetails), fields items/totalCount/page/pageSize. Nested attendance fields id/memberId/subscriptionId/classSessionId/coachEmployeeId/businessDate/remainingSessions/registeredAt; exact type/nullability is in linked DTO. Filters stored CheckIn branch+Member with ClassesSubscriptionId nonnull; descending CheckedInAt then Id. Nonexistent member/no history returns empty page (no explicit member 404). 400 bad pagination, 401/403 identity/capability/scope; GUID route mismatch404; unforeseen500 ProblemDetails. No mutation idempotency/conflict UX required.

FAKE request:

```http
GET /api/coaching/classes/members/33333333-3333-4333-8333-333333333333/attendance?branchId=99999999-9999-4999-8999-999999999999&page=1&pageSize=25 HTTP/1.1
Authorization: Bearer <TOKEN_PLACEHOLDER>
```

FAKE possible response:

```json
{"items":[],"totalCount":0,"page":1,"pageSize":25}
```

Actual source adapter and BLL:

```csharp
[HttpGet("classes/members/{member:guid}/attendance"), Authorize(Policy = "Permission:Classes.View")]
public Task<IActionResult> ClassMemberAttendance(Guid member, Guid branchId, int page = 1, int pageSize = 25, CancellationToken ct = default) => Execute(() => coaching.ClassMemberHistoryAsync(branchId, member, page, pageSize, ct));

public async Task<CoachingPage<ClassAttendanceDetails>> ClassMemberHistoryAsync(Guid branch, Guid member, int page = 1, int pageSize = 25, CancellationToken ct = default)
    {
        Page(page, pageSize); await access.RequireAsync(CoachingPermissions.ClassesView, branch, ct);
        var q = db.CheckIns.AsNoTracking().Where(x => x.BranchId == branch && x.MemberId == member && x.ClassesSubscriptionId != null);
        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(x => x.CheckedInAt).ThenBy(x => x.Id).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return new(rows.Select(ClassAttendance).ToArray(), total, page, pageSize);
    }
```

## Later occurrence validation correction

Current source now requires `r.StartsAt != default`, `r.StartsAt >= access.Now`, and `access.LocalDate(r.StartsAt) == access.Today`. It still stamps effective branch BusinessDate; gym-local calendar Today and an overnight open BusinessDay can differ. Earlier branch snapshot concern about unrestricted future dates is **superseded by this source change**. Do not keep the obsolete concern as an unresolved policy blocker.

## Later verification reported by separate feature work

**DOCUMENTED ONLY, not audit execution**: updated project history reports 84/84 focused tests passed, zero failures/skips, clean backend rebuild, a new forward completeness migration, and 37 Postman examples covering 36 routes. Operational publication/final full suite remain pending at that history checkpoint. This audit does not turn that report into independently tested full release status. No application/Production migration was applied by this task.
