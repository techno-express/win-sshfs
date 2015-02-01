namespace GUI_WindowsForms
{
    partial class OptionsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsForm));
            this.Loglevel = new System.Windows.Forms.ComboBox();
            this.virtualdriveletter = new System.Windows.Forms.ComboBox();
            this.checkBox_startup = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.label4 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Loglevel
            // 
            this.Loglevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Loglevel.FormattingEnabled = true;
            this.Loglevel.Items.AddRange(new object[] {
            SimpleMind.Loglevel.Debug,
            SimpleMind.Loglevel.Warning,
            SimpleMind.Loglevel.Error,
            SimpleMind.Loglevel.None});
            this.Loglevel.Location = new System.Drawing.Point(131, 78);
            this.Loglevel.Name = "Loglevel";
            this.Loglevel.Size = new System.Drawing.Size(121, 21);
            this.Loglevel.TabIndex = 2;
            this.Loglevel.SelectionChangeCommitted += new System.EventHandler(this.Loglevel_SelectedIndexChanged);
            // 
            // virtualdriveletter
            // 
            this.virtualdriveletter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.virtualdriveletter.FormattingEnabled = true;
            this.virtualdriveletter.Items.AddRange(new object[] {
            "A:\\",
            "B:\\",
            "C:\\",
            "D:\\",
            "E:\\",
            "F:\\",
            "G:\\",
            "H:\\",
            "I:\\",
            "J:\\",
            "K:\\",
            "L:\\",
            "M:\\",
            "N:\\",
            "O:\\",
            "P:\\",
            "Q:\\",
            "R:\\",
            "S:\\",
            "T:\\",
            "U:\\",
            "V:\\",
            "W:\\",
            "X:\\",
            "Y:\\",
            "Z:\\"});
            this.virtualdriveletter.Location = new System.Drawing.Point(131, 114);
            this.virtualdriveletter.Name = "virtualdriveletter";
            this.virtualdriveletter.Size = new System.Drawing.Size(121, 21);
            this.virtualdriveletter.TabIndex = 4;
            this.virtualdriveletter.DropDownClosed += new System.EventHandler(this.virtualdriveletter_DropDownClosed);
            // 
            // checkBox_startup
            // 
            this.checkBox_startup.AutoSize = true;
            this.checkBox_startup.Location = new System.Drawing.Point(11, 19);
            this.checkBox_startup.Name = "checkBox_startup";
            this.checkBox_startup.Size = new System.Drawing.Size(158, 17);
            this.checkBox_startup.TabIndex = 6;
            this.checkBox_startup.Text = "Start program with Windows";
            this.checkBox_startup.UseVisualStyleBackColor = true;
            this.checkBox_startup.CheckedChanged += new System.EventHandler(this.checkBox_startup_CheckedChanged_1);
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(11, 42);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(241, 17);
            this.checkBox3.TabIndex = 7;
            this.checkBox3.Text = "Reconnect all active connections at wake up";
            this.checkBox3.UseVisualStyleBackColor = true;
            this.checkBox3.CheckedChanged += new System.EventHandler(this.checkBox3_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 81);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(50, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Loglevel:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 117);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(97, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Virtual Drive Letter:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(97, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(69, 24);
            this.label1.TabIndex = 13;
            this.label1.Text = "FiSSH";
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.LinkArea = new System.Windows.Forms.LinkArea(28, 4);
            this.linkLabel1.Location = new System.Drawing.Point(57, 88);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(174, 17);
            this.linkLabel1.TabIndex = 12;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "For more informations click HERE";
            this.linkLabel1.UseCompatibleTextRendering = true;
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label4.Location = new System.Drawing.Point(12, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(259, 13);
            this.label4.TabIndex = 11;
            this.label4.Text = "FiSSH is a Open Source project with the MIT-License";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(90, 336);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(103, 35);
            this.button1.TabIndex = 10;
            this.button1.Text = "OK";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox_startup);
            this.groupBox1.Controls.Add(this.Loglevel);
            this.groupBox1.Controls.Add(this.virtualdriveletter);
            this.groupBox1.Controls.Add(this.checkBox3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(4, 137);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(285, 182);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Options";
            // 
            // OptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(293, 389);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "OptionsForm";
            this.Text = "Options";
            this.Load += new System.EventHandler(this.Loglevel_SelectedIndexChanged);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox Loglevel;
        private System.Windows.Forms.ComboBox virtualdriveletter;
        private System.Windows.Forms.CheckBox checkBox_startup;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.GroupBox groupBox1;

    }
}