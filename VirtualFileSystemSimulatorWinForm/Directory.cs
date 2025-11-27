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

        // اضافه کردن فرزند
        public void AddChild(Node child)
        {
            Children.Add(child);
        }

        // پیدا کردن فرزند بر اساس نام
        public Node FindChild(string name)
        {
            return Children.FirstOrDefault(child => child.Name == name);
        }
        // تعداد فرزندان
        public int CountChild()
        {
            return Children.Count;
        }
        // نام تکراری ؟
        public bool IsNameExistChild(string name)
        {
            return Children.Any(child => child.Name == name);
        }
        // آیا فرزندی دارد؟
        public bool HasChild()
        {
            return Children.Count != 0;
        }

        // حذف فرزند
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
