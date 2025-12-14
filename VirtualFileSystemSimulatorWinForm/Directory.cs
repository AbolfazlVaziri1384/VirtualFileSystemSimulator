
using System.Collections.Generic;
using System.Linq;

public class Directory : Node
{
    public List<Node> Children { get; set; }

    public Directory(string name, Node parent = null, string timestamp = null,
                     string permissions = "rwxr-xr-x", string owner = "admin", string group = "admin")
        : base(name, true, parent, timestamp, permissions, owner, group)
    {
        Children = new List<Node>();
    }
    // Add child
    public void AddChild(Node child)
    {
        Children.Add(child);
    }
    //Find child by Name
    public Node FindChild(string name)
    {
        return Children.FirstOrDefault(child => child.Name == name);
    }
    // Count of node's child
    public int CountChild()
    {
        return Children.Count;
    }
    public bool IsExistChildName(string name)
    {
        return Children.Any(child => child.Name == name);
    }
    public bool HasChild()
    {
        return Children.Count != 0;
    }
    public bool RemoveChild(string name)
    {
        var child = FindChild(name);
        if (child != null)
        {
            Children.Remove(child);
            return true;
        }
        return false;
    }
    public override Dictionary<string, object> ToDictBase()
    {
        var dict = base.ToDictBase();
        dict["children"] = Children.Select(c => c.ToDictBase()).ToList();
        return dict;
    }
}
