namespace MRAnalysis
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.analysisMrs1 = new MRAnalysis.AnalysisMrs();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.analysisMro1 = new MRAnalysis.AnalysisMro();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1022, 605);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.analysisMrs1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(1014, 579);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "MRS解析";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // analysisMrs1
            // 
            this.analysisMrs1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.analysisMrs1.Location = new System.Drawing.Point(3, 3);
            this.analysisMrs1.LogEntities = ((System.Collections.Generic.IList<MRAnalysis.Model.Log>)(resources.GetObject("analysisMrs1.LogEntities")));
            this.analysisMrs1.Name = "analysisMrs1";
            this.analysisMrs1.Size = new System.Drawing.Size(1008, 573);
            this.analysisMrs1.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.analysisMro1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(1014, 510);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "MRO解析";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // analysisMro1
            // 
            this.analysisMro1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.analysisMro1.Location = new System.Drawing.Point(3, 3);
            this.analysisMro1.LogEntities = ((System.Collections.Generic.IList<MRAnalysis.Model.Log>)(resources.GetObject("analysisMro1.LogEntities")));
            this.analysisMro1.Name = "analysisMro1";
            this.analysisMro1.Size = new System.Drawing.Size(1008, 504);
            this.analysisMro1.TabIndex = 0;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1022, 605);
            this.Controls.Add(this.tabControl1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainForm";
            this.Text = "解析工具";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private AnalysisMrs analysisMrs1;
        private System.Windows.Forms.TabPage tabPage2;
        private AnalysisMro analysisMro1;
    }
}

