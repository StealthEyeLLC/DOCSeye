using System.IO.Compression;
using System.Text;
using DOCSeye.Providers;

namespace DOCSeye.Build002;

public sealed record O001Artifacts(string Extended,string Standard,string ExtendedSha256,string StandardSha256);

public static class O001Generator
{
    public static O001Artifacts Generate(string root)
    {
        Directory.CreateDirectory(root);
        string extended=Path.Combine(root,"O-001.odt"),standard=Path.Combine(root,"O-001-standard.odt");
        Write(extended,true);Write(standard,false);
        return new(extended,standard,OdfPackage.HashFile(extended),OdfPackage.HashFile(standard));
    }

    private static void Write(string path,bool extended)
    {
        var entries=new SortedDictionary<string,(byte[] bytes,CompressionLevel level)>(StringComparer.Ordinal)
        {
            ["META-INF/manifest.xml"]=(Utf8(Manifest(extended)),CompressionLevel.Optimal),
            ["content.xml"]=(Utf8(Content(extended)),CompressionLevel.Optimal),
            ["meta.xml"]=(Utf8(Meta()),CompressionLevel.Optimal),
            ["settings.xml"]=(Utf8(Settings()),CompressionLevel.Optimal),
            ["styles.xml"]=(Utf8(Styles()),CompressionLevel.Optimal),
            ["Pictures/fixture.svg"]=(Utf8(Svg()),CompressionLevel.Optimal)
        };
        if(extended)
        {
            entries["Basic/Standard/Module1.xml"]=(Utf8(BasicModule()),CompressionLevel.Optimal);
            entries["Basic/Standard/script-lb.xml"]=(Utf8(BasicLibrary()),CompressionLevel.Optimal);
            entries["Basic/script-lc.xml"]=(Utf8(BasicLibraries()),CompressionLevel.Optimal);
            entries["Scripts/probe.txt"]=(Utf8("synthetic active-content sentinel; must remain inert\n"),CompressionLevel.Optimal);
        }
        OdfPackage.WritePackage(path,entries);
    }

