using System.Text.Json;
namespace Bluey.Link;
public sealed record LiveLimits(int SessionSeconds=300,int Responses=5,int AudioSeconds=60,int MaxTokens=256)
{
    public void Validate(int minimumSeconds=30){if(SessionSeconds < minimumSeconds || SessionSeconds >600 || Responses is <1 or >20 || AudioSeconds is <1 or >120 || MaxTokens is <16 or >1024)throw new ProtocolException("live limits");}
}
public static class LiveProtocol
{
    public static readonly string[] Models=["gpt-realtime-2.1-mini","gpt-realtime-2.1"];
    public static void Id(JsonElement p,string key){if(!Wire.Uuid(Wire.Text(p,key)))throw new ProtocolException("live identity");}
    public static bool Flag(JsonElement p,string key)=>p.GetProperty(key).ValueKind switch{JsonValueKind.True=>true,JsonValueKind.False=>false,_=>throw new ProtocolException("live consent")};
    public static void Validate(string type,JsonElement p)
    {
        switch(type){
        case "liveConsent": Wire.Keys(p,"consentId","microphone","cloud","continuous");Id(p,"consentId");Flag(p,"microphone");Flag(p,"cloud");Flag(p,"continuous");break;
        case "liveRequest": Wire.Keys(p,"authorizationId","consentId","requestId");Id(p,"authorizationId");Id(p,"consentId");Id(p,"requestId");break;
        case "liveGrant": Wire.Keys(p,"authorizationId","consentId");Id(p,"authorizationId");Id(p,"consentId");break;
        case "liveStop": Wire.Keys(p,"code");if(!new[]{"stopped","denied","expired","limit","issuer","disconnected"}.Contains(Wire.Text(p,"code")))throw new ProtocolException("live stop");break;
        case "liveToken":
            Wire.Keys(p,"authorizationId","requestId","secret","expiresAt","model","sessionSeconds","responses","audioSeconds","maxTokens");Id(p,"authorizationId");Id(p,"requestId");
            var secret=Wire.Text(p,"secret",512);if(secret.Length<16||secret.Any(c=>c<33||c>126))throw new ProtocolException("live secret");
            if(!Models.Contains(Wire.Text(p,"model",64)))throw new ProtocolException("model");Wire.Integer(p,"expiresAt");
            new LiveLimits(checked((int)Wire.Integer(p,"sessionSeconds")),checked((int)Wire.Integer(p,"responses")),checked((int)Wire.Integer(p,"audioSeconds")),checked((int)Wire.Integer(p,"maxTokens"))).Validate(1);break;
        default:throw new ProtocolException("live command");}
    }
}
public sealed class ClientCredential(string value,long expiresAt)
{
    public string Value {get;}=value;public long ExpiresAt {get;}=expiresAt;
    public override string ToString()=>"[client secret redacted]";
}
public interface IClientSecretIssuer { Task<ClientCredential> Mint(string model,LiveLimits limits,CancellationToken stop); }
public sealed class DeniedIssuer : IClientSecretIssuer {public Task<ClientCredential> Mint(string model,LiveLimits limits,CancellationToken stop)=>throw new InvalidOperationException("disabled");}
public sealed class LivePacket(string type,object payload)
{
    public string Type{get;}=type;public object Payload{get;}=payload;public override string ToString()=>Type+" [payload redacted]";
}
