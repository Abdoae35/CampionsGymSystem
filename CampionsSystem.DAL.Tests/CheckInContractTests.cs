using System.Text.Json;
using CampionsSystem.BLL.CheckIns;
using CampionsSystem.DAL.Enums;
namespace CampionsSystem.DAL.Tests;
public sealed class CheckInContractTests
{
    [Theory][InlineData("businessDate")][InlineData("subscriptionId")][InlineData("remainingSessions")][InlineData("allowed")]
    public void MutationDoesNotAcceptAuthoritativeClientFacts(string field)
    {
        var body=$"{{\"operationId\":\"{Guid.NewGuid()}\",\"method\":0,\"memberId\":\"{Guid.NewGuid()}\",\"{field}\":null}}";
        Assert.Throws<JsonException>(()=>JsonSerializer.Deserialize<CreateCheckInRequest>(body,new JsonSerializerOptions(JsonSerializerDefaults.Web)));
    }
    [Fact] public void EnumCompatibilityAndExplicitBorrowDefaultArePreserved()
    {
        Assert.Equal(1,(int)CheckInType.BorrowedGym);Assert.Equal(2,(int)CheckInType.Activity);Assert.Equal(3,(int)CheckInType.Class);Assert.Equal(4,(int)CheckInType.Trial);
        Assert.False(new CreateCheckInRequest(Guid.NewGuid(),CheckInMethod.Manual,Guid.NewGuid()).ConfirmBorrow);
    }
}
