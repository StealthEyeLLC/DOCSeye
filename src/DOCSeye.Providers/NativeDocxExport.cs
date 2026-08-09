using System.Security.Cryptography;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using W = DocumentFormat.OpenXml.Wordprocessing;
using DOCSeye.Core;

namespace DOCSeye.Providers;

public sealed record NativeDocxExportPlan(ExportOutcome Outcome,string Classification,IReadOnlyList<DocxFeatureClassification> Features,IReadOnlyList<string> DeclaredLosses);
public sealed record NativeDocxExportResult(ExportOutcome Outcome,string Classification,string Path,string Sha256,Guid SemanticRevisionId,string SemanticRoot,IReadOnlyList<string> DeclaredLosses);

public static class NativeDocxExport
{
    public static NativeDocxExportPlan Plan(SemanticState state)
    {
        var features=new List<DocxFeatureClassification>();var losses=new SortedSet<string>(StringComparer.Ordinal);
        foreach(var group in state.Objects.Values.Where(o=>!o.Retired).GroupBy(o=>o.Type).OrderBy(g=>g.Key,StringComparer.Ordinal))
        {
            string cls=group.Key switch
            {
                "text_block"=>"supported",
                "table" or "table_cell" or "list" or "list_item"=>"supported_with_transform",
                "link" or "note" or "note_reference" or "figure" or "math" or "field"=>"fallback_only",
                _=>"fallback_only"
            };
            if(cls=="fallback_only")losses.Add(group.Key+": represented textually or omitted from native DOCX semantics");
            features.Add(new(group.Key,cls,group.First().Id,null,$"count={group.Count()}"));
        }
        return new(ExportOutcome.TranslatedWithDeclaredLoss,"translated_with_declared_loss",features,losses.ToArray());
    }

