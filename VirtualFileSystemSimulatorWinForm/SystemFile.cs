using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static VirtualFileSystemSimulatorWinForm.Form1;

namespace VirtualFileSystemSimulatorWinForm
{
    public class SystemFile
    {
        public Directory Root { get; private set; }
        public Directory CurrentDirectory { get; private set; }
        public Features features = new Features();

        public SystemFile()
        {
            Root = new Directory("/");
            CurrentDirectory = Root;
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
        public void CreateFile(string path, RichTextBox rchCommandLine, string customTime = null)
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

            parentDir.AddChild(newFile);
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
                    features.AddToCommandList($"Path not found: {path}", rchCommandLine, false);
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

    }

}
