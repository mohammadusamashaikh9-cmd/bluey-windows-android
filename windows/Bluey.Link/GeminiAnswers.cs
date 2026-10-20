using System.Buffers.Binary;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
namespace Bluey.Link;

public enum AudioAnswerFailure { Credential, Model, InvalidAudio, Quota, Blocked, Empty, Malformed, Provider, Network, Timeout }
public sealed class AudioAnswerException(AudioAnswerFailure reason) : IOException("Gemini request stopped: "+reason)
{
    public AudioAnswerFailure Reason { get; }=reason;
    public override string ToString()=>"Gemini ["+Reason+"]";
}
public static class PcmWave
{
    public static byte[] Encode(ReadOnlySpan<byte> pcm)
    {
        if(pcm.Length<5*AudioProtocol.ChunkBytes||pcm.Length>AudioProtocol.MaxBytes||pcm.Length%AudioProtocol.ChunkBytes!=0)
            throw new AudioAnswerException(AudioAnswerFailure.InvalidAudio);
        var wave=new byte[44+pcm.Length];
        "RIFF"u8.CopyTo(wave);BinaryPrimitives.WriteInt32LittleEndian(wave.AsSpan(4),36+pcm.Length);
        "WAVEfmt "u8.CopyTo(wave.AsSpan(8));BinaryPrimitives.WriteInt32LittleEndian(wave.AsSpan(16),16);
        BinaryPrimitives.WriteInt16LittleEndian(wave.AsSpan(20),1); // Linear PCM.
        BinaryPrimitives.WriteInt16LittleEndian(wave.AsSpan(22),1); // Mono.
        BinaryPrimitives.WriteInt32LittleEndian(wave.AsSpan(24),24000);
        BinaryPrimitives.WriteInt32LittleEndian(wave.AsSpan(28),48000);
        BinaryPrimitives.WriteInt16LittleEndian(wave.AsSpan(32),2);
        BinaryPrimitives.WriteInt16LittleEndian(wave.AsSpan(34),16);
        "data"u8.CopyTo(wave.AsSpan(36));BinaryPrimitives.WriteInt32LittleEndian(wave.AsSpan(40),pcm.Length);
        pcm.CopyTo(wave.AsSpan(44));return wave;
    }
}
// Only LiveAuthority calls this after current lease/consent/desktop authorization.
// ContractVerified describes the public API contract, never a user's billing eligibility.
public sealed class GeminiAnswers : IAudioAnswers
{
    public const string Model="gemini-3.8-flash";
    public const int MaxRequestBytes=330000,MaxResponseBytes=16384,MaxOutputTokens=128;
    private readonly IMasterKey keys;private readonly HttpClient http;private readonly TimeProvider clock;
    private static readonly Uri Endpoint=new("https://generativelanguage.googleapis.com/v1beta/models/"+Model+":generateContent");
    public bool ContractVerified=>true;
    public GeminiAnswers(IMasterKey keys,HttpClient http,string model=Model,TimeProvider? clock=null)
    {
        if(model!=Model)throw new AudioAnswerException(AudioAnswerFailure.Model);
        this.keys=keys;this.http=http;this.clock=clock??TimeProvider.System;
    }
    public static HttpClient CreateHttpClient()=>new(new SocketsHttpHandler {
        AllowAutoRedirect=false,UseCookies=false,ConnectTimeout=TimeSpan.FromSeconds(5),MaxResponseHeadersLength=16,MaxConnectionsPerServer=1
    }) { Timeout=System.Threading.Timeout.InfiniteTimeSpan };

