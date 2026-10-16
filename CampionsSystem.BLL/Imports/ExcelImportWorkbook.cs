using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using CampionsSystem.BLL.Operations;

namespace CampionsSystem.BLL.Imports;

internal static class ExcelImportWorkbook
{
    internal const int MaxBytes = 1_048_576;
    internal const int MaxRows = 100;
    private static readonly XNamespace Ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private static readonly string[] LeadColumns = ["BranchId", "PrimaryPhone", "FullName", "SecondaryPhone", "Email", "Source", "Notes"];
    private static readonly string[] MemberColumns = ["TargetRegistrationBranchId", "CreationType", "PrimaryPhone", "FullName", "SecondaryPhone", "DateOfBirth", "Gender", "Notes", "SalesEmployeeId"];
    private static readonly string[] FreelanceColumns = ["TargetRegistrationBranchId", "FreelancePlanId", "PrimaryPhone", "FullName", "SecondaryPhone", "DateOfBirth", "Gender", "Notes"];

    internal static string[] Columns(string kind) => kind switch
    {
        "leads" => LeadColumns, "members" => MemberColumns, "freelance-customers" => FreelanceColumns,
        _ => throw new BusinessRuleException("Unknown import type.")
    };

    internal static byte[] Template(string kind)
    {
        var columns = Columns(kind);
        using var output = new MemoryStream();
        using (var zip = new ZipArchive(output, ZipArchiveMode.Create, true))
        {
            Put(zip, "[Content_Types].xml", """<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types"><Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/><Default Extension="xml" ContentType="application/xml"/><Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/><Override PartName="/xl/worksheets/sheet1.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/><Override PartName="/xl/worksheets/sheet2.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"/><Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/></Types>""");
            Put(zip, "_rels/.rels", """<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>""");
            Put(zip, "xl/workbook.xml", """<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets><sheet name="Data" sheetId="1" r:id="rId1"/><sheet name="Instructions" sheetId="2" r:id="rId2"/></sheets></workbook>""");
            Put(zip, "xl/_rels/workbook.xml.rels", """<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet1.xml"/><Relationship Id="rId2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet" Target="worksheets/sheet2.xml"/><Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/></Relationships>""");
            Put(zip, "xl/styles.xml", """<styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main"><fonts count="1"><font><sz val="11"/></font></fonts><fills count="1"><fill><patternFill patternType="none"/></fill></fills><borders count="1"><border/></borders><cellStyleXfs count="1"><xf/></cellStyleXfs><cellXfs count="1"><xf xfId="0" numFmtId="49"/></cellXfs></styleSheet>""");
            Put(zip, "xl/worksheets/sheet1.xml", Sheet([columns]));
            var example = kind switch
            {
                "leads" => "BranchId=<branch UUID>; PrimaryPhone=01012345678; FullName=Example Lead; Source=Instagram",
                "members" => "TargetRegistrationBranchId=<branch UUID>; CreationType=NormalSales or ManagementCreated; PrimaryPhone=01012345678; FullName=Example Member; SalesEmployeeId=<employee UUID for ManagementCreated>",
                _ => "TargetRegistrationBranchId=<branch UUID>; FreelancePlanId=<plan UUID>; PrimaryPhone=01012345678; FullName=Example Customer"
            };
            Put(zip, "xl/worksheets/sheet2.xml", Sheet([["Import instructions"], ["Fill Data sheet from row 2. Keep phones as text, including their leading zero. Maximum 100 rows and 1 MiB file."], ["UUIDs must refer to authorized active branches/plans. Dates use YYYY-MM-DD. No formulas or macros."], [example], ["Required: " + (kind == "leads" ? "BranchId, PrimaryPhone" : kind == "members" ? "TargetRegistrationBranchId, CreationType, PrimaryPhone, FullName" : "TargetRegistrationBranchId, FreelancePlanId, PrimaryPhone, FullName")], ["Preview does not create records. Confirm explicitly. Existing manual registration rules still apply."]]));
        }
        return output.ToArray();
    }

    private static void Put(ZipArchive zip, string name, string xml)
    {
        using var writer = new StreamWriter(zip.CreateEntry(name, CompressionLevel.Fastest).Open(), new UTF8Encoding(false));
        writer.Write(xml);
    }
    private static string Sheet(IReadOnlyList<string[]> rows)
    {
        var sb = new StringBuilder("<worksheet xmlns=\"http://schemas.openxmlformats.org/spreadsheetml/2006/main\"><sheetData>");
        for (var row = 0; row < rows.Count; row++)
        {
            sb.Append("<row r=\"").Append(row + 1).Append("\">");
            for (var col = 0; col < rows[row].Length; col++)
                sb.Append("<c r=\"").Append((char)('A' + col)).Append(row + 1).Append("\" t=\"inlineStr\" s=\"0\"><is><t>")
                    .Append(System.Security.SecurityElement.Escape(rows[row][col])).Append("</t></is></c>");
            sb.Append("</row>");
        }
        return sb.Append("</sheetData></worksheet>").ToString();
    }

