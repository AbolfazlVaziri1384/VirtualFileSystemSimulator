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
                rchCommandList.Text += "> " + input + "\n";
                rchCommandList.ScrollToCaret();
            }
            if (rchCommandList != null && !rchCommandList.IsDisposed && !isCommand)
            {
                rchCommandList.Text += input + "\n";
                rchCommandList.ScrollToCaret();
            }
        }
        public void AutoCompleteCommand(TextBox txtCommandLine)
        {
            ArrayList CommandList = new ArrayList();
            CommandList.Add("mkdir");
            CommandList.Add("mkdir -p");
            foreach (string s in CommandList)
            {
                if (s.Contains(txtCommandLine.Text))
                {
                    txtCommandLine.Text = s;
                    break;
                }
            }
        }
    }
}
