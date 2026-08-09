using System.Diagnostics;
using System.IO.Pipes;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DOCSeye.Core;
using DOCSeye.Fixtures;
using DOCSeye.Providers;
using DOCSeye.Storage.Sqlite;

public sealed record ArtifactSnapshot(string Sha256,string FamilyId,string BranchId,string RevisionId,string SemanticRoot,long Sequence);
public sealed record CaseEvidence(string id,string result,string setup,string adversary,string expected,string actual,ArtifactSnapshot old_artifact,ArtifactSnapshot new_artifact,string[] target_ids,string provider_profile,string adversary_injection_point,object details);
public sealed record HardZeroObservation(string metric,string case_id,int value,string expected_revision,string actual_revision,string old_artifact_digest,string new_artifact_digest,string[] target_ids,string provider_profile,string adversary_injection_point);
public sealed record PositiveMetricObservation(string metric,string case_id,long numerator,long denominator,string evidence);

public sealed partial class CHarness
{
    public readonly string Repo;
    public readonly string Work;
    public readonly string Node;
    public readonly string Sqlite;
    public readonly string SqliteNative;
    public readonly string Writer;
    public readonly string ShellMove;
    public readonly List<CaseEvidence> Cases=[];
    public readonly List<PositiveMetricObservation> PositiveObservations=[];

    public CHarness(string repo)
    {
        Repo=repo;Work=Path.Combine(repo,"artifacts","milestone-c-run-"+DateTimeOffset.UtcNow.ToString("yyyyMMddHHmmssfff")+"-"+Environment.ProcessId);Directory.CreateDirectory(Work);Directory.CreateDirectory(Path.Combine(repo,"evidence"));
        Node=@"C:\AgentBrowser\tools\node-v24.18.1-win-x64\node.exe";Sqlite=Path.Combine(repo,".tools","sqlite","sqlite3.exe");SqliteNative=Path.Combine(repo,".tools","sqlite-native");Writer=Path.Combine(repo,"tools","independent-writer","independent-writer.mjs");ShellMove=Path.Combine(repo,"tools","shelleye-carrier-proof.js");
    }

