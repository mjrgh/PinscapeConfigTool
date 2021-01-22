namespace PinscapeConfigTool
{
    partial class TvOnTester
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
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.ckRelayOn = new System.Windows.Forms.CheckBox();
            this.btnHelp = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.label7 = new System.Windows.Forms.Label();
            this.btnRelayPulse = new System.Windows.Forms.Button();
            this.lblStatusPin = new System.Windows.Forms.Label();
            this.lblLatchPin = new System.Windows.Forms.Label();
            this.lblRelayPin = new System.Windows.Forms.Label();
            this.statusTimer = new System.Windows.Forms.Timer(this.components);
            this.lblStatus = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(12, 16);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(111, 16);
            this.label5.TabIndex = 23;
            this.label5.Text = "Testeur TV ON";
            // 
            // label4
            // 
            this.label4.Image = global::PinscapeConfigTool.Resources.h1TvOn;
            this.label4.Location = new System.Drawing.Point(423, 12);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(55, 23);
            this.label4.TabIndex = 22;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label1.Location = new System.Drawing.Point(-1, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(554, 2);
            this.label1.TabIndex = 21;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(151, 13);
            this.label2.TabIndex = 24;
            this.label2.Text = "Broche d\'état de l\'alimentation:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(35, 115);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(120, 13);
            this.label3.TabIndex = 25;
            this.label3.Text = "Goupille de verrouillage:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(52, 138);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(103, 13);
            this.label6.TabIndex = 26;
            this.label6.Text = "Broche de relais TV:";
            // 
            // ckRelayOn
            // 
            this.ckRelayOn.Appearance = System.Windows.Forms.Appearance.Button;
            this.ckRelayOn.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.ckRelayOn.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Yellow;
            this.ckRelayOn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ckRelayOn.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ckRelayOn.ForeColor = System.Drawing.Color.Purple;
            this.ckRelayOn.Location = new System.Drawing.Point(381, 92);
            this.ckRelayOn.Name = "ckRelayOn";
            this.ckRelayOn.Size = new System.Drawing.Size(92, 26);
            this.ckRelayOn.TabIndex = 27;
            this.ckRelayOn.Text = "Relayer sur";
            this.ckRelayOn.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.ckRelayOn.UseVisualStyleBackColor = true;
            this.ckRelayOn.CheckedChanged += new System.EventHandler(this.ckRelayOn_CheckedChanged);
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
            this.btnHelp.Location = new System.Drawing.Point(297, 228);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(84, 27);
            this.btnHelp.TabIndex = 29;
            this.btnHelp.Text = "Aide";
            this.btnHelp.UseVisualStyleBackColor = false;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
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
            this.btnClose.Location = new System.Drawing.Point(387, 228);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(91, 27);
            this.btnClose.TabIndex = 28;
            this.btnClose.Text = "Fermer";
            this.btnClose.UseVisualStyleBackColor = false;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // label7
            // 
            this.label7.BackColor = System.Drawing.SystemColors.ControlDarkDark;
            this.label7.Location = new System.Drawing.Point(-1, 210);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(554, 2);
            this.label7.TabIndex = 30;
            // 
            // btnRelayPulse
            // 
            this.btnRelayPulse.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.btnRelayPulse.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Yellow;
            this.btnRelayPulse.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRelayPulse.Font = new System.Drawing.Font("Arial", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnRelayPulse.ForeColor = System.Drawing.Color.Purple;
            this.btnRelayPulse.Location = new System.Drawing.Point(338, 128);
            this.btnRelayPulse.Name = "btnRelayPulse";
            this.btnRelayPulse.Size = new System.Drawing.Size(135, 26);
            this.btnRelayPulse.TabIndex = 31;
            this.btnRelayPulse.Text = "Relais d\'impulsions";
            this.btnRelayPulse.UseVisualStyleBackColor = true;
            this.btnRelayPulse.Click += new System.EventHandler(this.btnRelayPulse_Click);
            // 
            // lblStatusPin
            // 
            this.lblStatusPin.AutoSize = true;
            this.lblStatusPin.Location = new System.Drawing.Point(161, 92);
            this.lblStatusPin.Name = "lblStatusPin";
            this.lblStatusPin.Size = new System.Drawing.Size(22, 13);
            this.lblStatusPin.TabIndex = 32;
            this.lblStatusPin.Text = "NC";
            // 
            // lblLatchPin
            // 
            this.lblLatchPin.AutoSize = true;
            this.lblLatchPin.Location = new System.Drawing.Point(161, 115);
            this.lblLatchPin.Name = "lblLatchPin";
            this.lblLatchPin.Size = new System.Drawing.Size(22, 13);
            this.lblLatchPin.TabIndex = 33;
            this.lblLatchPin.Text = "NC";
            // 
            // lblRelayPin
            // 
            this.lblRelayPin.AutoSize = true;
            this.lblRelayPin.Location = new System.Drawing.Point(161, 138);
            this.lblRelayPin.Name = "lblRelayPin";
            this.lblRelayPin.Size = new System.Drawing.Size(22, 13);
            this.lblRelayPin.TabIndex = 34;
            this.lblRelayPin.Text = "NC";
            // 
            // statusTimer
            // 
            this.statusTimer.Enabled = true;
            this.statusTimer.Tick += new System.EventHandler(this.statusTimer_Tick);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(161, 161);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(53, 13);
            this.lblStatus.TabIndex = 36;
            this.lblStatus.Text = "Unknown";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(129, 161);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(26, 13);
            this.label9.TabIndex = 35;
            this.label9.Text = "Etat";
            // 
            // TvOnTester
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(485, 267);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.lblRelayPin);
            this.Controls.Add(this.lblLatchPin);
            this.Controls.Add(this.lblStatusPin);
            this.Controls.Add(this.btnRelayPulse);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.ckRelayOn);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "TvOnTester";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Testeur TV ON";
            this.Load += new System.EventHandler(this.TvOnTester_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox ckRelayOn;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button btnRelayPulse;
        private System.Windows.Forms.Label lblStatusPin;
        private System.Windows.Forms.Label lblLatchPin;
        private System.Windows.Forms.Label lblRelayPin;
        private System.Windows.Forms.Timer statusTimer;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label label9;
    }
}