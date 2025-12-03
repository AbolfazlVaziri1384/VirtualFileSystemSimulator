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
        public Features Feature = new Features();
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
                    break;
                case "touch":
                    TouchCommand(_InputArray, rchCommandList);
                    break;
                case "ls":
                    LsCommand(_InputArray, rchCommandList);
                    break;
                case "cd":
                    CdCommand(_InputArray, rchCommandList);
                    break;
                case "pwd":
                    PwdCommand(_InputArray);
                    break;
                case "rm":
                    RmCommand(_InputArray, rchCommandList);
                    break;
                case "usertype":
                    UserTypeCommand(_InputArray, rchCommandList);
                    break;
                case "tree":
                    TreeCommand(_InputArray, rchCommandList);
                    break;
                case "ln":
                    LnCommand(_InputArray, rchCommandList);
                    break;
                case "stat":
                    StatCommand(_InputArray, rchCommandList);
                    break;
                case "echo":
                    EchoCommand(_InputArray, rchCommandList);
                    break;
                case "cat":
                    CatCommand(_InputArray, rchCommandList);
                    break;
                case "mv":
                    MvCommand(_InputArray, rchCommandList);
                    break;
                case "cp":
                    CpCommand(_InputArray, rchCommandList);
                    break;
                default:
                    Feature.AddToCommandList("Syntax Error", rchCommandList, false);
                    break;
            }
            UpdateCurrentRoute(Fs.CurrentDirectory, txtCurrentRoute);
            UpdateTreeView(TreeView);
            rchCommandList.ScrollToCaret();
        }



        private void Form1_Load(object sender, EventArgs e)
        {
            UpdateTreeView(TreeView);
        }
        public void MkdirCommand(string[] inputs, RichTextBox commandList)
        {
            if (Feature.CheckLength(inputs, 2, 3, rchCommandList))
            {
                if (inputs[1] == "-p")
                {
                    Fs.Mkdir(inputs[2], true, commandList);
                }
                else
                {
                    Fs.Mkdir(inputs[1], false, commandList);
                }
            }
        }
        public void TouchCommand(string[] inputs, RichTextBox commandList)
        {
            if (Feature.CheckLength(inputs, 2, 3, rchCommandList))
            {
                if (inputs[1] == "-t")
                {
                    string _DateTime = inputs[2] + " " + inputs[3];
                    Fs.Touch(inputs[4], commandList, _DateTime);
                }
                else
                {
                    Fs.Touch(inputs[1], commandList);
                }
            }

        }
        public void EchoCommand(string[] inputs, RichTextBox commandList)
        {
            if (Feature.CheckLength(inputs, 3, 6, rchCommandList))
            {
                string _DateTime = null;
                if (inputs.Length > 6 && inputs[3] == "-t")
                {
                    _DateTime = inputs[4] + " " + inputs[5];
                }
                Fs.Echo(inputs[2], inputs[1].Trim('\"'), Fs.CurrentDirectory, commandList, _DateTime);
            }

        }
        public void CatCommand(string[] inputs, RichTextBox commandList)
        {
            if (Feature.CheckLength(inputs, 2, 2, rchCommandList))
            {
                Fs.Cat(inputs[1], commandList);
            }
        }
        public void MvCommand(string[] inputs, RichTextBox commandList)
        {
            if (Feature.CheckLength(inputs, 3, 3, rchCommandList))
            {
                Fs.Mv(inputs, commandList);
            }
        }
        public void CpCommand(string[] inputs, RichTextBox commandList)
        {
            if (Feature.CheckLength(inputs, 3, 3, rchCommandList))
            {
                Fs.Cp(inputs, commandList);
            }
        }
        public void LsCommand(string[] inputs, RichTextBox commandList)
        {
            if (Feature.CheckLength(inputs, 1, 4, rchCommandList))
            {
                bool MoreInfo = inputs.Contains("-l");
                bool ShowHidden = inputs.Contains("-a");
                string Path = null;

                foreach (string input in inputs)
                    if (input.Contains("/"))
                        Path = input;

                Feature.AddToCommandList(Fs.Ls(rchCommandList, Path, MoreInfo, ShowHidden), rchCommandList, false);
            }

        }
        public void CdCommand(string[] inputs, RichTextBox commandList)
        {
            if (Feature.CheckLength(inputs, 1, 2, rchCommandList))
            {
                if (inputs.Length == 2)
                {
                    Fs.Cd(rchCommandList, inputs[1]);
                }
                else
                {
                    //رفتن به ROOT
                    Fs.Cd(rchCommandList);
                }
            }
        }
        public void RmCommand(string[] inputs, RichTextBox commandList)
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
                Fs.Rm(rchCommandList, Name, IsRecursive, IsForce);
            }
        }
        public void UserTypeCommand(string[] inputs, RichTextBox commandList)
        {
            if (Feature.CheckLength(inputs, 1, 1, rchCommandList))
            {
                Feature.AddToCommandList(Fs.ShowUserType().ToString(), rchCommandList, false);
            }
        }
        public void TreeCommand(string[] inputs, RichTextBox commandList)
        {
            if (Feature.CheckLength(inputs, 1, 4, commandList))
            {
                int? deep = null;
                string path = null;

                foreach (string input in inputs.Skip(1))
                {
                    if (input.StartsWith("-n"))
                    {
                        string numberPart = input.Substring(2); // delete "-n"

                        if (string.IsNullOrEmpty(numberPart))
                        {
                            Feature.AddToCommandList("Invalid format for -n parameter. Use: -n<number>", commandList, false);
                            return;
                        }

                        if (int.TryParse(numberPart, out int tempDeep) && tempDeep > 0)
                        {
                            deep = tempDeep;
                        }
                        else
                        {
                            Feature.AddToCommandList($"Invalid depth value: {numberPart}. Depth must be a positive number.", commandList, false);
                            return;
                        }
                    }
                    else if (!input.StartsWith("-"))
                    {
                        path = input;
                    }
                }

                string treeResult = Fs.Tree(commandList, path, null, "", true, deep, 0);
                Feature.AddToCommandList(treeResult, commandList, false);
            }
        }
        public void LnCommand(string[] inputs, RichTextBox commandList)
        {
            if (Feature.CheckLength(inputs, 3, 4, commandList))
            {
                Fs.Ln(rchCommandList, inputs);
            }
        }
        public void StatCommand(string[] inputs, RichTextBox commandList)
        {
            if (Feature.CheckLength(inputs, 2, 3, commandList))
            {
                Fs.Stat(rchCommandList, inputs);
            }
        }
        public void PwdCommand(string[] inputs)
        {
            if (Feature.CheckLength(inputs, 1, 1, rchCommandList))
            {
                Fs.Pwd(Fs.CurrentDirectory, rchCommandList);
            }
        }

        public static void BuildTreeView(Directory directory, TreeNodeCollection nodes)
        {
            TreeNode currentNode = new TreeNode("📁 " + directory.Name)
            {
                Tag = directory,
                ImageKey = "📁",
                SelectedImageKey = "📂",
                ForeColor = Color.Blue
            };
            if (!currentNode.Text.StartsWith("📁 ."))
            {
                nodes.Add(currentNode);
            }
            var sortedChildren = directory.Children.OrderBy(c => c.Name).ToList();

            foreach (var child in sortedChildren)
            {
                if (child is Directory dir)
                {
                    BuildTreeView(dir, currentNode.Nodes);
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

                    //Do not show hidden file
                    if (!fileNode.Text.StartsWith("💾 .") && !fileNode.Text.StartsWith("🔗 ."))
                    {
                        currentNode.Nodes.Add(fileNode);
                    }
                }
            }
        }


        private static void UpdateCurrentRoute(Directory currentDirectory, System.Windows.Forms.TextBox txtCurrentRoute)
        {
            if (currentDirectory == null || txtCurrentRoute == null)
                return;

            try
            {
                txtCurrentRoute.Text = SystemFile.NodePathToString(currentDirectory);
            }
            catch (Exception ex)
            {
                txtCurrentRoute.Text = "/error";
                Console.WriteLine($"Error updating current route: {ex.Message}");
            }
        }
        private void UpdateTreeView(System.Windows.Forms.TreeView treeview)
        {
            treeview.BeginUpdate();
            treeview.Nodes.Clear();

            BuildTreeView(Fs.Root, treeview.Nodes);

            treeview.ExpandAll();
            treeview.EndUpdate();
        }
        public string LastCommands = string.Empty;

        private void txtCommandLine_TextChanged(object sender, EventArgs e)
        {
            LastCommands = txtCommandLine.Text;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            System.Windows.Forms.Application.Exit();
        }

    }
}
