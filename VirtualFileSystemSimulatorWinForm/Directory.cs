using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static VirtualFileSystemSimulatorWinForm.Form1;

namespace VirtualFileSystemSimulatorWinForm
{
    public class Directory : Node
    {
        public List<Node> Children { get; set; }
        public Directory Parent { get; set; }

        public Directory(string name, Directory parent = null) : base(name)
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

    }

}
