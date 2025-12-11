using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace VirtualFileSystemSimulatorWinForm
{
    public class SystemFile
    {
        public Directory Root { get; set; }
        public Directory CurrentDirectory { get; private set; }
        public Features Feature = new Features();
        public Json UserManager { get; set; }
        public string SystemName { get; set; }
        public string CommitVersion { get; set; }

        public SystemFile(Json userManager)
        {
            UserManager = userManager;
            SystemName = UserManager.CurrentUser.Username;
            CommitVersion = "main";
            Root = (Directory)UserManager.LoadVfsForCurrentUser(SystemName, CommitVersion);
            CurrentDirectory = Root;
        }

        // ذخیره بعد از عملیات
        public void Save()
        {
            UserManager.SaveVfsForCurrentUser(Root, SystemName, CommitVersion);
        }

        //برای گروه بندی ها 
        //همه اول زیر مجموعه ادمین هستن
        //گروه اسم owner افراد هستش
        public void LoadAnotherSystemFile(string systemname, string commitversion, RichTextBox rchCommandLine)
        {
            if (!UserManager.UserIsExist(systemname))
            {
                Feature.AddToCommandList("This SystemFile is not exist", rchCommandLine, false);
                return;
            }
            string[,] _UsersAdnGroups = new string[UserManager.CurrentUser.Groups.Split('/').ToList().Count(), 2];
            for (int i = 0; i < UserManager.CurrentUser.Groups.Split('/').ToList().Count(); i++)
            {
                _UsersAdnGroups[i, 0] = UserManager.CurrentUser.Groups.Split('/')[i].Split(',')[0];
                _UsersAdnGroups[i, 1] = UserManager.CurrentUser.Groups.Split('/')[i].Split(',')[1];
            }
            systemname = systemname.ToLower();
            if (!UserManager.GetAllCommits(systemname).Contains(commitversion))
            {
                Feature.AddToCommandList("This Commit of SystemFile is not exist", rchCommandLine, false);
                return;
            }
            if (systemname == UserManager.CurrentUser.Username)
            {
                SystemName = systemname;
                UserManager.CurrentUser.UserType = (int)User.UserTypeEnum.Admin;

                CommitVersion = commitversion;

                Root = (Directory)UserManager.LoadVfsForCurrentUser(SystemName, commitversion);
                CurrentDirectory = Root;
                return;
            }
            for (int i = 0; i < UserManager.CurrentUser.Groups.Split('/').ToList().Count(); i++)
            {
                if (_UsersAdnGroups[i, 0] == systemname)
                {
                    SystemName = systemname;
                    switch (_UsersAdnGroups[i, 1])
                    {
                        case "owner":
                            UserManager.CurrentUser.UserType = (int)User.UserTypeEnum.Owner;
                            break;
                        case "group":
                            UserManager.CurrentUser.UserType = (int)User.UserTypeEnum.Group;
                            break;
                        case "other":
                            UserManager.CurrentUser.UserType = (int)User.UserTypeEnum.Others;
                            break;
                        case "admin":
                            UserManager.CurrentUser.UserType = (int)User.UserTypeEnum.Admin;
                            break;
                        default:
                            break;
                    }
                    CommitVersion = commitversion;
                    Root = (Directory)UserManager.LoadVfsForCurrentUser(SystemName, commitversion);
                    CurrentDirectory = Root;
                    return;
                }
            }
            Feature.AddToCommandList("You can not open this SystemFile", rchCommandLine, false);
            return;
        }
        public void AddGroupsForUser(string username, User.UserTypeEnum newusertype, RichTextBox rchCommandLine)
        {
            if (!UserManager.UserIsExist(username))
            {
                Feature.AddToCommandList("This User is not exist", rchCommandLine, false);
                return;
            }
            UserManager.AddToCurrentGroup(username, newusertype);
        }
        public void RemoveGroupsForUser(string groupname, string username, RichTextBox rchCommandLine)
        {
            if (UserManager.CurrentUser.Username != groupname)
            {
                Feature.AddToCommandList("You can not remove this user of group", rchCommandLine, false);
                return;
            }
            if (!UserManager.UserIsExist(username))
            {
                Feature.AddToCommandList("This User is not exist", rchCommandLine, false);
                return;
            }

            if (UserManager.RemoveGroup(username, groupname))
            {
                Feature.AddToCommandList("Susecc", rchCommandLine, false);
                return;
            }
            Feature.AddToCommandList("This User do not have this permission", rchCommandLine, false);
            return;
        }



        // For Show User Type 
        public User.UserTypeEnum ShowUserType()
        {
            return (User.UserTypeEnum)UserManager.CurrentUser.UserType;
        }
        public void ChangeUserType(string username, User.UserTypeEnum usertype, RichTextBox rchCommandLine)
        {
            if (!UserManager.CurrentUser.IsAdmin())
            {
                Feature.AddToCommandList("You do not have Permission to Change UserType", rchCommandLine, false);
                return;
            }
            if (UserManager.ChangeUserType(username, usertype))
            {
                Feature.AddToCommandList("Success", rchCommandLine, false);
                return;
            }
            Feature.AddToCommandList("This username is not valid", rchCommandLine, false);
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
            if (!UserManager.CurrentUser.HasPermission(current, "w"))
            {
                Feature.AddToCommandList("You do not have Permission to make directory in this path", rchCommandLine, false);
                return;
            }
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
                    var _NewDirectory = new Directory(_CurrentPart, current, owner: UserManager.CurrentUser.Username);
                    current.AddChild(_NewDirectory);
                    CurrentDirectory = _NewDirectory;
                }
                else
                {
                    if (createparents)
                    {
                        var _NewDirectory = new Directory(_CurrentPart, current, owner: UserManager.CurrentUser.Username);
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

                if (!UserManager.CurrentUser.HasPermission(_ParentDirectory, "w"))
                {
                    Feature.AddToCommandList("You do not have Permission to make file in this path", rchCommandLine, false);
                    return;
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
                    _NewFile = new File(_FileName.Split('.')[0], fileType: _FileExtension, owner: UserManager.CurrentUser.Username);
                }
                else
                {
                    // For files we can not show there
                    _NewFile = new File("." + _FileName.Split('.')[1], fileType: _FileExtension, owner: UserManager.CurrentUser.Username);
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
                Save();
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
                if (!UserManager.CurrentUser.HasPermission(_DirectoryNode, "w"))
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
            Save();
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

                    if (!UserManager.CurrentUser.HasPermission(_DirectoryNode, "w"))
                    {
                        Feature.AddToCommandList("You do not have Permission to make link in this path", rchCommandLine, false);
                        return;
                    }

                    if (CurrentDirectory.IsExistChildName(_Name))
                    {
                        Feature.AddToCommandList($"this name is exist !", rchCommandLine, false);
                        return;
                    }
                    CurrentDirectory.AddChild(new File(_Name, isLink: true, link: _Path, owner: UserManager.CurrentUser.Username));

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

                    if (!UserManager.CurrentUser.HasPermission(_DirectoryNode, "w"))
                    {
                        Feature.AddToCommandList("You do not have Permission to make link in this path", rchCommandLine, false);
                        return;
                    }
                }
                else
                {
                    // For find name
                    _DirectoryNode = CurrentDirectory.FindChild(_Path.Split('.')[0]);

                    if (!UserManager.CurrentUser.HasPermission(_DirectoryNode, "w"))
                    {
                        Feature.AddToCommandList("You do not have Permission to make link for this file", rchCommandLine, false);
                        return;
                    }
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
                    var hardLink = new File(_Name, fileType: targetFile.FileType, content: targetFile.Content, isLink: targetFile.IsLink, link: targetFile.Link, owner: UserManager.CurrentUser.Username)
                    {
                        Permissions = targetFile.Permissions
                    };

                    CurrentDirectory.AddChild(hardLink);
                }


            }
            Save();
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
            if (!UserManager.CurrentUser.HasPermission(_DirectoryNode, "r"))
            {
                Feature.AddToCommandList("You do not have Permission to get information about this", rchCommandLine, false);
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
            if (!UserManager.CurrentUser.HasPermission(_ParentDirectory, "r"))
            {
                Feature.AddToCommandList("You do not have Permission to read file's content", rchCommandLine, false);
                return;
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
                if (!UserManager.CurrentUser.HasPermission(_FirstDirectory, "w"))
                {
                    Feature.AddToCommandList("You do not have Permission to move", rchCommandLine, false);
                    return;
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
                if (!UserManager.CurrentUser.HasPermission(_SecondDirectory, "w"))
                {
                    Feature.AddToCommandList("You do not have Permission to move", rchCommandLine, false);
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
                if (!UserManager.CurrentUser.HasPermission(file, "w"))
                {
                    Feature.AddToCommandList("You do not have Permission to move", rchCommandLine, false);
                    return;
                }
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
                if (!UserManager.CurrentUser.HasPermission(file, "w"))
                {
                    Feature.AddToCommandList("You do not have Permission to change name", rchCommandLine, false);
                    return;
                }
                file.Name = _SecondFileName;
            }
            Save();
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
                if (!UserManager.CurrentUser.HasPermission(_FirstDirectory, "w"))
                {
                    Feature.AddToCommandList("You do not have Permission to copy", rchCommandLine, false);
                    return;
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
            if (!UserManager.CurrentUser.HasPermission(_SecondDirectory, "w"))
            {
                Feature.AddToCommandList("You do not have Permission to copy", rchCommandLine, false);
                return;
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
                if (!UserManager.CurrentUser.HasPermission(file, "w"))
                {
                    Feature.AddToCommandList("You do not have Permission to copy", rchCommandLine, false);
                    return;
                }
                _SecondDirectory.AddChild(file);
            }
            else
            {
                Directory temp = _FirstDirectory;
                _SecondDirectory.AddChild(temp);
            }
            Save();
        }
        public void Chmod(string permission, string file_directory, RichTextBox rchCommandLine)
        {
            Node _FileOrDirectory = CurrentDirectory.FindChild(file_directory);
            char[] _PermissionChars = _FileOrDirectory.Permissions.ToCharArray();

            if (_FileOrDirectory == null)
            {
                Feature.AddToCommandList($"Error: '{file_directory}' not found", rchCommandLine, false);
                return;
            }

            if (int.TryParse(permission, out int _PermissionCode))
            {
                if (_PermissionCode < 0 || _PermissionCode > 777)
                {
                    Feature.AddToCommandList($"Error: Permission code '{_PermissionCode}' is invalid. Must be between 000 and 777", rchCommandLine, false);
                    return;
                }

                // اعمال مجوزها به صورت دستی رقم به رقم
                int tempCode = _PermissionCode;
                int position = 0; // 0: others, 1: group, 2: owner

                while (tempCode > 0)
                {
                    int digit = tempCode % 10;

                    // بررسی اینکه رقم معتبر باشد (0-7)
                    if (digit < 0 || digit > 7)
                    {
                        Feature.AddToCommandList($"Error: Invalid permission digit '{digit}'", rchCommandLine, false);
                        return;
                    }

                    // تبدیل رقم به مجوزهای خواندن/نوشتن/اجرا
                    // Owner (موقعیت 2), Group (موقعیت 1), Others (موقعیت 0)
                    switch (position)
                    {
                        case 2: // Owner
                            if ((digit & 4) != 0)
                            {
                                _PermissionChars[0] = 'r';
                            }
                            if ((digit & 2) != 0)
                            {
                                _PermissionChars[1] = 'w';
                            }
                            if ((digit & 1) != 0)
                            {
                                _PermissionChars[2] = 'x';
                            }
                            break;
                        case 1: // Group
                            if ((digit & 4) != 0)
                            {
                                _PermissionChars[3] = 'r';
                            }
                            if ((digit & 2) != 0)
                            {
                                _PermissionChars[4] = 'w';
                            }
                            if ((digit & 1) != 0)
                            {
                                _PermissionChars[5] = 'x';
                            }
                            break;
                        case 0: // Others
                            if ((digit & 4) != 0)
                            {
                                _PermissionChars[6] = 'r';
                            }
                            if ((digit & 2) != 0)
                            {
                                _PermissionChars[7] = 'w';
                            }
                            if ((digit & 1) != 0)
                            {
                                _PermissionChars[8] = 'x';
                            }
                            break;
                    }

                    tempCode /= 10;
                    position++;

                    // جلوگیری از بیش از 3 رقم
                    if (position > 3)
                    {
                        Feature.AddToCommandList("Warning: Permission code has more than 3 digits. Using first 3 digits", rchCommandLine, false);
                        return;
                    }
                }
                _FileOrDirectory.Permissions = string.Join("", _PermissionChars);
                Feature.AddToCommandList($"Permissions for '{file_directory}' changed to {_PermissionCode:D3}", rchCommandLine, false);
            }
            // اگر permission به فرمت نمادین (مثل u+rwx,g+rx,o+r) بود
            else if (IsSymbolicPermission(permission))
            {
                ApplySymbolicPermission(_FileOrDirectory, permission, rchCommandLine, ref _PermissionChars);
                _FileOrDirectory.Permissions = string.Join("", _PermissionChars);
                Feature.AddToCommandList($"Permissions for '{file_directory}' changed using symbolic mode: {permission}", rchCommandLine, false);
            }
            else
            {
                Feature.AddToCommandList($"Error: Invalid permission format '{permission}'. Use numeric (e.g., 755) or symbolic (e.g., u+rwx) format", rchCommandLine, false);
            }
            Save();
        }

        // متد کمکی برای تشخیص فرمت نمادین
        private bool IsSymbolicPermission(string permission)
        {
            // الگوی ساده برای فرمت نمادین (مثل u+rwx,g+rx,o+r)
            var pattern = @"^([ownergroupother]*[+=-][rwxXst]*,?)+$";
            return Regex.IsMatch(permission, pattern, RegexOptions.IgnoreCase);
        }

        // متد کمکی برای اعمال مجوزهای نمادین
        private void ApplySymbolicPermission(Node node, string symbolicPermission, RichTextBox rchCommandLine, ref char[] permissionchars)
        {
            string[] parts = symbolicPermission.Split(',');

            foreach (var part in parts)
            {
                if (part.Contains("+"))
                {
                    string[] a = part.Split('+');
                    if (a[0] == "owner")
                    {
                        char[] b = a[1].ToCharArray();
                        if (b.Contains('r'))
                        {
                            permissionchars[0] = 'r';
                        }
                        if (b.Contains('w'))
                        {
                            permissionchars[1] = 'w';
                        }
                        if (b.Contains('x'))
                        {
                            permissionchars[2] = 'x';
                        }
                    }
                    else if (a[0] == "group")
                    {
                        char[] b = a[1].ToCharArray();
                        if (b.Contains('r'))
                        {
                            permissionchars[3] = 'r';
                        }
                        if (b.Contains('w'))
                        {
                            permissionchars[4] = 'w';
                        }
                        if (b.Contains('x'))
                        {
                            permissionchars[5] = 'x';
                        }
                    }
                    else if (a[0] == "other")
                    {
                        char[] b = a[1].ToCharArray();
                        if (b.Contains('r'))
                        {
                            permissionchars[6] = 'r';
                        }
                        if (b.Contains('w'))
                        {
                            permissionchars[7] = 'w';
                        }
                        if (b.Contains('x'))
                        {
                            permissionchars[8] = 'x';
                        }
                    }
                }
                else if (part.Contains("-"))
                {
                    string[] a = part.Split('-');
                    if (a[0] == "owner")
                    {
                        char[] b = a[1].ToCharArray();
                        if (b.Contains('r'))
                        {
                            permissionchars[0] = '-';
                        }
                        if (b.Contains('w'))
                        {
                            permissionchars[1] = '-';
                        }
                        if (b.Contains('x'))
                        {
                            permissionchars[2] = '-';
                        }
                    }
                    else if (a[0] == "group")
                    {
                        char[] b = a[1].ToCharArray();
                        if (b.Contains('r'))
                        {
                            permissionchars[3] = '-';
                        }
                        if (b.Contains('w'))
                        {
                            permissionchars[4] = '-';
                        }
                        if (b.Contains('x'))
                        {
                            permissionchars[5] = '-';
                        }
                    }
                    else if (a[0] == "other")
                    {
                        char[] b = a[1].ToCharArray();
                        if (b.Contains('r'))
                        {
                            permissionchars[6] = '-';
                        }
                        if (b.Contains('w'))
                        {
                            permissionchars[7] = '-';
                        }
                        if (b.Contains('x'))
                        {
                            permissionchars[8] = '-';
                        }
                    }
                }
                else if (part.Contains("="))
                {
                    string[] a = part.Split('=');
                    if (a[0] == "owner")
                    {
                        char[] b = a[1].ToCharArray();
                        if (b.Contains('r'))
                        {
                            permissionchars[0] = 'r';
                        }
                        if (b.Contains('w'))
                        {
                            permissionchars[1] = 'w';
                        }
                        if (b.Contains('x'))
                        {
                            permissionchars[2] = 'x';
                        }
                    }
                    else if (a[0] == "group")
                    {
                        char[] b = a[1].ToCharArray();
                        if (b.Contains('r'))
                        {
                            permissionchars[3] = 'r';
                        }
                        if (b.Contains('w'))
                        {
                            permissionchars[4] = 'w';
                        }
                        if (b.Contains('x'))
                        {
                            permissionchars[5] = 'x';
                        }
                    }
                    else if (a[0] == "other")
                    {
                        char[] b = a[1].ToCharArray();
                        if (b.Contains('r'))
                        {
                            permissionchars[6] = 'r';
                        }
                        if (b.Contains('w'))
                        {
                            permissionchars[7] = 'w';
                        }
                        if (b.Contains('x'))
                        {
                            permissionchars[8] = 'x';
                        }
                    }
                }
            }
        }
        public void Find(string path, string option, string pattern, RichTextBox rchCommandLine)
        {
            Directory _SearchDirectory;

            if (path == ".")
            {
                _SearchDirectory = CurrentDirectory;
            }
            else if (path == "~")
            {
                _SearchDirectory = Root;
            }
            else
            {
                var dirNode = ResolvePath(path, rchCommandLine);
                if (dirNode is Directory directory)
                {
                    _SearchDirectory = directory;
                }
                else
                {
                    Feature.AddToCommandList($"'{path}' is not a directory", rchCommandLine, false);
                    return;
                }
            }

            // جدا کردن option (مثلاً -name)
            if (option != "-name" && option != "-type")
            {
                Feature.AddToCommandList($"Unsupported option: {option}", rchCommandLine, false);
                return;
            }

            // اگر option مربوط به نام فایل است
            if (option == "-name")
            {
                SearchByName(_SearchDirectory, pattern, rchCommandLine);
            }
            else if (option == "-type")
            {
                // برای جستجوی بر اساس نوع (f برای فایل، d برای دایرکتوری)
                //// جستجوی تمام دایرکتوری‌ها
                //Find(".", "-type", "d", richTextBox1);

                SearchByType(_SearchDirectory, pattern, rchCommandLine);
            }
        }

        // متد بازگشتی برای جستجو بر اساس نام
        private void SearchByName(Directory currentDir, string pattern, RichTextBox rchCommandLine)
        {
            foreach (var item in currentDir.Children)
            {


                // بررسی تطابق با الگوی wildcard
                if (item is File file)
                {
                    string fullName = file.Name + "." + file.FileType;

                    if (IsPatternMatch(fullName, pattern))
                    {
                        Feature.AddToCommandList(
                            NodePathToString((Directory)item.Parent) + '/' + fullName,
                            rchCommandLine,
                            false
                        );
                    }
                }


                else if (item is Directory directory)
                {
                    // جستجوی بازگشتی در زیردایرکتوری‌ها
                    SearchByName(directory, pattern, rchCommandLine);
                }
            }
        }

        // متد برای جستجو بر اساس نوع
        private void SearchByType(Directory currentDir, string type, RichTextBox rchCommandLine)
        {
            foreach (var item in currentDir.Children)
            {
                if (type == "f" && item is File file)
                {
                    Feature.AddToCommandList(NodePathToString((Directory)item.Parent) + '/' + file.Name + '.' + file.FileType, rchCommandLine, false);
                }
                else if (type == "d" && item is Directory)
                {
                    Feature.AddToCommandList(NodePathToString((Directory)item), rchCommandLine, false);
                }

                // اگر آیتم دایرکتوری است، به صورت بازگشتی ادامه بده
                if (item is Directory directory)
                {
                    SearchByType(directory, type, rchCommandLine);
                }
            }
        }

        // متد برای تطبیق الگوی wildcard (ساده)
        private bool IsPatternMatch(string fileName, string pattern)
        {
            // 1. ابتدا wildcard‌ها را به معادل regex تبدیل کنیم
            string regexPattern = pattern
                .Replace(".", "\\.")  // ابتدا نقطه را escape کنیم
                .Replace("*", ".*")   // سپس * را به .* تبدیل کنیم
                .Replace("?", ".");   // سپس ? را به . تبدیل کنیم

            // 2. حالا بقیه کاراکترهای خاص regex را escape کنیم
            // اما قبلاً نقطه را escape کرده‌ایم، پس باید مراقب باشیم
            regexPattern = "^" + regexPattern + "$";

            return Regex.IsMatch(fileName, regexPattern, RegexOptions.IgnoreCase);
        }

        public void Revert(string commitrevert, RichTextBox rchCommandLine)
        {
            LoadAnotherSystemFile(SystemName, commitrevert, rchCommandLine);
        }
        public void Commit(string commitname, RichTextBox rchCommandLine)
        {
            if (UserManager.CopyVfsWithCommit(SystemName, CommitVersion, commitname))
            {
                Feature.AddToCommandList("Success", rchCommandLine, false);
                return;

            }
            Feature.AddToCommandList("failed", rchCommandLine, false);
            return;
        }
        public void CommitList(RichTextBox rchCommandLine)
        {
            foreach (var item in UserManager.GetAllCommits(SystemName))
                Feature.AddToCommandList($"{item}", rchCommandLine, false);
        }
        public void DeleteCommit(string commitname, RichTextBox rchCommandLine)
        {
            if (commitname == CommitVersion)
            {
                Feature.AddToCommandList("You already in this commit ,so you can not delete this", rchCommandLine, false);
                return;
            }

            if (UserManager.DeleteCommit(SystemName, commitname))
            {
                Feature.AddToCommandList("Success", rchCommandLine, false);
                return;

            }
            Feature.AddToCommandList("failed", rchCommandLine, false);
            return;
        }
    }

}
