namespace PinscapeConfigTool
{
    partial class PlungerSetup
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PlungerSetup));
            this.exposure = new System.Windows.Forms.Label();
            this.btnClose = new System.Windows.Forms.Button();
            this.txtInfo = new System.Windows.Forms.Label();
            this.txtInfo2 = new System.Windows.Forms.Label();
            this.rbHiRes = new System.Windows.Forms.RadioButton();
            this.rbLowRes = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnHelp = new System.Windows.Forms.Button();
            this.txtInfo3 = new System.Windows.Forms.Label();
            this.btnCal = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblCal = new System.Windows.Forms.Label();
            this.timerCal = new System.Windows.Forms.Timer(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.btnSave = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.btnStopSave = new System.Windows.Forms.Button();
            this.timerRefresh = new System.Windows.Forms.Timer(this.components);
            this.ckEnhance = new System.Windows.Forms.CheckBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtJitterWindow = new System.Windows.Forms.NumericUpDown();
            this.lblJitterUnits = new System.Windows.Forms.Label();
            this.pnlJitter = new System.Windows.Forms.Panel();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.pnlBarCode = new System.Windows.Forms.Panel();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.txtBarCodeOffset = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.pnlReverse = new System.Windows.Forms.Panel();
            this.ckReverseOrientation = new System.Windows.Forms.CheckBox();
            this.label14 = new System.Windows.Forms.Label();
            this.cbZoom = new System.Windows.Forms.ComboBox();
            this.lblZoom = new System.Windows.Forms.Label();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtJitterWindow)).BeginInit();
            this.pnlJitter.SuspendLayout();
            this.pnlBottom.SuspendLayout();
            this.pnlBarCode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtBarCodeOffset)).BeginInit();
            this.pnlReverse.SuspendLayout();
            this.SuspendLayout();
            // 
            // exposure
            // 
            this.exposure.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.exposure.Location = new System.Drawing.Point(16, 78);
            this.exposure.Name = "exposure";
            this.exposure.Size = new System.Drawing.Size(523, 77);
            this.exposure.TabIndex = 0;
            this.exposure.Paint += new System.Windows.Forms.PaintEventHandler(this.exposure_Paint);
            this.exposure.MouseCaptureChanged += new System.EventHandler(this.exposure_MouseCaptureChanged);
            this.exposure.MouseDown += new System.Windows.Forms.MouseEventHandler(this.exposure_MouseDown);
            this.exposure.MouseMove += new System.Windows.Forms.MouseEventHandler(this.exposure_MouseMove);
            this.exposure.MouseUp += new System.Windows.Forms.MouseEventHandler(this.exposure_MouseUp);
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
            this.btnClose.Location = new System.Drawing.Point(448, 13);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(91, 28);
            this.btnClose.TabIndex = 4;
            this.btnClose.Text = "Fermer";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // txtInfo
            // 
            this.txtInfo.Location = new System.Drawing.Point(16, 164);
            this.txtInfo.Name = "txtInfo";
            this.txtInfo.Size = new System.Drawing.Size(523, 23);
            this.txtInfo.TabIndex = 2;
            // 
            // txtInfo2
            // 
            this.txtInfo2.Location = new System.Drawing.Point(16, 187);
            this.txtInfo2.Name = "txtInfo2";
            this.txtInfo2.Size = new System.Drawing.Size(523, 23);
            this.txtInfo2.TabIndex = 3;
            // 
            // rbHiRes
            // 
            this.rbHiRes.AutoSize = true;
            this.rbHiRes.Checked = true;
            this.rbHiRes.Location = new System.Drawing.Point(269, 263);
            this.rbHiRes.Name = "rbHiRes";
            this.rbHiRes.Size = new System.Drawing.Size(102, 17);
            this.rbHiRes.TabIndex = 0;
            this.rbHiRes.TabStop = true;
            this.rbHiRes.Text = "Pleine résolution";
            this.rbHiRes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbHiRes.UseVisualStyleBackColor = true;
            // 
            // rbLowRes
            // 
            this.rbLowRes.AutoSize = true;
            this.rbLowRes.Location = new System.Drawing.Point(104, 3);
            this.rbLowRes.Name = "rbLowRes";
            this.rbLowRes.Size = new System.Drawing.Size(102, 17);
            this.rbLowRes.TabIndex = 1;
            this.rbLowRes.Text = "Basse résolution";
            this.rbLowRes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbLowRes.UseVisualStyleBackColor = true;
            this.rbLowRes.CheckedChanged += new System.EventHandler(this.rbLowRes_CheckedChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.rbLowRes);
            this.panel2.Location = new System.Drawing.Point(279, 260);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(209, 23);
            this.panel2.TabIndex = 9;
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
            this.btnHelp.Location = new System.Drawing.Point(360, 13);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(84, 28);
            this.btnHelp.TabIndex = 3;
            this.btnHelp.Text = "Aide";
            this.btnHelp.UseVisualStyleBackColor = false;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // txtInfo3
            // 
            this.txtInfo3.Location = new System.Drawing.Point(16, 210);
            this.txtInfo3.Name = "txtInfo3";
            this.txtInfo3.Size = new System.Drawing.Size(523, 23);
            this.txtInfo3.TabIndex = 12;
            // 
            // btnCal
            // 
            this.btnCal.AutoSize = true;
            this.btnCal.BackColor = System.Drawing.SystemColors.Window;
            this.btnCal.FlatAppearance.BorderColor = System.Drawing.Color.DimGray;
            this.btnCal.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnCal.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Yellow;
            this.btnCal.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCal.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCal.ForeColor = System.Drawing.Color.Purple;
            this.btnCal.Location = new System.Drawing.Point(12, 345);
            this.btnCal.Name = "btnCal";
            this.btnCal.Size = new System.Drawing.Size(87, 27);
            this.btnCal.TabIndex = 2;
            this.btnCal.Text = "Étalonner";
            this.btnCal.UseVisualStyleBackColor = false;
            this.btnCal.Click += new System.EventHandler(this.btnCal_Click);
            // 
            // label2
            // 
            this.label2.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label2.Location = new System.Drawing.Point(13, 292);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(526, 2);
            this.label2.TabIndex = 15;
            // 
            // label3
            // 
            this.label3.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label3.Location = new System.Drawing.Point(13, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(526, 2);
            this.label3.TabIndex = 16;
            // 
            // lblCal
            // 
            this.lblCal.Location = new System.Drawing.Point(116, 294);
            this.lblCal.Name = "lblCal";
            this.lblCal.Size = new System.Drawing.Size(423, 129);
            this.lblCal.TabIndex = 17;
            this.lblCal.Text = "label4";
            this.lblCal.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // timerCal
            // 
            this.timerCal.Enabled = true;
            this.timerCal.Tick += new System.EventHandler(this.timerCal_Tick);
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
            this.label4.Image = global::PinscapeConfigTool.Resources.h1plunger;
            this.label4.Location = new System.Drawing.Point(454, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(100, 23);
            this.label4.TabIndex = 19;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(13, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(239, 16);
            this.label5.TabIndex = 20;
            this.label5.Text = "Visionneuse de capteur de piston";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 62);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(114, 13);
            this.label6.TabIndex = 21;
            this.label6.Text = "Instantané du capteur:";
            // 
            // btnSave
            // 
            this.btnSave.BackColor = System.Drawing.Color.Transparent;
            this.btnSave.FlatAppearance.BorderSize = 0;
            this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
            this.btnSave.Location = new System.Drawing.Point(508, 252);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(31, 31);
            this.btnSave.TabIndex = 1;
            this.toolTip1.SetToolTip(this.btnSave, "Save pixel data to a disk file for analysis");
            this.btnSave.UseVisualStyleBackColor = false;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnStopSave
            // 
            this.btnStopSave.BackColor = System.Drawing.Color.Transparent;
            this.btnStopSave.FlatAppearance.BorderSize = 0;
            this.btnStopSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnStopSave.Image = ((System.Drawing.Image)(resources.GetObject("btnStopSave.Image")));
            this.btnStopSave.Location = new System.Drawing.Point(508, 252);
            this.btnStopSave.Name = "btnStopSave";
            this.btnStopSave.Size = new System.Drawing.Size(31, 31);
            this.btnStopSave.TabIndex = 23;
            this.toolTip1.SetToolTip(this.btnStopSave, "Stop saving pixel data");
            this.btnStopSave.UseVisualStyleBackColor = false;
            this.btnStopSave.Visible = false;
            this.btnStopSave.Click += new System.EventHandler(this.btnStopSave_Click);
            // 
            // timerRefresh
            // 
            this.timerRefresh.Enabled = true;
            this.timerRefresh.Interval = 20;
            this.timerRefresh.Tick += new System.EventHandler(this.timerRefresh_Tick);
            // 
            // ckEnhance
            // 
            this.ckEnhance.AutoSize = true;
            this.ckEnhance.Location = new System.Drawing.Point(16, 264);
            this.ckEnhance.Name = "ckEnhance";
            this.ckEnhance.Size = new System.Drawing.Size(127, 17);
            this.ckEnhance.TabIndex = 0;
            this.ckEnhance.Text = "Améliorer le contraste";
            this.ckEnhance.UseVisualStyleBackColor = true;
            this.ckEnhance.CheckedChanged += new System.EventHandler(this.ckEnhance_CheckedChanged);
            // 
            // label7
            // 
            this.label7.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label7.Location = new System.Drawing.Point(13, 1);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(526, 2);
            this.label7.TabIndex = 24;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(16, 14);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(76, 13);
            this.label8.TabIndex = 25;
            this.label8.Text = "Filtre de gigue:";
            // 
            // txtJitterWindow
            // 
            this.txtJitterWindow.Location = new System.Drawing.Point(98, 12);
            this.txtJitterWindow.Name = "txtJitterWindow";
            this.txtJitterWindow.Size = new System.Drawing.Size(72, 20);
            this.txtJitterWindow.TabIndex = 26;
            this.txtJitterWindow.ValueChanged += new System.EventHandler(this.txtJitterWindow_ValueChanged);
            // 
            // lblJitterUnits
            // 
            this.lblJitterUnits.AutoSize = true;
            this.lblJitterUnits.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblJitterUnits.Location = new System.Drawing.Point(191, 14);
            this.lblJitterUnits.Name = "lblJitterUnits";
            this.lblJitterUnits.Size = new System.Drawing.Size(326, 13);
            this.lblJitterUnits.TabIndex = 27;
            this.lblJitterUnits.Text = "(Unités de périphérique natives, 0 à 4095; utilisez 0 pour désactiver)";
            // 
            // pnlJitter
            // 
            this.pnlJitter.Controls.Add(this.label8);
            this.pnlJitter.Controls.Add(this.lblJitterUnits);
            this.pnlJitter.Controls.Add(this.txtJitterWindow);
            this.pnlJitter.Controls.Add(this.label3);
            this.pnlJitter.Location = new System.Drawing.Point(0, 470);
            this.pnlJitter.Name = "pnlJitter";
            this.pnlJitter.Size = new System.Drawing.Size(556, 44);
            this.pnlJitter.TabIndex = 28;
            // 
            // pnlBottom
            // 
            this.pnlBottom.Controls.Add(this.btnHelp);
            this.pnlBottom.Controls.Add(this.btnClose);
            this.pnlBottom.Controls.Add(this.label7);
            this.pnlBottom.Location = new System.Drawing.Point(0, 558);
            this.pnlBottom.Name = "pnlBottom";
            this.pnlBottom.Size = new System.Drawing.Size(553, 55);
            this.pnlBottom.TabIndex = 29;
            // 
            // pnlBarCode
            // 
            this.pnlBarCode.Controls.Add(this.label9);
            this.pnlBarCode.Controls.Add(this.label10);
            this.pnlBarCode.Controls.Add(this.txtBarCodeOffset);
            this.pnlBarCode.Controls.Add(this.label11);
            this.pnlBarCode.Location = new System.Drawing.Point(0, 514);
            this.pnlBarCode.Name = "pnlBarCode";
            this.pnlBarCode.Size = new System.Drawing.Size(556, 44);
            this.pnlBarCode.TabIndex = 29;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(16, 14);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(139, 13);
            this.label9.TabIndex = 25;
            this.label9.Text = "Décalage du code à barres:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(263, 14);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(276, 13);
            this.label10.TabIndex = 27;
            this.label10.Text = "Emplacement en pixels du bord gauche du code à barres";
            // 
            // txtBarCodeOffset
            // 
            this.txtBarCodeOffset.Location = new System.Drawing.Point(161, 12);
            this.txtBarCodeOffset.Name = "txtBarCodeOffset";
            this.txtBarCodeOffset.Size = new System.Drawing.Size(72, 20);
            this.txtBarCodeOffset.TabIndex = 26;
            this.txtBarCodeOffset.ValueChanged += new System.EventHandler(this.txtBarCodeOffset_ValueChanged);
            // 
            // label11
            // 
            this.label11.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label11.Location = new System.Drawing.Point(13, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(526, 2);
            this.label11.TabIndex = 16;
            // 
            // pnlReverse
            // 
            this.pnlReverse.Controls.Add(this.ckReverseOrientation);
            this.pnlReverse.Controls.Add(this.label14);
            this.pnlReverse.Location = new System.Drawing.Point(0, 426);
            this.pnlReverse.Name = "pnlReverse";
            this.pnlReverse.Size = new System.Drawing.Size(556, 44);
            this.pnlReverse.TabIndex = 29;
            // 
            // ckReverseOrientation
            // 
            this.ckReverseOrientation.AutoSize = true;
            this.ckReverseOrientation.Location = new System.Drawing.Point(19, 14);
            this.ckReverseOrientation.Name = "ckReverseOrientation";
            this.ckReverseOrientation.Size = new System.Drawing.Size(308, 17);
            this.ckReverseOrientation.TabIndex = 17;
            this.ckReverseOrientation.Text = "Orientation inversée (agit comme si le capteur était retourné)";
            this.ckReverseOrientation.UseVisualStyleBackColor = true;
            this.ckReverseOrientation.CheckedChanged += new System.EventHandler(this.ckReverseOrientation_CheckedChanged);
            // 
            // label14
            // 
            this.label14.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label14.Location = new System.Drawing.Point(13, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(526, 2);
            this.label14.TabIndex = 16;
            // 
            // cbZoom
            // 
            this.cbZoom.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cbZoom.FormattingEnabled = true;
            this.cbZoom.Items.AddRange(new object[] {
            "100%",
            "200%",
            "400%",
            "800%",
            "1600%",
            "3200%"});
            this.cbZoom.Location = new System.Drawing.Point(183, 260);
            this.cbZoom.Name = "cbZoom";
            this.cbZoom.Size = new System.Drawing.Size(64, 21);
            this.cbZoom.TabIndex = 30;
            this.cbZoom.SelectedIndexChanged += new System.EventHandler(this.cbZoom_SelectedIndexChanged);
            // 
            // lblZoom
            // 
            this.lblZoom.AutoSize = true;
            this.lblZoom.Location = new System.Drawing.Point(142, 265);
            this.lblZoom.Name = "lblZoom";
            this.lblZoom.Size = new System.Drawing.Size(37, 13);
            this.lblZoom.TabIndex = 31;
            this.lblZoom.Text = "Zoom:";
            // 
            // PlungerSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(552, 611);
            this.Controls.Add(this.rbHiRes);
            this.Controls.Add(this.lblZoom);
            this.Controls.Add(this.cbZoom);
            this.Controls.Add(this.pnlReverse);
            this.Controls.Add(this.pnlBarCode);
            this.Controls.Add(this.pnlBottom);
            this.Controls.Add(this.pnlJitter);
            this.Controls.Add(this.ckEnhance);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblCal);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCal);
            this.Controls.Add(this.txtInfo3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.txtInfo2);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.exposure);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnStopSave);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PlungerSetup";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Pinscape Setup";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.PlungerSetup_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.PlungerSetup_FormClosed);
            this.Load += new System.EventHandler(this.PlungerSetup_Load);
            this.Shown += new System.EventHandler(this.PlungerSetup_Shown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PlungerSetup_KeyPress);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtJitterWindow)).EndInit();
            this.pnlJitter.ResumeLayout(false);
            this.pnlJitter.PerformLayout();
            this.pnlBottom.ResumeLayout(false);
            this.pnlBottom.PerformLayout();
            this.pnlBarCode.ResumeLayout(false);
            this.pnlBarCode.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtBarCodeOffset)).EndInit();
            this.pnlReverse.ResumeLayout(false);
            this.pnlReverse.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label exposure;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label txtInfo;
        private System.Windows.Forms.Label txtInfo2;
        private System.Windows.Forms.RadioButton rbHiRes;
        private System.Windows.Forms.RadioButton rbLowRes;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Label txtInfo3;
        private System.Windows.Forms.Button btnCal;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblCal;
        private System.Windows.Forms.Timer timerCal;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Button btnStopSave;
        private System.Windows.Forms.Timer timerRefresh;
        private System.Windows.Forms.CheckBox ckEnhance;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown txtJitterWindow;
        private System.Windows.Forms.Label lblJitterUnits;
        private System.Windows.Forms.Panel pnlJitter;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Panel pnlBarCode;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown txtBarCodeOffset;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Panel pnlReverse;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.CheckBox ckReverseOrientation;
        private System.Windows.Forms.ComboBox cbZoom;
        private System.Windows.Forms.Label lblZoom;
    }
}