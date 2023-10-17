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
            this.btnAnalyse = new System.Windows.Forms.Button();
            this.txtFeedback = new System.Windows.Forms.TextBox();
            this.txtBreakOnChange = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnExportTargetColumn = new System.Windows.Forms.Button();
            this.lblExportPacketsCount = new System.Windows.Forms.Label();
            this.lblExportFilename = new System.Windows.Forms.Label();
            this.btnExportCSV = new System.Windows.Forms.Button();
            this.cmbExportType = new System.Windows.Forms.ComboBox();
            this.btnAutoTuneExport = new System.Windows.Forms.Button();
            this.btnExportCRCHack = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtLogging
            // 
            this.txtLogging.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLogging.Location = new System.Drawing.Point(843, 10);
            this.txtLogging.Margin = new System.Windows.Forms.Padding(2);
            this.txtLogging.Multiline = true;
            this.txtLogging.Name = "txtLogging";
            this.txtLogging.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLogging.Size = new System.Drawing.Size(344, 434);
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
            // btnAnalyse
            // 
            this.btnAnalyse.Location = new System.Drawing.Point(9, 43);
            this.btnAnalyse.Margin = new System.Windows.Forms.Padding(2);
            this.btnAnalyse.Name = "btnAnalyse";
            this.btnAnalyse.Size = new System.Drawing.Size(203, 28);
            this.btnAnalyse.TabIndex = 4;
            this.btnAnalyse.Text = "Analyse";
            this.btnAnalyse.UseVisualStyleBackColor = true;
            this.btnAnalyse.Click += new System.EventHandler(this.btnAnalyse_Click);
            // 
            // txtFeedback
            // 
            this.txtFeedback.Location = new System.Drawing.Point(432, 10);
            this.txtFeedback.Margin = new System.Windows.Forms.Padding(2);
            this.txtFeedback.Multiline = true;
            this.txtFeedback.Name = "txtFeedback";
            this.txtFeedback.Size = new System.Drawing.Size(408, 434);
            this.txtFeedback.TabIndex = 5;
            // 
            // txtBreakOnChange
            // 
            this.txtBreakOnChange.Location = new System.Drawing.Point(319, 57);
            this.txtBreakOnChange.Margin = new System.Windows.Forms.Padding(2);
            this.txtBreakOnChange.Name = "txtBreakOnChange";
            this.txtBreakOnChange.Size = new System.Drawing.Size(42, 20);
            this.txtBreakOnChange.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(229, 59);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Analysis Column:";
            // 
            // btnExportTargetColumn
            // 
            this.btnExportTargetColumn.Location = new System.Drawing.Point(9, 75);
            this.btnExportTargetColumn.Margin = new System.Windows.Forms.Padding(2);
            this.btnExportTargetColumn.Name = "btnExportTargetColumn";
            this.btnExportTargetColumn.Size = new System.Drawing.Size(203, 28);
            this.btnExportTargetColumn.TabIndex = 8;
            this.btnExportTargetColumn.Text = "Export Analysis WRL";
            this.btnExportTargetColumn.UseVisualStyleBackColor = true;
            this.btnExportTargetColumn.Click += new System.EventHandler(this.btnExportTargetColumn_Click);
            // 
            // lblExportPacketsCount
            // 
            this.lblExportPacketsCount.AutoSize = true;
            this.lblExportPacketsCount.Location = new System.Drawing.Point(229, 88);
            this.lblExportPacketsCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblExportPacketsCount.Name = "lblExportPacketsCount";
            this.lblExportPacketsCount.Size = new System.Drawing.Size(109, 13);
            this.lblExportPacketsCount.TabIndex = 9;
            this.lblExportPacketsCount.Text = "No packets to export.";
            // 
            // lblExportFilename
            // 
            this.lblExportFilename.AutoSize = true;
            this.lblExportFilename.Location = new System.Drawing.Point(229, 115);
            this.lblExportFilename.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.lblExportFilename.Name = "lblExportFilename";
            this.lblExportFilename.Size = new System.Drawing.Size(132, 13);
            this.lblExportFilename.TabIndex = 10;
            this.lblExportFilename.Text = "Export Filename undefined";
            // 
            // btnExportCSV
            // 
            this.btnExportCSV.Location = new System.Drawing.Point(9, 139);
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
            this.cmbExportType.Location = new System.Drawing.Point(232, 188);
            this.cmbExportType.Name = "cmbExportType";
            this.cmbExportType.Size = new System.Drawing.Size(129, 21);
            this.cmbExportType.TabIndex = 12;
            this.cmbExportType.SelectedIndexChanged += new System.EventHandler(this.cmbExportType_SelectedIndexChanged);
            // 
            // btnAutoTuneExport
            // 
            this.btnAutoTuneExport.Location = new System.Drawing.Point(9, 107);
            this.btnAutoTuneExport.Margin = new System.Windows.Forms.Padding(2);
            this.btnAutoTuneExport.Name = "btnAutoTuneExport";
            this.btnAutoTuneExport.Size = new System.Drawing.Size(203, 28);
            this.btnAutoTuneExport.TabIndex = 13;
            this.btnAutoTuneExport.Text = "Export Filtered for Autotune";
            this.btnAutoTuneExport.UseVisualStyleBackColor = true;
            this.btnAutoTuneExport.Click += new System.EventHandler(this.btnAutoTuneExport_Click);
            // 
            // btnExportCRCHack
            // 
            this.btnExportCRCHack.Location = new System.Drawing.Point(9, 218);
            this.btnExportCRCHack.Margin = new System.Windows.Forms.Padding(2);
            this.btnExportCRCHack.Name = "btnExportCRCHack";
            this.btnExportCRCHack.Size = new System.Drawing.Size(140, 26);
            this.btnExportCRCHack.TabIndex = 14;
            this.btnExportCRCHack.Text = "Export WRL CRC";
            this.btnExportCRCHack.UseVisualStyleBackColor = true;
            this.btnExportCRCHack.Visible = false;
            this.btnExportCRCHack.Click += new System.EventHandler(this.btnExportCRCHack_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(159, 191);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(67, 13);
            this.label2.TabIndex = 15;
            this.label2.Text = "Export Type:";
            this.label2.Click += new System.EventHandler(this.label2_Click);
            // 
            // WoolichFileDecoderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1196, 586);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnExportCRCHack);
            this.Controls.Add(this.btnAutoTuneExport);
            this.Controls.Add(this.cmbExportType);
            this.Controls.Add(this.btnExportCSV);
            this.Controls.Add(this.lblExportFilename);
            this.Controls.Add(this.lblExportPacketsCount);
            this.Controls.Add(this.btnExportTargetColumn);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtBreakOnChange);
            this.Controls.Add(this.txtFeedback);
            this.Controls.Add(this.btnAnalyse);
            this.Controls.Add(this.lblFileName);
            this.Controls.Add(this.btnOpenFile);
            this.Controls.Add(this.txtLogging);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MinimumSize = new System.Drawing.Size(1129, 625);
            this.Name = "WoolichFileDecoderForm";
            this.Text = "Woolich File Decoder";
            this.Load += new System.EventHandler(this.WoolichFileDecoder_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txtLogging;
        private System.Windows.Forms.Button btnOpenFile;
        private System.Windows.Forms.OpenFileDialog openWRLFileDialog;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Button btnAnalyse;
        private System.Windows.Forms.TextBox txtFeedback;
        private System.Windows.Forms.TextBox txtBreakOnChange;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnExportTargetColumn;
        private System.Windows.Forms.Label lblExportPacketsCount;
        private System.Windows.Forms.Label lblExportFilename;
        private System.Windows.Forms.Button btnExportCSV;
        private System.Windows.Forms.ComboBox cmbExportType;
        private System.Windows.Forms.Button btnAutoTuneExport;
        private System.Windows.Forms.Button btnExportCRCHack;
        private System.Windows.Forms.Label label2;
    }
}

