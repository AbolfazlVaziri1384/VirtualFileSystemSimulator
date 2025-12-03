using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VirtualFileSystemSimulatorWinForm
{
    public class SystemFile
    {
        public Directory Root { get; private set; }
        public Directory CurrentDirectory { get; private set; }
        public Features Feature = new Features();
        public UserType CurrentUserType = UserType.Owner;
        public Json UserManager { get; set; }
        private readonly byte[] _key = Encoding.UTF8.GetBytes("your_secret_key_change_this"); // کلید رمزنگاری (تغییر دهید)

        public SystemFile()
        {
            Root = new Directory("/");
            CurrentDirectory = Root;
        }
        public enum UserType { Owner, Group, Others }

        // For Change User Type
        public void ChangeUserType(UserType usertype)
        {
            CurrentUserType = usertype;
        }

        // For Show User Type 
        public UserType ShowUserType()
        {
            return CurrentUserType;
        }

        // رمزنگاری ساده XOR
        public byte[] SimpleEncrypt(byte[] data)
        {
            var encrypted = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                encrypted[i] = (byte)(data[i] ^ _key[i % _key.Length]);
            }
            return encrypted;
        }
        public byte[] SimpleDecrypt(byte[] data)
        {
            return SimpleEncrypt(data); // symmetric
        }
        // بعد از لاگین موفق در فرم، VFS را لود کنید
        public void LoadAfterLogin()
        {
            Root = (Directory)UserManager.LoadVfsForCurrentUser(this);
            CurrentDirectory = Root;
        }

        // ذخیره بعد از عملیات
        public void Save()
        {
            UserManager.SaveVfsForCurrentUser(this, Root);
        }

        // چک مجوزها (مثال ساده)
        private bool HasPermission(Node node, string action) // action: "r", "w", "x"
        {
            if (UserManager.CurrentUser == null) return false;
            if (UserManager.CurrentUser.IsAdmin) return true;

            int offset = UserManager.CurrentUser.Username == node.Owner ? 0 : (UserManager.CurrentUser.Username == node.Group ? 3 : 6);
            char perm = node.Permissions[offset + (action == "r" ? 0 : (action == "w" ? 1 : 2))];
            return perm != '-';
        }

        // For managing ".." or "." in the path
        private void NormalizePath(ref string path)
        {
            if (path.StartsWith(".."))
            {
                if (CurrentDirectory.Parent != null)
                    CurrentDirectory = (Directory)CurrentDirectory.Parent;
                path = path.Substring(3);
            }
            else if (path.StartsWith("."))
            {
                path = path.Substring(2);
            }
        }

        // For making directory
        public void Mkdir(string path, bool createparents, RichTextBox rchCommandLine)
        {
            if (UserManager.CurrentUser == null)
            {
                Feature.AddToCommandList("Login required", rchCommandLine, false);
                return;
            }

            if (string.IsNullOrEmpty(path))
                Feature.AddToCommandList("Path cannot be empty", rchCommandLine, false);

            if (createparents)
                NormalizePath(ref path);

            var parts = path.Split('/').Where(p => !string.IsNullOrEmpty(p)).ToArray();

            if (path.StartsWith("/"))
            {
                // Absolute path
                CreateDirectory(Root, parts, 0, createparents, rchCommandLine);
            }
            else
            {
                // Relative path
                CreateDirectory(CurrentDirectory, parts, 0, createparents, rchCommandLine);
            }
            Save();
        }









        // For Create Directory
        private void CreateDirectory(Directory current, string[] parts, int index, bool createparents, RichTextBox rchCommandLine)
        {
            if (index >= parts.Length)
                return;

            string _CurrentPart = parts[index];

            // Is exist name ?
            var _ExistingNode = current.FindChild(_CurrentPart);

            if (_ExistingNode != null)
            {
                if (_ExistingNode is Directory _ExistingDirectory)
                {
                    // Is Exist directory and go next part
                    CreateDirectory(_ExistingDirectory, parts, index + 1, createparents, rchCommandLine);
                    if (index >= parts.Length - 1)
                        Feature.AddToCommandList($"'{_CurrentPart}' already exists as a directory", rchCommandLine, false);
                }
                else
                {
                    Feature.AddToCommandList($"'{_CurrentPart}' is already exists as a file", rchCommandLine, false);
                }
            }
            else
            {
                if (index == parts.Length - 1)
                {
                    var _NewDirectory = new Directory(_CurrentPart, current);
                    current.AddChild(_NewDirectory);
                    CurrentDirectory = _NewDirectory;
                }
                else
                {
                    if (createparents)
                    {
                        var _NewDirectory = new Directory(_CurrentPart, current);
                        current.AddChild(_NewDirectory);
                        CreateDirectory(_NewDirectory, parts, index + 1, createparents, rchCommandLine);
                    }
                    else
                    {
                        Feature.AddToCommandList($"Parent directory '{_CurrentPart}' does not exist. Use -p to create parent directories.", rchCommandLine, false);
                    }
                }
            }
        }

        // For making file
        public void Touch(string pathandfilename, RichTextBox rchCommandLine, string customtime = null, string content = null)
        {
            try
            {
                if (string.IsNullOrEmpty(pathandfilename))
                    Feature.AddToCommandList("Path cannot be empty", rchCommandLine, false);

                string _DirectoryPath = GetDirectoryPath(pathandfilename);
                string _FileName = GetFileName(pathandfilename);

                Directory _ParentDirectory;
                if (_DirectoryPath == ".")
                {
                    _ParentDirectory = CurrentDirectory;
                }
                else
                {
                    var _DirectoryNode = ResolvePath(_DirectoryPath, rchCommandLine);
                    if (_DirectoryNode is Directory _Directory)
                    {
                        _ParentDirectory = _Directory;
                    }
                    else
                    {
                        Feature.AddToCommandList($"'{_DirectoryPath}' is not a directory", rchCommandLine, false);
                        return;
                    }
                }
                if (!_FileName.StartsWith("."))
                {
                    // Check existing for hiden file
                    if (_ParentDirectory.FindChild(_FileName.Split('.')[0]) != null)
                    {
                        Feature.AddToCommandList($"'{_FileName}' already exists", rchCommandLine, false);
                        return;
                    }
                }
                else
                {
                    // Check existing for normal file
                    if (_ParentDirectory.FindChild(_FileName.Split('.')[1]) != null)
                    {
                        Feature.AddToCommandList($"'{_FileName}' already exists", rchCommandLine, false);
                        return;
                    }
                }

                // Making new file
                string _FileExtension = Path.GetExtension(_FileName).TrimStart('.');
                if (string.IsNullOrEmpty(_FileExtension))
                    _FileExtension = "txt";

                File _NewFile;
                if (!_FileName.StartsWith("."))
                {
                    // For files we can show there
                    _NewFile = new File(_FileName.Split('.')[0], fileType: _FileExtension);
                }
                else
                {
                    // For files we can not show there
                    _NewFile = new File("." + _FileName.Split('.')[1], fileType: _FileExtension);
                }
                // Add custom time if it exist
                if (!string.IsNullOrEmpty(customtime))
                {
                    _NewFile.Timestamp = customtime;
                }
                // Add text if it exist
                if (!string.IsNullOrEmpty(content))
                {
                    _NewFile.Content = content;
                }

                _ParentDirectory.AddChild(_NewFile);
            }
            catch
            {
                Feature.AddToCommandList("Your command is invalide", rchCommandLine, false);
            }
        }

        // Extraction path from "path + file name"
        private string GetDirectoryPath(string fullpath)
        {
            if (fullpath == null)
                return ".";
            if (fullpath.Contains('/'))
            {
                int _LastSlash = fullpath.LastIndexOf('/');
                return fullpath.Substring(0, _LastSlash);
            }
            else
            {
                // Current path
                return ".";
            }
        }

        // Extraction file name from "path + file name"
        private string GetFileName(string fullpath)
        {
            if (fullpath.Contains('/'))
            {
                int _LastSlash = fullpath.LastIndexOf('/');
                return fullpath.Substring(_LastSlash + 1);
            }
            else
            {
                return fullpath;
            }
        }

        // For detect Relative or Absolute path
        private Node ResolvePath(string path, RichTextBox rchCommandLine)
        {
            if (path.StartsWith("/"))
            {
                // Absolute path
                return ResolveAbsolutePath(path, rchCommandLine);
            }
            else
            {
                // Relative path
                return ResolveRelativePath(path, rchCommandLine);
            }
        }

        private Node ResolveAbsolutePath(string path, RichTextBox rchCommandLine)
        {
            var _Parts = path.Split('/').Where(p => !string.IsNullOrEmpty(p)).ToArray();
            int _Count = 1;
            var _Current = Root;

            foreach (var _Part in _Parts)
            {
                if (_Part == "..")
                {
                    if (_Current.Parent != null)
                    {
                        _Current = (Directory)_Current.Parent;
                        _Count++;
                    }
                    continue;
                }

                if (_Part == ".")
                {
                    _Count++;
                    continue;
                }

                var _Child = _Current.FindChild(_Part);
                if (_Child is Directory dir)
                {
                    _Current = dir;
                }
                else if (_Count == _Parts.Length)
                {

                }
                else if (_Child != null)
                {
                    return _Child;
                }
                else
                {
                    Feature.AddToCommandList($"Path not found: {path}", rchCommandLine, false);
                    return null;
                }
            }

            return _Current;
        }

        private Node ResolveRelativePath(string path, RichTextBox rchCommandLine)
        {
            var _Parts = path.Split('/').Where(p => !string.IsNullOrEmpty(p)).ToArray();
            var _Current = CurrentDirectory;

            foreach (var part in _Parts)
            {
                if (part == "..")
                {
                    if (_Current.Parent != null)
                        _Current = (Directory)_Current.Parent;
                    continue;
                }

                if (part == ".")
                    continue;

                var _Child = _Current.FindChild(part);
                if (_Child is Directory dir)
                {
                    _Current = dir;
                }
                else if (_Child != null)
                {
                    return _Child;
                }
                else
                {
                    Feature.AddToCommandList($"Path not found: {path}", rchCommandLine, false);
                    return null;
                }
            }
            return _Current;
        }

        // For making and show tree
        public string Tree(RichTextBox rchCommandLine, string path = null, Directory directory = null,
                  string indent = "", bool islast = true, int? maxdepth = null, int currentdepth = 0)
        {
            Directory _TargetDirectory = directory;

            if (path != null)
            {
                var _DirectoryNode = ResolvePath(path, rchCommandLine);
                if (_DirectoryNode is Directory directoryFromPath)
                {
                    _TargetDirectory = directoryFromPath;
                }
                else
                {
                    Feature.AddToCommandList($"'{path}' is not a directory", rchCommandLine, false);
                    return string.Empty;
                }
            }
            else if (_TargetDirectory == null)
            {
                _TargetDirectory = CurrentDirectory;
            }

            if (_TargetDirectory == null)
            {
                return string.Empty;
            }

            // Check depth
            if (maxdepth.HasValue && currentdepth > maxdepth.Value)
            {
                return string.Empty;
            }

            string _Tree = string.Empty;

            _Tree += indent;
            _Tree += islast ? "└── " : "├── ";
            _Tree += _TargetDirectory.Name + "\n";

            // Check depth
            if (maxdepth.HasValue && currentdepth == maxdepth.Value)
            {
                return _Tree;
            }

            var _SortedChildren = _TargetDirectory.Children.OrderBy(c => c.Name).ToList();

            for (int i = 0; i < _SortedChildren.Count; i++)
            {
                var _Child = _SortedChildren[i];
                bool _IsLastChild = i == _SortedChildren.Count - 1;

                string _ChildIndent = indent + (islast ? "    " : "│   ");

                if (_Child is Directory childDirectory)
                {
                    if (childDirectory != _TargetDirectory)
                    {
                        _Tree += Tree(rchCommandLine, null, childDirectory, _ChildIndent, _IsLastChild, maxdepth, currentdepth + 1);
                    }
                }
                else
                {
                    _Tree += _ChildIndent;
                    _Tree += _IsLastChild ? "└── " : "├── ";
                    File file = (File)_Child;

                    if (!string.IsNullOrEmpty(file.FileType))
                    {
                        _Tree += file.Name + "." + file.FileType + "\n";
                    }
                    else
                    {
                        _Tree += file.Name + "\n";
                    }
                }
            }
            return _Tree;
        }

        // For show all file and directory in the specifided path
        public string Ls(RichTextBox rchCommandLine, string path = null, bool moreinfo = false, bool showhidden = false)
        {

            Directory _ParentDirectory;
            if (path == null)
            {
                _ParentDirectory = CurrentDirectory;
            }
            else
            {
                var _DirectoryNode = ResolvePath(path, rchCommandLine);
                if (_DirectoryNode is Directory directory)
                {
                    _ParentDirectory = directory;
                }
                else
                {
                    Feature.AddToCommandList($"'{path}' is not a directory", rchCommandLine, false);
                    return null;
                }
            }
            string _FilesOrFolders = "";
            var _SortedChildren = _ParentDirectory.Children.OrderBy(c => c.Name).ToList();

            for (int i = 0; i < _SortedChildren.Count; i++)
            {
                var _Child = _SortedChildren[i];
                if (showhidden)
                {
                    if (_Child is Directory dir)
                    {
                        if (moreinfo)
                            _FilesOrFolders += _Child.Timestamp + "    " + _Child.Permissions + "    " + _Child.Name + "\n";
                        else
                            _FilesOrFolders += _Child.Name + "    ";
                    }
                    else
                    {

                        File file = (File)_Child;
                        if (moreinfo)
                            _FilesOrFolders += file.Timestamp + "    " + file.Permissions + "    " + file.Name + "." + file.FileType + "\n";
                        else
                            _FilesOrFolders += file.Name + "." + file.FileType + "    ";
                    }

                }
                else if (!_Child.Name.StartsWith("."))
                {
                    if (_Child is Directory dir)
                    {
                        if (moreinfo)
                            _FilesOrFolders += _Child.Timestamp + "    " + _Child.Permissions + "    " + _Child.Name + "\n";
                        else
                            _FilesOrFolders += _Child.Name + "    ";
                    }
                    else
                    {

                        File file = (File)_Child;
                        if (moreinfo)
                            _FilesOrFolders += file.Timestamp + "    " + file.Permissions + "    " + file.Name + "." + file.FileType + "\n";
                        else
                            _FilesOrFolders += file.Name + "." + file.FileType + "    ";
                    }

                }
            }
            return _FilesOrFolders;
        }

        // For go to specifided path
        public void Cd(RichTextBox rchCommandLine, string path = null)
        {
            if (path == "..")
            {
                if (CurrentDirectory.Parent != null)
                    CurrentDirectory = (Directory)CurrentDirectory.Parent;
            }
            else if (path == null)
            {
                CurrentDirectory = Root;
            }
            else
            {
                Directory _ParentDirectory;
                if (path == null)
                {
                    _ParentDirectory = CurrentDirectory;
                }
                else
                {
                    var _DirectoryNode = ResolvePath(path, rchCommandLine);
                    if (_DirectoryNode is Directory directory)
                    {
                        CurrentDirectory = directory;
                    }
                    else
                    {
                        Feature.AddToCommandList($"'{path}' is not a directory", rchCommandLine, false);
                    }
                }
            }
        }

        // For remove file or folder 
        public void Rm(RichTextBox rchCommandLine, string name, bool isrecursive, bool isforce)
        {
            var _DirectoryNode = CurrentDirectory.FindChild(name);
            if (_DirectoryNode != null)
            {
                if (!IsDeleteable(_DirectoryNode))
                {
                    Feature.AddToCommandList($"Permission denied: Cannot delete '{name}'", rchCommandLine, false);
                    return;
                }
                if (_DirectoryNode is Directory directory)
                {
                    if (directory.Parent == null)
                    {
                        Feature.AddToCommandList("You can not delete root!", rchCommandLine, false);
                        return;
                    }

                    // If it is directory
                    if (directory.HasChild())
                    {
                        if (isrecursive)
                        {
                            if (isforce)
                            {
                                CurrentDirectory.RemoveChild(name);
                            }
                            else
                            {
                                // Ask for delete
                                DialogResult result = MessageBox.Show($"Are you sure you want to remove directory '{name}' and all its contents?",
                                    "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                                if (result == DialogResult.Yes)
                                {
                                    CurrentDirectory.RemoveChild(name);
                                }
                                else
                                {
                                    Feature.AddToCommandList($"Deletion of directory '{name}' cancelled.", rchCommandLine, false);
                                }
                            }
                        }
                        else
                        {
                            Feature.AddToCommandList($"Directory '{name}' has File or Folder; Please use -r in your command", rchCommandLine, false);
                        }
                    }
                    else
                    {
                        // for empty directory
                        if (isforce)
                        {
                            CurrentDirectory.RemoveChild(name);
                        }
                        else
                        {
                            // Ask for delete
                            DialogResult _result = MessageBox.Show($"Are you sure you want to remove directory '{name}'?",
                                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                            if (_result == DialogResult.Yes)
                            {
                                CurrentDirectory.RemoveChild(name);
                            }
                            else
                            {
                                Feature.AddToCommandList($"Deletion of directory '{name}' cancelled.", rchCommandLine, false);
                            }
                        }
                    }
                    return;
                }
                else
                {
                    // If it is file
                    if (!name.StartsWith("."))
                    {
                        if (name.Contains("."))
                            name = name.Split('.')[0];
                    }
                    else
                    {
                        if (name.Contains("."))
                            name = "." + name.Split('.')[1];
                    }

                    if (isforce)
                    {
                        CurrentDirectory.RemoveChild(name);
                    }
                    else
                    {
                        // Ask for delete
                        DialogResult _result = MessageBox.Show($"Are you sure you want to remove file '{name}'?",
                            "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                        if (_result == DialogResult.Yes)
                        {
                            CurrentDirectory.RemoveChild(name);
                        }
                        else
                        {
                            Feature.AddToCommandList($"Deletion of file '{name}' cancelled.", rchCommandLine, false);
                        }
                    }
                    return;
                }
            }
            else
            {
                Feature.AddToCommandList($"'{name}' is not found", rchCommandLine, false);
            }
        }
        //-----------------------این باید برده شود در کلاس پرمیشن ه____________________
        public bool IsDeleteable(Node node)
        {
            switch (CurrentUserType)
            {
                case UserType.Owner:
                    if (node.Permissions[1] == 'w')
                        return true;
                    break;
                case UserType.Group:
                    if (node.Permissions[4] == 'w')
                        return true;
                    break;
                case UserType.Others:
                    if (node.Permissions[7] == 'w')
                        return true;
                    break;
                default:
                    break;
            }
            return false;
        }

        // For making link file or directry
        public void Ln(RichTextBox rchCommandLine, string[] inputs)
        {
            bool _IsSoft = inputs[1] == "-s";
            string _Path = inputs[2];
            string _Name = inputs[3];
            if (_IsSoft)
            {
                if (!_Path.StartsWith("..") && !_Path.StartsWith("/") && !_Path.StartsWith("."))
                {
                    Feature.AddToCommandList($"path is not correct", rchCommandLine, false);
                    return;
                }
                else
                {
                    var _DirectoryNode = ResolvePath(_Path, rchCommandLine);
                    if (_DirectoryNode == null) return;
                    if (CurrentDirectory.IsExistChildName(_Name))
                    {
                        Feature.AddToCommandList($"this name is exist !", rchCommandLine, false);
                        return;
                    }
                    CurrentDirectory.AddChild(new File(_Name, islink: true, link: _DirectoryNode));

                }
            }
            else
            {
                Node _DirectoryNode = null;
                _Path = inputs[1];
                _Name = inputs[2];
                if (_Path.StartsWith("..") || _Path.StartsWith("/") || _Path.StartsWith("."))
                {
                    // Find parent
                    _DirectoryNode = ResolvePath(_Path, rchCommandLine);
                }
                else
                {
                    // For find name
                    _DirectoryNode = CurrentDirectory.FindChild(_Path.Split('.')[0]);
                }
                if (_DirectoryNode == null)
                {
                    Feature.AddToCommandList($"path is not correct", rchCommandLine, false);
                    return;
                }

                if (_DirectoryNode is Directory)
                {
                    Feature.AddToCommandList($"Hard link cannot be created for directory: {_Path}", rchCommandLine, false);
                    return;
                }
                if (CurrentDirectory.IsExistChildName(_Name))
                {
                    Feature.AddToCommandList($"this name is exist !", rchCommandLine, false);
                    return;
                }
                if (_DirectoryNode is File targetFile)
                {
                    var hardLink = new File(_Name, fileType: targetFile.FileType, content: targetFile.Content, islink: targetFile.IsLink, link: targetFile.Link)
                    {
                        Permissions = targetFile.Permissions
                    };

                    CurrentDirectory.AddChild(hardLink);
                }


            }
        }

        // For get information about file or directory
        public void Stat(RichTextBox rchCommandLine, string[] inputs)
        {
            bool _MoreInfo = false;
            if (inputs.Length > 2) _MoreInfo = inputs[2] == "-l";
            string _Path = inputs[1];
            var _DirectoryNode = ResolvePath(_Path, rchCommandLine);
            if (_DirectoryNode == null)
            {
                Feature.AddToCommandList("Maybe name is not correct", rchCommandLine, false);
                return;
            }
            if (_DirectoryNode is Directory _Directory)
            {
                Feature.AddToCommandList($"Name : {_Directory.Name}", rchCommandLine, false);
                Feature.AddToCommandList("Type : Directory", rchCommandLine, false);
                Feature.AddToCommandList($"Size : {_Directory.CountChild()}", rchCommandLine, false);
                Feature.AddToCommandList($"CreationTime : {_Directory.Timestamp}", rchCommandLine, false);
                Feature.AddToCommandList($"Permissions : {_Directory.Permissions}", rchCommandLine, false);
                return;
            }
            else if (_DirectoryNode is File _File)
            {
                if (_File.IsLink)
                {
                    Feature.AddToCommandList($"Name : {_File.Name}", rchCommandLine, false);
                    Feature.AddToCommandList("Type : Link", rchCommandLine, false);
                    Feature.AddToCommandList($"Type : {_File.Size}", rchCommandLine, false);
                    Feature.AddToCommandList($"CreationTime : {_File.Timestamp}", rchCommandLine, false);
                    Feature.AddToCommandList($"Permissions : {_File.Permissions}", rchCommandLine, false);
                    return;
                }
                else
                {
                    Feature.AddToCommandList($"Name : {_File.Name}", rchCommandLine, false);
                    Feature.AddToCommandList("Type : File", rchCommandLine, false);
                    Feature.AddToCommandList($"Type : {_File.Size}", rchCommandLine, false);
                    Feature.AddToCommandList($"CreationTime : {_File.Timestamp}", rchCommandLine, false);
                    Feature.AddToCommandList($"Permissions : {_File.Permissions}", rchCommandLine, false);
                    return;
                }
            }
        }

        // For show current route
        public void Pwd(Directory currentdirectory, RichTextBox rchCommandLine)
        {
            if (currentdirectory == null)
                return;

            try
            {
                string _FullPath = NodePathToString(currentdirectory);
                Feature.AddToCommandList(_FullPath, rchCommandLine, false);
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error updating current route: {ex.Message}", rchCommandLine, false);
            }
        }

        // For save content in file
        public void Echo(string name, string content, Directory currentDirectory, RichTextBox rchCommandLine, string datetime = null)
        {
            // Combine path and file name
            string _FullPathAndFileName = NodePathToString(currentDirectory) + "/" + name;
            Touch(_FullPathAndFileName, rchCommandLine, datetime, content);

        }

        // Function to make string of node path
        public static string NodePathToString(Directory currentDirectory)
        {
            if (currentDirectory == null)
                return null;

            // Making path of currentDirectory to root
            var _PathStack = new Stack<string>();
            Directory temp = currentDirectory;

            while (temp != null && temp.Name != "/")
            {
                _PathStack.Push(temp.Name);
                temp = (Directory)temp.Parent;
            }

            // Combine parts
            return "/" + string.Join("/", _PathStack);
        }

        // For show file's content
        public void Cat(string path, RichTextBox rchCommandLine)
        {
            string _DirectoryPath = NodePathToString(CurrentDirectory);
            if (path.StartsWith(".") || path.StartsWith("/"))
                _DirectoryPath = GetDirectoryPath(path);

            string _FileName = GetFileName(path);

            Directory _ParentDirectory;
            if (_DirectoryPath == ".")
            {
                _ParentDirectory = CurrentDirectory;
            }
            else
            {
                var dirNode = ResolvePath(_DirectoryPath, rchCommandLine);
                if (dirNode is Directory directory)
                {
                    _ParentDirectory = directory;
                }
                else
                {
                    Feature.AddToCommandList($"'{_DirectoryPath}' is not a directory", rchCommandLine, false);
                    return;
                }
            }

            if (_ParentDirectory.FindChild(_FileName) != null)
            {
                File file = (File)_ParentDirectory.FindChild(_FileName);
                Feature.AddToCommandList($"{file.Content}", rchCommandLine, false);
                return;
            }
            else
            {
                Feature.AddToCommandList($"'{_FileName}' is not exist", rchCommandLine, false);
                return;
            }
        }

        // For moving file or directory (CUT)
        public void Mv(string[] inputs, RichTextBox rchCommandLine)
        {
            string _FirstFileName = null;
            string _FirstDirectoryPath = null;

            if (inputs[1].StartsWith(".") || inputs[1].StartsWith("/"))
                _FirstDirectoryPath = inputs[1];
            else
                _FirstFileName = GetFileName(inputs[1]);

            string _SecondFileName = null;
            string _SecondDirectoryPath = NodePathToString(CurrentDirectory);

            if (inputs[2].StartsWith(".") || inputs[2].StartsWith("/"))
                _SecondDirectoryPath = inputs[2];
            else
                _SecondFileName = GetFileName(inputs[2]);

            Directory _FirstDirectory = null;
            if (_FirstDirectoryPath != null)
            {

                if (_FirstDirectoryPath == ".")
                {
                    _FirstDirectory = CurrentDirectory;
                }
                else
                {
                    var _DirectoryNode = ResolvePath(_FirstDirectoryPath, rchCommandLine);
                    if (_DirectoryNode is Directory directory)
                    {
                        _FirstDirectory = directory;
                    }
                    else
                    {
                        Feature.AddToCommandList($"'{_FirstDirectoryPath}' is not a directory", rchCommandLine, false);
                        return;
                    }
                }
            }
            Directory _SecondDirectory;
            if (_SecondDirectoryPath == ".")
            {
                _SecondDirectory = CurrentDirectory;
            }
            else
            {
                var _DirectoryNode = ResolvePath(_SecondDirectoryPath, rchCommandLine);
                if (_DirectoryNode is Directory directory)
                {
                    _SecondDirectory = directory;
                }
                else
                {
                    Feature.AddToCommandList($"'{_SecondDirectoryPath}' is not a directory", rchCommandLine, false);
                    return;
                }
            }
            if (!CurrentDirectory.IsExistChildName(_FirstFileName) && _FirstFileName != null)
            {
                Feature.AddToCommandList($"'{_FirstFileName}' is not a file", rchCommandLine, false);
                return;
            }
            if (_SecondDirectory.IsExistChildName(_FirstFileName) && _FirstFileName != null && _SecondFileName == null)
            {
                Feature.AddToCommandList($"'{_FirstFileName}' is already exist in this path", rchCommandLine, false);
                return;
            }
            if (!CurrentDirectory.IsExistChildName(inputs[1].Trim().Split('/').ToArray().Last()) && _FirstDirectoryPath != null)
            {
                Feature.AddToCommandList($"'{_FirstDirectoryPath}' is not a folder", rchCommandLine, false);
                return;
            }
            if (_SecondDirectory.IsExistChildName(inputs[1].Trim().Split('/').ToArray().Last()) && _FirstDirectoryPath != null)
            {
                Feature.AddToCommandList($"'{_FirstDirectoryPath}' is already exist in this path", rchCommandLine, false);
                return;
            }

            if (_SecondFileName == null && _FirstDirectoryPath == null)
            {
                File file = (File)CurrentDirectory.FindChild(_FirstFileName);
                _SecondDirectory.AddChild(file);
                file.Parent = _SecondDirectory;
                CurrentDirectory.RemoveChild(inputs[1].Trim().Split('/').ToArray().Last());
            }
            else if (_FirstFileName == null && _SecondFileName == null)
            {
                _SecondDirectory.AddChild(_FirstDirectory);
                _FirstDirectory.Parent = _SecondDirectory;
                CurrentDirectory.RemoveChild(inputs[1].Trim().Split('/').ToArray().Last());
            }
            else if (_FirstDirectoryPath == null)
            {
                File file = (File)CurrentDirectory.FindChild(_FirstFileName);
                file.Name = _SecondFileName;
            }

        }

        // For copy and paste file or directory to specifided path
        public void Cp(string[] inputs, RichTextBox rchCommandLine)
        {
            string _FirstFileName = null;
            string _FirstDirectoryPath = null;

            if (inputs[1].StartsWith(".") || inputs[1].StartsWith("/"))
                _FirstDirectoryPath = inputs[1];
            else
                _FirstFileName = GetFileName(inputs[1]);

            string _SecondDirectoryPath = NodePathToString(CurrentDirectory);

            if (inputs[2].StartsWith(".") || inputs[2].StartsWith("/"))
                _SecondDirectoryPath = inputs[2];

            Directory _FirstDirectory = null;
            if (_FirstDirectoryPath != null)
            {

                if (_FirstDirectoryPath == ".")
                {
                    _FirstDirectory = CurrentDirectory;
                }
                else
                {
                    var _DirectoryNode = ResolvePath(_FirstDirectoryPath, rchCommandLine);
                    if (_DirectoryNode is Directory directory)
                    {
                        _FirstDirectory = directory;
                    }
                    else
                    {
                        Feature.AddToCommandList($"'{_FirstDirectoryPath}' is not a directory", rchCommandLine, false);
                        return;
                    }
                }
            }
            Directory _SecondDirectory;
            if (_SecondDirectoryPath == ".")
            {
                _SecondDirectory = CurrentDirectory;
            }
            else
            {
                var _DirectoryNode = ResolvePath(_SecondDirectoryPath, rchCommandLine);
                if (_DirectoryNode is Directory directory)
                {
                    _SecondDirectory = directory;
                }
                else
                {
                    Feature.AddToCommandList($"'{_SecondDirectoryPath}' is not a directory", rchCommandLine, false);
                    return;
                }
            }
            if (!CurrentDirectory.IsExistChildName(_FirstFileName) && _FirstFileName != null)
            {
                Feature.AddToCommandList($"'{_FirstFileName}' is not a file", rchCommandLine, false);
                return;
            }
            if (_SecondDirectory.IsExistChildName(_FirstFileName) && _FirstFileName != null)
            {
                Feature.AddToCommandList($"'{_FirstFileName}' is already exist in this path", rchCommandLine, false);
                return;
            }
            if (!CurrentDirectory.IsExistChildName(inputs[1].Trim().Split('/').ToArray().Last()) && _FirstDirectoryPath != null)
            {
                Feature.AddToCommandList($"'{_FirstDirectoryPath}' is not a folder", rchCommandLine, false);
                return;
            }
            if (_SecondDirectory.IsExistChildName(inputs[1].Trim().Split('/').ToArray().Last()) && _FirstDirectoryPath != null)
            {
                Feature.AddToCommandList($"'{_FirstDirectoryPath}' is already exist in this path", rchCommandLine, false);
                return;
            }

            if (_FirstDirectoryPath == null)
            {
                File file = (File)CurrentDirectory.FindChild(_FirstFileName);
                _SecondDirectory.AddChild(file);
            }
            else
            {
                Directory temp = _FirstDirectory;
                _SecondDirectory.AddChild(temp);
            }

        }
    }

}
