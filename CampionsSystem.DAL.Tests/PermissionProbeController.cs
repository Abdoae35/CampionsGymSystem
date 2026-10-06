using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampionsSystem.DAL.Tests;

// Registered by the test host only. Not shipped in the API assembly.
[ApiController]
[Route("_tests/auth")]
public sealed class PermissionProbeController : ControllerBase
{
    [HttpGet("permission")]
    [Authorize(Policy = "Permission:Members.View")]
    public IActionResult Permission() => Ok();

    [HttpGet("owner")]
    [Authorize(Roles = "Owner")]
    public IActionResult Owner() => Ok();

    [HttpGet("standard-policy")]
    [Authorize(Policy = "TestAuthenticated")]
    public IActionResult StandardPolicy() => Ok();
}
