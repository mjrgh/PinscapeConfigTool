namespace PinscapeConfigTool
{
    partial class ExceptionMessageBox
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
            this.btnClose = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.lblGrayBar = new System.Windows.Forms.Label();
            this.btnDetails = new System.Windows.Forms.LinkLabel();
            this.txtDetails = new System.Windows.Forms.TextBox();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.AutoSize = true;
            this.btnClose.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(604, 102);
            this.btnClose.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.btnClose.MinimumSize = new System.Drawing.Size(92, 0);
            this.btnClose.Name = "btnClose";
            this.btnClose.Padding = new System.Windows.Forms.Padding(1);
            this.btnClose.Size = new System.Drawing.Size(92, 25);
            this.btnClose.TabIndex = 5;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // label1
            // 
            this.label1.Image = global::PinscapeConfigTool.Properties.Resources.warningIcon1;
            this.label1.Location = new System.Drawing.Point(11, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 38);
            this.label1.TabIndex = 4;
            // 
            // lblGrayBar
            // 
            this.lblGrayBar.BackColor = System.Drawing.SystemColors.Control;
            this.lblGrayBar.Location = new System.Drawing.Point(-6, 88);
            this.lblGrayBar.Name = "lblGrayBar";
            this.lblGrayBar.Size = new System.Drawing.Size(727, 51);
            this.lblGrayBar.TabIndex = 8;
            // 
            // btnDetails
            // 
            this.btnDetails.AutoSize = true;
            this.btnDetails.Location = new System.Drawing.Point(54, 63);
            this.btnDetails.Name = "btnDetails";
            this.btnDetails.Size = new System.Drawing.Size(159, 13);
            this.btnDetails.TabIndex = 9;
            this.btnDetails.TabStop = true;
            this.btnDetails.Text = "Afficher les détails techniques ...";
            this.btnDetails.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.btnDetails_LinkClicked);
            // 
            // txtDetails
            // 
            this.txtDetails.BackColor = System.Drawing.SystemColors.Window;
            this.txtDetails.Location = new System.Drawing.Point(57, 84);
            this.txtDetails.Multiline = true;
            this.txtDetails.Name = "txtDetails";
            this.txtDetails.ReadOnly = true;
            this.txtDetails.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtDetails.Size = new System.Drawing.Size(639, 119);
            this.txtDetails.TabIndex = 10;
            this.txtDetails.Visible = false;
            this.txtDetails.WordWrap = false;
            // 
            // txtMessage
            // 
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtMessage.Location = new System.Drawing.Point(57, 16);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(639, 38);
            this.txtMessage.TabIndex = 11;
            // 
            // ExceptionMessageBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(712, 225);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.txtDetails);
            this.Controls.Add(this.btnDetails);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblGrayBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExceptionMessageBox";
            this.Text = "Error";
            this.Load += new System.EventHandler(this.ExceptionMessageBox_Load);
            this.Shown += new System.EventHandler(this.ExceptionMessageBox_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblGrayBar;
        private System.Windows.Forms.LinkLabel btnDetails;
        private System.Windows.Forms.TextBox txtDetails;
        private System.Windows.Forms.TextBox txtMessage;
    }
}