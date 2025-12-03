using System.Collections.Generic;

public class File : Node
{
    public string Content { get; set; }
    public string FileType { get; set; }
    public bool IsLink { get; set; }
    public string Link { get; set; } // مسیر لینک به صورت رشته

    public File(string name, Node parent = null, string timestamp = null,
                string permissions = "rw-r--r--", string owner = "admin", string group = "admin",
                string fileType = "txt", string content = "", bool isLink = false, string link = null)
        : base(name, false, parent, timestamp, permissions, owner, group)
    {
        Content = content;
        FileType = fileType;
        IsLink = isLink;
        Link = link;
    }
    public int Size => Content.Length;

    public override Dictionary<string, object> ToDictBase()
    {
        var dict = base.ToDictBase();
        dict["content"] = Content;
        dict["file_type"] = FileType;
        dict["is_link"] = IsLink;
        dict["link"] = Link;
        return dict;
    }
}

