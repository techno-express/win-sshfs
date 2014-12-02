namespace WindowsFormsApplication2
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
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.Loglevel = new System.Windows.Forms.ComboBox();
            this.Loglevellabel = new System.Windows.Forms.Label();
            this.virtualdriveletter = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox1.Location = new System.Drawing.Point(110, 23);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(193, 19);
            this.checkBox1.TabIndex = 0;
            this.checkBox1.Text = "start FiSSH with system startup";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.checkBox2.Location = new System.Drawing.Point(42, 60);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(261, 19);
            this.checkBox2.TabIndex = 1;
            this.checkBox2.Text = "reconnect all active connections at wake up";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // Loglevel
            // 
            this.Loglevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.Loglevel.FormattingEnabled = true;
            this.Loglevel.Items.AddRange(new object[] {
            "Debugging",
            "Warnings",
            "Errors",
            "No log"});
            this.Loglevel.Location = new System.Drawing.Point(182, 105);
            this.Loglevel.Name = "Loglevel";
            this.Loglevel.Size = new System.Drawing.Size(121, 21);
            this.Loglevel.TabIndex = 2;
            // 
            // Loglevellabel
            // 
            this.Loglevellabel.AutoSize = true;
            this.Loglevellabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Loglevellabel.Location = new System.Drawing.Point(113, 106);
            this.Loglevellabel.Name = "Loglevellabel";
            this.Loglevellabel.Size = new System.Drawing.Size(63, 16);
            this.Loglevellabel.TabIndex = 3;
            this.Loglevellabel.Text = "Loglevel:";
            // 
            // virtualdriveletter
            // 
            this.virtualdriveletter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.virtualdriveletter.FormattingEnabled = true;
            this.virtualdriveletter.Items.AddRange(new object[] {
            "A",
            "B",
            "C",
            "D",
            "E",
            "F",
            "G",
            "H",
            "I",
            "J",
            "K",
            "L",
            "M",
            "N",
            "O",
            "P",
            "Q",
            "R",
            "S",
            "T",
            "U",
            "V",
            "W",
            "X",
            "Y",
            "Z"});
            this.virtualdriveletter.Location = new System.Drawing.Point(182, 153);
            this.virtualdriveletter.Name = "virtualdriveletter";
            this.virtualdriveletter.Size = new System.Drawing.Size(121, 21);
            this.virtualdriveletter.TabIndex = 4;
            this.virtualdriveletter.SelectedIndexChanged += new System.EventHandler(this.virtualdriveletter_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(23, 159);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(153, 15);
            this.label1.TabIndex = 5;
            this.label1.Text = "Drive Letter of Virtual Drive:";
            // 
            // OptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(427, 242);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.virtualdriveletter);
            this.Controls.Add(this.Loglevellabel);
            this.Controls.Add(this.Loglevel);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.MaximizeBox = false;
            this.Name = "OptionsForm";
            this.Text = "Options";
            this.Load += new System.EventHandler(this.Loglevel_SelectedIndexChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.ComboBox Loglevel;
        private System.Windows.Forms.Label Loglevellabel;
        private System.Windows.Forms.ComboBox virtualdriveletter;
        private System.Windows.Forms.Label label1;

    }
}