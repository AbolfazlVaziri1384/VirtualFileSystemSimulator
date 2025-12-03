namespace VirtualFileSystemSimulatorWinForm
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.rchCommandList = new System.Windows.Forms.RichTextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.TreeView = new System.Windows.Forms.TreeView();
            this.txtCommandLine = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtCurrentRoute = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // rchCommandList
            // 
            this.rchCommandList.BackColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.rchCommandList.ForeColor = System.Drawing.SystemColors.Window;
            this.rchCommandList.Location = new System.Drawing.Point(439, 49);
            this.rchCommandList.Margin = new System.Windows.Forms.Padding(4);
            this.rchCommandList.Name = "rchCommandList";
            this.rchCommandList.ReadOnly = true;
            this.rchCommandList.Size = new System.Drawing.Size(548, 416);
            this.rchCommandList.TabIndex = 9;
            this.rchCommandList.Text = "";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 19.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.BlueViolet;
            this.label1.Location = new System.Drawing.Point(439, 471);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 38);
            this.label1.TabIndex = 8;
            this.label1.Text = ">";
            // 
            // TreeView
            // 
            this.TreeView.Location = new System.Drawing.Point(15, 16);
            this.TreeView.Margin = new System.Windows.Forms.Padding(4);
            this.TreeView.Name = "TreeView";
            this.TreeView.Size = new System.Drawing.Size(416, 492);
            this.TreeView.TabIndex = 5;
            // 
            // txtCommandLine
            // 
            this.txtCommandLine.Location = new System.Drawing.Point(475, 476);
            this.txtCommandLine.Margin = new System.Windows.Forms.Padding(4);
            this.txtCommandLine.Name = "txtCommandLine";
            this.txtCommandLine.Size = new System.Drawing.Size(512, 28);
            this.txtCommandLine.TabIndex = 10;
            this.txtCommandLine.TextChanged += new System.EventHandler(this.txtCommandLine_TextChanged);
            this.txtCommandLine.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtCommandLine_KeyDown);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(440, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(133, 22);
            this.label2.TabIndex = 11;
            this.label2.Text = "Current Route :";
            // 
            // txtCurrentRoute
            // 
            this.txtCurrentRoute.Location = new System.Drawing.Point(574, 13);
            this.txtCurrentRoute.Name = "txtCurrentRoute";
            this.txtCurrentRoute.ReadOnly = true;
            this.txtCurrentRoute.Size = new System.Drawing.Size(413, 28);
            this.txtCurrentRoute.TabIndex = 12;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 22F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(1000, 520);
            this.Controls.Add(this.txtCurrentRoute);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtCommandLine);
            this.Controls.Add(this.rchCommandList);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TreeView);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 10.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "Form1";
            this.Text = "Virtual File System Simulator";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RichTextBox rchCommandList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TreeView TreeView;
        private System.Windows.Forms.TextBox txtCommandLine;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtCurrentRoute;
    }
}

