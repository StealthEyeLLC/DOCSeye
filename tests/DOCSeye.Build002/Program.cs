using System.Net;
using System.Net.Sockets;
using DOCSeye.Core;
using DOCSeye.Providers;
using DOCSeye.Storage.Sqlite;

namespace DOCSeye.Build002;

internal static class Program
{
    private static void Require(bool ok,string name){if(!ok)throw new InvalidOperationException("FAIL:"+name);Console.WriteLine("PASS "+name);}

    public static int Main(string[] args)
    {
        string root=args.Length>0?Path.GetFullPath(args[0]):Path.Combine(Path.GetTempPath(),"docseye-build002-preflight");
        string runtime=args.Length>1?Path.GetFullPath(args[1]):@"C:\StealthEyeLLC\DOCSeye-build002-runtime";
        string repo=FindRepo();
        Reset(root);Directory.CreateDirectory(root);
        var a=O001Generator.Generate(Path.Combine(root,"fixture-a"));var b=O001Generator.Generate(Path.Combine(root,"fixture-b"));
        Require(a.ExtendedSha256==b.ExtendedSha256,"O001 extended deterministic digest");
        Require(a.StandardSha256==b.StandardSha256,"O001 standard deterministic digest");
        var ext=OdfPackage.Inspect(a.Extended);var std=OdfPackage.Inspect(a.Standard);
        Require(ext.Valid&&std.Valid,"secure package preflight");
        Require(ext.Version=="1.4"&&std.Version=="1.4","ODF 1.4 classified");
        Require(ext.Profile=="extended","extended profile classified");
        Require(std.Profile=="standard","standard profile classified");
        Require(ext.ActiveContentMarkers>0,"active content detected before provider open");
        Require(ext.ExternalReferences.Count==1,"external reference detected before provider open");
        Require(ext.Entries.First().Name=="mimetype","mimetype first");

        var imported=OdfSemanticInterop.Import(a.Extended);
        Require(imported.State.Mode==AuthorityMode.ConvertedImported,"import authority transition");
        Require(imported.State.Objects.Keys.All(Ids.IsV4),"native identity v4");
        var dup=imported.State.Objects.Values.Where(x=>x.Type=="paragraph"&&Equals(x.Data.GetValueOrDefault("text"),"Duplicate paragraph")).ToArray();
        Require(dup.Length==2&&dup[0].Id!=dup[1].Id,"duplicate paragraph no rebound");
        Require(imported.State.Extensions.Values.Any(x=>x.Required),"required unknown preserved");
        Require(imported.State.Extensions.Values.Any(x=>!x.Required),"optional unknown preserved");
        Require(OdfSemanticInterop.Plan(imported.State).Outcome==ExportOutcome.Blocked,"required unknown blocks unsafe general export");
        Require(imported.State.SourceCapsules.Values.Single().DigestValid,"source capsule exact evidence");

        var stdImport=OdfSemanticInterop.Import(a.Standard);
        string db=Path.Combine(root,"standard.dnd");using var store=DndStore.Create(db,stdImport.State);var head=store.ReadHead();
        string exact=Path.Combine(root,"exact-source-reuse.odt");OdfSemanticInterop.ExactSourceReuse(stdImport.State,head,a.StandardSha256,exact);
        Require(OdfPackage.HashFile(exact)==a.StandardSha256,"exact_source_reuse byte exact");
        string direct=Path.Combine(root,"direct.odt");var directResult=OdfSemanticInterop.Export(stdImport.State,head,direct);
        Require(directResult.Outcome==ExportOutcome.TranslatedConformant,"direct export classified conformant candidate");
        Require(OdfPackage.Inspect(direct).Valid,"direct export package preflight");

        string odf=Path.Combine(runtime,"odf14"),jing=Path.Combine(runtime,"jing","dist","jing-20241231","bin","jing.jar");
        string java=ResolveJava(repo);string validation=Path.Combine(root,"validation");
        foreach(var r in ValidateAll(a.Standard,odf,jing,java,validation))Require(r.Valid,"standard fixture "+r.Layer);
        foreach(var r in ValidateAll(direct,odf,jing,java,Path.Combine(root,"validation-direct")))Require(r.Valid,"direct export "+r.Layer);

        string probe=Path.Combine(repo,"tools","libreoffice","libreoffice_writer_probe.py");string soffice=@"C:\Program Files\LibreOffice\program\soffice.exe";string profile=Path.Combine(root,"lo-profile");
        using var listener=new TcpListener(IPAddress.Loopback,49152);listener.Start(4);
        using(var lo=new LibreOfficeWriterProvider(soffice,probe,profile))
        {
            var p=lo.Start();Require(p.ProviderProcessIds.Count>0,"real LibreOffice process started");Require(p.ExtensionRoots.Count==0,"isolated profile has no user extensions");
            var observed=lo.Observe(a.Extended);Require(observed.Ok,"real Writer UNO observe O001");
            Require(observed.Payload.GetProperty("document").GetProperty("title").GetString()!="MACRO_EXECUTED","macro event did not execute");
            Thread.Sleep(250);Require(!listener.Pending(),"automatic external fetch zero");
            var sem=observed.Payload.GetProperty("writer_semantics");Require(sem.GetProperty("paragraphs").GetInt32()>8,"Writer semantic paragraphs observed");Require(sem.GetProperty("text_tables").GetInt32()>=1,"Writer semantic table observed");Require(sem.GetProperty("bookmarks").GetInt32()>=1,"Writer bookmarks observed");
            var unsaved=lo.UnsavedProbe(a.Standard);Require(unsaved.Ok&&unsaved.Payload.GetProperty("unsaved_state_after_mutation").GetBoolean(),"unsaved Writer state explicit");Require(unsaved.Payload.GetProperty("input_sha256_after_unsaved_mutation").GetString()==a.StandardSha256,"unsaved Writer state non-native/source unchanged");
            string resaved=Path.Combine(root,"writer-resaved.odt");var save=lo.Resave(a.Standard,resaved);Require(save.Ok&&File.Exists(resaved),"Writer resave separate artifact");Require(OdfPackage.Inspect(resaved).Valid,"Writer resave package valid");var diff=OdfValidation.Diff(a.Standard,resaved);Require(!diff.ByteEqual,"Writer normalization observed");
            string pdf=Path.Combine(root,"writer.pdf");var rendered=lo.RenderPdf(a.Standard,pdf,true);Require(rendered.Ok&&File.Exists(pdf)&&new FileInfo(pdf).Length>1000,"Writer PDF render");
            string firstManifest=p.ManifestationId;lo.Stop();using var lo2=new LibreOfficeWriterProvider(soffice,probe,Path.Combine(root,"lo-profile-restart"));var p2=lo2.Start();Require(firstManifest!=p2.ManifestationId,"LibreOffice restart new manifestation");Require(lo2.Observe(a.Standard).Ok,"reobserve after provider restart");lo2.Stop();
        }
        listener.Stop();
        Console.WriteLine("O001_EXTENDED_SHA256="+a.ExtendedSha256);Console.WriteLine("O001_STANDARD_SHA256="+a.StandardSha256);Console.WriteLine("PREFLIGHT_BUILD002=PASS");return 0;
    }

