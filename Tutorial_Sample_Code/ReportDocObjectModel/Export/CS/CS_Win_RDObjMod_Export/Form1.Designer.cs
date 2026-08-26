namespace ExpWinCS
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
            this.exportTypesList = new System.Windows.Forms.ComboBox();
            this.exportByType = new System.Windows.Forms.Button();
            this.message = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // crystalReportViewer
            // 
            this.crystalReportViewer.ActiveViewIndex = -1;
            this.crystalReportViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.crystalReportViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.crystalReportViewer.Location = new System.Drawing.Point(12, 40);
            this.crystalReportViewer.Name = "crystalReportViewer";
            this.crystalReportViewer.SelectionFormula = "";
            this.crystalReportViewer.Size = new System.Drawing.Size(645, 218);
            this.crystalReportViewer.TabIndex = 0;
            this.crystalReportViewer.ViewTimeSelectionFormula = "";
            // 
            // exportTypesList
            // 
            this.exportTypesList.FormattingEnabled = true;
            this.exportTypesList.Location = new System.Drawing.Point(13, 13);
            this.exportTypesList.Name = "exportTypesList";
            this.exportTypesList.Size = new System.Drawing.Size(121, 21);
            this.exportTypesList.TabIndex = 1;
            // 
            // exportByType
            // 
            this.exportByType.Location = new System.Drawing.Point(151, 10);
            this.exportByType.Name = "exportByType";
            this.exportByType.Size = new System.Drawing.Size(163, 23);
            this.exportByType.TabIndex = 2;
            this.exportByType.Text = "Export As Selected Type";
            this.exportByType.UseVisualStyleBackColor = true;
            this.exportByType.Click += new System.EventHandler(this.exportByType_Click);
            // 
            // message
            // 
            this.message.AutoSize = true;
            this.message.Location = new System.Drawing.Point(233, 20);
            this.message.Name = "message";
            this.message.Size = new System.Drawing.Size(0, 13);
            this.message.TabIndex = 3;
            this.message.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(334, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "message";
            this.label1.Visible = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(669, 266);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.message);
            this.Controls.Add(this.exportByType);
            this.Controls.Add(this.exportTypesList);
            this.Controls.Add(this.crystalReportViewer);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CrystalDecisions.Windows.Forms.CrystalReportViewer crystalReportViewer;
        private System.Windows.Forms.ComboBox exportTypesList;
        private System.Windows.Forms.Button exportByType;
        private System.Windows.Forms.Label message;
        private System.Windows.Forms.Label label1;
    }
}

