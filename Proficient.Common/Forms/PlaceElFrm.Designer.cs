namespace Proficient.Forms
{
    partial class PlaceElFrm
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
            this.radionumber = new System.Windows.Forms.RadioButton();
            this.radiooffset = new System.Windows.Forms.RadioButton();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.txtlabel = new System.Windows.Forms.Label();
            this.startoffsetlbl = new System.Windows.Forms.Label();
            this.startoffset = new System.Windows.Forms.TextBox();
            this.okbutton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // radionumber
            // 
            this.radionumber.AutoSize = true;
            this.radionumber.Checked = true;
            this.radionumber.Location = new System.Drawing.Point(30, 20);
            this.radionumber.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.radionumber.Name = "radionumber";
            this.radionumber.Size = new System.Drawing.Size(106, 17);
            this.radionumber.TabIndex = 0;
            this.radionumber.TabStop = true;
            this.radionumber.Text = "Place by Number";
            this.radionumber.UseVisualStyleBackColor = true;
            this.radionumber.CheckedChanged += new System.EventHandler(this.RadioNumber_CheckedChanged);
            // 
            // radiooffset
            // 
            this.radiooffset.AutoSize = true;
            this.radiooffset.Location = new System.Drawing.Point(145, 20);
            this.radiooffset.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.radiooffset.Name = "radiooffset";
            this.radiooffset.Size = new System.Drawing.Size(97, 17);
            this.radiooffset.TabIndex = 1;
            this.radiooffset.Text = "Place by Offset";
            this.radiooffset.UseVisualStyleBackColor = true;
            this.radiooffset.CheckedChanged += new System.EventHandler(this.RadioOffset_CheckedChanged);
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(199, 52);
            this.textBox1.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(33, 20);
            this.textBox1.TabIndex = 2;
            // 
            // txtlabel
            // 
            this.txtlabel.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.txtlabel.Location = new System.Drawing.Point(7, 46);
            this.txtlabel.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.txtlabel.Name = "txtlabel";
            this.txtlabel.Size = new System.Drawing.Size(181, 29);
            this.txtlabel.TabIndex = 3;
            this.txtlabel.Text = "label1";
            this.txtlabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // startoffsetlbl
            // 
            this.startoffsetlbl.AutoSize = true;
            this.startoffsetlbl.Location = new System.Drawing.Point(85, 83);
            this.startoffsetlbl.Margin = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.startoffsetlbl.Name = "startoffsetlbl";
            this.startoffsetlbl.Size = new System.Drawing.Size(101, 13);
            this.startoffsetlbl.TabIndex = 5;
            this.startoffsetlbl.Text = "Offset from Start (ft):";
            this.startoffsetlbl.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // startoffset
            // 
            this.startoffset.Location = new System.Drawing.Point(199, 81);
            this.startoffset.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.startoffset.Name = "startoffset";
            this.startoffset.Size = new System.Drawing.Size(33, 20);
            this.startoffset.TabIndex = 4;
            this.startoffset.Text = "0";
            // 
            // okbutton
            // 
            this.okbutton.Location = new System.Drawing.Point(103, 117);
            this.okbutton.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.okbutton.Name = "okbutton";
            this.okbutton.Size = new System.Drawing.Size(63, 20);
            this.okbutton.TabIndex = 6;
            this.okbutton.Text = "OK";
            this.okbutton.UseVisualStyleBackColor = true;
            this.okbutton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // PlaceElFrm
            // 
            this.AcceptButton = this.okbutton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(265, 151);
            this.Controls.Add(this.okbutton);
            this.Controls.Add(this.startoffsetlbl);
            this.Controls.Add(this.startoffset);
            this.Controls.Add(this.txtlabel);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.radiooffset);
            this.Controls.Add(this.radionumber);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(1, 1, 1, 1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "PlaceElFrm";
            this.ShowIcon = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Place Elements";
            this.Load += new System.EventHandler(this.PlaceElFrm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.RadioButton radionumber;
        public System.Windows.Forms.RadioButton radiooffset;
        public System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label txtlabel;
        private System.Windows.Forms.Label startoffsetlbl;
        public System.Windows.Forms.TextBox startoffset;
        private System.Windows.Forms.Button okbutton;
    }
}