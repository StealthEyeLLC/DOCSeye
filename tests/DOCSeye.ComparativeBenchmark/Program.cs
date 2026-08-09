using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using DocumentFormat.OpenXml.Packaging;
using DOCSeye.Core;

if(args.Length>=3&&args[0]=="--route")
{
    string route=args[1],work=Path.GetFullPath(args[2]);RouteMetrics result=route switch{"native"=>NativeRoute.Run(work),"docx_first"=>DocxFirstRoute.Run(work),_=>throw new InvalidOperationException("unknown benchmark route")};Console.WriteLine(JsonSerializer.Serialize(result));return;
}

string repo=FindRepo();string evidencePath=Path.Combine(repo,"evidence","comparative-benchmark.json");string workRoot=Path.Combine(repo,"artifacts","comparative-benchmark",Guid.NewGuid().ToString("N"));Directory.CreateDirectory(workRoot);var sequence=new[]{"docx_first","native","native","docx_first","docx_first","native"};var samples=new List<Sample>();int ordinal=0;foreach(string route in sequence)samples.Add(RunChild(route,Path.Combine(workRoot,$"{++ordinal:00}-{route}")));
var native=samples.Where(x=>x.Metrics.Route=="native").ToArray();var docx=samples.Where(x=>x.Metrics.Route=="docx_first").ToArray();if(native.Length!=3||docx.Length!=3)throw new InvalidOperationException("benchmark sample cardinality");
var n=Summarize(native);var d=Summarize(docx);bool correctness=samples.All(s=>s.Metrics.ExactTargetRecovered&&s.Metrics.WrongTargetEdits==0&&s.Metrics.StaleWrites==0&&s.Metrics.AmbiguousWrites==0);if(!correctness)throw new InvalidOperationException("comparative benchmark correctness gate failed");
var improvements=new List<string>();if(n.ProviderRediscoveries<d.ProviderRediscoveries)improvements.Add($"full provider rediscoveries: native {n.ProviderRediscoveries} vs DOCX-first {d.ProviderRediscoveries}");if(n.CorrespondenceRecoveryAttempts<d.CorrespondenceRecoveryAttempts)improvements.Add($"candidate correspondence-recovery attempts after external move: native {n.CorrespondenceRecoveryAttempts} vs DOCX-first {d.CorrespondenceRecoveryAttempts}");if(n.RepresentationDiffBytes<d.RepresentationDiffBytes)improvements.Add($"artifact representation diff bytes: native {n.RepresentationDiffBytes} vs DOCX-first {d.RepresentationDiffBytes}");
bool falsified=!(n.ProviderRediscoveries<d.ProviderRediscoveries&&n.CorrespondenceRecoveryAttempts<d.CorrespondenceRecoveryAttempts);if(falsified)throw new InvalidOperationException("F-09 triggered: no measured operational correspondence advantage");
string sqlite=Path.Combine(repo,".tools","sqlite-native","sqlite3.dll");string dEvidencePath=Path.Combine(repo,"evidence","milestone-d.json");using var dEvidence=JsonDocument.Parse(File.ReadAllText(dEvidencePath));var dInvocation=dEvidence.RootElement.GetProperty("invocation");var productionD=new{calls=dInvocation.GetProperty("total_calls").GetInt32(),invocations=1,mutations=48,model_facing_request_bytes=dInvocation.GetProperty("model_facing_request_bytes").GetInt64(),model_facing_response_bytes=dInvocation.GetProperty("model_facing_response_bytes").GetInt64(),evidence="evidence/milestone-d.json"};var evidence=new
{
    architecture_freeze=DndConstants.ArchitectureFreeze,
    old_docx_first_freeze="47b5280b71773ae497b5a56c4b590b9070b4bca5",
    implementation_input_head="148505dcb56add909f5c199bb70083421e489e09",
    generated_utc=DateTimeOffset.UtcNow,
    result="PASS",
    benchmark_contract="same supported typed workflow executed against an executable reconstruction of the frozen-but-unimplemented DOCX-first operating model and the current native model",
    baseline_scope=new{classification="executable_reconstruction",reason="the prior DOCX-first freeze was PLANNED / NOT IMPLEMENTED; no old product binary is claimed",docx_truth="DOCX package representation",retained_witness="w14:paraId",recovery="full provider rescan after external package move",word_dependency="none for the measured common workflow"},
    environment=new{machine=Environment.MachineName,os=RuntimeInformation.OSDescription,architecture=RuntimeInformation.ProcessArchitecture.ToString(),runtime=RuntimeInformation.FrameworkDescription,processor_count=Environment.ProcessorCount,openxml_version=typeof(WordprocessingDocument).Assembly.GetName().Version?.ToString(),sqlite_native_sha256=File.Exists(sqlite)?Sha(sqlite):"missing",word_processes=Process.GetProcessesByName("WINWORD").Length,winword_executable_present=KnownWordExecutables().Any(File.Exists)},
    same_supported_workflow=new{paragraphs=BenchmarkFixture.ParagraphCount,duplicate_targets=3,target="middle duplicate paragraph",host_calls=new[]{"open_validate","query_duplicates","retain","begin","replace","commit","delta_read","reconcile","inspect_retained","summarize"},external_action="move the retained target to ordinal 0 while preserving its native/provider witness",program_host_calls=10,program_host_invocations=1,useful_operations=10,query_calls=2},
    program_host_context=new{benchmark_boundary_note="the comparative harness measures the same 10 typed model-facing calls in one isolated benchmark invocation per route; these are not Milestone D call IDs",production_native_D=productionD,frozen_old_docx_first_D_plan=new{calls=60,mutations=29,invocations=1,status="planned / not implemented at commit 47b5280; no runtime measurements claimed"}},
    sampling=new{cold_process_samples_per_route=3,execution_order=sequence,latency_summary="median of three isolated child-process samples",peak_memory_summary="median of sampled child-process peak working set",no_numeric_threshold_frozen=true},
    routes=new{native=n,docx_first=d},
    samples=samples,
    comparison=new
    {
        model_facing_bytes=new{native=n.ModelFacingBytes,docx_first=d.ModelFacingBytes},
        provider_rediscoveries=new{native=n.ProviderRediscoveries,docx_first=d.ProviderRediscoveries},
        query_calls=new{native=n.QueryCalls,docx_first=d.QueryCalls},
        operation_density=new{native_per_call=n.UsefulOperationsPerCall,docx_first_per_call=d.UsefulOperationsPerCall,native_per_invocation=n.UsefulOperationsPerInvocation,docx_first_per_invocation=d.UsefulOperationsPerInvocation},
        correspondence_recovery_attempts=new{native=n.CorrespondenceRecoveryAttempts,docx_first=d.CorrespondenceRecoveryAttempts},
        semantic_delta_bytes=new{native=n.SemanticDeltaBytes,docx_first=d.SemanticDeltaBytes},
        representation_diff_bytes=new{native=n.RepresentationDiffBytes,docx_first=d.RepresentationDiffBytes},
        latency_ms=new{native=n.WorkflowLatencyMs,docx_first=d.WorkflowLatencyMs},
        peak_memory_bytes=new{native=n.PeakWorkingSetBytes,docx_first=d.PeakWorkingSetBytes},
        touched=new{native=new{logical_records=n.LogicalRecordsTouched,physical_records=n.PhysicalRecordsTouched,physical_bytes=n.PhysicalBytesTouched,unit=n.PhysicalRecordUnit},docx_first=new{logical_records=d.LogicalRecordsTouched,physical_records=d.PhysicalRecordsTouched,physical_bytes=d.PhysicalBytesTouched,unit=d.PhysicalRecordUnit}},
        permanent_format_cost=new{native_initial_artifact_bytes=n.InitialArtifactBytes,docx_first_initial_artifact_bytes=d.InitialArtifactBytes},
        correctness=new{exact_target_recovery=true,wrong_target_edits=0,stale_writes=0,ambiguous_writes=0}
    },
    observed_native_improvements=improvements,
    observed_native_costs=new[]{ $"initial native artifact bytes {n.InitialArtifactBytes} vs DOCX-first {d.InitialArtifactBytes}", $"median workflow latency native {n.WorkflowLatencyMs} ms vs DOCX-first {d.WorkflowLatencyMs} ms", $"median peak working set native {n.PeakWorkingSetBytes} bytes vs DOCX-first {d.PeakWorkingSetBytes} bytes", $"model-facing bytes native {n.ModelFacingBytes} vs DOCX-first {d.ModelFacingBytes}", $"semantic delta bytes native {n.SemanticDeltaBytes} vs reconstructed DOCX-first operational delta {d.SemanticDeltaBytes}" },
    falsifier_F09=new{triggered=false,conclusion="NOT TRIGGERED: under the same measured workflow, native retained identity survives the external move without any full provider rediscovery or candidate correspondence recovery, while the reconstructed DOCX-first model requires a full provider rescan/recovery by retained provider witness. Permanent format-size, latency, memory, delta, and touched-byte costs are reported without inventing a threshold."}
};
Directory.CreateDirectory(Path.GetDirectoryName(evidencePath)!);File.WriteAllText(evidencePath,JsonSerializer.Serialize(evidence,new JsonSerializerOptions{WriteIndented=true}),new UTF8Encoding(false));Console.WriteLine("COMPARATIVE BENCHMARK PASS");Console.WriteLine("EVIDENCE="+evidencePath);Console.WriteLine("NATIVE="+JsonSerializer.Serialize(n));Console.WriteLine("DOCX_FIRST="+JsonSerializer.Serialize(d));

