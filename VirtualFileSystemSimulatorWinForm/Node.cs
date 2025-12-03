using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace VirtualFileSystemSimulatorWinForm
{
    public abstract class Node
    {
        public string Name { get; set; }
        public bool IsDir { get; set; }
        public Node Parent { get; set; }
        public string Timestamp { get; set; }
        public string Permissions { get; set; }
        public string Owner { get; set; }
        public string Group { get; set; }

        protected Node(string name, bool isDir = false, Node parent = null, string timestamp = null, string permissions = "rwxr-xr-x", string owner = "admin", string group = "admin")
        {
            Name = name;
            IsDir = isDir;
            Parent = parent;
            Timestamp = timestamp ?? DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            Permissions = permissions;
            Owner = owner;
            Group = group;
        }

        public virtual Dictionary<string, object> ToDict()
        {
            var data = new Dictionary<string, object>
            {
                { "name", Name },
                { "is_dir", IsDir },
                { "timestamp", Timestamp },
                { "permissions", Permissions },
                { "owner", Owner },
                { "group", Group }
            };
            return data;
        }

        public static Node FromDict(Dictionary<string, object> data, Node parent = null)
        {
            bool isDir = (bool)data["is_dir"];
            if (isDir)
            {
                // Assuming Directory subclass exists
                var node = new Directory((string)data["name"], parent, (string)data["timestamp"], (string)data["permissions"], (string)data["owner"], (string)data["group"]);
                var childrenDict = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>((string)JsonConvert.SerializeObject(data["children"]));
                foreach (var child in childrenDict)
                {
                    node.Children[int.Parse(child.Key)] = FromDict(child.Value, node);
                }
                return node;
            }
            else
            {
                // Assuming File subclass exists
                return new File((string)data["name"], parent, (string)data["timestamp"], (string)data["permissions"], (string)data["owner"], (string)data["group"], data.ContainsKey("content") ? (string)data["content"] : "");
            }
        }
    }
}