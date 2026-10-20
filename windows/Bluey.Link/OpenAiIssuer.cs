using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace Bluey.Link;
public interface IMasterKey { byte[] Read(); }
public sealed class OpenAiIssuer(IMasterKey keys,HttpClient http,Func<long>? unixSeconds=null):IClientSecretIssuer
{
    public static object Configuration(string model,LiveLimits limits)=>new {
        expires_after=new{anchor="created_at",seconds=30},
        session=new{type="realtime",model,output_modalities=new[]{"text"},max_output_tokens=limits.MaxTokens,
            instructions="You are Bluey, a friendly concise desktop companion. Reply briefly in plain text. No tools or actions are available.",
            tools=Array.Empty<object>(),tool_choice="none",tracing=(object?)null,
            audio=new{input=new{format=new{type="audio/pcm",rate=24000},turn_detection=(object?)null,transcription=(object?)null}}}};
    public async Task<ClientCredential> Mint(string model,LiveLimits limits,CancellationToken stop)
    {
        limits.Validate();if(!LiveProtocol.Models.Contains(model))throw new ProtocolException("model");
        var bytes=keys.Read();try {
            using var request=new HttpRequestMessage(HttpMethod.Post,"https://api.openai.com/v1/realtime/client_secrets");
            request.Headers.Authorization=new AuthenticationHeaderValue("Bearer",Encoding.UTF8.GetString(bytes));
            request.Content=new StringContent(JsonSerializer.Serialize(Configuration(model,limits)),Encoding.UTF8,"application/json");
            using var response=await http.SendAsync(request,HttpCompletionOption.ResponseHeadersRead,stop);
            if(!response.IsSuccessStatusCode)throw new IOException("issuer rejected");
            await using var stream=await response.Content.ReadAsStreamAsync(stop);using var memory=new MemoryStream();var chunk=new byte[1024];
            while(true){var n=await stream.ReadAsync(chunk,stop);if(n==0)break;if(memory.Length+n>8192)throw new IOException("issuer size");memory.Write(chunk,0,n);}
            var data=memory.ToArray();try {
                using var doc=JsonDocument.Parse(data);var p=doc.RootElement;var value=p.GetProperty("value").GetString()??throw new IOException("issuer secret");var expires=p.GetProperty("expires_at").GetInt64();
                var now=(unixSeconds??(()=>DateTimeOffset.UtcNow.ToUnixTimeSeconds()))();
                if(value.Length is <16 or >512||value.Any(c=>c<33||c>126)||expires<=now||expires>now+60)throw new IOException("issuer expiry");return new(value,expires);
            }finally{CryptographicOperations.ZeroMemory(data);}
        }finally{CryptographicOperations.ZeroMemory(bytes);}
    }
}
