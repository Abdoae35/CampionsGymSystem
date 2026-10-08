using CampionsSystem.BLL.Members;
using CampionsSystem.BLL.Operations;

namespace CampionsSystem.DAL.Tests;

public sealed class MemberPhoneTests
{
    [Theory]
    [InlineData("01212345678", "+20", "+201212345678")]
    [InlineData("01212345678", "+44", "+441212345678")]
    [InlineData("+20 (12) 1234-5678", "+44", "+201212345678")]
    [InlineData("+441234567890", null, "+441234567890")]
    public void NormalizationUsesConfigurationAndRespectsInternationalInput(string input, string? country, string expected)
        => Assert.Equal(expected, PhoneNormalizer.Normalize(input, country));
    [Theory]
    [InlineData("01212345678", null)] [InlineData("abc", "+20")]
    [InlineData("+0123456789", "+20")] [InlineData("123", "+20")]
    [InlineData("01212345678", "20")]
    public void InvalidOrUnconfiguredPhonesFail(string input, string? country)
        => Assert.Throws<BusinessRuleException>(() => PhoneNormalizer.Normalize(input, country));
}
