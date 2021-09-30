namespace SchemaMgr
{
    partial class frmMain
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
			this.btnSelect = new System.Windows.Forms.Button();
			this.browseNewSchema = new System.Windows.Forms.OpenFileDialog();
			this.txtSelectedFile = new System.Windows.Forms.TextBox();
			this.cmdUpgrade = new System.Windows.Forms.Button();
			this.txtOutput = new System.Windows.Forms.TextBox();
			this.txtDriver = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.btnImportData = new System.Windows.Forms.Button();
			this.browseData = new System.Windows.Forms.OpenFileDialog();
			this.SuspendLayout();
			// 
			// btnSelect
			// 
			this.btnSelect.Location = new System.Drawing.Point(305, 10);
			this.btnSelect.Name = "btnSelect";
			this.btnSelect.Size = new System.Drawing.Size(79, 22);
			this.btnSelect.TabIndex = 0;
			this.btnSelect.Text = "Select";
			this.btnSelect.UseVisualStyleBackColor = true;
			this.btnSelect.Click += new System.EventHandler(this.cmdSelect_Click);
			// 
			// browseNewSchema
			// 
			this.browseNewSchema.Filter = "Database SchemaExtension (*.xml)|*.xml";
			this.browseNewSchema.Title = "Select schema definition";
			// 
			// txtSelectedFile
			// 
			this.txtSelectedFile.Location = new System.Drawing.Point(12, 12);
			this.txtSelectedFile.Name = "txtSelectedFile";
			this.txtSelectedFile.ReadOnly = true;
			this.txtSelectedFile.Size = new System.Drawing.Size(287, 20);
			this.txtSelectedFile.TabIndex = 1;
			// 
			// cmdUpgrade
			// 
			this.cmdUpgrade.Location = new System.Drawing.Point(300, 269);
			this.cmdUpgrade.Name = "cmdUpgrade";
			this.cmdUpgrade.Size = new System.Drawing.Size(90, 31);
			this.cmdUpgrade.TabIndex = 2;
			this.cmdUpgrade.Text = "Upgrade";
			this.cmdUpgrade.UseVisualStyleBackColor = true;
			this.cmdUpgrade.Click += new System.EventHandler(this.cmdUpgrade_Click);
			// 
			// txtOutput
			// 
			this.txtOutput.Location = new System.Drawing.Point(12, 38);
			this.txtOutput.Multiline = true;
			this.txtOutput.Name = "txtOutput";
			this.txtOutput.Size = new System.Drawing.Size(372, 162);
			this.txtOutput.TabIndex = 3;
			// 
			// txtDriver
			// 
			this.txtDriver.Location = new System.Drawing.Point(88, 220);
			this.txtDriver.Name = "txtDriver";
			this.txtDriver.Size = new System.Drawing.Size(295, 20);
			this.txtDriver.TabIndex = 4;
			this.txtDriver.TextChanged += new System.EventHandler(this.txtDriver_TextChanged);
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(13, 224);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(69, 13);
			this.label1.TabIndex = 5;
			this.label1.Text = "OldDb driver:";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(204, 270);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(90, 30);
			this.button1.TabIndex = 6;
			this.button1.Text = "Test";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(12, 270);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(90, 30);
			this.button2.TabIndex = 7;
			this.button2.Text = "Export";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// btnImportData
			// 
			this.btnImportData.Location = new System.Drawing.Point(108, 270);
			this.btnImportData.Name = "btnImportData";
			this.btnImportData.Size = new System.Drawing.Size(90, 30);
			this.btnImportData.TabIndex = 8;
			this.btnImportData.Text = "Import Data";
			this.btnImportData.UseVisualStyleBackColor = true;
			this.btnImportData.Click += new System.EventHandler(this.btnImportData_Click);
			// 
			// browseData
			// 
			this.browseData.Filter = "ExcelFile (*.xlsx)|*.xlsx";
			this.browseData.Title = "Import Data";
			// 
			// frmMain
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(396, 313);
			this.Controls.Add(this.btnImportData);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.txtDriver);
			this.Controls.Add(this.txtOutput);
			this.Controls.Add(this.cmdUpgrade);
			this.Controls.Add(this.txtSelectedFile);
			this.Controls.Add(this.btnSelect);
			this.MinimumSize = new System.Drawing.Size(412, 351);
			this.Name = "frmMain";
			this.Text = "SchemaExtension Upgrade";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.Resize += new System.EventHandler(this.Form1_Resize);
			this.ResumeLayout(false);
			this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.OpenFileDialog browseNewSchema;
        private System.Windows.Forms.TextBox txtSelectedFile;
        private System.Windows.Forms.Button cmdUpgrade;
        private System.Windows.Forms.TextBox txtOutput;
        private System.Windows.Forms.TextBox txtDriver;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button btnImportData;
		private System.Windows.Forms.OpenFileDialog browseData;
	}
}

