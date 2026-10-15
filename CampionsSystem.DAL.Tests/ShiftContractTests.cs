using System.Text.Json;
using CampionsSystem.BLL.Shifts;

namespace CampionsSystem.DAL.Tests;

public sealed class ShiftContractTests
{
    [Fact]
    public void SelfClosureRequiresAnExplicitPhysicalCountWhileOverrideDoesNot()
    {
        var id = Guid.NewGuid();
        var body = $$"""{"operationId":"{{id}}","expectedVersion":1,"reason":"Absence"}""";
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<CloseShiftRequest>(body, options));
        Assert.Null(JsonSerializer.Deserialize<OverrideCloseShiftRequest>(body, options)!.CountedCash);
    }
}
