using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml;
using CampionsSystem.BLL.Operations;
namespace CampionsSystem.BLL.MonthlyFinance;

// Small OOXML writer using the platform ZIP/XML APIs. Inline strings protect user
// descriptions from Excel formula injection. No macros, external links or formulas.
internal static class MonthlyWorkbook
{
    private const string Main="http://schemas.openxmlformats.org/spreadsheetml/2006/main";
    private const string Rel="http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    internal static readonly string[] SheetNames=["Monthly Summary","All Financial Transactions","Sales Performance","Employee Salaries & Advances","Coaches - Private - Classes","Vault & Shifts"];
    public static byte[] Create(MonthlyReport report,CancellationToken ct=default)
    {
        using var buffer=new MemoryStream();
        using(var zip=new ZipArchive(buffer,ZipArchiveMode.Create,true))
        {
            Write(zip,"[Content_Types].xml",x=>
            {
                x.WriteStartElement("Types","http://schemas.openxmlformats.org/package/2006/content-types");
                Empty(x,"Default",("Extension","rels"),("ContentType","application/vnd.openxmlformats-package.relationships+xml"));
                Empty(x,"Default",("Extension","xml"),("ContentType","application/xml"));
                Empty(x,"Override",("PartName","/xl/workbook.xml"),("ContentType","application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"));
                Empty(x,"Override",("PartName","/xl/styles.xml"),("ContentType","application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"));
                for(var i=1;i<=6;i++)Empty(x,"Override",("PartName",$"/xl/worksheets/sheet{i}.xml"),("ContentType","application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml"));
                x.WriteEndElement();
            });
            Write(zip,"_rels/.rels",x=>{x.WriteStartElement("Relationships","http://schemas.openxmlformats.org/package/2006/relationships");Empty(x,"Relationship",("Id","rId1"),("Type",Rel+"/officeDocument"),("Target","xl/workbook.xml"));x.WriteEndElement();});
            Write(zip,"xl/_rels/workbook.xml.rels",x=>{x.WriteStartElement("Relationships","http://schemas.openxmlformats.org/package/2006/relationships");for(var i=1;i<=6;i++)Empty(x,"Relationship",("Id",$"rId{i}"),("Type",Rel+"/worksheet"),("Target",$"worksheets/sheet{i}.xml"));Empty(x,"Relationship",("Id","rId7"),("Type",Rel+"/styles"),("Target","styles.xml"));x.WriteEndElement();});
            Write(zip,"xl/styles.xml",x=>
            {
                x.WriteStartElement("styleSheet",Main);
                x.WriteStartElement("fonts",Main);x.WriteAttributeString("count","2");
                for(var i=0;i<2;i++){x.WriteStartElement("font",Main);Empty(x,"sz",("val","11"));Empty(x,"name",("val","Calibri"));if(i==1)Empty(x,"b");x.WriteEndElement();}x.WriteEndElement();
                x.WriteStartElement("fills",Main);x.WriteAttributeString("count","2");
                foreach(var pattern in new[]{"none","gray125"}){x.WriteStartElement("fill",Main);Empty(x,"patternFill",("patternType",pattern));x.WriteEndElement();}x.WriteEndElement();
                x.WriteStartElement("borders",Main);x.WriteAttributeString("count","1");Empty(x,"border");x.WriteEndElement();
                x.WriteStartElement("cellStyleXfs",Main);x.WriteAttributeString("count","1");Empty(x,"xf",("numFmtId","0"),("fontId","0"),("fillId","0"),("borderId","0"));x.WriteEndElement();
                x.WriteStartElement("cellXfs",Main);x.WriteAttributeString("count","3");
                for(var i=0;i<3;i++){x.WriteStartElement("xf",Main);x.WriteAttributeString("numFmtId",i==2?"4":"0");x.WriteAttributeString("fontId",i==1?"1":"0");x.WriteAttributeString("fillId","0");x.WriteAttributeString("borderId","0");x.WriteAttributeString("xfId","0");Empty(x,"alignment",("vertical","top"),("wrapText","1"));x.WriteEndElement();}x.WriteEndElement();
                x.WriteStartElement("cellStyles",Main);x.WriteAttributeString("count","1");Empty(x,"cellStyle",("name","Normal"),("xfId","0"),("builtinId","0"));x.WriteEndElement();x.WriteEndElement();
            });
            Write(zip,"xl/workbook.xml",x=>
            {
                x.WriteStartElement("workbook",Main);x.WriteStartElement("sheets",Main);
                for(var i=0;i<6;i++){x.WriteStartElement("sheet",Main);x.WriteAttributeString("name",SheetNames[i]);x.WriteAttributeString("sheetId",(i+1).ToString(CultureInfo.InvariantCulture));x.WriteAttributeString("r","id",Rel,$"rId{i+1}");x.WriteEndElement();}
                x.WriteEndElement();x.WriteEndElement();
            });
            for(var i=0;i<6;i++)
            {
                ct.ThrowIfCancellationRequested();var sheet=i;
                Write(zip,$"xl/worksheets/sheet{i+1}.xml",x=>
                {
                    x.WriteStartElement("worksheet",Main);
                    x.WriteStartElement("sheetViews",Main);x.WriteStartElement("sheetView",Main);x.WriteAttributeString("workbookViewId","0");Empty(x,"pane",("ySplit","3"),("topLeftCell","A4"),("activePane","bottomLeft"),("state","frozen"));x.WriteEndElement();x.WriteEndElement();
                    Empty(x,"sheetFormatPr",("defaultRowHeight","36"));
                    x.WriteStartElement("cols",Main);Empty(x,"col",("min","1"),("max","1"),("width",sheet==0?"46":"24"),("customWidth","1"));Empty(x,"col",("min","2"),("max","16384"),("width","24"),("customWidth","1"));x.WriteEndElement();
                    x.WriteStartElement("sheetData",Main);var row=1;
                    Row(x,row++,[report.BranchName,report.Month.Name,report.Month.Id,report.Month.StartDate,report.Month.EndDate,"Revision",report.Revision,"Snapshot UTC",report.SnapshotAt],true);
                    switch(sheet)
                    {
                        case 0:
                            foreach(var p in typeof(MonthlySummary).GetProperties())Row(x,row++,[p.Name,p.GetValue(report.Summary)]);
                            Table(x,ref row,"Actual ordinary expenses by category",report.ExpenseCategories,ct);
                            Table(x,ref row,"Informational maintenance; excluded from costs/cashflow",report.Maintenance,ct);
                            foreach(var note in report.Limitations)Row(x,row++,["Disclosure",note]);break;
                        case 1:Table(x,ref row,"Ledger movements: count each row once; linked evidence is not extra money",report.Transactions,ct);break;
                        case 2:
                            var performance=report.SalesPerformance.SelectMany(p=>p.Employees.SelectMany(e=>new[]{e.NewClients,e.Renewals}.Select(c=>new{PeriodId=p.Period.Id,p.Period.Name,p.Period.StartDate,p.Period.EndDate,p.Finalized,e.EmployeeId,e.EmployeeName,c.Category,c.GrossCollections,c.Refunds,c.EligibleCollections,c.Configured,c.Target,c.TargetReached,c.BelowRate,c.ReachedRate,c.AppliedRate,c.Commission,e.HouseCollections,e.ExcludedCollections}))).ToArray();
                            Table(x,ref row,"Original commission evaluation periods: progress is provisional unless finalized",performance,ct);
                            var totals=report.SalesPerformance.Select(p=>new{p.Period.Id,p.Period.Name,p.Finalized,p.GrossCollections,p.Refunds,p.HouseCollections,p.ExcludedCollections,p.TotalCommission}).ToArray();
                            Table(x,ref row,"Period totals (not additional monthly collections)",totals,ct);
                            Table(x,ref row,"Commercial collection attribution evidence",report.SalesSources,ct);
                            Table(x,ref row,"Original newest-first refund allocations",report.SalesRefundAdjustments,ct);break;
                        case 3:
                            Table(x,ref row,"Prorated BASIC salary; advances offset the same branch once",report.Salaries,ct);
                            Table(x,ref row,"Recognized entitlements and outstanding as of closing cutoff",report.Payroll,ct);
                            Table(x,ref row,"Owner-declared external payments: no drawer/ledger posting",report.EmployeePayments,ct);
                            Table(x,ref row,"Actual advance payouts (already in ledger)",report.Advances,ct);break;
                        case 4:
                            Table(x,ref row,"Coach earnings, actual payouts and outstanding; Classes compensation NotConfigured",report.Coaches,ct);
                            Table(x,ref row,"Immutable earnings: actual sessions versus unused expiry recognition",report.CoachEarnings,ct);
                            Table(x,ref row,"Coach payouts (already included in Expense ledger debits)",report.CoachPayments,ct);
                            Table(x,ref row,"Class occurrences and valid actual attendance",report.Classes,ct);break;
                        case 5:
                            Table(x,ref row,"One shared branch drawer at original closing cutoff",new[]{report.Drawer},ct);
                            Table(x,ref row,"Auditable cash counts; discrepancies do not post money",report.CashCounts,ct);
                            Table(x,ref row,"Employee shifts: do not add their shared drawer balances/count differences",report.Shifts,ct);break;
                    }
                    x.WriteEndElement();x.WriteEndElement();
                });
            }
        }
        if(buffer.Length>32*1024*1024)throw new BusinessRuleException("Workbook exceeds the 32 MiB closing artifact limit.");
        return buffer.ToArray();
    }
    private static void Table<T>(XmlWriter x,ref int row,string title,IReadOnlyList<T> items,CancellationToken ct)
    {
        Row(x,row++,[title],true);var properties=typeof(T).GetProperties(BindingFlags.Public|BindingFlags.Instance);
        var cells=items.Select(item=>properties.Select(p=>CellValue(p.GetValue(item))).ToArray()).ToArray();
        // Preserve long evidence without truncation: consecutive numbered columns.
        var widths=properties.Select((_,i)=>Math.Max(1,cells.Select(c=>c[i] is string s?Chunks(s).Count:1).DefaultIfEmpty(1).Max())).ToArray();
        if(widths.Sum()>16384)throw new BusinessRuleException("Evidence exceeds Excel column limits.");
        Row(x,row++,properties.SelectMany((p,i)=>Enumerable.Range(1,widths[i]).Select(n=>(object?)(widths[i]==1?p.Name:$"{p.Name}_{n}"))).ToArray(),true);
        foreach(var values in cells)
        {
            ct.ThrowIfCancellationRequested();var expanded=new List<object?>();
            for(var i=0;i<values.Length;i++)
            {
                var chunks=values[i] is string s?Chunks(s):null;
                for(var n=0;n<widths[i];n++)expanded.Add(chunks!=null?n<chunks.Count?chunks[n]:"":n==0?values[i]:null);
            }
            Row(x,row++,expanded.ToArray());
        }
        Row(x,row++,[]);
    }
    private static object? CellValue(object? v)=>v switch
    { null=>null,DateTimeOffset time=>time.ToUniversalTime().ToString("O",CultureInfo.InvariantCulture),DateOnly date=>date.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture),Guid id=>id.ToString(),Enum e=>e.ToString(),bool b=>b?"true":"false",string or decimal or int or long=>v,_=>JsonSerializer.Serialize(v) };
    private static void Row(XmlWriter x,int number,object?[] values,bool heading=false)
    {
        if(number>1048576)throw new BusinessRuleException("Workbook exceeds Excel row limits.");
        x.WriteStartElement("row",Main);x.WriteAttributeString("r",number.ToString(CultureInfo.InvariantCulture));
        for(var i=0;i<values.Length;i++)
        {
            var value=CellValue(values[i]);x.WriteStartElement("c",Main);x.WriteAttributeString("r",Column(i+1)+number);
            x.WriteAttributeString("s",heading?"1":value is decimal?"2":"0");
            if(value is decimal or int or long){x.WriteStartElement("v",Main);x.WriteString(Convert.ToString(value,CultureInfo.InvariantCulture));x.WriteEndElement();}
            else{ x.WriteAttributeString("t","inlineStr");x.WriteStartElement("is",Main);x.WriteStartElement("t",Main);x.WriteAttributeString("xml","space",null,"preserve");x.WriteString(XmlSafe(value?.ToString()??""));x.WriteEndElement();x.WriteEndElement(); }
            x.WriteEndElement();
        }
        x.WriteEndElement();
    }
    private static IReadOnlyList<string> Chunks(string text)
    {
        var result=new List<string>();
        for(var start=0;start<text.Length;){var length=Math.Min(30000,text.Length-start);if(start+length<text.Length&&char.IsHighSurrogate(text[start+length-1]))length--;result.Add(text.Substring(start,length));start+=length;}
        return result;
    }
    private static string XmlSafe(string text)
    {
        var result=new StringBuilder(text.Length);
        for(var i=0;i<text.Length;i++)
        {
            if(char.IsHighSurrogate(text[i])&&i+1<text.Length&&char.IsLowSurrogate(text[i+1])){result.Append(text[i]);result.Append(text[++i]);}
            else if(XmlConvert.IsXmlChar(text[i]))result.Append(text[i]);
        }
        return result.ToString();
    }
    private static string Column(int index){var s="";while(index>0){index--;s=(char)('A'+index%26)+s;index/=26;}return s;}
    private static void Empty(XmlWriter x,string name,params(string Key,string Value)[] attrs){x.WriteStartElement(name);foreach(var (k,v) in attrs)x.WriteAttributeString(k,v);x.WriteEndElement();}
    private static void Write(ZipArchive z,string path,Action<XmlWriter> write)
    {
        var entry=z.CreateEntry(path,CompressionLevel.Fastest);entry.LastWriteTime=new DateTimeOffset(1980,1,1,0,0,0,TimeSpan.Zero);
        using var stream=entry.Open();using var xml=XmlWriter.Create(stream,new XmlWriterSettings{Encoding=new UTF8Encoding(false),CloseOutput=false,CheckCharacters=true});
        xml.WriteStartDocument();write(xml);xml.WriteEndDocument();
    }
}
