namespace Proficient.Forms
{
    partial class ExcelAssignFrm
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
            this.xllbl = new System.Windows.Forms.Label();
            this.filelocationtxt = new System.Windows.Forms.TextBox();
            this.xlfilebtn = new System.Windows.Forms.Button();
            this.xlgroup = new System.Windows.Forms.GroupBox();
            this.hdrLbl = new System.Windows.Forms.Label();
            this.hdrRowCtrl = new System.Windows.Forms.NumericUpDown();
            this.KeyColDrop = new System.Windows.Forms.ComboBox();
            this.keycolumnlbl = new System.Windows.Forms.Label();
            this.wkshtDrop = new System.Windows.Forms.ComboBox();
            this.Wkshtlbl = new System.Windows.Forms.Label();
            this.rvtgroup = new System.Windows.Forms.GroupBox();
            this.catDrop = new System.Windows.Forms.ComboBox();
            this.catlbl = new System.Windows.Forms.Label();
            this.familyDrop = new System.Windows.Forms.ComboBox();
            this.familylbl = new System.Windows.Forms.Label();
            this.srclbl = new System.Windows.Forms.Label();
            this.destlbl = new System.Windows.Forms.Label();
            this.addbtn = new System.Windows.Forms.Button();
            this.subtractbtn = new System.Windows.Forms.Button();
            this.assnbtn = new System.Windows.Forms.Button();
            this.closebtn = new System.Windows.Forms.Button();
            this.getColsBtn = new System.Windows.Forms.Button();
            this.typeInstLbl = new System.Windows.Forms.Label();
            this.assignbylbl = new System.Windows.Forms.Label();
            this.sc1 = new System.Windows.Forms.ComboBox();
            this.dp1 = new System.Windows.Forms.ComboBox();
            this.xlgroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.hdrRowCtrl)).BeginInit();
            this.rvtgroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // xllbl
            // 
            this.xllbl.AutoSize = true;
            this.xllbl.Location = new System.Drawing.Point(11, 32);
            this.xllbl.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.xllbl.Name = "xllbl";
            this.xllbl.Size = new System.Drawing.Size(70, 13);
            this.xllbl.TabIndex = 0;
            this.xllbl.Text = "File Location:";
            // 
            // filelocationtxt
            // 
            this.filelocationtxt.Location = new System.Drawing.Point(84, 30);
            this.filelocationtxt.Margin = new System.Windows.Forms.Padding(2);
            this.filelocationtxt.Name = "filelocationtxt";
            this.filelocationtxt.ReadOnly = true;
            this.filelocationtxt.Size = new System.Drawing.Size(195, 20);
            this.filelocationtxt.TabIndex = 1;
            // 
            // xlfilebtn
            // 
            this.xlfilebtn.Location = new System.Drawing.Point(284, 29);
            this.xlfilebtn.Margin = new System.Windows.Forms.Padding(2);
            this.xlfilebtn.Name = "xlfilebtn";
            this.xlfilebtn.Size = new System.Drawing.Size(23, 19);
            this.xlfilebtn.TabIndex = 2;
            this.xlfilebtn.Text = "...";
            this.xlfilebtn.UseVisualStyleBackColor = true;
            this.xlfilebtn.Click += new System.EventHandler(this.XlFileBtn_Click);
            // 
            // xlgroup
            // 
            this.xlgroup.Controls.Add(this.hdrLbl);
            this.xlgroup.Controls.Add(this.hdrRowCtrl);
            this.xlgroup.Controls.Add(this.KeyColDrop);
            this.xlgroup.Controls.Add(this.keycolumnlbl);
            this.xlgroup.Controls.Add(this.wkshtDrop);
            this.xlgroup.Controls.Add(this.Wkshtlbl);
            this.xlgroup.Controls.Add(this.xllbl);
            this.xlgroup.Controls.Add(this.xlfilebtn);
            this.xlgroup.Controls.Add(this.filelocationtxt);
            this.xlgroup.Location = new System.Drawing.Point(9, 10);
            this.xlgroup.Margin = new System.Windows.Forms.Padding(2);
            this.xlgroup.Name = "xlgroup";
            this.xlgroup.Padding = new System.Windows.Forms.Padding(2);
            this.xlgroup.Size = new System.Drawing.Size(320, 150);
            this.xlgroup.TabIndex = 3;
            this.xlgroup.TabStop = false;
            this.xlgroup.Text = "Excel";
            // 
            // hdrLbl
            // 
            this.hdrLbl.AutoSize = true;
            this.hdrLbl.Location = new System.Drawing.Point(14, 94);
            this.hdrLbl.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.hdrLbl.Name = "hdrLbl";
            this.hdrLbl.Size = new System.Drawing.Size(70, 13);
            this.hdrLbl.TabIndex = 9;
            this.hdrLbl.Text = "Header Row:";
            // 
            // hdrRowCtrl
            // 
            this.hdrRowCtrl.Location = new System.Drawing.Point(84, 93);
            this.hdrRowCtrl.Margin = new System.Windows.Forms.Padding(2);
            this.hdrRowCtrl.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.hdrRowCtrl.Name = "hdrRowCtrl";
            this.hdrRowCtrl.Size = new System.Drawing.Size(56, 20);
            this.hdrRowCtrl.TabIndex = 4;
            this.hdrRowCtrl.ThousandsSeparator = true;
            this.hdrRowCtrl.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.hdrRowCtrl.ValueChanged += new System.EventHandler(this.WsDrop_SelectedIndexChanged);
            // 
            // KeyColDrop
            // 
            this.KeyColDrop.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.KeyColDrop.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.KeyColDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.KeyColDrop.FormattingEnabled = true;
            this.KeyColDrop.Location = new System.Drawing.Point(84, 122);
            this.KeyColDrop.Margin = new System.Windows.Forms.Padding(2);
            this.KeyColDrop.Name = "KeyColDrop";
            this.KeyColDrop.Size = new System.Drawing.Size(195, 21);
            this.KeyColDrop.TabIndex = 5;
            this.KeyColDrop.SelectedIndexChanged += new System.EventHandler(this.KeyColumnDrop_SelectedIndexChanged);
            // 
            // keycolumnlbl
            // 
            this.keycolumnlbl.AutoSize = true;
            this.keycolumnlbl.Location = new System.Drawing.Point(14, 124);
            this.keycolumnlbl.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.keycolumnlbl.Name = "keycolumnlbl";
            this.keycolumnlbl.Size = new System.Drawing.Size(66, 13);
            this.keycolumnlbl.TabIndex = 2;
            this.keycolumnlbl.Text = "Key Column:";
            // 
            // wkshtDrop
            // 
            this.wkshtDrop.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.wkshtDrop.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.wkshtDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.wkshtDrop.FormattingEnabled = true;
            this.wkshtDrop.Location = new System.Drawing.Point(84, 60);
            this.wkshtDrop.Margin = new System.Windows.Forms.Padding(2);
            this.wkshtDrop.Name = "wkshtDrop";
            this.wkshtDrop.Size = new System.Drawing.Size(195, 21);
            this.wkshtDrop.TabIndex = 3;
            this.wkshtDrop.SelectedIndexChanged += new System.EventHandler(this.WsDrop_SelectedIndexChanged);
            // 
            // Wkshtlbl
            // 
            this.Wkshtlbl.AutoSize = true;
            this.Wkshtlbl.Location = new System.Drawing.Point(20, 63);
            this.Wkshtlbl.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Wkshtlbl.Name = "Wkshtlbl";
            this.Wkshtlbl.Size = new System.Drawing.Size(62, 13);
            this.Wkshtlbl.TabIndex = 3;
            this.Wkshtlbl.Text = "Worksheet:";
            // 
            // rvtgroup
            // 
            this.rvtgroup.Controls.Add(this.catDrop);
            this.rvtgroup.Controls.Add(this.catlbl);
            this.rvtgroup.Controls.Add(this.familyDrop);
            this.rvtgroup.Controls.Add(this.familylbl);
            this.rvtgroup.Location = new System.Drawing.Point(9, 164);
            this.rvtgroup.Margin = new System.Windows.Forms.Padding(2);
            this.rvtgroup.Name = "rvtgroup";
            this.rvtgroup.Padding = new System.Windows.Forms.Padding(2);
            this.rvtgroup.Size = new System.Drawing.Size(320, 109);
            this.rvtgroup.TabIndex = 4;
            this.rvtgroup.TabStop = false;
            this.rvtgroup.Text = "Revit";
            // 
            // catDrop
            // 
            this.catDrop.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.catDrop.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.catDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.catDrop.FormattingEnabled = true;
            this.catDrop.Location = new System.Drawing.Point(84, 30);
            this.catDrop.Margin = new System.Windows.Forms.Padding(2);
            this.catDrop.Name = "catDrop";
            this.catDrop.Size = new System.Drawing.Size(195, 21);
            this.catDrop.TabIndex = 6;
            this.catDrop.SelectedIndexChanged += new System.EventHandler(this.CatDrop_SelectedIndexChanged);
            // 
            // catlbl
            // 
            this.catlbl.AutoSize = true;
            this.catlbl.Location = new System.Drawing.Point(28, 30);
            this.catlbl.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.catlbl.Name = "catlbl";
            this.catlbl.Size = new System.Drawing.Size(52, 13);
            this.catlbl.TabIndex = 4;
            this.catlbl.Text = "Category:";
            // 
            // familyDrop
            // 
            this.familyDrop.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.familyDrop.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.familyDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.familyDrop.FormattingEnabled = true;
            this.familyDrop.Location = new System.Drawing.Point(84, 66);
            this.familyDrop.Margin = new System.Windows.Forms.Padding(2);
            this.familyDrop.Name = "familyDrop";
            this.familyDrop.Size = new System.Drawing.Size(195, 21);
            this.familyDrop.Sorted = true;
            this.familyDrop.TabIndex = 7;
            this.familyDrop.SelectedIndexChanged += new System.EventHandler(this.FamilyDrop_SelectedIndexChanged);
            // 
            // familylbl
            // 
            this.familylbl.AutoSize = true;
            this.familylbl.Location = new System.Drawing.Point(40, 68);
            this.familylbl.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.familylbl.Name = "familylbl";
            this.familylbl.Size = new System.Drawing.Size(39, 13);
            this.familylbl.TabIndex = 2;
            this.familylbl.Text = "Family:";
            // 
            // srclbl
            // 
            this.srclbl.AutoSize = true;
            this.srclbl.Location = new System.Drawing.Point(11, 286);
            this.srclbl.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.srclbl.Name = "srclbl";
            this.srclbl.Size = new System.Drawing.Size(82, 13);
            this.srclbl.TabIndex = 6;
            this.srclbl.Text = "Source Column:";
            // 
            // destlbl
            // 
            this.destlbl.AutoSize = true;
            this.destlbl.Location = new System.Drawing.Point(170, 288);
            this.destlbl.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.destlbl.Name = "destlbl";
            this.destlbl.Size = new System.Drawing.Size(114, 13);
            this.destlbl.TabIndex = 7;
            this.destlbl.Text = "Destination Parameter:";
            // 
            // addbtn
            // 
            this.addbtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.addbtn.Location = new System.Drawing.Point(288, 334);
            this.addbtn.Margin = new System.Windows.Forms.Padding(2);
            this.addbtn.Name = "addbtn";
            this.addbtn.Size = new System.Drawing.Size(19, 19);
            this.addbtn.TabIndex = 10;
            this.addbtn.Text = "+";
            this.addbtn.UseVisualStyleBackColor = true;
            this.addbtn.Click += new System.EventHandler(this.AddBtn_Click);
            // 
            // subtractbtn
            // 
            this.subtractbtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.subtractbtn.Location = new System.Drawing.Point(311, 334);
            this.subtractbtn.Margin = new System.Windows.Forms.Padding(2);
            this.subtractbtn.Name = "subtractbtn";
            this.subtractbtn.Size = new System.Drawing.Size(19, 19);
            this.subtractbtn.TabIndex = 11;
            this.subtractbtn.Text = "-";
            this.subtractbtn.UseVisualStyleBackColor = true;
            this.subtractbtn.Click += new System.EventHandler(this.SubtractBtn_Click);
            // 
            // assnbtn
            // 
            this.assnbtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.assnbtn.Location = new System.Drawing.Point(196, 375);
            this.assnbtn.Margin = new System.Windows.Forms.Padding(2);
            this.assnbtn.Name = "assnbtn";
            this.assnbtn.Size = new System.Drawing.Size(64, 23);
            this.assnbtn.TabIndex = 13;
            this.assnbtn.Text = "Assign";
            this.assnbtn.UseVisualStyleBackColor = true;
            this.assnbtn.Click += new System.EventHandler(this.AssnBtn_Click);
            // 
            // closebtn
            // 
            this.closebtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.closebtn.Location = new System.Drawing.Point(266, 375);
            this.closebtn.Margin = new System.Windows.Forms.Padding(2);
            this.closebtn.Name = "closebtn";
            this.closebtn.Size = new System.Drawing.Size(64, 23);
            this.closebtn.TabIndex = 14;
            this.closebtn.Text = "Close";
            this.closebtn.UseVisualStyleBackColor = true;
            this.closebtn.Click += new System.EventHandler(this.CloseBtn_Click);
            // 
            // getColsBtn
            // 
            this.getColsBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.getColsBtn.Location = new System.Drawing.Point(9, 375);
            this.getColsBtn.Margin = new System.Windows.Forms.Padding(2);
            this.getColsBtn.Name = "getColsBtn";
            this.getColsBtn.Size = new System.Drawing.Size(93, 23);
            this.getColsBtn.TabIndex = 12;
            this.getColsBtn.Text = "Get All Columns";
            this.getColsBtn.UseVisualStyleBackColor = true;
            // 
            // typeInstLbl
            // 
            this.typeInstLbl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.typeInstLbl.AutoSize = true;
            this.typeInstLbl.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.typeInstLbl.Location = new System.Drawing.Point(11, 343);
            this.typeInstLbl.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.typeInstLbl.Name = "typeInstLbl";
            this.typeInstLbl.Size = new System.Drawing.Size(0, 13);
            this.typeInstLbl.TabIndex = 13;
            // 
            // assignbylbl
            // 
            this.assignbylbl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.assignbylbl.AutoSize = true;
            this.assignbylbl.Location = new System.Drawing.Point(11, 341);
            this.assignbylbl.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.assignbylbl.Name = "assignbylbl";
            this.assignbylbl.Size = new System.Drawing.Size(0, 13);
            this.assignbylbl.TabIndex = 6;
            // 
            // sc1
            // 
            this.sc1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.sc1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.sc1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sc1.FormattingEnabled = true;
            this.sc1.Location = new System.Drawing.Point(14, 307);
            this.sc1.Margin = new System.Windows.Forms.Padding(2);
            this.sc1.Name = "sc1";
            this.sc1.Size = new System.Drawing.Size(155, 21);
            this.sc1.TabIndex = 8;
            // 
            // dp1
            // 
            this.dp1.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.dp1.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.dp1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.dp1.DropDownWidth = 209;
            this.dp1.FormattingEnabled = true;
            this.dp1.Location = new System.Drawing.Point(172, 307);
            this.dp1.Margin = new System.Windows.Forms.Padding(2);
            this.dp1.Name = "dp1";
            this.dp1.Size = new System.Drawing.Size(158, 21);
            this.dp1.Sorted = true;
            this.dp1.TabIndex = 9;
            this.dp1.SelectedIndexChanged += new System.EventHandler(this.DP1_SelectedIndexChanged);
            // 
            // ExcelAssignFrm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(339, 407);
            this.Controls.Add(this.sc1);
            this.Controls.Add(this.dp1);
            this.Controls.Add(this.typeInstLbl);
            this.Controls.Add(this.getColsBtn);
            this.Controls.Add(this.closebtn);
            this.Controls.Add(this.assnbtn);
            this.Controls.Add(this.subtractbtn);
            this.Controls.Add(this.addbtn);
            this.Controls.Add(this.destlbl);
            this.Controls.Add(this.assignbylbl);
            this.Controls.Add(this.srclbl);
            this.Controls.Add(this.rvtgroup);
            this.Controls.Add(this.xlgroup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ExcelAssignFrm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Excel Assign";
            this.xlgroup.ResumeLayout(false);
            this.xlgroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.hdrRowCtrl)).EndInit();
            this.rvtgroup.ResumeLayout(false);
            this.rvtgroup.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label xllbl;
        private System.Windows.Forms.TextBox filelocationtxt;
        private System.Windows.Forms.Button xlfilebtn;
        private System.Windows.Forms.GroupBox xlgroup;
        private System.Windows.Forms.ComboBox KeyColDrop;
        private System.Windows.Forms.Label keycolumnlbl;
        private System.Windows.Forms.ComboBox wkshtDrop;
        private System.Windows.Forms.Label Wkshtlbl;
        private System.Windows.Forms.GroupBox rvtgroup;
        private System.Windows.Forms.ComboBox familyDrop;
        private System.Windows.Forms.Label familylbl;
        private System.Windows.Forms.ComboBox catDrop;
        private System.Windows.Forms.Label catlbl;
        private System.Windows.Forms.Label srclbl;
        private System.Windows.Forms.Label destlbl;
        private System.Windows.Forms.Button addbtn;
        private System.Windows.Forms.Button subtractbtn;
        private System.Windows.Forms.Button assnbtn;
        private System.Windows.Forms.Button closebtn;
        private System.Windows.Forms.Button getColsBtn;
        private System.Windows.Forms.Label typeInstLbl;
        private System.Windows.Forms.Label assignbylbl;
        private System.Windows.Forms.Label hdrLbl;
        private System.Windows.Forms.NumericUpDown hdrRowCtrl;
        private System.Windows.Forms.ComboBox sc1;
        private System.Windows.Forms.ComboBox dp1;
    }
}