    public async Task<string> Answer(ReadOnlyMemory<byte> pcm,CancellationToken stop)
    {
        stop.ThrowIfCancellationRequested();
        using var deadline=new CancellationTokenSource(TimeSpan.FromSeconds(20),clock);
        using var linked=CancellationTokenSource.CreateLinkedTokenSource(stop,deadline.Token);var cancel=linked.Token;
        byte[]? wave=null,key=null,body=null,responseBytes=null;
        try {
            wave=PcmWave.Encode(pcm.Span);cancel.ThrowIfCancellationRequested();
            try {key=keys.Read();}catch(Exception e)when(e is IOException or UnauthorizedAccessException or CryptographicException){throw new AudioAnswerException(AudioAnswerFailure.Credential);}
            if(key.Length is <16 or >512||key.Any(b=>b<33||b>126))throw new AudioAnswerException(AudioAnswerFailure.Credential);
            body=new byte[MaxRequestBytes];var count=WriteRequest(body,wave);
            cancel.ThrowIfCancellationRequested();
            using var request=new HttpRequestMessage(HttpMethod.Post,Endpoint);
            request.Headers.Add("x-goog-api-key",Encoding.ASCII.GetString(key));request.Headers.Accept.Add(new("application/json"));
            request.Content=new ByteArrayContent(body,0,count);request.Content.Headers.ContentType=new MediaTypeHeaderValue("application/json");
            using var response=await http.SendAsync(request,HttpCompletionOption.ResponseHeadersRead,cancel).ConfigureAwait(false);
            cancel.ThrowIfCancellationRequested();
            if(!response.IsSuccessStatusCode)throw StatusFailure((int)response.StatusCode);
            if(response.StatusCode==HttpStatusCode.NoContent||response.Content.Headers.ContentLength==0)throw new AudioAnswerException(AudioAnswerFailure.Empty);
            if(response.StatusCode!=HttpStatusCode.OK)throw new AudioAnswerException(AudioAnswerFailure.Provider);
            if(response.Content.Headers.ContentLength>MaxResponseBytes)throw new AudioAnswerException(AudioAnswerFailure.Malformed);
            if(!string.Equals(response.Content.Headers.ContentType?.MediaType,"application/json",StringComparison.OrdinalIgnoreCase))throw new AudioAnswerException(AudioAnswerFailure.Malformed);
            await using var stream=await response.Content.ReadAsStreamAsync(cancel).ConfigureAwait(false);
            responseBytes=new byte[MaxResponseBytes+1];var used=0;
            while(true) {
                var n=await stream.ReadAsync(responseBytes.AsMemory(used,responseBytes.Length-used),cancel).ConfigureAwait(false);
                if(n==0)break;used+=n;if(used>MaxResponseBytes)throw new AudioAnswerException(AudioAnswerFailure.Malformed);
            }
            cancel.ThrowIfCancellationRequested();
            var result=Parse(responseBytes.AsMemory(0,used));cancel.ThrowIfCancellationRequested();return result;
        }
        catch(OperationCanceledException) {
            if(stop.IsCancellationRequested)throw new OperationCanceledException("Audio request canceled",stop);
            throw new AudioAnswerException(AudioAnswerFailure.Timeout);
        }
        catch(AudioAnswerException){throw;}
        catch(Exception e)when(e is HttpRequestException or IOException) {
            if(stop.IsCancellationRequested)throw new OperationCanceledException("Audio request canceled",stop);
            throw new AudioAnswerException(deadline.IsCancellationRequested?AudioAnswerFailure.Timeout:AudioAnswerFailure.Network);
        }
        finally {
            if(wave!=null)CryptographicOperations.ZeroMemory(wave);if(key!=null)CryptographicOperations.ZeroMemory(key);
            if(body!=null)CryptographicOperations.ZeroMemory(body);if(responseBytes!=null)CryptographicOperations.ZeroMemory(responseBytes);
        }
    }
    private static int WriteRequest(byte[] body,byte[] wave)
    {
        // A fixed writable buffer avoids retaining a managed base64 audio string.
        using var memory=new MemoryStream(body,0,body.Length,true,true);memory.SetLength(0);
        using(var json=new Utf8JsonWriter(memory)) {
            json.WriteStartObject();json.WritePropertyName("systemInstruction");json.WriteStartObject();json.WritePropertyName("parts");json.WriteStartArray();
            json.WriteStartObject();json.WriteString("text","You are Bluey, a friendly desktop companion. Answer the spoken question in at most two short sentences of plain text. Do not transcribe it. No tools or actions are available.");json.WriteEndObject();json.WriteEndArray();json.WriteEndObject();
            json.WritePropertyName("contents");json.WriteStartArray();json.WriteStartObject();json.WriteString("role","user");json.WritePropertyName("parts");json.WriteStartArray();
            json.WriteStartObject();json.WritePropertyName("inlineData");json.WriteStartObject();json.WriteString("mimeType","audio/wav");json.WriteBase64String("data",wave);json.WriteEndObject();json.WriteEndObject();
            json.WriteStartObject();json.WriteString("text","Understand the spoken question and give a brief answer, rather than a transcription.");json.WriteEndObject();json.WriteEndArray();json.WriteEndObject();json.WriteEndArray();
            json.WritePropertyName("generationConfig");json.WriteStartObject();json.WriteNumber("candidateCount",1);json.WriteNumber("maxOutputTokens",MaxOutputTokens);json.WriteString("responseMimeType","text/plain");
            json.WritePropertyName("responseModalities");json.WriteStartArray();json.WriteStringValue("TEXT");json.WriteEndArray();json.WriteEndObject();
            json.WritePropertyName("tools");json.WriteStartArray();json.WriteEndArray();json.WriteEndObject();
        }
        return checked((int)memory.Length);
    }
    private static AudioAnswerException StatusFailure(int status)=>new(status switch {
        429=>AudioAnswerFailure.Quota,401 or 403=>AudioAnswerFailure.Credential,_=>AudioAnswerFailure.Provider
    });
    private static string Parse(ReadOnlyMemory<byte> bytes)
    {
        if(bytes.Length==0)throw new AudioAnswerException(AudioAnswerFailure.Empty);
        try {
            _=new UTF8Encoding(false,true).GetCharCount(bytes.Span);
            using var doc=JsonDocument.Parse(bytes,new JsonDocumentOptions{MaxDepth=16});var root=doc.RootElement;
            Unique(root);if(root.ValueKind!=JsonValueKind.Object)throw new JsonException();
            if(root.TryGetProperty("error",out var error)) {
                var code=error.GetProperty("code").GetInt32();throw StatusFailure(code);
            }
            if(root.TryGetProperty("promptFeedback",out var feedback)&&feedback.TryGetProperty("blockReason",out var block)&&block.GetString() is {} why&&why!="BLOCK_REASON_UNSPECIFIED")throw new AudioAnswerException(AudioAnswerFailure.Blocked);
            if(!root.TryGetProperty("candidates",out var candidates)||candidates.ValueKind==JsonValueKind.Array&&candidates.GetArrayLength()==0)throw new AudioAnswerException(AudioAnswerFailure.Empty);
            if(candidates.ValueKind!=JsonValueKind.Array||candidates.GetArrayLength()!=1)throw new JsonException();var candidate=candidates[0];
            if(candidate.TryGetProperty("safetyRatings",out var ratings)) {
                if(ratings.ValueKind!=JsonValueKind.Array)throw new JsonException();
                foreach(var rating in ratings.EnumerateArray())if(rating.TryGetProperty("blocked",out var blocked)&&blocked.GetBoolean())throw new AudioAnswerException(AudioAnswerFailure.Blocked);
            }
            var finish=candidate.GetProperty("finishReason").GetString();
            if(finish is "SAFETY" or "RECITATION" or "BLOCKLIST" or "PROHIBITED_CONTENT" or "SPII" or "IMAGE_SAFETY")throw new AudioAnswerException(AudioAnswerFailure.Blocked);
            if(finish is not ("STOP" or "MAX_TOKENS"))throw new AudioAnswerException(AudioAnswerFailure.Provider);
            if(!candidate.TryGetProperty("content",out var content))throw new AudioAnswerException(AudioAnswerFailure.Empty);
            if(content.TryGetProperty("role",out var role)&&role.GetString()!="model")throw new JsonException();
            var parts=content.GetProperty("parts");if(parts.ValueKind!=JsonValueKind.Array||parts.GetArrayLength()>16)throw new JsonException();var text=new StringBuilder();
            foreach(var part in parts.EnumerateArray()) {
                if(part.TryGetProperty("functionCall",out _)||part.TryGetProperty("executableCode",out _)||part.TryGetProperty("codeExecutionResult",out _)||part.TryGetProperty("inlineData",out _)||part.TryGetProperty("fileData",out _))throw new JsonException();
                if(part.TryGetProperty("thought",out var thought)&&thought.GetBoolean())continue;
                var next=part.GetProperty("text").GetString()??throw new JsonException();if(text.Length+next.Length>4096)throw new JsonException();text.Append(next);
            }
            var clean=new StringBuilder(256);var space=false;
            foreach(var c in text.ToString()) {
                if(char.IsWhiteSpace(c)){space=clean.Length>0;continue;}
                if(char.IsControl(c)||char.IsSurrogate(c)||char.GetUnicodeCategory(c)==UnicodeCategory.Format)continue;
                if(space&&clean.Length<256)clean.Append(' ');space=false;if(clean.Length==256)break;clean.Append(c);
            }
            var result=clean.ToString().Trim();if(result.Length==0)throw new AudioAnswerException(AudioAnswerFailure.Empty);return result;
        }
        catch(AudioAnswerException){throw;}
        catch(Exception e)when(e is JsonException or InvalidOperationException or KeyNotFoundException or FormatException or DecoderFallbackException){throw new AudioAnswerException(AudioAnswerFailure.Malformed);}
    }
    private static void Unique(JsonElement value)
    {
        if(value.ValueKind==JsonValueKind.Object) {
            var names=new HashSet<string>(StringComparer.Ordinal);foreach(var item in value.EnumerateObject()){if(!names.Add(item.Name))throw new JsonException();Unique(item.Value);}
        } else if(value.ValueKind==JsonValueKind.Array)foreach(var item in value.EnumerateArray())Unique(item);
    }
}
