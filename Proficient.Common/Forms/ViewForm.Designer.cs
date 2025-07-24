namespace Proficient.Forms
{
    partial class ViewForm
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
            this.viewdropdown = new System.Windows.Forms.ComboBox();
            this.okbutton = new System.Windows.Forms.Button();
            this.cancelbutton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // viewdropdown
            // 
            this.viewdropdown.FormattingEnabled = true;
            this.viewdropdown.Location = new System.Drawing.Point(12, 12);
            this.viewdropdown.Name = "viewdropdown";
            this.viewdropdown.Size = new System.Drawing.Size(478, 21);
            this.viewdropdown.TabIndex = 0;
            this.viewdropdown.Text = "Select new view...";
            this.viewdropdown.SelectedIndexChanged += new System.EventHandler(this.ViewDropdown_SelectedIndexChanged);
            // 
            // okbutton
            // 
            this.okbutton.Location = new System.Drawing.Point(322, 49);
            this.okbutton.Name = "okbutton";
            this.okbutton.Size = new System.Drawing.Size(75, 23);
            this.okbutton.TabIndex = 1;
            this.okbutton.Text = "OK";
            this.okbutton.UseVisualStyleBackColor = true;
            this.okbutton.Click += new System.EventHandler(this.OkButton_Click);
            // 
            // cancelbutton
            // 
            this.cancelbutton.Location = new System.Drawing.Point(415, 49);
            this.cancelbutton.Name = "cancelbutton";
            this.cancelbutton.Size = new System.Drawing.Size(75, 23);
            this.cancelbutton.TabIndex = 2;
            this.cancelbutton.Text = "Cancel";
            this.cancelbutton.UseVisualStyleBackColor = true;
            this.cancelbutton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ViewForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(502, 84);
            this.Controls.Add(this.cancelbutton);
            this.Controls.Add(this.okbutton);
            this.Controls.Add(this.viewdropdown);
            this.Name = "ViewForm";
            this.Text = "Select View";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox viewdropdown;
        private System.Windows.Forms.Button okbutton;
        private System.Windows.Forms.Button cancelbutton;
    }
}