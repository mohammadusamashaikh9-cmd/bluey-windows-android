using Xunit;
namespace Bluey.Screen.Tests;
public sealed class ControlTreeTests
{
    private sealed class Node:IControlNode {
        public bool Secure{get;init;}public bool Offscreen{get;init;}public Box Bounds{get;init;}=new(0,0,200,100);
        public string? Role{get;init;}="button";public Node[] Nodes=[];public int NameReads,ChildReads;public Action? Read;
        public string Name{get{NameReads++;if(Secure||Offscreen)throw new InvalidOperationException("private field must not be read");Read?.Invoke();return "public label";}}
        public IEnumerable<IControlNode> Children{get{ChildReads++;return Nodes;}}
    }
    [Fact] public void PasswordAndOffscreenSubtreesAreNeverRead() {
        var password=new Node{Secure=true};var hidden=new Node{Offscreen=true};var allowed=new Node{Bounds=new(20,20,60,20)};
        var root=new Node{Role=null,Nodes=[password,hidden,allowed]};var r=ControlTree.Read(root,new(0,0,200,100),()=>0,default);
        Assert.Single(r.Controls);Assert.Equal(0,password.NameReads+password.ChildReads+hidden.NameReads+hidden.ChildReads);Assert.Equal(1,allowed.NameReads);
    }
    [Fact] public void NativeWindowRootCanEncloseTheDwmCaptureWithoutBecomingATarget() {
        var root=new Node{Bounds=new(-8,-8,216,116),Nodes=[new(){Bounds=new(10,10,80,20)}]};
        Assert.Single(ControlTree.Read(root,new(0,0,200,100),()=>0,default).Controls);
    }
    [Fact] public void TraversalHasDeadlineDepthAndQueueBounds() {
        long time=0;var children=Enumerable.Range(0,5000).Select(i=>new Node{Bounds=new(1,i%95+1,4,4),Read=()=>time+=100}).ToArray();
        var r=ControlTree.Read(new Node{Role=null,Nodes=children},new(0,0,200,100),()=>time,default);
        Assert.True(r.TimedOut);Assert.InRange(r.Visited,1,8);Assert.InRange(r.Controls.Length,1,5);
        var bottom=new Node();var current=bottom;for(int i=0;i<30;i++)current=new(){Role=null,Nodes=[current]};
        Assert.Empty(ControlTree.Read(current,new(0,0,200,100),()=>0,default).Controls);Assert.Equal(0,bottom.NameReads);
    }
    [Fact] public void CancellationAndClosedMenusCannotTraverse() {
        var child=new Node();var root=new Node{Role="menu item",Nodes=[child]};var stop=new CancellationTokenSource();stop.Cancel();
        Assert.Empty(ControlTree.Read(root,new(0,0,200,100),()=>0,stop.Token).Controls);
        ControlTree.Read(root,new(0,0,200,100),()=>0,default);Assert.Equal(0,child.NameReads);
    }
}