    private static string Content(bool extended)
    {
        string extNs=extended?" xmlns:seopt=\"urn:stealtheye:odf:optional:1\" xmlns:sereq=\"urn:stealtheye:odf:required:1\" xmlns:loext=\"urn:org:documentfoundation:names:experimental:office:xmlns:loext:1.0\" xmlns:script=\"urn:oasis:names:tc:opendocument:xmlns:script:1.0\" xmlns:dom=\"http://www.w3.org/2001/xml-events\"":"";
        string scripts=extended?"<office:scripts><office:script script:language=\"ooo:script\"><script:event-listener script:language=\"ooo:script\" script:event-name=\"dom:load\" xlink:href=\"vnd.sun.star.script:Standard.Module1.Main?language=Basic&amp;location=document\"/></office:script></office:scripts>":"";
        string unknown=extended?"<seopt:opaque seopt:token=\"preserve-me\">optional opaque meaning</seopt:opaque><sereq:required-meaning sereq:token=\"must-block\">required authored meaning</sereq:required-meaning>":"";
        string resolved=extended?" loext:resolved=\"true\"":"";
        string external=extended?"<text:p>External inert link: <text:a xlink:type=\"simple\" xlink:href=\"http://127.0.0.1:49152/odf-probe\">must not fetch</text:a>.</text:p>":"";
        return $"""<?xml version="1.0" encoding="UTF-8"?>
<office:document-content xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0" xmlns:table="urn:oasis:names:tc:opendocument:xmlns:table:1.0" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:fo-compatible:1.0" xmlns:draw="urn:oasis:names:tc:opendocument:xmlns:drawing:1.0" xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0" xmlns:xlink="http://www.w3.org/1999/xlink" xmlns:dc="http://purl.org/dc/elements/1.1/" xmlns:math="http://www.w3.org/1998/Math/MathML" xmlns:meta="urn:oasis:names:tc:opendocument:xmlns:meta:1.0"{extNs} office:version="1.4">
{scripts}
<office:font-face-decls/>
<office:automatic-styles>
 <style:style style:name="Pauto" style:family="paragraph"><style:paragraph-properties fo:margin-left="0.4in"/><style:text-properties fo:font-weight="bold"/></style:style>
 <style:style style:name="Tauto" style:family="text"><style:text-properties fo:font-style="italic"/></style:style>
</office:automatic-styles>
<office:body><office:text>
<text:tracked-changes>
 <text:changed-region text:id="ct-ins"><text:insertion><office:change-info><dc:creator>Alice</dc:creator><dc:date>2026-08-12T00:00:00Z</dc:date></office:change-info></text:insertion></text:changed-region>
 <text:changed-region text:id="ct-del"><text:deletion><office:change-info><dc:creator>Bob</dc:creator><dc:date>2026-08-12T00:00:01Z</dc:date></office:change-info><text:p>deleted text</text:p></text:deletion></text:changed-region>
 <text:changed-region text:id="ct-fmt"><text:format-change><office:change-info><dc:creator>Carol</dc:creator><dc:date>2026-08-12T00:00:02Z</dc:date></office:change-info></text:format-change></text:changed-region>
</text:tracked-changes>
<text:sequence-decls><text:sequence-decl text:display-outline-level="0" text:name="Figure"/></text:sequence-decls>
<text:section text:name="Front" text:style-name="Sect1">
 <text:h text:outline-level="1" text:style-name="Heading_20_1">Duplicate Heading</text:h>
 <text:p text:style-name="Body">Duplicate paragraph</text:p>
 <text:p text:style-name="Body">Duplicate paragraph</text:p>
 <text:h text:outline-level="1" text:style-name="Heading_20_1">Duplicate Heading</text:h>
 <text:p text:style-name="Pauto">Latin café; العربية שלום; 漢字; देवनागरी; e&#x301; / é; 👩‍💻 family 👨‍👩‍👧‍👦.</text:p>
 <text:p>Distinct anchor text <text:bookmark text:name="PointMark"/> repeated phrase repeated phrase.</text:p>
 <text:p><text:bookmark-start text:name="RangeMark"/>same quote same quote<text:bookmark-end text:name="RangeMark"/> and <text:reference-mark-start text:name="RefRange"/>same quote same quote<text:reference-mark-end text:name="RefRange"/>.</text:p>
 <text:p>Cross reference: <text:bookmark-ref text:ref-name="RangeMark" text:reference-format="text">same quote same quote</text:bookmark-ref>; broken: <text:bookmark-ref text:ref-name="MissingMark" text:reference-format="text">missing</text:bookmark-ref>.</text:p>
 <text:p>Review <text:change-start text:change-id="ct-ins"/>inserted words<text:change-end text:change-id="ct-ins"/> and <text:change-start text:change-id="ct-fmt"/><text:span text:style-name="Tauto">format change</text:span><text:change-end text:change-id="ct-fmt"/>.</text:p>
 <text:p>Comment target<office:annotation office:name="PointComment"{resolved}><dc:creator>Dana</dc:creator><dc:date>2026-08-12T00:01:00Z</dc:date><text:p>point comment</text:p></office:annotation> continues.</text:p>
 <text:p><office:annotation office:name="RangeComment"><dc:creator>Eve</dc:creator><dc:date>2026-08-12T00:02:00Z</dc:date><text:p>range comment same quote</text:p></office:annotation>same quote same quote<office:annotation-end office:name="RangeComment"/> duplicate: same quote same quote.</text:p>
 <text:list text:style-name="L1"><text:list-item><text:p>Item one</text:p><text:list text:style-name="L1"><text:list-item><text:p>Nested item</text:p></text:list></text:list-item><text:list-item text:start-value="5"><text:p>Restart five</text:p></text:list-item></text:list>
 <table:table table:name="T1"><table:table-header-rows><table:table-row><table:table-cell office:value-type="string"><text:p>Header A</text:p></table:table-cell><table:table-cell office:value-type="string"><text:p>Header B</text:p></table:table-cell></table:table-row></table:table-header-rows><table:table-row><table:table-cell office:value-type="string" table:number-columns-spanned="2" table:formula="of:=1+1"><text:p>Merged formula cell</text:p><text:list text:style-name="L1"><text:list-item><text:p>cell list</text:p></text:list-item></text:list></table:table-cell><table:covered-table-cell/></table:table-row></table:table>
 <text:p>Fields: page <text:page-number text:select-page="current">1</text:page-number>; date <text:date text:fixed="true" text:date-value="2026-08-12">2026-08-12</text:date>; sequence <text:sequence text:name="Figure" text:formula="ooow:Figure+1">1</text:sequence>.</text:p>
 <text:p>Footnote<text:note text:id="ftn1" text:note-class="footnote"><text:note-citation>1</text:note-citation><text:note-body><text:p>Footnote text</text:p></text:note-body></text:note> endnote<text:note text:id="end1" text:note-class="endnote"><text:note-citation>i</text:note-citation><text:note-body><text:p>Endnote text</text:p></text:note-body></text:note>.</text:p>
 <text:p>Math: <math:math><math:semantics><math:mrow><math:mi>x</math:mi><math:mo>+</math:mo><math:mn>1</math:mn></math:mrow></math:semantics></math:math></text:p>
 <draw:frame draw:name="Figure1" text:anchor-type="paragraph" svg:width="1in" svg:height="1in"><draw:image xlink:href="Pictures/fixture.svg" xlink:type="simple" xlink:show="embed" xlink:actuate="onLoad"/><svg:desc>Synthetic accessible SVG figure</svg:desc></draw:frame>
 <draw:frame draw:name="TextBox1" text:anchor-type="paragraph" svg:width="2in" svg:height="0.6in"><draw:text-box><text:p>Frame text box</text:p></draw:text-box></draw:frame>
 {external}
 <text:p>{unknown}</text:p>
</text:section>
<text:section text:name="Body2"><text:h text:outline-level="2">Second Section</text:h><text:p>Copied-looking structure Duplicate paragraph.</text:p></text:section>
</office:text></office:body></office:document-content>""";
    }

