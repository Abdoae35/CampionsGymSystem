using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CampionsSystem.DAL.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Module = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    BodyText = table.Column<string>(type: "text", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractTemplates", x => x.Id);
                    table.CheckConstraint("CK_ContractTemplates_Version", "\"Version\" > 0");
                    table.ForeignKey(
                        name: "FK_ContractTemplates_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FullName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    EmploymentType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    JobTitle = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    HireDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TerminationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EmploymentStatus = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                    table.CheckConstraint("CK_Employees_EmploymentStatus", "\"EmploymentStatus\" IN ('Active', 'Resigned', 'Suspended')");
                    table.CheckConstraint("CK_Employees_EmploymentType", "\"EmploymentType\" IN ('Staff', 'Freelance')");
                    table.CheckConstraint("CK_Employees_HireDateTerminationDate", "\"TerminationDate\" >= \"HireDate\"");
                    table.ForeignKey(
                        name: "FK_Employees_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GymBranches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Address = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Phone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    MaxBorrowedGymSessions = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    QuickRefundWindowHours = table.Column<int>(type: "integer", nullable: false, defaultValue: 24),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GymBranches", x => x.Id);
                    table.CheckConstraint("CK_GymBranches_MaxBorrowedGymSessions", "\"MaxBorrowedGymSessions\" >= 0");
                    table.CheckConstraint("CK_GymBranches_QuickRefundWindowHours", "\"QuickRefundWindowHours\" >= 0");
                    table.ForeignKey(
                        name: "FK_GymBranches_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MembershipPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AccessMode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    SessionCount = table.Column<int>(type: "integer", nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AllowFreeze = table.Column<bool>(type: "boolean", nullable: false),
                    IncludedFreezeDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipPlans", x => x.Id);
                    table.CheckConstraint("CK_MembershipPlans_AccessMode", "\"AccessMode\" IN ('Unlimited', 'SessionBased')");
                    table.CheckConstraint("CK_MembershipPlans_AccessSessions", "(\"AccessMode\" = 'Unlimited' AND \"SessionCount\" IS NULL) OR (\"AccessMode\" = 'SessionBased' AND \"SessionCount\" IS NOT NULL AND \"SessionCount\" > 0)");
                    table.CheckConstraint("CK_MembershipPlans_DurationDays", "\"DurationDays\" > 0");
                    table.CheckConstraint("CK_MembershipPlans_IncludedFreezeDays", "\"IncludedFreezeDays\" >= 0");
                    table.CheckConstraint("CK_MembershipPlans_Price", "\"Price\" >= 0");
                    table.CheckConstraint("CK_MembershipPlans_SessionCount", "\"SessionCount\" > 0");
                    table.ForeignKey(
                        name: "FK_MembershipPlans_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeSalarySettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSalarySettings", x => x.Id);
                    table.CheckConstraint("CK_EmployeeSalarySettings_BasicSalary", "\"BasicSalary\" >= 0");
                    table.CheckConstraint("CK_EmployeeSalarySettings_EffectiveFromEffectiveTo", "\"EffectiveTo\" >= \"EffectiveFrom\"");
                    table.ForeignKey(
                        name: "FK_EmployeeSalarySettings_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeSalarySettings_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    Action = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EntityType = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: true),
                    OldValues = table.Column<string>(type: "jsonb", nullable: true),
                    NewValues = table.Column<string>(type: "jsonb", nullable: true),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    DeviceIdentifier = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuditLogs_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuthorizedDevices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    DeviceKeyHash = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    RegisteredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    RegisteredByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevokedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorizedDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorizedDevices_AspNetUsers_RegisteredByUserId",
                        column: x => x.RegisteredByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AuthorizedDevices_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BranchClosures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ExtensionDays = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    AppliesToGym = table.Column<bool>(type: "boolean", nullable: false),
                    AppliesToPrivate = table.Column<bool>(type: "boolean", nullable: false),
                    AppliesToActivities = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchClosures", x => x.Id);
                    table.CheckConstraint("CK_BranchClosures_ExtensionDays", "\"ExtensionDays\" >= 0");
                    table.CheckConstraint("CK_BranchClosures_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
                    table.ForeignKey(
                        name: "FK_BranchClosures_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BranchClosures_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BranchNetworkRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    RuleValue = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchNetworkRules", x => x.Id);
                    table.CheckConstraint("CK_BranchNetworkRules_RuleType", "\"RuleType\" IN ('PublicIP', 'CIDR', 'VPN')");
                    table.ForeignKey(
                        name: "FK_BranchNetworkRules_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommissionPeriods",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    StartAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReopenedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReopenedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionPeriods", x => x.Id);
                    table.CheckConstraint("CK_CommissionPeriods_StartAtEndAt", "\"EndAt\" >= \"StartAt\"");
                    table.CheckConstraint("CK_CommissionPeriods_Status", "\"Status\" IN ('Open', 'Closed')");
                    table.ForeignKey(
                        name: "FK_CommissionPeriods_AspNetUsers_ClosedByUserId",
                        column: x => x.ClosedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommissionPeriods_AspNetUsers_ReopenedByUserId",
                        column: x => x.ReopenedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommissionPeriods_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeBranchAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeBranchAssignments", x => x.Id);
                    table.CheckConstraint("CK_EmployeeBranchAssignments_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
                    table.ForeignKey(
                        name: "FK_EmployeeBranchAssignments_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeBranchAssignments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeBranchAssignments_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AccountType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialAccounts", x => x.Id);
                    table.CheckConstraint("CK_FinancialAccounts_AccountType", "\"AccountType\" IN ('Cash', 'InstaPay', 'Bank', 'Card', 'Wallet')");
                    table.ForeignKey(
                        name: "FK_FinancialAccounts_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FreelanceAgreements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreelanceAgreements", x => x.Id);
                    table.CheckConstraint("CK_FreelanceAgreements_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
                    table.ForeignKey(
                        name: "FK_FreelanceAgreements_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FreelanceAgreements_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FreelanceAgreements_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "GroupClasses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    CoachEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GroupClasses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GroupClasses_Employees_CoachEmployeeId",
                        column: x => x.CoachEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_GroupClasses_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberCode = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    FullName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PrimaryPhone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    SecondaryPhone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true),
                    IsPrimaryPhoneVerified = table.Column<bool>(type: "boolean", nullable: false),
                    PrimaryPhoneVerifiedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    HomeBranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    DateOfBirth = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    Notes = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Members_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Members_GymBranches_HomeBranchId",
                        column: x => x.HomeBranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Offers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: true),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    OfferType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Value = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsPermanent = table.Column<bool>(type: "boolean", nullable: false),
                    EligibilityDescription = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offers", x => x.Id);
                    table.CheckConstraint("CK_Offers_OfferType", "\"OfferType\" IN ('PercentageDiscount', 'FixedDiscount', 'ExtraDays')");
                    table.CheckConstraint("CK_Offers_Percentage", "\"OfferType\" <> 'PercentageDiscount' OR \"Value\" <= 100");
                    table.CheckConstraint("CK_Offers_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
                    table.CheckConstraint("CK_Offers_Value", "\"Value\" >= 0");
                    table.ForeignKey(
                        name: "FK_Offers_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Offers_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrivatePackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    SessionCount = table.Column<int>(type: "integer", nullable: false),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivatePackages", x => x.Id);
                    table.CheckConstraint("CK_PrivatePackages_DurationDays", "\"DurationDays\" > 0");
                    table.CheckConstraint("CK_PrivatePackages_Price", "\"Price\" >= 0");
                    table.CheckConstraint("CK_PrivatePackages_SessionCount", "\"SessionCount\" > 0");
                    table.ForeignKey(
                        name: "FK_PrivatePackages_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    OpenedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    OpeningCash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ExpectedClosingCash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ActualClosingCash = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Difference = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    OpenedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClosedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                    table.CheckConstraint("CK_Shifts_ActualClosingCash", "\"ActualClosingCash\" >= 0");
                    table.CheckConstraint("CK_Shifts_ExpectedClosingCash", "\"ExpectedClosingCash\" >= 0");
                    table.CheckConstraint("CK_Shifts_OpenedAtClosedAt", "\"ClosedAt\" >= \"OpenedAt\"");
                    table.CheckConstraint("CK_Shifts_OpeningCash", "\"OpeningCash\" >= 0");
                    table.CheckConstraint("CK_Shifts_Status", "\"Status\" IN ('Open', 'Closed')");
                    table.ForeignKey(
                        name: "FK_Shifts_AspNetUsers_ClosedByUserId",
                        column: x => x.ClosedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shifts_AspNetUsers_OpenedByUserId",
                        column: x => x.OpenedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Shifts_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SportsActivities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SportsActivities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SportsActivities_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payrolls",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommissionPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    BasicSalary = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CommissionAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DeductionAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BonusAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    NetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PaidByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaidFromFinancialAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payrolls", x => x.Id);
                    table.CheckConstraint("CK_Payrolls_BasicSalary", "\"BasicSalary\" >= 0");
                    table.CheckConstraint("CK_Payrolls_BonusAmount", "\"BonusAmount\" >= 0");
                    table.CheckConstraint("CK_Payrolls_CommissionAmount", "\"CommissionAmount\" >= 0");
                    table.CheckConstraint("CK_Payrolls_DeductionAmount", "\"DeductionAmount\" >= 0");
                    table.CheckConstraint("CK_Payrolls_NetAmount", "\"NetAmount\" >= 0");
                    table.CheckConstraint("CK_Payrolls_Status", "\"Status\" IN ('Pending', 'Paid')");
                    table.ForeignKey(
                        name: "FK_Payrolls_AspNetUsers_PaidByUserId",
                        column: x => x.PaidByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payrolls_CommissionPeriods_CommissionPeriodId",
                        column: x => x.CommissionPeriodId,
                        principalTable: "CommissionPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payrolls_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payrolls_FinancialAccounts_PaidFromFinancialAccountId",
                        column: x => x.PaidFromFinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payrolls_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClassSchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GroupClassId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSchedules", x => x.Id);
                    table.CheckConstraint("CK_ClassSchedules_DayOfWeek", "\"DayOfWeek\" BETWEEN 0 AND 6");
                    table.CheckConstraint("CK_ClassSchedules_TimeRange", "\"EndTime\" > \"StartTime\"");
                    table.ForeignKey(
                        name: "FK_ClassSchedules_GroupClasses_GroupClassId",
                        column: x => x.GroupClassId,
                        principalTable: "GroupClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Leads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    FullName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Phone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: true),
                    Source = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ConvertedMemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leads", x => x.Id);
                    table.CheckConstraint("CK_Leads_Status", "\"Status\" IN ('New', 'Contacted', 'Interested', 'FollowUp', 'Converted', 'Lost', 'Inactive')");
                    table.ForeignKey(
                        name: "FK_Leads_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leads_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Leads_Members_ConvertedMemberId",
                        column: x => x.ConvertedMemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MemberSalesAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    AssignmentType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberSalesAssignments", x => x.Id);
                    table.CheckConstraint("CK_MemberSalesAssignments_AssignedAtEndedAt", "\"EndedAt\" >= \"AssignedAt\"");
                    table.CheckConstraint("CK_MemberSalesAssignments_AssignmentEmployee", "(\"AssignmentType\" = 'House' AND \"SalesEmployeeId\" IS NULL) OR (\"AssignmentType\" = 'Sales' AND \"SalesEmployeeId\" IS NOT NULL)");
                    table.CheckConstraint("CK_MemberSalesAssignments_AssignmentType", "\"AssignmentType\" IN ('Sales', 'House')");
                    table.ForeignKey(
                        name: "FK_MemberSalesAssignments_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberSalesAssignments_Employees_SalesEmployeeId",
                        column: x => x.SalesEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberSalesAssignments_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MemberSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    CityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PurchasedAtBranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AccessMode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    TotalSessions = table.Column<int>(type: "integer", nullable: true),
                    RemainingSessions = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberSubscriptions", x => x.Id);
                    table.CheckConstraint("CK_MemberSubscriptions_AccessMode", "\"AccessMode\" IN ('Unlimited', 'SessionBased')");
                    table.CheckConstraint("CK_MemberSubscriptions_AccessSessions", "(\"AccessMode\" = 'Unlimited' AND \"TotalSessions\" IS NULL AND \"RemainingSessions\" IS NULL) OR (\"AccessMode\" = 'SessionBased' AND \"TotalSessions\" IS NOT NULL AND \"RemainingSessions\" IS NOT NULL)");
                    table.CheckConstraint("CK_MemberSubscriptions_RemainingSessions", "\"RemainingSessions\" >= 0");
                    table.CheckConstraint("CK_MemberSubscriptions_SessionBounds", "\"RemainingSessions\" <= \"TotalSessions\" AND ((\"TotalSessions\" IS NULL) = (\"RemainingSessions\" IS NULL))");
                    table.CheckConstraint("CK_MemberSubscriptions_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
                    table.CheckConstraint("CK_MemberSubscriptions_Status", "\"Status\" IN ('Scheduled', 'Active', 'Frozen', 'Expired', 'Cancelled', 'SuspendedForInstallment')");
                    table.CheckConstraint("CK_MemberSubscriptions_TotalSessions", "\"TotalSessions\" >= 0");
                    table.ForeignKey(
                        name: "FK_MemberSubscriptions_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberSubscriptions_GymBranches_PurchasedAtBranchId",
                        column: x => x.PurchasedAtBranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberSubscriptions_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberSubscriptions_MembershipPlans_MembershipPlanId",
                        column: x => x.MembershipPlanId,
                        principalTable: "MembershipPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MessageLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: true),
                    MessageType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    TemplateKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    DestinationPhone = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    ProviderMessageId = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FailureReason = table.Column<string>(type: "text", nullable: true),
                    SentAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageLogs", x => x.Id);
                    table.CheckConstraint("CK_MessageLogs_MessageType", "\"MessageType\" IN ('WhatsApp', 'SMS', 'OTP')");
                    table.CheckConstraint("CK_MessageLogs_Status", "\"Status\" IN ('Pending', 'Sent', 'Failed')");
                    table.ForeignKey(
                        name: "FK_MessageLogs_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrivateSubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalSessions = table.Column<int>(type: "integer", nullable: false),
                    RemainingSessions = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateSubscriptions", x => x.Id);
                    table.CheckConstraint("CK_PrivateSubscriptions_RemainingSessions", "\"RemainingSessions\" >= 0");
                    table.CheckConstraint("CK_PrivateSubscriptions_SessionBounds", "\"RemainingSessions\" <= \"TotalSessions\" AND ((\"TotalSessions\" IS NULL) = (\"RemainingSessions\" IS NULL))");
                    table.CheckConstraint("CK_PrivateSubscriptions_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
                    table.CheckConstraint("CK_PrivateSubscriptions_Status", "\"Status\" IN ('Scheduled', 'Active', 'Expired', 'Cancelled')");
                    table.CheckConstraint("CK_PrivateSubscriptions_TotalSessions", "\"TotalSessions\" >= 0");
                    table.ForeignKey(
                        name: "FK_PrivateSubscriptions_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivateSubscriptions_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MemberOfferEntitlements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    OfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    GrantedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberOfferEntitlements", x => x.Id);
                    table.CheckConstraint("CK_MemberOfferEntitlements_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
                    table.ForeignKey(
                        name: "FK_MemberOfferEntitlements_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberOfferEntitlements_Employees_GrantedByEmployeeId",
                        column: x => x.GrantedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberOfferEntitlements_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberOfferEntitlements_Offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfferMembershipPlans",
                columns: table => new
                {
                    OfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipPlanId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferMembershipPlans", x => new { x.OfferId, x.MembershipPlanId });
                    table.ForeignKey(
                        name: "FK_OfferMembershipPlans_MembershipPlans_MembershipPlanId",
                        column: x => x.MembershipPlanId,
                        principalTable: "MembershipPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfferMembershipPlans_Offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Expenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpenseCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    ExpenseDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expenses", x => x.Id);
                    table.CheckConstraint("CK_Expenses_Amount", "\"Amount\" >= 0");
                    table.CheckConstraint("CK_Expenses_Status", "\"Status\" IN ('Recorded', 'Cancelled')");
                    table.ForeignKey(
                        name: "FK_Expenses_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_ExpenseCategories_ExpenseCategoryId",
                        column: x => x.ExpenseCategoryId,
                        principalTable: "ExpenseCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_FinancialAccounts_FinancialAccountId",
                        column: x => x.FinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Expenses_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SaleTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedShiftId = table.Column<Guid>(type: "uuid", nullable: true),
                    SaleType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SaleCategory = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    SalesEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    OfferId = table.Column<Guid>(type: "uuid", nullable: true),
                    GrossAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SaleTransactions", x => x.Id);
                    table.CheckConstraint("CK_SaleTransactions_Amounts", "\"DiscountAmount\" <= \"GrossAmount\" AND \"NetAmount\" = \"GrossAmount\" - \"DiscountAmount\"");
                    table.CheckConstraint("CK_SaleTransactions_DiscountAmount", "\"DiscountAmount\" >= 0");
                    table.CheckConstraint("CK_SaleTransactions_GrossAmount", "\"GrossAmount\" >= 0");
                    table.CheckConstraint("CK_SaleTransactions_NetAmount", "\"NetAmount\" >= 0");
                    table.CheckConstraint("CK_SaleTransactions_SaleCategory", "\"SaleCategory\" IN ('NewMembership', 'Renewal')");
                    table.CheckConstraint("CK_SaleTransactions_SaleType", "\"SaleType\" IN ('GymMembership', 'Private', 'Activity', 'Other')");
                    table.CheckConstraint("CK_SaleTransactions_Status", "\"Status\" IN ('Open', 'Completed', 'Cancelled', 'PartiallyRefunded', 'Refunded')");
                    table.ForeignKey(
                        name: "FK_SaleTransactions_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleTransactions_Employees_SalesEmployeeId",
                        column: x => x.SalesEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleTransactions_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleTransactions_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleTransactions_Offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SaleTransactions_Shifts_CreatedShiftId",
                        column: x => x.CreatedShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ShiftEmployees",
                columns: table => new
                {
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LeftAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShiftEmployees", x => new { x.ShiftId, x.EmployeeId });
                    table.CheckConstraint("CK_ShiftEmployees_JoinedAtLeftAt", "\"LeftAt\" >= \"JoinedAt\"");
                    table.ForeignKey(
                        name: "FK_ShiftEmployees_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ShiftEmployees_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivityCoachAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SportsActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityCoachAssignments", x => x.Id);
                    table.CheckConstraint("CK_ActivityCoachAssignments_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
                    table.ForeignKey(
                        name: "FK_ActivityCoachAssignments_Employees_CoachEmployeeId",
                        column: x => x.CoachEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivityCoachAssignments_SportsActivities_SportsActivityId",
                        column: x => x.SportsActivityId,
                        principalTable: "SportsActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivityPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SportsActivityId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    AccessMode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SessionCount = table.Column<int>(type: "integer", nullable: true),
                    DurationDays = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AllowFreeze = table.Column<bool>(type: "boolean", nullable: false),
                    IncludedFreezeDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivityPackages", x => x.Id);
                    table.CheckConstraint("CK_ActivityPackages_AccessMode", "\"AccessMode\" IN ('Unlimited', 'SessionBased')");
                    table.CheckConstraint("CK_ActivityPackages_AccessSessions", "(\"AccessMode\" = 'Unlimited' AND \"SessionCount\" IS NULL) OR (\"AccessMode\" = 'SessionBased' AND \"SessionCount\" IS NOT NULL AND \"SessionCount\" > 0)");
                    table.CheckConstraint("CK_ActivityPackages_DurationDays", "\"DurationDays\" > 0");
                    table.CheckConstraint("CK_ActivityPackages_IncludedFreezeDays", "\"IncludedFreezeDays\" >= 0");
                    table.CheckConstraint("CK_ActivityPackages_Price", "\"Price\" >= 0");
                    table.CheckConstraint("CK_ActivityPackages_SessionCount", "\"SessionCount\" > 0");
                    table.ForeignKey(
                        name: "FK_ActivityPackages_SportsActivities_SportsActivityId",
                        column: x => x.SportsActivityId,
                        principalTable: "SportsActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CommissionRules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CommissionType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SportsActivityId = table.Column<Guid>(type: "uuid", nullable: true),
                    GroupClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    CalculationMethod = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    TargetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    BelowTargetRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    ReachedTargetRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    FlatRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    FixedAmountPerAttendance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: false),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionRules", x => x.Id);
                    table.CheckConstraint("CK_CommissionRules_BelowTargetRate", "\"BelowTargetRate\" >= 0 AND \"BelowTargetRate\" <= 100");
                    table.CheckConstraint("CK_CommissionRules_CalculationMethod", "\"CalculationMethod\" IN ('TargetPercentage', 'FlatPercentage', 'FixedPerAttendance')");
                    table.CheckConstraint("CK_CommissionRules_CommissionType", "\"CommissionType\" IN ('SalesNew', 'SalesRenewal', 'Private', 'Activity', 'Class', 'Freelance')");
                    table.CheckConstraint("CK_CommissionRules_EffectiveFromEffectiveTo", "\"EffectiveTo\" >= \"EffectiveFrom\"");
                    table.CheckConstraint("CK_CommissionRules_FixedAmountPerAttendance", "\"FixedAmountPerAttendance\" >= 0");
                    table.CheckConstraint("CK_CommissionRules_FlatRate", "\"FlatRate\" >= 0 AND \"FlatRate\" <= 100");
                    table.CheckConstraint("CK_CommissionRules_ReachedTargetRate", "\"ReachedTargetRate\" >= 0 AND \"ReachedTargetRate\" <= 100");
                    table.CheckConstraint("CK_CommissionRules_TargetAmount", "\"TargetAmount\" >= 0");
                    table.ForeignKey(
                        name: "FK_CommissionRules_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommissionRules_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommissionRules_GroupClasses_GroupClassId",
                        column: x => x.GroupClassId,
                        principalTable: "GroupClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommissionRules_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CommissionRules_SportsActivities_SportsActivityId",
                        column: x => x.SportsActivityId,
                        principalTable: "SportsActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeCommissionSummaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CommissionPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommissionType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SportsActivityId = table.Column<Guid>(type: "uuid", nullable: true),
                    GroupClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    TotalEligibleAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    EligibleQuantity = table.Column<int>(type: "integer", nullable: true),
                    TargetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    TargetReached = table.Column<bool>(type: "boolean", nullable: false),
                    AppliedRate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    CommissionAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FinalizedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeCommissionSummaries", x => x.Id);
                    table.CheckConstraint("CK_EmployeeCommissionSummaries_AppliedRate", "\"AppliedRate\" >= 0 AND \"AppliedRate\" <= 100");
                    table.CheckConstraint("CK_EmployeeCommissionSummaries_CommissionAmount", "\"CommissionAmount\" >= 0");
                    table.CheckConstraint("CK_EmployeeCommissionSummaries_CommissionType", "\"CommissionType\" IN ('SalesNew', 'SalesRenewal', 'Private', 'Activity', 'Class', 'Freelance')");
                    table.CheckConstraint("CK_EmployeeCommissionSummaries_EligibleQuantity", "\"EligibleQuantity\" >= 0");
                    table.CheckConstraint("CK_EmployeeCommissionSummaries_TargetAmount", "\"TargetAmount\" >= 0");
                    table.CheckConstraint("CK_EmployeeCommissionSummaries_TotalEligibleAmount", "\"TotalEligibleAmount\" >= 0");
                    table.ForeignKey(
                        name: "FK_EmployeeCommissionSummaries_CommissionPeriods_CommissionPeri~",
                        column: x => x.CommissionPeriodId,
                        principalTable: "CommissionPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeCommissionSummaries_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeCommissionSummaries_GroupClasses_GroupClassId",
                        column: x => x.GroupClassId,
                        principalTable: "GroupClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeCommissionSummaries_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeCommissionSummaries_SportsActivities_SportsActivityId",
                        column: x => x.SportsActivityId,
                        principalTable: "SportsActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayrollDeductions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PayrollId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayrollDeductions", x => x.Id);
                    table.CheckConstraint("CK_PayrollDeductions_Amount", "\"Amount\" >= 0");
                    table.ForeignKey(
                        name: "FK_PayrollDeductions_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayrollDeductions_Payrolls_PayrollId",
                        column: x => x.PayrollId,
                        principalTable: "Payrolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeadAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: false),
                    SalesEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    AssignedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadAssignments", x => x.Id);
                    table.CheckConstraint("CK_LeadAssignments_AssignedAtEndedAt", "\"EndedAt\" >= \"AssignedAt\"");
                    table.ForeignKey(
                        name: "FK_LeadAssignments_AspNetUsers_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeadAssignments_Employees_SalesEmployeeId",
                        column: x => x.SalesEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeadAssignments_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LeadInteractions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LeadId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    InteractionType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    Outcome = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NextFollowUpAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeadInteractions", x => x.Id);
                    table.CheckConstraint("CK_LeadInteractions_InteractionType", "\"InteractionType\" IN ('Call', 'WhatsApp', 'Visit', 'Other')");
                    table.ForeignKey(
                        name: "FK_LeadInteractions_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeadInteractions_Leads_LeadId",
                        column: x => x.LeadId,
                        principalTable: "Leads",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionFreezes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberSubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    FreezeType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DaysApplied = table.Column<int>(type: "integer", nullable: true),
                    Reason = table.Column<string>(type: "text", nullable: true),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionFreezes", x => x.Id);
                    table.CheckConstraint("CK_SubscriptionFreezes_DaysApplied", "\"DaysApplied\" >= 0");
                    table.CheckConstraint("CK_SubscriptionFreezes_FreezeType", "\"FreezeType\" IN ('Normal', 'Medical')");
                    table.CheckConstraint("CK_SubscriptionFreezes_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
                    table.ForeignKey(
                        name: "FK_SubscriptionFreezes_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionFreezes_MemberSubscriptions_MemberSubscriptionId",
                        column: x => x.MemberSubscriptionId,
                        principalTable: "MemberSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrivateCoachAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrivateSubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssignedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    EndedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    SessionsAllocated = table.Column<int>(type: "integer", nullable: false),
                    SessionsConsumed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CommissionBasisAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TransferReason = table.Column<string>(type: "text", nullable: true),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateCoachAssignments", x => x.Id);
                    table.CheckConstraint("CK_PrivateCoachAssignments_AssignedAtEndedAt", "\"EndedAt\" >= \"AssignedAt\"");
                    table.CheckConstraint("CK_PrivateCoachAssignments_CommissionBasisAmount", "\"CommissionBasisAmount\" >= 0");
                    table.CheckConstraint("CK_PrivateCoachAssignments_SessionBounds", "\"SessionsConsumed\" <= \"SessionsAllocated\"");
                    table.CheckConstraint("CK_PrivateCoachAssignments_SessionsAllocated", "\"SessionsAllocated\" >= 0");
                    table.CheckConstraint("CK_PrivateCoachAssignments_SessionsConsumed", "\"SessionsConsumed\" >= 0");
                    table.ForeignKey(
                        name: "FK_PrivateCoachAssignments_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivateCoachAssignments_Employees_CoachEmployeeId",
                        column: x => x.CoachEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivateCoachAssignments_PrivateSubscriptions_PrivateSubscri~",
                        column: x => x.PrivateSubscriptionId,
                        principalTable: "PrivateSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "InstallmentAgreements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SaleTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NumberOfInstallments = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InstallmentAgreements", x => x.Id);
                    table.CheckConstraint("CK_InstallmentAgreements_NumberOfInstallments", "\"NumberOfInstallments\" > 0");
                    table.CheckConstraint("CK_InstallmentAgreements_Status", "\"Status\" IN ('Active', 'Completed', 'Defaulted', 'Cancelled')");
                    table.CheckConstraint("CK_InstallmentAgreements_TotalAmount", "\"TotalAmount\" >= 0");
                    table.ForeignKey(
                        name: "FK_InstallmentAgreements_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_InstallmentAgreements_SaleTransactions_SaleTransactionId",
                        column: x => x.SaleTransactionId,
                        principalTable: "SaleTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MemberContracts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    SaleTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContractTemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateVersion = table.Column<int>(type: "integer", nullable: false),
                    TemplateSnapshotText = table.Column<string>(type: "text", nullable: false),
                    GeneratedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    GeneratedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    PhysicalArchiveReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberContracts", x => x.Id);
                    table.CheckConstraint("CK_MemberContracts_TemplateVersion", "\"TemplateVersion\" > 0");
                    table.ForeignKey(
                        name: "FK_MemberContracts_AspNetUsers_GeneratedByUserId",
                        column: x => x.GeneratedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberContracts_ContractTemplates_ContractTemplateId",
                        column: x => x.ContractTemplateId,
                        principalTable: "ContractTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberContracts_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberContracts_SaleTransactions_SaleTransactionId",
                        column: x => x.SaleTransactionId,
                        principalTable: "SaleTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MembershipSubscriptionTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberSubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SaleTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipPlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PurchasedDurationDays = table.Column<int>(type: "integer", nullable: false),
                    PurchasedSessions = table.Column<int>(type: "integer", nullable: true),
                    PlanPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    NetPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PreviousEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NewEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipSubscriptionTransactions", x => x.Id);
                    table.CheckConstraint("CK_MembershipSubscriptionTransactions_NetPriceSnapshot", "\"NetPriceSnapshot\" >= 0");
                    table.CheckConstraint("CK_MembershipSubscriptionTransactions_PlanPriceSnapshot", "\"PlanPriceSnapshot\" >= 0");
                    table.CheckConstraint("CK_MembershipSubscriptionTransactions_PurchasedDurationDays", "\"PurchasedDurationDays\" > 0");
                    table.CheckConstraint("CK_MembershipSubscriptionTransactions_PurchasedSessions", "\"PurchasedSessions\" > 0");
                    table.CheckConstraint("CK_MembershipSubscriptionTransactions_TransactionType", "\"TransactionType\" IN ('Initial', 'Renewal')");
                    table.ForeignKey(
                        name: "FK_MembershipSubscriptionTransactions_MemberSubscriptions_Memb~",
                        column: x => x.MemberSubscriptionId,
                        principalTable: "MemberSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MembershipSubscriptionTransactions_MembershipPlans_Membersh~",
                        column: x => x.MembershipPlanId,
                        principalTable: "MembershipPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MembershipSubscriptionTransactions_SaleTransactions_SaleTra~",
                        column: x => x.SaleTransactionId,
                        principalTable: "SaleTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrivateSubscriptionTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrivateSubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SaleTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrivatePackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SessionsAdded = table.Column<int>(type: "integer", nullable: false),
                    DurationDaysAdded = table.Column<int>(type: "integer", nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitSessionValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PreviousEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NewEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateSubscriptionTransactions", x => x.Id);
                    table.CheckConstraint("CK_PrivateSubscriptionTransactions_DurationDaysAdded", "\"DurationDaysAdded\" > 0");
                    table.CheckConstraint("CK_PrivateSubscriptionTransactions_NetAmount", "\"NetAmount\" >= 0");
                    table.CheckConstraint("CK_PrivateSubscriptionTransactions_SessionsAdded", "\"SessionsAdded\" > 0");
                    table.CheckConstraint("CK_PrivateSubscriptionTransactions_TransactionType", "\"TransactionType\" IN ('Initial', 'Renewal')");
                    table.CheckConstraint("CK_PrivateSubscriptionTransactions_UnitSessionValue", "\"UnitSessionValue\" >= 0");
                    table.ForeignKey(
                        name: "FK_PrivateSubscriptionTransactions_PrivatePackages_PrivatePack~",
                        column: x => x.PrivatePackageId,
                        principalTable: "PrivatePackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivateSubscriptionTransactions_PrivateSubscriptions_Privat~",
                        column: x => x.PrivateSubscriptionId,
                        principalTable: "PrivateSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivateSubscriptionTransactions_SaleTransactions_SaleTransa~",
                        column: x => x.SaleTransactionId,
                        principalTable: "SaleTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivitySchedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityCoachAssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DayOfWeek = table.Column<int>(type: "integer", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivitySchedules", x => x.Id);
                    table.CheckConstraint("CK_ActivitySchedules_DayOfWeek", "\"DayOfWeek\" BETWEEN 0 AND 6");
                    table.CheckConstraint("CK_ActivitySchedules_TimeRange", "\"EndTime\" > \"StartTime\"");
                    table.ForeignKey(
                        name: "FK_ActivitySchedules_ActivityCoachAssignments_ActivityCoachAss~",
                        column: x => x.ActivityCoachAssignmentId,
                        principalTable: "ActivityCoachAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivitySubscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccessMode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityCoachAssignmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    TotalSessions = table.Column<int>(type: "integer", nullable: true),
                    RemainingSessions = table.Column<int>(type: "integer", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivitySubscriptions", x => x.Id);
                    table.CheckConstraint("CK_ActivitySubscriptions_AccessMode", "\"AccessMode\" IN ('Unlimited', 'SessionBased')");
                    table.CheckConstraint("CK_ActivitySubscriptions_AccessSessions", "(\"AccessMode\" = 'Unlimited' AND \"TotalSessions\" IS NULL AND \"RemainingSessions\" IS NULL) OR (\"AccessMode\" = 'SessionBased' AND \"TotalSessions\" IS NOT NULL AND \"RemainingSessions\" IS NOT NULL)");
                    table.CheckConstraint("CK_ActivitySubscriptions_RemainingSessions", "\"RemainingSessions\" >= 0");
                    table.CheckConstraint("CK_ActivitySubscriptions_SessionBounds", "\"RemainingSessions\" <= \"TotalSessions\" AND ((\"TotalSessions\" IS NULL) = (\"RemainingSessions\" IS NULL))");
                    table.CheckConstraint("CK_ActivitySubscriptions_StartDateEndDate", "\"EndDate\" >= \"StartDate\"");
                    table.CheckConstraint("CK_ActivitySubscriptions_Status", "\"Status\" IN ('Scheduled', 'Active', 'Expired', 'Cancelled', 'SuspendedForInstallment')");
                    table.CheckConstraint("CK_ActivitySubscriptions_TotalSessions", "\"TotalSessions\" >= 0");
                    table.ForeignKey(
                        name: "FK_ActivitySubscriptions_ActivityCoachAssignments_ActivityCoac~",
                        column: x => x.ActivityCoachAssignmentId,
                        principalTable: "ActivityCoachAssignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivitySubscriptions_ActivityPackages_ActivityPackageId",
                        column: x => x.ActivityPackageId,
                        principalTable: "ActivityPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivitySubscriptions_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivitySubscriptions_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OfferActivityPackages",
                columns: table => new
                {
                    OfferId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityPackageId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OfferActivityPackages", x => new { x.OfferId, x.ActivityPackageId });
                    table.ForeignKey(
                        name: "FK_OfferActivityPackages_ActivityPackages_ActivityPackageId",
                        column: x => x.ActivityPackageId,
                        principalTable: "ActivityPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OfferActivityPackages_Offers_OfferId",
                        column: x => x.OfferId,
                        principalTable: "Offers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Installments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstallmentAgreementId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstallmentNumber = table.Column<int>(type: "integer", nullable: false),
                    DueDate = table.Column<DateOnly>(type: "date", nullable: false),
                    AmountDue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountPaid = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Installments", x => x.Id);
                    table.CheckConstraint("CK_Installments_AmountDue", "\"AmountDue\" >= 0");
                    table.CheckConstraint("CK_Installments_AmountPaid", "\"AmountPaid\" >= 0");
                    table.CheckConstraint("CK_Installments_InstallmentNumber", "\"InstallmentNumber\" > 0");
                    table.CheckConstraint("CK_Installments_Status", "\"Status\" IN ('Pending', 'PartiallyPaid', 'Paid', 'Overdue')");
                    table.ForeignKey(
                        name: "FK_Installments_InstallmentAgreements_InstallmentAgreementId",
                        column: x => x.InstallmentAgreementId,
                        principalTable: "InstallmentAgreements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ActivitySubscriptionTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivitySubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    SaleTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActivityPackageId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SessionsAdded = table.Column<int>(type: "integer", nullable: true),
                    DurationDaysAdded = table.Column<int>(type: "integer", nullable: false),
                    NetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitSessionValue = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PreviousEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NewEndDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActivitySubscriptionTransactions", x => x.Id);
                    table.CheckConstraint("CK_ActivitySubscriptionTransactions_DurationDaysAdded", "\"DurationDaysAdded\" > 0");
                    table.CheckConstraint("CK_ActivitySubscriptionTransactions_NetAmount", "\"NetAmount\" >= 0");
                    table.CheckConstraint("CK_ActivitySubscriptionTransactions_SessionsAdded", "\"SessionsAdded\" > 0");
                    table.CheckConstraint("CK_ActivitySubscriptionTransactions_TransactionType", "\"TransactionType\" IN ('Initial', 'Renewal')");
                    table.CheckConstraint("CK_ActivitySubscriptionTransactions_UnitSessionValue", "\"UnitSessionValue\" >= 0");
                    table.ForeignKey(
                        name: "FK_ActivitySubscriptionTransactions_ActivityPackages_ActivityP~",
                        column: x => x.ActivityPackageId,
                        principalTable: "ActivityPackages",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivitySubscriptionTransactions_ActivitySubscriptions_Acti~",
                        column: x => x.ActivitySubscriptionId,
                        principalTable: "ActivitySubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ActivitySubscriptionTransactions_SaleTransactions_SaleTrans~",
                        column: x => x.SaleTransactionId,
                        principalTable: "SaleTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CheckIns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckInType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    MemberSubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ActivitySubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    GroupClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    CoachEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    CheckInMethod = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SessionConsumed = table.Column<bool>(type: "boolean", nullable: false),
                    CheckedInAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckIns", x => x.Id);
                    table.CheckConstraint("CK_CheckIns_CheckInMethod", "\"CheckInMethod\" IN ('Scanner', 'Manual', 'Coach')");
                    table.CheckConstraint("CK_CheckIns_CheckInType", "\"CheckInType\" IN ('Gym', 'BorrowedGym', 'Activity', 'Class')");
                    table.CheckConstraint("CK_CheckIns_SubscriptionType", "(\"CheckInType\" IN ('Gym', 'BorrowedGym') AND \"MemberSubscriptionId\" IS NOT NULL AND \"ActivitySubscriptionId\" IS NULL AND \"GroupClassId\" IS NULL) OR (\"CheckInType\" = 'Activity' AND \"ActivitySubscriptionId\" IS NOT NULL AND \"MemberSubscriptionId\" IS NULL AND \"GroupClassId\" IS NULL) OR (\"CheckInType\" = 'Class' AND \"GroupClassId\" IS NOT NULL AND \"MemberSubscriptionId\" IS NULL AND \"ActivitySubscriptionId\" IS NULL)");
                    table.ForeignKey(
                        name: "FK_CheckIns_ActivitySubscriptions_ActivitySubscriptionId",
                        column: x => x.ActivitySubscriptionId,
                        principalTable: "ActivitySubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CheckIns_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CheckIns_Employees_CoachEmployeeId",
                        column: x => x.CoachEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CheckIns_GroupClasses_GroupClassId",
                        column: x => x.GroupClassId,
                        principalTable: "GroupClasses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CheckIns_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CheckIns_MemberSubscriptions_MemberSubscriptionId",
                        column: x => x.MemberSubscriptionId,
                        principalTable: "MemberSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CheckIns_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SaleTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: false),
                    CommissionPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstallmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceivedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.CheckConstraint("CK_Payments_Amount", "\"Amount\" >= 0");
                    table.CheckConstraint("CK_Payments_Status", "\"Status\" IN ('Completed', 'Cancelled')");
                    table.ForeignKey(
                        name: "FK_Payments_AspNetUsers_ReceivedByUserId",
                        column: x => x.ReceivedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_CommissionPeriods_CommissionPeriodId",
                        column: x => x.CommissionPeriodId,
                        principalTable: "CommissionPeriods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Installments_InstallmentId",
                        column: x => x.InstallmentId,
                        principalTable: "Installments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_SaleTransactions_SaleTransactionId",
                        column: x => x.SaleTransactionId,
                        principalTable: "SaleTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BorrowedGymSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MemberId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExpiredSubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CheckInId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    BorrowedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SettledBySubscriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    SettledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BorrowedGymSessions", x => x.Id);
                    table.CheckConstraint("CK_BorrowedGymSessions_Status", "\"Status\" IN ('Outstanding', 'Settled')");
                    table.ForeignKey(
                        name: "FK_BorrowedGymSessions_CheckIns_CheckInId",
                        column: x => x.CheckInId,
                        principalTable: "CheckIns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowedGymSessions_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowedGymSessions_MemberSubscriptions_ExpiredSubscription~",
                        column: x => x.ExpiredSubscriptionId,
                        principalTable: "MemberSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowedGymSessions_MemberSubscriptions_SettledBySubscripti~",
                        column: x => x.SettledBySubscriptionId,
                        principalTable: "MemberSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BorrowedGymSessions_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PrivateSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrivateSubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CoachEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    GymCheckInId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrivateSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrivateSessions_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivateSessions_CheckIns_GymCheckInId",
                        column: x => x.GymCheckInId,
                        principalTable: "CheckIns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivateSessions_Employees_CoachEmployeeId",
                        column: x => x.CoachEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PrivateSessions_PrivateSubscriptions_PrivateSubscriptionId",
                        column: x => x.PrivateSubscriptionId,
                        principalTable: "PrivateSubscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentParts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ProviderReference = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentParts", x => x.Id);
                    table.CheckConstraint("CK_PaymentParts_Amount", "\"Amount\" >= 0");
                    table.ForeignKey(
                        name: "FK_PaymentParts_FinancialAccounts_FinancialAccountId",
                        column: x => x.FinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentParts_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Refunds",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SaleTransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalPaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    RefundType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    ProcessedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Refunds", x => x.Id);
                    table.CheckConstraint("CK_Refunds_Amount", "\"Amount\" >= 0");
                    table.CheckConstraint("CK_Refunds_RefundType", "\"RefundType\" IN ('QuickRefund', 'ManagerRefund')");
                    table.ForeignKey(
                        name: "FK_Refunds_AspNetUsers_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Refunds_AspNetUsers_ProcessedByUserId",
                        column: x => x.ProcessedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Refunds_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Refunds_Payments_OriginalPaymentId",
                        column: x => x.OriginalPaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Refunds_SaleTransactions_SaleTransactionId",
                        column: x => x.SaleTransactionId,
                        principalTable: "SaleTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Refunds_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialTransactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    BranchId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SourceType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    RefundId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpenseId = table.Column<Guid>(type: "uuid", nullable: true),
                    PayrollId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactions", x => x.Id);
                    table.CheckConstraint("CK_FinancialTransactions_Amount", "\"Amount\" >= 0");
                    table.CheckConstraint("CK_FinancialTransactions_Source", "(\"SourceType\" = 'Payment' AND \"PaymentId\" IS NOT NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NULL) OR (\"SourceType\" = 'Refund' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NOT NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NULL) OR (\"SourceType\" = 'Expense' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NOT NULL AND \"PayrollId\" IS NULL) OR (\"SourceType\" = 'Payroll' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NOT NULL) OR (\"SourceType\" = 'Adjustment' AND \"PaymentId\" IS NULL AND \"RefundId\" IS NULL AND \"ExpenseId\" IS NULL AND \"PayrollId\" IS NULL)");
                    table.CheckConstraint("CK_FinancialTransactions_SourceType", "\"SourceType\" IN ('Payment', 'Refund', 'Expense', 'Payroll', 'Adjustment')");
                    table.CheckConstraint("CK_FinancialTransactions_TransactionType", "\"TransactionType\" IN ('Credit', 'Debit')");
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_Expenses_ExpenseId",
                        column: x => x.ExpenseId,
                        principalTable: "Expenses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_FinancialAccounts_FinancialAccountId",
                        column: x => x.FinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_GymBranches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "GymBranches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_Payrolls_PayrollId",
                        column: x => x.PayrollId,
                        principalTable: "Payrolls",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_Refunds_RefundId",
                        column: x => x.RefundId,
                        principalTable: "Refunds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RefundParts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RefundId = table.Column<Guid>(type: "uuid", nullable: false),
                    FinancialAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundParts", x => x.Id);
                    table.CheckConstraint("CK_RefundParts_Amount", "\"Amount\" >= 0");
                    table.ForeignKey(
                        name: "FK_RefundParts_FinancialAccounts_FinancialAccountId",
                        column: x => x.FinancialAccountId,
                        principalTable: "FinancialAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RefundParts_Refunds_RefundId",
                        column: x => x.RefundId,
                        principalTable: "Refunds",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCoachAssignments_CoachEmployeeId",
                table: "ActivityCoachAssignments",
                column: "CoachEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityCoachAssignments_SportsActivityId",
                table: "ActivityCoachAssignments",
                column: "SportsActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivityPackages_SportsActivityId",
                table: "ActivityPackages",
                column: "SportsActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySchedules_ActivityCoachAssignmentId",
                table: "ActivitySchedules",
                column: "ActivityCoachAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySubscriptions_ActivityCoachAssignmentId",
                table: "ActivitySubscriptions",
                column: "ActivityCoachAssignmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySubscriptions_ActivityPackageId",
                table: "ActivitySubscriptions",
                column: "ActivityPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySubscriptions_BranchId",
                table: "ActivitySubscriptions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySubscriptions_MemberId",
                table: "ActivitySubscriptions",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySubscriptionTransactions_ActivityPackageId",
                table: "ActivitySubscriptionTransactions",
                column: "ActivityPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySubscriptionTransactions_ActivitySubscriptionId",
                table: "ActivitySubscriptionTransactions",
                column: "ActivitySubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ActivitySubscriptionTransactions_SaleTransactionId",
                table: "ActivitySubscriptionTransactions",
                column: "SaleTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_BranchId_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "BranchId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId_CreatedAt",
                table: "AuditLogs",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizedDevices_BranchId",
                table: "AuthorizedDevices",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorizedDevices_RegisteredByUserId",
                table: "AuthorizedDevices",
                column: "RegisteredByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowedGymSessions_BranchId",
                table: "BorrowedGymSessions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowedGymSessions_CheckInId",
                table: "BorrowedGymSessions",
                column: "CheckInId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowedGymSessions_ExpiredSubscriptionId",
                table: "BorrowedGymSessions",
                column: "ExpiredSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowedGymSessions_MemberId",
                table: "BorrowedGymSessions",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_BorrowedGymSessions_SettledBySubscriptionId",
                table: "BorrowedGymSessions",
                column: "SettledBySubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchClosures_BranchId",
                table: "BranchClosures",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchClosures_CreatedByUserId",
                table: "BranchClosures",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchNetworkRules_BranchId",
                table: "BranchNetworkRules",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_ActivitySubscriptionId",
                table: "CheckIns",
                column: "ActivitySubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_BranchId_CheckedInAt",
                table: "CheckIns",
                columns: new[] { "BranchId", "CheckedInAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_CoachEmployeeId",
                table: "CheckIns",
                column: "CoachEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_CreatedByUserId",
                table: "CheckIns",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_GroupClassId",
                table: "CheckIns",
                column: "GroupClassId");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_MemberId_CheckedInAt",
                table: "CheckIns",
                columns: new[] { "MemberId", "CheckedInAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_MemberSubscriptionId",
                table: "CheckIns",
                column: "MemberSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSchedules_GroupClassId",
                table: "ClassSchedules",
                column: "GroupClassId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionPeriods_BranchId",
                table: "CommissionPeriods",
                column: "BranchId",
                unique: true,
                filter: "\"Status\" = 'Open'");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionPeriods_ClosedByUserId",
                table: "CommissionPeriods",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionPeriods_ReopenedByUserId",
                table: "CommissionPeriods",
                column: "ReopenedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionRules_BranchId_EmployeeId_CommissionType_Effectiv~",
                table: "CommissionRules",
                columns: new[] { "BranchId", "EmployeeId", "CommissionType", "EffectiveFrom" });

            migrationBuilder.CreateIndex(
                name: "IX_CommissionRules_CreatedByUserId",
                table: "CommissionRules",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionRules_EmployeeId",
                table: "CommissionRules",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionRules_GroupClassId",
                table: "CommissionRules",
                column: "GroupClassId");

            migrationBuilder.CreateIndex(
                name: "IX_CommissionRules_SportsActivityId",
                table: "CommissionRules",
                column: "SportsActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTemplates_CreatedByUserId",
                table: "ContractTemplates",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTemplates_Name_Version",
                table: "ContractTemplates",
                columns: new[] { "Name", "Version" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeBranchAssignments_AssignedByUserId",
                table: "EmployeeBranchAssignments",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeBranchAssignments_BranchId",
                table: "EmployeeBranchAssignments",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeBranchAssignments_EmployeeId",
                table: "EmployeeBranchAssignments",
                column: "EmployeeId",
                unique: true,
                filter: "\"EndDate\" IS NULL AND \"IsPrimary\"");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeBranchAssignments_EmployeeId_EndDate",
                table: "EmployeeBranchAssignments",
                columns: new[] { "EmployeeId", "EndDate" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCommissionSummaries_BranchId",
                table: "EmployeeCommissionSummaries",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCommissionSummaries_CommissionPeriodId_EmployeeId_Co~",
                table: "EmployeeCommissionSummaries",
                columns: new[] { "CommissionPeriodId", "EmployeeId", "CommissionType" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCommissionSummaries_EmployeeId",
                table: "EmployeeCommissionSummaries",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCommissionSummaries_GroupClassId",
                table: "EmployeeCommissionSummaries",
                column: "GroupClassId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCommissionSummaries_SportsActivityId",
                table: "EmployeeCommissionSummaries",
                column: "SportsActivityId");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_UserId",
                table: "Employees",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalarySettings_CreatedByUserId",
                table: "EmployeeSalarySettings",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalarySettings_EmployeeId",
                table: "EmployeeSalarySettings",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_Name",
                table: "ExpenseCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ApprovedByUserId",
                table: "Expenses",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_BranchId",
                table: "Expenses",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_CreatedByUserId",
                table: "Expenses",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ExpenseCategoryId",
                table: "Expenses",
                column: "ExpenseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_FinancialAccountId",
                table: "Expenses",
                column: "FinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ShiftId",
                table: "Expenses",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialAccounts_BranchId",
                table: "FinancialAccounts",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_BranchId",
                table: "FinancialTransactions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_CreatedByUserId",
                table: "FinancialTransactions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_ExpenseId",
                table: "FinancialTransactions",
                column: "ExpenseId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_FinancialAccountId_OccurredAt",
                table: "FinancialTransactions",
                columns: new[] { "FinancialAccountId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_PaymentId",
                table: "FinancialTransactions",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_PayrollId",
                table: "FinancialTransactions",
                column: "PayrollId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_RefundId",
                table: "FinancialTransactions",
                column: "RefundId");

            migrationBuilder.CreateIndex(
                name: "IX_FreelanceAgreements_BranchId",
                table: "FreelanceAgreements",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_FreelanceAgreements_CreatedByUserId",
                table: "FreelanceAgreements",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_FreelanceAgreements_EmployeeId",
                table: "FreelanceAgreements",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupClasses_BranchId",
                table: "GroupClasses",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_GroupClasses_CoachEmployeeId",
                table: "GroupClasses",
                column: "CoachEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_GymBranches_CityId",
                table: "GymBranches",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_InstallmentAgreements_CreatedByUserId",
                table: "InstallmentAgreements",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_InstallmentAgreements_SaleTransactionId",
                table: "InstallmentAgreements",
                column: "SaleTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Installments_InstallmentAgreementId_InstallmentNumber",
                table: "Installments",
                columns: new[] { "InstallmentAgreementId", "InstallmentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeadAssignments_AssignedByUserId",
                table: "LeadAssignments",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadAssignments_LeadId",
                table: "LeadAssignments",
                column: "LeadId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadAssignments_SalesEmployeeId",
                table: "LeadAssignments",
                column: "SalesEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadInteractions_EmployeeId",
                table: "LeadInteractions",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeadInteractions_LeadId_NextFollowUpAt",
                table: "LeadInteractions",
                columns: new[] { "LeadId", "NextFollowUpAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Leads_BranchId",
                table: "Leads",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_ConvertedMemberId",
                table: "Leads",
                column: "ConvertedMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Leads_CreatedByUserId",
                table: "Leads",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberContracts_ContractTemplateId",
                table: "MemberContracts",
                column: "ContractTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberContracts_GeneratedByUserId",
                table: "MemberContracts",
                column: "GeneratedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberContracts_MemberId",
                table: "MemberContracts",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberContracts_SaleTransactionId",
                table: "MemberContracts",
                column: "SaleTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberOfferEntitlements_CreatedByUserId",
                table: "MemberOfferEntitlements",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberOfferEntitlements_GrantedByEmployeeId",
                table: "MemberOfferEntitlements",
                column: "GrantedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberOfferEntitlements_MemberId",
                table: "MemberOfferEntitlements",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberOfferEntitlements_OfferId",
                table: "MemberOfferEntitlements",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_CreatedByUserId",
                table: "Members",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_HomeBranchId",
                table: "Members",
                column: "HomeBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_MemberCode",
                table: "Members",
                column: "MemberCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Members_PrimaryPhone",
                table: "Members",
                column: "PrimaryPhone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberSalesAssignments_AssignedByUserId",
                table: "MemberSalesAssignments",
                column: "AssignedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberSalesAssignments_MemberId",
                table: "MemberSalesAssignments",
                column: "MemberId",
                unique: true,
                filter: "\"EndedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MemberSalesAssignments_MemberId_EndedAt",
                table: "MemberSalesAssignments",
                columns: new[] { "MemberId", "EndedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MemberSalesAssignments_SalesEmployeeId",
                table: "MemberSalesAssignments",
                column: "SalesEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipPlans_CityId",
                table: "MembershipPlans",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSubscriptionTransactions_MembershipPlanId",
                table: "MembershipSubscriptionTransactions",
                column: "MembershipPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSubscriptionTransactions_MemberSubscriptionId",
                table: "MembershipSubscriptionTransactions",
                column: "MemberSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_MembershipSubscriptionTransactions_SaleTransactionId",
                table: "MembershipSubscriptionTransactions",
                column: "SaleTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberSubscriptions_CityId",
                table: "MemberSubscriptions",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberSubscriptions_MemberId",
                table: "MemberSubscriptions",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberSubscriptions_MembershipPlanId",
                table: "MemberSubscriptions",
                column: "MembershipPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberSubscriptions_PurchasedAtBranchId",
                table: "MemberSubscriptions",
                column: "PurchasedAtBranchId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageLogs_MemberId",
                table: "MessageLogs",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferActivityPackages_ActivityPackageId",
                table: "OfferActivityPackages",
                column: "ActivityPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_OfferMembershipPlans_MembershipPlanId",
                table: "OfferMembershipPlans",
                column: "MembershipPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_BranchId",
                table: "Offers",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_CreatedByUserId",
                table: "Offers",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentParts_FinancialAccountId",
                table: "PaymentParts",
                column: "FinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentParts_PaymentId",
                table: "PaymentParts",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BranchId_PaidAt",
                table: "Payments",
                columns: new[] { "BranchId", "PaidAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CommissionPeriodId",
                table: "Payments",
                column: "CommissionPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InstallmentId",
                table: "Payments",
                column: "InstallmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ReceivedByUserId",
                table: "Payments",
                column: "ReceivedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SaleTransactionId",
                table: "Payments",
                column: "SaleTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ShiftId",
                table: "Payments",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollDeductions_CreatedByUserId",
                table: "PayrollDeductions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PayrollDeductions_PayrollId",
                table: "PayrollDeductions",
                column: "PayrollId");

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_BranchId",
                table: "Payrolls",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_CommissionPeriodId",
                table: "Payrolls",
                column: "CommissionPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_EmployeeId",
                table: "Payrolls",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_PaidByUserId",
                table: "Payrolls",
                column: "PaidByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Payrolls_PaidFromFinancialAccountId",
                table: "Payrolls",
                column: "PaidFromFinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Permissions_Code",
                table: "Permissions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PrivateCoachAssignments_CoachEmployeeId",
                table: "PrivateCoachAssignments",
                column: "CoachEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateCoachAssignments_CreatedByUserId",
                table: "PrivateCoachAssignments",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateCoachAssignments_PrivateSubscriptionId",
                table: "PrivateCoachAssignments",
                column: "PrivateSubscriptionId",
                unique: true,
                filter: "\"EndedAt\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PrivatePackages_BranchId",
                table: "PrivatePackages",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSessions_CoachEmployeeId",
                table: "PrivateSessions",
                column: "CoachEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSessions_CreatedByUserId",
                table: "PrivateSessions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSessions_GymCheckInId",
                table: "PrivateSessions",
                column: "GymCheckInId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSessions_PrivateSubscriptionId_SessionAt",
                table: "PrivateSessions",
                columns: new[] { "PrivateSubscriptionId", "SessionAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSubscriptions_BranchId",
                table: "PrivateSubscriptions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSubscriptions_MemberId",
                table: "PrivateSubscriptions",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSubscriptionTransactions_PrivatePackageId",
                table: "PrivateSubscriptionTransactions",
                column: "PrivatePackageId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSubscriptionTransactions_PrivateSubscriptionId",
                table: "PrivateSubscriptionTransactions",
                column: "PrivateSubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PrivateSubscriptionTransactions_SaleTransactionId",
                table: "PrivateSubscriptionTransactions",
                column: "SaleTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundParts_FinancialAccountId",
                table: "RefundParts",
                column: "FinancialAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundParts_RefundId",
                table: "RefundParts",
                column: "RefundId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_ApprovedByUserId",
                table: "Refunds",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_BranchId",
                table: "Refunds",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_OriginalPaymentId",
                table: "Refunds",
                column: "OriginalPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_ProcessedByUserId",
                table: "Refunds",
                column: "ProcessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_SaleTransactionId",
                table: "Refunds",
                column: "SaleTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Refunds_ShiftId",
                table: "Refunds",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleTransactions_BranchId_CreatedAt",
                table: "SaleTransactions",
                columns: new[] { "BranchId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SaleTransactions_CreatedByUserId",
                table: "SaleTransactions",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleTransactions_CreatedShiftId",
                table: "SaleTransactions",
                column: "CreatedShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleTransactions_MemberId_CreatedAt",
                table: "SaleTransactions",
                columns: new[] { "MemberId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_SaleTransactions_OfferId",
                table: "SaleTransactions",
                column: "OfferId");

            migrationBuilder.CreateIndex(
                name: "IX_SaleTransactions_SalesEmployeeId",
                table: "SaleTransactions",
                column: "SalesEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_ShiftEmployees_EmployeeId",
                table: "ShiftEmployees",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_BranchId",
                table: "Shifts",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_ClosedByUserId",
                table: "Shifts",
                column: "ClosedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_OpenedByUserId",
                table: "Shifts",
                column: "OpenedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SportsActivities_BranchId",
                table: "SportsActivities",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionFreezes_ApprovedByUserId",
                table: "SubscriptionFreezes",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionFreezes_MemberSubscriptionId",
                table: "SubscriptionFreezes",
                column: "MemberSubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActivitySchedules");

            migrationBuilder.DropTable(
                name: "ActivitySubscriptionTransactions");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "AuthorizedDevices");

            migrationBuilder.DropTable(
                name: "BorrowedGymSessions");

            migrationBuilder.DropTable(
                name: "BranchClosures");

            migrationBuilder.DropTable(
                name: "BranchNetworkRules");

            migrationBuilder.DropTable(
                name: "ClassSchedules");

            migrationBuilder.DropTable(
                name: "CommissionRules");

            migrationBuilder.DropTable(
                name: "EmployeeBranchAssignments");

            migrationBuilder.DropTable(
                name: "EmployeeCommissionSummaries");

            migrationBuilder.DropTable(
                name: "EmployeeSalarySettings");

            migrationBuilder.DropTable(
                name: "FinancialTransactions");

            migrationBuilder.DropTable(
                name: "FreelanceAgreements");

            migrationBuilder.DropTable(
                name: "LeadAssignments");

            migrationBuilder.DropTable(
                name: "LeadInteractions");

            migrationBuilder.DropTable(
                name: "MemberContracts");

            migrationBuilder.DropTable(
                name: "MemberOfferEntitlements");

            migrationBuilder.DropTable(
                name: "MemberSalesAssignments");

            migrationBuilder.DropTable(
                name: "MembershipSubscriptionTransactions");

            migrationBuilder.DropTable(
                name: "MessageLogs");

            migrationBuilder.DropTable(
                name: "OfferActivityPackages");

            migrationBuilder.DropTable(
                name: "OfferMembershipPlans");

            migrationBuilder.DropTable(
                name: "PaymentParts");

            migrationBuilder.DropTable(
                name: "PayrollDeductions");

            migrationBuilder.DropTable(
                name: "PrivateCoachAssignments");

            migrationBuilder.DropTable(
                name: "PrivateSessions");

            migrationBuilder.DropTable(
                name: "PrivateSubscriptionTransactions");

            migrationBuilder.DropTable(
                name: "RefundParts");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "ShiftEmployees");

            migrationBuilder.DropTable(
                name: "SubscriptionFreezes");

            migrationBuilder.DropTable(
                name: "Expenses");

            migrationBuilder.DropTable(
                name: "Leads");

            migrationBuilder.DropTable(
                name: "ContractTemplates");

            migrationBuilder.DropTable(
                name: "Payrolls");

            migrationBuilder.DropTable(
                name: "CheckIns");

            migrationBuilder.DropTable(
                name: "PrivatePackages");

            migrationBuilder.DropTable(
                name: "PrivateSubscriptions");

            migrationBuilder.DropTable(
                name: "Refunds");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "ExpenseCategories");

            migrationBuilder.DropTable(
                name: "FinancialAccounts");

            migrationBuilder.DropTable(
                name: "ActivitySubscriptions");

            migrationBuilder.DropTable(
                name: "GroupClasses");

            migrationBuilder.DropTable(
                name: "MemberSubscriptions");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "ActivityCoachAssignments");

            migrationBuilder.DropTable(
                name: "ActivityPackages");

            migrationBuilder.DropTable(
                name: "MembershipPlans");

            migrationBuilder.DropTable(
                name: "CommissionPeriods");

            migrationBuilder.DropTable(
                name: "Installments");

            migrationBuilder.DropTable(
                name: "SportsActivities");

            migrationBuilder.DropTable(
                name: "InstallmentAgreements");

            migrationBuilder.DropTable(
                name: "SaleTransactions");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "Offers");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "GymBranches");

            migrationBuilder.DropTable(
                name: "Cities");
        }
    }
}
