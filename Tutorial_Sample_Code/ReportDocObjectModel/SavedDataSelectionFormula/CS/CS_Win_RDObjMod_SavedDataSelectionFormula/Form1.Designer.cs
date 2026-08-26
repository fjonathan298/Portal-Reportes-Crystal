namespace CS_Win_RDObjMod_SavedDataSelectionFormula
{
    partial class Form1
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
            this.crystalReportViewer = new CrystalDecisions.Windows.Forms.CrystalReportViewer();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lastYearsSales = new System.Windows.Forms.TextBox();
            this.operatorValueList = new System.Windows.Forms.ComboBox();
            this.letterOfName = new System.Windows.Forms.TextBox();
            this.redisplay = new System.Windows.Forms.Button();
            this.formula = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // crystalReportViewer
            // 
            this.crystalReportViewer.ActiveViewIndex = -1;
            this.crystalReportViewer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.crystalReportViewer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.crystalReportViewer.Cursor = System.Windows.Forms.Cursors.Default;
            this.crystalReportViewer.Location = new System.Drawing.Point(0, 115);
            this.crystalReportViewer.Name = "crystalReportViewer";
            this.crystalReportViewer.Size = new System.Drawing.Size(514, 238);
            this.crystalReportViewer.TabIndex = 0;
            this.crystalReportViewer.ToolPanelView = CrystalDecisions.Windows.Forms.ToolPanelViewType.None;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-3, -1);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(157, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Display the following customers:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-3, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(104, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "- last year\'s sales > $";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(-5, 36);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 13);
            this.label3.TabIndex = 3;
            this.label3.Text = "- first letter of name is";
            // 
            // lastYearsSales
            // 
            this.lastYearsSales.Location = new System.Drawing.Point(107, 13);
            this.lastYearsSales.Name = "lastYearsSales";
            this.lastYearsSales.Size = new System.Drawing.Size(89, 20);
            this.lastYearsSales.TabIndex = 4;
            // 
            // operatorValueList
            // 
            this.operatorValueList.FormattingEnabled = true;
            this.operatorValueList.Location = new System.Drawing.Point(107, 33);
            this.operatorValueList.Name = "operatorValueList";
            this.operatorValueList.Size = new System.Drawing.Size(89, 21);
            this.operatorValueList.TabIndex = 5;
            // 
            // letterOfName
            // 
            this.letterOfName.Location = new System.Drawing.Point(202, 33);
            this.letterOfName.Name = "letterOfName";
            this.letterOfName.Size = new System.Drawing.Size(90, 20);
            this.letterOfName.TabIndex = 6;
            // 
            // redisplay
            // 
            this.redisplay.Location = new System.Drawing.Point(202, 7);
            this.redisplay.Name = "redisplay";
            this.redisplay.Size = new System.Drawing.Size(114, 22);
            this.redisplay.TabIndex = 7;
            this.redisplay.Text = "Redisplay Report";
            this.redisplay.UseVisualStyleBackColor = true;
            this.redisplay.Click += new System.EventHandler(this.redisplay_Click);
            // 
            // formula
            // 
            this.formula.AutoSize = true;
            this.formula.Location = new System.Drawing.Point(12, 67);
            this.formula.Name = "formula";
            this.formula.Size = new System.Drawing.Size(0, 13);
            this.formula.TabIndex = 8;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 350);
            this.Controls.Add(this.formula);
            this.Controls.Add(this.redisplay);
            this.Controls.Add(this.letterOfName);
            this.Controls.Add(this.operatorValueList);
            this.Controls.Add(this.lastYearsSales);
            this.Controls.Add(this.label3);
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
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox lastYearsSales;
        private System.Windows.Forms.ComboBox operatorValueList;
        private System.Windows.Forms.TextBox letterOfName;
        private System.Windows.Forms.Button redisplay;
        private System.Windows.Forms.Label formula;
    }
}

