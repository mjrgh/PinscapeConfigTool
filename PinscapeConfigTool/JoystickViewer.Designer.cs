namespace PinscapeConfigTool
{
    partial class JoystickViewer
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JoystickViewer));
			this.lblTitle = new System.Windows.Forms.Label();
			this.lblTitleBar = new System.Windows.Forms.Label();
			this.lblAccel = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.lblPlunger = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.lblXY = new System.Windows.Forms.Label();
			this.lblZ = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.lblTextBoxBar = new System.Windows.Forms.Label();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.timer1 = new System.Windows.Forms.Timer(this.components);
			this.btnSave = new System.Windows.Forms.Button();
			this.label1 = new System.Windows.Forms.Label();
			this.lblIcon = new System.Windows.Forms.Label();
			this.btnLogging = new System.Windows.Forms.Button();
			this.lblBottomMarker = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.timer2 = new System.Windows.Forms.Timer(this.components);
			this.jsKeyState = new System.Windows.Forms.PictureBox();
			this.label6 = new System.Windows.Forms.Label();
			this.btnHelp = new System.Windows.Forms.Button();
			this.btnClose = new System.Windows.Forms.Button();
			this.linkCenter = new System.Windows.Forms.LinkLabel();
			this.lblVelocityPlot = new System.Windows.Forms.Label();
			this.lblVXVY = new System.Windows.Forms.Label();
			this.lblAccelVelocityTitle = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.velocityScale = new System.Windows.Forms.TextBox();
			this.lblVelocityScalingError = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.jsKeyState)).BeginInit();
			this.SuspendLayout();
			// 
			// lblTitle
			// 
			this.lblTitle.AutoSize = true;
			this.lblTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.lblTitle.Location = new System.Drawing.Point(12, 9);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(115, 16);
			this.lblTitle.TabIndex = 22;
			this.lblTitle.Text = "Joystick Viewer";
			// 
			// lblTitleBar
			// 
			this.lblTitleBar.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.lblTitleBar.Location = new System.Drawing.Point(-1, 42);
			this.lblTitleBar.Name = "lblTitleBar";
			this.lblTitleBar.Size = new System.Drawing.Size(937, 1);
			this.lblTitleBar.TabIndex = 21;
			// 
			// lblAccel
			// 
			this.lblAccel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblAccel.Location = new System.Drawing.Point(12, 61);
			this.lblAccel.Name = "lblAccel";
			this.lblAccel.Size = new System.Drawing.Size(110, 110);
			this.lblAccel.TabIndex = 23;
			this.lblAccel.Paint += new System.Windows.Forms.PaintEventHandler(this.lblAccel_Paint);
			// 
			// label2
			// 
			this.label2.AutoSize = true;
			this.label2.Location = new System.Drawing.Point(19, 174);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(97, 13);
			this.label2.TabIndex = 24;
			this.label2.Text = "Accelerometer (x,y)";
			// 
			// lblPlunger
			// 
			this.lblPlunger.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblPlunger.Location = new System.Drawing.Point(254, 148);
			this.lblPlunger.Name = "lblPlunger";
			this.lblPlunger.Size = new System.Drawing.Size(603, 23);
			this.lblPlunger.TabIndex = 25;
			this.lblPlunger.Click += new System.EventHandler(this.lblPlunger_Click);
			this.lblPlunger.Paint += new System.Windows.Forms.PaintEventHandler(this.lblPlunger_Paint);
			// 
			// label3
			// 
			this.label3.AutoSize = true;
			this.label3.Location = new System.Drawing.Point(526, 182);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(57, 13);
			this.label3.TabIndex = 26;
			this.label3.Text = "Plunger (z)";
			this.label3.Click += new System.EventHandler(this.label3_Click);
			// 
			// lblXY
			// 
			this.lblXY.Location = new System.Drawing.Point(19, 192);
			this.lblXY.Name = "lblXY";
			this.lblXY.Size = new System.Drawing.Size(97, 13);
			this.lblXY.TabIndex = 27;
			this.lblXY.Text = "0,0";
			this.lblXY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblZ
			// 
			this.lblZ.Location = new System.Drawing.Point(526, 199);
			this.lblZ.Name = "lblZ";
			this.lblZ.Size = new System.Drawing.Size(60, 13);
			this.lblZ.TabIndex = 28;
			this.lblZ.Text = "0";
			this.lblZ.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lblZ.Click += new System.EventHandler(this.lblZ_Click);
			// 
			// label4
			// 
			this.label4.AutoSize = true;
			this.label4.Location = new System.Drawing.Point(254, 64);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(436, 13);
			this.label4.TabIndex = 31;
			this.label4.Text = "This shows the accelerometer and plunger readings coming directly from the USB in" +
    "terface.";
			this.label4.Click += new System.EventHandler(this.label4_Click);
			// 
			// lblTextBoxBar
			// 
			this.lblTextBoxBar.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.lblTextBoxBar.Location = new System.Drawing.Point(-10, 313);
			this.lblTextBoxBar.Name = "lblTextBoxBar";
			this.lblTextBoxBar.Size = new System.Drawing.Size(770, 1);
			this.lblTextBoxBar.TabIndex = 32;
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(12, 340);
			this.textBox1.MaxLength = 1000000;
			this.textBox1.Multiline = true;
			this.textBox1.Name = "textBox1";
			this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.textBox1.Size = new System.Drawing.Size(688, 317);
			this.textBox1.TabIndex = 33;
			// 
			// timer1
			// 
			this.timer1.Enabled = true;
			this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
			// 
			// btnSave
			// 
			this.btnSave.FlatAppearance.BorderSize = 0;
			this.btnSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnSave.Image = global::PinscapeConfigTool.Properties.Resources.saveIcon;
			this.btnSave.Location = new System.Drawing.Point(706, 337);
			this.btnSave.Name = "btnSave";
			this.btnSave.Size = new System.Drawing.Size(32, 32);
			this.btnSave.TabIndex = 34;
			this.btnSave.UseVisualStyleBackColor = true;
			this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
			// 
			// label1
			// 
			this.label1.Image = ((System.Drawing.Image)(resources.GetObject("label1.Image")));
			this.label1.Location = new System.Drawing.Point(345, 130);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(21, 18);
			this.label1.TabIndex = 30;
			this.label1.Click += new System.EventHandler(this.label1_Click);
			// 
			// lblIcon
			// 
			this.lblIcon.Image = global::PinscapeConfigTool.Properties.Resources.joystickicon;
			this.lblIcon.Location = new System.Drawing.Point(824, 5);
			this.lblIcon.Name = "lblIcon";
			this.lblIcon.Size = new System.Drawing.Size(33, 32);
			this.lblIcon.TabIndex = 29;
			// 
			// btnLogging
			// 
			this.btnLogging.AutoSize = true;
			this.btnLogging.BackColor = System.Drawing.SystemColors.Window;
			this.btnLogging.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnLogging.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
			this.btnLogging.ForeColor = System.Drawing.Color.Purple;
			this.btnLogging.Location = new System.Drawing.Point(615, 261);
			this.btnLogging.Name = "btnLogging";
			this.btnLogging.Padding = new System.Windows.Forms.Padding(2);
			this.btnLogging.Size = new System.Drawing.Size(80, 31);
			this.btnLogging.TabIndex = 35;
			this.btnLogging.Text = "&Capture...";
			this.btnLogging.UseVisualStyleBackColor = false;
			this.btnLogging.Click += new System.EventHandler(this.btnLogging_Click);
			// 
			// lblBottomMarker
			// 
			this.lblBottomMarker.BackColor = System.Drawing.SystemColors.ControlDarkDark;
			this.lblBottomMarker.Location = new System.Drawing.Point(-1, 672);
			this.lblBottomMarker.Name = "lblBottomMarker";
			this.lblBottomMarker.Size = new System.Drawing.Size(770, 1);
			this.lblBottomMarker.TabIndex = 36;
			// 
			// label5
			// 
			this.label5.AutoSize = true;
			this.label5.Location = new System.Drawing.Point(9, 324);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(224, 13);
			this.label5.TabIndex = 37;
			this.label5.Text = "Hold down the CTRL key to capture readings.";
			// 
			// timer2
			// 
			this.timer2.Enabled = true;
			this.timer2.Interval = 20;
			this.timer2.Tick += new System.EventHandler(this.timer2_Tick);
			// 
			// jsKeyState
			// 
			this.jsKeyState.BackColor = System.Drawing.SystemColors.Window;
			this.jsKeyState.InitialImage = null;
			this.jsKeyState.Location = new System.Drawing.Point(12, 243);
			this.jsKeyState.Name = "jsKeyState";
			this.jsKeyState.Size = new System.Drawing.Size(375, 50);
			this.jsKeyState.TabIndex = 40;
			this.jsKeyState.TabStop = false;
			this.jsKeyState.Paint += new System.Windows.Forms.PaintEventHandler(this.jsKeyState_Paint);
			// 
			// label6
			// 
			this.label6.AutoSize = true;
			this.label6.Location = new System.Drawing.Point(9, 227);
			this.label6.Name = "label6";
			this.label6.Size = new System.Drawing.Size(43, 13);
			this.label6.TabIndex = 41;
			this.label6.Text = "Buttons";
			// 
			// btnHelp
			// 
			this.btnHelp.AutoSize = true;
			this.btnHelp.BackColor = System.Drawing.SystemColors.Window;
			this.btnHelp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnHelp.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
			this.btnHelp.ForeColor = System.Drawing.Color.Purple;
			this.btnHelp.Location = new System.Drawing.Point(698, 261);
			this.btnHelp.Name = "btnHelp";
			this.btnHelp.Padding = new System.Windows.Forms.Padding(2);
			this.btnHelp.Size = new System.Drawing.Size(78, 31);
			this.btnHelp.TabIndex = 42;
			this.btnHelp.Text = "Help";
			this.btnHelp.UseVisualStyleBackColor = false;
			this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
			// 
			// btnClose
			// 
			this.btnClose.AutoSize = true;
			this.btnClose.BackColor = System.Drawing.SystemColors.Window;
			this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnClose.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold);
			this.btnClose.ForeColor = System.Drawing.Color.Purple;
			this.btnClose.Location = new System.Drawing.Point(779, 261);
			this.btnClose.Name = "btnClose";
			this.btnClose.Padding = new System.Windows.Forms.Padding(2);
			this.btnClose.Size = new System.Drawing.Size(78, 31);
			this.btnClose.TabIndex = 43;
			this.btnClose.Text = "Close";
			this.btnClose.UseVisualStyleBackColor = false;
			this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
			// 
			// linkCenter
			// 
			this.linkCenter.AutoSize = true;
			this.linkCenter.Location = new System.Drawing.Point(36, 209);
			this.linkCenter.Name = "linkCenter";
			this.linkCenter.Size = new System.Drawing.Size(63, 13);
			this.linkCenter.TabIndex = 44;
			this.linkCenter.TabStop = true;
			this.linkCenter.Text = "Center Now";
			this.linkCenter.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkCenter_LinkClicked);
			// 
			// lblVelocityPlot
			// 
			this.lblVelocityPlot.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.lblVelocityPlot.Location = new System.Drawing.Point(128, 61);
			this.lblVelocityPlot.Name = "lblVelocityPlot";
			this.lblVelocityPlot.Size = new System.Drawing.Size(110, 110);
			this.lblVelocityPlot.TabIndex = 45;
			this.lblVelocityPlot.Paint += new System.Windows.Forms.PaintEventHandler(this.lblVelocityPlot_Paint);
			// 
			// lblVXVY
			// 
			this.lblVXVY.Location = new System.Drawing.Point(134, 192);
			this.lblVXVY.Name = "lblVXVY";
			this.lblVXVY.Size = new System.Drawing.Size(97, 13);
			this.lblVXVY.TabIndex = 47;
			this.lblVXVY.Text = "0,0";
			this.lblVXVY.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// lblAccelVelocityTitle
			// 
			this.lblAccelVelocityTitle.AutoSize = true;
			this.lblAccelVelocityTitle.Location = new System.Drawing.Point(146, 174);
			this.lblAccelVelocityTitle.Name = "lblAccelVelocityTitle";
			this.lblAccelVelocityTitle.Size = new System.Drawing.Size(74, 13);
			this.lblAccelVelocityTitle.TabIndex = 46;
			this.lblAccelVelocityTitle.Text = "Accel Velocity";
			// 
			// label7
			// 
			this.label7.AutoSize = true;
			this.label7.Location = new System.Drawing.Point(144, 209);
			this.label7.Name = "label7";
			this.label7.Size = new System.Drawing.Size(37, 13);
			this.label7.TabIndex = 48;
			this.label7.Text = "Scale:";
			// 
			// velocityScale
			// 
			this.velocityScale.Location = new System.Drawing.Point(187, 206);
			this.velocityScale.Name = "velocityScale";
			this.velocityScale.Size = new System.Drawing.Size(36, 20);
			this.velocityScale.TabIndex = 49;
			this.velocityScale.TextChanged += new System.EventHandler(this.velocityScale_TextChanged);
			// 
			// lblVelocityScalingError
			// 
			this.lblVelocityScalingError.AutoSize = true;
			this.lblVelocityScalingError.ForeColor = System.Drawing.Color.Red;
			this.lblVelocityScalingError.Location = new System.Drawing.Point(230, 209);
			this.lblVelocityScalingError.Name = "lblVelocityScalingError";
			this.lblVelocityScalingError.Size = new System.Drawing.Size(164, 13);
			this.lblVelocityScalingError.TabIndex = 50;
			this.lblVelocityScalingError.Text = "Must be 1 to 255 (or 0 for default)";
			this.lblVelocityScalingError.Visible = false;
			// 
			// JoystickViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.ClientSize = new System.Drawing.Size(872, 301);
			this.Controls.Add(this.lblVelocityScalingError);
			this.Controls.Add(this.velocityScale);
			this.Controls.Add(this.label7);
			this.Controls.Add(this.lblVXVY);
			this.Controls.Add(this.lblAccelVelocityTitle);
			this.Controls.Add(this.lblVelocityPlot);
			this.Controls.Add(this.linkCenter);
			this.Controls.Add(this.btnClose);
			this.Controls.Add(this.btnHelp);
			this.Controls.Add(this.label6);
			this.Controls.Add(this.jsKeyState);
			this.Controls.Add(this.label5);
			this.Controls.Add(this.lblBottomMarker);
			this.Controls.Add(this.btnLogging);
			this.Controls.Add(this.btnSave);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.lblTextBoxBar);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lblIcon);
			this.Controls.Add(this.lblZ);
			this.Controls.Add(this.lblXY);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.lblPlunger);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.lblAccel);
			this.Controls.Add(this.lblTitle);
			this.Controls.Add(this.lblTitleBar);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "JoystickViewer";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Pinscape Setup";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.JoystickViewer_FormClosing);
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.JoystickViewer_FormClosed);
			this.Load += new System.EventHandler(this.JoystickViewer_Load);
			this.Shown += new System.EventHandler(this.JoystickViewer_Shown);
			this.Resize += new System.EventHandler(this.JoystickViewer_Resize);
			((System.ComponentModel.ISupportInitialize)(this.jsKeyState)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblTitleBar;
        private System.Windows.Forms.Label lblAccel;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblPlunger;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblXY;
        private System.Windows.Forms.Label lblZ;
        private System.Windows.Forms.Label lblIcon;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblTextBoxBar;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.Button btnLogging;
        private System.Windows.Forms.Label lblBottomMarker;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Timer timer2;
        private System.Windows.Forms.PictureBox jsKeyState;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.LinkLabel linkCenter;
		private System.Windows.Forms.Label lblVelocityPlot;
		private System.Windows.Forms.Label lblVXVY;
		private System.Windows.Forms.Label lblAccelVelocityTitle;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.TextBox velocityScale;
		private System.Windows.Forms.Label lblVelocityScalingError;
	}
}