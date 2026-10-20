using System.Text;
using System.Text.Json;
using Xunit;
namespace Bluey.Link.Tests;
public sealed class ProtocolTests
{
    [Fact] public void SharedFixturesDecodeWithDeclaredOutcomes()
    {
        var cases=File.ReadAllLines(Path.Combine(AppContext.BaseDirectory,"v1-cases.jsonl")); Assert.Equal(40,cases.Length);
        foreach(var line in cases)
        {
            using var doc=JsonDocument.Parse(line);var root=doc.RootElement;var framer=new Frames();var count=0;var decoded=new List<Message>();
            Exception? error=null;
            try { foreach(var c in root.GetProperty("chunks").EnumerateArray())foreach(var bytes in framer.Feed(Convert.FromBase64String(c.GetString()!))){decoded.Add(Wire.Decode(bytes));count++;} framer.End(); }
            catch(ProtocolException e){error=e;}
            Assert.True(root.GetProperty("valid").GetBoolean() ? error==null && count>0 : error!=null,root.GetProperty("name").GetString());
            if(error==null) {
                var expected=root.GetProperty("expected").EnumerateArray().ToArray();Assert.Equal(expected.Length,decoded.Count);
                for(var i=0;i<expected.Length;i++) {var e=expected[i];var m=decoded[i];
                    Assert.Equal(e.GetProperty("type").GetString(),m.Type);Assert.Equal(e.GetProperty("id").GetString(),m.Id);
                    Assert.Equal(e.GetProperty("session").GetString(),m.Session);Assert.Equal(e.GetProperty("seq").GetInt64(),m.Seq);
                    Assert.True(JsonElement.DeepEquals(e.GetProperty("payload"),m.Payload));
                }
            }
        }
    }
    [Fact] public void PresentationFixturesDecodeWithDeclaredOutcomes()
    {
        var cases=File.ReadAllLines(Path.Combine(AppContext.BaseDirectory,"f4a-cases.jsonl")); Assert.Equal(27,cases.Length);
        foreach(var line in cases)
        {
            using var doc=JsonDocument.Parse(line);var root=doc.RootElement;var framer=new Frames();var count=0;var decoded=new List<Message>();
            Exception? error=null;
            try { foreach(var c in root.GetProperty("chunks").EnumerateArray())foreach(var bytes in framer.Feed(Convert.FromBase64String(c.GetString()!))){decoded.Add(Wire.Decode(bytes));count++;} framer.End(); }
            catch(ProtocolException e){error=e;}
            Assert.True(root.GetProperty("valid").GetBoolean() ? error==null && count>0 : error!=null,root.GetProperty("name").GetString());
            if(error==null) {
                var expected=root.GetProperty("expected").EnumerateArray().ToArray();Assert.Equal(expected.Length,decoded.Count);
                for(var i=0;i<expected.Length;i++) {var e=expected[i];var m=decoded[i];
                    Assert.Equal(e.GetProperty("type").GetString(),m.Type);Assert.Equal(e.GetProperty("id").GetString(),m.Id);
                    Assert.Equal(e.GetProperty("session").GetString(),m.Session);Assert.Equal(e.GetProperty("seq").GetInt64(),m.Seq);
                    Assert.True(JsonElement.DeepEquals(e.GetProperty("payload"),m.Payload));
                }
            }
        }
    }
    [Fact] public void LiveFixturesDecodeWithDeclaredOutcomes()
    {
        var cases=File.ReadAllLines(Path.Combine(AppContext.BaseDirectory,"f4b-cases.jsonl")); Assert.Equal(26,cases.Length);
        foreach(var line in cases)
        {
            using var doc=JsonDocument.Parse(line);var root=doc.RootElement;var framer=new Frames();var count=0;var decoded=new List<Message>();
            Exception? error=null;
            try { foreach(var c in root.GetProperty("chunks").EnumerateArray())foreach(var bytes in framer.Feed(Convert.FromBase64String(c.GetString()!))){decoded.Add(Wire.Decode(bytes));count++;} framer.End(); }
            catch(ProtocolException e){error=e;}
            Assert.True(root.GetProperty("valid").GetBoolean() ? error==null && count>0 : error!=null,root.GetProperty("name").GetString());
            if(error==null) {
                var expected=root.GetProperty("expected").EnumerateArray().ToArray();Assert.Equal(expected.Length,decoded.Count);
                for(var i=0;i<expected.Length;i++) {var e=expected[i];var m=decoded[i];
                    Assert.Equal(e.GetProperty("type").GetString(),m.Type);Assert.Equal(e.GetProperty("id").GetString(),m.Id);
                    Assert.Equal(e.GetProperty("session").GetString(),m.Session);Assert.Equal(e.GetProperty("seq").GetInt64(),m.Seq);
                    Assert.True(JsonElement.DeepEquals(e.GetProperty("payload"),m.Payload));
                }
            }
        }
    }
    [Fact] public void SequenceSessionDuplicatesAndStalenessAreRejected()
    {
        var s=Guid.NewGuid().ToString();var guard=new SequenceGuard(s);
        var m=Wire.Decode(Wire.Encode("ping",s,0,new{})[..^1]);guard.Accept(m);
        Assert.Throws<ProtocolException>(()=>guard.Accept(m));
        var wrong=Wire.Decode(Wire.Encode("ping",Guid.NewGuid().ToString(),1,new{})[..^1]);Assert.Throws<ProtocolException>(()=>guard.Accept(wrong));
        guard.Fresh(new(0,0,"happy",0,100),10);guard.Fresh(new(0,0,"happy",0,200),110);
        Assert.Throws<ProtocolException>(()=>guard.Fresh(new(0,0,"happy",0,201),4000));
    }
    [Fact] public void EnrollmentExpiresConsumesAndCannotCrossGenerations()
    {
        long time=0;var i=new Invitation(()=>time);var secret=i.Start();var generation=i.Generation;
        Assert.True(i.Check(secret));Assert.False(i.Check(new string('x',43)));
        Assert.True(i.Consume(generation));Assert.False(i.Check(secret));
        secret=i.Start();time=300001;Assert.False(i.Check(secret));
        i.Start();generation=i.Generation;i.Start();Assert.False(i.Consume(generation));
        secret=i.Start();for(var n=0;n<8;n++)Assert.False(i.Check(new string('x',43)));Assert.False(i.Check(secret));
    }
    [Fact] public void ChangeThresholdsAndOneSecondRefreshMatchSwift()
    {
        var f=new LinkFace(0,0,"listening",0,0);
        Assert.False(FaceFilter.Significant(f,f with {GazeX=.0039,Talk=.019},0,999));
        Assert.True(FaceFilter.Significant(f,f,0,1000));Assert.True(FaceFilter.Significant(f,f with {GazeX=.004},0,1));
        Assert.True(FaceFilter.Significant(f,f with {Talk=.02},0,1));Assert.True(FaceFilter.Significant(f,f with {Mood="happy"},0,1));
    }
    [Fact] public void MessageRateAndIncompleteFramesAreBounded()
    {
        var rate=new MessageRate();for(var i=0;i<120;i++)rate.Accept(0);
        Assert.Throws<ProtocolException>(()=>rate.Accept(999));rate.Accept(1000);
        Assert.Throws<ProtocolException>(()=>new Frames().Feed(new byte[4097]));
        var frames=new Frames();frames.Feed("{"u8);Assert.Throws<ProtocolException>(frames.End);
    }
}
