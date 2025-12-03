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
                Register("admin", "admin", true);
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

        public bool Register(string username, string password, bool isAdmin = false)
        {
            if (_users.ContainsKey(username))
            {
                return false; // کاربر وجود دارد
            }
            using (SHA256 sha256 = SHA256.Create())
            {
                var passwordHash = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(password))).Replace("-", "");
                var newUser = new User(username, passwordHash, isAdmin);
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

        // لود VFS برای کاربر جاری (فایل جداگانه برای هر کاربر)
        public Node LoadVfsForCurrentUser(SystemFile fs)
        {
            if (CurrentUser == null) return null;
            string userVfsFile = $"vfs_{CurrentUser.Username}.json";
            if (!System.IO.File.Exists(userVfsFile)) return new Directory("/", null, null, "rwxr-xr-x", CurrentUser.Username, "admin"); // VFS جدید اگر نبود

            var content = System.IO.File.ReadAllBytes(userVfsFile);
            var parts = Encoding.UTF8.GetString(content).Split('|', ((char)StringSplitOptions.RemoveEmptyEntries));
            if (parts.Length != 2)
            {
                throw new Exception("Invalid file format");
            }
            var encrypted = Encoding.UTF8.GetBytes(parts[0]);
            var hashValue = parts[1];
            var jsonBytes = fs.SimpleDecrypt(encrypted); // فرض بر استفاده از متد SimpleDecrypt از SystemFile
            using (SHA256 sha256 = SHA256.Create())
            {
                var calculatedHash = BitConverter.ToString(sha256.ComputeHash(jsonBytes)).Replace("-", "");
                if (calculatedHash != hashValue)
                {
                    throw new Exception("Data tampered! File may have been modified.");
                }
            }
            var data = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);
            return Node.FromDict(data);
        }

        // ذخیره VFS برای کاربر جاری
        public void SaveVfsForCurrentUser(SystemFile fs, Node root)
        {
            if (CurrentUser == null) return;
            string userVfsFile = $"vfs_{CurrentUser.Username}.json";
            var data = root.ToDict();
            var jsonData = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
            var jsonBytes = Encoding.UTF8.GetBytes(jsonData);
            var encrypted = fs.SimpleEncrypt(jsonBytes); // فرض بر استفاده از متد SimpleEncrypt از SystemFile
            using (SHA256 sha256 = SHA256.Create())
            {
                var hashValue = BitConverter.ToString(sha256.ComputeHash(jsonBytes)).Replace("-", "");
                var finalData = Encoding.UTF8.GetString(encrypted) + "|" + hashValue;
                System.IO.File.WriteAllText(userVfsFile, finalData);
            }
        }
    }


}