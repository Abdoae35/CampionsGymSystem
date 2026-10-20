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

    [Fact]
    public void OldDrawerSnapshotDeserializesWithoutNewAccountName()
    {
        var json = $$"""{"branchId":"{{Guid.NewGuid()}}","accountId":"{{Guid.NewGuid()}}","bookCash":10,"expectedPhysicalCash":10,"lastCountedCash":10,"movementSequence":1,"lastCountId":null,"observedDiscrepancies":0}""";
        var position = JsonSerializer.Deserialize<DrawerPosition>(json, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(position);
        Assert.Null(position.AccountName);
        Assert.Equal(10, position.BookCash);
    }
}
