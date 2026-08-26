namespace CS_Win_CRVObjMod_ParametersSubrpt
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
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.orderStartDate = new System.Windows.Forms.TextBox();
            this.orderEndDate = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // crystalReportViewer
            // 
            this.crystalReportViewer.ActiveViewIndex = -1;
            this.crystalReportViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.crystalReportViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.crystalReportViewer.Location = new System.Drawing.Point(0, 87);
            this.crystalReportViewer.Name = "crystalReportViewer";
            this.crystalReportViewer.SelectionFormula = "";
            this.crystalReportViewer.Size = new System.Drawing.Size(400, 186);
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
            this.redisplay.Location = new System.Drawing.Point(216, 58);
            this.redisplay.Name = "redisplay";
            this.redisplay.Size = new System.Drawing.Size(113, 23);
            this.redisplay.TabIndex = 2;
            this.redisplay.Text = "Redisplay Report";
            this.redisplay.UseVisualStyleBackColor = true;
            this.redisplay.Click += new System.EventHandler(this.redisplay_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(213, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(84, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Order Start Date";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(213, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(81, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Order End Date";
            // 
            // orderStartDate
            // 
            this.orderStartDate.Location = new System.Drawing.Point(300, 6);
            this.orderStartDate.Name = "orderStartDate";
            this.orderStartDate.Size = new System.Drawing.Size(100, 20);
            this.orderStartDate.TabIndex = 5;
            // 
            // orderEndDate
            // 
            this.orderEndDate.Location = new System.Drawing.Point(300, 32);
            this.orderEndDate.Name = "orderEndDate";
            this.orderEndDate.Size = new System.Drawing.Size(100, 20);
            this.orderEndDate.TabIndex = 6;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(400, 273);
            this.Controls.Add(this.orderEndDate);
            this.Controls.Add(this.orderStartDate);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.redisplay);
            this.Controls.Add(this.defaultParameterValuesList);
            this.Controls.Add(this.crystalReportViewer);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private CrystalDecisions.Windows.Forms.CrystalReportViewer crystalReportViewer;
        private System.Windows.Forms.ListBox defaultParameterValuesList;
        private System.Windows.Forms.Button redisplay;

     
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox orderStartDate;
        private System.Windows.Forms.TextBox orderEndDate;
    }
}

