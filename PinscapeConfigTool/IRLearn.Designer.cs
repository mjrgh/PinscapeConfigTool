namespace PinscapeConfigTool
{
    partial class IRLearn
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
                mutex.Dispose();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IRLearn));
            this.btnClose = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.timerRefresh = new System.Windows.Forms.Timer(this.components);
            this.panelStart = new System.Windows.Forms.Panel();
            this.btnLearn = new System.Windows.Forms.Button();
            this.lblPrep = new System.Windows.Forms.Label();
            this.panelLearn = new System.Windows.Forms.Panel();
            this.lnkRawData = new System.Windows.Forms.LinkLabel();
            this.lnkRedo = new System.Windows.Forms.LinkLabel();
            this.lblRawData = new System.Windows.Forms.Label();
            this.lblDetails = new System.Windows.Forms.Label();
            this.lblStatus = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.panelStart.SuspendLayout();
            this.panelLearn.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.AutoSize = true;
            this.btnClose.BackColor = System.Drawing.SystemColors.Window;
            this.btnClose.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            this.btnClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Yellow;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.ForeColor = System.Drawing.Color.Purple;
            this.btnClose.Location = new System.Drawing.Point(448, 281);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(91, 27);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Annuler";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.AutoSize = true;
            this.btnHelp.BackColor = System.Drawing.SystemColors.Window;
            this.btnHelp.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            this.btnHelp.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnHelp.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Yellow;
            this.btnHelp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHelp.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHelp.ForeColor = System.Drawing.Color.Purple;
            this.btnHelp.Location = new System.Drawing.Point(261, 281);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(84, 27);
            this.btnHelp.TabIndex = 11;
            this.btnHelp.Text = "Aide";
            this.btnHelp.UseVisualStyleBackColor = false;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label3.Location = new System.Drawing.Point(13, 272);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(526, 0);
            this.label3.TabIndex = 16;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label1.Location = new System.Drawing.Point(0, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(554, 2);
            this.label1.TabIndex = 18;
            // 
            // label4
            // 
            this.label4.Image = global::PinscapeConfigTool.Resources.h1IR;
            this.label4.Location = new System.Drawing.Point(465, 4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 33);
            this.label4.TabIndex = 19;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(13, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(273, 16);
            this.label5.TabIndex = 20;
            this.label5.Text = "Apprendre la commande à distance IR";
            // 
            // timerRefresh
            // 
            this.timerRefresh.Enabled = true;
            this.timerRefresh.Tick += new System.EventHandler(this.timerRefresh_Tick);
            // 
            // panelStart
            // 
            this.panelStart.Controls.Add(this.btnLearn);
            this.panelStart.Controls.Add(this.lblPrep);
            this.panelStart.Location = new System.Drawing.Point(3, 47);
            this.panelStart.Name = "panelStart";
            this.panelStart.Size = new System.Drawing.Size(546, 211);
            this.panelStart.TabIndex = 21;
            // 
            // btnLearn
            // 
            this.btnLearn.AutoSize = true;
            this.btnLearn.BackColor = System.Drawing.SystemColors.Window;
            this.btnLearn.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            this.btnLearn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnLearn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Yellow;
            this.btnLearn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLearn.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLearn.ForeColor = System.Drawing.Color.Purple;
            this.btnLearn.Location = new System.Drawing.Point(228, 144);
            this.btnLearn.Name = "btnLearn";
            this.btnLearn.Size = new System.Drawing.Size(91, 27);
            this.btnLearn.TabIndex = 22;
            this.btnLearn.Text = "Learn";
            this.btnLearn.UseVisualStyleBackColor = false;
            this.btnLearn.Click += new System.EventHandler(this.btnLearn_Click);
            // 
            // lblPrep
            // 
            this.lblPrep.Location = new System.Drawing.Point(9, 16);
            this.lblPrep.Name = "lblPrep";
            this.lblPrep.Size = new System.Drawing.Size(516, 116);
            this.lblPrep.TabIndex = 0;
            this.lblPrep.Text = resources.GetString("lblPrep.Text");
            // 
            // panelLearn
            // 
            this.panelLearn.Controls.Add(this.lnkRawData);
            this.panelLearn.Controls.Add(this.lnkRedo);
            this.panelLearn.Controls.Add(this.lblRawData);
            this.panelLearn.Controls.Add(this.lblDetails);
            this.panelLearn.Controls.Add(this.lblStatus);
            this.panelLearn.Location = new System.Drawing.Point(3, 47);
            this.panelLearn.Name = "panelLearn";
            this.panelLearn.Size = new System.Drawing.Size(546, 211);
            this.panelLearn.TabIndex = 22;
            this.panelLearn.Visible = false;
            // 
            // lnkRawData
            // 
            this.lnkRawData.AutoSize = true;
            this.lnkRawData.Location = new System.Drawing.Point(242, 126);
            this.lnkRawData.Name = "lnkRawData";
            this.lnkRawData.Size = new System.Drawing.Size(135, 13);
            this.lnkRawData.TabIndex = 4;
            this.lnkRawData.TabStop = true;
            this.lnkRawData.Text = "Afficher les données brutes";
            this.lnkRawData.Visible = false;
            this.lnkRawData.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRawData_LinkClicked);
            // 
            // lnkRedo
            // 
            this.lnkRedo.AutoSize = true;
            this.lnkRedo.Location = new System.Drawing.Point(169, 126);
            this.lnkRedo.Name = "lnkRedo";
            this.lnkRedo.Size = new System.Drawing.Size(57, 13);
            this.lnkRedo.TabIndex = 3;
            this.lnkRedo.TabStop = true;
            this.lnkRedo.Text = "Réessayer";
            this.lnkRedo.Visible = false;
            this.lnkRedo.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRedo_LinkClicked);
            // 
            // lblRawData
            // 
            this.lblRawData.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lblRawData.Location = new System.Drawing.Point(12, 146);
            this.lblRawData.Name = "lblRawData";
            this.lblRawData.Size = new System.Drawing.Size(522, 59);
            this.lblRawData.TabIndex = 2;
            this.lblRawData.Paint += new System.Windows.Forms.PaintEventHandler(this.lblRawData_Paint);
            // 
            // lblDetails
            // 
            this.lblDetails.Location = new System.Drawing.Point(10, 45);
            this.lblDetails.Name = "lblDetails";
            this.lblDetails.Size = new System.Drawing.Size(527, 71);
            this.lblDetails.TabIndex = 1;
            this.lblDetails.Text = "Maintenant, APPUYEZ ET MAINTENEZ la touche de la télécommande que vous voulez que" +
    " Pinscape apprenne. Gardez la télécommande pointée vers le capteur de l\'unité Pi" +
    "nscape.";
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStatus.ForeColor = System.Drawing.Color.Blue;
            this.lblStatus.Location = new System.Drawing.Point(195, 16);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(148, 13);
            this.lblStatus.TabIndex = 0;
            this.lblStatus.Text = "*** APPRENTISSAGE ***";
            // 
            // btnSave
            // 
            this.btnSave.AutoSize = true;
            this.btnSave.BackColor = System.Drawing.SystemColors.Window;
            this.btnSave.Enabled = false;
            this.btnSave.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            this.btnSave.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnSave.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Yellow;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSave.ForeColor = System.Drawing.Color.Purple;
            this.btnSave.Location = new System.Drawing.Point(351, 281);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(92, 27);
            this.btnSave.TabIndex = 23;
            this.btnSave.Text = "Saveugarder";
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // IRLearn
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(552, 316);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.panelLearn);
            this.Controls.Add(this.panelStart);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "IRLearn";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Pinscape Setup";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IRLearn_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.IRLearn_FormClosed);
            this.Load += new System.EventHandler(this.IRLearn_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.IRLearn_KeyPress);
            this.panelStart.ResumeLayout(false);
            this.panelStart.PerformLayout();
            this.panelLearn.ResumeLayout(false);
            this.panelLearn.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Timer timerRefresh;
        private System.Windows.Forms.Panel panelStart;
        private System.Windows.Forms.Button btnLearn;
        private System.Windows.Forms.Label lblPrep;
        private System.Windows.Forms.Panel panelLearn;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblDetails;
        private System.Windows.Forms.Label lblRawData;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.LinkLabel lnkRawData;
        private System.Windows.Forms.LinkLabel lnkRedo;
    }
}