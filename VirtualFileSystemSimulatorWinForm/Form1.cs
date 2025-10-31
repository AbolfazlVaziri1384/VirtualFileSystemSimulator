using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace VirtualFileSystemSimulatorWinForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        //برای کل برنامه
        public static FileSystem fs = new FileSystem();
        public bool IsFirstUp = true;
        public Stack<string> FirstAllCommand = new Stack<string>();
        public Stack<string> LastAllCommand = new Stack<string>();
        private void txtCommandLine_KeyDown(object sender, KeyEventArgs e)
        {
            string Input = string.Empty;

            if (e.KeyCode == Keys.Enter)
            {
                // برای جلوگیری از صدای سیستم هنگامی که اینتر را می زنیم
                e.SuppressKeyPress = true;
                Input = txtCommandLine.Text;
                AddToCommandList(Input, rchCommandList);
                AnalizeInput(Input);
                txtCommandLine.Text = string.Empty;
                LastCommands = string.Empty;
                IsFirstUp = true;
            }
            //دستور هایی از قبل
            if (e.KeyCode == Keys.Up)
            {
                // برای جلوگیری از صدای سیستم هنگامی که اینتر را می زنیم
                e.SuppressKeyPress = true;

                try
                {
                    if (IsFirstUp)
                    {
                        FirstAllCommand.Clear();
                        LastAllCommand.Clear();
                        string[] a = rchCommandList.Text.Trim().Split('\n').ToArray();
                        foreach (string s in a)
                        {
                            if (!string.IsNullOrEmpty(s) && s.StartsWith(">"))
                            {
                                FirstAllCommand.Push(s.Substring(2));
                            }
                        }
                        FirstAllCommand.Push(LastCommands);
                    }
                    LastAllCommand.Push(FirstAllCommand.Pop());
                    txtCommandLine.Text = LastAllCommand.Peek();
                    IsFirstUp = false;
                }
                catch (Exception)
                {

                }

            }
            //دستور هایی از قبل
            if (e.KeyCode == Keys.Down)
            {
                // برای جلوگیری از صدای سیستم هنگامی که اینتر را می زنیم
                e.SuppressKeyPress = true;

                try
                {
                    if (!IsFirstUp)
                    {
                        FirstAllCommand.Push(LastAllCommand.Pop());
                        txtCommandLine.Text = FirstAllCommand.Peek();
                    }
                }
                catch (Exception)
                {

                }

            }
            //برای تکمیل خودکار از shift استفاده می کنیم
            if (e.KeyCode == Keys.ShiftKey)
            {
                // برای جلوگیری از صدای سیستم هنگامی که اینتر را می زنیم
                e.SuppressKeyPress = true;

                try
                {
                    ArrayList CommandList = new ArrayList();
                    CommandList.Add("mkdir");
                    CommandList.Add("mkdir -p");
                    foreach (string s in CommandList)
                    {
                        if (s.Contains(txtCommandLine.Text))
                        {
                            txtCommandLine.Text = s;
                            break;
                        }
                    }
                }
                catch (Exception)
                {

                }

            }
        }

        public void AnalizeInput(string Input)
        {
            string[] InputArray = Input.Trim().Split(' ').ToArray();
            switch (InputArray[0])
            {
                case "mkdir":
                    Mkdir_Command(InputArray, rchCommandList, TreeView);
                    UpdateCurrentRoute(fs.CurrentDirectory, txtCurrentRoute);
                    break;
            }

            // ایجاد ساختار
            //fs.CreateDirectory("/home");
            //fs.CreateDirectory("/home/user");
            //fs.ChangeDirectory("/home/user");
            //fs.CreateFile("document.txt");

            // نمایش درخت
            //fs.PrintTree(fs.Root);
        }

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
        public class Directory : Node
        {
            public List<Node> Children { get; set; }
            public Directory Parent { get; set; }

            public Directory(string name, Directory parent = null) : base(name)
            {
                Children = new List<Node>();
                Parent = parent;
            }

            // اضافه کردن فرزند
            public void AddChild(Node child)
            {
                Children.Add(child);
            }

            // پیدا کردن فرزند بر اساس نام
            public Node FindChild(string name)
            {
                return Children.FirstOrDefault(child => child.Name == name);
            }

            // حذف فرزند
            public bool RemoveChild(string name)
            {
                var child = FindChild(name);
                if (child != null)
                {
                    Children.Remove(child);
                    return true;
                }
                return false;
            }

        }
        public class FileSystem
        {
            public Directory Root { get; private set; }
            public Directory CurrentDirectory { get; private set; }

            public FileSystem()
            {
                Root = new Directory("/");
                CurrentDirectory = Root;
            }

            // پیاده‌سازی دستور cd
            public void ChangeDirectory(string path)
            {
                if (string.IsNullOrEmpty(path) || path == "/")
                {
                    CurrentDirectory = Root;
                    return;
                }

                if (path == "..")
                {
                    if (CurrentDirectory.Parent != null)
                        CurrentDirectory = CurrentDirectory.Parent;
                    return;
                }

                var target = ResolvePath(path);
                if (target is Directory directory)
                {
                    CurrentDirectory = directory;
                }
                else
                {
                    throw new Exception($"'{path}' is not a directory");
                }
            }
            // تابع جدید برای مدیریت مسیرهای حاوی .. و .
            private string NormalizePath(string path)
            {
                var parts = path.Split('/').ToList();
                string result = string.Empty;
                bool IsHasPoint = false;
                bool IsFirst = true;
                foreach (var part in parts)
                {
                    if (part == "..")
                    {
                        // به یک سطح بالاتر برو
                        //if (result.Count > 0)
                        //    result.RemoveAt(result.Count - 1);
                        CurrentDirectory = CurrentDirectory.Parent;
                        result = CurrentDirectory.Name + "/" + result;
                        IsHasPoint = true;
                    }
                    else if (part == ".")
                    {
                        // نادیده بگیر - در جای خود بمان
                        result = CurrentDirectory.Name + "/" + result;
                        IsHasPoint = true;
                    }
                    else
                    {
                        if (!IsFirst)
                            result += "/";
                        result = result + part;
                        IsFirst = false;
                    }
                }
                if (IsHasPoint)
                    return "/" + result;
                else
                    return path;

            }
            // پیاده‌سازی دستور mkdir
            public void CreateDirectory(string path, bool createParents, RichTextBox rchCommandLine)
            {
                if (string.IsNullOrEmpty(path))
                    throw new Exception("Path cannot be empty");
                path = NormalizePath(path);
                // جدا کردن مسیر به بخش‌ها
                var parts = path.Split('/').Where(p => !string.IsNullOrEmpty(p)).ToArray();

                if (path.StartsWith("/"))
                {
                    // مسیر مطلق - از ریشه شروع کن
                    CreateDirectoryRecursive(Root, parts, 0, createParents, rchCommandLine);
                }
                else
                {
                    // مسیر نسبی - از دایرکتوری جاری شروع کن
                    CreateDirectoryRecursive(CurrentDirectory, parts, 0, createParents, rchCommandLine);
                }
            }

            private void CreateDirectoryRecursive(Directory current, string[] parts, int index, bool createParents, RichTextBox rchCommandLine)
            {
                // اگر به انتهای مسیر رسیدیم
                if (index >= parts.Length)
                    //اگر اروری برای تکراری بودن نام پوشه در 239 کار نکرد از این استفاده کن
                    //AddToCommandList($"'{currentPart}' already exists as a Directory", rchCommandLine, false);
                    return;

                string currentPart = parts[index];

                // بررسی وجود گره با همین نام
                var existingNode = current.FindChild(currentPart);

                if (existingNode != null)
                {
                    if (existingNode is Directory existingDir)
                    {
                        // دایرکتوری از قبل وجود دارد - به قسمت بعدی برو
                        CreateDirectoryRecursive(existingDir, parts, index + 1, createParents, rchCommandLine);
                        if (index >= parts.Length - 1)
                            AddToCommandList($"'{currentPart}' already exists as a Directory", rchCommandLine, false);
                    }
                    else
                    {
                        // یک فایل با همین نام وجود دارد - خطا
                        AddToCommandList($"'{currentPart}' is already exists as a file", rchCommandLine, false);

                    }
                }
                else
                {
                    if (index == parts.Length - 1)
                    {
                        // این آخرین بخش مسیر است - دایرکتوری جدید ایجاد کن
                        var newDir = new Directory(currentPart, current);
                        current.AddChild(newDir);
                        CurrentDirectory = newDir;
                    }
                    else
                    {
                        if (createParents)
                        {
                            // ایجاد دایرکتوری‌های والد به صورت خودکار
                            var newDir = new Directory(currentPart, current);
                            current.AddChild(newDir);
                            //CurrentDirectory = current;
                            CreateDirectoryRecursive(newDir, parts, index + 1, createParents, rchCommandLine);
                            //CurrentDirectory = current;
                        }
                        else
                        {
                            // دایرکتوری والد وجود ندارد و createParents=false - خط

                            AddToCommandList($"Parent directory '{currentPart}' does not exist. Use -p to create parent directories.", rchCommandLine, false);

                        }
                    }
                }
            }

            // پیاده‌سازی دستور touch
            public void CreateFile(string path, DateTime? customTime = null)
            {
                if (string.IsNullOrEmpty(path))
                    throw new Exception("Path cannot be empty");

                // جدا کردن مسیر به دایرکتوری والد و نام فایل
                string directoryPath = GetDirectoryPath(path);
                string fileName = GetFileName(path);

                // پیدا کردن دایرکتوری والد
                Directory parentDir;
                if (directoryPath == ".")
                {
                    parentDir = CurrentDirectory;
                }
                else
                {
                    var dirNode = ResolvePath(directoryPath);
                    if (dirNode is Directory directory)
                    {
                        parentDir = directory;
                    }
                    else
                    {
                        throw new Exception($"'{directoryPath}' is not a directory");
                    }
                }

                // بررسی وجود فایل/دایرکتوری با همین نام
                if (parentDir.FindChild(fileName) != null)
                {
                    throw new Exception($"'{fileName}' already exists");
                }

                // ایجاد فایل جدید
                string fileExtension = Path.GetExtension(fileName).TrimStart('.');
                if (string.IsNullOrEmpty(fileExtension))
                    fileExtension = "txt";

                var newFile = new File(fileName, fileExtension);

                // تنظیم زمان در صورت وجود
                if (customTime.HasValue)
                {
                    newFile.CreationTime = customTime.Value;
                }

                parentDir.AddChild(newFile);
            }

            // تابع کمکی برای استخراج مسیر دایرکتوری از مسیر کامل
            private string GetDirectoryPath(string fullPath)
            {
                if (fullPath.Contains('/'))
                {
                    int lastSlash = fullPath.LastIndexOf('/');
                    return fullPath.Substring(0, lastSlash);
                }
                else
                {
                    return "."; // مسیر جاری
                }
            }

            // تابع کمکی برای استخراج نام فایل از مسیر کامل
            private string GetFileName(string fullPath)
            {
                if (fullPath.Contains('/'))
                {
                    int lastSlash = fullPath.LastIndexOf('/');
                    return fullPath.Substring(lastSlash + 1);
                }
                else
                {
                    return fullPath;
                }
            }

            // تابع کمکی برای حل مسیر
            private Node ResolvePath(string path)
            {
                if (path.StartsWith("/"))
                {
                    // مسیر مطلق - از ریشه شروع کن
                    return ResolveAbsolutePath(path);
                }
                else
                {
                    // مسیر نسبی - از دایرکتوری جاری شروع کن
                    return ResolveRelativePath(path);
                }
            }

            private Node ResolveAbsolutePath(string path)
            {
                var parts = path.Split('/').Where(p => !string.IsNullOrEmpty(p)).ToArray();
                var current = Root;

                foreach (var part in parts)
                {
                    if (part == "..")
                    {
                        if (current.Parent != null)
                            current = current.Parent;
                        continue;
                    }

                    if (part == ".")
                        continue;

                    var child = current.FindChild(part);
                    if (child is Directory dir)
                    {
                        current = dir;
                    }
                    else if (child != null)
                    {
                        return child;
                    }
                    else
                    {
                        throw new Exception($"Path not found: {path}");
                    }
                }

                return current;
            }

            private Node ResolveRelativePath(string path)
            {
                // مشابه بالا اما از CurrentDirectory شروع می‌کند
                var parts = path.Split('/').Where(p => !string.IsNullOrEmpty(p)).ToArray();
                var current = CurrentDirectory;

                // بقیه کد مشابه ResolveAbsolutePath
                foreach (var part in parts)
                {
                    if (part == "..")
                    {
                        if (current.Parent != null)
                            current = current.Parent;
                        continue;
                    }

                    if (part == ".")
                        continue;

                    var child = current.FindChild(part);
                    if (child is Directory dir)
                    {
                        current = dir;
                    }
                    else if (child != null)
                    {
                        return child;
                    }
                    else
                    {
                        throw new Exception($"Path not found: {path}");
                    }
                }
                return current;
            }
            //public void ListDirectory(string path = null)
            //{
            //    Directory target;
            //    if (string.IsNullOrEmpty(path))
            //    {
            //        target = CurrentDirectory;
            //    }
            //    else
            //    {
            //        var node = ResolvePath(path);
            //        if (node is Directory dir)
            //            target = dir;
            //        else
            //            throw new Exception($"'{path}' is not a directory");
            //    }

            //    foreach (var child in target.Children)
            //    {
            //        if (child is Directory)
            //            Console.WriteLine($"[DIR]  {child.Name}");
            //        else
            //            Console.WriteLine($"[FILE] {child.Name}");
            //    }
            //}

            public void PrintTree(Directory directory, string indent = "", bool isLast = true)
            {
                // تنظیم رنگ برای دایرکتوری‌ها
                Console.ForegroundColor = ConsoleColor.Yellow;

                Console.Write(indent);
                Console.Write(isLast ? "└── " : "├── ");
                Console.WriteLine(directory.Name);

                Console.ResetColor();

                // مرتب‌سازی کودکان برای نمایش مرتب‌تر
                var sortedChildren = directory.Children.OrderBy(c => c.Name).ToList();

                for (int i = 0; i < sortedChildren.Count; i++)
                {
                    var child = sortedChildren[i];
                    bool isLastChild = i == sortedChildren.Count - 1;

                    string childIndent = indent + (isLast ? "    " : "│   ");

                    if (child is Directory dir)
                    {
                        PrintTree(dir, childIndent, isLastChild);
                    }
                    else
                    {
                        Console.Write(childIndent);
                        Console.Write(isLastChild ? "└── " : "├── ");

                        // رنگ برای فایل‌ها
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        Console.WriteLine(child.Name);
                        Console.ResetColor();
                    }
                }
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
        public static void Mkdir_Command(string[] Inputs, RichTextBox commandList, System.Windows.Forms.TreeView treeView)
        {
            if (Inputs.Length < 2)
            {
                AddToCommandList("Syntax Error", commandList, false);
            }
            else
            {
                if (Inputs[1] == "-p")
                {
                    fs.CreateDirectory(Inputs[2], true, commandList);
                }
                else
                {
                    fs.CreateDirectory(Inputs[1], false, commandList);
                }
                UpdateTreeView(treeView);
                //// منطق دستور mkdir
                //AddToCommandList($"Directory '{Inputs[1]}' created", commandList);
            }
        }
        public static void BuildTreeView(Directory directory, TreeNodeCollection nodes)
        {
            // ایجاد نود اصلی برای دایرکتوری فعلی
            TreeNode currentNode = new TreeNode(directory.Name)
            {
                Tag = directory, // برای دسترسی بعدی به شیء اصلی
                ImageKey = "📁", // اگر آیکون داری
                SelectedImageKey = "📂"
            };

            nodes.Add(currentNode);

            // مرتب‌سازی برای نمایش منظم‌تر
            var sortedChildren = directory.Children.OrderBy(c => c.Name).ToList();

            foreach (var child in sortedChildren)
            {
                if (child is Directory dir)
                {
                    // فراخوانی بازگشتی برای پوشه‌ها
                    BuildTreeView(dir, currentNode.Nodes);
                }
                else
                {
                    // اضافه کردن فایل‌ها
                    TreeNode fileNode = new TreeNode(child.Name)
                    {
                        Tag = child,
                        ImageKey = "💾",
                        SelectedImageKey = "💾"
                    };
                    currentNode.Nodes.Add(fileNode);
                }
            }
        }

        private static void AddToCommandList(string input, RichTextBox rchCommandList, bool isCommand = true)
        {
            if (rchCommandList != null && !rchCommandList.IsDisposed && isCommand)
            {
                rchCommandList.Text += "> " + input + "\n";
                rchCommandList.ScrollToCaret();
            }
            if (rchCommandList != null && !rchCommandList.IsDisposed && !isCommand)
            {
                rchCommandList.Text += input + "\n";
                rchCommandList.ScrollToCaret();
            }
        }
        private static void UpdateCurrentRoute(Directory currentDirectory, System.Windows.Forms.TextBox txtCurrentRoute)
        {
            if (currentDirectory == null || txtCurrentRoute == null)
                return;

            try
            {
                // ساخت مسیر از دایرکتوری جاری تا ریشه
                var pathStack = new Stack<string>();
                Directory temp = currentDirectory;

                while (temp != null && temp.Name != "/")
                {
                    pathStack.Push(temp.Name);
                    temp = temp.Parent;
                }

                // ترکیب بخش‌های مسیر
                string fullPath = "/" + string.Join("/", pathStack);
                txtCurrentRoute.Text = fullPath;
            }
            catch (Exception ex)
            {
                txtCurrentRoute.Text = "/error";
                Console.WriteLine($"Error updating current route: {ex.Message}");
            }
        }
        private static void UpdateTreeView(System.Windows.Forms.TreeView treeview)
        {
            treeview.BeginUpdate();
            treeview.Nodes.Clear();

            BuildTreeView(fs.Root, treeview.Nodes);

            treeview.ExpandAll(); // برای باز کردن همه گره‌ها
            treeview.EndUpdate();
        }
        public string LastCommands = string.Empty;

        private void txtCommandLine_TextChanged(object sender, EventArgs e)
        {
            LastCommands = txtCommandLine.Text;
        }

    }
}
