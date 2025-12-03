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

        //باید درست و بهینه شود---------------------
        public void AutoCompleteCommand(TextBox txtCommandLine)
        {
            DateTime dateTime = DateTime.Now;
            ArrayList CommandList = new ArrayList();
            CommandList.Add("mkdir");
            CommandList.Add("mkdir -p");
            CommandList.Add("touch");
            CommandList.Add($"touch -t {dateTime:yyyy-MM-dd HH:mm}");
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
        public bool CheckLength(string[] input , int Down , int Up , RichTextBox rchCommandList)
        {
            if (input.Length < Down || input.Length > Up)
            {
                AddToCommandList("Syntax Error", rchCommandList, false);
                return false;
            }
            return true;
        }
    }
}
