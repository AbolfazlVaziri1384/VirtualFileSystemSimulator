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

        public void Save()
        {
            UserManager.SaveVfsForCurrentUser(Root, SystemName, CommitVersion);
        }

        public void LoadAnotherSystemFile(string systemname, string commitversion, RichTextBox rchCommandLine)
        {
            try
            {
                if (!UserManager.UserIsExist(systemname))
                {
                    Feature.AddToCommandList("The specified system file does not exist. Please check the name and try again.", rchCommandLine, false);
                    return;
                }

                string[,] _UsersAdnGroups = new string[UserManager.CurrentUser.Groups.Split('/').ToList().Count(), 2];

                try
                {
                    for (int i = 0; i < UserManager.CurrentUser.Groups.Split('/').ToList().Count(); i++)
                    {
                        _UsersAdnGroups[i, 0] = UserManager.CurrentUser.Groups.Split('/')[i].Split(',')[0];
                        _UsersAdnGroups[i, 1] = UserManager.CurrentUser.Groups.Split('/')[i].Split(',')[1];
                    }
                }
                catch (Exception ex)
                {
                    Feature.AddToCommandList($"Error processing user groups: {ex.Message}. Please contact administrator.", rchCommandLine, false);
                    return;
                }

                systemname = systemname.ToLower();

                if (!UserManager.GetAllCommits(systemname).Contains(commitversion))
                {
                    Feature.AddToCommandList($"Commit version '{commitversion}' not found for system file '{systemname}'. Please verify the commit version.", rchCommandLine, false);
                    return;
                }

                if (systemname == UserManager.CurrentUser.Username)
                {
                    try
                    {
                        SystemName = systemname;
                        UserManager.CurrentUser.UserType = (int)User.UserTypeEnum.Admin;
                        CommitVersion = commitversion;
                        Root = (Directory)UserManager.LoadVfsForCurrentUser(SystemName, commitversion);
                        CurrentDirectory = Root;
                        Feature.AddToCommandList($"Successfully opened your system file '{SystemName}' with commit version '{CommitVersion}'.", rchCommandLine, false);
                    }
                    catch (Exception ex)
                    {
                        Feature.AddToCommandList($"Error loading your system file: {ex.Message}. Please try again.", rchCommandLine, false);
                        return;
                    }
                    return;
                }

                bool accessGranted = false;
                for (int i = 0; i < UserManager.CurrentUser.Groups.Split('/').ToList().Count(); i++)
                {
                    if (_UsersAdnGroups[i, 0] == systemname)
                    {
                        try
                        {
                            SystemName = systemname;

                            switch (_UsersAdnGroups[i, 1])
                            {
                                case "owner":
                                    UserManager.CurrentUser.UserType = (int)User.UserTypeEnum.Owner;
                                    Feature.AddToCommandList($"Access granted as Owner to system file '{SystemName}'.", rchCommandLine, false);
                                    break;
                                case "group":
                                    UserManager.CurrentUser.UserType = (int)User.UserTypeEnum.Group;
                                    Feature.AddToCommandList($"Access granted as Group member to system file '{SystemName}'.", rchCommandLine, false);
                                    break;
                                case "other":
                                    UserManager.CurrentUser.UserType = (int)User.UserTypeEnum.Others;
                                    Feature.AddToCommandList($"Access granted with Others permission to system file '{SystemName}'.", rchCommandLine, false);
                                    break;
                                case "admin":
                                    UserManager.CurrentUser.UserType = (int)User.UserTypeEnum.Admin;
                                    Feature.AddToCommandList($"Access granted as Administrator to system file '{SystemName}'.", rchCommandLine, false);
                                    break;
                                default:
                                    Feature.AddToCommandList($"Access granted with unknown permission type to system file '{SystemName}'.", rchCommandLine, false);
                                    break;
                            }

                            CommitVersion = commitversion;
                            Root = (Directory)UserManager.LoadVfsForCurrentUser(SystemName, commitversion);
                            CurrentDirectory = Root;
                            accessGranted = true;
                        }
                        catch (Exception ex)
                        {
                            Feature.AddToCommandList($"Error loading system file '{systemname}': {ex.Message}. Please try again.", rchCommandLine, false);
                            return;
                        }
                        break;
                    }
                }

                if (!accessGranted)
                {
                    Feature.AddToCommandList($"Access denied. You do not have permission to open system file '{systemname}'. Please contact the file owner or administrator.", rchCommandLine, false);
                    return;
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"An unexpected error occurred: {ex.Message}. Please try again or contact support.", rchCommandLine, false);
                return;
            }
        }
        public void AddGroupsForUser(string username, User.UserTypeEnum newusertype, RichTextBox rchCommandLine)
        {
            try
            {
                if (!UserManager.UserIsExist(username))
                {
                    Feature.AddToCommandList($"User '{username}' does not exist. Please check the username and try again.", rchCommandLine, false);
                    return;
                }

                try
                {
                    UserManager.AddToCurrentGroup(username, newusertype);
                    Feature.AddToCommandList($"Successfully added user '{username}' to the group with '{newusertype}' permission level.", rchCommandLine, false);
                }
                catch (Exception ex)
                {
                    Feature.AddToCommandList($"Failed to add user '{username}' to the group. Error: {ex.Message}. Please try again.", rchCommandLine, false);
                    return;
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"An unexpected error occurred while processing group addition: {ex.Message}. Please try again or contact support.", rchCommandLine, false);
                return;
            }
        }
        public void RemoveGroupsForUser(string groupname, string username, RichTextBox rchCommandLine)
        {
            try
            {
                if (UserManager.CurrentUser.Username != groupname)
                {
                    Feature.AddToCommandList($"Permission denied. You can only remove users from groups that you own. Current group owner: '{groupname}', Your username: '{UserManager.CurrentUser.Username}'.", rchCommandLine, false);
                    return;
                }

                if (!UserManager.UserIsExist(username))
                {
                    Feature.AddToCommandList($"User '{username}' does not exist. Please verify the username and try again.", rchCommandLine, false);
                    return;
                }

                try
                {
                    if (UserManager.RemoveGroup(username, groupname))
                    {
                        Feature.AddToCommandList($"Successfully removed user '{username}' from group '{groupname}'.", rchCommandLine, false);
                        return;
                    }

                    Feature.AddToCommandList($"User '{username}' does not have permission for group '{groupname}' or is not a member of this group.", rchCommandLine, false);
                    return;
                }
                catch (Exception ex)
                {
                    Feature.AddToCommandList($"Error removing user '{username}' from group '{groupname}': {ex.Message}. Please try again.", rchCommandLine, false);
                    return;
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"An unexpected error occurred during group removal: {ex.Message}. Please try again or contact support.", rchCommandLine, false);
                return;
            }
        }



        // For Show User Type 
        public User.UserTypeEnum ShowUserType()
        {
            return (User.UserTypeEnum)UserManager.CurrentUser.UserType;
        }
        public void ChangeUserType(string username, User.UserTypeEnum usertype, RichTextBox rchCommandLine)
        {
            try
            {
                if (!UserManager.CurrentUser.IsAdmin())
                {
                    Feature.AddToCommandList("Permission denied. Only administrators can change user types. Please contact your system administrator.", rchCommandLine, false);
                    return;
                }

                try
                {
                    if (UserManager.ChangeUserType(username, usertype))
                    {
                        Feature.AddToCommandList($"Successfully changed user type for '{username}' to '{usertype}'.", rchCommandLine, false);
                        return;
                    }

                    Feature.AddToCommandList($"Operation failed. User '{username}' may not exist or the user type change could not be processed.", rchCommandLine, false);
                }
                catch (Exception ex)
                {
                    Feature.AddToCommandList($"Error changing user type for '{username}': {ex.Message}. Please verify the username and try again.", rchCommandLine, false);
                    return;
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"An unexpected error occurred during user type change: {ex.Message}. Please try again or contact support.", rchCommandLine, false);
                return;
            }
        }

        // For managing ".." or "." in the path
        private void NormalizePath(ref string path)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Error normalizing path '{path}': {ex.Message}");
            }
        }

        // For making directory
        public void Mkdir(string path, bool createparents, RichTextBox rchCommandLine)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    Feature.AddToCommandList("Path cannot be empty. Please specify a directory name or path.", rchCommandLine, false);
                    return;
                }

                if (createparents)
                {
                    try
                    {
                        NormalizePath(ref path);
                    }
                    catch (Exception ex)
                    {
                        Feature.AddToCommandList($"Error processing path: {ex.Message}. Please check the path format.", rchCommandLine, false);
                        return;
                    }
                }

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
                Feature.AddToCommandList($"Directory creation process completed for path: '{path}'.", rchCommandLine, false);
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"An unexpected error occurred while creating directory: {ex.Message}. Please try again.", rchCommandLine, false);
                return;
            }
        }

        // For Create Directory
        private void CreateDirectory(Directory current, string[] parts, int index, bool createparents, RichTextBox rchCommandLine)
        {
            try
            {
                if (!UserManager.CurrentUser.HasPermission(current, "w"))
                {
                    Feature.AddToCommandList($"Permission denied. You do not have write permission in directory '{current.Name}'.", rchCommandLine, false);
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
                            Feature.AddToCommandList($"Directory '{_CurrentPart}' already exists in this location.", rchCommandLine, false);
                    }
                    else
                    {
                        Feature.AddToCommandList($"Cannot create directory '{_CurrentPart}' because a file with this name already exists.", rchCommandLine, false);
                    }
                }
                else
                {
                    if (index == parts.Length - 1)
                    {
                        var _NewDirectory = new Directory(_CurrentPart, current, owner: UserManager.CurrentUser.Username);
                        current.AddChild(_NewDirectory);
                        CurrentDirectory = _NewDirectory;
                        Feature.AddToCommandList($"Directory '{_CurrentPart}' created successfully.", rchCommandLine, false);
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
                            Feature.AddToCommandList($"Parent directory '{_CurrentPart}' does not exist. Use the -p flag to create parent directories automatically.", rchCommandLine, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error creating directory '{parts[index]}': {ex.Message}. Please try again.", rchCommandLine, false);
                return;
            }
        }

        // Extraction path from "path + file name"
        private string GetDirectoryPath(string fullpath)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Error extracting directory path from '{fullpath}': {ex.Message}");
            }
        }

        // Extraction file name from "path + file name"
        private string GetFileName(string fullpath)
        {
            try
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
            catch (Exception ex)
            {
                throw new Exception($"Error extracting file name from '{fullpath}': {ex.Message}");
            }
        }

        // For detect Relative or Absolute path
        public Node ResolvePath(string path, RichTextBox rchCommandLine)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    Feature.AddToCommandList("Path cannot be empty. Please specify a valid path.", rchCommandLine, false);
                    return null;
                }

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
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error resolving path '{path}': {ex.Message}. Please check the path format.", rchCommandLine, false);
                return null;
            }
        }

        private Node ResolveAbsolutePath(string path, RichTextBox rchCommandLine)
        {
            try
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
                        // Last part - might be a file
                    }
                    else if (_Child != null)
                    {
                        return _Child;
                    }
                    else
                    {
                        Feature.AddToCommandList($"Path not found: '{path}'. The directory or file '{_Part}' does not exist.", rchCommandLine, false);
                        return null;
                    }
                }

                return _Current;
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error resolving absolute path '{path}': {ex.Message}.", rchCommandLine, false);
                return null;
            }
        }

        private Node ResolveRelativePath(string path, RichTextBox rchCommandLine)
        {
            try
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
                        Feature.AddToCommandList($"Path not found: '{path}'. The directory or file '{part}' does not exist in the current location.", rchCommandLine, false);
                        return null;
                    }
                }
                return _Current;
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error resolving relative path '{path}': {ex.Message}.", rchCommandLine, false);
                return null;
            }
        }

        // For making file
        public void Touch(string pathandfilename, RichTextBox rchCommandLine, string customtime = null, string content = null)
        {
            try
            {
                if (string.IsNullOrEmpty(pathandfilename))
                {
                    Feature.AddToCommandList("Path and filename cannot be empty. Please specify a file name.", rchCommandLine, false);
                    return;
                }

                string _DirectoryPath;
                string _FileName;

                try
                {
                    _DirectoryPath = GetDirectoryPath(pathandfilename);
                    _FileName = GetFileName(pathandfilename);
                }
                catch (Exception ex)
                {
                    Feature.AddToCommandList($"Error parsing path '{pathandfilename}': {ex.Message}. Please check the path format.", rchCommandLine, false);
                    return;
                }

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
                    else if (_DirectoryNode == null)
                    {
                        // Error message already shown by ResolvePath
                        return;
                    }
                    else
                    {
                        Feature.AddToCommandList($"'{_DirectoryPath}' is not a valid directory. It might be a file.", rchCommandLine, false);
                        return;
                    }
                }

                if (!UserManager.CurrentUser.HasPermission(_ParentDirectory, "w"))
                {
                    Feature.AddToCommandList($"Permission denied. You do not have write permission in directory '{_ParentDirectory.Name}'.", rchCommandLine, false);
                    return;
                }

                // Check if file already exists
                try
                {
                    if (_FileName.StartsWith("."))
                    {
                        string[] parts = _FileName.Split('.');
                        // Hidden file - check full name
                        if (_ParentDirectory.FindChild('.' + parts[1]) != null)
                        {
                            Feature.AddToCommandList($"File '{'.' + parts[1]}' already exists. Please choose a different name.", rchCommandLine, false);
                            return;
                        }
                    }
                    else
                    {
                        // Regular file - check name without extension
                        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_FileName);
                        if (_ParentDirectory.FindChild(fileNameWithoutExtension) != null)
                        {
                            Feature.AddToCommandList($"File '{_FileName}' already exists. Please choose a different name.", rchCommandLine, false);
                            return;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Feature.AddToCommandList($"Error checking for existing file: {ex.Message}.", rchCommandLine, false);
                    return;
                }

                // Extract file extension
                string _FileExtension;
                try
                {
                    if (_FileName.StartsWith("."))
                    {
                        // For hidden files
                        string[] parts = _FileName.Split('.');
                        if (parts.Length > 2 && !string.IsNullOrEmpty(parts[parts.Length - 1]))
                        {
                            _FileExtension = parts[parts.Length - 1];
                        }
                        else
                        {
                            _FileExtension = "txt";
                        }
                    }
                    else
                    {
                        // For regular files
                        _FileExtension = Path.GetExtension(_FileName).TrimStart('.').Trim();
                        if (string.IsNullOrEmpty(_FileExtension))
                            _FileExtension = "txt";
                    }
                }
                catch (Exception ex)
                {
                    Feature.AddToCommandList($"Error determining file extension: {ex.Message}. Using default 'txt' extension.", rchCommandLine, false);
                    _FileExtension = "txt";
                }

                // Create new file
                File _NewFile;
                try
                {
                    if (_FileName.StartsWith("."))
                    {
                        // For hidden files - keep full name
                        string[] parts = _FileName.Split('.');
                        _NewFile = new File('.' + parts[1], fileType: _FileExtension, owner: UserManager.CurrentUser.Username);
                    }
                    else
                    {
                        // For regular files - only filename without extension
                        _NewFile = new File(Path.GetFileNameWithoutExtension(_FileName),
                                           fileType: _FileExtension,
                                           owner: UserManager.CurrentUser.Username);
                    }

                    // Add custom time if it exists
                    if (!string.IsNullOrEmpty(customtime))
                    {
                        _NewFile.Timestamp = customtime;
                    }

                    // Add text if it exists
                    if (!string.IsNullOrEmpty(content))
                    {
                        _NewFile.Content = content;
                    }
                }
                catch (Exception ex)
                {
                    Feature.AddToCommandList($"Error creating file object: {ex.Message}.", rchCommandLine, false);
                    return;
                }

                try
                {
                    _ParentDirectory.AddChild(_NewFile);
                    Save();
                    Feature.AddToCommandList($"File '{_FileName}' created successfully in '{_ParentDirectory.Name}'.", rchCommandLine, false);
                }
                catch (Exception ex)
                {
                    Feature.AddToCommandList($"Error saving file '{_FileName}': {ex.Message}.", rchCommandLine, false);
                    return;
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"An unexpected error occurred while creating file: {ex.Message}. Please try again.", rchCommandLine, false);
            }
        }

        // For making and show tree
        public string Tree(RichTextBox rchCommandLine, string path = null, Directory directory = null,
                  string indent = "", bool islast = true, int? maxdepth = null, int currentdepth = 0)
        {
            try
            {
                Directory _TargetDirectory = directory;

                if (path != null)
                {
                    var _DirectoryNode = ResolvePath(path, rchCommandLine);
                    if (_DirectoryNode is Directory directoryFromPath)
                    {
                        _TargetDirectory = directoryFromPath;
                    }
                    else if (_DirectoryNode == null)
                    {
                        // Error message already shown by ResolvePath
                        return string.Empty;
                    }
                    else
                    {
                        Feature.AddToCommandList($"'{path}' is not a directory. Please specify a valid directory path.", rchCommandLine, false);
                        return string.Empty;
                    }
                }
                else if (_TargetDirectory == null)
                {
                    _TargetDirectory = CurrentDirectory;
                }

                if (_TargetDirectory == null)
                {
                    Feature.AddToCommandList("Cannot display tree: No valid directory specified.", rchCommandLine, false);
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
                    Feature.AddToCommandList($"Tree display limited to depth {maxdepth.Value}.", rchCommandLine, false);
                    return _Tree;
                }

                try
                {
                    var _SortedChildren = _TargetDirectory.Children.OrderBy(c => c.Name).ToList();

                    if (_SortedChildren.Count == 0 && currentdepth == 0)
                    {
                        Feature.AddToCommandList($"Directory '{_TargetDirectory.Name}' is empty.", rchCommandLine, false);
                    }

                    for (int i = 0; i < _SortedChildren.Count; i++)
                    {
                        var _Child = _SortedChildren[i];
                        bool _IsLastChild = i == _SortedChildren.Count - 1;

                        string _ChildIndent = indent + (islast ? "    " : "│   ");

                        if (_Child is Directory childDirectory)
                        {
                            if (childDirectory != _TargetDirectory)
                            {
                                string childTree = Tree(rchCommandLine, null, childDirectory, _ChildIndent, _IsLastChild, maxdepth, currentdepth + 1);
                                if (!string.IsNullOrEmpty(childTree))
                                {
                                    _Tree += childTree;
                                }
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

                    // Add summary for top-level directory
                    if (currentdepth == 0 && !string.IsNullOrEmpty(_Tree))
                    {
                        int dirCount = _SortedChildren.Count(c => c is Directory);
                        int fileCount = _SortedChildren.Count(c => c is File);
                        Feature.AddToCommandList($"Directory tree generated: {dirCount} directories, {fileCount} files.", rchCommandLine, false);
                    }

                    return _Tree;
                }
                catch (Exception ex)
                {
                    Feature.AddToCommandList($"Error generating directory tree: {ex.Message}.", rchCommandLine, false);
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"An unexpected error occurred while generating directory tree: {ex.Message}.", rchCommandLine, false);
                return string.Empty;
            }
        }

        // For show all file and directory in the specified path
        public string Ls(RichTextBox rchCommandLine, string path = null, bool moreinfo = false, bool showhidden = false)
        {
            try
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
                    else if (_DirectoryNode == null)
                    {
                        // Error message already shown by ResolvePath
                        return null;
                    }
                    else
                    {
                        Feature.AddToCommandList($"'{path}' is not a directory. Please specify a valid directory path.", rchCommandLine, false);
                        return null;
                    }
                }

                if (_ParentDirectory == null)
                {
                    Feature.AddToCommandList("Cannot list contents: Directory not found or inaccessible.", rchCommandLine, false);
                    return null;
                }

                try
                {
                    string _FilesOrFolders = "";
                    var _SortedChildren = _ParentDirectory.Children.OrderBy(c => c.Name).ToList();

                    if (_SortedChildren.Count == 0)
                    {
                        Feature.AddToCommandList($"Directory '{_ParentDirectory.Name}' is empty.", rchCommandLine, false);
                        return "";
                    }

                    int visibleCount = 0;
                    int hiddenCount = 0;
                    int directoryCount = 0;
                    int fileCount = 0;

                    for (int i = 0; i < _SortedChildren.Count; i++)
                    {
                        var _Child = _SortedChildren[i];
                        bool isHidden = _Child.Name.StartsWith(".");

                        if (showhidden || !isHidden)
                        {
                            if (isHidden) hiddenCount++;
                            else visibleCount++;

                            if (_Child is Directory dir)
                            {
                                directoryCount++;
                                if (moreinfo)
                                    _FilesOrFolders += _Child.Timestamp + "    " + _Child.Permissions + "    " + _Child.Name + "\n";
                                else
                                    _FilesOrFolders += _Child.Name + "    ";
                            }
                            else
                            {
                                fileCount++;
                                File file = (File)_Child;
                                if (moreinfo)
                                    _FilesOrFolders += file.Timestamp + "    " + file.Permissions + "    " + file.Name + "." + file.FileType + "\n";
                                else
                                    _FilesOrFolders += file.Name + "." + file.FileType + "    ";
                            }
                        }
                        else
                        {
                            if (isHidden) hiddenCount++;
                        }
                    }

                    // Add summary information
                    string summary = $"Found {visibleCount} visible items";
                    if (showhidden && hiddenCount > 0)
                    {
                        summary += $" and {hiddenCount} hidden items";
                    }
                    summary += $" ({directoryCount} directories, {fileCount} files)";

                    if (hiddenCount > 0 && !showhidden)
                    {
                        summary += $". Use -a flag to show {hiddenCount} hidden items.";
                    }

                    Feature.AddToCommandList(summary, rchCommandLine, false);

                    // Add newline if not in moreinfo mode
                    if (!moreinfo && !string.IsNullOrEmpty(_FilesOrFolders))
                    {
                        _FilesOrFolders += "\n";
                    }

                    return _FilesOrFolders;
                }
                catch (Exception ex)
                {
                    Feature.AddToCommandList($"Error listing directory contents: {ex.Message}.", rchCommandLine, false);
                    return null;
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"An unexpected error occurred while listing directory: {ex.Message}.", rchCommandLine, false);
                return null;
            }
        }

        // For go to specified path
        public void Cd(RichTextBox rchCommandLine, string path = null)
        {
            try
            {
                if (path == "..")
                {
                    if (CurrentDirectory.Parent != null)
                    {
                        CurrentDirectory = (Directory)CurrentDirectory.Parent;
                        Feature.AddToCommandList($"Changed to parent directory: {CurrentDirectory.Name}", rchCommandLine, false);
                    }
                    else
                    {
                        Feature.AddToCommandList("Already at the root directory. Cannot go up further.", rchCommandLine, false);
                    }
                }
                else if (path == null || path == "/")
                {
                    CurrentDirectory = Root;
                    Feature.AddToCommandList($"Changed to root directory: {Root.Name}", rchCommandLine, false);
                }
                else
                {
                    Directory _ParentDirectory;

                    var _DirectoryNode = ResolvePath(path, rchCommandLine);
                    if (_DirectoryNode is Directory directory)
                    {
                        CurrentDirectory = directory;
                        Feature.AddToCommandList($"Changed directory to: {CurrentDirectory.Name}", rchCommandLine, false);
                    }
                    else if (_DirectoryNode == null)
                    {
                        // Error message already shown by ResolvePath
                        return;
                    }
                    else
                    {
                        Feature.AddToCommandList($"'{path}' is not a directory. Cannot change to this location.", rchCommandLine, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error changing directory: {ex.Message}. Please check the path and try again.", rchCommandLine, false);
                return;
            }
        }

        // For remove file or folder 
        public void Rm(RichTextBox rchCommandLine, string name, bool isrecursive, bool isforce)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    Feature.AddToCommandList("Please specify a file or directory name to remove.", rchCommandLine, false);
                    return;
                }

                var _DirectoryNode = CurrentDirectory.FindChild(name);
                if (_DirectoryNode != null)
                {
                    try
                    {
                        if (!UserManager.CurrentUser.HasPermission(_DirectoryNode, "w"))
                        {
                            Feature.AddToCommandList($"Permission denied: You do not have write permission for '{name}'.", rchCommandLine, false);
                            return;
                        }

                        if (_DirectoryNode is Directory directory)
                        {
                            if (directory.Parent == null)
                            {
                                Feature.AddToCommandList("Cannot delete root directory. This operation is not allowed.", rchCommandLine, false);
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
                                        Feature.AddToCommandList($"Directory '{name}' and all its contents removed successfully.", rchCommandLine, false);
                                    }
                                    else
                                    {
                                        // Ask for delete
                                        DialogResult result = MessageBox.Show($"Are you sure you want to remove directory '{name}' and all its contents?\n\nThis action cannot be undone.",
                                            "Confirm Delete Directory", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                                        if (result == DialogResult.Yes)
                                        {
                                            CurrentDirectory.RemoveChild(name);
                                            Feature.AddToCommandList($"Directory '{name}' and all its contents removed successfully.", rchCommandLine, false);
                                        }
                                        else
                                        {
                                            Feature.AddToCommandList($"Deletion of directory '{name}' was cancelled by user.", rchCommandLine, false);
                                        }
                                    }
                                }
                                else
                                {
                                    Feature.AddToCommandList($"Directory '{name}' contains files or folders. Use the -r (recursive) flag to remove non-empty directories.", rchCommandLine, false);
                                }
                            }
                            else
                            {
                                // for empty directory
                                if (isforce)
                                {
                                    CurrentDirectory.RemoveChild(name);
                                    Feature.AddToCommandList($"Empty directory '{name}' removed successfully.", rchCommandLine, false);
                                }
                                else
                                {
                                    // Ask for delete
                                    DialogResult _result = MessageBox.Show($"Are you sure you want to remove empty directory '{name}'?\n\nThis action cannot be undone.",
                                        "Confirm Delete Directory", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                                    if (_result == DialogResult.Yes)
                                    {
                                        CurrentDirectory.RemoveChild(name);
                                        Feature.AddToCommandList($"Empty directory '{name}' removed successfully.", rchCommandLine, false);
                                    }
                                    else
                                    {
                                        Feature.AddToCommandList($"Deletion of directory '{name}' was cancelled by user.", rchCommandLine, false);
                                    }
                                }
                            }
                            Save();
                            return;
                        }
                        else
                        {
                            // If it is file
                            string originalName = name;
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
                                Feature.AddToCommandList($"File '{originalName}' removed successfully.", rchCommandLine, false);
                            }
                            else
                            {
                                // Ask for delete
                                DialogResult _result = MessageBox.Show($"Are you sure you want to remove file '{originalName}'?\n\nThis action cannot be undone.",
                                    "Confirm Delete File", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

                                if (_result == DialogResult.Yes)
                                {
                                    CurrentDirectory.RemoveChild(name);
                                    Feature.AddToCommandList($"File '{originalName}' removed successfully.", rchCommandLine, false);
                                }
                                else
                                {
                                    Feature.AddToCommandList($"Deletion of file '{originalName}' was cancelled by user.", rchCommandLine, false);
                                }
                            }
                            Save();
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Feature.AddToCommandList($"Error removing '{name}': {ex.Message}.", rchCommandLine, false);
                        return;
                    }
                }
                else
                {
                    Feature.AddToCommandList($"'{name}' not found in current directory. Please check the name and try again.", rchCommandLine, false);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"An unexpected error occurred during removal: {ex.Message}. Please try again.", rchCommandLine, false);
            }
        }

        // For making link file or directory
        public void Ln(RichTextBox rchCommandLine, string[] inputs)
        {
            try
            {
                if (inputs == null || inputs.Length < 3)
                {
                    Feature.AddToCommandList("Invalid command syntax. Usage: ln [-s] <target> <link_name>", rchCommandLine, false);
                    return;
                }

                bool _IsSoft = inputs[1] == "-s";
                string _Path, _Name;

                if (_IsSoft)
                {
                    if (inputs.Length < 4)
                    {
                        Feature.AddToCommandList("Invalid syntax for soft link. Usage: ln -s <target> <link_name>", rchCommandLine, false);
                        return;
                    }

                    _Path = inputs[2];
                    _Name = inputs[3];

                    if (!_Path.StartsWith("..") && !_Path.StartsWith("/") && !_Path.StartsWith("."))
                    {
                        Feature.AddToCommandList($"Invalid path '{_Path}'. Path must start with '/', '.', or '..' for soft links.", rchCommandLine, false);
                        return;
                    }

                    try
                    {
                        var _DirectoryNode = ResolvePath(_Path, rchCommandLine);
                        if (_DirectoryNode == null) return;

                        if (!UserManager.CurrentUser.HasPermission(_DirectoryNode, "w"))
                        {
                            Feature.AddToCommandList($"Permission denied. You do not have write permission for the target '{_Path}'.", rchCommandLine, false);
                            return;
                        }

                        if (CurrentDirectory.IsExistChildName(_Name))
                        {
                            Feature.AddToCommandList($"Cannot create link: A file or directory named '{_Name}' already exists in this location.", rchCommandLine, false);
                            return;
                        }

                        CurrentDirectory.AddChild(new File(_Name, isLink: true, link: _Path, owner: UserManager.CurrentUser.Username));
                        Feature.AddToCommandList($"Soft link '{_Name}' created successfully pointing to '{_Path}'.", rchCommandLine, false);
                    }
                    catch (Exception ex)
                    {
                        Feature.AddToCommandList($"Error creating soft link: {ex.Message}.", rchCommandLine, false);
                        return;
                    }
                }
                else
                {
                    // Hard link
                    if (inputs.Length < 3)
                    {
                        Feature.AddToCommandList("Invalid syntax for hard link. Usage: ln <target> <link_name>", rchCommandLine, false);
                        return;
                    }

                    _Path = inputs[1];
                    _Name = inputs[2];
                    Node _DirectoryNode = null;

                    try
                    {
                        if (_Path.StartsWith("..") || _Path.StartsWith("/") || _Path.StartsWith("."))
                        {
                            // Find parent
                            _DirectoryNode = ResolvePath(_Path, rchCommandLine);
                            if (_DirectoryNode == null) return;

                            if (!UserManager.CurrentUser.HasPermission(_DirectoryNode, "w"))
                            {
                                Feature.AddToCommandList($"Permission denied. You do not have write permission for the target '{_Path}'.", rchCommandLine, false);
                                return;
                            }
                        }
                        else
                        {
                            // For find name
                            string searchName = _Path.Contains('.') ? _Path.Split('.')[0] : _Path;
                            _DirectoryNode = CurrentDirectory.FindChild(searchName);

                            if (_DirectoryNode == null)
                            {
                                Feature.AddToCommandList($"Target '{_Path}' not found in current directory.", rchCommandLine, false);
                                return;
                            }

                            if (!UserManager.CurrentUser.HasPermission(_DirectoryNode, "w"))
                            {
                                Feature.AddToCommandList($"Permission denied. You do not have write permission for the file '{_Path}'.", rchCommandLine, false);
                                return;
                            }
                        }

                        if (_DirectoryNode == null)
                        {
                            Feature.AddToCommandList($"Invalid path '{_Path}'. Target not found.", rchCommandLine, false);
                            return;
                        }

                        if (_DirectoryNode is Directory)
                        {
                            Feature.AddToCommandList($"Hard links cannot be created for directories. Use soft links for directories: ln -s {_Path} {_Name}", rchCommandLine, false);
                            return;
                        }

                        if (CurrentDirectory.IsExistChildName(_Name))
                        {
                            Feature.AddToCommandList($"Cannot create link: A file or directory named '{_Name}' already exists in this location.", rchCommandLine, false);
                            return;
                        }

                        if (_DirectoryNode is File targetFile)
                        {
                            var hardLink = new File(_Name, fileType: targetFile.FileType, content: targetFile.Content, isLink: targetFile.IsLink, link: targetFile.Link, owner: UserManager.CurrentUser.Username)
                            {
                                Permissions = targetFile.Permissions
                            };

                            CurrentDirectory.AddChild(hardLink);
                            Feature.AddToCommandList($"Hard link '{_Name}' created successfully for file '{_Path}'.", rchCommandLine, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        Feature.AddToCommandList($"Error creating hard link: {ex.Message}.", rchCommandLine, false);
                        return;
                    }
                }

                try
                {
                    Save();
                }
                catch (Exception ex)
                {
                    Feature.AddToCommandList($"Link created but error saving changes: {ex.Message}.", rchCommandLine, false);
                    return;
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"An unexpected error occurred while creating link: {ex.Message}. Please check command syntax and try again.", rchCommandLine, false);
            }
        }

        // For get information about file or directory
        public void Stat(RichTextBox rchCommandLine, string[] inputs)
        {
            try
            {
                if (inputs == null || inputs.Length < 2)
                {
                    Feature.AddToCommandList("Please specify a file or directory name. Usage: stat <path> [-l]", rchCommandLine, false);
                    return;
                }

                bool _MoreInfo = false;
                if (inputs.Length > 2)
                    _MoreInfo = inputs[2] == "-l";

                string _Path = inputs[1];

                if (string.IsNullOrEmpty(_Path))
                {
                    Feature.AddToCommandList("Path cannot be empty. Please specify a valid file or directory path.", rchCommandLine, false);
                    return;
                }

                var _DirectoryNode = ResolvePath(_Path, rchCommandLine);
                if (_DirectoryNode == null)
                {
                    Feature.AddToCommandList($"Path '{_Path}' not found. Please check the path and try again.", rchCommandLine, false);
                    return;
                }

                try
                {
                    if (!UserManager.CurrentUser.HasPermission(_DirectoryNode, "r"))
                    {
                        Feature.AddToCommandList($"Permission denied: You do not have read permission for '{_Path}'.", rchCommandLine, false);
                        return;
                    }

                    Feature.AddToCommandList($"Information for: {_Path}", rchCommandLine, false);
                    Feature.AddToCommandList("----------------------------------------", rchCommandLine, false);

                    if (_DirectoryNode is Directory _Directory)
                    {
                        Feature.AddToCommandList($"Name: {_Directory.Name}", rchCommandLine, false);
                        Feature.AddToCommandList($"Type: Directory", rchCommandLine, false);
                        Feature.AddToCommandList($"Items count: {_Directory.CountChild()} items", rchCommandLine, false);
                        Feature.AddToCommandList($"Created: {_Directory.Timestamp}", rchCommandLine, false);
                        Feature.AddToCommandList($"Permissions: {_Directory.Permissions}", rchCommandLine, false);

                        if (_MoreInfo)
                        {
                            Feature.AddToCommandList($"Owner: {_Directory.Owner}", rchCommandLine, false);
                            Feature.AddToCommandList($"Parent: {(_Directory.Parent != null ? _Directory.Parent.Name : "Root")}", rchCommandLine, false);
                        }

                        Feature.AddToCommandList("----------------------------------------", rchCommandLine, false);
                        Feature.AddToCommandList($"Directory information retrieved successfully.", rchCommandLine, false);
                    }
                    else if (_DirectoryNode is File _File)
                    {
                        string fileTypeInfo = _File.IsLink ? "Symbolic Link" : "File";

                        Feature.AddToCommandList($"Name: {_File.Name}", rchCommandLine, false);
                        Feature.AddToCommandList($"Type: {fileTypeInfo}", rchCommandLine, false);

                        if (_File.IsLink && !string.IsNullOrEmpty(_File.Link))
                        {
                            Feature.AddToCommandList($"Link target: {_File.Link}", rchCommandLine, false);
                        }

                        if (!string.IsNullOrEmpty(_File.FileType))
                        {
                            Feature.AddToCommandList($"File type: {_File.FileType}", rchCommandLine, false);
                        }

                        Feature.AddToCommandList($"Size: {_File.Size} bytes", rchCommandLine, false);
                        Feature.AddToCommandList($"Created: {_File.Timestamp}", rchCommandLine, false);
                        Feature.AddToCommandList($"Permissions: {_File.Permissions}", rchCommandLine, false);

                        if (_MoreInfo)
                        {
                            Feature.AddToCommandList($"Owner: {_File.Owner}", rchCommandLine, false);
                            Feature.AddToCommandList($"Parent directory: {(_File.Parent != null ? _File.Parent.Name : "Unknown")}", rchCommandLine, false);
                            if (_File.IsLink)
                            {
                                Feature.AddToCommandList($"Link type: {(_File.IsLink ? "Symbolic" : "Hard")}", rchCommandLine, false);
                            }
                        }

                        Feature.AddToCommandList("----------------------------------------", rchCommandLine, false);
                        Feature.AddToCommandList($"File information retrieved successfully.", rchCommandLine, false);
                    }
                }
                catch (Exception ex)
                {
                    Feature.AddToCommandList($"Error retrieving information for '{_Path}': {ex.Message}.", rchCommandLine, false);
                    return;
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"An unexpected error occurred while getting file information: {ex.Message}. Please try again.", rchCommandLine, false);
            }
        }

        // For show current route
        public void Pwd(Directory currentdirectory, RichTextBox rchCommandLine)
        {
            try
            {
                if (currentdirectory == null)
                {
                    Feature.AddToCommandList("Current directory is not available. Please check your session.", rchCommandLine, false);
                    return;
                }

                try
                {
                    string _FullPath = NodePathToString(currentdirectory);

                    if (string.IsNullOrEmpty(_FullPath))
                    {
                        Feature.AddToCommandList("Unable to determine current path. You may be at the root directory.", rchCommandLine, false);
                        return;
                    }

                    Feature.AddToCommandList($"Current directory: {_FullPath}", rchCommandLine, false);
                }
                catch (Exception ex)
                {
                    Feature.AddToCommandList($"Error determining current directory path: {ex.Message}. Please try again.", rchCommandLine, false);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"An unexpected error occurred while getting current directory: {ex.Message}.", rchCommandLine, false);
            }
        }

        // For save content in file
        public void Echo(string name, string content, Directory currentDirectory, RichTextBox rchCommandLine, string datetime = null)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    Feature.AddToCommandList("Please specify a file name for the echo command.", rchCommandLine, false);
                    return;
                }

                if (currentDirectory == null)
                {
                    Feature.AddToCommandList("Current directory is not available. Cannot save file.", rchCommandLine, false);
                    return;
                }

                try
                {
                    // Combine path and file name
                    string _FullPathAndFileName = NodePathToString(currentDirectory) + "/" + name;

                    if (string.IsNullOrEmpty(_FullPathAndFileName) || _FullPathAndFileName == "/")
                    {
                        Feature.AddToCommandList("Invalid path generated. Cannot save file.", rchCommandLine, false);
                        return;
                    }

                    Feature.AddToCommandList($"Saving content to file: {name}", rchCommandLine, false);
                    Touch(_FullPathAndFileName, rchCommandLine, datetime, content);
                }
                catch (Exception ex)
                {
                    Feature.AddToCommandList($"Error preparing to save file '{name}': {ex.Message}.", rchCommandLine, false);
                    return;
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"An unexpected error occurred while saving content to file: {ex.Message}. Please try again.", rchCommandLine, false);
            }
        }

        // Function to make string of node path
        public string NodePathToString(Directory currentDirectory)
        {
            try
            {
                if (currentDirectory == null)
                {
                    throw new ArgumentNullException(nameof(currentDirectory), "Directory cannot be null");
                }

                // Making path of currentDirectory to root
                var _PathStack = new Stack<string>();
                Directory temp = currentDirectory;

                while (temp != null && temp.Name != "/")
                {
                    if (string.IsNullOrEmpty(temp.Name))
                    {
                        throw new InvalidOperationException("Encountered directory with empty or null name");
                    }

                    _PathStack.Push(temp.Name);

                    if (temp.Parent == temp)
                    {
                        throw new InvalidOperationException("Circular reference detected in directory structure");
                    }

                    temp = (Directory)temp.Parent;
                }

                // Combine parts
                string result = "/" + string.Join("/", _PathStack);

                if (result == "/")
                {
                    return "/";
                }

                return result;
            }
            catch (ArgumentNullException ex)
            {
                throw new Exception($"Invalid directory: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                throw new Exception($"Directory structure error: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error converting directory path to string: {ex.Message}");
            }
        }
        // For show file's content
        public void Cat(string path, RichTextBox rchCommandLine)
        {
            try
            {
                // Check input path
                if (string.IsNullOrWhiteSpace(path))
                {
                    Feature.AddToCommandList("Error: Please specify a file path. Usage: cat <filename> or cat <path/to/file>", rchCommandLine, false);
                    return;
                }

                try
                {
                    string _DirectoryPath = NodePathToString(CurrentDirectory);
                    if (path.StartsWith(".") || path.StartsWith("/"))
                        _DirectoryPath = GetDirectoryPath(path);

                    string _FileName = GetFileName(path);

                    // Check file name
                    if (string.IsNullOrWhiteSpace(_FileName))
                    {
                        Feature.AddToCommandList("Error: Please provide a valid file name.", rchCommandLine, false);
                        return;
                    }

                    Directory _ParentDirectory;
                    if (_DirectoryPath == ".")
                    {
                        _ParentDirectory = CurrentDirectory;
                        Feature.AddToCommandList($"Looking for file '{_FileName}' in current directory...", rchCommandLine, false);
                    }
                    else
                    {
                        Feature.AddToCommandList($"Looking for file '{_FileName}' in directory '{_DirectoryPath}'...", rchCommandLine, false);
                        var dirNode = ResolvePath(_DirectoryPath, rchCommandLine);
                        if (dirNode is Directory directory)
                        {
                            _ParentDirectory = directory;
                        }
                        else if (dirNode == null)
                        {
                            Feature.AddToCommandList($"Error: Directory '{_DirectoryPath}' does not exist or cannot be accessed.", rchCommandLine, false);
                            return;
                        }
                        else
                        {
                            Feature.AddToCommandList($"Error: '{_DirectoryPath}' is not a directory. It might be a file.", rchCommandLine, false);
                            return;
                        }
                    }

                    // Check access permissions
                    if (!UserManager.CurrentUser.HasPermission(_ParentDirectory, "r"))
                    {
                        Feature.AddToCommandList($"Access Denied: You don't have read permission for directory '{_ParentDirectory.Name}'.", rchCommandLine, false);
                        Feature.AddToCommandList("Required permission: Read (r)", rchCommandLine, false);
                        return;
                    }

                    var foundNode = _ParentDirectory.FindChild(_FileName);
                    if (foundNode != null)
                    {
                        if (foundNode is File file)
                        {
                            // Check file access permissions
                            if (!UserManager.CurrentUser.HasPermission(file, "r"))
                            {
                                Feature.AddToCommandList($"Access Denied: You don't have read permission for file '{file.Name}'.", rchCommandLine, false);
                                return;
                            }

                            if (file.IsLink == false)
                            {
                                Feature.AddToCommandList($"=== Content of '{file.Name}' ===", rchCommandLine, false);

                                if (string.IsNullOrEmpty(file.Content))
                                {
                                    Feature.AddToCommandList("[File is empty]", rchCommandLine, false);
                                }
                                else
                                {
                                    Feature.AddToCommandList($"{file.Content}", rchCommandLine, false);
                                }

                                Feature.AddToCommandList($"=== End of file ({file.Content?.Length ?? 0} characters) ===", rchCommandLine, false);
                                Feature.AddToCommandList($"File content displayed successfully.", rchCommandLine, false);
                            }
                            else
                            {
                                Feature.AddToCommandList($"Note: '{file.Name}' is a symbolic link.", rchCommandLine, false);
                                Feature.AddToCommandList($"Link target: {file.Link}", rchCommandLine, false);

                                var linkedNode = ResolvePath(file.Link, rchCommandLine);
                                if (linkedNode is Directory linkedDir)
                                {
                                    Feature.AddToCommandList($"Following link to directory: {file.Link}", rchCommandLine, false);
                                    CurrentDirectory = linkedDir;
                                    Feature.AddToCommandList($"Current directory changed to: {NodePathToString(CurrentDirectory)}", rchCommandLine, false);
                                }
                                else if (linkedNode is File linkedFile)
                                {
                                    Feature.AddToCommandList($"Following link to file: {linkedFile.Name}", rchCommandLine, false);

                                    // Check target file access permissions
                                    if (!UserManager.CurrentUser.HasPermission(linkedFile, "r"))
                                    {
                                        Feature.AddToCommandList($"Access Denied: You don't have read permission for target file '{linkedFile.Name}'.", rchCommandLine, false);
                                        return;
                                    }

                                    Feature.AddToCommandList($"=== Content of linked file '{linkedFile.Name}' ===", rchCommandLine, false);

                                    if (string.IsNullOrEmpty(linkedFile.Content))
                                    {
                                        Feature.AddToCommandList("[File is empty]", rchCommandLine, false);
                                    }
                                    else
                                    {
                                        Feature.AddToCommandList($"{linkedFile.Content}", rchCommandLine, false);
                                    }

                                    Feature.AddToCommandList($"=== End of linked file ({linkedFile.Content?.Length ?? 0} characters) ===", rchCommandLine, false);
                                    Feature.AddToCommandList($"Linked file content displayed successfully.", rchCommandLine, false);
                                }
                                else
                                {
                                    Feature.AddToCommandList($"Warning: Broken link - target '{file.Link}' not found or cannot be accessed.", rchCommandLine, false);
                                }
                            }
                        }
                        else if (foundNode is Directory)
                        {
                            Feature.AddToCommandList($"Error: '{_FileName}' is a directory, not a file. Use 'ls' to list directory contents.", rchCommandLine, false);
                        }
                    }
                    else
                    {
                        Feature.AddToCommandList($"Error: File '{_FileName}' not found in directory '{_ParentDirectory.Name}'.", rchCommandLine, false);
                        Feature.AddToCommandList($"Available files in this directory: {string.Join(", ", _ParentDirectory.Children.Where(c => c is File).Select(c => c.Name))}", rchCommandLine, false);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Feature.AddToCommandList($"Error processing file path '{path}': {ex.Message}. Please check the path and try again.", rchCommandLine, false);
                    return;
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"An unexpected error occurred while reading file: {ex.Message}. Please try again.", rchCommandLine, false);
            }
        }

        // For moving file or directory (CUT)
        public void Mv(string[] inputs, RichTextBox rchCommandLine)
        {
            try
            {
                if (inputs == null || inputs.Length < 3)
                {
                    Feature.AddToCommandList("Invalid command syntax. Usage: mv <source> <destination>", rchCommandLine, false);
                    return;
                }

                if (string.IsNullOrEmpty(inputs[1]) || string.IsNullOrEmpty(inputs[2]))
                {
                    Feature.AddToCommandList("Both source and destination must be specified. Usage: mv <source> <destination>", rchCommandLine, false);
                    return;
                }

                try
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
                        Feature.AddToCommandList($"Processing source path: {_FirstDirectoryPath}", rchCommandLine, false);

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
                            else if (_DirectoryNode == null)
                            {
                                // Error message already shown by ResolvePath
                                return;
                            }
                            else
                            {
                                Feature.AddToCommandList($"Source '{_FirstDirectoryPath}' is not a directory. Please specify a valid directory.", rchCommandLine, false);
                                return;
                            }
                        }

                        if (!UserManager.CurrentUser.HasPermission(_FirstDirectory, "w"))
                        {
                            Feature.AddToCommandList($"Permission denied: You do not have write permission for source directory '{_FirstDirectory.Name}'.", rchCommandLine, false);
                            return;
                        }
                    }

                    Directory _SecondDirectory;
                    Feature.AddToCommandList($"Processing destination: {_SecondDirectoryPath}", rchCommandLine, false);

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
                        else if (_DirectoryNode == null)
                        {
                            // Error message already shown by ResolvePath
                            return;
                        }
                        else
                        {
                            Feature.AddToCommandList($"Destination '{_SecondDirectoryPath}' is not a directory. Please specify a valid directory.", rchCommandLine, false);
                            return;
                        }

                        if (!UserManager.CurrentUser.HasPermission(_SecondDirectory, "w"))
                        {
                            Feature.AddToCommandList($"Permission denied: You do not have write permission for destination directory '{_SecondDirectory.Name}'.", rchCommandLine, false);
                            return;
                        }
                    }

                    // Validate source existence
                    if (_FirstFileName != null)
                    {
                        string sourceName = inputs[1].Trim().Split('/').ToArray().Last();
                        if (!CurrentDirectory.IsExistChildName(sourceName))
                        {
                            Feature.AddToCommandList($"Source file '{sourceName}' not found in current directory.", rchCommandLine, false);
                            return;
                        }
                    }

                    // Check for conflicts in destination
                    if (_FirstFileName != null && _SecondFileName == null)
                    {
                        if (_SecondDirectory.IsExistChildName(_FirstFileName))
                        {
                            Feature.AddToCommandList($"Cannot move: File '{_FirstFileName}' already exists in the destination directory.", rchCommandLine, false);
                            return;
                        }
                    }

                    if (_FirstDirectoryPath != null)
                    {
                        string sourceDirName = inputs[1].Trim().Split('/').ToArray().Last();
                        if (!CurrentDirectory.IsExistChildName(sourceDirName))
                        {
                            Feature.AddToCommandList($"Source directory '{sourceDirName}' not found in current directory.", rchCommandLine, false);
                            return;
                        }

                        if (_SecondDirectory.IsExistChildName(sourceDirName))
                        {
                            Feature.AddToCommandList($"Cannot move: Directory '{sourceDirName}' already exists in the destination directory.", rchCommandLine, false);
                            return;
                        }
                    }

                    // Perform the move/rename operation
                    if (_SecondFileName == null && _FirstDirectoryPath == null)
                    {
                        // Moving a file
                        File file = (File)CurrentDirectory.FindChild(_FirstFileName);
                        if (file == null)
                        {
                            Feature.AddToCommandList($"File '{_FirstFileName}' not found.", rchCommandLine, false);
                            return;
                        }

                        if (!UserManager.CurrentUser.HasPermission(file, "w"))
                        {
                            Feature.AddToCommandList($"Permission denied: You do not have write permission for file '{_FirstFileName}'.", rchCommandLine, false);
                            return;
                        }

                        _SecondDirectory.AddChild(file);
                        file.Parent = _SecondDirectory;
                        CurrentDirectory.RemoveChild(_FirstFileName);
                        Feature.AddToCommandList($"File '{_FirstFileName}' moved successfully to '{_SecondDirectory.Name}'.", rchCommandLine, false);
                    }
                    else if (_FirstFileName == null && _SecondFileName == null)
                    {
                        // Moving a directory
                        _SecondDirectory.AddChild(_FirstDirectory);
                        _FirstDirectory.Parent = _SecondDirectory;
                        CurrentDirectory.RemoveChild(inputs[1].Trim().Split('/').ToArray().Last());
                        Feature.AddToCommandList($"Directory '{_FirstDirectory.Name}' moved successfully to '{_SecondDirectory.Name}'.", rchCommandLine, false);
                    }
                    else if (_FirstDirectoryPath == null)
                    {
                        // Renaming a file
                        File file = (File)CurrentDirectory.FindChild(_FirstFileName);
                        if (file == null)
                        {
                            Feature.AddToCommandList($"File '{_FirstFileName}' not found.", rchCommandLine, false);
                            return;
                        }

                        if (!UserManager.CurrentUser.HasPermission(file, "w"))
                        {
                            Feature.AddToCommandList($"Permission denied: You do not have write permission to rename file '{_FirstFileName}'.", rchCommandLine, false);
                            return;
                        }

                        string originalName = file.Name;
                        file.Name = _SecondFileName;
                        Feature.AddToCommandList($"File renamed successfully from '{originalName}' to '{_SecondFileName}'.", rchCommandLine, false);
                    }

                    try
                    {
                        Save();
                        Feature.AddToCommandList("Changes saved successfully.", rchCommandLine, false);
                    }
                    catch (Exception ex)
                    {
                        Feature.AddToCommandList($"Operation completed but error saving changes: {ex.Message}.", rchCommandLine, false);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Feature.AddToCommandList($"Error processing move/rename operation: {ex.Message}.", rchCommandLine, false);
                    return;
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"An unexpected error occurred while moving/renaming: {ex.Message}. Please try again.", rchCommandLine, false);
            }
        }

        // For copy and paste file or directory to specified path
        public void Cp(string[] inputs, RichTextBox rchCommandLine)
        {
            try
            {
                if (inputs == null || inputs.Length < 3)
                {
                    Feature.AddToCommandList("Invalid command syntax. Usage: cp <source> <destination>", rchCommandLine, false);
                    return;
                }

                if (string.IsNullOrEmpty(inputs[1]) || string.IsNullOrEmpty(inputs[2]))
                {
                    Feature.AddToCommandList("Both source and destination must be specified. Usage: cp <source> <destination>", rchCommandLine, false);
                    return;
                }

                try
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
                        Feature.AddToCommandList($"Processing source: {_FirstDirectoryPath}", rchCommandLine, false);

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
                            else if (_DirectoryNode == null)
                            {
                                // Error message already shown by ResolvePath
                                return;
                            }
                            else
                            {
                                Feature.AddToCommandList($"Source '{_FirstDirectoryPath}' is not a directory. Please specify a valid directory.", rchCommandLine, false);
                                return;
                            }
                        }

                        if (!UserManager.CurrentUser.HasPermission(_FirstDirectory, "w"))
                        {
                            Feature.AddToCommandList($"Permission denied: You do not have write permission for source directory '{_FirstDirectory.Name}'.", rchCommandLine, false);
                            return;
                        }
                    }

                    Directory _SecondDirectory;
                    Feature.AddToCommandList($"Processing destination: {_SecondDirectoryPath}", rchCommandLine, false);

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
                        else if (_DirectoryNode == null)
                        {
                            // Error message already shown by ResolvePath
                            return;
                        }
                        else
                        {
                            Feature.AddToCommandList($"Destination '{_SecondDirectoryPath}' is not a directory. Please specify a valid directory.", rchCommandLine, false);
                            return;
                        }
                    }

                    if (!UserManager.CurrentUser.HasPermission(_SecondDirectory, "w"))
                    {
                        Feature.AddToCommandList($"Permission denied: You do not have write permission for destination directory '{_SecondDirectory.Name}'.", rchCommandLine, false);
                        return;
                    }

                    // Validate source existence
                    if (_FirstFileName != null)
                    {
                        string sourceName = inputs[1].Trim().Split('/').ToArray().Last();
                        if (!CurrentDirectory.IsExistChildName(sourceName))
                        {
                            Feature.AddToCommandList($"Source file '{sourceName}' not found in current directory.", rchCommandLine, false);
                            return;
                        }

                        if (_SecondDirectory.IsExistChildName(_FirstFileName))
                        {
                            Feature.AddToCommandList($"Cannot copy: File '{_FirstFileName}' already exists in the destination directory.", rchCommandLine, false);
                            return;
                        }
                    }

                    if (_FirstDirectoryPath != null)
                    {
                        string sourceDirName = inputs[1].Trim().Split('/').ToArray().Last();
                        if (!CurrentDirectory.IsExistChildName(sourceDirName))
                        {
                            Feature.AddToCommandList($"Source directory '{sourceDirName}' not found in current directory.", rchCommandLine, false);
                            return;
                        }

                        if (_SecondDirectory.IsExistChildName(sourceDirName))
                        {
                            Feature.AddToCommandList($"Cannot copy: Directory '{sourceDirName}' already exists in the destination directory.", rchCommandLine, false);
                            return;
                        }
                    }

                    // Perform the copy operation
                    if (_FirstDirectoryPath == null)
                    {
                        // Copying a file
                        File file = (File)CurrentDirectory.FindChild(_FirstFileName);
                        if (file == null)
                        {
                            Feature.AddToCommandList($"File '{_FirstFileName}' not found.", rchCommandLine, false);
                            return;
                        }

                        if (!UserManager.CurrentUser.HasPermission(file, "w"))
                        {
                            Feature.AddToCommandList($"Permission denied: You do not have write permission to copy file '{_FirstFileName}'.", rchCommandLine, false);
                            return;
                        }

                        // Create a copy of the file
                        File copiedFile = new File(file.Name,
                            fileType: file.FileType,
                            content: file.Content,
                            isLink: file.IsLink,
                            link: file.Link,
                            owner: UserManager.CurrentUser.Username)
                        {
                            Permissions = file.Permissions,
                            Timestamp = file.Timestamp
                        };

                        _SecondDirectory.AddChild(copiedFile);
                        Feature.AddToCommandList($"File '{_FirstFileName}' copied successfully to '{_SecondDirectory.Name}'.", rchCommandLine, false);
                    }
                    else
                    {
                        // Copying a directory
                        // Note: This creates a reference, not a deep copy. You might want to implement deep copy.
                        Directory temp = _FirstDirectory;
                        _SecondDirectory.AddChild(temp);
                        Feature.AddToCommandList($"Directory '{_FirstDirectory.Name}' copied successfully to '{_SecondDirectory.Name}'.", rchCommandLine, false);
                        Feature.AddToCommandList("Note: Directory copy creates a reference. Use 'cp -r' for recursive copy if available.", rchCommandLine, false);
                    }

                    try
                    {
                        Save();
                        Feature.AddToCommandList("Copy operation completed and changes saved.", rchCommandLine, false);
                    }
                    catch (Exception ex)
                    {
                        Feature.AddToCommandList($"Copy completed but error saving changes: {ex.Message}.", rchCommandLine, false);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Feature.AddToCommandList($"Error processing copy operation: {ex.Message}.", rchCommandLine, false);
                    return;
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"An unexpected error occurred while copying: {ex.Message}. Please try again.", rchCommandLine, false);
            }
        }
        public void Chmod(string permission, string file_directory, RichTextBox rchCommandLine)
        {
            try
            {
                if (string.IsNullOrEmpty(permission))
                {
                    Feature.AddToCommandList("Error: Permission mode is required. Usage: chmod <mode> <file/directory>", rchCommandLine, false);
                    return;
                }

                if (string.IsNullOrEmpty(file_directory))
                {
                    Feature.AddToCommandList("Error: File or directory name is required. Usage: chmod <mode> <file/directory>", rchCommandLine, false);
                    return;
                }

                try
                {
                    Node _FileOrDirectory = CurrentDirectory.FindChild(file_directory);

                    if (_FileOrDirectory == null)
                    {
                        Feature.AddToCommandList($"Error: '{file_directory}' not found in current directory. Use 'ls' to see available items.", rchCommandLine, false);
                        return;
                    }

                    if (!UserManager.CurrentUser.HasPermission(_FileOrDirectory, "w"))
                    {
                        Feature.AddToCommandList($"Permission denied: You do not have write permission to change permissions for '{file_directory}'.", rchCommandLine, false);
                        return;
                    }

                    Feature.AddToCommandList($"Changing permissions for: {file_directory}", rchCommandLine, false);
                    Feature.AddToCommandList($"Current permissions: {_FileOrDirectory.Permissions}", rchCommandLine, false);

                    char[] _PermissionChars = _FileOrDirectory.Permissions.ToCharArray();

                    if (int.TryParse(permission, out int _PermissionCode))
                    {
                        Feature.AddToCommandList($"Processing numeric permission code: {_PermissionCode:D3}", rchCommandLine, false);

                        if (_PermissionCode < 0 || _PermissionCode > 777)
                        {
                            Feature.AddToCommandList($"Error: Permission code '{_PermissionCode}' is invalid. Must be between 000 and 777.", rchCommandLine, false);
                            return;
                        }

                        // Apply permissions manually digit by digit
                        int tempCode = _PermissionCode;
                        int position = 0; // 0: others, 1: group, 2: owner
                        bool hasError = false;

                        while (tempCode > 0 && !hasError)
                        {
                            int digit = tempCode % 10;

                            // Check if digit is valid (0-7)
                            if (digit < 0 || digit > 7)
                            {
                                Feature.AddToCommandList($"Error: Invalid permission digit '{digit}'. Each digit must be between 0 and 7.", rchCommandLine, false);
                                hasError = true;
                                break;
                            }

                            // Convert digit to read/write/execute permissions
                            // Owner (position 2), Group (position 1), Others (position 0)
                            switch (position)
                            {
                                case 2: // Owner
                                    _PermissionChars[0] = (digit & 4) != 0 ? 'r' : '-';
                                    _PermissionChars[1] = (digit & 2) != 0 ? 'w' : '-';
                                    _PermissionChars[2] = (digit & 1) != 0 ? 'x' : '-';
                                    Feature.AddToCommandList($"  Owner permissions: {_PermissionChars[0]}{_PermissionChars[1]}{_PermissionChars[2]} (digit: {digit})", rchCommandLine, false);
                                    break;
                                case 1: // Group
                                    _PermissionChars[3] = (digit & 4) != 0 ? 'r' : '-';
                                    _PermissionChars[4] = (digit & 2) != 0 ? 'w' : '-';
                                    _PermissionChars[5] = (digit & 1) != 0 ? 'x' : '-';
                                    Feature.AddToCommandList($"  Group permissions: {_PermissionChars[3]}{_PermissionChars[4]}{_PermissionChars[5]} (digit: {digit})", rchCommandLine, false);
                                    break;
                                case 0: // Others
                                    _PermissionChars[6] = (digit & 4) != 0 ? 'r' : '-';
                                    _PermissionChars[7] = (digit & 2) != 0 ? 'w' : '-';
                                    _PermissionChars[8] = (digit & 1) != 0 ? 'x' : '-';
                                    Feature.AddToCommandList($"  Others permissions: {_PermissionChars[6]}{_PermissionChars[7]}{_PermissionChars[8]} (digit: {digit})", rchCommandLine, false);
                                    break;
                            }

                            tempCode /= 10;
                            position++;

                            // Prevent more than 3 digits
                            if (position > 3)
                            {
                                Feature.AddToCommandList("Warning: Permission code has more than 3 digits. Using first 3 digits only.", rchCommandLine, false);
                                break;
                            }
                        }

                        if (!hasError)
                        {
                            // Handle cases where permission code has less than 3 digits
                            while (position < 3)
                            {
                                switch (position)
                                {
                                    case 0: // Others - default to no permissions
                                        _PermissionChars[6] = '-';
                                        _PermissionChars[7] = '-';
                                        _PermissionChars[8] = '-';
                                        Feature.AddToCommandList($"  Others permissions: --- (default, no digit provided)", rchCommandLine, false);
                                        break;
                                    case 1: // Group - default to no permissions
                                        _PermissionChars[3] = '-';
                                        _PermissionChars[4] = '-';
                                        _PermissionChars[5] = '-';
                                        Feature.AddToCommandList($"  Group permissions: --- (default, no digit provided)", rchCommandLine, false);
                                        break;
                                }
                                position++;
                            }

                            _FileOrDirectory.Permissions = new string(_PermissionChars);
                            Feature.AddToCommandList($"Permissions changed successfully: {_FileOrDirectory.Permissions}", rchCommandLine, false);
                        }
                    }
                    // If permission is in symbolic format (e.g., u+rwx,g+rx,o+r)
                    else if (IsSymbolicPermission(permission))
                    {
                        Feature.AddToCommandList($"Processing symbolic permission: {permission}", rchCommandLine, false);

                        try
                        {
                            ApplySymbolicPermission(_FileOrDirectory, permission, rchCommandLine, ref _PermissionChars);
                            _FileOrDirectory.Permissions = new string(_PermissionChars);
                            Feature.AddToCommandList($"Permissions changed successfully using symbolic mode: {_FileOrDirectory.Permissions}", rchCommandLine, false);
                        }
                        catch (Exception ex)
                        {
                            Feature.AddToCommandList($"Error applying symbolic permissions: {ex.Message}", rchCommandLine, false);
                            return;
                        }
                    }
                    else
                    {
                        Feature.AddToCommandList($"Error: Invalid permission format '{permission}'.", rchCommandLine, false);
                        Feature.AddToCommandList("Valid formats:", rchCommandLine, false);
                        Feature.AddToCommandList("  Numeric: 755, 644, 777 (owner/group/others)", rchCommandLine, false);
                        Feature.AddToCommandList("  Symbolic: u+rwx,g+rx,o+r (user/group/others with +, -, or =)", rchCommandLine, false);
                        return;
                    }

                    try
                    {
                        Save();
                        Feature.AddToCommandList("Permission changes saved successfully.", rchCommandLine, false);
                    }
                    catch (Exception ex)
                    {
                        Feature.AddToCommandList($"Permission changed but error saving: {ex.Message}", rchCommandLine, false);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Feature.AddToCommandList($"Error processing chmod command: {ex.Message}", rchCommandLine, false);
                    return;
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"An unexpected error occurred while changing permissions: {ex.Message}", rchCommandLine, false);
            }
        }

        // Helper method for detecting symbolic format
        private bool IsSymbolicPermission(string permission)
        {
            try
            {
                // Simple pattern for symbolic format (e.g., u+rwx,g+rx,o+r)
                var pattern = @"^([uogoa]*[+=-][rwxXst]*,?)+$";
                return Regex.IsMatch(permission, pattern, RegexOptions.IgnoreCase);
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Helper method for applying symbolic permissions
        private void ApplySymbolicPermission(Node node, string symbolicPermission, RichTextBox rchCommandLine, ref char[] permissionchars)
        {
            try
            {
                string[] parts = symbolicPermission.Split(',');

                foreach (var part in parts)
                {
                    if (part.Contains("+"))
                    {
                        string[] a = part.Split('+');
                        if (a[0].Contains("u") || a[0].Contains("owner"))
                        {
                            char[] b = a[1].ToCharArray();
                            if (b.Contains('r')) permissionchars[0] = 'r';
                            if (b.Contains('w')) permissionchars[1] = 'w';
                            if (b.Contains('x')) permissionchars[2] = 'x';
                            Feature.AddToCommandList($"  Added permissions for owner: {a[1]}", rchCommandLine, false);
                        }
                        if (a[0].Contains("g") || a[0].Contains("group"))
                        {
                            char[] b = a[1].ToCharArray();
                            if (b.Contains('r')) permissionchars[3] = 'r';
                            if (b.Contains('w')) permissionchars[4] = 'w';
                            if (b.Contains('x')) permissionchars[5] = 'x';
                            Feature.AddToCommandList($"  Added permissions for group: {a[1]}", rchCommandLine, false);
                        }
                        if (a[0].Contains("o") || a[0].Contains("other"))
                        {
                            char[] b = a[1].ToCharArray();
                            if (b.Contains('r')) permissionchars[6] = 'r';
                            if (b.Contains('w')) permissionchars[7] = 'w';
                            if (b.Contains('x')) permissionchars[8] = 'x';
                            Feature.AddToCommandList($"  Added permissions for others: {a[1]}", rchCommandLine, false);
                        }
                    }
                    else if (part.Contains("-"))
                    {
                        string[] a = part.Split('-');
                        if (a[0].Contains("u") || a[0].Contains("owner"))
                        {
                            char[] b = a[1].ToCharArray();
                            if (b.Contains('r')) permissionchars[0] = '-';
                            if (b.Contains('w')) permissionchars[1] = '-';
                            if (b.Contains('x')) permissionchars[2] = '-';
                            Feature.AddToCommandList($"  Removed permissions for owner: {a[1]}", rchCommandLine, false);
                        }
                        if (a[0].Contains("g") || a[0].Contains("group"))
                        {
                            char[] b = a[1].ToCharArray();
                            if (b.Contains('r')) permissionchars[3] = '-';
                            if (b.Contains('w')) permissionchars[4] = '-';
                            if (b.Contains('x')) permissionchars[5] = '-';
                            Feature.AddToCommandList($"  Removed permissions for group: {a[1]}", rchCommandLine, false);
                        }
                        if (a[0].Contains("o") || a[0].Contains("other"))
                        {
                            char[] b = a[1].ToCharArray();
                            if (b.Contains('r')) permissionchars[6] = '-';
                            if (b.Contains('w')) permissionchars[7] = '-';
                            if (b.Contains('x')) permissionchars[8] = '-';
                            Feature.AddToCommandList($"  Removed permissions for others: {a[1]}", rchCommandLine, false);
                        }
                    }
                    else if (part.Contains("="))
                    {
                        string[] a = part.Split('=');
                        if (a[0].Contains("u") || a[0].Contains("owner"))
                        {
                            char[] b = a[1].ToCharArray();
                            permissionchars[0] = b.Contains('r') ? 'r' : '-';
                            permissionchars[1] = b.Contains('w') ? 'w' : '-';
                            permissionchars[2] = b.Contains('x') ? 'x' : '-';
                            Feature.AddToCommandList($"  Set exact permissions for owner: {a[1]}", rchCommandLine, false);
                        }
                        if (a[0].Contains("g") || a[0].Contains("group"))
                        {
                            char[] b = a[1].ToCharArray();
                            permissionchars[3] = b.Contains('r') ? 'r' : '-';
                            permissionchars[4] = b.Contains('w') ? 'w' : '-';
                            permissionchars[5] = b.Contains('x') ? 'x' : '-';
                            Feature.AddToCommandList($"  Set exact permissions for group: {a[1]}", rchCommandLine, false);
                        }
                        if (a[0].Contains("o") || a[0].Contains("other"))
                        {
                            char[] b = a[1].ToCharArray();
                            permissionchars[6] = b.Contains('r') ? 'r' : '-';
                            permissionchars[7] = b.Contains('w') ? 'w' : '-';
                            permissionchars[8] = b.Contains('x') ? 'x' : '-';
                            Feature.AddToCommandList($"  Set exact permissions for others: {a[1]}", rchCommandLine, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error applying symbolic permission '{symbolicPermission}': {ex.Message}");
            }
        }
        public void Find(string path, string option, string pattern, RichTextBox rchCommandLine)
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    Feature.AddToCommandList("Error: Search path is required. Usage: find <path> <option> <pattern>", rchCommandLine, false);
                    return;
                }

                if (string.IsNullOrEmpty(option))
                {
                    Feature.AddToCommandList("Error: Search option is required. Usage: find <path> <option> <pattern>", rchCommandLine, false);
                    Feature.AddToCommandList("Available options: -name (search by name), -type (search by type)", rchCommandLine, false);
                    return;
                }

                if (string.IsNullOrEmpty(pattern))
                {
                    Feature.AddToCommandList("Error: Search pattern is required. Usage: find <path> <option> <pattern>", rchCommandLine, false);
                    Feature.AddToCommandList("Examples: find . -name '*.txt', find / -type f", rchCommandLine, false);
                    return;
                }

                Feature.AddToCommandList($"Starting search in '{path}' with option '{option}' for pattern '{pattern}'...", rchCommandLine, false);

                Directory _SearchDirectory;

                if (path == ".")
                {
                    _SearchDirectory = CurrentDirectory;
                    Feature.AddToCommandList($"Searching in current directory: {NodePathToString(_SearchDirectory)}", rchCommandLine, false);
                }
                else if (path == "~")
                {
                    _SearchDirectory = Root;
                    Feature.AddToCommandList("Searching in root directory", rchCommandLine, false);
                }
                else
                {
                    var dirNode = ResolvePath(path, rchCommandLine);
                    if (dirNode is Directory directory)
                    {
                        _SearchDirectory = directory;
                        Feature.AddToCommandList($"Searching in directory: {NodePathToString(_SearchDirectory)}", rchCommandLine, false);
                    }
                    else if (dirNode == null)
                    {
                        // Error message already shown by ResolvePath
                        return;
                    }
                    else
                    {
                        Feature.AddToCommandList($"Error: '{path}' is not a directory. Please specify a valid directory path.", rchCommandLine, false);
                        return;
                    }
                }

                // Separate option (e.g., -name)
                if (option != "-name" && option != "-type")
                {
                    Feature.AddToCommandList($"Error: Unsupported option '{option}'.", rchCommandLine, false);
                    Feature.AddToCommandList("Supported options:", rchCommandLine, false);
                    Feature.AddToCommandList("  -name <pattern> : Search files/directories by name pattern", rchCommandLine, false);
                    Feature.AddToCommandList("  -type <f|d>     : Search by type (f=files, d=directories)", rchCommandLine, false);
                    return;
                }

                // If option is for file name
                if (option == "-name")
                {
                    Feature.AddToCommandList($"Searching for items matching name pattern: '{pattern}'", rchCommandLine, false);
                    SearchByName(_SearchDirectory, pattern, rchCommandLine);
                }
                else if (option == "-type")
                {
                    // For search by type (f for file, d for directory)
                    if (pattern != "f" && pattern != "d")
                    {
                        Feature.AddToCommandList($"Error: Invalid type '{pattern}'. Use 'f' for files or 'd' for directories.", rchCommandLine, false);
                        return;
                    }

                    string typeDescription = pattern == "f" ? "files" : "directories";
                    Feature.AddToCommandList($"Searching for {typeDescription}...", rchCommandLine, false);
                    SearchByType(_SearchDirectory, pattern, rchCommandLine);
                }

                Feature.AddToCommandList("Search completed.", rchCommandLine, false);
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"An unexpected error occurred during search: {ex.Message}. Please try again.", rchCommandLine, false);
            }
        }

        // Recursive method for search by name
        private void SearchByName(Directory currentDir, string pattern, RichTextBox rchCommandLine)
        {
            try
            {
                if (currentDir == null)
                {
                    Feature.AddToCommandList("Error: Cannot search in null directory.", rchCommandLine, false);
                    return;
                }

                if (!UserManager.CurrentUser.HasPermission(currentDir, "r"))
                {
                    Feature.AddToCommandList($"Warning: Skipping directory '{currentDir.Name}' - No read permission.", rchCommandLine, false);
                    return;
                }

                int foundCount = 0;

                foreach (var item in currentDir.Children)
                {
                    try
                    {
                        if (!UserManager.CurrentUser.HasPermission(item, "r"))
                        {
                            continue; // Skip items without read permission
                        }

                        // Check wildcard pattern match
                        if (item is File file)
                        {
                            string fullName = file.Name + "." + file.FileType;

                            if (IsPatternMatch(fullName, pattern, rchCommandLine))
                            {
                                Feature.AddToCommandList(
                                    NodePathToString((Directory)item.Parent) + '/' + fullName,
                                    rchCommandLine,
                                    false
                                );
                                foundCount++;
                            }
                        }
                        else if (item is Directory directory)
                        {
                            // Check directory name match
                            if (IsPatternMatch(directory.Name, pattern, rchCommandLine))
                            {
                                Feature.AddToCommandList(
                                    NodePathToString(directory),
                                    rchCommandLine,
                                    false
                                );
                                foundCount++;
                            }

                            // Recursive search in subdirectories
                            SearchByName(directory, pattern, rchCommandLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        Feature.AddToCommandList($"Warning: Error processing item '{item?.Name}': {ex.Message}. Skipping...", rchCommandLine, false);
                        continue;
                    }
                }

                if (foundCount == 0 && currentDir.Children.Count > 0)
                {
                    Feature.AddToCommandList($"No matches found in '{currentDir.Name}' for pattern '{pattern}'.", rchCommandLine, false);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error searching directory '{currentDir?.Name}': {ex.Message}.", rchCommandLine, false);
            }
        }

        // Method for search by type
        private void SearchByType(Directory currentDir, string type, RichTextBox rchCommandLine)
        {
            try
            {
                if (currentDir == null)
                {
                    Feature.AddToCommandList("Error: Cannot search in null directory.", rchCommandLine, false);
                    return;
                }

                if (!UserManager.CurrentUser.HasPermission(currentDir, "r"))
                {
                    Feature.AddToCommandList($"Warning: Skipping directory '{currentDir.Name}' - No read permission.", rchCommandLine, false);
                    return;
                }

                int foundCount = 0;

                foreach (var item in currentDir.Children)
                {
                    try
                    {
                        if (!UserManager.CurrentUser.HasPermission(item, "r"))
                        {
                            continue; // Skip items without read permission
                        }

                        if (type == "f" && item is File file)
                        {
                            Feature.AddToCommandList(NodePathToString((Directory)item.Parent) + '/' + file.Name + '.' + file.FileType, rchCommandLine, false);
                            foundCount++;
                        }
                        else if (type == "d" && item is Directory)
                        {
                            Feature.AddToCommandList(NodePathToString((Directory)item), rchCommandLine, false);
                            foundCount++;
                        }

                        // If item is directory, continue recursively
                        if (item is Directory directory)
                        {
                            SearchByType(directory, type, rchCommandLine);
                        }
                    }
                    catch (Exception ex)
                    {
                        Feature.AddToCommandList($"Warning: Error processing item '{item?.Name}': {ex.Message}. Skipping...", rchCommandLine, false);
                        continue;
                    }
                }

                if (foundCount == 0 && currentDir.Children.Count > 0)
                {
                    string typeDescription = type == "f" ? "files" : "directories";
                    Feature.AddToCommandList($"No {typeDescription} found in '{currentDir.Name}'.", rchCommandLine, false);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error searching directory '{currentDir?.Name}': {ex.Message}.", rchCommandLine, false);
            }
        }

        // Method for wildcard pattern matching (simple)
        private bool IsPatternMatch(string fileName, string pattern, RichTextBox rchCommandLine)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    return false;
                }

                if (string.IsNullOrEmpty(pattern))
                {
                    return false;
                }

                Feature.AddToCommandList($"Checking pattern '{pattern}' against '{fileName}'...", rchCommandLine, false); // Using verbose flag or similar

                // 1. First convert wildcards to regex equivalents
                string regexPattern = pattern
                    .Replace(".", "\\.")  // First escape dot
                    .Replace("*", ".*")   // Then convert * to .*
                    .Replace("?", ".");   // Then convert ? to .

                // 2. Now escape other special regex characters
                // But we already escaped dot, so we need to be careful
                regexPattern = "^" + regexPattern + "$";

                bool match = Regex.IsMatch(fileName, regexPattern, RegexOptions.IgnoreCase);

                if (match)
                {
                    Feature.AddToCommandList($"  ✓ Match found: '{fileName}'", rchCommandLine, false);
                }

                return match;
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error matching pattern '{pattern}' against '{fileName}': {ex.Message}", rchCommandLine, false);
                return false;
            }
        }
        public void Revert(string commitrevert, RichTextBox rchCommandLine)
        {
            try
            {
                if (string.IsNullOrEmpty(commitrevert))
                {
                    Feature.AddToCommandList("Error: Please specify a commit to revert to. Usage: revert <commit_name>", rchCommandLine, false);
                    return;
                }

                Feature.AddToCommandList($"Reverting to commit: {commitrevert}...", rchCommandLine, false);
                LoadAnotherSystemFile(SystemName, commitrevert, rchCommandLine);
                Feature.AddToCommandList($"Successfully reverted to commit '{commitrevert}'.", rchCommandLine, false);
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error reverting to commit '{commitrevert}': {ex.Message}. Please check the commit name and try again.", rchCommandLine, false);
            }
        }

        public void Commit(string commitname, RichTextBox rchCommandLine)
        {
            try
            {
                if (string.IsNullOrEmpty(commitname))
                {
                    Feature.AddToCommandList("Error: Please specify a commit name. Usage: commit <commit_name>", rchCommandLine, false);
                    return;
                }

                if (commitname.Contains(" ") || commitname.Contains("/") || commitname.Contains("\\"))
                {
                    Feature.AddToCommandList($"Error: Invalid commit name '{commitname}'. Commit names cannot contain spaces or path separators.", rchCommandLine, false);
                    return;
                }

                Feature.AddToCommandList($"Creating new commit: {commitname}...", rchCommandLine, false);
                Feature.AddToCommandList($"Current commit: {CommitVersion}", rchCommandLine, false);

                if (UserManager.CopyVfsWithCommit(SystemName, CommitVersion, commitname))
                {
                    Feature.AddToCommandList($"✓ Commit '{commitname}' created successfully.", rchCommandLine, false);
                    Feature.AddToCommandList($"  Previous commit: {CommitVersion}", rchCommandLine, false);
                    Feature.AddToCommandList($"  New commit: {commitname}", rchCommandLine, false);
                    return;
                }

                Feature.AddToCommandList($"✗ Failed to create commit '{commitname}'.", rchCommandLine, false);
                Feature.AddToCommandList($"Possible reasons:", rchCommandLine, false);
                Feature.AddToCommandList($"  - Commit name already exists", rchCommandLine, false);
                Feature.AddToCommandList($"  - Insufficient permissions", rchCommandLine, false);
                Feature.AddToCommandList($"  - System error occurred", rchCommandLine, false);
                return;
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error creating commit '{commitname}': {ex.Message}. Please try again.", rchCommandLine, false);
            }
        }

        public void CommitList(RichTextBox rchCommandLine)
        {
            try
            {
                Feature.AddToCommandList($"Commit history for system: {SystemName}", rchCommandLine, false);
                Feature.AddToCommandList("----------------------------------------", rchCommandLine, false);

                var commits = UserManager.GetAllCommits(SystemName);

                if (commits == null || !commits.Any())
                {
                    Feature.AddToCommandList("No commits found.", rchCommandLine, false);
                    return;
                }

                int commitCount = 0;
                foreach (var item in commits)
                {
                    string currentIndicator = (item == CommitVersion) ? " ← Current" : "";
                    Feature.AddToCommandList($"{item}{currentIndicator}", rchCommandLine, false);
                    commitCount++;
                }

                Feature.AddToCommandList("----------------------------------------", rchCommandLine, false);
                Feature.AddToCommandList($"Total commits: {commitCount}", rchCommandLine, false);
                Feature.AddToCommandList($"Current commit: {CommitVersion}", rchCommandLine, false);
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error retrieving commit list: {ex.Message}. Please try again.", rchCommandLine, false);
            }
        }

        public void DeleteCommit(string commitname, RichTextBox rchCommandLine)
        {
            try
            {
                if (string.IsNullOrEmpty(commitname))
                {
                    Feature.AddToCommandList("Error: Please specify a commit to delete. Usage: deletecommit <commit_name>", rchCommandLine, false);
                    return;
                }

                Feature.AddToCommandList($"Attempting to delete commit: {commitname}...", rchCommandLine, false);

                if (commitname == CommitVersion)
                {
                    Feature.AddToCommandList($"Error: Cannot delete current commit '{CommitVersion}'.", rchCommandLine, false);
                    Feature.AddToCommandList($"  You are currently using this commit.", rchCommandLine, false);
                    Feature.AddToCommandList($"  Switch to a different commit first, then try again.", rchCommandLine, false);
                    return;
                }

                // Confirm deletion for important commits
                if (commitname == "main" || commitname == "master" || commitname == "primary")
                {
                    DialogResult result = MessageBox.Show(
                        $"Warning: You are about to delete an important commit '{commitname}'.\n\n" +
                        "This action cannot be undone. Are you sure you want to continue?",
                        "Confirm Deletion",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);

                    if (result != DialogResult.Yes)
                    {
                        Feature.AddToCommandList($"Deletion of commit '{commitname}' cancelled by user.", rchCommandLine, false);
                        return;
                    }
                }

                if (UserManager.DeleteCommit(SystemName, commitname))
                {
                    Feature.AddToCommandList($"✓ Commit '{commitname}' deleted successfully.", rchCommandLine, false);
                    Feature.AddToCommandList($"  Remaining commits: {string.Join(", ", UserManager.GetAllCommits(SystemName))}", rchCommandLine, false);
                    return;
                }

                Feature.AddToCommandList($"✗ Failed to delete commit '{commitname}'.", rchCommandLine, false);
                Feature.AddToCommandList($"Possible reasons:", rchCommandLine, false);
                Feature.AddToCommandList($"  - Commit does not exist", rchCommandLine, false);
                Feature.AddToCommandList($"  - Insufficient permissions", rchCommandLine, false);
                Feature.AddToCommandList($"  - System error occurred", rchCommandLine, false);
                return;
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error deleting commit '{commitname}': {ex.Message}. Please try again.", rchCommandLine, false);
            }
        }
    }

}
