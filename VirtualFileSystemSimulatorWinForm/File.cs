using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static VirtualFileSystemSimulatorWinForm.Form1;
using Newtonsoft.Json;

namespace VirtualFileSystemSimulatorWinForm
{
    public class File : Node
    {
        public string Content { get; set; }
        public string FileType { get; set; }
        public bool IsLink { get; set; } = false;
        public Node Link { get; set; }

        public File(string name, Node parent = null, string timestamp = null, string permissions = "rw-r--r--", string owner = "admin", string group = "admin", string fileType = "txt", string content = "", bool islink = false, Node link = null) : base(name, false, parent, timestamp, permissions, owner, group)
        {
            Content = content;
            FileType = fileType;
            IsLink = islink;
            Link = link;
        }

        public int Size => Content.Length;

        public override Dictionary<string, object> ToDict()
        {
            var data = base.ToDict();
            data["content"] = Content;
            data["file_type"] = FileType;
            data["is_link"] = IsLink;
            if (IsLink && Link != null)
            {
                data["link"] = Link.ToDict();
            }
            return data;
        }
    }
}