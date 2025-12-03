using System;
using System.Collections.Generic;
using System.Text.Json;

public abstract class Node
{
    public string Name { get; set; }
    public bool IsDir { get; set; }
    public Node Parent { get; set; }
    public string Timestamp { get; set; }
    public string Permissions { get; set; }
    public string Owner { get; set; }
    public string Group { get; set; }

    protected Node(string name, bool isDir = false, Node parent = null, string timestamp = null,
                   string permissions = "rwxr-xr-x", string owner = "admin", string group = "admin")
    {
        Name = name;
        IsDir = isDir;
        Parent = parent;
        Timestamp = timestamp ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        Permissions = permissions;
        Owner = owner;
        Group = group;
    }

    public virtual Dictionary<string, object> ToDictBase()
    {
        var dict = new Dictionary<string, object>()
        {
            {"name", Name},
            {"timestamp", Timestamp},
            {"permissions", Permissions},
            {"owner", Owner},
            {"group", Group},
            {"is_dir", IsDir}
        };

        return dict;
    }
    public static Node FromDict(Dictionary<string, object> data, Node parent = null)
    {
        string GetString(object value)
        {
            if (value == null) return null;
            if (value is JsonElement e)
            {
                switch (e.ValueKind)
                {
                    case JsonValueKind.String: return e.GetString();
                    case JsonValueKind.Number: return e.GetRawText();
                    case JsonValueKind.True: return "true";
                    case JsonValueKind.False: return "false";
                    default: return e.GetRawText();
                }
            }
            return value.ToString();
        }

        bool GetBool(object value)
        {
            if (value is bool b) return b;
            if (value is JsonElement e)
            {
                if (e.ValueKind == JsonValueKind.True) return true;
                if (e.ValueKind == JsonValueKind.False) return false;
                return bool.Parse(e.GetRawText());
            }
            return bool.Parse(GetString(value));
        }

        bool isDir = GetBool(data["is_dir"]);

        if (isDir)
        {
            var node = new Directory(
                GetString(data["name"]),
                parent,
                GetString(data["timestamp"]),
                GetString(data["permissions"]),
                GetString(data["owner"]),
                GetString(data["group"])
            );

            if (data.ContainsKey("children"))
            {
                if (data["children"] is JsonElement je && je.ValueKind == JsonValueKind.Array)
                {
                    foreach (var childElem in je.EnumerateArray())
                    {
                        var childDict = JsonSerializer.Deserialize<Dictionary<string, object>>(childElem.GetRawText());
                        node.Children.Add(FromDict(childDict, node));
                    }
                }
                else if (data["children"] is List<object> listObj)
                {
                    foreach (var child in listObj)
                    {
                        var childDict = child as Dictionary<string, object>;
                        if (childDict != null)
                            node.Children.Add(FromDict(childDict, node));
                    }
                }
            }

            return node;
        }
        else
        {
            return new File(
                GetString(data["name"]),
                parent,
                GetString(data["timestamp"]),
                GetString(data["permissions"]),
                GetString(data["owner"]),
                GetString(data["group"]),
                GetString(data.ContainsKey("file_type") ? data["file_type"] : "txt"),
                GetString(data.ContainsKey("content") ? data["content"] : ""),
                data.ContainsKey("is_link") ? GetBool(data["is_link"]) : false,
                GetString(data.ContainsKey("link") ? data["link"] : null)
            );
        }
    }

}
