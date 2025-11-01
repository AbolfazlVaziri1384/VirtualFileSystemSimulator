using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VirtualFileSystemSimulatorWinForm
{
    public abstract class Node
    {
        public string Name { get; set; }
        public DateTime CreationTime { get; set; }
        public string Permissions { get; set; } // اختیاری: مانند "rwxr-xr--"
                                                //توجه اگر خواستی که اجازه را تغییر بدی باید از ورودی ان را بگیری
        protected Node(string name)
        {
            Name = name;
            CreationTime = DateTime.Now;
            Permissions = "rwxr-xr--"; // مقدار پیش‌فرض
        }
    }
}
