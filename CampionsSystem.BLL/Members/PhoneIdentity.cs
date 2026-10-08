using CampionsSystem.DAL.Data;
using Microsoft.EntityFrameworkCore;
namespace CampionsSystem.BLL.Members;

// Shared transaction lock serializes registration across Leads and Members for one canonical phone.
internal static class PhoneIdentity
{
    internal static Task LockAsync(ApplicationDbContext db,string phone,CancellationToken ct) =>
        db.Database.ExecuteSqlInterpolatedAsync($"SELECT pg_advisory_xact_lock(hashtextextended({phone}, 0))",ct);
    internal static async Task<Guid?> FindMemberAsync(ApplicationDbContext db,string phone,CancellationToken ct)
    {
        var ids=await db.Database.SqlQuery<Guid>($"""
            SELECT m."Id" AS "Value" FROM "Members" m JOIN "GymBranches" b ON b."Id"=m."HomeBranchId"
            WHERE m."PrimaryPhoneNormalized"={phone} OR (m."PrimaryPhoneNormalized" IS NULL AND CASE
              WHEN regexp_replace(btrim(m."PrimaryPhone"),'[[:space:]().-]','','g') ~ '^[+][1-9][0-9]*$'
                THEN regexp_replace(btrim(m."PrimaryPhone"),'[[:space:]().-]','','g')
              WHEN b."CountryCallingCode" IS NOT NULL AND regexp_replace(btrim(m."PrimaryPhone"),'[[:space:]().-]','','g') ~ '^[0-9]+$'
                THEN b."CountryCallingCode" || regexp_replace(regexp_replace(btrim(m."PrimaryPhone"),'[[:space:]().-]','','g'),'^0','')
              ELSE NULL END={phone}) LIMIT 1
            """).ToListAsync(ct);
        return ids.Count==0?null:ids[0];
    }
    internal static async Task<Guid?> FindLeadAsync(ApplicationDbContext db,string phone,CancellationToken ct)
    {
        var ids=await db.Database.SqlQuery<Guid>($"""
            SELECT l."Id" AS "Value" FROM "Leads" l JOIN "GymBranches" b ON b."Id"=l."BranchId"
            WHERE l."PhoneNormalized"={phone} OR (l."PhoneNormalized" IS NULL AND CASE
              WHEN regexp_replace(btrim(l."Phone"),'[[:space:]().-]','','g') ~ '^[+][1-9][0-9]*$'
                THEN regexp_replace(btrim(l."Phone"),'[[:space:]().-]','','g')
              WHEN b."CountryCallingCode" IS NOT NULL AND regexp_replace(btrim(l."Phone"),'[[:space:]().-]','','g') ~ '^[0-9]+$'
                THEN b."CountryCallingCode" || regexp_replace(regexp_replace(btrim(l."Phone"),'[[:space:]().-]','','g'),'^0','')
              ELSE NULL END={phone}) LIMIT 1
            """).ToListAsync(ct);
        return ids.Count==0?null:ids[0];
    }
}
