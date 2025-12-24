using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualFileSystemSimulatorWinForm
{
    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public int UserType { get; set; }
        public string Groups { get; set; }

        public enum UserTypeEnum { Owner = 0, Group = 1, Others = 2, Admin = 3 }

        public User(string username, string passwordHash)
        {
            Username = username;
            PasswordHash = passwordHash;

            //Everyone enters their own page when they log in, so they are the admins of that section and there is no need for additional checks.
            UserType = 3;

            Groups = string.Empty;
        }
        public bool HasPermission(Node node, string action) // action: "r", "w", "x"
        {
            if (node == null || string.IsNullOrEmpty(node.Permissions))
                return false;

            if (this.UserType == (int)User.UserTypeEnum.Admin)
                return true;

            string _Permission = node.Permissions;
            switch (action)
            {
                case "w":
                    if (this.UserType == (int)User.UserTypeEnum.Owner && _Permission[1] == 'w') return true;
                    if (this.UserType == (int)User.UserTypeEnum.Group && _Permission[4] == 'w') return true;
                    if (this.UserType == (int)User.UserTypeEnum.Others && _Permission[7] == 'w') return true;
                    break;
                case "r":
                    if (this.UserType == (int)User.UserTypeEnum.Owner && _Permission[0] == 'r') return true;
                    if (this.UserType == (int)User.UserTypeEnum.Group && _Permission[3] == 'r') return true;
                    if (this.UserType == (int)User.UserTypeEnum.Others && _Permission[6] == 'r') return true;
                    break;
                case "x":
                    if (this.UserType == (int)User.UserTypeEnum.Owner && _Permission[2] == 'x') return true;
                    if (this.UserType == (int)User.UserTypeEnum.Group && _Permission[5] == 'x') return true;
                    if (this.UserType == (int)User.UserTypeEnum.Others && _Permission[8] == 'x') return true;
                    break;
                default:
                    break;
            }
            return false;
        }

        public bool IsAdmin()
        {
            if (this == null) return false;
            if (this.UserType == (int)User.UserTypeEnum.Admin) return true;
            return false;
        }
    }
}
