namespace CS_Win_CRVObjMod_CustomizeViewer
{
    partial class Form1
    {

        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code


        private void InitializeComponent()
        {
            this.crystalReportViewer = new CrystalDecisions.Windows.Forms.CrystalReportViewer();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.listCRVReport = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.listCRVToolbar = new System.Windows.Forms.ListBox();
            this.redisplay = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.selectBackColor = new System.Windows.Forms.ComboBox();
            this.pageNumber = new System.Windows.Forms.TextBox();
            this.goToPage = new System.Windows.Forms.Button();
            this.searchText = new System.Windows.Forms.TextBox();
            this.search = new System.Windows.Forms.Button();
            this.message = new System.Windows.Forms.Label();
            this.zoomFactor = new System.Windows.Forms.TextBox();
            this.updateZoomFactor = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // crystalReportViewer
            // 
            this.crystalReportViewer.ActiveViewIndex = -1;
            this.crystalReportViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.crystalReportViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.crystalReportViewer.Location = new System.Drawing.Point(0, 188);
            this.crystalReportViewer.Name = "crystalReportViewer";
            this.crystalReportViewer.SelectionFormula = "";
            this.crystalReportViewer.Size = new System.Drawing.Size(629, 262);
            this.crystalReportViewer.TabIndex = 0;
            this.crystalReportViewer.ViewTimeSelectionFormula = "";
            this.crystalReportViewer.Load += new System.EventHandler(this.crystalReportViewer_Load);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25F));
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.listCRVReport, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label2, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.listCRVToolbar, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.redisplay, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.label3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.selectBackColor, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.pageNumber, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.goToPage, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.searchText, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.search, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.message, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.zoomFactor, 2, 3);
            this.tableLayoutPanel1.Controls.Add(this.updateZoomFactor, 3, 3);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, -2);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 5;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 40F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 15F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(629, 191);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(127, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "Select report elements to display";
            // 
            // listCRVReport
            // 
            this.listCRVReport.FormattingEnabled = true;
            this.listCRVReport.Location = new System.Drawing.Point(160, 3);
            this.listCRVReport.Name = "listCRVReport";
            this.listCRVReport.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listCRVReport.Size = new System.Drawing.Size(120, 69);
            this.listCRVReport.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(317, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(132, 26);
            this.label2.TabIndex = 2;
            this.label2.Text = "Select toolbar elements to display";
            // 
            // listCRVToolbar
            // 
            this.listCRVToolbar.FormattingEnabled = true;
            this.listCRVToolbar.Location = new System.Drawing.Point(474, 3);
            this.listCRVToolbar.Name = "listCRVToolbar";
            this.listCRVToolbar.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.listCRVToolbar.Size = new System.Drawing.Size(120, 69);
            this.listCRVToolbar.TabIndex = 3;
            // 
            // redisplay
            // 
            this.redisplay.Location = new System.Drawing.Point(3, 107);
            this.redisplay.Name = "redisplay";
            this.redisplay.Size = new System.Drawing.Size(109, 22);
            this.redisplay.TabIndex = 4;
            this.redisplay.Text = "Redisplay Report";
            this.redisplay.UseVisualStyleBackColor = true;
            this.redisplay.Click += new System.EventHandler(this.redisplay_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 76);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(123, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Select background color";
            // 
            // selectBackColor
            // 
            this.selectBackColor.FormattingEnabled = true;
            this.selectBackColor.Location = new System.Drawing.Point(160, 79);
            this.selectBackColor.Name = "selectBackColor";
            this.selectBackColor.Size = new System.Drawing.Size(121, 21);
            this.selectBackColor.TabIndex = 6;
            // 
            // pageNumber
            // 
            this.pageNumber.Location = new System.Drawing.Point(3, 135);
            this.pageNumber.Name = "pageNumber";
            this.pageNumber.Size = new System.Drawing.Size(100, 20);
            this.pageNumber.TabIndex = 7;
            // 
            // goToPage
            // 
            this.goToPage.Location = new System.Drawing.Point(160, 135);
            this.goToPage.Name = "goToPage";
            this.goToPage.Size = new System.Drawing.Size(75, 22);
            this.goToPage.TabIndex = 8;
            this.goToPage.Text = "Go to Page";
            this.goToPage.UseVisualStyleBackColor = true;
            this.goToPage.Click += new System.EventHandler(this.goToPage_Click);
            // 
            // searchText
            // 
            this.searchText.Location = new System.Drawing.Point(3, 163);
            this.searchText.Name = "searchText";
            this.searchText.Size = new System.Drawing.Size(100, 20);
            this.searchText.TabIndex = 9;
            // 
            // search
            // 
            this.search.Location = new System.Drawing.Point(160, 163);
            this.search.Name = "search";
            this.search.Size = new System.Drawing.Size(96, 23);
            this.search.TabIndex = 10;
            this.search.Text = "Search For Text";
            this.search.UseVisualStyleBackColor = true;
            this.search.Click += new System.EventHandler(this.search_Click);
            // 
            // message
            // 
            this.message.AutoSize = true;
            this.message.ForeColor = System.Drawing.Color.Red;
            this.message.Location = new System.Drawing.Point(317, 160);
            this.message.Name = "message";
            this.message.Size = new System.Drawing.Size(0, 13);
            this.message.TabIndex = 11;
            // 
            // zoomFactor
            // 
            this.zoomFactor.Location = new System.Drawing.Point(317, 135);
            this.zoomFactor.Name = "zoomFactor";
            this.zoomFactor.Size = new System.Drawing.Size(100, 20);
            this.zoomFactor.TabIndex = 12;
            // 
            // updateZoomFactor
            // 
            this.updateZoomFactor.Location = new System.Drawing.Point(474, 135);
            this.updateZoomFactor.Name = "updateZoomFactor";
            this.updateZoomFactor.Size = new System.Drawing.Size(75, 22);
            this.updateZoomFactor.TabIndex = 13;
            this.updateZoomFactor.Text = "% Zoom";
            this.updateZoomFactor.UseVisualStyleBackColor = true;
            this.updateZoomFactor.Click += new System.EventHandler(this.updateZoomFactor_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(629, 448);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.crystalReportViewer);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private CrystalDecisions.Windows.Forms.CrystalReportViewer crystalReportViewer;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox listCRVReport;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListBox listCRVToolbar;
        private System.Windows.Forms.Button redisplay;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox selectBackColor;
        private System.Windows.Forms.TextBox pageNumber;
        private System.Windows.Forms.Button goToPage;
        private System.Windows.Forms.TextBox searchText;
        private System.Windows.Forms.Button search;
        private System.Windows.Forms.Label message;
        private System.Windows.Forms.TextBox zoomFactor;
        private System.Windows.Forms.Button updateZoomFactor;
    }
}