Sample RunChild(string route,string work)
{
    string exe=Environment.ProcessPath??throw new InvalidOperationException("process path missing");var psi=new ProcessStartInfo(exe){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};psi.ArgumentList.Add("--route");psi.ArgumentList.Add(route);psi.ArgumentList.Add(work);var p=Process.Start(psi)??throw new InvalidOperationException("benchmark child start failed");var stdout=p.StandardOutput.ReadToEndAsync();var stderr=p.StandardError.ReadToEndAsync();long peak=0;var wall=Stopwatch.StartNew();while(!p.WaitForExit(10)){try{p.Refresh();peak=Math.Max(peak,p.WorkingSet64);}catch{}}try{p.Refresh();peak=Math.Max(peak,p.PeakWorkingSet64);}catch{}wall.Stop();Task.WaitAll([stdout,stderr]);if(p.ExitCode!=0)throw new InvalidOperationException($"benchmark {route} failed: {stderr.Result}");string line=stdout.Result.Split(['\r','\n'],StringSplitOptions.RemoveEmptyEntries).LastOrDefault()??throw new InvalidOperationException("benchmark child output missing");var metrics=JsonSerializer.Deserialize<RouteMetrics>(line,new JsonSerializerOptions{PropertyNameCaseInsensitive=true})??throw new InvalidOperationException("benchmark child JSON invalid");return new(route,wall.ElapsedMilliseconds,peak,metrics,stderr.Result.Trim());
}

