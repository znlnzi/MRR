namespace MRAnalysis
{
    partial class BaseControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.btnAnalysis = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtDate = new System.Windows.Forms.TextBox();
            this.dgvLog = new System.Windows.Forms.DataGridView();
            this.logBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.lbProgress = new System.Windows.Forms.Label();
            this.progressBarEx1 = new MRAnalysis.ProgressBarEx();
            this.progressBarEx2 = new MRAnalysis.ProgressBarEx();
            ((System.ComponentModel.ISupportInitialize)(this.LogEntityBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLog)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.logBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // btnAnalysis
            // 
            this.btnAnalysis.Location = new System.Drawing.Point(8, 48);
            this.btnAnalysis.Name = "btnAnalysis";
            this.btnAnalysis.Size = new System.Drawing.Size(75, 23);
            this.btnAnalysis.TabIndex = 2;
            this.btnAnalysis.Text = "解析";
            this.btnAnalysis.UseVisualStyleBackColor = true;
            this.btnAnalysis.Click += new System.EventHandler(this.btnAnalysis_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "选择时间:";
            // 
            // txtDate
            // 
            this.txtDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDate.Location = new System.Drawing.Point(68, 15);
            this.txtDate.Name = "txtDate";
            this.txtDate.Size = new System.Drawing.Size(885, 21);
            this.txtDate.TabIndex = 0;
            // 
            // dgvLog
            // 
            this.dgvLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvLog.BackgroundColor = System.Drawing.Color.White;
            this.dgvLog.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvLog.Location = new System.Drawing.Point(5, 110);
            this.dgvLog.Name = "dgvLog";
            this.dgvLog.RowTemplate.Height = 23;
            this.dgvLog.Size = new System.Drawing.Size(948, 0);
            this.dgvLog.TabIndex = 0;
            this.dgvLog.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dgvLog_CellPainting);
            // 
            // logBindingSource
            // 
            this.logBindingSource.DataSource = typeof(MRAnalysis.Model.Log);
            // 
            // lbProgress
            // 
            this.lbProgress.AutoSize = true;
            this.lbProgress.Location = new System.Drawing.Point(473, 53);
            this.lbProgress.Name = "lbProgress";
            this.lbProgress.Size = new System.Drawing.Size(0, 12);
            this.lbProgress.TabIndex = 4;
            this.lbProgress.Visible = false;
            // 
            // progressBarEx1
            // 
            this.progressBarEx1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarEx1.FontColor = System.Drawing.Color.Black;
            this.progressBarEx1.Location = new System.Drawing.Point(89, 48);
            this.progressBarEx1.Name = "progressBarEx1";
            this.progressBarEx1.ShowText = true;
            this.progressBarEx1.Size = new System.Drawing.Size(864, 23);
            this.progressBarEx1.TabIndex = 6;
            // 
            // progressBarEx2
            // 
            this.progressBarEx2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBarEx2.FontColor = System.Drawing.Color.Black;
            this.progressBarEx2.Location = new System.Drawing.Point(8, 81);
            this.progressBarEx2.Name = "progressBarEx2";
            this.progressBarEx2.ShowText = true;
            this.progressBarEx2.Size = new System.Drawing.Size(945, 23);
            this.progressBarEx2.TabIndex = 7;
            // 
            // BaseControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.progressBarEx2);
            this.Controls.Add(this.progressBarEx1);
            this.Controls.Add(this.lbProgress);
            this.Controls.Add(this.dgvLog);
            this.Controls.Add(this.btnAnalysis);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtDate);
            this.Name = "BaseControl";
            this.Size = new System.Drawing.Size(965, 119);
            ((System.ComponentModel.ISupportInitialize)(this.LogEntityBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgvLog)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.logBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.DataGridView dgvLog;
        private System.Windows.Forms.Button btnAnalysis;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtDate;
        private System.Windows.Forms.BindingSource logBindingSource;
        private System.Windows.Forms.Label lbProgress;
        private ProgressBarEx progressBarEx1;
        private ProgressBarEx progressBarEx2;
    }
}
