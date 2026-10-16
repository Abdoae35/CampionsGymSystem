using System.IO.Compression;
using System.Security;
using System.Text;
using CampionsSystem.BLL.Authentication;
using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Crm;
using CampionsSystem.BLL.Imports;
using CampionsSystem.BLL.Members;
using CampionsSystem.BLL.Operations;
using CampionsSystem.DAL.Data;
using CampionsSystem.DAL.Entities;
using CampionsSystem.DAL.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;

namespace CampionsSystem.DAL.Tests;

public sealed class ExcelImportTests : IAsyncLifetime
{
    private string schema = "gym_excel_import_" + Guid.NewGuid().ToString("N");
    private string connection = null!;
    private ApplicationDbContext db = null!;
    private TestBranchUser actor = null!;
    private OperationAccess access = null!;
    private ExcelImportService service = null!;
    private GymBranch branch = null!, other = null!;
    private Employee employee = null!, coach = null!;
    private MembershipPlan freelancePlan = null!;
    private readonly Permissions permissions = new();
    private sealed class Permissions : IPermissionService
    {
        public Task<HashSet<string>> GetUserPermissionsAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(new HashSet<string> { "Members.OwnSalesCustomers" });
    }

    public async Task InitializeAsync()
    {
        var admin = Environment.GetEnvironmentVariable("GYM_SCHEMA_TEST_CONNECTION")
            ?? $"Host=localhost;Database=postgres;Username={Environment.UserName};Timeout=5";
        var config = new NpgsqlConnectionStringBuilder(admin);
        if (config.Host is not ("localhost" or "127.0.0.1" or "::1")) throw new InvalidOperationException("Tests require localhost.");
        await using (var cn = new NpgsqlConnection(admin))
        {
            await cn.OpenAsync();
            await using var command = new NpgsqlCommand($"CREATE SCHEMA {schema}", cn);
            await command.ExecuteNonQueryAsync();
        }
        config.SearchPath = schema; config.Pooling = false; connection = config.ConnectionString;
        db = new(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection).Options);
        await db.Database.MigrateAsync();
        var user = new ApplicationUser { UserName = "excel-operator" };
        branch = new() { Name = "Tanta", CountryCallingCode = "+20", City = new() { Name = "Gharbia" } };
        other = new() { Name = "Other", CountryCallingCode = "+20", City = branch.City };
        employee = new() { User = user, FullName = "Operator", Phone = "employee", HireDate = DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1) };
        coach = new() { FullName = "Freelance Coach", Phone = "coach", EmploymentType = EmploymentType.Freelance,
            HireDate = employee.HireDate };
        freelancePlan = new() { Name = "Freelance", Branch = branch, City = branch.City, PlanCategory = PlanCategory.Freelance,
            FreelanceCoachEmployee = coach, DurationDays = 30, Price = 1000 };
        db.AddRange(branch, other, employee, coach, freelancePlan,
            new EmployeeBranchAssignment { Employee = employee, Branch = branch, StartDate = employee.HireDate,
                IsPrimary = true, AssignedByUser = user });
        await db.SaveChangesAsync();
        await OperationalShiftFixture.OpenAsync(db, user.Id, branch.Id);
        actor = new(new(user.Id, user.UserName, employee.Id, null, [],
            ["Leads.Manage", "Leads.Assign", "Members.Create", "Members.OwnSalesCustomers", "Members.AssignSales", "FreelancePlans.ManageEligibleMembers"],
            new(branch.Id, branch.Name)));
        access = new(actor, db, TimeProvider.System, Options.Create(new AuthContextOptions()));
        var members = new MemberManagementService(db, actor, access, permissions);
        var leads = new LeadManagementService(db, actor, access, permissions, members);
        service = new(db, actor, access, leads, members, permissions);
    }
    public async Task DisposeAsync()
    {
        if (db is not null) await db.DisposeAsync();
        if (connection is not null)
        {
            await using var cn = new NpgsqlConnection(connection); await cn.OpenAsync();
            await using var command = new NpgsqlCommand($"DROP SCHEMA IF EXISTS {schema} CASCADE", cn);
            await command.ExecuteNonQueryAsync();
        }
    }

    private static byte[] Data(string kind, params string[][] rows)
    {
        using var memory = new MemoryStream(); memory.Write(ExcelImportWorkbook.Template(kind)); memory.Position = 0;
        using (var zip = new ZipArchive(memory, ZipArchiveMode.Update, true))
        {
            zip.GetEntry("xl/worksheets/sheet1.xml")!.Delete();
            using var writer = new StreamWriter(zip.CreateEntry("xl/worksheets/sheet1.xml").Open(), new UTF8Encoding(false));
            writer.Write("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");
            var all = new[] { ExcelImportWorkbook.Columns(kind) }.Concat(rows).ToArray();
            for (var i = 0; i < all.Length; i++)
            {
                writer.Write($"<row r=\"{i + 1}\">");
                for (var j = 0; j < all[i].Length; j++)
                    writer.Write($"<c r=\"{(char)('A' + j)}{i + 1}\" t=\"inlineStr\"><is><t>{SecurityElement.Escape(all[i][j])}</t></is></c>");
                writer.Write("</row>");
            }
            writer.Write("</sheetData></worksheet>");
        }
        return memory.ToArray();
    }
    private static byte[] ChangeDataSheet(byte[] source, Func<string, string> change)
    {
        using var memory = new MemoryStream(); memory.Write(source); memory.Position = 0;
        using (var zip = new ZipArchive(memory, ZipArchiveMode.Update, true))
        {
            var entry = zip.GetEntry("xl/worksheets/sheet1.xml")!;
            string xml; using (var reader = new StreamReader(entry.Open())) xml = reader.ReadToEnd();
            entry.Delete();
            using var writer = new StreamWriter(zip.CreateEntry("xl/worksheets/sheet1.xml").Open());
            writer.Write(change(xml));
        }
        return memory.ToArray();
    }
    private string[] Lead(string phone = "01011111111") => [branch.Id.ToString(), phone, "Lead", "", "", "Instagram", ""];
    private string[] Member(string phone = "01022222222") => [branch.Id.ToString(), "NormalSales", phone, "Member", "", "", "", "", ""];
    private string[] Freelance(string phone = "01033333333") => [branch.Id.ToString(), freelancePlan.Id.ToString(), phone, "Freelance Customer", "", "", "", ""];

    [Theory]
    [InlineData("leads", 7)] [InlineData("members", 9)] [InlineData("freelance-customers", 8)]
    public async Task TemplatesAreValidAndDistinct(string kind, int columns)
    {
        var bytes = await service.TemplateAsync(kind, default);
        using var zip = new ZipArchive(new MemoryStream(bytes));
        Assert.Equal("Data", System.Xml.Linq.XDocument.Load(zip.GetEntry("xl/workbook.xml")!.Open())
            .Descendants().First(x => x.Name.LocalName == "sheet").Attribute("name")!.Value);
        Assert.Equal(columns, ExcelImportWorkbook.Columns(kind).Length);
        Assert.NotNull(zip.GetEntry("xl/worksheets/sheet2.xml"));
        Assert.True(bytes.Length > 100);
    }
    [Fact] public async Task LeadPreviewIsReadOnlyAndConfirmIsIdempotentWithMixedRows()
    {
        var sheet = Data("leads", Lead(), Lead("+201011111111"), Lead("bad"), Lead("01044444444"));
        var preview = await service.PreviewAsync("leads", "leads.xlsx", sheet, default);
        Assert.Equal(4, preview.TotalRows); Assert.Equal(2, preview.ValidRows); Assert.Equal(1, preview.InvalidRows);
        Assert.Equal(1, preview.DuplicateRows); Assert.Equal(0, await db.Leads.CountAsync());
        var confirmed = await service.ConfirmAsync(preview.ImportId, default);
        Assert.Equal(2, confirmed.CreatedRows); Assert.Equal(2, await db.Leads.CountAsync());
        Assert.Equal(2, (await service.ConfirmAsync(preview.ImportId, default)).CreatedRows);
        Assert.Equal(2, await db.Leads.CountAsync());
        Assert.Empty(await db.SaleTransactions.ToListAsync()); Assert.Empty(await db.FinancialTransactions.ToListAsync());
    }
    [Fact] public async Task NormalAndFreelanceImportsReuseManualRegistrationWithoutMoney()
    {
        var normal = await service.PreviewAsync("members", "members.xlsx", Data("members", Member()), default);
        Assert.Equal(0, await db.Members.CountAsync());
        Assert.Equal(1, (await service.ConfirmAsync(normal.ImportId, default)).CreatedRows);
        var freelance = await service.PreviewAsync("freelance-customers", "freelance.xlsx", Data("freelance-customers", Freelance()), default);
        Assert.Equal(1, (await service.ConfirmAsync(freelance.ImportId, default)).CreatedRows);
        Assert.Equal(2, await db.Members.CountAsync());
        Assert.Single(await db.MemberSalesAssignments.ToListAsync());
        Assert.Single(await db.MembershipPlanEligibleMembers.ToListAsync());
        Assert.Empty(await db.MemberSubscriptions.ToListAsync()); Assert.Empty(await db.SaleTransactions.ToListAsync());
        Assert.Empty(await db.Payments.ToListAsync()); Assert.Empty(await db.FinancialTransactions.ToListAsync());
    }
    [Fact] public async Task ManagementCreatedImportUsesTheExplicitEligibleSalesOwner()
    {
        var row = Member("01022222223"); row[1] = "ManagementCreated"; row[8] = employee.Id.ToString();
        var preview = await service.PreviewAsync("members", "members.xlsx", Data("members", row), default);
        Assert.Equal(1, preview.ValidRows);
        Assert.Equal(1, (await service.ConfirmAsync(preview.ImportId, default)).CreatedRows);
        Assert.Equal(employee.Id, (await db.MemberSalesAssignments.SingleAsync()).SalesEmployeeId);
        Assert.Empty(await db.MemberSubscriptions.ToListAsync());
        Assert.Empty(await db.FinancialTransactions.ToListAsync());
    }
    [Fact] public async Task ExistingPhoneWrongBranchInactivePlanAndShiftAreRejected()
    {
        var first = await service.PreviewAsync("leads", "leads.xlsx", Data("leads", Lead()), default);
        await service.ConfirmAsync(first.ImportId, default);
        var duplicate = await service.PreviewAsync("members", "members.xlsx", Data("members", Member("01011111111")), default);
        Assert.Equal(1, duplicate.DuplicateRows);
        var wrong = Member("01055555555"); wrong[0] = other.Id.ToString();
        var cross = await service.PreviewAsync("members", "members.xlsx", Data("members", wrong), default);
        Assert.Equal(1, cross.InvalidRows);
        var wrongLead = Lead("01055555556"); wrongLead[0] = other.Id.ToString();
        var crossLead = await service.PreviewAsync("leads", "leads.xlsx", Data("leads", wrongLead), default);
        Assert.Equal(1, crossLead.InvalidRows);
        await db.MembershipPlans.Where(x => x.Id == freelancePlan.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, false));
        var inactive = await service.PreviewAsync("freelance-customers", "freelance.xlsx", Data("freelance-customers", Freelance()), default);
        Assert.Equal(1, inactive.InvalidRows);
        await db.MembershipPlans.Where(x => x.Id == freelancePlan.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.IsActive, true));
        await db.Employees.Where(x => x.Id == coach.Id).ExecuteUpdateAsync(s => s.SetProperty(x => x.EmploymentStatus, EmploymentStatus.Suspended));
        var ineligibleCoach = await service.PreviewAsync("freelance-customers", "freelance.xlsx", Data("freelance-customers", Freelance()), default);
        Assert.Equal(1, ineligibleCoach.InvalidRows);
        await db.Shifts.ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, ShiftStatus.Closed));
        var missingShift = await service.PreviewAsync("members", "members.xlsx", Data("members", Member("01066666666")), default);
        Assert.Equal(1, missingShift.InvalidRows);
    }
    [Fact] public async Task MemberConfirmationRechecksTheOwnShiftAfterPreview()
    {
        var preview = await service.PreviewAsync("members", "members.xlsx", Data("members", Member("01022222224")), default);
        Assert.Equal(1, preview.ValidRows);
        await db.Shifts.ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, ShiftStatus.Closed));
        var confirmed = await service.ConfirmAsync(preview.ImportId, default);
        Assert.Equal(1, confirmed.FailedRows);
        Assert.Empty(await db.Members.ToListAsync());
        Assert.Empty(await db.FinancialTransactions.ToListAsync());
    }
    [Fact] public async Task RevocationAndStateChangeAreRecheckedAtConfirmation()
    {
        var preview = await service.PreviewAsync("leads", "leads.xlsx", Data("leads", Lead()), default);
        actor.Context = actor.Context! with { Permissions = [] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.ConfirmAsync(preview.ImportId, default));
        Assert.Empty(await db.Leads.ToListAsync());
        actor.Context = actor.Context! with { Permissions = ["Leads.Manage", "Leads.Assign"] };
        var preMember = await service.PreviewAsync("leads", "leads.xlsx", Data("leads", Lead("01077777777")), default);
        db.Members.Add(new() { HomeBranchId = branch.Id, FullName = "Existing", PrimaryPhone = "01077777777",
            PrimaryPhoneNormalized = "+201077777777", MemberCode = "EXISTING", CreatedByUserId = employee.UserId!.Value });
        await db.SaveChangesAsync();
        Assert.Equal(1, (await service.ConfirmAsync(preMember.ImportId, default)).DuplicateRows);
        Assert.Equal(0, await db.Leads.CountAsync());
    }
    [Fact] public async Task InvalidFilesAndMissingCapabilityFailSafely()
    {
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.PreviewAsync("leads", "data.xls", Data("leads", Lead()), default));
        await Assert.ThrowsAsync<BusinessRuleException>(() => service.PreviewAsync("leads", "data.xlsx", [1, 2, 3], default));
        actor.Context = actor.Context! with { Permissions = [] };
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => service.TemplateAsync("leads", default));
        Assert.Empty(await db.ExcelImportBatches.ToListAsync());
    }
    [Fact] public async Task ConcurrentConfirmationCreatesEachMemberOnlyOnce()
    {
        var preview = await service.PreviewAsync("members", "members.xlsx", Data("members", Member()), default);
        async Task<ExcelImportResult> ConfirmOnIndependentConnection()
        {
            await using var independent = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection).Options);
            var identity = new TestBranchUser(actor.Context!);
            var operation = new OperationAccess(identity, independent, TimeProvider.System, Options.Create(new AuthContextOptions()));
            var memberService = new MemberManagementService(independent, identity, operation, permissions);
            var leadService = new LeadManagementService(independent, identity, operation, permissions, memberService);
            return await new ExcelImportService(independent, identity, operation, leadService, memberService, permissions)
                .ConfirmAsync(preview.ImportId, default);
        }
        var results = await Task.WhenAll(ConfirmOnIndependentConnection(), ConfirmOnIndependentConnection());
        Assert.All(results, result => Assert.Equal(1, result.CreatedRows));
        Assert.Single(await db.Members.ToListAsync());
        Assert.Single(await db.MemberSalesAssignments.ToListAsync());
    }
    [Fact] public async Task TwoConcurrentLeadImportsCannotCreateTheSamePhone()
    {
        var first = await service.PreviewAsync("leads", "first.xlsx", Data("leads", Lead()), default);
        var second = await service.PreviewAsync("leads", "second.xlsx", Data("leads", Lead("+201011111111")), default);
        async Task<ExcelImportResult> Confirm(Guid id)
        {
            await using var independent = new ApplicationDbContext(new DbContextOptionsBuilder<ApplicationDbContext>().UseNpgsql(connection).Options);
            var identity = new TestBranchUser(actor.Context!);
            var operation = new OperationAccess(identity, independent, TimeProvider.System, Options.Create(new AuthContextOptions()));
            var memberService = new MemberManagementService(independent, identity, operation, permissions);
            var leadService = new LeadManagementService(independent, identity, operation, permissions, memberService);
            return await new ExcelImportService(independent, identity, operation, leadService, memberService, permissions).ConfirmAsync(id, default);
        }
        var results = await Task.WhenAll(Confirm(first.ImportId), Confirm(second.ImportId));
        Assert.Equal(1, results.Sum(x => x.CreatedRows));
        Assert.Equal(1, results.Sum(x => x.DuplicateRows));
        Assert.Single(await db.Leads.ToListAsync());
    }
    [Fact] public async Task MissingRequiredValuesAndInvalidDatesStayInPreview()
    {
        var missing = Member("01088888888"); missing[3] = "";
        var badDate = Member("01099999999"); badDate[5] = "31/12/2020";
        var result = await service.PreviewAsync("members", "members.xlsx", Data("members", missing, badDate), default);
        Assert.Equal(2, result.InvalidRows);
        Assert.Equal(0, (await service.ConfirmAsync(result.ImportId, default)).CreatedRows);
        Assert.Empty(await db.Members.ToListAsync());
    }
    [Fact] public void MalformedHeadersAndFormulaCellsAreRejected()
    {
        var valid = Data("leads", Lead());
        var badHeader = ChangeDataSheet(valid, xml => xml.Replace(">PrimaryPhone<", ">UnknownColumn<", StringComparison.Ordinal));
        Assert.Throws<BusinessRuleException>(() => ExcelImportWorkbook.Parse("leads", badHeader));
        var formula = ChangeDataSheet(valid, xml => xml.Replace("<is><t>01011111111", "<f>1+1</f><is><t>01011111111", StringComparison.Ordinal));
        Assert.Throws<BusinessRuleException>(() => ExcelImportWorkbook.Parse("leads", formula));
    }
}
