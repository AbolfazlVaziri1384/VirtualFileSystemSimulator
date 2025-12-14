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
using VirtualFileSystemSimulatorWinForm;

namespace VirtualFileSystemSimulatorWinForm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        public SystemFile Fs;
        public static Features Feature = new Features();
        public bool IsFirstUpKey = true;
        public Stack<string> FirstAllCommand = new Stack<string>();
        public Stack<string> LastAllCommand = new Stack<string>();
        private void txtCommandLine_KeyDown(object sender, KeyEventArgs e)
        {
            string _Input = string.Empty;

            if (e.KeyCode == Keys.Enter)
            {
                // For delete "beb" sound with enter key
                e.SuppressKeyPress = true;
                _Input = txtCommandLine.Text;
                Feature.AddToCommandList(_Input, rchCommandList);
                AnalizeInput(_Input);
                txtCommandLine.Text = string.Empty;
                LastCommands = string.Empty;
                IsFirstUpKey = true;
            }

            // For use before send's commands
            if (e.KeyCode == Keys.Up)
            {
                // For delete "beb" sound with enter key
                e.SuppressKeyPress = true;

                try
                {
                    if (IsFirstUpKey)
                    {
                        FirstAllCommand.Clear();
                        LastAllCommand.Clear();
                        string[] _AllCommand = rchCommandList.Text.Trim().Split('\n').ToArray();
                        foreach (string item in _AllCommand)
                        {
                            if (!string.IsNullOrEmpty(item) && item.StartsWith(">"))
                            {
                                FirstAllCommand.Push(item.Substring(2));
                            }
                        }
                        FirstAllCommand.Push(LastCommands);
                    }
                    LastAllCommand.Push(FirstAllCommand.Pop());
                    txtCommandLine.Text = LastAllCommand.Peek();
                    IsFirstUpKey = false;
                }
                catch (Exception)
                {

                }

            }

            // For use before send's commands
            if (e.KeyCode == Keys.Down)
            {
                // For delete "beb" sound with enter key
                e.SuppressKeyPress = true;

                try
                {
                    if (!IsFirstUpKey)
                    {
                        FirstAllCommand.Push(LastAllCommand.Pop());
                        txtCommandLine.Text = FirstAllCommand.Peek();
                    }
                }
                catch (Exception)
                {

                }

            }
            // For Auto Complete with press "shift"
            if (e.KeyCode == Keys.ShiftKey)
            {
                // For delete "beb" sound with enter key
                e.SuppressKeyPress = true;

                try
                {
                    Feature.AutoCompleteCommand(txtCommandLine);
                }
                catch (Exception)
                {

                }

            }
        }

        // For Analizing inputs
        public void AnalizeInput(string input)
        {
            string[] _InputArray = input.Trim().Split(' ').ToArray();
            switch (_InputArray[0])
            {
                case "mkdir":
                    MkdirCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "touch":
                    TouchCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "ls":
                    LsCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "cd":
                    CdCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "pwd":
                    PwdCommand(_InputArray);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "rm":
                    RmCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "usertype":
                    UserTypeCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "changeusertype":
                    ChangeUserTypeCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "tree":
                    TreeCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "ln":
                    LnCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "stat":
                    StatCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "echo":
                    EchoCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "cat":
                    CatCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "mv":
                    MvCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "cp":
                    CpCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "load":
                    LoadCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "addgroup":
                    AddGroupCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "rmgroup":
                    RemoveGroupCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "systemfile":
                    SystemFileCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "find":
                    FindCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "chmod":
                    ChmodCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "revert":
                    RevertCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "commit":
                    CommitCommand(_InputArray, rchCommandList);
                    UpdateTreeView(TreeView, rchCommandList);
                    break;
                case "open":
                    OpenTreeViewCommand(_InputArray, rchCommandList);
                    break;
                case "close":
                    CloseTreeViewCommand(_InputArray, rchCommandList);
                    break;
                default:
                    Feature.AddToCommandList("Syntax Error", rchCommandList, false);
                    break;
            }
            UpdateCurrentRoute(Fs.CurrentDirectory, txtCurrentRoute, rchCommandList);
            rchCommandList.ScrollToCaret();
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateTreeView(TreeView, rchCommandList);
        }
        public void MkdirCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 2, 3, rchCommandList))
                {
                    if (inputs[1] == "-p")
                    {
                        if (inputs.Length < 3)
                        {
                            Feature.AddToCommandList("Error: Directory name is required when using -p flag. Usage: mkdir -p <directory_name>", commandList, false);
                            return;
                        }
                        Feature.AddToCommandList($"Creating directory '{inputs[2]}' with parent directories...", commandList, false);
                        Fs.Mkdir(inputs[2], true, commandList);
                    }
                    else
                    {
                        Feature.AddToCommandList($"Creating directory '{inputs[1]}'...", commandList, false);
                        Fs.Mkdir(inputs[1], false, commandList);
                    }
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in mkdir command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }

        public void TouchCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 2, 3, rchCommandList))
                {
                    if (inputs[1] == "-t")
                    {
                        if (inputs.Length < 5)
                        {
                            Feature.AddToCommandList("Error: Insufficient parameters for -t option. Usage: touch -t <date> <time> <filename>", commandList, false);
                            return;
                        }
                        string _DateTime = inputs[2] + " " + inputs[3];
                        Feature.AddToCommandList($"Creating file '{inputs[4]}' with custom timestamp {_DateTime}...", commandList, false);
                        Fs.Touch(inputs[4], commandList, _DateTime);
                    }
                    else
                    {
                        Feature.AddToCommandList($"Creating file '{inputs[1]}'...", commandList, false);
                        Fs.Touch(inputs[1], commandList);
                    }
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in touch command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }

        public void EchoCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 3, 6, rchCommandList))
                {
                    string _DateTime = null;
                    if (inputs.Length > 6 && inputs[3] == "-t")
                    {
                        if (inputs.Length < 6)
                        {
                            Feature.AddToCommandList("Error: Insufficient parameters for -t option. Usage: echo <content> <filename> -t <date> <time>", commandList, false);
                            return;
                        }
                        _DateTime = inputs[4] + " " + inputs[5];
                        Feature.AddToCommandList($"Writing content to file '{inputs[2]}' with custom timestamp {_DateTime}...", commandList, false);
                    }
                    else
                    {
                        Feature.AddToCommandList($"Writing content to file '{inputs[2]}'...", commandList, false);
                    }

                    string content = inputs[1].Trim('\"');
                    Feature.AddToCommandList($"Content length: {content.Length} characters", commandList, false);
                    Fs.Echo(inputs[2], content, Fs.CurrentDirectory, commandList, _DateTime);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in echo command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }

        public void CatCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 2, 2, rchCommandList))
                {
                    Feature.AddToCommandList($"Displaying content of file '{inputs[1]}'...", commandList, false);
                    Fs.Cat(inputs[1], commandList);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in cat command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }

        public void MvCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 3, 3, rchCommandList))
                {
                    Feature.AddToCommandList($"Moving/Renaming '{inputs[1]}' to '{inputs[2]}'...", commandList, false);
                    Fs.Mv(inputs, commandList);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in mv command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }

        public void CpCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 3, 3, rchCommandList))
                {
                    Feature.AddToCommandList($"Copying '{inputs[1]}' to '{inputs[2]}'...", commandList, false);
                    Fs.Cp(inputs, commandList);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in cp command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }

        public void OpenTreeViewCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 2, 2, rchCommandList))
                {
                    if (inputs[1] == "-a")
                    {
                        Feature.AddToCommandList("Expanding all nodes in tree view...", commandList, false);
                        OpenAllTreeView(TreeView);
                        Feature.AddToCommandList("All tree view nodes expanded.", commandList, false);
                    }
                    else if (inputs[1] == "-c")
                    {
                        Feature.AddToCommandList("Collapsing tree view to current path only...", commandList, false);
                        KeepOnlyCurrentPathExpanded(TreeView);
                        Feature.AddToCommandList("Tree view collapsed to current path.", commandList, false);
                    }
                    else if (!string.IsNullOrEmpty(inputs[1]))
                    {
                        Feature.AddToCommandList($"Expanding path '{inputs[1]}' in tree view...", commandList, false);
                        KeepOnlyPathExpanded(inputs[1], TreeView);
                        Feature.AddToCommandList($"Path '{inputs[1]}' expanded in tree view.", commandList, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in tree view command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }

        public void CloseTreeViewCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 1, 1, rchCommandList))
                {
                    Feature.AddToCommandList("Collapsing all nodes in tree view...", commandList, false);
                    CloseAllTreeView(TreeView);
                    Feature.AddToCommandList("Tree view collapsed.", commandList, false);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in close tree view command: {ex.Message}.", commandList, false);
            }
        }

        // For Load Another FileSystem
        public void LoadCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 2, 3, rchCommandList))
                {
                    string _CommitVersion = "main";
                    if (inputs.Length == 3)
                    {
                        _CommitVersion = inputs[2];
                        Feature.AddToCommandList($"Loading system file '{inputs[1]}' with commit version '{_CommitVersion}'...", commandList , false);
                    }
                    else
                    {
                        Feature.AddToCommandList($"Loading system file '{inputs[1]}' with default commit version 'main'...", commandList, false);
                    }

                    Fs.LoadAnotherSystemFile(inputs[1], _CommitVersion, commandList);
                    Feature.AddToCommandList($"System file '{inputs[1]}' loaded successfully.", commandList, false);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in load command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }

        public void RevertCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 2, 2, rchCommandList))
                {
                    Feature.AddToCommandList($"Reverting to commit '{inputs[1]}'...", commandList, false);
                    Fs.Revert(inputs[1], commandList);
                    Feature.AddToCommandList($"Reverted to commit '{inputs[1]}'.", commandList, false);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in revert command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }
        public void CommitCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 2, 3, rchCommandList))
                {
                    if (inputs[1] == "-v")
                    {
                        Feature.AddToCommandList($"Current Commit Version: {Fs.CommitVersion}", rchCommandList, false);
                        return;
                    }
                    else if (inputs[1] == "-l")
                    {
                        Feature.AddToCommandList("Listing all commits...", rchCommandList, false);
                        Fs.CommitList(commandList);
                        return;
                    }
                    else if (inputs[1] == "-m")
                    {
                        if (inputs.Length < 3 || inputs[2] == "\"\"")
                        {
                            Feature.AddToCommandList("Error: Commit name is required. Usage: commit -m \"<commit_name>\"", rchCommandList, false);
                            return;
                        }
                        string commitName = inputs[2].Trim().Trim('"');
                        Feature.AddToCommandList($"Creating new commit: {commitName}", rchCommandList, false);
                        Fs.Commit(commitName, commandList);
                        return;
                    }
                    else if (inputs[1] == "-d")
                    {
                        if (inputs.Length < 3 || inputs[2] == "\"\"")
                        {
                            Feature.AddToCommandList("Error: Commit name is required for deletion. Usage: commit -d \"<commit_name>\"", rchCommandList, false);
                            return;
                        }
                        string commitName = inputs[2].Trim().Trim('"');
                        Feature.AddToCommandList($"Deleting commit: {commitName}", rchCommandList, false);
                        Fs.DeleteCommit(commitName, commandList);
                        return;
                    }
                    else
                    {
                        Feature.AddToCommandList($"Error: Unknown commit option '{inputs[1]}'", rchCommandList, false);
                        Feature.AddToCommandList("Available commit options:", rchCommandList, false);
                        Feature.AddToCommandList("  -v : Show current commit version", rchCommandList, false);
                        Feature.AddToCommandList("  -l : List all commits", rchCommandList, false);
                        Feature.AddToCommandList("  -m : Make a new commit", rchCommandList, false);
                        Feature.AddToCommandList("  -d : Delete a commit", rchCommandList, false);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in commit command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }

        public void AddGroupCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 3, 3, rchCommandList))
                {
                    string username = inputs[1];
                    string permissionType = inputs[2];

                    Feature.AddToCommandList($"Adding user '{username}' to group with permission '{permissionType}'...", rchCommandList , false);

                    switch (permissionType)
                    {
                        case "owner":
                            Fs.AddGroupsForUser(username, User.UserTypeEnum.Owner, rchCommandList);
                            break;
                        case "group":
                            Fs.AddGroupsForUser(username, User.UserTypeEnum.Group, rchCommandList);
                            break;
                        case "other":
                            Fs.AddGroupsForUser(username, User.UserTypeEnum.Others, rchCommandList);
                            break;
                        case "admin":
                            Fs.AddGroupsForUser(username, User.UserTypeEnum.Admin, rchCommandList);
                            break;
                        default:
                            Feature.AddToCommandList($"Error: Unknown permission type '{permissionType}'", rchCommandList, false);
                            Feature.AddToCommandList("Available permission types: owner, group, other, admin", rchCommandList, false);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in addgroup command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }

        public void RemoveGroupCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 3, 3, rchCommandList))
                {
                    string _Groupname = inputs[1];
                    string _Username = inputs[2];

                    Feature.AddToCommandList($"Removing user '{_Username}' from group '{_Groupname}'...", rchCommandList, false);
                    Fs.RemoveGroupsForUser(_Groupname, _Username, commandList);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in removegroup command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }

        public void SystemFileCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 1, 1, rchCommandList))
                {
                    Feature.AddToCommandList($"Current System File: {Fs.SystemName}", rchCommandList, false);
                    Feature.AddToCommandList($"Current Commit: {Fs.CommitVersion}", rchCommandList, false);
                    Feature.AddToCommandList($"Current Directory: {Fs.NodePathToString(Fs.CurrentDirectory)}", rchCommandList, false);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in systemfile command: {ex.Message}", commandList, false);
            }
        }

        public void FindCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 4, 4, rchCommandList))
                {
                    string path = inputs[1];
                    string option = inputs[2];
                    string pattern = inputs[3].Trim().Trim('"');

                    Feature.AddToCommandList($"Searching in '{path}' with option '{option}' for pattern '{pattern}'...", rchCommandList, false);
                    Fs.Find(path, option, pattern, commandList);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in find command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }

        public void ChmodCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 3, 3, rchCommandList))
                {
                    string permission = inputs[1];
                    string target = inputs[2];

                    Feature.AddToCommandList($"Changing permissions of '{target}' to '{permission}'...", rchCommandList, false);
                    Fs.Chmod(permission, target, commandList);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in chmod command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }

        public void LsCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 1, 4, rchCommandList))
                {
                    bool MoreInfo = inputs.Contains("-l");
                    bool ShowHidden = inputs.Contains("-a");
                    string Path = null;

                    foreach (string input in inputs)
                        if (input.Contains("/"))
                            Path = input;

                    string options = "";
                    if (MoreInfo) options += "detailed view ";
                    if (ShowHidden) options += "show hidden ";
                    if (!string.IsNullOrEmpty(Path)) options += $"path: {Path} ";

                    Feature.AddToCommandList($"Listing directory contents ({options.Trim()})...", rchCommandList, false);

                    string result = Fs.Ls(rchCommandList, Path, MoreInfo, ShowHidden);
                    if (!string.IsNullOrEmpty(result))
                    {
                        Feature.AddToCommandList(result, rchCommandList, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in ls command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }

        public void CdCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 1, 2, rchCommandList))
                {
                    if (inputs.Length == 2)
                    {
                        Feature.AddToCommandList($"Changing directory to '{inputs[1]}'...", rchCommandList, false);
                        Fs.Cd(rchCommandList, inputs[1]);
                    }
                    else
                    {
                        // Go to ROOT
                        Feature.AddToCommandList("Changing to root directory...", rchCommandList , false);
                        Fs.Cd(rchCommandList);
                    }
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in cd command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }

        public void RmCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 2, 4, rchCommandList))
                {
                    bool IsRecursive = inputs.Contains("-r");
                    bool IsForce = inputs.Contains("-f");
                    string Name;

                    if (inputs.Contains("-rf"))
                    {
                        IsRecursive = true;
                        IsForce = true;
                    }
                    Name = inputs[inputs.Length - 1];

                    string options = "";
                    if (IsRecursive) options += "recursive ";
                    if (IsForce) options += "force ";

                    Feature.AddToCommandList($"Removing '{Name}' ({options.Trim()})...", rchCommandList, false);
                    Fs.Rm(rchCommandList, Name, IsRecursive, IsForce);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in rm command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }

        public void UserTypeCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 1, 1, rchCommandList))
                {
                    string userType = Fs.ShowUserType().ToString();
                    Feature.AddToCommandList($"Current User Type: {userType}", rchCommandList, false);
                    Feature.AddToCommandList($"Username: {Fs.UserManager.CurrentUser?.Username ?? "Unknown"}", rchCommandList, false);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in usertype command: {ex.Message}", commandList, false);
            }
        }

        public void ChangeUserTypeCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 3, 3, rchCommandList))
                {
                    string username = inputs[1];
                    string newType = inputs[2];

                    Feature.AddToCommandList($"Changing user type for '{username}' to '{newType}'...", rchCommandList, false    );

                    switch (newType)
                    {
                        case "owner":
                            Fs.ChangeUserType(username, User.UserTypeEnum.Owner, rchCommandList);
                            break;
                        case "group":
                            Fs.ChangeUserType(username, User.UserTypeEnum.Group, rchCommandList);
                            break;
                        case "other":
                            Fs.ChangeUserType(username, User.UserTypeEnum.Others, rchCommandList);
                            break;
                        case "admin":
                            Fs.ChangeUserType(username, User.UserTypeEnum.Admin, rchCommandList);
                            break;
                        default:
                            Feature.AddToCommandList($"Error: Unknown user type '{newType}'", rchCommandList, false);
                            Feature.AddToCommandList("Available user types: owner, group, other, admin", rchCommandList, false);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in changeusertype command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }

        public void TreeCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 1, 4, commandList))
                {
                    int? deep = null;
                    string path = null;

                    foreach (string input in inputs.Skip(1))
                    {
                        if (input.StartsWith("-n"))
                        {
                            string numberPart = input.Substring(2); // remove "-n"

                            if (string.IsNullOrEmpty(numberPart))
                            {
                                Feature.AddToCommandList("Error: Invalid format for -n parameter. Use: -n<number> (e.g., -n3)", commandList, false);
                                return;
                            }

                            if (int.TryParse(numberPart, out int tempDeep) && tempDeep > 0)
                            {
                                deep = tempDeep;
                                Feature.AddToCommandList($"Setting tree depth limit: {deep} levels", commandList, false);
                            }
                            else
                            {
                                Feature.AddToCommandList($"Error: Invalid depth value '{numberPart}'. Depth must be a positive number.", commandList, false);
                                return;
                            }
                        }
                        else if (!input.StartsWith("-"))
                        {
                            path = input;
                            Feature.AddToCommandList($"Tree starting from path: {path}", commandList, false);
                        }
                    }

                    if (path == null)
                    {
                        Feature.AddToCommandList($"Tree starting from current directory: {Fs.NodePathToString((Directory)Fs.CurrentDirectory)}", commandList, false);
                    }

                    Feature.AddToCommandList("Generating directory tree...", commandList, false);
                    string treeResult = Fs.Tree(commandList, path, null, "", true, deep, 0);

                    if (!string.IsNullOrEmpty(treeResult))
                    {
                        Feature.AddToCommandList("Directory Tree:", commandList, false);
                        Feature.AddToCommandList(treeResult, commandList, false);
                    }
                    else
                    {
                        Feature.AddToCommandList("Tree is empty or no items to display.", commandList, false);
                    }
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in tree command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }
        public void LnCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 3, 4, commandList))
                {
                    Feature.AddToCommandList("Creating link...", commandList, false     );
                    Fs.Ln(rchCommandList, inputs);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in ln command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }

        public void StatCommand(string[] inputs, RichTextBox commandList)
        {
            try
            {
                if (Feature.CheckLength(inputs, 2, 3, commandList))
                {
                    string target = inputs[1];
                    Feature.AddToCommandList($"Getting file information for '{target}'...", commandList , false);
                    Fs.Stat(rchCommandList, inputs);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in stat command: {ex.Message}. Please check command syntax.", commandList, false);
            }
        }

        public void PwdCommand(string[] inputs)
        {
            try
            {
                if (Feature.CheckLength(inputs, 1, 1, rchCommandList))
                {
                    Feature.AddToCommandList("Getting current directory...", rchCommandList, false);
                    Fs.Pwd(Fs.CurrentDirectory, rchCommandList);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error in pwd command: {ex.Message}", rchCommandList, false);
            }
        }

        public static void BuildTreeView(Directory directory, TreeNodeCollection nodes, RichTextBox commandList)
        {
            try
            {
                if (directory == null)
                {
                    return;
                }

                TreeNode currentNode = new TreeNode("📁 " + directory.Name)
                {
                    Tag = directory,
                    ImageKey = "📁",
                    SelectedImageKey = "📂",
                    ForeColor = Color.Blue
                };

                // Don't add hidden directories (starting with dot)
                if (!currentNode.Text.StartsWith("📁 ."))
                {
                    nodes.Add(currentNode);
                }

                var sortedChildren = directory.Children.OrderBy(c => c.Name).ToList();


                foreach (var child in sortedChildren)
                {
                    try
                    {
                        if (child is Directory dir)
                        {
                            BuildTreeView(dir, currentNode.Nodes, commandList);
                        }
                        else
                        {
                            File file = (File)child;
                            TreeNode fileNode;

                            if (!file.IsLink)
                            {
                                fileNode = new TreeNode("💾 " + child.Name)
                                {
                                    Tag = child,
                                    ImageKey = "💾",
                                    SelectedImageKey = "💾",
                                    ForeColor = Color.Red
                                };
                            }
                            else
                            {
                                fileNode = new TreeNode("🔗 " + child.Name)
                                {
                                    Tag = child,
                                    ImageKey = "🔗",
                                    SelectedImageKey = "🔗",
                                    ForeColor = Color.Violet
                                };
                            }

                            // Do not show hidden files
                            if (!fileNode.Text.StartsWith("💾 .") && !fileNode.Text.StartsWith("🔗 ."))
                            {
                                currentNode.Nodes.Add(fileNode);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Feature.AddToCommandList($"Warning: Error processing item '{child?.Name}' in tree view: {ex.Message}. Skipping...", commandList, false);
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error building tree view: {ex.Message}", commandList, false);
            }
        }

        private void UpdateCurrentRoute(Directory currentDirectory, System.Windows.Forms.TextBox txtCurrentRoute, RichTextBox commandList)
        {
            try
            {
                if (currentDirectory == null)
                {
                    txtCurrentRoute.Text = "/error (directory not found)";
                    Feature.AddToCommandList("Warning: Current directory is null. Cannot update route display.", commandList, false);
                    return;
                }

                if (txtCurrentRoute == null)
                {
                    Feature.AddToCommandList("Error: Route text box is null. Cannot update display.", commandList, false);
                    return;
                }

                try
                {
                    string path = Fs.NodePathToString(currentDirectory);
                    txtCurrentRoute.Text = path;
                }
                catch (Exception ex)
                {
                    txtCurrentRoute.Text = "/error";
                    Feature.AddToCommandList($"Error updating current route: {ex.Message}. Path display may be incorrect.", commandList, false);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Unexpected error in UpdateCurrentRoute: {ex.Message}", commandList, false);
            }
        }

        private void OpenAllTreeView(System.Windows.Forms.TreeView treeview)
        {
            try
            {
                Feature.AddToCommandList("Expanding all nodes in tree view...", rchCommandList, false);
                treeview.ExpandAll();
                Feature.AddToCommandList("All tree view nodes expanded successfully.", rchCommandList, false);
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error expanding tree view: {ex.Message}", rchCommandList, false);
            }
        }

        private void CloseAllTreeView(System.Windows.Forms.TreeView treeview)
        {
            try
            {
                Feature.AddToCommandList("Collapsing all nodes in tree view...", rchCommandList, false);
                treeview.CollapseAll();
                Feature.AddToCommandList("All tree view nodes collapsed successfully.", rchCommandList, false   );
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error collapsing tree view: {ex.Message}", rchCommandList, false);
            }
        }

        private void KeepOnlyPathExpanded(string path, System.Windows.Forms.TreeView treeView)
        {
            try
            {
                Feature.AddToCommandList($"Expanding only path '{path}' in tree view...", rchCommandList , false);

                // First collapse all nodes
                treeView.CollapseAll();

                TreeNode currentNode = null;

                // Find node related to current directory
                var directoryNode = Fs.ResolvePath(path, rchCommandList);
                if (directoryNode is Directory _Directory)
                {
                    currentNode = FindTreeNodeByDirectory(treeView.Nodes, _Directory);
                }
                else
                {
                    Feature.AddToCommandList($"Path '{path}' not found or is not a directory.", rchCommandList, false);
                    return;
                }

                if (currentNode != null)
                {
                    // Expand path from root to selected node
                    TreeNode nodeToExpand = currentNode;
                    while (nodeToExpand != null)
                    {
                        nodeToExpand.Expand();
                        nodeToExpand = nodeToExpand.Parent;
                    }

                    // Also select the node
                    treeView.SelectedNode = currentNode;
                    currentNode.EnsureVisible();

                    Feature.AddToCommandList($"Path '{path}' expanded and selected in tree view.", rchCommandList, false);
                }
                else
                {
                    Feature.AddToCommandList($"Could not find tree node for path '{path}'.", rchCommandList, false);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error expanding path in tree view: {ex.Message}", rchCommandList, false);
            }
        }

        private void KeepOnlyCurrentPathExpanded(System.Windows.Forms.TreeView treeView)
        {
            try
            {

                // First collapse all nodes
                treeView.CollapseAll();

                // Find node related to current directory
                TreeNode currentNode = FindTreeNodeByDirectory(treeView.Nodes, Fs.CurrentDirectory);

                if (currentNode != null)
                {
                    // Expand path from root to selected node
                    TreeNode nodeToExpand = currentNode;
                    while (nodeToExpand != null)
                    {
                        nodeToExpand.Expand();
                        nodeToExpand = nodeToExpand.Parent;
                    }

                    // Also select the node
                    treeView.SelectedNode = currentNode;
                    currentNode.EnsureVisible();

                }
                else
                {
                    Feature.AddToCommandList("Could not find current directory in tree view.", rchCommandList, false);
                }
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error expanding current path in tree view: {ex.Message}", rchCommandList, false);
            }
        }

        private TreeNode FindTreeNodeByDirectory(TreeNodeCollection nodes, Directory directory)
        {
            try
            {
                if (directory == null)
                {
                    return null;
                }

                foreach (TreeNode node in nodes)
                {
                    // If node Tag equals directory
                    if (node.Tag != null && node.Tag == directory)
                    {
                        return node;
                    }

                    // Search in children
                    TreeNode foundNode = FindTreeNodeByDirectory(node.Nodes, directory);
                    if (foundNode != null)
                    {
                        return foundNode;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error finding tree node for directory '{directory?.Name}': {ex.Message}", rchCommandList, false);
                return null;
            }
        }

        private void UpdateTreeView(System.Windows.Forms.TreeView treeview, RichTextBox commandList)
        {
            try
            {
                treeview.BeginUpdate();
                treeview.Nodes.Clear();

                BuildTreeView(Fs.Root, treeview.Nodes, commandList);

                KeepOnlyCurrentPathExpanded(treeview);
                treeview.EndUpdate();
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error updating tree view: {ex.Message}", rchCommandList, false);
                try
                {
                    treeview.EndUpdate(); // Ensure EndUpdate is called even on error
                }
                catch { }
            }
        }

        public string LastCommands = string.Empty;

        private void txtCommandLine_TextChanged(object sender, EventArgs e)
        {
            try
            {
                LastCommands = txtCommandLine.Text;
            }
            catch (Exception ex)
            {
                Feature.AddToCommandList($"Error updating command history: {ex.Message}", rchCommandList, false);
            }
        }
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

    }
}