    internal static IReadOnlyList<(int RowNumber, Dictionary<string, string> Values)> Parse(string kind, byte[] bytes)
    {
        if (bytes.Length is < 100 or > MaxBytes) throw new BusinessRuleException("Excel file must be a nonempty .xlsx no larger than 1 MiB.");
        try
        {
            using var input = new MemoryStream(bytes);
            using var zip = new ZipArchive(input, ZipArchiveMode.Read);
            if (zip.Entries.Count is < 5 or > 50
                || zip.Entries.GroupBy(e => e.FullName, StringComparer.OrdinalIgnoreCase).Any(g => g.Count() > 1)
                || zip.Entries.Any(e => e.Length > 2_000_000 || e.FullName.Contains("..", StringComparison.Ordinal)
                || e.FullName.EndsWith("vbaProject.bin", StringComparison.OrdinalIgnoreCase) || e.FullName.Contains("externalLinks/", StringComparison.OrdinalIgnoreCase)))
                throw new BusinessRuleException("Unsafe or oversized Excel workbook.");
            var workbook = Read(zip, "xl/workbook.xml");
            if ((string?)workbook.Root?.Element(Ns + "sheets")?.Elements(Ns + "sheet").FirstOrDefault()?.Attribute("name") != "Data")
                throw new BusinessRuleException("The first worksheet must be Data.");
            var shared = zip.GetEntry("xl/sharedStrings.xml") is null ? [] : Read(zip, "xl/sharedStrings.xml")
                .Descendants(Ns + "si").Select(si => string.Concat(si.Descendants(Ns + "t").Select(t => t.Value))).ToArray();
            if (shared.Length > 10_000) throw new BusinessRuleException("Too many shared strings.");
            var sheet = Read(zip, "xl/worksheets/sheet1.xml");
            if (sheet.Descendants(Ns + "f").Any()) throw new BusinessRuleException("Formula cells are not permitted.");
            var parsed = new SortedDictionary<int, Dictionary<int, string>>();
            foreach (var row in sheet.Descendants(Ns + "sheetData").Elements(Ns + "row"))
            {
                foreach (var cell in row.Elements(Ns + "c"))
                {
                    var reference = (string?)cell.Attribute("r") ?? "";
                    var match = Regex.Match(reference, "^([A-Z]{1,2})([0-9]{1,4})$", RegexOptions.CultureInvariant);
                    if (!match.Success) throw new BusinessRuleException("Invalid Excel cell address.");
                    var col = 0; foreach (var ch in match.Groups[1].Value) col = col * 26 + ch - 'A' + 1;
                    var number = int.Parse(match.Groups[2].Value);
                    if (number is < 1 or > MaxRows + 1 || col is < 1 or > 20) throw new BusinessRuleException("Excel row or column limit exceeded.");
                    var type = (string?)cell.Attribute("t");
                    var raw = type == "inlineStr" ? string.Concat(cell.Descendants(Ns + "t").Select(t => t.Value)) : (string?)cell.Element(Ns + "v") ?? "";
                    if (type == "s") raw = int.TryParse(raw, out var index) && index >= 0 && index < shared.Length ? shared[index] : throw new BusinessRuleException("Invalid shared string index.");
                    if (type == "e") throw new BusinessRuleException("Excel error cells are not permitted.");
                    if (raw.Length > 4096) throw new BusinessRuleException("Excel cell value is too long.");
                    if (!parsed.TryGetValue(number, out var cells)) parsed[number] = cells = [];
                    if (!cells.TryAdd(col, raw.Trim())) throw new BusinessRuleException("Duplicate Excel cell address.");
                }
            }
            if (!parsed.TryGetValue(1, out var header)) throw new BusinessRuleException("Missing header row.");
            var allowed = Columns(kind);
            var names = header.Values.Where(v => v.Length > 0).ToArray();
            if (names.Length != names.Distinct(StringComparer.Ordinal).Count() || names.Any(n => !allowed.Contains(n, StringComparer.Ordinal)))
                throw new BusinessRuleException("Unknown or repeated Excel column name.");
            var required = kind == "leads" ? allowed[..2] : kind == "members" ? allowed[..4] : allowed[..4];
            if (required.Any(n => !names.Contains(n, StringComparer.Ordinal))) throw new BusinessRuleException("Required Excel column is missing.");
            var rows = new List<(int, Dictionary<string, string>)>();
            foreach (var (number, cells) in parsed.Where(p => p.Key > 1))
            {
                if (cells.Values.All(string.IsNullOrWhiteSpace)) continue;
                if (cells.Keys.Any(c => !header.ContainsKey(c) || string.IsNullOrEmpty(header[c]))) throw new BusinessRuleException("Data appears under an unnamed column.");
                rows.Add((number, cells.ToDictionary(c => header[c.Key], c => c.Value, StringComparer.Ordinal)));
            }
            if (rows.Count is < 1 or > MaxRows) throw new BusinessRuleException("Data sheet must contain 1–100 rows.");
            return rows;
        }
        catch (BusinessRuleException) { throw; }
        catch (Exception e) when (e is InvalidDataException or XmlException or IOException or ArgumentException or OverflowException)
        { throw new BusinessRuleException("Invalid or unsupported .xlsx workbook."); }
    }

    private static XDocument Read(ZipArchive zip, string path)
    {
        var entry = zip.GetEntry(path) ?? throw new BusinessRuleException("Missing Excel workbook component.");
        using var stream = entry.Open();
        using var xml = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit, XmlResolver = null, MaxCharactersInDocument = 2_000_000 });
        return XDocument.Load(xml);
    }
}
