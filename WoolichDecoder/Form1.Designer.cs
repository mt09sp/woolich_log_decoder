namespace WoolichDecoder
{
    partial class WoolichFileDecoderForm
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
            this.txtLogging = new System.Windows.Forms.TextBox();
            this.btnOpenFile = new System.Windows.Forms.Button();
            this.openWRLFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.lblFileName = new System.Windows.Forms.Label();
            this.txtFeedback = new System.Windows.Forms.TextBox();
            this.lblExportPacketsCount = new System.Windows.Forms.Label();
            this.lblExportFilename = new System.Windows.Forms.Label();
            this.btnExportCSV = new System.Windows.Forms.Button();
            this.cmbExportType = new System.Windows.Forms.ComboBox();
            this.btnAutoTuneExport = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnExportCRCHack = new System.Windows.Forms.Button();
            this.btnExportTargetColumn = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtBreakOnChange = new System.Windows.Forms.TextBox();
            this.btnAnalyse = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.aTFCheckedListBox = new System.Windows.Forms.CheckedListBox();
            this.label4 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtLogging
            // 
            this.txtLogging.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLogging.Location = new System.Drawing.Point(843, 59);
            this.txtLogging.Margin = new System.Windows.Forms.Padding(2);
            this.txtLogging.Multiline = true;
            this.txtLogging.Name = "txtLogging";
            this.txtLogging.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLogging.Size = new System.Drawing.Size(344, 516);
            this.txtLogging.TabIndex = 1;
            // 
            // btnOpenFile
            // 
            this.btnOpenFile.Location = new System.Drawing.Point(9, 11);
            this.btnOpenFile.Margin = new System.Windows.Forms.Padding(2);
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new System.Drawing.Size(203, 28);
            this.btnOpenFile.TabIndex = 2;
            this.btnOpenFile.Text = "Open File";
            this.btnOpenFile.UseVisualStyleBackColor = true;
            this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
            // 
            // openWRLFileDialog
            // 
            this.openWRLFileDialog.AddExtension = false;
            this.openWRLFileDialog.DefaultExt = "WRL";
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Location = new System.Drawing.Point(229, 25);
            this.lblFileName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(85, 13);
            this.lblFileName.TabIndex = 3;
            this.lblFileName.Text = "No File Selected";
            // 
            // txtFeedback
            // 
            this.txtFeedback.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFeedback.Location = new System.Drawing.Point(432, 59);
            this.txtFeedback.Margin = new System.Windows.Forms.Padding(2);
            this.txtFeedback.Multiline = true;
            this.txtFeedback.Name = "txtFeedback";
            this.txtFeedback.Size = new System.Drawing.Size(407, 516);
            this.txtFeedback.TabIndex = 5;
            // 
            // lblExportPacketsCount
            // 
            this.lblExportPacketsCount.AutoSize = true;
            this.lblExportPacketsCount.Location = new System.Drawing.Point(14, 59);
            this.lblExportPacketsCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblExportPacketsCount.Name = "lblExportPacketsCount";
            this.lblExportPacketsCount.Size = new System.Drawing.Size(109, 13);
            this.lblExportPacketsCount.TabIndex = 9;
            this.lblExportPacketsCount.Text = "No packets to export.";
            // 
            // lblExportFilename
            // 
            this.lblExportFilename.AutoSize = true;
            this.lblExportFilename.Location = new System.Drawing.Point(438, 25);
            this.lblExportFilename.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblExportFilename.Name = "lblExportFilename";
            this.lblExportFilename.Size = new System.Drawing.Size(132, 13);
            this.lblExportFilename.TabIndex = 10;
            this.lblExportFilename.Text = "Export Filename undefined";
            // 
            // btnExportCSV
            // 
            this.btnExportCSV.Location = new System.Drawing.Point(17, 287);
            this.btnExportCSV.Margin = new System.Windows.Forms.Padding(2);
            this.btnExportCSV.Name = "btnExportCSV";
            this.btnExportCSV.Size = new System.Drawing.Size(203, 28);
            this.btnExportCSV.TabIndex = 11;
            this.btnExportCSV.Text = "Export CSV";
            this.btnExportCSV.UseVisualStyleBackColor = true;
            this.btnExportCSV.Click += new System.EventHandler(this.btnExportCSV_Click);
            // 
            // cmbExportType
            // 
            this.cmbExportType.FormattingEnabled = true;
            this.cmbExportType.Items.AddRange(new object[] {
            "Export Full File",
            "Export Analysis Only"});
            this.cmbExportType.Location = new System.Drawing.Point(111, 252);
            this.cmbExportType.Name = "cmbExportType";
            this.cmbExportType.Size = new System.Drawing.Size(129, 21);
            this.cmbExportType.TabIndex = 12;
            this.cmbExportType.SelectedIndexChanged += new System.EventHandler(this.cmbExportType_SelectedIndexChanged);
            // 
            // btnAutoTuneExport
            // 
            this.btnAutoTuneExport.BackColor = System.Drawing.Color.DarkSeaGreen;
            this.btnAutoTuneExport.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAutoTuneExport.Location = new System.Drawing.Point(301, 108);
            this.btnAutoTuneExport.Margin = new System.Windows.Forms.Padding(2);
            this.btnAutoTuneExport.Name = "btnAutoTuneExport";
            this.btnAutoTuneExport.Size = new System.Drawing.Size(126, 94);
            this.btnAutoTuneExport.TabIndex = 13;
            this.btnAutoTuneExport.Text = "Export Filtered for Autotune";
            this.btnAutoTuneExport.UseVisualStyleBackColor = false;
            this.btnAutoTuneExport.Click += new System.EventHandler(this.btnAutoTuneExport_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 255);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(91, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "CSV Export Type:";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.btnExportCRCHack);
            this.panel1.Controls.Add(this.btnExportTargetColumn);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.txtBreakOnChange);
            this.panel1.Controls.Add(this.btnAnalyse);
            this.panel1.Location = new System.Drawing.Point(12, 407);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(415, 167);
            this.panel1.TabIndex = 16;
            // 
            // btnExportCRCHack
            // 
            this.btnExportCRCHack.Location = new System.Drawing.Point(11, 123);
            this.btnExportCRCHack.Margin = new System.Windows.Forms.Padding(2);
            this.btnExportCRCHack.Name = "btnExportCRCHack";
            this.btnExportCRCHack.Size = new System.Drawing.Size(140, 26);
            this.btnExportCRCHack.TabIndex = 19;
            this.btnExportCRCHack.Text = "Export WRL CRC";
            this.btnExportCRCHack.UseVisualStyleBackColor = true;
            this.btnExportCRCHack.Visible = false;
            // 
            // btnExportTargetColumn
            // 
            this.btnExportTargetColumn.Location = new System.Drawing.Point(11, 81);
            this.btnExportTargetColumn.Margin = new System.Windows.Forms.Padding(2);
            this.btnExportTargetColumn.Name = "btnExportTargetColumn";
            this.btnExportTargetColumn.Size = new System.Drawing.Size(203, 28);
            this.btnExportTargetColumn.TabIndex = 18;
            this.btnExportTargetColumn.Text = "Export Analysis WRL";
            this.btnExportTargetColumn.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(231, 57);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 17;
            this.label1.Text = "Analysis Column:";
            // 
            // txtBreakOnChange
            // 
            this.txtBreakOnChange.Location = new System.Drawing.Point(321, 54);
            this.txtBreakOnChange.Margin = new System.Windows.Forms.Padding(2);
            this.txtBreakOnChange.Name = "txtBreakOnChange";
            this.txtBreakOnChange.Size = new System.Drawing.Size(42, 20);
            this.txtBreakOnChange.TabIndex = 16;
            // 
            // btnAnalyse
            // 
            this.btnAnalyse.Location = new System.Drawing.Point(11, 49);
            this.btnAnalyse.Margin = new System.Windows.Forms.Padding(2);
            this.btnAnalyse.Name = "btnAnalyse";
            this.btnAnalyse.Size = new System.Drawing.Size(203, 28);
            this.btnAnalyse.TabIndex = 15;
            this.btnAnalyse.Text = "Analyse";
            this.btnAnalyse.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(323, 13);
            this.label3.TabIndex = 20;
            this.label3.Text = "Log File Analysis Functions (To analyse a particular packet column)";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // aTFCheckedListBox
            // 
            this.aTFCheckedListBox.FormattingEnabled = true;
            this.aTFCheckedListBox.Location = new System.Drawing.Point(12, 108);
            this.aTFCheckedListBox.Name = "aTFCheckedListBox";
            this.aTFCheckedListBox.Size = new System.Drawing.Size(284, 109);
            this.aTFCheckedListBox.TabIndex = 17;
            this.aTFCheckedListBox.SelectedIndexChanged += new System.EventHandler(this.aTFCheckedListBox_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 89);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(122, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Autotune export options ";
            // 
            // WoolichFileDecoderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1196, 586);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.aTFCheckedListBox);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnAutoTuneExport);
            this.Controls.Add(this.cmbExportType);
            this.Controls.Add(this.btnExportCSV);
            this.Controls.Add(this.lblExportFilename);
            this.Controls.Add(this.lblExportPacketsCount);
            this.Controls.Add(this.txtFeedback);
            this.Controls.Add(this.lblFileName);
            this.Controls.Add(this.btnOpenFile);
            this.Controls.Add(this.txtLogging);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(1129, 625);
            this.Name = "WoolichFileDecoderForm";
            this.Text = "Woolich File Decoder";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.WoolichFileDecoderForm_FormClosing);
            this.Load += new System.EventHandler(this.WoolichFileDecoder_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtLogging;
        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.OpenFileDialog openWRLFileDialog;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.TextBox txtFeedback;
        private System.Windows.Forms.Label lblExportPacketsCount;
        private System.Windows.Forms.Label lblExportFilename;
        private System.Windows.Forms.Button btnExportCSV;
        private System.Windows.Forms.ComboBox cmbExportType;
        private System.Windows.Forms.Button btnAutoTuneExport;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button btnExportCRCHack;
        private System.Windows.Forms.Button btnExportTargetColumn;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtBreakOnChange;
        private System.Windows.Forms.Button btnAnalyse;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckedListBox aTFCheckedListBox;
        private System.Windows.Forms.Label label4;
    }
}

