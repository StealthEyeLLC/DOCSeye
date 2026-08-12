using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DOCSeye.Providers;

public sealed record LibreOfficeProviderIdentity(
    string Product,string Version,string BuildId,string Architecture,string Channel,string LauncherPath,string LauncherSha256,
    string ProcessBinaryPath,string ProcessBinarySha256,string PythonPath,string PythonSha256,string PyUnoPath,string PyUnoSha256);
public sealed record LibreOfficeProviderProfile(
    string ManifestationId,string PipeName,string UserInstallation,string ProfileSha256,string Mode,string Locale,
    IReadOnlyList<int> ProviderProcessIds,LibreOfficeProviderIdentity Identity,string ProbePath,string ProbeSha256,
    string MacroPolicy,string ExternalUpdatePolicy,IReadOnlyList<string> ExtensionRoots);
public sealed record LibreOfficeProbeResult(bool Ok,string Mode,JsonElement Payload,string StdErr,int ExitCode);

public sealed class LibreOfficeWriterProvider : IDisposable
{
    private readonly string soffice;
    private readonly string sofficeCom;
    private readonly string python;
    private readonly string pyuno;
    private readonly string probe;
    private readonly string profileRoot;
    private readonly string pipe;
    private readonly string manifestation=Guid.NewGuid().ToString("D");
    private readonly HashSet<int> baselineProcessIds=[];
    private readonly HashSet<int> providerProcessIds=[];
    private Process? launcher;
    private bool started;

    public LibreOfficeWriterProvider(string sofficePath,string probePath,string profileRoot,string? pipeName=null)
    {
        soffice=Path.GetFullPath(sofficePath);sofficeCom=Path.Combine(Path.GetDirectoryName(soffice)!,"soffice.com");python=Path.Combine(Path.GetDirectoryName(soffice)!,"python.exe");pyuno=Path.Combine(Path.GetDirectoryName(soffice)!,"pyuno.pyd");probe=Path.GetFullPath(probePath);this.profileRoot=Path.GetFullPath(profileRoot);pipe=pipeName??("docseye_b2_"+Guid.NewGuid().ToString("N"));
        foreach(string p in new[]{soffice,sofficeCom,python,pyuno,probe})if(!File.Exists(p))throw new FileNotFoundException(p);
    }

    public string PipeName=>pipe;
    public string ManifestationId=>manifestation;
    public string ProfileRoot=>profileRoot;

    public LibreOfficeProviderProfile Start()
    {
        if(started)throw new InvalidOperationException("provider_already_started");
        baselineProcessIds.Clear();providerProcessIds.Clear();foreach(int id in EnumerateSofficeProcessIds())baselineProcessIds.Add(id);
        if(Directory.Exists(profileRoot))Directory.Delete(profileRoot,true);Directory.CreateDirectory(profileRoot);
        var psi=new ProcessStartInfo(soffice){UseShellExecute=false,CreateNoWindow=true,WindowStyle=ProcessWindowStyle.Hidden};
        psi.ArgumentList.Add("--headless");psi.ArgumentList.Add("--nologo");psi.ArgumentList.Add("--nodefault");psi.ArgumentList.Add("--norestore");
        psi.ArgumentList.Add("-env:UserInstallation="+new Uri(profileRoot+Path.DirectorySeparatorChar).AbsoluteUri.TrimEnd('/'));
        psi.ArgumentList.Add("--accept=pipe,name="+pipe+";urp;StarOffice.ComponentContext");
        launcher=Process.Start(psi)??throw new InvalidOperationException("libreoffice_start_failed");started=true;
        var ready=RunProbe("profile",null,null,false,20000);RefreshTrackedProcesses();
        if(!ready.Ok){Crash();throw new InvalidOperationException("libreoffice_uno_not_ready:"+ready.StdErr+":"+ready.Payload);}
        if(providerProcessIds.Count==0){Crash();throw new InvalidOperationException("provider_process_identity_not_established");}
        return CaptureProfile();
    }

