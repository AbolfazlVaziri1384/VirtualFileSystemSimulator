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

        public File(string name, string fileType = "txt") : base(name)
        {
            Content = "";
            FileType = fileType;
        }

        public int Size => Content.Length;
    }


}