    private static string Styles()=>"""<?xml version="1.0" encoding="UTF-8"?>
<office:document-styles xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0" xmlns:style="urn:oasis:names:tc:opendocument:xmlns:style:1.0" xmlns:text="urn:oasis:names:tc:opendocument:xmlns:text:1.0" xmlns:fo="urn:oasis:names:tc:opendocument:xmlns:fo-compatible:1.0" xmlns:svg="urn:oasis:names:tc:opendocument:xmlns:svg-compatible:1.0" office:version="1.4">
<office:font-face-decls/>
<office:styles>
 <style:default-style style:family="paragraph"><style:paragraph-properties fo:orphans="2" fo:widows="2"/></style:default-style>
 <style:style style:name="Body" style:display-name="Body" style:family="paragraph"/>
 <style:style style:name="Heading_20_1" style:display-name="Heading 1" style:family="paragraph" style:parent-style-name="Body"><style:text-properties fo:font-size="16pt" fo:font-weight="bold"/></style:style>
 <style:style style:name="Emphasis" style:family="text"><style:text-properties fo:font-style="italic"/></style:style>
 <text:list-style style:name="L1"><text:list-level-style-number text:level="1" style:num-format="1" text:start-value="1"><style:list-level-properties text:space-before="0.25in" text:min-label-width="0.25in"/></text:list-level-style-number><text:list-level-style-bullet text:level="2" text:bullet-char="•"/></text:list-style>
</office:styles>
<office:automatic-styles><style:page-layout style:name="pm1"><style:page-layout-properties fo:page-width="8.5in" fo:page-height="11in" style:print-orientation="portrait" fo:margin="0.7in"/><style:header-style/><style:footer-style/></style:page-layout><style:style style:name="Sect1" style:family="section"><style:section-properties text:dont-balance-text-columns="false"><style:columns fo:column-count="2" fo:column-gap="0.2in"/></style:section-properties></style:style></office:automatic-styles>
<office:master-styles><style:master-page style:name="Standard" style:page-layout-name="pm1"><style:header><text:p>Header text</text:p></style:header><style:footer><text:p>Footer <text:page-number>1</text:page-number></text:p></style:footer></style:master-page></office:master-styles>
</office:document-styles>""";

