using CampionsSystem.BLL.Imports;
using CampionsSystem.BLL.Operations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace CampionsSystem.API.Controllers;

public sealed class ExcelImportUploadRequest
{
    [Required] public IFormFile File { get; set; } = null!;
}

[ApiController, Authorize, Route("api/excel-imports")]
public sealed class ExcelImportsController(ExcelImportService imports) : BranchControllerBase
{
    [HttpGet("{kind}/template")]
    [Produces("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")]
    public async Task<IActionResult> Template(string kind, CancellationToken ct)
    {
        var response = await Execute(() => imports.TemplateAsync(kind, ct));
        return response is OkObjectResult { Value: byte[] bytes }
            ? File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Campeones-{kind}-import-template.xlsx")
            : response;
    }

    [HttpPost("{kind}/preview")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(1_100_000)]
    [ProducesResponseType(typeof(ExcelImportResult), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(403)]
    public Task<IActionResult> Preview(string kind, [FromForm] ExcelImportUploadRequest request, CancellationToken ct) => Execute(async () =>
    {
        var file = request.File;
        if (file is null || file.Length is < 100 or > 1_048_576)
            throw new BusinessRuleException("Upload an .xlsx workbook no larger than 1 MiB.");
        await using var stream = file.OpenReadStream();
        using var data = new MemoryStream();
        await stream.CopyToAsync(data, ct);
        return await imports.PreviewAsync(kind, file.FileName, data.ToArray(), ct);
    });

    [HttpPost("{id:guid}/confirm")]
    [ProducesResponseType(typeof(ExcelImportResult), 200)]
    [ProducesResponseType(typeof(ProblemDetails), 400)]
    [ProducesResponseType(403)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public Task<IActionResult> Confirm(Guid id, CancellationToken ct) => Execute(() => imports.ConfirmAsync(id, ct));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ExcelImportResult), 200)]
    [ProducesResponseType(403)]
    [ProducesResponseType(typeof(ProblemDetails), 404)]
    public Task<IActionResult> Result(Guid id, CancellationToken ct) => Execute(() => imports.GetAsync(id, ct));
}
