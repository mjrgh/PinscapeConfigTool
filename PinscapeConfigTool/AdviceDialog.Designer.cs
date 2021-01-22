namespace PinscapeConfigTool
{
    partial class AdviceDialog
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
            this.btnOK = new System.Windows.Forms.Button();
            this.ckHide = new System.Windows.Forms.CheckBox();
            this.txtMessage = new System.Windows.Forms.Label();
            this.lblGrayBar = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnOK
            // 
            this.btnOK.AutoSize = true;
            this.btnOK.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnOK.Location = new System.Drawing.Point(520, 79);
            this.btnOK.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnOK.MinimumSize = new System.Drawing.Size(90, 0);
            this.btnOK.Name = "btnOK";
            this.btnOK.Padding = new System.Windows.Forms.Padding(1);
            this.btnOK.Size = new System.Drawing.Size(90, 27);
            this.btnOK.TabIndex = 0;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // ckHide
            // 
            this.ckHide.AutoSize = true;
            this.ckHide.BackColor = System.Drawing.SystemColors.Control;
            this.ckHide.Location = new System.Drawing.Point(167, 84);
            this.ckHide.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.ckHide.Name = "ckHide";
            this.ckHide.Size = new System.Drawing.Size(338, 19);
            this.ckHide.TabIndex = 1;
            this.ckHide.Text = "Je l'ai, pas besoin de me montrer ce message à nouveau";
            this.ckHide.UseVisualStyleBackColor = false;
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(55, 14);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(558, 39);
            this.txtMessage.TabIndex = 2;
            // 
            // lblGrayBar
            // 
            this.lblGrayBar.BackColor = System.Drawing.SystemColors.Control;
            this.lblGrayBar.Location = new System.Drawing.Point(-8, 67);
            this.lblGrayBar.Name = "lblGrayBar";
            this.lblGrayBar.Size = new System.Drawing.Size(646, 69);
            this.lblGrayBar.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.Image = global::PinscapeConfigTool.Properties.Resources.infoIcon;
            this.label2.Location = new System.Drawing.Point(12, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 37);
            this.label2.TabIndex = 4;
            // 
            // AdviceDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(625, 116);
            this.ControlBox = false;
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.ckHide);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblGrayBar);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "AdviceDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pinscape Setup";
            this.Shown += new System.EventHandler(this.AdviceDialog_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.CheckBox ckHide;
        private System.Windows.Forms.Label txtMessage;
        private System.Windows.Forms.Label lblGrayBar;
        private System.Windows.Forms.Label label2;
    }
}