    private static string Meta()=>"""<?xml version="1.0" encoding="UTF-8"?>
<office:document-meta xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0" xmlns:meta="urn:oasis:names:tc:opendocument:xmlns:meta:1.0" xmlns:dc="http://purl.org/dc/elements/1.1/" office:version="1.4"><office:meta><meta:generator>DOCSeye O-001 deterministic generator</meta:generator><dc:title>O-001 Provider Neutrality</dc:title><dc:language>en-US</dc:language><meta:user-defined meta:name="Synthetic">true</meta:user-defined></office:meta></office:document-meta>""";
    private static string Settings()=>"""<?xml version="1.0" encoding="UTF-8"?>
<office:document-settings xmlns:office="urn:oasis:names:tc:opendocument:xmlns:office:1.0" office:version="1.4"><office:settings/></office:document-settings>""";
    private static string Svg()=>"""<svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" viewBox="0 0 64 64"><title>Synthetic square</title><rect x="8" y="8" width="48" height="48" fill="#808080"/></svg>""";
    private static string BasicModule()=>"""<?xml version="1.0" encoding="UTF-8"?><script:module xmlns:script="http://openoffice.org/2000/script" script:name="Module1" script:language="StarBasic">Sub Main
ThisComponent.DocumentProperties.Title = "MACRO_EXECUTED"
End Sub</script:module>""";
    private static string BasicLibrary()=>"""<?xml version="1.0" encoding="UTF-8"?><library:library xmlns:library="http://openoffice.org/2000/library" library:name="Standard" library:readonly="false" library:passwordprotected="false"><library:element library:name="Module1"/></library:library>""";
    private static string BasicLibraries()=>"""<?xml version="1.0" encoding="UTF-8"?><library:libraries xmlns:library="http://openoffice.org/2000/library"><library:library library:name="Standard" library:link="false"/></library:libraries>""";
    private static string Manifest(bool extended)
    {
        string extra=extended?"<manifest:file-entry manifest:full-path=\"Basic/\" manifest:media-type=\"application/vnd.sun.star.basic-library\"/><manifest:file-entry manifest:full-path=\"Basic/Standard/\" manifest:media-type=\"application/vnd.sun.star.basic-library\"/><manifest:file-entry manifest:full-path=\"Basic/Standard/Module1.xml\" manifest:media-type=\"text/xml\"/><manifest:file-entry manifest:full-path=\"Basic/Standard/script-lb.xml\" manifest:media-type=\"text/xml\"/><manifest:file-entry manifest:full-path=\"Basic/script-lc.xml\" manifest:media-type=\"text/xml\"/><manifest:file-entry manifest:full-path=\"Scripts/probe.txt\" manifest:media-type=\"text/plain\"/>":"";
        return $"""<?xml version="1.0" encoding="UTF-8"?><manifest:manifest xmlns:manifest="urn:oasis:names:tc:opendocument:xmlns:manifest:1.0" manifest:version="1.4"><manifest:file-entry manifest:full-path="/" manifest:media-type="application/vnd.oasis.opendocument.text" manifest:version="1.4"/><manifest:file-entry manifest:full-path="content.xml" manifest:media-type="text/xml"/><manifest:file-entry manifest:full-path="styles.xml" manifest:media-type="text/xml"/><manifest:file-entry manifest:full-path="meta.xml" manifest:media-type="text/xml"/><manifest:file-entry manifest:full-path="settings.xml" manifest:media-type="text/xml"/><manifest:file-entry manifest:full-path="Pictures/fixture.svg" manifest:media-type="image/svg+xml"/>{extra}</manifest:manifest>""";
    }
    private static byte[] Utf8(string s)=>new UTF8Encoding(false).GetBytes(s.Replace("\r\n","\n",StringComparison.Ordinal));
}
