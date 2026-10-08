using System.Text.RegularExpressions;
using CampionsSystem.BLL.Operations;

namespace CampionsSystem.BLL.Members;

// Shared simple canonical policy; no implicit country assumptions or external subsystem.
public static class PhoneNormalizer
{
    public static string Normalize(string input, string? countryCallingCode)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length > 64)
            throw new BusinessRuleException("A phone number up to 64 characters is required.");
        var compact = Regex.Replace(input.Trim(), @"[\s().-]", "");
        if (!compact.StartsWith('+'))
        {
            if (countryCallingCode is null || !Regex.IsMatch(countryCallingCode, @"^[+][1-9][0-9]{0,2}$"))
                throw new BusinessRuleException("Configure the registration branch country calling code before entering local phones.");
            if (!Regex.IsMatch(compact, @"^[0-9]+$")) throw new BusinessRuleException("Invalid phone format.");
            if (compact.StartsWith('0')) compact = compact[1..];
            compact = countryCallingCode + compact;
        }
        if (!Regex.IsMatch(compact, @"^[+][1-9][0-9]{6,14}$")) throw new BusinessRuleException("Invalid international phone format.");
        return compact;
    }
}
