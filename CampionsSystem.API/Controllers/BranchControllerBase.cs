using CampionsSystem.BLL.Branches;
using CampionsSystem.BLL.Operations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace CampionsSystem.API.Controllers;

public abstract class BranchControllerBase : ControllerBase
{
    protected async Task<IActionResult> Execute<T>(Func<Task<T>> action)
    {
        try { return Ok(await action()); }
        catch (UnauthorizedAccessException) { return Forbid(); }
        catch (BranchNotFoundException error) { return Problem(statusCode: 404, title: error.Message); }
        catch (BusinessRuleException error) { return Problem(statusCode: 400, title: error.Message); }
        catch (DbUpdateConcurrencyException)
        { return Problem(statusCode: 409, title: "Record changed; reload and retry with its current version."); }
        catch (DbUpdateException error) when (error.InnerException is PostgresException
            { SqlState: PostgresErrorCodes.UniqueViolation or PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected })
        { return Problem(statusCode: 409, title: "Concurrent or overlapping change; reload and retry."); }
        catch (PostgresException error) when (error.SqlState is PostgresErrorCodes.SerializationFailure or PostgresErrorCodes.DeadlockDetected)
        { return Problem(statusCode: 409, title: "Concurrent change; reload and retry."); }
    }
}
