using System.Security.Cryptography;
using System.Text.Json;
namespace Bluey.Link;

public enum VoiceProvider { OfflineSimulation, GeminiFree, OpenAIRealtime }
// Application boundary only. No provider SDK, transport credentials or network work in the reducer.
public interface IAudioAnswers
{
    bool ContractVerified { get; }
    Task<string> Answer(ReadOnlyMemory<byte> pcm, CancellationToken stop);
}
public sealed class UnavailableAudioAnswers : IAudioAnswers
{
    public bool ContractVerified => false;
    public Task<string> Answer(ReadOnlyMemory<byte> pcm,CancellationToken stop) => throw new InvalidOperationException("Provider contract unverified");
}
public static class AudioProtocol
{
    public const int ChunkBytes=960, MaxChunks=250, MaxBytes=240000;
    public const string Format="pcm_s16le_24000_mono";
    public static readonly string[] Types=["audioGrant","audioBegin","audioChunk","audioEnd","audioCancel","audioReply","audioDone"];
    public static void Validate(string type,JsonElement p)
    {
        LiveProtocol.Id(p,"authorizationId");
        if(type=="audioGrant") {
            Wire.Keys(p,"authorizationId","consentId","provider","sessionSeconds");LiveProtocol.Id(p,"consentId");
            if(Wire.Text(p,"provider")!="GeminiFree"||Wire.Integer(p,"sessionSeconds") is <1 or >60)throw new ProtocolException("audio grant");return;
        }
        LiveProtocol.Id(p,"turnId");
        switch(type) {
        case "audioBegin": Wire.Keys(p,"authorizationId","turnId","format");if(Wire.Text(p,"format")!=Format)throw new ProtocolException("audio format");break;
        case "audioChunk":
            Wire.Keys(p,"authorizationId","turnId","index","data");if(Wire.Integer(p,"index")>=MaxChunks)throw new ProtocolException("audio index");
            var data=Decode(p);CryptographicOperations.ZeroMemory(data);break;
        case "audioEnd":
            Wire.Keys(p,"authorizationId","turnId","chunks","bytes");
            var chunks=Wire.Integer(p,"chunks");if(chunks is <5 or >MaxChunks||Wire.Integer(p,"bytes")!=chunks*ChunkBytes)throw new ProtocolException("audio length");break;
        case "audioCancel": case "audioDone": Wire.Keys(p,"authorizationId","turnId");break;
        case "audioReply":
            Wire.Keys(p,"authorizationId","turnId","text");var text=Wire.Text(p,"text",256);
            if(text.Any(char.IsSurrogate))throw new ProtocolException("audio text");break;
        default:throw new ProtocolException("audio command");
        }
    }
    public static byte[] Decode(JsonElement p)
    {
        var text=Wire.Text(p,"data",1280);byte[] bytes;
        try{bytes=Convert.FromBase64String(text);}catch(FormatException){throw new ProtocolException("audio encoding");}
        if(bytes.Length!=ChunkBytes||Convert.ToBase64String(bytes)!=text){CryptographicOperations.ZeroMemory(bytes);throw new ProtocolException("audio encoding");}return bytes;
    }
}
// Exactly one current turn. No disk persistence and no audio-bearing ToString.
public sealed class AudioTurn : IDisposable
{
    private readonly byte[] bytes=new byte[AudioProtocol.MaxBytes];private int count;private bool closed;
    public string Id {get;} public long Started {get;}
    public AudioTurn(string id,long now){Id=id;Started=now;}
    public void Append(JsonElement p,long now)
    {
        Check(now);if(Wire.Text(p,"turnId")!=Id||Wire.Integer(p,"index")!=count||count>=AudioProtocol.MaxChunks)throw new ProtocolException("audio order");
        var next=AudioProtocol.Decode(p);try{next.CopyTo(bytes,count*AudioProtocol.ChunkBytes);count++;}finally{CryptographicOperations.ZeroMemory(next);}
    }
    public byte[] Complete(JsonElement p,long now)
    {
        Check(now);if(Wire.Text(p,"turnId")!=Id||count<5||Wire.Integer(p,"chunks")!=count||Wire.Integer(p,"bytes")!=count*AudioProtocol.ChunkBytes)throw new ProtocolException("audio incomplete");
        var result=bytes.AsSpan(0,count*AudioProtocol.ChunkBytes).ToArray();Dispose();return result;
    }
    private void Check(long now){if(closed||now<Started||now-Started>8000)throw new ProtocolException("audio timeout");}
    public bool Expired(long now)=>now-Started>8000;
    public void Dispose(){CryptographicOperations.ZeroMemory(bytes);count=0;closed=true;}
    public override string ToString()=>"[audio turn redacted]";
}
