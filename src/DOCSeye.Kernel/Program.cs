using System.IO.Pipes;
using System.Text;
using System.Text.Json;
using DOCSeye.Core;
using DOCSeye.Storage.Sqlite;

string pipeName=Environment.GetEnvironmentVariable("DOCSEYE_PIPE")??"docseye-kernel";
var sessions=new Dictionary<string,NativeDocumentSession>(StringComparer.Ordinal);
ProgramHostInvocation? programHost=null;
var jsonOptions=new JsonSerializerOptions{PropertyNamingPolicy=JsonNamingPolicy.CamelCase};
Console.CancelKeyPress+=(s,e)=>{e.Cancel=true;Environment.Exit(0);};
try
{
    while(true)
    {
        await using var pipe=new NamedPipeServerStream(pipeName,PipeDirection.InOut,NamedPipeServerStream.MaxAllowedServerInstances,PipeTransmissionMode.Byte,PipeOptions.Asynchronous);
        await pipe.WaitForConnectionAsync();
        using var reader=new StreamReader(pipe,new UTF8Encoding(false),false,65536,true);
        using var writer=new StreamWriter(pipe,new UTF8Encoding(false),65536,true){AutoFlush=true};
        string? line;
        while(pipe.IsConnected&&(line=await reader.ReadLineAsync()) is not null)
        {
            object response;
            try
            {
                using var doc=JsonDocument.Parse(line);var root=doc.RootElement;object? id=root.TryGetProperty("id",out var idEl)?JsonSerializer.Deserialize<object>(idEl.GetRawText()):null;
                string method=root.GetProperty("method").GetString()??throw new InvalidOperationException("missing method");
                JsonElement p=root.TryGetProperty("params",out var pp)?pp:default;
                object result=Dispatch(method,p);
                response=new{jsonrpc="2.0",id,result};
            }
            catch(Exception ex){response=new{jsonrpc="2.0",id=(object?)null,error=new{code=Classify(ex),message=ex.Message}};}
            await writer.WriteLineAsync(JsonSerializer.Serialize(response,jsonOptions));
        }
    }
}
finally{programHost?.Dispose();foreach(var s in sessions.Values)s.Dispose();}

object Dispatch(string method,JsonElement p)
{
    if(method.StartsWith("D.",StringComparison.Ordinal))return (programHost??=new ProgramHostInvocation()).Invoke(method,p);
    return method switch
    {
        "rpc.hello"=>new{protocol="docseye-rpc",version=1,kernelPid=Environment.ProcessId,architectureFreeze=DndConstants.ArchitectureFreeze},
        "document.open"=>Open(p.GetProperty("path").GetString()??throw new InvalidOperationException("path required")),
        "document.inspect"=>Inspect(p.GetProperty("sessionId").GetString()??throw new InvalidOperationException("sessionId required")),
        "document.validate"=>Validate(p.GetProperty("sessionId").GetString()??throw new InvalidOperationException("sessionId required")),
        "document.close"=>Close(p.GetProperty("sessionId").GetString()??throw new InvalidOperationException("sessionId required")),
        _=>throw new InvalidOperationException("unknown_method")
    };
}

object Open(string path)
{
    string id=Guid.NewGuid().ToString("N");var s=NativeDocumentSession.Open(path,true);sessions[id]=s;var h=TryHead(s);
    return new{sessionId=id,path=s.Path,writeAuthority=s.WriteAuthority,classification=s.Validation.Classification,head=h};
}
object Inspect(string id){var s=Get(id);return new{sessionId=id,path=s.Path,writeAuthority=s.WriteAuthority,classification=s.Validation.Classification,head=TryHead(s)};}
object Validate(string id){var s=Get(id);var v=s.ReconcileExternal(true);return new{sessionId=id,writeAuthority=s.WriteAuthority,classification=v.Classification,failedLevel=v.FailedLevel,diagnostics=v.Diagnostics,head=TryHead(s)};}
object Close(string id){if(!sessions.Remove(id,out var s))throw new KeyNotFoundException("session_not_found");s.Dispose();return new{sessionId=id,closed=true};}
NativeDocumentSession Get(string id)=>sessions.TryGetValue(id,out var s)?s:throw new KeyNotFoundException("session_not_found");
object? TryHead(NativeDocumentSession s){try{var h=s.ReadHead();return new{familyId=Ids.Lower(h.FamilyId),branchId=Ids.Lower(h.BranchId),revisionId=Ids.Lower(h.RevisionId),sequence=h.Sequence,semanticRoot=Convert.ToHexString(h.SemanticRoot).ToLowerInvariant(),parents=h.Parents.Select(Ids.Lower).ToArray(),mode=h.Mode};}catch{return null;}}
string Classify(Exception ex)=>ex switch{FileNotFoundException=>"not_found",KeyNotFoundException=>"not_found",InvalidOperationException ioe when ioe.Message=="unknown_method"=>"method_not_found",SemanticRefusalException sre=>sre.Code,_=>"kernel_error"};
