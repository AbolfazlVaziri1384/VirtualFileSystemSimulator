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
        //برای کل برنامه
        public SystemFile fs = new SystemFile();
        public Features features = new Features();
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
                features.AddToCommandList(Input, rchCommandList);
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
                    features.AutoCompleteCommand(txtCommandLine);
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
                    break;
                case "touch":
                    Touch_Command(InputArray, rchCommandList, TreeView);
                    break;
                case "ls":
                    Ls_Command(InputArray, rchCommandList, TreeView);
                    break;
                case "cd":
                    Cd_Command(InputArray, rchCommandList, TreeView);
                    break;
                case "pwd":
                    Pwd_Command(InputArray, TreeView);
                    break;
                case "rm":
                    Rm_Command(InputArray, rchCommandList, TreeView);
                    break;
                case "usertype":
                    Usertype_Command(InputArray, rchCommandList, TreeView);
                    break;
                case "tree":
                    Tree_Command(InputArray, rchCommandList, TreeView);
                    break;
                case "ln":
                    Ln_Command(InputArray, rchCommandList, TreeView);
                    break;
                case "stat":
                    Stat_Command(InputArray, rchCommandList, TreeView);
                    break;
                case "echo":
                    Echo_Command(InputArray, rchCommandList, TreeView);
                    break;
                case "cat":
                    Cat_Command(InputArray, rchCommandList, TreeView);
                    break;
                case "mv":
                    Mv_Command(InputArray, rchCommandList, TreeView);
                    break;
                case "cp":
                    Cp_Command(InputArray, rchCommandList, TreeView);
                    break;
                default:
                    features.AddToCommandList("Syntax Error", rchCommandList, false);
                    break;
            }
            UpdateCurrentRoute(fs.CurrentDirectory, txtCurrentRoute);
            rchCommandList.ScrollToCaret();
        }



        private void Form1_Load(object sender, EventArgs e)
        {

        }
        public void Mkdir_Command(string[] Inputs, RichTextBox commandList, System.Windows.Forms.TreeView treeView)
        {
            if (features.CheckLength(Inputs, 2, 3, rchCommandList))
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
        public void Touch_Command(string[] Inputs, RichTextBox commandList, System.Windows.Forms.TreeView treeView)
        {
            if (features.CheckLength(Inputs, 2, 3, rchCommandList))
            {
                if (Inputs[1] == "-t")
                {
                    string dateTime = Inputs[2] + " " + Inputs[3];
                    fs.CreateFile(Inputs[4], commandList, dateTime);
                }
                else
                {
                    fs.CreateFile(Inputs[1], commandList);
                }
                UpdateTreeView(treeView);
            }

        }
        public void Echo_Command(string[] Inputs, RichTextBox commandList, System.Windows.Forms.TreeView treeView)
        {
            if (features.CheckLength(Inputs, 3, 6, rchCommandList))
            {
                string dateTime = null;
                if (Inputs.Length > 6 && Inputs[3] == "-t")
                {
                    dateTime = Inputs[4] + " " + Inputs[5];
                }
                fs.Echo(Inputs[2], Inputs[1].Trim('\"'), fs.CurrentDirectory, commandList, dateTime);
                UpdateTreeView(treeView);
            }

        }
        public void Cat_Command(string[] Inputs, RichTextBox commandList, System.Windows.Forms.TreeView treeView)
        {
            if (features.CheckLength(Inputs, 2, 2, rchCommandList))
            {
                fs.Cat(Inputs[1], commandList);
                UpdateTreeView(treeView);
            }

        }
        public void Mv_Command(string[] Inputs, RichTextBox commandList, System.Windows.Forms.TreeView treeView)
        {
            if (features.CheckLength(Inputs, 3, 3, rchCommandList))
            {
                fs.Mv(Inputs, commandList);
                UpdateTreeView(treeView);
            }

        }
        public void Cp_Command(string[] Inputs, RichTextBox commandList, System.Windows.Forms.TreeView treeView)
        {
            if (features.CheckLength(Inputs, 3, 3, rchCommandList))
            {
                fs.Cp(Inputs, commandList);
                UpdateTreeView(treeView);
            }

        }
        public void Ls_Command(string[] Inputs, RichTextBox commandList, System.Windows.Forms.TreeView treeView)
        {
            if (features.CheckLength(Inputs, 1, 4, rchCommandList))
            {
                bool MoreInfo = false;
                bool ShowHidden = false;
                string path = null;

                if (Inputs.Contains("-l"))
                    MoreInfo = true;
                if (Inputs.Contains("-a"))
                    ShowHidden = true;

                foreach (string input in Inputs)
                    if (input.Contains("/"))
                        path = input;


                features.AddToCommandList(fs.LsShow(rchCommandList, path, MoreInfo, ShowHidden), rchCommandList, false);

                UpdateTreeView(treeView);
            }

        }
        public void Cd_Command(string[] Inputs, RichTextBox commandList, System.Windows.Forms.TreeView treeView)
        {
            if (features.CheckLength(Inputs, 1, 2, rchCommandList))
            {
                if (Inputs.Length == 2)
                {
                    fs.Cd(rchCommandList, Inputs[1]);
                }
                else
                {
                    //رفتن به ROOT
                    fs.Cd(rchCommandList);
                }
            }
            UpdateTreeView(treeView);

        }
        public void Rm_Command(string[] Inputs, RichTextBox commandList, System.Windows.Forms.TreeView treeView)
        {
            if (features.CheckLength(Inputs, 2, 4, rchCommandList))
            {
                bool IsRecursive = false;
                bool IsForce = false;
                string Name;

                if (Inputs.Contains("-rf"))
                {
                    IsRecursive = true;
                    IsForce = true;
                }
                else
                {
                    if (Inputs.Contains("-r"))
                        IsRecursive = true;
                    if (Inputs.Contains("-f"))
                        IsForce = true;
                }
                Name = Inputs[Inputs.Length - 1];
                fs.Rm(rchCommandList, Name, IsRecursive, IsForce);
            }
            UpdateTreeView(treeView);

        }
        public void Usertype_Command(string[] Inputs, RichTextBox commandList, System.Windows.Forms.TreeView treeView)
        {
            if (features.CheckLength(Inputs, 1, 2, rchCommandList))
            {
                if (Inputs.Length == 1)
                {
                    features.AddToCommandList(fs.ShowUserType().ToString(), rchCommandList, false);
                }
                else
                {
                    switch (Inputs[1])
                    {
                        case "owner":
                            fs.ChangeUserType(SystemFile.UserType.owner);
                            features.AddToCommandList($"Change is succsesfully! \nNow your user is {fs.ShowUserType().ToString()}", rchCommandList, false);
                            break;
                        case "group":
                            fs.ChangeUserType(SystemFile.UserType.group);
                            features.AddToCommandList($"Change is succsesfully! \nNow your user is {fs.ShowUserType().ToString()}", rchCommandList, false);
                            break;
                        case "others":
                            fs.ChangeUserType(SystemFile.UserType.others);
                            features.AddToCommandList($"Change is succsesfully! \nNow your user is {fs.ShowUserType().ToString()}", rchCommandList, false);
                            break;
                        default:
                            features.AddToCommandList("Not found your user type", rchCommandList, false);
                            break;
                    }
                }
            }
            UpdateTreeView(treeView);

        }
        public void Tree_Command(string[] Inputs, RichTextBox commandList, System.Windows.Forms.TreeView treeView)
        {
            if (features.CheckLength(Inputs, 1, 4, commandList))
            {
                int? deep = null; // استفاده از nullable برای حالت پیش‌فرض
                string path = null;

                foreach (string input in Inputs.Skip(1)) // از ایندکس ۱ شروع کن (پرش از دستور "tree")
                {
                    if (input.StartsWith("-n"))
                    {
                        // پردازش پارامترهای مختلف: -n3 یا -n 3
                        string numberPart = input.Substring(2); // حذف "-n"

                        if (string.IsNullOrEmpty(numberPart))
                        {
                            features.AddToCommandList("Invalid format for -n parameter. Use: -n<number>", commandList, false);
                            return;
                        }

                        if (int.TryParse(numberPart, out int tempDeep) && tempDeep > 0)
                        {
                            deep = tempDeep;
                        }
                        else
                        {
                            features.AddToCommandList($"Invalid depth value: {numberPart}. Depth must be a positive number.", commandList, false);
                            return;
                        }
                    }
                    else if (!input.StartsWith("-")) // اگر آرگومان، پارامتر نیست (مسیر است)
                    {
                        path = input;
                    }
                }

                // فراخوانی تابع Tree با پارامترهای استخراج شده
                string treeResult = fs.Tree(commandList, path, null, "", true, deep, 0);
                features.AddToCommandList(treeResult, commandList, false);
            }
            UpdateTreeView(treeView);
        }
        public void Ln_Command(string[] Inputs, RichTextBox commandList, System.Windows.Forms.TreeView treeView)
        {
            if (features.CheckLength(Inputs, 3, 4, commandList))
            {
                fs.Ln(rchCommandList, Inputs);
            }
            UpdateTreeView(treeView);
        }
        public void Stat_Command(string[] Inputs, RichTextBox commandList, System.Windows.Forms.TreeView treeView)
        {
            if (features.CheckLength(Inputs, 2, 3, commandList))
            {
                fs.Stat(rchCommandList, Inputs);
            }
            UpdateTreeView(treeView);
        }
        public void Pwd_Command(string[] Inputs, System.Windows.Forms.TreeView treeView)
        {
            if (features.CheckLength(Inputs, 1, 1, rchCommandList))
            {
                fs.Pwd(fs.CurrentDirectory, rchCommandList);
            }
            UpdateTreeView(treeView);

        }

        public static void BuildTreeView(Directory directory, TreeNodeCollection nodes)
        {
            // ایجاد نود اصلی برای دایرکتوری فعلی
            TreeNode currentNode = new TreeNode("📁 " + directory.Name)
            {
                Tag = directory, // برای دسترسی بعدی به شیء اصلی
                ImageKey = "📁", // اگر آیکون داری
                SelectedImageKey = "📂",
                ForeColor = Color.Blue
            };
            //برای نمایش ندادن موراد هیدن
            if (!currentNode.Text.StartsWith("📁 ."))
            {
                nodes.Add(currentNode);
            }
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
                    File file = (File)child;
                    TreeNode fileNode;
                    //لینک به بقیه موارد نیست
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
                    //برای نمایش ندادن موراد هیدن
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
        private void UpdateTreeView(System.Windows.Forms.TreeView treeview)
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
