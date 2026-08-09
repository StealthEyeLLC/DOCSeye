using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

internal sealed record ChromeCdpObservation(string Dom,string[] AccessibilityRoles,string[] ExternalResources,int BrowserProcessId,long DurationMs);

internal static class ChromeCdpProbe
{
    public static ChromeCdpObservation Observe(string chromeExe,string pageUrl,string profilePath,int timeoutMs=30000)
    {
        Directory.CreateDirectory(profilePath);var sw=Stopwatch.StartNew();
        var psi=new ProcessStartInfo(chromeExe){UseShellExecute=false,CreateNoWindow=true,RedirectStandardOutput=true,RedirectStandardError=true};
        foreach(string a in new[]{"--headless=new","--no-sandbox","--disable-gpu","--disable-background-networking","--disable-component-update","--disable-extensions","--disable-default-apps","--disable-sync","--disable-features=OptimizationHints,MediaRouter,Translate","--no-first-run","--no-default-browser-check","--host-resolver-rules=MAP * 0.0.0.0","--remote-debugging-port=0","--user-data-dir="+profilePath,pageUrl})psi.ArgumentList.Add(a);
        using var process=Process.Start(psi)??throw new InvalidOperationException("chrome_start_failed");var stdout=process.StandardOutput.ReadToEndAsync();var stderr=process.StandardError.ReadToEndAsync();
        try
        {
            string activePort=Path.Combine(profilePath,"DevToolsActivePort");WaitUntil(()=>File.Exists(activePort)&&File.ReadAllLines(activePort).Length>=1,timeoutMs,"chrome_devtools_port_timeout");int port=int.Parse(File.ReadAllLines(activePort)[0]);
            using var http=new HttpClient{Timeout=TimeSpan.FromSeconds(5)};string? wsUrl=null;WaitUntil(()=>{try{using var list=JsonDocument.Parse(http.GetStringAsync($"http://127.0.0.1:{port}/json/list").GetAwaiter().GetResult());var target=list.RootElement.EnumerateArray().FirstOrDefault(x=>x.TryGetProperty("type",out var type)&&type.GetString()=="page"&&x.TryGetProperty("webSocketDebuggerUrl",out _));if(target.ValueKind==JsonValueKind.Object)wsUrl=target.GetProperty("webSocketDebuggerUrl").GetString();return !string.IsNullOrWhiteSpace(wsUrl);}catch{return false;}},timeoutMs,"chrome_page_target_timeout");
            using var ws=new ClientWebSocket();ws.ConnectAsync(new Uri(wsUrl!),CancellationToken.None).GetAwaiter().GetResult();int id=0;
            JsonElement Call(string method,object? parameters=null)
            {
                int callId=++id;var requestObject=new Dictionary<string,object?>(StringComparer.Ordinal){{"id",callId},{"method",method}};if(parameters is not null)requestObject["params"]=parameters;byte[] request=JsonSerializer.SerializeToUtf8Bytes(requestObject);ws.SendAsync(request,WebSocketMessageType.Text,true,CancellationToken.None).GetAwaiter().GetResult();
                while(true){using var ms=new MemoryStream();var buffer=new byte[32768];WebSocketReceiveResult rr;do{rr=ws.ReceiveAsync(buffer,CancellationToken.None).GetAwaiter().GetResult();if(rr.MessageType==WebSocketMessageType.Close)throw new InvalidOperationException("chrome_devtools_closed");ms.Write(buffer,0,rr.Count);}while(!rr.EndOfMessage);using var doc=JsonDocument.Parse(ms.ToArray());var root=doc.RootElement;if(root.TryGetProperty("id",out var rid)&&rid.GetInt32()==callId){if(root.TryGetProperty("error",out var error))throw new InvalidOperationException("chrome_cdp_error:"+error.GetRawText());return root.Clone();}}
            }
            Call("Runtime.enable");Call("DOM.enable");Call("Accessibility.enable");
            WaitUntil(()=>{var ready=Call("Runtime.evaluate",new{expression="document.readyState",returnByValue=true});return ready.GetProperty("result").GetProperty("result").TryGetProperty("value",out var v)&&v.GetString()=="complete";},timeoutMs,"chrome_document_ready_timeout");
            var document=Call("DOM.getDocument",new{depth=0,pierce=true});int nodeId=document.GetProperty("result").GetProperty("root").GetProperty("nodeId").GetInt32();string dom=Call("DOM.getOuterHTML",new{nodeId}).GetProperty("result").GetProperty("outerHTML").GetString()??string.Empty;
            var ax=Call("Accessibility.getFullAXTree");var roles=ax.GetProperty("result").GetProperty("nodes").EnumerateArray().Select(n=>n.TryGetProperty("role",out var role)&&role.TryGetProperty("value",out var value)?value.GetString():null).Where(x=>!string.IsNullOrWhiteSpace(x)).Select(x=>x!).Distinct(StringComparer.Ordinal).OrderBy(x=>x,StringComparer.Ordinal).ToArray();
            string expression="JSON.stringify(performance.getEntriesByType('resource').map(e=>e.name).filter(n=>!n.startsWith('file:')&&!n.startsWith('data:')&&!n.startsWith('blob:')))";var resource=Call("Runtime.evaluate",new{expression,returnByValue=true});string resourcesJson=resource.GetProperty("result").GetProperty("result").GetProperty("value").GetString()??"[]";string[] external=JsonSerializer.Deserialize<string[]>(resourcesJson)??[];
            sw.Stop();return new(dom,roles,external,process.Id,sw.ElapsedMilliseconds);
        }
        finally
        {
            if(!process.HasExited){try{process.Kill(true);}catch{}try{process.WaitForExit(5000);}catch{}}try{Task.WaitAll([stdout,stderr],2000);}catch{}
        }
    }

    private static void WaitUntil(Func<bool> predicate,int timeoutMs,string failure)
    {
        var sw=Stopwatch.StartNew();while(sw.ElapsedMilliseconds<timeoutMs){if(predicate())return;Thread.Sleep(50);}throw new TimeoutException(failure);
    }
}