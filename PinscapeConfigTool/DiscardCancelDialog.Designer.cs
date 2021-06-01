namespace PinscapeConfigTool
{
    partial class DiscardCancelDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.txtMessage = new System.Windows.Forms.Label();
            this.btnDiscard = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.lblGrayBar = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Image = global::PinscapeConfigTool.Properties.Resources.warningIcon1;
            this.label1.Location = new System.Drawing.Point(12, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(37, 38);
            this.label1.TabIndex = 0;
            // 
            // txtMessage
            // 
            this.txtMessage.Location = new System.Drawing.Point(55, 15);
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.Size = new System.Drawing.Size(646, 28);
            this.txtMessage.TabIndex = 1;
            // 
            // btnDiscard
            // 
            this.btnDiscard.AutoSize = true;
            this.btnDiscard.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnDiscard.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnDiscard.Location = new System.Drawing.Point(503, 68);
            this.btnDiscard.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.btnDiscard.MinimumSize = new System.Drawing.Size(90, 0);
            this.btnDiscard.Name = "btnDiscard";
            this.btnDiscard.Padding = new System.Windows.Forms.Padding(1);
            this.btnDiscard.Size = new System.Drawing.Size(90, 27);
            this.btnDiscard.TabIndex = 2;
            this.btnDiscard.Text = "&Jeter";
            this.btnDiscard.UseVisualStyleBackColor = true;
            this.btnDiscard.Click += new System.EventHandler(this.btnDiscard_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.AutoSize = true;
            this.btnCancel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(607, 68);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
            this.btnCancel.MinimumSize = new System.Drawing.Size(92, 0);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Padding = new System.Windows.Forms.Padding(1);
            this.btnCancel.Size = new System.Drawing.Size(92, 27);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Annuler";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblGrayBar
            // 
            this.lblGrayBar.BackColor = System.Drawing.SystemColors.Control;
            this.lblGrayBar.Location = new System.Drawing.Point(-7, 58);
            this.lblGrayBar.Name = "lblGrayBar";
            this.lblGrayBar.Size = new System.Drawing.Size(727, 60);
            this.lblGrayBar.TabIndex = 3;
            // 
            // DiscardCancelDialog
            // 
            this.AcceptButton = this.btnDiscard;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(713, 104);
            this.ControlBox = false;
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnDiscard);
            this.Controls.Add(this.txtMessage);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblGrayBar);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "DiscardCancelDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Pinscape Setup";
            this.Shown += new System.EventHandler(this.DiscardCancelDialog_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label txtMessage;
        private System.Windows.Forms.Button btnDiscard;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label lblGrayBar;
    }
}