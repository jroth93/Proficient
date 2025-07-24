namespace Proficient.Ductulator
{
    partial class UserSettings
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
            this.label2 = new System.Windows.Forms.Label();
            this.Savebutton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.maxdepthtxt = new System.Windows.Forms.TextBox();
            this.mindepthtxt = new System.Windows.Forms.TextBox();
            this.Velocitytxt = new System.Windows.Forms.TextBox();
            this.Frictiontxt = new System.Windows.Forms.TextBox();
            this.radiovert = new System.Windows.Forms.RadioButton();
            this.radiohor = new System.Windows.Forms.RadioButton();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(108, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Default Friction Value";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 79);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(111, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Default Velocity Value";
            // 
            // Savebutton
            // 
            this.Savebutton.Location = new System.Drawing.Point(177, 344);
            this.Savebutton.Name = "Savebutton";
            this.Savebutton.Size = new System.Drawing.Size(75, 23);
            this.Savebutton.TabIndex = 4;
            this.Savebutton.Text = "Save";
            this.Savebutton.UseVisualStyleBackColor = true;
            this.Savebutton.Click += new System.EventHandler(this.Savebutton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(27, 122);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Default Minimum Depth";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(27, 167);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(120, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Default Maximum Depth";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(27, 208);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(128, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Friction Digits of Precision";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(27, 247);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(126, 13);
            this.label6.TabIndex = 11;
            this.label6.Text = "Keep Application On Top";
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Location = new System.Drawing.Point(206, 247);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(15, 14);
            this.checkBox1.TabIndex = 12;
            this.checkBox1.UseVisualStyleBackColor = true;
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(193, 206);
            this.numericUpDown1.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDown1.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(47, 20);
            this.numericUpDown1.TabIndex = 10;
            // 
            // maxdepthtxt
            // 
            this.maxdepthtxt.Location = new System.Drawing.Point(167, 164);
            this.maxdepthtxt.Name = "maxdepthtxt";
            this.maxdepthtxt.Size = new System.Drawing.Size(100, 20);
            this.maxdepthtxt.TabIndex = 8;
            // 
            // mindepthtxt
            // 
            this.mindepthtxt.Location = new System.Drawing.Point(167, 119);
            this.mindepthtxt.Name = "mindepthtxt";
            this.mindepthtxt.Size = new System.Drawing.Size(100, 20);
            this.mindepthtxt.TabIndex = 7;
            // 
            // Velocitytxt
            // 
            this.Velocitytxt.Location = new System.Drawing.Point(167, 76);
            this.Velocitytxt.Name = "Velocitytxt";
            this.Velocitytxt.Size = new System.Drawing.Size(100, 20);
            this.Velocitytxt.TabIndex = 3;
            // 
            // Frictiontxt
            // 
            this.Frictiontxt.Location = new System.Drawing.Point(167, 35);
            this.Frictiontxt.Name = "Frictiontxt";
            this.Frictiontxt.Size = new System.Drawing.Size(100, 20);
            this.Frictiontxt.TabIndex = 2;
            // 
            // radiovert
            // 
            this.radiovert.AutoSize = true;
            this.radiovert.Location = new System.Drawing.Point(180, 278);
            this.radiovert.Name = "radiovert";
            this.radiovert.Size = new System.Drawing.Size(60, 17);
            this.radiovert.TabIndex = 13;
            this.radiovert.TabStop = true;
            this.radiovert.Text = "Vertical";
            this.radiovert.UseVisualStyleBackColor = true;
            // 
            // radiohor
            // 
            this.radiohor.AutoSize = true;
            this.radiohor.Location = new System.Drawing.Point(180, 301);
            this.radiohor.Name = "radiohor";
            this.radiohor.Size = new System.Drawing.Size(72, 17);
            this.radiohor.TabIndex = 14;
            this.radiohor.TabStop = true;
            this.radiohor.Text = "Horizontal";
            this.radiohor.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(27, 291);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(113, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Application Orientation";
            // 
            // UserSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(302, 398);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.radiohor);
            this.Controls.Add(this.radiovert);
            this.Controls.Add(this.checkBox1);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.numericUpDown1);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.maxdepthtxt);
            this.Controls.Add(this.mindepthtxt);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Savebutton);
            this.Controls.Add(this.Velocitytxt);
            this.Controls.Add(this.Frictiontxt);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "UserSettings";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "User Settings";
            this.Load += new System.EventHandler(this.UserSettings_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox Frictiontxt;
        private System.Windows.Forms.TextBox Velocitytxt;
        private System.Windows.Forms.Button Savebutton;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox mindepthtxt;
        private System.Windows.Forms.TextBox maxdepthtxt;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.RadioButton radiovert;
        private System.Windows.Forms.RadioButton radiohor;
        private System.Windows.Forms.Label label7;
    }
}