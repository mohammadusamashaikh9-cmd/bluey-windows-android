using System.Text.Json;
using Xunit;
namespace Bluey.Link.Tests;
public sealed class PresentationTests
{
    private const string Turn="30000000-0000-0000-0000-000000000001";
    private static Message Event(string state,string kind="state",string text="",string turn=Turn)=>Wire.Decode(Wire.Encode("presentation",Guid.NewGuid().ToString(),0,new{turnId=turn,state,@event=kind,text})[..^1]);
    private static void Speaking(CaptionSession s){s.Apply(Event("waking"),0);s.Apply(Event("listening"),600);s.Apply(Event("asking",turn:"30000000-0000-0000-0000-000000000002"),700);}
    [Fact] public void CaptionLifecycleMatchesSourceReadingTimeAndClearsOnSleep() {
        var s=new CaptionSession();s.Apply(Event("waking"),0);s.Apply(Event("listening"),600);s.Apply(Event("asking"),900);s.Apply(Event("thinking"),1000);s.Apply(Event("speaking"),1700);
        s.Apply(Event("speaking","delta","Offline "),1701);s.Apply(Event("speaking","delta","Bluey"),1800);s.Apply(Event("speaking","done"),2000);s.Apply(Event("listening"),2001);
        Assert.Equal("Offline Bluey",s.Sample(7799).Caption);Assert.Null(s.Sample(7800).Caption);
        Assert.Equal(5800,CaptionSession.ReadMs("short"));Assert.Equal(15000,CaptionSession.ReadMs(string.Join(" ",Enumerable.Repeat("word",80))));
        s.Apply(Event("asleep","cancel"),8000);Assert.Null(s.Sample(8000).Caption);Assert.Throws<ProtocolException>(()=>s.Apply(Event("speaking","delta","late"),8001));
    }
    [Fact] public void IllegalTransitionsOversizedCaptionsAndRepeatedCompletionsFailClosed() {
        var s=new CaptionSession();Assert.Throws<ProtocolException>(()=>s.Apply(Event("speaking","delta","x"),0));
        s=new();s.Apply(Event("waking"),0);Assert.Throws<ProtocolException>(()=>s.Apply(Event("speaking"),1));
        s=new();s.Apply(Event("waking"),0);s.Apply(Event("thinking"),1);s.Apply(Event("speaking"),2);
        for(int i=0;i<8;i++)s.Apply(Event("speaking","delta",new string('x',256)),3+i);
        Assert.Throws<ProtocolException>(()=>s.Apply(Event("speaking","delta","x"),12));
        s.Apply(Event("speaking","done"),13);Assert.Throws<ProtocolException>(()=>s.Apply(Event("speaking","done"),14));
    }
    [Fact] public void OwnershipDisconnectStaleCancellationAndInterruptedStreamAreBounded() {
        var h=new PresentationHub();h.Apply("old",Event("waking"),0);h.Release("old");h.Apply("new",Event("waking"),1);h.Release("old");
        Assert.Equal("waking",h.Sample(1).State);Assert.Throws<ProtocolException>(()=>h.Apply("other",Event("waking"),2));
        h.Apply("new",Event("thinking"),3);h.Apply("new",Event("speaking"),4);h.Apply("new",Event("speaking","delta","preview"),5);
        Assert.NotNull(h.Sample(15004).Caption);Assert.Null(h.Sample(15005).Caption);h.Release("new");Assert.Equal("asleep",h.Sample(15006).State);
        var s=new CaptionSession();s.Apply(Event("waking"),0);s.Apply(Event("listening"),1);var next="30000000-0000-0000-0000-000000000002";
        s.Apply(Event("asking",turn:next),2);s.Apply(Event("asleep","cancel"),3);Assert.Equal("asking",s.Sample(3).State);
    }
}
