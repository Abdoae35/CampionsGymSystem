namespace CampionsSystem.DAL.Entities;

// A short-lived, server-owned preview. Financial and customer data live in their existing tables.
public sealed class ExcelImportBatch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CreatedByUserId { get; set; }
    public string Kind { get; set; } = null!;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public ICollection<ExcelImportRow> Rows { get; set; } = new List<ExcelImportRow>();
}

public sealed class ExcelImportRow
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BatchId { get; set; }
    public ExcelImportBatch Batch { get; set; } = null!;
    public int RowNumber { get; set; }
    public string PayloadJson { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string? Message { get; set; }
    public Guid? CreatedRecordId { get; set; }
}
