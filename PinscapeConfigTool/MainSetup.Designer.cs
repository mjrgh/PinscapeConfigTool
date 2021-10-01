﻿namespace PinscapeConfigTool
{
    partial class MainSetup
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainSetup));
			this.webBrowser1 = new System.Windows.Forms.WebBrowser();
			this.bgworkerDownload = new System.ComponentModel.BackgroundWorker();
			this.broadcastTimer = new System.Windows.Forms.Timer(this.components);
			this.SuspendLayout();
			// 
			// webBrowser1
			// 
			this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webBrowser1.Location = new System.Drawing.Point(0, 0);
			this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
			this.webBrowser1.Name = "webBrowser1";
			this.webBrowser1.Size = new System.Drawing.Size(1075, 700);
			this.webBrowser1.TabIndex = 0;
			this.webBrowser1.DocumentCompleted += new System.Windows.Forms.WebBrowserDocumentCompletedEventHandler(this.webBrowser1_DocumentCompleted);
			this.webBrowser1.Navigating += new System.Windows.Forms.WebBrowserNavigatingEventHandler(this.webBrowser1_Navigating);
			this.webBrowser1.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.webBrowser1_PreviewKeyDown);
			// 
			// bgworkerDownload
			// 
			this.bgworkerDownload.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgworkerDownload_DoWork);
			this.bgworkerDownload.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgworkerDownload_ProgressChanged);
			// 
			// broadcastTimer
			// 
			this.broadcastTimer.Enabled = true;
			this.broadcastTimer.Interval = 1000;
			this.broadcastTimer.Tick += new System.EventHandler(this.broadcastTimer_Tick);
			// 
			// MainSetup
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.ClientSize = new System.Drawing.Size(1075, 700);
			this.Controls.Add(this.webBrowser1);
			this.HelpButton = true;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "MainSetup";
			this.Text = "Pinscape Setup";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainSetup_FormClosing);
			this.Load += new System.EventHandler(this.MainSetup_Load);
			this.Shown += new System.EventHandler(this.MainSetup_Shown);
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.ComponentModel.BackgroundWorker bgworkerDownload;
        private System.Windows.Forms.Timer broadcastTimer;
    }
}