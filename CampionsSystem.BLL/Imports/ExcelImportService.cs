using System.Globalization;
using System.Text.Json;
using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Crm;
using CampionsSystem.BLL.Members;
using CampionsSystem.BLL.Operations;
using CampionsSystem.BLL.Shifts;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;

namespace CampionsSystem.BLL.Imports;

public sealed record ExcelImportRowResult(int RowNumber, string Status, string? Message, Guid? CreatedRecordId,
    IReadOnlyDictionary<string, string> Values);
public sealed record ExcelImportResult(Guid ImportId, string Kind, DateTimeOffset ExpiresAt, int TotalRows,
    int ValidRows, int InvalidRows, int DuplicateRows, int CreatedRows, int FailedRows,
    IReadOnlyList<ExcelImportRowResult> Rows, IReadOnlyList<string> Warnings);

// Preview persists only server-owned requests. Each confirmed row and its result commit in one transaction.
public sealed class ExcelImportService(ApplicationDbContext db, ICurrentUserService current, OperationAccess access,
    LeadManagementService leads, MemberManagementService members, IPermissionService permissions)
{
    public async Task<byte[]> TemplateAsync(string kind, CancellationToken ct)
    {
        await RequireAsync(kind, ct);
        return ExcelImportWorkbook.Template(kind);
    }

    private async Task<CurrentUserContext> RequireAsync(string kind, CancellationToken ct)
    {
        ExcelImportWorkbook.Columns(kind);
        var actor = await current.GetAsync(ct) ?? throw new UnauthorizedAccessException();
        var permission = kind == "leads" ? "Leads.Manage" : "Members.Create";
        if (!actor.Permissions.Contains(permission)) throw new UnauthorizedAccessException();
        if (kind == "freelance-customers" && !actor.Permissions.Contains("FreelancePlans.ManageEligibleMembers"))
            throw new UnauthorizedAccessException();
        return actor;
    }

    public async Task<ExcelImportResult> PreviewAsync(string kind, string fileName, byte[] bytes, CancellationToken ct)
    {
        var actor = await RequireAsync(kind, ct);
        if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase) || fileName.Length > 255)
            throw new BusinessRuleException("Upload an .xlsx workbook.");
        var sourceRows = ExcelImportWorkbook.Parse(kind, bytes);
        var batch = new ExcelImportBatch { CreatedByUserId = actor.UserId, Kind = kind, CreatedAt = access.Now,
            ExpiresAt = access.Now.AddMinutes(30) };
        var phones = new HashSet<string>(StringComparer.Ordinal);
        foreach (var (number, values) in sourceRows)
        {
            string status = "Ready", message = "";
            object? request = null;
            try
            {
                var made = await MakeRequestAsync(kind, values, actor, ct);
                request = made.Request;
                var normalized = made.Phone;
                if (!phones.Add(normalized)) { status = "Duplicate"; message = "Phone repeats within this workbook."; }
                else if (await PhoneIdentity.FindMemberAsync(db, normalized, ct) is not null
                    || await PhoneIdentity.FindLeadAsync(db, normalized, ct) is not null)
                { status = "Duplicate"; message = "Phone already belongs to a Member or Lead."; }
            }
            catch (BusinessRuleException e) { status = "Invalid"; message = e.Message; }
            catch (UnauthorizedAccessException) { status = "Invalid"; message = "Branch or action is not permitted."; }
            batch.Rows.Add(new ExcelImportRow { RowNumber = number,
                PayloadJson = JsonSerializer.Serialize(new { Values = values, Request = request is null ? (JsonElement?)null : JsonSerializer.SerializeToElement(request) }),
                Status = status,
                Message = message.Length == 0 ? null : message });
        }
        db.ExcelImportBatches.Add(batch);
        access.Audit(actor.UserId, actor.CurrentBranchId, "ExcelImport.Previewed", nameof(ExcelImportBatch), batch.Id,
            after: new { kind, RowCount = batch.Rows.Count, batch.ExpiresAt });
        await db.SaveChangesAsync(ct);
        return Result(batch, batch.Rows.OrderBy(x => x.RowNumber).ToArray());
    }

    private async Task<(object Request, string Phone)> MakeRequestAsync(string kind, Dictionary<string, string> cells,
        CurrentUserContext actor, CancellationToken ct)
    {
        string Get(string name) => cells.GetValueOrDefault(name, "");
        Guid RequiredGuid(string name) => Guid.TryParse(Get(name), out var id) && id != Guid.Empty ? id
            : throw new BusinessRuleException($"{name} must be a nonempty UUID.");
        var branchId = RequiredGuid(kind == "leads" ? "BranchId" : "TargetRegistrationBranchId");
        if (kind != "members" || Get("CreationType") != "NormalSales" || branchId == actor.CurrentBranchId
            || !actor.Permissions.Contains("Members.CreateCrossBranch"))
            BranchAccessService.EnsureScope(actor, branchId);
        var branch = await db.GymBranches.AsNoTracking().SingleOrDefaultAsync(b => b.Id == branchId && b.IsActive, ct)
            ?? throw new BusinessRuleException("An active registration branch is required.");
        var phone = PhoneNormalizer.Normalize(Get("PrimaryPhone"), branch.CountryCallingCode);
        var name = Get("FullName");
        if (name.Length > 256 || Get("SecondaryPhone").Length > 64 || Get("Notes").Length > 4096
            || kind != "leads" && string.IsNullOrWhiteSpace(name)) throw new BusinessRuleException("Invalid name or profile field length.");
        if (kind == "leads")
        {
            if (Get("Email").Length > 320 || Get("Source").Length > 256
                || Get("Email").Length > 0 && !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(Get("Email")))
                throw new BusinessRuleException("Invalid Lead email or source.");
            if (!actor.Permissions.Contains("Leads.Assign") && (actor.EmployeeId is null || !actor.Permissions.Contains("Members.OwnSalesCustomers")))
                throw new UnauthorizedAccessException();
            return (new CreateLeadRequest { BranchId = branchId, PrimaryPhone = Get("PrimaryPhone"),
                FullName = name, SecondaryPhone = Get("SecondaryPhone"), Email = Get("Email"),
                Source = Get("Source"), Notes = Get("Notes") }, phone);
        }
        DateOnly? dob = Get("DateOfBirth").Length == 0 ? null : DateOnly.TryParseExact(Get("DateOfBirth"), "yyyy-MM-dd",
            CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed) ? parsed : throw new BusinessRuleException("DateOfBirth must be YYYY-MM-DD.");
        if (dob > access.Today || Get("Gender").Length > 32) throw new BusinessRuleException("Invalid date of birth or gender.");
        if (actor.CurrentBranchId is not Guid operational)
            throw new BusinessRuleException("Open your own employee shift before importing Members.");
        await new FinancialOperations(db, current, access).RequireShiftAsync(operational, actor.UserId, ct);
        var request = new CreateMemberRequest { TargetRegistrationBranchId = branchId, PrimaryPhone = Get("PrimaryPhone"),
            FullName = name, SecondaryPhone = Get("SecondaryPhone"), DateOfBirth = dob, Gender = Get("Gender"), Notes = Get("Notes") };
        if (kind == "freelance-customers")
        {
            request.CreationType = MemberCreationType.FreelanceHouse;
            request.FreelancePlanId = RequiredGuid("FreelancePlanId");
            var plan = await db.MembershipPlans.AsNoTracking().SingleOrDefaultAsync(p => p.Id == request.FreelancePlanId && p.IsActive
                && p.PlanCategory == PlanCategory.Freelance && p.CityId == branch.CityId, ct);
            if (plan is null || plan.BranchId != actor.CurrentBranchId && !actor.Permissions.Contains("Branches.AccessAll"))
                throw new BusinessRuleException("An active, authorized Freelance plan in the registration city is required.");
            if (!await db.Employees.AsNoTracking().AnyAsync(e => e.Id == plan.FreelanceCoachEmployeeId && e.EmploymentType == EmploymentType.Freelance
                && e.EmploymentStatus == EmploymentStatus.Active && e.HireDate <= access.Today
                && (e.TerminationDate == null || e.TerminationDate >= access.Today), ct))
                throw new BusinessRuleException("The Freelance coach is inactive or ineligible.");
        }
        else
        {
            if (!Enum.TryParse<MemberCreationType>(Get("CreationType"), false, out var type)
                || type is not (MemberCreationType.NormalSales or MemberCreationType.ManagementCreated))
                throw new BusinessRuleException("CreationType must be NormalSales or ManagementCreated.");
            request.CreationType = type;
            if (type == MemberCreationType.NormalSales)
            {
                if (Get("SalesEmployeeId").Length > 0) throw new BusinessRuleException("NormalSales must not set SalesEmployeeId.");
                if (actor.EmployeeId is null || !actor.Permissions.Contains("Members.OwnSalesCustomers")) throw new UnauthorizedAccessException();
                if (branchId != actor.CurrentBranchId && !actor.Permissions.Contains("Members.CreateCrossBranch")) throw new UnauthorizedAccessException();
            }
            else
            {
                if (!actor.Permissions.Contains("Members.AssignSales")) throw new UnauthorizedAccessException();
                request.SalesEmployeeId = RequiredGuid("SalesEmployeeId");
                var selected = await db.Employees.AsNoTracking().Include(e => e.User).SingleOrDefaultAsync(e => e.Id == request.SalesEmployeeId, ct);
                if (selected?.UserId is not Guid selectedUser || selected.User is not { IsActive: true }
                    || selected.User.LockoutEnabled && selected.User.LockoutEnd > access.Now
                    || selected.EmploymentStatus != EmploymentStatus.Active || selected.HireDate > access.Today
                    || selected.TerminationDate < access.Today
                    || !(await permissions.GetUserPermissionsAsync(selectedUser, ct)).Contains("Members.OwnSalesCustomers"))
                    throw new BusinessRuleException("An eligible active Sales owner is required.");
            }
        }
        return (request, phone);
    }

    public async Task<ExcelImportResult> GetAsync(Guid id, CancellationToken ct)
    {
        var actor = await current.GetAsync(ct) ?? throw new UnauthorizedAccessException();
        var batch = await db.ExcelImportBatches.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new BranchNotFoundException("Import not found.");
        if (batch.CreatedByUserId != actor.UserId || batch.ExpiresAt < access.Now) throw new UnauthorizedAccessException();
        await RequireAsync(batch.Kind, ct);
        var rows = await db.ExcelImportRows.AsNoTracking().Where(x => x.BatchId == id).OrderBy(x => x.RowNumber).ToArrayAsync(ct);
        return Result(batch, rows);
    }

    public async Task<ExcelImportResult> ConfirmAsync(Guid id, CancellationToken ct)
    {
        var batch = await db.ExcelImportBatches.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new BranchNotFoundException("Import not found.");
        var actor = await RequireAsync(batch.Kind, ct);
        if (batch.CreatedByUserId != actor.UserId || batch.ExpiresAt < access.Now) throw new UnauthorizedAccessException();
        var ids = await db.ExcelImportRows.AsNoTracking().Where(x => x.BatchId == id && x.Status == "Ready")
            .OrderBy(x => x.RowNumber).Select(x => x.Id).ToArrayAsync(ct);
        foreach (var rowId in ids)
        {
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(ct);
            var row = await db.ExcelImportRows.FromSqlInterpolated($"SELECT * FROM \"ExcelImportRows\" WHERE \"Id\"={rowId} FOR UPDATE")
                .SingleAsync(ct);
            if (row.Status != "Ready") { await transaction.CommitAsync(ct); continue; }
            // Recheck current account, capability, branch and shift on every row. The manual service rechecks under its own locks.
            actor = await RequireAsync(batch.Kind, ct);
            if (actor.UserId != batch.CreatedByUserId || batch.ExpiresAt < access.Now) throw new UnauthorizedAccessException();
            try
            {
                if (batch.Kind == "leads")
                {
                    using var payload = JsonDocument.Parse(row.PayloadJson);
                    var request = payload.RootElement.GetProperty("Request").Deserialize<CreateLeadRequest>()!;
                    var created = await leads.CreateLeadAsync(request, ct);
                    row.Status = created.Result == "Created" ? "Created" : "Duplicate";
                    row.CreatedRecordId = created.Result == "Created" ? created.Lead?.Id : null;
                    row.Message = created.Result == "Created" ? null : created.Result;
                }
                else
                {
                    using var payload = JsonDocument.Parse(row.PayloadJson);
                    var request = payload.RootElement.GetProperty("Request").Deserialize<CreateMemberRequest>()!;
                    var created = await members.CreateMemberAsync(request, ct);
                    row.Status = "Created"; row.CreatedRecordId = created.Id; row.Message = null;
                }
            }
            catch (BusinessRuleException e)
            {
                db.ChangeTracker.Clear();
                row = await db.ExcelImportRows.FromSqlInterpolated($"SELECT * FROM \"ExcelImportRows\" WHERE \"Id\"={rowId} FOR UPDATE").SingleAsync(ct);
                row.Status = "Failed"; row.Message = e.Message[..Math.Min(e.Message.Length, 2048)];
            }
            catch (UnauthorizedAccessException)
            {
                db.ChangeTracker.Clear();
                row = await db.ExcelImportRows.FromSqlInterpolated($"SELECT * FROM \"ExcelImportRows\" WHERE \"Id\"={rowId} FOR UPDATE").SingleAsync(ct);
                row.Status = "Failed"; row.Message = "Branch or action is no longer permitted.";
            }
            access.Audit(actor.UserId, actor.CurrentBranchId, "ExcelImport.RowConfirmed", nameof(ExcelImportRow), row.Id,
                after: new { batch.Id, row.RowNumber, row.Status, row.CreatedRecordId, row.Message });
            await db.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        return await GetAsync(id, ct);
    }

    private static ExcelImportResult Result(ExcelImportBatch batch, IReadOnlyList<ExcelImportRow> rows)
    {
        var results = rows.Select(r =>
        {
            using var payload = JsonDocument.Parse(r.PayloadJson);
            var values = payload.RootElement.GetProperty("Values").Deserialize<Dictionary<string, string>>()!;
            return new ExcelImportRowResult(r.RowNumber, r.Status, r.Message, r.CreatedRecordId, values);
        }).ToArray();
        return new(batch.Id, batch.Kind, batch.ExpiresAt, rows.Count, results.Count(r => r.Status == "Ready"),
            results.Count(r => r.Status == "Invalid"), results.Count(r => r.Status == "Duplicate"),
            results.Count(r => r.Status == "Created"), results.Count(r => r.Status == "Failed"), results,
            ["Preview is advisory. Confirmation rechecks current permissions, branch scope, Shift, eligibility and phone uniqueness."]);
    }
}
