using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace VirtualFileSystemSimulatorWinForm
{
    public class Json
    {
        private readonly string JsonFolderPath = "VFS_JsonFiles";
        private readonly string UsersFile = "users.json";
        private Dictionary<string, User> Users = new Dictionary<string, User>();
        public User CurrentUser { get; private set; }

        public Json()
        {
            if (!System.IO.Directory.Exists(JsonFolderPath))
            {
                System.IO.Directory.CreateDirectory(JsonFolderPath);
            }

            LoadUsers();
            if (!Users.ContainsKey("admin"))
            {
                Register("admin", "admin");
            }
        }

        // For make path with file name
        private string GetFullPath(string filename)
        {
            return Path.Combine(JsonFolderPath, filename);
        }

        private void LoadUsers()
        {
            string _FullPath = GetFullPath(UsersFile);
            if (System.IO.File.Exists(_FullPath))
            {
                var _Json = System.IO.File.ReadAllText(_FullPath);
                var _UsersList = JsonSerializer.Deserialize<List<User>>(_Json);
                foreach (var user in _UsersList)
                {
                    Users[user.Username] = user;
                }
            }
        }

        private void SaveUsers()
        {
            var json = JsonSerializer.Serialize(Users.Values.ToList());
            string fullPath = GetFullPath(UsersFile);
            System.IO.File.WriteAllText(fullPath, json);
        }

        public bool UserIsExist(string username)
        {
            if (Users.ContainsKey(username))
            {
                return true;
            }
            else
            { return false; }
        }
        public bool Register(string username, string password)
        {
            if (Users.ContainsKey(username))
            {
                return false;
            }
            using (SHA256 sha256 = SHA256.Create())
            {
                var passwordHash = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "");
                var newUser = new User(username, passwordHash);
                Users[username] = newUser;
                SaveUsers();
                return true;
            }
        }
        public bool Login(string username, string password)
        {
            if (Users.TryGetValue(username, out var user))
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    var passwordHash = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "");
                    if (user.PasswordHash == passwordHash)
                    {
                        CurrentUser = user;
                        return true;
                    }
                }
            }
            return false;
        }
        public bool ChangeUserType(string username, User.UserTypeEnum newusertype)
        {
            bool _IsDone = false;
            if (Users.ContainsKey(username))
            {
                string[,] _UsersAdnGroups = new string[Users[username].Groups.Split('/').ToList().Count(), 2];
                for (int i = 0; i < Users[username].Groups.Split('/').ToList().Count(); i++)
                {
                    _UsersAdnGroups[i, 0] = Users[username].Groups.Split('/')[i].Split(',')[0];
                    _UsersAdnGroups[i, 1] = Users[username].Groups.Split('/')[i].Split(',')[1];
                }

                for (int i = 0; i < _UsersAdnGroups.GetLength(0); i++)
                {
                    if (_UsersAdnGroups[i, 0] == CurrentUser.Username)
                    {
                        int temp = (int)newusertype;
                        _UsersAdnGroups[i, 1] = temp.ToString();
                        _IsDone = true;
                    }
                }
                List<string> _Upadte = new List<string>();
                for (int i = 0; i < _UsersAdnGroups.GetLength(0); i++)
                {
                    _Upadte.Add(_UsersAdnGroups[i, 0] + ',' + _UsersAdnGroups[i, 1]);
                }

                Users[username].Groups = string.Join("/", _Upadte);
                SaveUsers();
            }
            return _IsDone;
        }
        public void AddToCurrentGroup(string username, User.UserTypeEnum newusertype)
        {
            if (Users.ContainsKey(username))
            {
                string[,] _UsersAdnGroups = new string[Users[username].Groups.Split('/').ToList().Count() + 1, 2];
                int i = 0;
                for (; i < Users[username].Groups.Split('/').ToList().Count(); i++)
                {
                    _UsersAdnGroups[i, 0] = Users[username].Groups.Split('/')[i].Split(',')[0];
                    _UsersAdnGroups[i, 1] = Users[username].Groups.Split('/')[i].Split(',')[0];
                }
                _UsersAdnGroups[i, 0] = CurrentUser.Username;
                int temp = (int)newusertype;
                _UsersAdnGroups[i, 1] = temp.ToString();

                List<string> _Upadte = new List<string>();
                for (int j = 0; j < _UsersAdnGroups.GetLength(0); j++)
                {
                    _Upadte.Add(_UsersAdnGroups[j, 0] + ',' + _UsersAdnGroups[j, 1]);
                }
                Users[username].Groups = string.Join("/", _Upadte);
                SaveUsers();
            }
        }
        public bool RemoveGroup(string username, string groupname)
        {
            bool _IsDone = false;
            if (Users.ContainsKey(username))
            {
                groupname = groupname.ToLower();

                string[,] _UsersAdnGroups = new string[Users[username].Groups.Split('/').ToList().Count(), 2];
                for (int i = 0; i < Users[username].Groups.Split('/').ToList().Count(); i++)
                {
                    _UsersAdnGroups[i, 0] = Users[username].Groups.Split('/')[i].Split(',')[0];
                    _UsersAdnGroups[i, 1] = Users[username].Groups.Split('/')[i].Split(',')[0];
                }

                List<string> _Upadte = new List<string>();
                for (int j = 0; j < _UsersAdnGroups.GetLength(0); j++)
                {
                    if (_UsersAdnGroups[j, 1] == groupname) { _IsDone = true; continue; }
                    _Upadte.Add(_UsersAdnGroups[j, 0] + ',' + _UsersAdnGroups[j, 1]);
                }
                Users[username].Groups = string.Join("/", _Upadte);
                SaveUsers();
            }
            return _IsDone;
        }

        public Node LoadVfsForCurrentUser(string systemname, string commitversion)
        {
            if (CurrentUser == null) return null;
            string userVfsFile = $"vfs_{systemname}_{commitversion}.json";
            string fullPath = GetFullPath(userVfsFile);

            if (!System.IO.File.Exists(fullPath))
                return new Directory("/", null, null, "rwxr-xr-x", CurrentUser.Username, "admin");

            var jsonString = System.IO.File.ReadAllText(fullPath);

            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);

            return Node.FromDict(data);
        }

        public void SaveVfsForCurrentUser(Node root, string systemname, string commitversion)
        {
            if (CurrentUser == null) return;

            string userVfsFile = $"vfs_{systemname}_{commitversion}.json";
            string fullPath = GetFullPath(userVfsFile);

            var data = root.ToDictBase();

            var jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

            System.IO.File.WriteAllText(fullPath, jsonData);
        }

        public bool CopyVfsWithCommit(string systemname, string sourcecommit, string newcommit)
        {
            if (CurrentUser == null)
                return false;

            string _SourceFile = GetFullPath($"vfs_{systemname}_{sourcecommit}.json");
            string _DestinationFile = GetFullPath($"vfs_{systemname}_{newcommit}.json");

            if (!System.IO.File.Exists(_SourceFile))
            {
                var newRoot = new Directory("/", null, null, "rwxr-xr-x", CurrentUser.Username, "admin");
                SaveVfsForCurrentUser(newRoot, systemname, newcommit);
                return true;
            }

            try
            {
                System.IO.File.Copy(_SourceFile, _DestinationFile, overwrite: true);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public List<string> GetAllCommits(string systemname)
        {
            var commits = new List<string>();

            try
            {
                string searchPattern = $"vfs_{systemname}_*.json";
                var files = System.IO.Directory.GetFiles(JsonFolderPath, searchPattern);

                foreach (var file in files)
                {
                    string fileName = System.IO.Path.GetFileNameWithoutExtension(file);
                    string[] parts = fileName.Split('_');

                    if (parts.Length >= 3)
                    {
                        commits.Add(parts[2]);
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return commits;
        }

        public bool DeleteCommit(string systemname, string commit)
        {
            try
            {
                string fileToDelete = GetFullPath($"vfs_{systemname}_{commit}.json");

                if (System.IO.File.Exists(fileToDelete))
                {
                    System.IO.File.Delete(fileToDelete);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}