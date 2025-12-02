using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static VirtualFileSystemSimulatorWinForm.Form1;

namespace VirtualFileSystemSimulatorWinForm
{
    public class SystemFile
    {
        public Directory Root { get; private set; }
        public Directory CurrentDirectory { get; private set; }
        public Features features = new Features();
        public UserType _UserType = UserType.owner;

        public SystemFile()
        {
            Root = new Directory("/");
            CurrentDirectory = Root;
        }
        public enum UserType { owner, group, others }
        public void ChangeUserType(UserType userType)
        {
            _UserType = userType;
        }
        public UserType ShowUserType()
        {
            return _UserType;
        }
        // پیاده‌سازی دستور cd
        public void ChangeDirectory(string path, RichTextBox rchCommandLine)
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

            var target = ResolvePath(path, rchCommandLine);
            if (target is Directory directory)
            {
                CurrentDirectory = directory;
            }
            else
            {
                features.AddToCommandList($"'{path}' is not a directory", rchCommandLine, false);
            }
        }
        // تابع جدید برای مدیریت مسیرهای حاوی .. و .
        private void NormalizePath(ref string path)
        {
            if (path.StartsWith(".."))
            {
                if (CurrentDirectory.Parent != null)
                    CurrentDirectory = CurrentDirectory.Parent;
                path = path.Substring(3);
            }
            else if (path.StartsWith("."))
            {
                path = path.Substring(2);
            }
        }
        // پیاده‌سازی دستور mkdir
        public void CreateDirectory(string path, bool createParents, RichTextBox rchCommandLine)
        {
            if (string.IsNullOrEmpty(path))
                features.AddToCommandList("Path cannot be empty", rchCommandLine, false);
            if (createParents)
                NormalizePath(ref path);
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
                        features.AddToCommandList($"'{currentPart}' already exists as a Directory", rchCommandLine, false);
                }
                else
                {
                    // یک فایل با همین نام وجود دارد - خطا
                    features.AddToCommandList($"'{currentPart}' is already exists as a file", rchCommandLine, false);

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

                        features.AddToCommandList($"Parent directory '{currentPart}' does not exist. Use -p to create parent directories.", rchCommandLine, false);

                    }
                }
            }
        }

        // پیاده‌سازی دستور touch
        public void CreateFile(string path, RichTextBox rchCommandLine, string customTime = null, string content = null)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                    features.AddToCommandList("Path cannot be empty", rchCommandLine, false);

                // جدا کردن مسیر به دایرکتوری والد و نام فایل
                // برای مثال /user/ali/report.txt
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
                    var dirNode = ResolvePath(directoryPath, rchCommandLine);
                    if (dirNode is Directory directory)
                    {
                        parentDir = directory;
                    }
                    else
                    {
                        features.AddToCommandList($"'{directoryPath}' is not a directory", rchCommandLine, false);
                        return;
                    }
                }
                if (!fileName.StartsWith("."))
                {
                    //برای فایل های قابل نمایش
                    // بررسی وجود فایل/دایرکتوری با همین نام
                    if (parentDir.FindChild(fileName.Split('.')[0]) != null)
                    {
                        features.AddToCommandList($"'{fileName}' already exists", rchCommandLine, false);
                        return;
                    }
                }
                else
                {
                    //برای فایل های غیر قابل نمایش
                    // بررسی وجود فایل/دایرکتوری با همین نام
                    if (parentDir.FindChild(fileName.Split('.')[1]) != null)
                    {
                        features.AddToCommandList($"'{fileName}' already exists", rchCommandLine, false);
                        return;
                    }
                }

                // ایجاد فایل جدید
                string fileExtension = Path.GetExtension(fileName).TrimStart('.');
                if (string.IsNullOrEmpty(fileExtension))
                    fileExtension = "txt";
                File newFile;
                if (!fileName.StartsWith("."))
                {
                    //برای فایل های قابل نمایش
                    newFile = new File(fileName.Split('.')[0], fileExtension);
                }
                else
                {
                    //برای فایل های غیر قابل نمایش
                    newFile = new File("." + fileName.Split('.')[1], fileExtension);
                }
                // تنظیم زمان در صورت وجود
                if (!string.IsNullOrEmpty(customTime))
                {
                    newFile.CreationTime = customTime;
                }
                // تنظیم متن در صورت وجود
                if (!string.IsNullOrEmpty(content))
                {
                    newFile.Content = content;
                }

                parentDir.AddChild(newFile);
            }
            catch (Exception ex)
            {
                features.AddToCommandList("Your command is invalide", rchCommandLine, false);
            }
        }

        // تابع کمکی برای استخراج مسیر دایرکتوری از مسیر کامل
        private string GetDirectoryPath(string fullPath)
        {
            if (fullPath == null)
                return ".";
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
        private Node ResolvePath(string path, RichTextBox rchCommandLine)
        {
            if (path.StartsWith("/"))
            {
                // مسیر مطلق - از ریشه شروع کن
                return ResolveAbsolutePath(path, rchCommandLine);
            }
            else
            {
                // مسیر نسبی - از دایرکتوری جاری شروع کن
                return ResolveRelativePath(path, rchCommandLine);
            }
        }

        private Node ResolveAbsolutePath(string path, RichTextBox rchCommandLine)
        {
            var parts = path.Split('/').Where(p => !string.IsNullOrEmpty(p)).ToArray();
            int Count = 1;
            var current = Root;

            foreach (var part in parts)
            {
                if (part == "..")
                {
                    if (current.Parent != null)
                    {
                        current = current.Parent;
                        Count++;
                    }
                    continue;
                }

                if (part == ".")
                {
                    Count++;
                    continue;
                }

                var child = current.FindChild(part);
                if (child is Directory dir)
                {
                    current = dir;
                }
                else if (Count == parts.Length)
                {

                }
                else if (child != null)
                {
                    return child;
                }
                else
                {
                    features.AddToCommandList($"Path not found: {path}", rchCommandLine, false);
                    return null;
                }
            }

            return current;
        }

        private Node ResolveRelativePath(string path, RichTextBox rchCommandLine)
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
                    features.AddToCommandList($"Path not found: {path}", rchCommandLine, false);
                    return null;
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

        public string Tree(RichTextBox rchCommandLine, string path = null, Directory dir = null,
                  string indent = "", bool isLast = true, int? maxDepth = null, int currentDepth = 0)
        {
            Directory targetDir = dir;

            if (path != null)
            {
                var dirNode = ResolvePath(path, rchCommandLine);
                if (dirNode is Directory directoryFromPath)
                {
                    targetDir = directoryFromPath;
                }
                else
                {
                    features.AddToCommandList($"'{path}' is not a directory", rchCommandLine, false);
                    return string.Empty;
                }
            }
            else if (targetDir == null)
            {
                targetDir = CurrentDirectory;
            }

            if (targetDir == null)
            {
                return string.Empty;
            }

            // بررسی اگر به حداکثر عمق مجاز رسیده باشیم
            if (maxDepth.HasValue && currentDepth > maxDepth.Value)
            {
                return string.Empty;
            }

            string tree = string.Empty;

            tree += indent;
            tree += isLast ? "└── " : "├── ";
            tree += targetDir.Name + "\n";

            // اگر به عمق مجاز رسیده باشیم، فرزندان را نمایش ندهیم
            if (maxDepth.HasValue && currentDepth == maxDepth.Value)
            {
                return tree;
            }

            // مرتب‌سازی فرزندان برای نمایش مرتب‌تر
            var sortedChildren = targetDir.Children.OrderBy(c => c.Name).ToList();

            for (int i = 0; i < sortedChildren.Count; i++)
            {
                var child = sortedChildren[i];
                bool isLastChild = i == sortedChildren.Count - 1;

                string childIndent = indent + (isLast ? "    " : "│   ");

                if (child is Directory childDirectory)
                {
                    if (childDirectory != targetDir) // جلوگیری از خودارجاعی ساده
                    {
                        tree += Tree(rchCommandLine, null, childDirectory, childIndent, isLastChild, maxDepth, currentDepth + 1);
                    }
                }
                else
                {
                    tree += childIndent;
                    tree += isLastChild ? "└── " : "├── ";
                    File file = (File)child;

                    if (!string.IsNullOrEmpty(file.FileType))
                    {
                        tree += file.Name + "." + file.FileType + "\n";
                    }
                    else
                    {
                        tree += file.Name + "\n";
                    }
                }
            }
            return tree;
        }
        public string LsShow(RichTextBox rchCommandLine, string Path = null, bool MoreInfo = false, bool ShowHidden = false)
        {

            // پیدا کردن دایرکتوری والد
            Directory parentDir;
            if (Path == null)
            {
                parentDir = CurrentDirectory;
            }
            else
            {
                var dirNode = ResolvePath(Path, rchCommandLine);
                if (dirNode is Directory directory)
                {
                    parentDir = directory;
                }
                else
                {
                    features.AddToCommandList($"'{Path}' is not a directory", rchCommandLine, false);
                    return null;
                }
            }
            string FilesOrFolders = "";
            // مرتب‌سازی فرزندان برای نمایش مرتب‌تر
            var sortedChildren = parentDir.Children.OrderBy(c => c.Name).ToList();

            for (int i = 0; i < sortedChildren.Count; i++)
            {
                var child = sortedChildren[i];
                if (ShowHidden)
                {
                    if (child is Directory dir)
                    {
                        if (MoreInfo)
                            FilesOrFolders += child.CreationTime + "    " + child.Permissions + "    " + child.Name + "\n";
                        else
                            FilesOrFolders += child.Name + "    ";
                    }
                    else
                    {

                        File file = (File)child;
                        if (MoreInfo)
                            FilesOrFolders += file.CreationTime + "    " + file.Permissions + "    " + file.Name + "." + file.FileType + "\n";
                        else
                            FilesOrFolders += file.Name + "." + file.FileType + "    ";
                    }

                }
                else if (!child.Name.StartsWith("."))
                {
                    if (child is Directory dir)
                    {
                        if (MoreInfo)
                            FilesOrFolders += child.CreationTime + "    " + child.Permissions + "    " + child.Name + "\n";
                        else
                            FilesOrFolders += child.Name + "    ";
                    }
                    else
                    {

                        File file = (File)child;
                        if (MoreInfo)
                            FilesOrFolders += file.CreationTime + "    " + file.Permissions + "    " + file.Name + "." + file.FileType + "\n";
                        else
                            FilesOrFolders += file.Name + "." + file.FileType + "    ";
                    }

                }
            }
            return FilesOrFolders;
        }
        public void Cd(RichTextBox rchCommandLine, string path = null)
        {
            if (path == "..")
            {
                if (CurrentDirectory.Parent != null)
                    CurrentDirectory = CurrentDirectory.Parent;
            }
            else if (path == null)
            {
                CurrentDirectory = Root;
            }
            else
            {
                Directory parentDir;
                if (path == null)
                {
                    parentDir = CurrentDirectory;
                }
                else
                {
                    var dirNode = ResolvePath(path, rchCommandLine);
                    if (dirNode is Directory directory)
                    {
                        CurrentDirectory = directory;
                    }
                    else
                    {
                        features.AddToCommandList($"'{path}' is not a directory", rchCommandLine, false);
                    }
                }
            }
        }
        public void Rm(RichTextBox rchCommandLine, string Name, bool IsRecursive, bool IsForce)
        {
            var dirNode = CurrentDirectory.FindChild(Name);
            if (dirNode != null)
            {
                if (!IsDeleteable(dirNode))
                {
                    features.AddToCommandList($"Permission denied: Cannot delete '{Name}'", rchCommandLine, false);
                    return;
                }
                if (dirNode is Directory directory)
                {
                    if (directory.Parent == null)
                    {
                        features.AddToCommandList("You can not delete root!", rchCommandLine, false);
                        return;
                    }
                    // اگر دایرکتوری باشد
                    if (directory.HasChild())
                    {
                        if (IsRecursive)
                        {
                            if (IsForce)
                            {
                                CurrentDirectory.RemoveChild(Name);
                            }
                            else
                            {
                                // پرسش از کاربر برای تأیید حذف
                                DialogResult result = MessageBox.Show($"Are you sure you want to remove directory '{Name}' and all its contents?",
                                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                                if (result == DialogResult.Yes)
                                {
                                    CurrentDirectory.RemoveChild(Name);
                                }
                                else
                                {
                                    features.AddToCommandList($"Deletion of directory '{Name}' cancelled.", rchCommandLine, false);
                                }
                            }
                        }
                        else
                        {
                            features.AddToCommandList($"Directory '{Name}' has File or Folder; Please use -r in your command", rchCommandLine, false);
                        }
                    }
                    else
                    {
                        // دایرکتوری خالی است
                        if (IsForce)
                        {
                            CurrentDirectory.RemoveChild(Name);
                        }
                        else
                        {
                            // پرسش از کاربر برای تأیید حذف دایرکتوری خالی
                            DialogResult result = MessageBox.Show($"Are you sure you want to remove directory '{Name}'?",
                                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                            if (result == DialogResult.Yes)
                            {
                                CurrentDirectory.RemoveChild(Name);
                            }
                            else
                            {
                                features.AddToCommandList($"Deletion of directory '{Name}' cancelled.", rchCommandLine, false);
                            }
                        }
                    }
                    return;
                }
                else
                {
                    // اگر فایل باشد
                    if (!Name.StartsWith("."))
                    {
                        if (Name.Contains("."))
                            Name = Name.Split('.')[0];
                    }
                    else
                    {
                        if (Name.Contains("."))
                            Name = "." + Name.Split('.')[1];
                    }

                    if (IsForce)
                    {
                        CurrentDirectory.RemoveChild(Name);
                    }
                    else
                    {
                        // پرسش از کاربر برای تأیید حذف فایل
                        DialogResult result = MessageBox.Show($"Are you sure you want to remove file '{Name}'?",
                            "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (result == DialogResult.Yes)
                        {
                            CurrentDirectory.RemoveChild(Name);
                        }
                        else
                        {
                            features.AddToCommandList($"Deletion of file '{Name}' cancelled.", rchCommandLine, false);
                        }
                    }
                    return;
                }
            }
            else
            {
                features.AddToCommandList($"'{Name}' is not found", rchCommandLine, false);
            }
        }
        public bool IsDeleteable(Node node)
        {
            switch (_UserType)
            {
                case UserType.owner:
                    if (node.Permissions[1] == 'w')
                        return true;
                    break;
                case UserType.group:
                    if (node.Permissions[4] == 'w')
                        return true;
                    break;
                case UserType.others:
                    if (node.Permissions[7] == 'w')
                        return true;
                    break;
                default:
                    break;
            }
            return false;
        }
        public void Ln(RichTextBox rchCommandLine, string[] Inputs)
        {
            if (Inputs[1] == "-s")
            {
                if (!Inputs[2].StartsWith("..") && !Inputs[2].StartsWith("/") && !Inputs[2].StartsWith("."))
                {
                    features.AddToCommandList($"path is not correct", rchCommandLine, false);
                    return;
                }
                else
                {
                    var dirNode = ResolvePath(Inputs[2], rchCommandLine);
                    if (dirNode == null) return;
                    if (CurrentDirectory.IsNameExistChild(Inputs[3]))
                    {
                        features.AddToCommandList($"this name is exist !", rchCommandLine, false);
                        return;
                    }
                    CurrentDirectory.AddChild(new File(Inputs[3], "", "", true, dirNode));

                }
            }
            else
            {
                Node dirNode = null;
                if (Inputs[1].StartsWith("..") || Inputs[1].StartsWith("/") || Inputs[1].StartsWith("."))
                {
                    // پیدا کردن دایرکتوری والد
                    dirNode = ResolvePath(Inputs[1], rchCommandLine);
                }
                else
                {
                    // برای اسم فایل و نام پوشه کار کنه
                    dirNode = CurrentDirectory.FindChild(Inputs[1].Split('.')[0]);
                }
                if (dirNode == null)
                {
                    features.AddToCommandList($"path is not correct", rchCommandLine, false);
                    return;
                }



                // بررسی اینکه هدف یک فایل است (لینک سخت فقط برای فایل‌ها)
                if (dirNode is Directory)
                {
                    features.AddToCommandList($"Hard link cannot be created for directory: {Inputs[1]}", rchCommandLine, false);
                    return;
                }
                if (CurrentDirectory.IsNameExistChild(Inputs[2]))
                {
                    features.AddToCommandList($"this name is exist !", rchCommandLine, false);
                    return;
                }
                // ایجاد لینک سخت
                if (dirNode is File targetFile)
                {
                    var hardLink = new File(Inputs[2], targetFile.FileType, targetFile.Content, targetFile.IsLink, targetFile.Link)
                    {
                        Permissions = targetFile.Permissions
                    };

                    CurrentDirectory.AddChild(hardLink);
                }


            }
        }
        public void Stat(RichTextBox rchCommandLine, string[] Inputs)
        {
            bool MoreInfo = false;
            if (Inputs.Length > 2 && Inputs[2] == "-l") MoreInfo = true;
            var dirNode = ResolvePath(Inputs[1], rchCommandLine);
            if (dirNode == null)
            {
                features.AddToCommandList("Maybe name is not correct", rchCommandLine, false);
                return;
            }
            if (dirNode is Directory directory)
            {
                features.AddToCommandList($"Name : {directory.Name}", rchCommandLine, false);
                features.AddToCommandList("Type : Directory", rchCommandLine, false);
                features.AddToCommandList($"Size : {directory.CountChild()}", rchCommandLine, false);
                features.AddToCommandList($"CreationTime : {directory.CreationTime}", rchCommandLine, false);
                features.AddToCommandList($"Permissions : {directory.Permissions}", rchCommandLine, false);
                return;
            }
            else if (dirNode is File File)
            {
                if (File.IsLink)
                {
                    features.AddToCommandList($"Name : {File.Name}", rchCommandLine, false);
                    features.AddToCommandList("Type : Link", rchCommandLine, false);
                    features.AddToCommandList($"Type : {File.Size}", rchCommandLine, false);
                    features.AddToCommandList($"CreationTime : {File.CreationTime}", rchCommandLine, false);
                    features.AddToCommandList($"Permissions : {File.Permissions}", rchCommandLine, false);
                    return;
                }
                else
                {
                    features.AddToCommandList($"Name : {File.Name}", rchCommandLine, false);
                    features.AddToCommandList("Type : File", rchCommandLine, false);
                    features.AddToCommandList($"Type : {File.Size}", rchCommandLine, false);
                    features.AddToCommandList($"CreationTime : {File.CreationTime}", rchCommandLine, false);
                    features.AddToCommandList($"Permissions : {File.Permissions}", rchCommandLine, false);
                    return;
                }
            }
        }
        public void Pwd(Directory currentDirectory, RichTextBox rchCommandLine)
        {
            if (currentDirectory == null)
                return;

            try
            {
                string fullPath = NodePathToString(currentDirectory);
                features.AddToCommandList(fullPath, rchCommandLine, false);
            }
            catch (Exception ex)
            {
                features.AddToCommandList($"Error updating current route: {ex.Message}", rchCommandLine, false);
            }
        }
        public void Echo(string name, string content, Directory currentDirectory, RichTextBox rchCommandLine, string datetime = null)
        {
            // ترکیب بخش‌های مسیر
            string fullPathandfilename = NodePathToString(currentDirectory) + "/" + name;
            CreateFile(fullPathandfilename, rchCommandLine, datetime, content);

        }
        public string NodePathToString(Directory currentDirectory)
        {
            if (currentDirectory == null)
                return null;

            // ساخت مسیر از دایرکتوری جاری تا ریشه
            var pathStack = new Stack<string>();
            Directory temp = currentDirectory;

            while (temp != null && temp.Name != "/")
            {
                pathStack.Push(temp.Name);
                temp = temp.Parent;
            }

            // ترکیب بخش‌های مسیر
            return "/" + string.Join("/", pathStack);
        }

        public void Cat(string path, RichTextBox rchCommandLine)
        {
            string directoryPath = NodePathToString(CurrentDirectory);
            if (path.StartsWith(".") || path.StartsWith("/"))
                // جدا کردن مسیر به دایرکتوری والد و نام فایل
                // برای مثال /user/ali/report.txt
                directoryPath = GetDirectoryPath(path);

            string fileName = GetFileName(path);

            // پیدا کردن دایرکتوری والد
            Directory parentDir;
            if (directoryPath == ".")
            {
                parentDir = CurrentDirectory;
            }
            else
            {
                var dirNode = ResolvePath(directoryPath, rchCommandLine);
                if (dirNode is Directory directory)
                {
                    parentDir = directory;
                }
                else
                {
                    features.AddToCommandList($"'{directoryPath}' is not a directory", rchCommandLine, false);
                    return;
                }
            }
            //if (!fileName.StartsWith("."))
            //{
            //    //برای فایل های قابل نمایش
            //    // بررسی وجود فایل/دایرکتوری با همین نام
            //    if (parentDir.FindChild(fileName.Split('.')[0]) != null)
            //    {
            //        features.AddToCommandList($"'{fileName}' already exists", rchCommandLine, false);
            //        return;
            //    }
            //}
            //else
            //{
            //    //برای فایل های غیر قابل نمایش
            //    // بررسی وجود فایل/دایرکتوری با همین نام
            //    if (parentDir.FindChild(fileName.Split('.')[1]) != null)
            //    {
            //        features.AddToCommandList($"'{fileName}' already exists", rchCommandLine, false);
            //        return;
            //    }
            //}

            if (parentDir.FindChild(fileName) != null)
            {
                File file = (File)parentDir.FindChild(fileName);
                features.AddToCommandList($"{file.Content}", rchCommandLine, false);
                return;
            }
            else
            {
                features.AddToCommandList($"'{fileName}' is not exist", rchCommandLine, false);
                return;
            }
        }
        public void Mv(string[] Inputs, RichTextBox rchCommandLine)
        {
            string mainfileName = null;
            string mainfolderpath = null;

            if (Inputs[1].StartsWith(".") || Inputs[1].StartsWith("/"))
                mainfolderpath = Inputs[1];
            else
                mainfileName = GetFileName(Inputs[1]);

            string secondfileName = null;
            string secondfolderpath = NodePathToString(CurrentDirectory);

            if (Inputs[2].StartsWith(".") || Inputs[2].StartsWith("/"))
                // جدا کردن مسیر به دایرکتوری والد و نام فایل
                // برای مثال /user/ali/report.txt
                secondfolderpath = Inputs[2];
            else
                secondfileName = GetFileName(Inputs[2]);

            // پیدا کردن دایرکتوری والد
            Directory FirstDir = null;
            if (mainfolderpath != null)
            {
                
                if (mainfolderpath == ".")
                {
                    FirstDir = CurrentDirectory;
                }
                else
                {
                    var dirNode = ResolvePath(mainfolderpath, rchCommandLine);
                    if (dirNode is Directory directory)
                    {
                        FirstDir = directory;
                    }
                    else
                    {
                        features.AddToCommandList($"'{mainfolderpath}' is not a directory", rchCommandLine, false);
                        return;
                    }
                }
            }
            Directory SecondDir;
            if (secondfolderpath == ".")
            {
                SecondDir = CurrentDirectory;
            }
            else
            {
                var dirNode = ResolvePath(secondfolderpath, rchCommandLine);
                if (dirNode is Directory directory)
                {
                    SecondDir = directory;
                }
                else
                {
                    features.AddToCommandList($"'{secondfolderpath}' is not a directory", rchCommandLine, false);
                    return;
                }
            }
            if (!CurrentDirectory.IsNameExistChild(mainfileName) && mainfileName != null)
            {
                features.AddToCommandList($"'{mainfileName}' is not a file", rchCommandLine, false);
                return;
            }
            if (SecondDir.IsNameExistChild(mainfileName) && mainfileName != null && secondfileName == null)
            {
                features.AddToCommandList($"'{mainfileName}' is already exist in this path", rchCommandLine, false);
                return;
            }
            if (!CurrentDirectory.IsNameExistChild(Inputs[1].Trim().Split('/').ToArray().Last()) && mainfolderpath != null)
            {
                features.AddToCommandList($"'{mainfolderpath}' is not a folder", rchCommandLine, false);
                return;
            }
            if (SecondDir.IsNameExistChild(Inputs[1].Trim().Split('/').ToArray().Last()) && mainfolderpath != null)
            {
                features.AddToCommandList($"'{mainfolderpath}' is already exist in this path", rchCommandLine, false);
                return;
            }

            if (secondfileName == null && mainfolderpath == null)
            {
                File file = (File)CurrentDirectory.FindChild(mainfileName);
                SecondDir.AddChild(file);
                CurrentDirectory.RemoveChild(Inputs[1].Trim().Split('/').ToArray().Last());
            }
            else if(mainfileName == null && secondfileName == null)
            {
                SecondDir.AddChild(FirstDir);
                CurrentDirectory.RemoveChild(Inputs[1].Trim().Split('/').ToArray().Last());
            } 
            else if(mainfolderpath == null)
            {
                File file = (File)CurrentDirectory.FindChild(mainfileName);
                file.Name = secondfileName;
            }
            
        }
        public void Cp(string[] Inputs, RichTextBox rchCommandLine)
        {
            string mainfileName = null;
            string mainfolderpath = null;

            if (Inputs[1].StartsWith(".") || Inputs[1].StartsWith("/"))
                mainfolderpath = Inputs[1];
            else
                mainfileName = GetFileName(Inputs[1]);

            string secondfolderpath = NodePathToString(CurrentDirectory);

            if (Inputs[2].StartsWith(".") || Inputs[2].StartsWith("/"))
                // جدا کردن مسیر به دایرکتوری والد و نام فایل
                // برای مثال /user/ali/report.txt
                secondfolderpath = Inputs[2];

            // پیدا کردن دایرکتوری والد
            Directory FirstDir = null;
            if (mainfolderpath != null)
            {
                
                if (mainfolderpath == ".")
                {
                    FirstDir = CurrentDirectory;
                }
                else
                {
                    var dirNode = ResolvePath(mainfolderpath, rchCommandLine);
                    if (dirNode is Directory directory)
                    {
                        FirstDir = directory;
                    }
                    else
                    {
                        features.AddToCommandList($"'{mainfolderpath}' is not a directory", rchCommandLine, false);
                        return;
                    }
                }
            }
            Directory SecondDir;
            if (secondfolderpath == ".")
            {
                SecondDir = CurrentDirectory;
            }
            else
            {
                var dirNode = ResolvePath(secondfolderpath, rchCommandLine);
                if (dirNode is Directory directory)
                {
                    SecondDir = directory;
                }
                else
                {
                    features.AddToCommandList($"'{secondfolderpath}' is not a directory", rchCommandLine, false);
                    return;
                }
            }
            if (!CurrentDirectory.IsNameExistChild(mainfileName) && mainfileName != null)
            {
                features.AddToCommandList($"'{mainfileName}' is not a file", rchCommandLine, false);
                return;
            }
            if (SecondDir.IsNameExistChild(mainfileName) && mainfileName != null)
            {
                features.AddToCommandList($"'{mainfileName}' is already exist in this path", rchCommandLine, false);
                return;
            }
            if (!CurrentDirectory.IsNameExistChild(Inputs[1].Trim().Split('/').ToArray().Last()) && mainfolderpath != null)
            {
                features.AddToCommandList($"'{mainfolderpath}' is not a folder", rchCommandLine, false);
                return;
            }
            if (SecondDir.IsNameExistChild(Inputs[1].Trim().Split('/').ToArray().Last()) && mainfolderpath != null)
            {
                features.AddToCommandList($"'{mainfolderpath}' is already exist in this path", rchCommandLine, false);
                return;
            }

            if (mainfolderpath == null)
            {
                File file = (File)CurrentDirectory.FindChild(mainfileName);
                SecondDir.AddChild(file);
            }
            else
            {
                Directory temp = FirstDir;
                SecondDir.AddChild(temp);
            } 
            
        }
    }

}
