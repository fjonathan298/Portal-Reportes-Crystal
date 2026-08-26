namespace CS_Win_ReducedCode_ReportData
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
            this.drillLabel = new System.Windows.Forms.Label();
            this.crystalReportViewer = new CrystalDecisions.Windows.Forms.CrystalReportViewer();
            this.World_Sales_Report1 = new CS_Win_ReducedCode_ReportData.World_Sales_Report();
            this.SuspendLayout();
            // 
            // drillLabel
            // 
            this.drillLabel.AutoSize = true;
            this.drillLabel.Location = new System.Drawing.Point(13, 13);
            this.drillLabel.Name = "drillLabel";
            this.drillLabel.Size = new System.Drawing.Size(35, 13);
            this.drillLabel.TabIndex = 0;
            this.drillLabel.Text = "label1";
            // 
            // crystalReportViewer
            // 
            this.crystalReportViewer.ActiveViewIndex = 0;
            this.crystalReportViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.crystalReportViewer.Cursor = System.Windows.Forms.Cursors.Default;
            this.crystalReportViewer.Location = new System.Drawing.Point(91, 20);
            this.crystalReportViewer.Name = "crystalReportViewer";
            this.crystalReportViewer.ReportSource = this.World_Sales_Report1;
            this.crystalReportViewer.Size = new System.Drawing.Size(1072, 710);
            this.crystalReportViewer.TabIndex = 1;
            this.crystalReportViewer.Drill += new CrystalDecisions.Windows.Forms.DrillEventHandler(this.crystalReportViewer_Drill);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1284, 729);
            this.Controls.Add(this.crystalReportViewer);
            this.Controls.Add(this.drillLabel);
            this.Name = "Form1";
            this.Text = "Form1";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion


        private System.Windows.Forms.Label drillLabel;
        private CrystalDecisions.Windows.Forms.CrystalReportViewer crystalReportViewer;
        private World_Sales_Report World_Sales_Report1;
    }
}