Summary Summarize(Sample[] route)
{
    long Med(Func<Sample,long> f){var a=route.Select(f).OrderBy(x=>x).ToArray();return a[a.Length/2];}int MedI(Func<Sample,int> f)=>checked((int)Med(x=>f(x)));var representative=route.OrderBy(x=>x.Metrics.WorkflowLatencyMs).ElementAt(route.Length/2).Metrics;return new(route[0].Route,route[0].Metrics.AuthorityModel,Med(x=>x.Metrics.InitialArtifactBytes),Med(x=>x.Metrics.FinalArtifactBytes),MedI(x=>x.Metrics.ProgramHostCalls),MedI(x=>x.Metrics.ProgramHostInvocations),MedI(x=>x.Metrics.UsefulOperations),MedI(x=>x.Metrics.QueryCalls),MedI(x=>x.Metrics.ProviderRediscoveries),MedI(x=>x.Metrics.CorrespondenceRecoveryAttempts),Med(x=>x.Metrics.ModelFacingRequestBytes+x.Metrics.ModelFacingResponseBytes),Med(x=>x.Metrics.SemanticDeltaBytes),Med(x=>x.Metrics.RepresentationDiffBytes),MedI(x=>x.Metrics.LogicalRecordsTouched),MedI(x=>x.Metrics.PhysicalRecordsTouched),Med(x=>x.Metrics.PhysicalBytesTouched),representative.PhysicalRecordUnit,Med(x=>x.WallMs),Med(x=>x.PeakWorkingSetBytes),(double)MedI(x=>x.Metrics.UsefulOperations)/Math.Max(1,MedI(x=>x.Metrics.ProgramHostCalls)),(double)MedI(x=>x.Metrics.UsefulOperations)/Math.Max(1,MedI(x=>x.Metrics.ProgramHostInvocations)),route.All(x=>x.Metrics.ExactTargetRecovered),route.Sum(x=>x.Metrics.WrongTargetEdits),route.Sum(x=>x.Metrics.StaleWrites),route.Sum(x=>x.Metrics.AmbiguousWrites),representative.Detail);
}

static string FindRepo(){string d=AppContext.BaseDirectory;for(int i=0;i<8;i++){if(File.Exists(Path.Combine(d,"DOCSeye.slnx")))return d;var p=Directory.GetParent(d);if(p is null)break;d=p.FullName;}throw new DirectoryNotFoundException("repo root not found");}
static string Sha(string path){using var fs=File.OpenRead(path);return Convert.ToHexString(SHA256.HashData(fs)).ToLowerInvariant();}
static IEnumerable<string> KnownWordExecutables(){yield return @"C:\Program Files\Microsoft Office\root\Office16\WINWORD.EXE";yield return @"C:\Program Files (x86)\Microsoft Office\root\Office16\WINWORD.EXE";}

internal sealed record Sample(string Route,long WallMs,long PeakWorkingSetBytes,RouteMetrics Metrics,string Stderr);
internal sealed record Summary(string Route,string AuthorityModel,long InitialArtifactBytes,long FinalArtifactBytes,int ProgramHostCalls,int ProgramHostInvocations,int UsefulOperations,int QueryCalls,int ProviderRediscoveries,int CorrespondenceRecoveryAttempts,long ModelFacingBytes,long SemanticDeltaBytes,long RepresentationDiffBytes,int LogicalRecordsTouched,int PhysicalRecordsTouched,long PhysicalBytesTouched,string PhysicalRecordUnit,long WorkflowLatencyMs,long PeakWorkingSetBytes,double UsefulOperationsPerCall,double UsefulOperationsPerInvocation,bool ExactTargetRecovered,int WrongTargetEdits,int StaleWrites,int AmbiguousWrites,object Detail);