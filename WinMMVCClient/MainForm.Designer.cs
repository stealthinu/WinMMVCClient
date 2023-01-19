namespace WinMMVCClient
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.comboBoxInput = new System.Windows.Forms.ComboBox();
            this.comboBoxOutput = new System.Windows.Forms.ComboBox();
            this.labelInput = new System.Windows.Forms.Label();
            this.labelOutput = new System.Windows.Forms.Label();
            this.buttonStart = new System.Windows.Forms.Button();
            this.plotViewWave = new OxyPlot.WindowsForms.PlotView();
            this.buttonStop = new System.Windows.Forms.Button();
            this.listBoxTarget = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // comboBoxInput
            // 
            this.comboBoxInput.FormattingEnabled = true;
            this.comboBoxInput.Location = new System.Drawing.Point(11, 35);
            this.comboBoxInput.Name = "comboBoxInput";
            this.comboBoxInput.Size = new System.Drawing.Size(300, 28);
            this.comboBoxInput.TabIndex = 0;
            // 
            // comboBoxOutput
            // 
            this.comboBoxOutput.FormattingEnabled = true;
            this.comboBoxOutput.Location = new System.Drawing.Point(317, 35);
            this.comboBoxOutput.Name = "comboBoxOutput";
            this.comboBoxOutput.Size = new System.Drawing.Size(300, 28);
            this.comboBoxOutput.TabIndex = 1;
            // 
            // labelInput
            // 
            this.labelInput.AutoSize = true;
            this.labelInput.Location = new System.Drawing.Point(11, 9);
            this.labelInput.Name = "labelInput";
            this.labelInput.Size = new System.Drawing.Size(43, 20);
            this.labelInput.TabIndex = 3;
            this.labelInput.Text = "Input";
            // 
            // labelOutput
            // 
            this.labelOutput.AutoSize = true;
            this.labelOutput.Location = new System.Drawing.Point(317, 9);
            this.labelOutput.Name = "labelOutput";
            this.labelOutput.Size = new System.Drawing.Size(55, 20);
            this.labelOutput.TabIndex = 4;
            this.labelOutput.Text = "Output";
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(14, 396);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(117, 43);
            this.buttonStart.TabIndex = 5;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // plotViewWave
            // 
            this.plotViewWave.Location = new System.Drawing.Point(317, 71);
            this.plotViewWave.Name = "plotViewWave";
            this.plotViewWave.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotViewWave.Size = new System.Drawing.Size(300, 302);
            this.plotViewWave.TabIndex = 6;
            this.plotViewWave.Text = "plotViewWave";
            this.plotViewWave.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotViewWave.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotViewWave.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(158, 398);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(121, 41);
            this.buttonStop.TabIndex = 7;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // listBoxTarget
            // 
            this.listBoxTarget.FormattingEnabled = true;
            this.listBoxTarget.ItemHeight = 20;
            this.listBoxTarget.Location = new System.Drawing.Point(11, 69);
            this.listBoxTarget.Name = "listBoxTarget";
            this.listBoxTarget.Size = new System.Drawing.Size(300, 304);
            this.listBoxTarget.TabIndex = 8;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 451);
            this.Controls.Add(this.listBoxTarget);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.plotViewWave);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.labelOutput);
            this.Controls.Add(this.labelInput);
            this.Controls.Add(this.comboBoxOutput);
            this.Controls.Add(this.comboBoxInput);
            this.Name = "MainForm";
            this.Text = "MMVC Client";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComboBox comboBoxInput;
        private ComboBox comboBoxOutput;
        private Label labelInput;
        private Label labelOutput;
        private Button buttonStart;
        private OxyPlot.WindowsForms.PlotView plotViewWave;
        private Button buttonStop;
        private ListBox listBoxTarget;
    }
}