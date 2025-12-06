using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace VirtualFileSystemSimulatorWinForm
{
    public class Json
    {
        private readonly string _usersFile = "users.json";
        private Dictionary<string, User> _users = new Dictionary<string, User>();
        public User CurrentUser { get; private set; }

        public Json()
        {
            LoadUsers();
            if (!_users.ContainsKey("admin"))
            {
                Register("admin", "admin", User.UserTypeEnum.Admin);
            }
        }

        private void LoadUsers()
        {
            if (System.IO.File.Exists(_usersFile))
            {
                var json = System.IO.File.ReadAllText(_usersFile);
                var usersList = JsonSerializer.Deserialize<List<User>>(json);
                foreach (var user in usersList)
                {
                    _users[user.Username] = user;
                }
            }
        }

        private void SaveUsers()
        {
            var json = JsonSerializer.Serialize(_users.Values.ToList());
            System.IO.File.WriteAllText(_usersFile, json);
        }

        public bool UserIsExist(string username)
        {
            if (_users.ContainsKey(username))
            {
                return true; // کاربر وجود دارد
            }
            else
                { return false; }
        }
        public bool Register(string username, string password , User.UserTypeEnum userType)
        {
            if (_users.ContainsKey(username))
            {
                return false; // کاربر وجود دارد
            }
            using (SHA256 sha256 = SHA256.Create())
            {
                var passwordHash = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "");
                var newUser = new User(username, passwordHash , (int)userType);
                _users[username] = newUser;
                SaveUsers();
                return true;
            }
        }
        public bool Login(string username, string password)
        {
            if (_users.TryGetValue(username, out var user))
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
        public bool ChangeUserType(string username , User.UserTypeEnum newusertype)
        {
            if (_users.ContainsKey(username))
            {
                _users[username].UserType = (int)newusertype;
                SaveUsers();
                return true;
            }
            return false;
        }
        public void AddToCurrentGroup(string username)
        {
            if (_users.ContainsKey(username))
            {
                var _groups = _users[username].Groups.Split(',').ToList();
                _groups.Add(CurrentUser.Username.ToLower());
                _users[username].Groups = string.Join(",", _groups);
                SaveUsers();
            }
        }
        public void RemoveGroup(string username,string groupname)
        {
            if (_users.ContainsKey(username))
            {
                groupname = groupname.ToLower();
                var _groups = _users[username].Groups.Split(',').ToList();
                _groups.Remove(groupname);
                _users[username].Groups = string.Join(",", _groups);
                SaveUsers();
            }
        }

        // لود VFS برای کاربر جاری (فایل جداگانه برای هر کاربر)
        public Node LoadVfsForCurrentUser(string systemname)
        {
            if (CurrentUser == null) return null;
            string userVfsFile = $"vfs_{systemname}.json";

            // اگر فایل وجود نداشت، VFS جدید ایجاد کن
            if (!System.IO.File.Exists(userVfsFile))
                return new Directory("/", null, null, "rwxr-xr-x", CurrentUser.Username, "admin");

            var jsonString = System.IO.File.ReadAllText(userVfsFile);

            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonString);

            return Node.FromDict(data);
        }



        // ذخیره VFS برای کاربر جاری
        public void SaveVfsForCurrentUser(Node root , string systemname)
        {
            if (CurrentUser == null) return;

            string userVfsFile = $"vfs_{systemname}.json";

            // ساخت دیکشنری از Nodeها
            var data = root.ToDictBase();

            // Serialize با WriteIndented تا خوانا باشد
            var jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

            System.IO.File.WriteAllText(userVfsFile, jsonData);
        }


    }


}