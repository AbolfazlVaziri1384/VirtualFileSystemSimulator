using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VirtualFileSystemSimulatorWinForm
{
    public class Features
    {
        public void AddToCommandList(string input, RichTextBox rchCommandList, bool isCommand = true)
        {
            if (rchCommandList != null && !rchCommandList.IsDisposed && isCommand)
            {
                rchCommandList.AppendText("> " + input + "\n");
                rchCommandList.ScrollToCaret();
            }
            if (rchCommandList != null && !rchCommandList.IsDisposed && !isCommand)
            {
                rchCommandList.AppendText(input + "\n");
                rchCommandList.ScrollToCaret();
            }
        }

        // Automatic Command Complete 
        public void AutoCompleteCommand(TextBox txtCommandLine)
        {
            DateTime dateTime = DateTime.Now;
            var CommandList = new List<string> 
            { "mkdir",
              "touch",
             $"touch -t {dateTime:yyyy-MM-dd HH:mm}",
              "ls",
              "cd",
              "pwd",
              "rm",
              "usertype",
              "changeusertype",
              "tree",
              "ln",
              "stat",
              "echo",
              "cat",
              "mv",
              "cp",
              "load",
              "addgroup",
              "rmgroup",
              "systemfile",
              "find",
              "chmod",
              "revert",
              "commit",
              "open",
              "close",
            };
            foreach (string s in CommandList)
            {
                if (s.Contains(txtCommandLine.Text))
                {
                    txtCommandLine.Text = s;
                    break;
                }
            }
        }

        // For Check Input Length
        public bool CheckLength(string[] input, int Down, int Up, RichTextBox rchCommandList)
        {
            try
            {
                if (input.Length < Down || input.Length > Up)
                {
                    if (input.Length < Down)
                    {
                        AddToCommandList($"Syntax Error: Too few arguments.", rchCommandList, false);
                    }
                    else if (input.Length > Up)
                    {
                        AddToCommandList($"Syntax Error: Too many arguments.", rchCommandList, false);
                    }
                    else
                    {
                        AddToCommandList($"Syntax Error: Invalid number of arguments.", rchCommandList, false);
                    }
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                AddToCommandList($"Error validating command syntax: {ex.Message}. Please try again with proper format.", rchCommandList, false);
                return false;
            }
        }
    }
}
