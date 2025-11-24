using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VirtualFileSystemSimulatorWinForm.Form1;

namespace VirtualFileSystemSimulatorWinForm
{
    public class File : Node
    {

        public string Content { get; set; }
        public string FileType { get; set; }
        public bool IsLink { get; set; } = false;
        public Node Link { get; set; }

        public File(string name, string fileType = "txt", string content = "", bool islink = false, Node link = null) : base(name)
        {
            Content = content;
            FileType = fileType;
            IsLink = islink;
            Link = link;
        }

        public int Size => Content.Length;
    }


}
