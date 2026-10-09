using CampionsSystem.DAL.Entities;
namespace CampionsSystem.BLL.Members;
// Shared Member search predicate; consumers retain their own authorization/projection.
internal static class MemberSearch
{
    internal static IQueryable<Member> Apply(IQueryable<Member> query,string? input,Guid? clientCodeBranch=null)
    {
        if(string.IsNullOrWhiteSpace(input))return query;
        var search=input.Trim().ToLowerInvariant();
        var compact=System.Text.RegularExpressions.Regex.Replace(search,@"[\s().-]","");
        var phone=System.Text.RegularExpressions.Regex.IsMatch(compact,@"^[+]?[0-9]+$")?compact.TrimStart('0'):null;
        if(string.IsNullOrEmpty(phone))phone=null;
        long? code=long.TryParse(search,out var parsed)&&parsed>0?parsed:null;
        return query.Where(m=>m.FullName.ToLower().Contains(search) || m.PrimaryPhone.Contains(search)
            || m.PrimaryPhoneNormalized!=null && phone!=null && m.PrimaryPhoneNormalized.Contains(phone)
            || m.MemberCode.ToLower().Contains(search)
            || code!=null && m.ClientCode==code && (clientCodeBranch==null || m.HomeBranchId==clientCodeBranch));
    }
}
