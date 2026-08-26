namespace CS_Base
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
            this.defaultParameterValuesList = new System.Windows.Forms.ListBox();
            this.redisplay = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // crystalReportViewer
            // 
            this.crystalReportViewer.ActiveViewIndex = -1;
            this.crystalReportViewer.AutoScroll = true;
            this.crystalReportViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.crystalReportViewer.Cursor = System.Windows.Forms.Cursors.Default;
            this.crystalReportViewer.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.crystalReportViewer.Location = new System.Drawing.Point(0, 104);
            this.crystalReportViewer.Name = "crystalReportViewer";
            this.crystalReportViewer.SelectionFormula = "";
            this.crystalReportViewer.Size = new System.Drawing.Size(803, 400);
            this.crystalReportViewer.TabIndex = 0;
            this.crystalReportViewer.ViewTimeSelectionFormula = "";
            this.crystalReportViewer.Load += new System.EventHandler(this.crystalReportViewer_Load);
            // 
            // defaultParameterValuesList
            // 
            this.defaultParameterValuesList.FormattingEnabled = true;
            this.defaultParameterValuesList.Location = new System.Drawing.Point(0, 12);
            this.defaultParameterValuesList.Name = "defaultParameterValuesList";
            this.defaultParameterValuesList.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.defaultParameterValuesList.Size = new System.Drawing.Size(210, 69);
            this.defaultParameterValuesList.TabIndex = 1;
            // 
            // redisplay
            // 
            this.redisplay.Location = new System.Drawing.Point(216, 12);
            this.redisplay.Name = "redisplay";
            this.redisplay.Size = new System.Drawing.Size(132, 24);
            this.redisplay.TabIndex = 2;
            this.redisplay.Text = "Redisplay Report";
            this.redisplay.UseVisualStyleBackColor = true;
            this.redisplay.Click += new System.EventHandler(this.redisplay_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(803, 504);
            this.Controls.Add(this.crystalReportViewer);
            this.Controls.Add(this.redisplay);
            this.Controls.Add(this.defaultParameterValuesList);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private CrystalDecisions.Windows.Forms.CrystalReportViewer crystalReportViewer;
        private System.Windows.Forms.ListBox defaultParameterValuesList;
        private System.Windows.Forms.Button redisplay;
    }
}