    private static IEnumerable<OdfValidationResult> ValidateAll(string odt,string odf,string jing,string java,string root)
    {
        string schema=Path.Combine(odf,"OpenDocument-v1.4-schema.rng"),manifest=Path.Combine(odf,"OpenDocument-v1.4-manifest-schema.rng");
        yield return OdfValidation.ValidateContentWithJing(odt,schema,jing,java,Path.Combine(root,"content"));
        yield return OdfValidation.ValidateStylesWithJing(odt,schema,jing,java,Path.Combine(root,"styles"));
        yield return OdfValidation.ValidateMetaWithJing(odt,schema,jing,java,Path.Combine(root,"meta"));
        yield return OdfValidation.ValidateSettingsWithJing(odt,schema,jing,java,Path.Combine(root,"settings"));
        yield return OdfValidation.ValidateManifestWithJing(odt,manifest,jing,java,Path.Combine(root,"manifest"));
    }
    private static string ResolveJava(string repo){string p=Path.Combine(Path.GetDirectoryName(repo)!,"DOCSeye",".tools","java","jdk-25.0.4+7-jre","bin","java.exe");if(File.Exists(p))return p;p=Path.Combine(repo,".tools","java","jdk-25.0.4+7-jre","bin","java.exe");if(File.Exists(p))return p;throw new FileNotFoundException("java runtime");}
    private static string FindRepo(){string? p=AppContext.BaseDirectory;while(p is not null){if(File.Exists(Path.Combine(p,"DOCSeye.slnx")))return p;p=Directory.GetParent(p)?.FullName;}return Environment.GetEnvironmentVariable("DOCSEYE_REPO")??throw new DirectoryNotFoundException("DOCSeye repo");}
    private static void Reset(string path){if(Directory.Exists(path))Directory.Delete(path,true);}
}
