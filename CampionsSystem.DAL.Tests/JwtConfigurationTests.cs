using System.Security.Cryptography;
using CampionsSystem.API.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CampionsSystem.DAL.Tests;

public sealed class JwtConfigurationTests
{
    [Theory]
    [InlineData("Jwt:Key", "")]
    [InlineData("Jwt:Key", "invalid-base64")]
    [InlineData("Jwt:Key", "dG9vLXNob3J0")]
    [InlineData("Jwt:Issuer", "")]
    [InlineData("Jwt:Audience", "")]
    [InlineData("Jwt:AccessTokenMinutes", "0")]
    [InlineData("Jwt:AccessTokenMinutes", "61")]
    public void InvalidJwtSettingsAreRejected(string setting, string value)
    {
        var values = new Dictionary<string, string?>
        {
            ["Jwt:Key"] = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)),
            ["Jwt:Issuer"] = "Tests",
            ["Jwt:Audience"] = "Tests.Clients",
            ["Jwt:AccessTokenMinutes"] = "5",
            [setting] = value
        };
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddGymAuthentication(new ConfigurationBuilder().AddInMemoryCollection(values).Build());
        using var provider = services.BuildServiceProvider();
        Assert.Throws<OptionsValidationException>(() => provider.GetRequiredService<IOptions<JwtOptions>>().Value);
    }
}