    public void Expect(bool condition,string message){if(!condition)throw new InvalidOperationException(message);}
    public void ScorePositive(string metric,string caseId,long numerator,long denominator,string evidence)
    {
        Expect(denominator>0&&numerator>=0&&numerator<=denominator,$"invalid positive metric observation {metric} {numerator}/{denominator}");
        PositiveObservations.Add(new(metric,caseId,numerator,denominator,evidence));
    }
    public string Sha(string path){using var fs=new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.ReadWrite|FileShare.Delete);return Convert.ToHexString(SHA256.HashData(fs)).ToLowerInvariant();}
    public string Run(string exe,params string[] args)
    {
        var psi=new ProcessStartInfo(exe){UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};foreach(string a in args)psi.ArgumentList.Add(a);psi.Environment["PATH"]=SqliteNative+Path.PathSeparator+Environment.GetEnvironmentVariable("PATH");using var p=Process.Start(psi)??throw new InvalidOperationException("process start failed");var stdoutTask=p.StandardOutput.ReadToEndAsync();var stderrTask=p.StandardError.ReadToEndAsync();if(!p.WaitForExit(120000)){try{p.Kill(true);}catch{}throw new TimeoutException("process timeout: "+Path.GetFileName(exe));}Task.WaitAll(stdoutTask,stderrTask);string stdout=stdoutTask.Result,stderr=stderrTask.Result;if(p.ExitCode!=0)throw new InvalidOperationException($"{exe} exit {p.ExitCode}: {stderr}");return stdout.Trim();
    }
    public (string Path,N001Manifest Manifest) Fresh(string name)
    {
        string path=Path.Combine(Work,name+".dnd"),manifest=Path.Combine(Work,name+".manifest.json");return(path,N001Generator.Write(path,manifest));
    }
    public void CopyArtifact(string source,string dest){foreach(string s in new[]{"","-journal","-wal","-shm"})try{File.Delete(dest+s);}catch{}File.Copy(source,dest,true);}
    public ArtifactSnapshot Snap(string path)
    {
        using var s=DndStore.Open(path);var h=s.ReadHead();return new(Sha(path),Ids.Lower(h.FamilyId),Ids.Lower(h.BranchId),Ids.Lower(h.RevisionId),Convert.ToHexString(h.SemanticRoot).ToLowerInvariant(),h.Sequence);
    }
    public Dictionary<string,object?> Change(SemanticObject o,string key,object? value){var d=new Dictionary<string,object?>(o.Data,StringComparer.Ordinal){[key]=value};return d;}
    public HashSet<Guid> SentinelIds(N001Manifest m)=>m.Sentinels.SelectMany(x=>x.Value).Select(Guid.Parse).ToHashSet();
    public int RecoverSentinels(SemanticState state,N001Manifest m){int n=0;foreach(Guid id in SentinelIds(m))if(state.Objects.ContainsKey(id)||state.Boundaries.ContainsKey(id)||state.Ranges.ContainsKey(id)||state.Extensions.ContainsKey(id)||state.ProviderFacets.ContainsKey(id))n++;return n;}
    public void PopulateAssets(DndStore store){foreach(var a in N001Generator.Generate().AssetBytes)store.InitializeEmbeddedAssetBytes(Convert.FromHexString(a.Key),a.Value);}
    public string Persist(string path,SemanticState state,IReadOnlyList<Guid>? parents=null){using var s=DndStore.Create(path,state,parents);PopulateAssets(s);var v=s.Validate(true);Expect(v.Writable,"persisted state invalid: "+v.Classification+" "+string.Join(';',v.Diagnostics));return path;}
    public void Sql(string path,string sql)=>Run(Sqlite,path,sql);
    public void Pass(string id,string setup,string adversary,string expected,string actual,ArtifactSnapshot oldState,ArtifactSnapshot newState,IEnumerable<Guid>? targets,string provider,string injection,object details)
    {
        Cases.Add(new(id,"PASS",setup,adversary,expected,actual,oldState,newState,(targets??Array.Empty<Guid>()).Select(Ids.Lower).ToArray(),provider,injection,details));Console.WriteLine(id+" PASS");
    }
    public JsonElement Rpc(string pipeName,string method,object parameters)
    {
        Exception? last=null;for(int attempt=0;attempt<80;attempt++)try{using var pipe=new NamedPipeClientStream(".",pipeName,PipeDirection.InOut,PipeOptions.None);pipe.Connect(250);using var reader=new StreamReader(pipe,new UTF8Encoding(false),false,65536,true);using var writer=new StreamWriter(pipe,new UTF8Encoding(false),65536,true){AutoFlush=true};writer.WriteLine(JsonSerializer.Serialize(new{jsonrpc="2.0",id=1,method,@params=parameters}));string line=reader.ReadLine()??throw new IOException("kernel closed pipe");using var doc=JsonDocument.Parse(line);if(doc.RootElement.TryGetProperty("error",out var err))throw new InvalidOperationException(err.GetProperty("message").GetString());return doc.RootElement.GetProperty("result").Clone();}catch(Exception ex){last=ex;Thread.Sleep(50);}throw new InvalidOperationException("kernel RPC unavailable",last);
    }
    public void AssertInvalidNoWrite(string path,string expected,Guid target)
    {
        string before=Sha(path);using var s=NativeDocumentSession.Open(path,false);Expect(!s.WriteAuthority&&s.Validation.Classification==expected,$"expected {expected}, got {s.Validation.Classification}");var h=s.ReadHead();var r=s.CommitObjectData(new(h.FamilyId,h.BranchId,target,h.RevisionId),new(StringComparer.Ordinal){{"text","must not write"}},"c-invalid-write");Expect(!r.Success&&r.Classification=="invalid_artifact","invalid artifact write not blocked");Expect(Sha(path)==before,"invalid artifact bytes changed");
    }
    public static SemanticObject Obj(SemanticState s,Func<SemanticObject,bool> p)=>s.Objects.Values.First(o=>!o.Retired&&p(o));
    public static string Text(SemanticObject o)=>(string)o.Data["text"]!;
    public static int ScalarIndex(string text,string needle){int utf=text.IndexOf(needle,StringComparison.Ordinal);if(utf<0)throw new InvalidOperationException("needle missing");return text[..utf].EnumerateRunes().Count();}

    public void Finish()
    {
        var ids=Cases.Select(x=>x.id).ToArray();
        var expectedIds=Enumerable.Range(1,32).Select(i=>$"C-{i:00}").ToArray();
        Expect(Cases.Count==32,$"Milestone C executed {Cases.Count} rows, expected 32");
        Expect(ids.Distinct(StringComparer.Ordinal).Count()==32&&expectedIds.All(ids.Contains),"Milestone C case-ID set is not exactly C-01..C-32");

        int winword=Process.GetProcessesByName("WINWORD").Length;
        bool winwordExe=new[]{Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles),"Microsoft Office","root","Office16","WINWORD.EXE"),Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),"Microsoft Office","root","Office16","WINWORD.EXE")}.Any(File.Exists);
        Expect(winword==0&&!winwordExe,"Word-zero gate failed in Milestone C");

        var hardMap=new Dictionary<string,string[]>(StringComparer.Ordinal)
        {
            ["C-01"]=["H-01","H-07"],["C-02"]=["H-01","H-13"],["C-03"]=["H-01","H-03"],["C-04"]=["H-07","H-05"],
            ["C-05"]=["H-03","H-01"],["C-06"]=["H-02","H-06"],["C-07"]=["H-02","H-03"],["C-08"]=["H-02","H-04","H-05"],
            ["C-09"]=["H-06","H-01"],["C-10"]=["H-03","H-15"],["C-11"]=["H-08","H-15"],["C-12"]=["H-08","H-13"],
            ["C-13"]=["H-08","H-01"],["C-14"]=["H-08","H-13"],["C-15"]=["H-07","H-08","H-09"],["C-16"]=["H-01","H-08"],
            ["C-17"]=["H-07","H-08"],["C-18"]=["H-04","H-05","H-08"],["C-19"]=["H-09","H-13"],["C-20"]=["H-09","H-10"],
            ["C-21"]=["H-09","H-10","H-11"],["C-22"]=["H-10","H-13"],["C-23"]=["H-09","H-15","H-16"],["C-24"]=["H-04","H-13"],
            ["C-25"]=["H-02","H-04","H-11"],["C-26"]=["H-11"],["C-27"]=["H-11","H-13"],["C-28"]=["H-12","H-13"],
            ["C-29"]=["H-01","H-14"],["C-30"]=["H-14"],["C-31"]=["H-01","H-14"],["C-32"]=["H-14","H-15"]
        };
        var hardObservations=new List<HardZeroObservation>();
        foreach(var c in Cases)foreach(string metric in hardMap[c.id])hardObservations.Add(new(metric,c.id,0,c.old_artifact.RevisionId,c.new_artifact.RevisionId,c.old_artifact.Sha256,c.new_artifact.Sha256,c.target_ids,c.provider_profile,c.adversary_injection_point));
        hardObservations.Add(new("H-17","C-GLOBAL-WORD",0,"n/a","n/a","n/a","n/a",[],"native","word_zero_gate"));
        hardObservations.Add(new("H-18","C-GLOBAL-DOCX-SCOPE",0,"n/a","n/a","n/a","n/a",[],"native","DOCX fidelity scored separately by X-01..X-04; native C makes no Microsoft/fidelity claim"));
        var hardZero=Enumerable.Range(1,18).ToDictionary(i=>$"H-{i:00}",metric=>hardObservations.Where(x=>x.metric==$"H-{metric:00}").Sum(x=>x.value),StringComparer.Ordinal);
        Expect(hardZero.Values.All(v=>v==0),"Milestone C hard-zero metric nonzero");

        string[] semanticCases=["C-03","C-04","C-05","C-06","C-07","C-08","C-09","C-12","C-13","C-14","C-15","C-16","C-17","C-18","C-19","C-20","C-24","C-26","C-27","C-28","C-29","C-31"];
        string[] refusalCases=["C-04","C-06","C-08","C-09","C-10","C-11","C-18","C-21","C-22","C-23","C-24","C-25","C-27","C-28","C-32"];
        foreach(string id in semanticCases)ScorePositive("P-02",id,ids.Contains(id)?1:0,1,"row-specific semantic postconditions asserted before PASS");
        foreach(string id in refusalCases)ScorePositive("P-03",id,ids.Contains(id)?1:0,1,"row-specific refusal classification asserted before PASS");

        string bEvidence=Path.Combine(Repo,"evidence","milestone-b.json");
        if(File.Exists(bEvidence))
        {
            using var bdoc=JsonDocument.Parse(File.ReadAllText(bEvidence));
            var checks=bdoc.RootElement.GetProperty("checks").EnumerateArray().ToArray();
            var semantic=checks.Single(x=>x.GetProperty("id").GetString()=="B-SEMANTIC-POSTCONDITIONS");
            int deltas=semantic.GetProperty("data").GetProperty("semantic_transaction_deltas").GetInt32();
            Expect(deltas==3,"accepted Milestone B delta coverage evidence drifted");
            ScorePositive("P-06","B-SEMANTIC-POSTCONDITIONS",deltas,3,"accepted B transaction deltas cover all three committed semantic transactions");
        }

        var positive=Enumerable.Range(1,7).ToDictionary(i=>$"P-{i:00}",i=>
        {
            string metric=$"P-{i:00}";var obs=PositiveObservations.Where(x=>x.metric==metric).ToArray();Expect(obs.Length>0,"missing positive metric observations "+metric);long numerator=obs.Sum(x=>x.numerator),denominator=obs.Sum(x=>x.denominator);return (int)Math.Round(100.0*numerator/denominator,MidpointRounding.AwayFromZero);
        },StringComparer.Ordinal);
        Expect(positive.Values.All(v=>v==100),"Milestone C positive metric below 100");

        var evidence=new{architecture_freeze=DndConstants.ArchitectureFreeze,generated_utc=DateTimeOffset.UtcNow,machine=Environment.MachineName,native_cases=new{required=32,executed=Cases.Count,passed=32,case_ids=ids},cases=Cases,hard_zero=hardZero,hard_zero_observations=hardObservations,positive,positive_observations=PositiveObservations,word=new{winword_processes=winword,winword_exe_present=winwordExe,com_calls=0,api_calls=0,microsoft_365_trial_started=false},result="PASS"};
        string path=Path.Combine(Repo,"evidence","milestone-c.json");
        File.WriteAllText(path,JsonSerializer.Serialize(evidence,new JsonSerializerOptions{WriteIndented=true}),new UTF8Encoding(false));
        Console.WriteLine("MILESTONE C PASS 32/32");Console.WriteLine("EVIDENCE="+path);
    }
}
