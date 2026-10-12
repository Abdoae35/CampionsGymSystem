# Exact capability reference
Policy inventory below is extracted from actual registered controller metadata; additional service-only keys appear separately. Capability presence in source/catalog does not mean activated/granted for any deployment. Role grants and user overrides were not queried. Coaching keys below are implemented policies; catalog presence is not a grant.
| Exact permission key | Registered actions using policy |
|---|---|
| `Activities.Manage` | OperationsController.PurchaseActivity |
| `Activities.ManageCoaches` | OperationsController.Assign, OperationsController.CreateCoach, OperationsController.Percentage, OperationsController.Unassign |
| `Activities.ViewCoachRevenue` | OperationsController.ActivityRevenue |
| `Advances.Create` | EmployeeAdvancesController.Paid |
| `Advances.View` | EmployeeAdvancesController.Get, EmployeeAdvancesController.History |
| `Branches.AccessAll` | FinancialAccountsController.Create, SubscriptionsController.ApplyHoliday, SubscriptionsController.Holiday |
| `Branches.Manage` | BranchesController.Create, BranchesController.Update, FinancialAccountsController.Create |
| `Branches.View` | BranchesController.Get, BranchesController.List |
| `CRM.ManageHouseFollowUps` | CrmController.Assign, CrmController.House |
| `CRM.ManageMemberFollowUps` | CrmController.Complete, CrmController.Open |
| `CRM.ViewMemberFollowUps` | CrmController.Mine |
| `CheckIns.Create` | CheckInsController.Create, CheckInsController.Evaluate, CheckInsController.Lookup, CheckInsController.Trial |
| `CheckIns.View` | CheckInsController.Debts, CheckInsController.History, CheckInsController.Today |
| `Classes.Manage` | CoachingController.CompleteOccurrence, CoachingController.CreateClassType, CoachingController.CreateClassesPackage, CoachingController.CreateOccurrence, CoachingController.PurchaseClasses, CoachingController.UpdateClassType, CoachingController.UpdateClassesPackage, OperationsController.Attendance, OperationsController.CreateSession, OperationsController.UpdateSession |
| `Classes.RegisterAttendance` | CoachingController.RegisterClass |
| `Classes.View` | CoachingController.ClassMemberAttendance, CoachingController.ClassAttendance, CoachingController.ClassMembers, CoachingController.ClassSubscriptions, CoachingController.ClassTypes, CoachingController.ClassesPackages, CoachingController.Occurrences |
| `Classes.ViewAttendanceReport` | CoachingController.ClassStatistics, OperationsController.ClassReport |
| `CoachEarnings.ManageTargets` | CoachingController.Targets |
| `CoachEarnings.Pay` | CoachingController.Payout |
| `CoachEarnings.Recognize` | CoachingController.Recognize |
| `CoachEarnings.View` | CoachingController.Balance, CoachingController.EarningsHistory, CoachingController.PayoutHistory, CoachingController.Progress, CoachingController.TargetHistory |
| `Commission.ClosePeriod` | CommissionsController.Finalize, CommissionsController.Open, OperationsController.FinalizeActivity |
| `Commission.ManageRules` | CommissionsController.Configure |
| `Commission.View` | CommissionsController.Employee, CommissionsController.History, CommissionsController.Progress, CommissionsController.RefundAllocations, CommissionsController.Rules, CommissionsController.Sources |
| `Employees.Manage` | EmployeeBranchesController.Assign, EmployeesController.Create, EmployeesController.Status, EmployeesController.Update, SubscriptionsController.Employee |
| `Employees.Transfer` | EmployeeBranchesController.Transfer |
| `Employees.View` | EmployeeBranchesController.Current, EmployeeBranchesController.History, EmployeesController.BranchEmployees, EmployeesController.Get, EmployeesController.List |
| `Expenses.Create` | ExpensesController.Paid |
| `Expenses.Manage` | ExpensesController.Category |
| `Expenses.Reverse` | ExpensesController.Return |
| `Expenses.View` | ExpensesController.Categories, ExpensesController.Get, ExpensesController.History, ExpensesController.Returns |
| `FreelancePlans.Manage` | OperationsController.CreatePlan, OperationsController.PurchaseFreelance, OperationsController.UpdatePlan |
| `FreelancePlans.ManageEligibleMembers` | MembershipPlansController.AddEligible, MembershipPlansController.Candidates, MembershipPlansController.Eligible, MembershipPlansController.RemoveEligible, OperationsController.AddEligible, OperationsController.RemoveEligible |
| `FreelancePlans.View` | OperationsController.FreelanceReport |
| `Leads.Assign` | LeadsController.Assign, LeadsController.Bulk |
| `Leads.Convert` | LeadsController.Convert |
| `Leads.Manage` | LeadsController.Contact, LeadsController.Create, LeadsController.Status, LeadsController.Update |
| `Leads.View` | LeadsController.Assignment, LeadsController.AssignmentHistory, LeadsController.Contacts, LeadsController.Get, LeadsController.List, LeadsController.Lookup, LeadsController.Mine |
| `Members.AssignSales` | MembersController.AssignSales, MembersController.ReturnToHouse |
| `Members.ChangeStatus` | MembersController.Status |
| `Members.Create` | MembersController.Create |
| `Members.Edit` | MembersController.Update |
| `Members.Manage` | OffersController.Audiences |
| `Members.View` | MembersController.CurrentSales, MembersController.Get, MembersController.List |
| `Members.ViewSalesHistory` | MembersController.SalesHistory |
| `MembershipPlans.Manage` | MembershipBenefitsController.Create, MembershipBenefitsController.Status, MembershipBenefitsController.Update, MembershipPlanCategoriesController.Create, MembershipPlanCategoriesController.Status, MembershipPlanCategoriesController.Update, MembershipPlansController.Create, MembershipPlansController.SetBenefits, MembershipPlansController.Status, MembershipPlansController.Update |
| `MembershipPlans.View` | MembershipBenefitsController.Get, MembershipBenefitsController.List, MembershipPlanCategoriesController.List, MembershipPlansController.Benefits, MembershipPlansController.Get, MembershipPlansController.List |
| `Offers.Manage` | OffersController.Create, OffersController.Details, OffersController.Grant, OffersController.Management, OffersController.Revoke, OffersController.Update |
| `Payments.Create` | CoachingController.PurchaseClasses, CoachingController.PurchasePrivate, OffersController.Eligible, OffersController.Quote, SalesController.Create, SalesController.Pay |
| `Payments.Refund` | SalesController.Refund |
| `Payments.View` | FinancialAccountsController.Accounts, SalesController.Details, SalesController.History, SalesController.Installments, SalesController.Payments, SalesController.Refunds, SalesController.Totals |
| `Private.Manage` | CoachingController.CreatePrivatePackage, CoachingController.PurchasePrivate, CoachingController.UpdatePrivatePackage |
| `Private.RegisterSession` | CoachingController.RegisterPrivate |
| `Private.SettleExpiry` | CoachingController.SettleExpiry |
| `Private.Transfer` | CoachingController.Transfer |
| `Private.View` | CoachingController.ExpiryHistory, CoachingController.PrivateMembers, CoachingController.PrivatePackages, CoachingController.PrivateSessions, CoachingController.PrivateSubscription, CoachingController.PrivateSubscriptions |
| `Sales.QuickCancel` | SalesController.QuickCancel |
| `Shifts.Manage` | BusinessDaysController.Close, BusinessDaysController.Open, ShiftsController.Close, ShiftsController.Open |
| `Shifts.View` | BusinessDaysController.Current, ShiftsController.Current, ShiftsController.Get, ShiftsController.History, ShiftsController.Summary |
| `Subscriptions.Extend` | SubscriptionsController.ApplyHoliday, SubscriptionsController.Extend, SubscriptionsController.Holiday |
| `Subscriptions.Freeze` | SubscriptionsController.Complete, SubscriptionsController.Freeze |
| `Subscriptions.Manage` | SubscriptionsController.Employee |
| `Subscriptions.View` | SubscriptionsController.Current, SubscriptionsController.Details, SubscriptionsController.History |
| `Vault.Manage` | VaultController.Count, VaultController.Setup |
| `Vault.View` | VaultController.Audit, VaultController.Counts, VaultController.Movements, VaultController.Position, VaultController.Summary |
| `Vault.Withdraw` | VaultController.Withdraw |

## Service-only capabilities and catalog compatibility
Additional keys directly checked in baseline BLL: `Members.ChangePrimaryPhone`, `Members.CreateCrossBranch`, `Members.OwnSalesCustomers`.
Catalog/seed declarations not directly routed: `dev.inactive`, `dev.owner`, `dev.reception`, `dev.suspended`.
Exact policy prefix `Permission:`; no role bypass. Additional conjunction/ownership checks are in each module rule file. The full role-template source is not a claim about existing users.