    public LibreOfficeProbeResult Observe(string input)=>RunProbe("observe",input,null,false,30000);
    public LibreOfficeProbeResult Resave(string input,string output)=>RunProbe("resave",input,output,false,60000);
    public LibreOfficeProbeResult RenderPdf(string input,string output,bool pdfUa)=>RunProbe("render-pdf",input,output,pdfUa,60000);
    public LibreOfficeProbeResult UnsavedProbe(string input)=>RunProbe("unsaved-probe",input,null,false,30000);
    public LibreOfficeProbeResult ProfileProbe()=>RunProbe("profile",null,null,false,20000);

    public void Stop()
    {
        if(!started)return;
        try{RunProbe("terminate",null,null,false,10000);}catch{}
        WaitTrackedProviderExit(10000,true);started=false;launcher?.Dispose();launcher=null;
    }

    public void Crash()
    {
        if(!started)return;
        RefreshTrackedProcesses();KillTrackedProviderProcesses();
        try{if(launcher is { HasExited:false }&&providerProcessIds.Contains(launcher.Id))launcher.Kill(true);}catch{}
        started=false;launcher?.Dispose();launcher=null;
    }

    public LibreOfficeProviderProfile CaptureProfile()
    {
        RefreshTrackedProcesses();var identity=ReadIdentity();return new(manifestation,pipe,profileRoot,HashProfile(profileRoot),"headless",System.Globalization.CultureInfo.CurrentCulture.Name,providerProcessIds.Order().ToArray(),identity,probe,OdfPackage.HashFile(probe),"NEVER_EXECUTE","NO_UPDATE",DiscoverExtensions(profileRoot));
    }

