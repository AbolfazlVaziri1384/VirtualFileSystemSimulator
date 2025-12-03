using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static VirtualFileSystemSimulatorWinForm.Form1;
using Newtonsoft.Json;

namespace VirtualFileSystemSimulatorWinForm
{
    public class Directory : Node
    {
        public List<Node> Children { get; set; }
        public Directory(string name, Node parent = null, string timestamp = null, string permissions = "rwxr-xr-x", string owner = "admin", string group = "admin") : base(name,false,null, timestamp, permissions, owner, group)
        {
            Children = new List<Node>();
            Parent = parent;
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

        public override Dictionary<string, object> ToDict()
        {
            var data = base.ToDict();
            data["children"] = Children.Select(c => new { c.Name, Dict = c.ToDict() }).ToDictionary(c => c.Name, c => c.Dict);
            return data;
        }
    }
}