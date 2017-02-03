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
            this.panel2.SuspendLayout();
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
            this.btnClose.Location = new System.Drawing.Point(448, 432);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(91, 27);
            this.btnClose.TabIndex = 1;
            this.btnClose.Text = "Close";
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
            this.rbHiRes.Location = new System.Drawing.Point(3, 3);
            this.rbHiRes.Name = "rbHiRes";
            this.rbHiRes.Size = new System.Drawing.Size(89, 17);
            this.rbHiRes.TabIndex = 6;
            this.rbHiRes.TabStop = true;
            this.rbHiRes.Text = "Full resolution";
            this.rbHiRes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbHiRes.UseVisualStyleBackColor = true;
            // 
            // rbLowRes
            // 
            this.rbLowRes.AutoSize = true;
            this.rbLowRes.Location = new System.Drawing.Point(98, 3);
            this.rbLowRes.Name = "rbLowRes";
            this.rbLowRes.Size = new System.Drawing.Size(93, 17);
            this.rbLowRes.TabIndex = 7;
            this.rbLowRes.Text = "Low resolution";
            this.rbLowRes.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.rbLowRes.UseVisualStyleBackColor = true;
            this.rbLowRes.CheckedChanged += new System.EventHandler(this.rbLowRes_CheckedChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.rbHiRes);
            this.panel2.Controls.Add(this.rbLowRes);
            this.panel2.Location = new System.Drawing.Point(172, 256);
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
            this.btnHelp.Location = new System.Drawing.Point(358, 432);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(84, 27);
            this.btnHelp.TabIndex = 11;
            this.btnHelp.Text = "Help";
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
            this.btnCal.TabIndex = 13;
            this.btnCal.Text = "Calibrate";
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
            this.label3.Location = new System.Drawing.Point(13, 423);
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
            this.label5.Size = new System.Drawing.Size(165, 16);
            this.label5.TabIndex = 20;
            this.label5.Text = "Plunger Sensor Viewer";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 62);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(89, 13);
            this.label6.TabIndex = 21;
            this.label6.Text = "Sensor snapshot:";
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
            this.btnSave.TabIndex = 22;
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
            // PlungerSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(552, 466);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblCal);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCal);
            this.Controls.Add(this.txtInfo3);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.txtInfo2);
            this.Controls.Add(this.txtInfo);
            this.Controls.Add(this.btnClose);
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
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form3_FormClosed);
            this.Load += new System.EventHandler(this.Form3_Load);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.PlungerSetup_KeyPress);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
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
    }
}