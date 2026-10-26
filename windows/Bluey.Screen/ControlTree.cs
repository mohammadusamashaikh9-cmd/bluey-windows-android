namespace Bluey.Screen;
// Reading-only interface: no value/text retrieval, focus, invoke or input capability exists here.
public interface IControlNode
{
    bool Secure{get;}bool Offscreen{get;}Box Bounds{get;}string? Role{get;}string Name{get;}
    IEnumerable<IControlNode> Children{get;}
}
public sealed record ControlReadout(Control[] Controls,bool TimedOut,int Visited);
public static class ControlTree
{
    public static ControlReadout Read(IControlNode root,Box selected,Func<long> clock,CancellationToken stop)
    {
        var start=clock();var queue=new Queue<(IControlNode Node,int Depth)>();queue.Enqueue((root,0));var controls=new List<Control>();var seen=new HashSet<Box>();int visited=0;
        bool More()=>!stop.IsCancellationRequested&&clock()-start<450&&visited<5000;
        while(queue.Count>0&&More()&&controls.Count<140) {
            var (node,depth)=queue.Dequeue();visited++;
            try {
                if(node.Secure||node.Offscreen||!More())continue;
                var bounds=node.Bounds;if(!bounds.Valid||bounds.Right<=selected.X||bounds.X>=selected.Right||bounds.Bottom<=selected.Y||bounds.Y>=selected.Bottom)continue;
                var role=node.Role;if(role!=null&&selected.Contains(bounds)&&bounds.Width>3&&bounds.Height>3&&More()) {
                    var name=Targets.Clean(node.Name,128);if(name.Length>0&&seen.Add(bounds))controls.Add(new(role,name,bounds,true,false));
                }
                if(depth>=10||role=="menu item"||!More())continue;
                foreach(var child in node.Children){if(queue.Count>=512||!More())break;queue.Enqueue((child,depth+1));}
            } catch(InvalidOperationException){}catch(System.Runtime.InteropServices.COMException){}
        }
        return new(controls.ToArray(),stop.IsCancellationRequested||clock()-start>=450,visited);
    }
}
