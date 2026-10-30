# Campeones Gym Management Backend

An ASP.NET Core API for managing memberships, branch operations, and financial workflows across a gym business.

## Overview

Campeones centralizes the information and daily processes used by gym owners, managers, receptionists, sales staff, and coaches. It connects member access, subscriptions, staff activity, and financial records across branches, helping teams work from a consistent operational history while respecting branch responsibilities.

## Key Features

- **Members and memberships:** Member profiles, configurable plans, subscription history, renewals, freezes, and managed extensions.
- **Multi-branch operations:** Branch assignments and scoped access to operational and financial records.
- **Attendance and check-in:** Manual and scanner-supplied member-code lookup, access evaluation, session consumption, and visit history.
- **Sales and finance:** Sales, split payments, installments, refunds, expenses, commissions, and monthly financial reporting.
- **Shared cash drawer and shifts:** Branch drawer movements, employee shifts, cash counts, and reconciliation reporting.
- **Private coaching and classes:** Private sessions, coach assignments and earnings, class occurrences, attendance, and coach statistics. Classes compensation is not configured.
- **Employees and coaches:** Employee records, employment status, branch assignments, and employee-linked gym access.
- **CRM and follow-ups:** Lead management, conversion to member profiles, sales ownership, and member follow-up tasks.
- **Access control:** Identity accounts, configurable role permissions, individual allow/deny overrides, and branch-scoped authorization.
- **Reporting and audit:** Shift, coaching, commission, and monthly finance reports, with audit records for sensitive actions.

## Technical Architecture

The controller-based API exposes business operations to a separate React/TypeScript frontend. The API layer handles HTTP requests, authentication, and authorization; the Business Logic layer implements workflows and business rules; and the Data Access layer defines entities, relationships, EF Core mappings, and migrations. PostgreSQL stores the data through the Npgsql provider. ASP.NET Core Identity and JWT bearer authentication support user access, while services enforce effective permissions and branch scope. Cross-origin browser integration requires a proxy or an appropriate CORS configuration.

## Engineering Challenges and Solutions

- **Branch boundaries:** Branch assignments and server-side scope checks restrict access to operational records; financial accounts and drawer activity are associated with branches.
- **Fine-grained access:** Role grants and per-user overrides are evaluated with account and branch context, allowing access to be tailored without relying solely on role names.
- **Membership lifecycle:** Plans can activate on first check-in; access evaluation accounts for subscription status and session balance, while freezes and extensions preserve lifecycle history.
- **Safe financial retries:** Operation identifiers and request matching make supported payment and refund retries idempotent, reducing duplicate postings.
- **Cash consistency:** Database transactions and drawer locking coordinate financial postings and shifts. Physical counts are recorded as observations, keeping discrepancies visible rather than silently changing the ledger.
- **Staff access lifecycle:** Employee entitlements link staff to member access and are closed when employment becomes inactive.
- **Traceability and relationships:** Audit records, persisted transaction evidence, relational constraints, and transactional business rules connect sensitive changes to their operational context.

## Testing and Quality

The xUnit test project includes domain and API tests, PostgreSQL-backed integration tests, and concurrency, permission, migration, and financial workflow tests. Integration fixtures use isolated database schemas. Coverage varies by workflow; this documentation review did not run the tests or verify deployment readiness.

## Technology Stack

- C# and .NET 10; ASP.NET Core Web API and Identity
- Entity Framework Core and PostgreSQL via Npgsql
- JWT bearer authentication and OpenAPI
- xUnit and ASP.NET Core integration testing

## Repository Structure

```text
CampionsSystem.API/        Controllers, authentication, and application startup
CampionsSystem.BLL/        Business services and workflow rules
CampionsSystem.DAL/        Entities, EF Core configuration, and migrations
CampionsSystem.DAL.Tests/  Automated tests
docs/                      Architecture, feature, and API documentation
postman/                   API collections
```

## Getting Started

1. Install the .NET 10 SDK and PostgreSQL, and prepare an isolated database.
2. Configure `ConnectionStrings:DefaultConnection` and a base64-encoded `Jwt:Key` of at least 32 bytes using .NET user secrets or another secure configuration provider. Review the JWT issuer and audience for your environment.
3. Review and apply the EF Core migrations to the intended database; startup does not apply them automatically. Configure the required accounts, permissions, and branch data for the workflows you intend to use.
4. Run `dotnet run --project CampionsSystem.API/CampionsSystem.csproj`. Run tests with `dotnet test CampionsSystem.DAL.Tests/CampionsSystem.DAL.Tests.csproj`; PostgreSQL-backed tests require a suitable test database configuration.

See [Project Documentation](docs/README.md) for setup and feature details.