    public static NativeDocxExportResult Export(SemanticState state,RevisionHead head,string outputPath)
    {
        if(state.RevisionId!=head.RevisionId||!state.ComputeRoot().SequenceEqual(head.SemanticRoot))throw new InvalidOperationException("semantic_head_mismatch");
        var plan=Plan(state);Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(outputPath))!);if(File.Exists(outputPath))File.Delete(outputPath);
        using(var doc=WordprocessingDocument.Create(outputPath,DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
        {
            doc.PackageProperties.Title="DOCSeye native semantic export";
            doc.PackageProperties.Subject=$"revision={Ids.Lower(head.RevisionId)} root={Convert.ToHexString(head.SemanticRoot).ToLowerInvariant()}";
            doc.PackageProperties.Creator="DOCSeye";
            var main=doc.AddMainDocumentPart();main.Document=new W.Document();var body=new W.Body();main.Document.Append(body);
            var stylesPart=main.AddNewPart<StyleDefinitionsPart>();var styles=new W.Styles();
            var normal=new W.Style{Type=W.StyleValues.Paragraph,StyleId="Normal",Default=true};normal.Append(new W.Name{Val="Normal"});styles.Append(normal);
            for(int level=1;level<=6;level++){var heading=new W.Style{Type=W.StyleValues.Paragraph,StyleId="Heading"+level};heading.Append(new W.Name{Val="Heading "+level});heading.Append(new W.BasedOn{Val="Normal"});styles.Append(heading);}stylesPart.Styles=styles;stylesPart.Styles.Save();
            foreach(var obj in state.Objects.Values.Where(o=>!o.Retired).OrderBy(o=>LogicalPath(state,o),StringComparer.Ordinal))
            {
                if(obj.Type=="table_cell"||obj.Type=="table_row"||obj.Type=="table_column"||IsNestedText(state,obj))continue;
                if(obj.Type=="table"){body.Append(BuildTable(state,obj));continue;}
                string? text=ObjectText(state,obj);if(text is null)continue;
                var p=new W.Paragraph();if(obj.Role=="heading")p.Append(new W.ParagraphProperties(new W.ParagraphStyleId{Val="Heading"+Math.Clamp(Convert.ToInt32(obj.Data.GetValueOrDefault("heading_level")??1),1,6)}));
                p.Append(new W.Run(new W.Text(text){Space=DocumentFormat.OpenXml.SpaceProcessingModeValues.Preserve}));body.Append(p);
            }
            body.Append(new W.SectionProperties(new W.PageSize{Width=12240,Height=15840},new W.PageMargin{Top=1080,Right=1080,Bottom=1080,Left=1080,Header=720,Footer=720,Gutter=0}));
            main.Document.Save();
        }
        var validation=DocxInterop.Validate(outputPath);if(!validation.OpcValid||!validation.SchemaValid)throw new InvalidOperationException("native_docx_validation_failed:"+string.Join(";",validation.Errors.Take(8)));
        return new(plan.Outcome,plan.Classification,outputPath,Hash(outputPath),head.RevisionId,Convert.ToHexString(head.SemanticRoot).ToLowerInvariant(),plan.DeclaredLosses);
    }

    private static W.Table BuildTable(SemanticState state,SemanticObject table)
    {
        var result=new W.Table(new W.TableProperties(new W.TableBorders(new W.TopBorder{Val=W.BorderValues.Single,Size=4},new W.LeftBorder{Val=W.BorderValues.Single,Size=4},new W.BottomBorder{Val=W.BorderValues.Single,Size=4},new W.RightBorder{Val=W.BorderValues.Single,Size=4},new W.InsideHorizontalBorder{Val=W.BorderValues.Single,Size=4},new W.InsideVerticalBorder{Val=W.BorderValues.Single,Size=4})));
        int columnCount=Math.Max(1,Convert.ToInt32(table.Data.GetValueOrDefault("columns")??1));var grid=new W.TableGrid();for(int i=0;i<columnCount;i++)grid.Append(new W.GridColumn{Width="2400"});result.Append(grid);
        var cells=state.Objects.Values.Where(o=>!o.Retired&&o.Type=="table_cell"&&o.ParentId==table.Id).OrderBy(o=>Convert.ToInt32(o.Data.GetValueOrDefault("row")??0)).ThenBy(o=>Convert.ToInt32(o.Data.GetValueOrDefault("column")??0)).GroupBy(o=>Convert.ToInt32(o.Data.GetValueOrDefault("row")??0));
        foreach(var row in cells){var tr=new W.TableRow();foreach(var c in row){var tc=new W.TableCell();var props=new W.TableCellProperties();int cs=Convert.ToInt32(c.Data.GetValueOrDefault("column_span")??1);int rs=Convert.ToInt32(c.Data.GetValueOrDefault("row_span")??1);if(cs>1)props.Append(new W.GridSpan{Val=cs});if(rs>1)props.Append(new W.VerticalMerge{Val=W.MergedCellValues.Restart});tc.Append(props);tc.Append(new W.Paragraph(new W.Run(new W.Text(c.Data.GetValueOrDefault("text") as string??string.Empty))));tr.Append(tc);}result.Append(tr);}
        return result;
    }
    private static string? ObjectText(SemanticState state,SemanticObject o)=>o.Type switch
    {
        "text_block"=>o.Data.GetValueOrDefault("text") as string??string.Empty,
        "list_item"=>"List item: "+string.Join(" ",state.Objects.Values.Where(x=>!x.Retired&&x.ParentId==o.Id&&x.Type=="text_block").OrderBy(x=>x.Order).Select(x=>x.Data.GetValueOrDefault("text") as string??string.Empty)),
        "link"=>"Link: "+(o.Data.GetValueOrDefault("label") as string??"")+" "+(o.Data.GetValueOrDefault("href") as string??""),
        "note"=>"Note: "+string.Join(" ",Descendants(state,o.Id).Where(x=>x.Type=="text_block").Select(x=>x.Data.GetValueOrDefault("text") as string??string.Empty)),
        "figure"=>"Figure: "+(o.Data.GetValueOrDefault("caption") as string??"")+" [alt: "+(o.Data.GetValueOrDefault("alt_text") as string??"")+"]",
        "math"=>"Math: "+(o.Data.GetValueOrDefault("presentation_mathml") as string??""),
        "field"=>"Field "+(o.Data.GetValueOrDefault("class") as string??"field")+": "+(o.Data.GetValueOrDefault("cached_result")?.ToString()??"stale"),
        "page_break"=>"[Page break]",
        _=>null
    };
    private static bool IsNestedText(SemanticState s,SemanticObject o)=>o.Type=="text_block"&&o.ParentId is Guid p&&s.Objects.TryGetValue(p,out var parent)&&(parent.Type=="list_item"||parent.Type=="flow"&&parent.ParentId is Guid pp&&s.Objects.TryGetValue(pp,out var grand)&&grand.Type=="note");
    private static IEnumerable<SemanticObject> Descendants(SemanticState s,Guid root){var q=new Queue<Guid>();q.Enqueue(root);while(q.Count>0){Guid p=q.Dequeue();foreach(var c in s.Objects.Values.Where(o=>!o.Retired&&o.ParentId==p).OrderBy(o=>o.Order)){yield return c;q.Enqueue(c.Id);}}}
    private static string LogicalPath(SemanticState s,SemanticObject o){var parts=new Stack<string>();SemanticObject? cur=o;while(cur is not null){parts.Push(Convert.ToHexString(cur.Order.Bytes()));cur=cur.ParentId is Guid p&&s.Objects.TryGetValue(p,out var parent)?parent:null;}return string.Join('/',parts)+":"+Ids.Lower(o.Id);}
    private static string Hash(string path){using var fs=File.OpenRead(path);return Convert.ToHexString(SHA256.HashData(fs)).ToLowerInvariant();}
}