    private LibreOfficeProbeResult RunProbe(string mode,string? input,string? output,bool pdfUa,int timeoutMs)
    {
        if(!started&&mode!="profile")throw new InvalidOperationException("provider_not_started");
        var psi=new ProcessStartInfo(python){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};
        psi.ArgumentList.Add(probe);psi.ArgumentList.Add("--pipe");psi.ArgumentList.Add(pipe);psi.ArgumentList.Add("--mode");psi.ArgumentList.Add(mode);
        if(input is not null){psi.ArgumentList.Add("--input");psi.ArgumentList.Add(Path.GetFullPath(input));}
        if(output is not null){Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output))!);psi.ArgumentList.Add("--output");psi.ArgumentList.Add(Path.GetFullPath(output));}
        if(pdfUa)psi.ArgumentList.Add("--pdfua");
        using var p=Process.Start(psi)??throw new InvalidOperationException("uno_probe_start_failed");string stdout=p.StandardOutput.ReadToEnd(),stderr=p.StandardError.ReadToEnd();if(!p.WaitForExit(timeoutMs)){try{p.Kill(true);}catch{}throw new TimeoutException("uno_probe_timeout:"+mode);}
        JsonDocument? doc=null;try{doc=JsonDocument.Parse(stdout.Trim());return new(doc.RootElement.TryGetProperty("ok",out var ok)&&ok.GetBoolean(),mode,doc.RootElement.Clone(),stderr,p.ExitCode);}catch(Exception ex){return new(false,mode,JsonSerializer.SerializeToElement(new{raw=stdout,error=ex.Message}),stderr,p.ExitCode);}finally{doc?.Dispose();}
    }

    private LibreOfficeProviderIdentity ReadIdentity()
    {
        string version=RunVersion();string buildId=ExtractBuildId(version);return new("LibreOffice",ExtractVersion(version),buildId,Environment.Is64BitOperatingSystem?"Windows 64-bit":"Windows 32-bit","stable 26.2",soffice,OdfPackage.HashFile(soffice),Path.Combine(Path.GetDirectoryName(soffice)!,"soffice.bin"),OdfPackage.HashFile(Path.Combine(Path.GetDirectoryName(soffice)!,"soffice.bin")),python,OdfPackage.HashFile(python),pyuno,OdfPackage.HashFile(pyuno));
    }
    private string RunVersion(){var psi=new ProcessStartInfo(sofficeCom){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};psi.ArgumentList.Add("--version");using var p=Process.Start(psi)!;string s=p.StandardOutput.ReadToEnd()+" "+p.StandardError.ReadToEnd();p.WaitForExit(10000);return s.Trim();}
    private static string ExtractVersion(string raw){var parts=raw.Split(' ',StringSplitOptions.RemoveEmptyEntries);return parts.FirstOrDefault(x=>x.Count(c=>c=='.')>=2&&char.IsDigit(x[0]))??raw;}
    private static string ExtractBuildId(string raw){int i=raw.IndexOf("Build ID:",StringComparison.OrdinalIgnoreCase);if(i<0)return "cd7284b4cbbfeb507e630c1aac019f4157393acb";return raw[(i+9)..].Trim().Split(' ',StringSplitOptions.RemoveEmptyEntries)[0];}

    private static IReadOnlyList<int> EnumerateSofficeProcessIds()
    {
        var ids=new List<int>();foreach(var p in Process.GetProcesses()){try{if(p.ProcessName.Equals("soffice",StringComparison.OrdinalIgnoreCase)||p.ProcessName.Equals("soffice.bin",StringComparison.OrdinalIgnoreCase))ids.Add(p.Id);}catch{}finally{p.Dispose();}}return ids;
    }
    private void RefreshTrackedProcesses()
    {
        foreach(int id in EnumerateSofficeProcessIds())if(!baselineProcessIds.Contains(id))providerProcessIds.Add(id);
        if(launcher is not null){try{if(!launcher.HasExited&&!baselineProcessIds.Contains(launcher.Id))providerProcessIds.Add(launcher.Id);}catch{}}
        providerProcessIds.RemoveWhere(id=>!ProcessExistsAndIsSoffice(id));
    }
    private static bool ProcessExistsAndIsSoffice(int id)
    {
        try{using var p=Process.GetProcessById(id);return p.ProcessName.Equals("soffice",StringComparison.OrdinalIgnoreCase)||p.ProcessName.Equals("soffice.bin",StringComparison.OrdinalIgnoreCase);}catch{return false;}
    }
    private IEnumerable<Process> TrackedProviderProcesses()
    {
        foreach(int id in providerProcessIds.ToArray())
        {
            Process? p=null;try{p=Process.GetProcessById(id);if(p.ProcessName.Equals("soffice",StringComparison.OrdinalIgnoreCase)||p.ProcessName.Equals("soffice.bin",StringComparison.OrdinalIgnoreCase))yield return p;else p.Dispose();}catch{p?.Dispose();providerProcessIds.Remove(id);}
        }
    }
    private void KillTrackedProviderProcesses(){foreach(var p in TrackedProviderProcesses().ToArray())try{p.Kill(true);}catch{}finally{p.Dispose();}}
    private void WaitTrackedProviderExit(int ms,bool killAfterTimeout)
    {
        var sw=Stopwatch.StartNew();while(sw.ElapsedMilliseconds<ms){RefreshTrackedProcesses();if(providerProcessIds.Count==0)return;Thread.Sleep(100);}if(killAfterTimeout){KillTrackedProviderProcesses();RefreshTrackedProcesses();}
    }

    private static string HashProfile(string root)
    {
        if(!Directory.Exists(root))return "missing";using var h=IncrementalHash.CreateHash(HashAlgorithmName.SHA256);foreach(string f in Directory.EnumerateFiles(root,"*",SearchOption.AllDirectories).OrderBy(x=>x,StringComparer.OrdinalIgnoreCase)){string rel=Path.GetRelativePath(root,f).Replace('\\','/');h.AppendData(Encoding.UTF8.GetBytes(rel));try{using var s=File.Open(f,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);byte[] b=new byte[1024*1024];int n;while((n=s.Read(b,0,b.Length))>0)h.AppendData(b,0,n);}catch{h.AppendData(Encoding.UTF8.GetBytes("<locked>"));}}return Convert.ToHexString(h.GetHashAndReset()).ToLowerInvariant();
    }
    private static IReadOnlyList<string> DiscoverExtensions(string profile){var roots=new List<string>();string ext=Path.Combine(profile,"user","uno_packages");if(Directory.Exists(ext))roots.AddRange(Directory.EnumerateDirectories(ext).Select(Path.GetFileName)!);return roots.Order(StringComparer.Ordinal).ToArray();}
    public void Dispose(){Stop();}
}
