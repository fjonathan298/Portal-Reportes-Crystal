namespace CS_Win_CRVObjMod_FilteringData
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.selectOperatorList = new System.Windows.Forms.ComboBox();
            this.customerName = new System.Windows.Forms.TextBox();
            this.redisplay = new System.Windows.Forms.Button();
            this.lastYearsSales = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            
            this.crystalReportViewer.ActiveViewIndex = -1;
            this.crystalReportViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.crystalReportViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.crystalReportViewer.Location = new System.Drawing.Point(0, 79);
            this.crystalReportViewer.Name = "crystalReportViewer";
            this.crystalReportViewer.SelectionFormula = "";
            this.crystalReportViewer.Size = new System.Drawing.Size(403, 194);
            this.crystalReportViewer.TabIndex = 0;
            this.crystalReportViewer.ViewTimeSelectionFormula = "";
            this.crystalReportViewer.Load += new System.EventHandler(this.crystalReportViewer_Load);
             
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(225, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Enter the minimum value for last year\'s sales: $";
             
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 34);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(139, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Display the customer names";
             
            this.selectOperatorList.FormattingEnabled = true;
            this.selectOperatorList.Location = new System.Drawing.Point(158, 34);
            this.selectOperatorList.Name = "selectOperatorList";
            this.selectOperatorList.Size = new System.Drawing.Size(121, 21);
            this.selectOperatorList.TabIndex = 4;
            
            this.customerName.Location = new System.Drawing.Point(286, 34);
            this.customerName.Name = "customerName";
            this.customerName.Size = new System.Drawing.Size(100, 20);
            this.customerName.TabIndex = 5;
            this.customerName.Text = "A";
             
            this.redisplay.Location = new System.Drawing.Point(15, 50);
            this.redisplay.Name = "redisplay";
            this.redisplay.Size = new System.Drawing.Size(105, 23);
            this.redisplay.TabIndex = 6;
            this.redisplay.Text = "Redisplay Report";
            this.redisplay.UseVisualStyleBackColor = true;
            this.redisplay.Click += new System.EventHandler(this.redisplay_Click);
             
            this.lastYearsSales.Location = new System.Drawing.Point(243, 6);
            this.lastYearsSales.Name = "lastYearsSales";
            this.lastYearsSales.Size = new System.Drawing.Size(100, 20);
            this.lastYearsSales.TabIndex = 7;
            this.lastYearsSales.Text = "11000.00";
             
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(403, 273);
            this.Controls.Add(this.lastYearsSales);
            this.Controls.Add(this.redisplay);
            this.Controls.Add(this.customerName);
            this.Controls.Add(this.selectOperatorList);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.crystalReportViewer);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CrystalDecisions.Windows.Forms.CrystalReportViewer crystalReportViewer;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox selectOperatorList;
        private System.Windows.Forms.TextBox customerName;
        private System.Windows.Forms.Button redisplay;
        private System.Windows.Forms.TextBox lastYearsSales;
    }
}

