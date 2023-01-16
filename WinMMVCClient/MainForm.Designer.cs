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
            this.plotViewSpectrogram = new OxyPlot.WindowsForms.PlotView();
            this.SuspendLayout();
            // 
            // comboBoxInput
            // 
            this.comboBoxInput.FormattingEnabled = true;
            this.comboBoxInput.Location = new System.Drawing.Point(10, 26);
            this.comboBoxInput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBoxInput.Name = "comboBoxInput";
            this.comboBoxInput.Size = new System.Drawing.Size(235, 23);
            this.comboBoxInput.TabIndex = 0;
            // 
            // comboBoxOutput
            // 
            this.comboBoxOutput.FormattingEnabled = true;
            this.comboBoxOutput.Location = new System.Drawing.Point(250, 26);
            this.comboBoxOutput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBoxOutput.Name = "comboBoxOutput";
            this.comboBoxOutput.Size = new System.Drawing.Size(261, 23);
            this.comboBoxOutput.TabIndex = 1;
            // 
            // labelInput
            // 
            this.labelInput.AutoSize = true;
            this.labelInput.Location = new System.Drawing.Point(10, 7);
            this.labelInput.Name = "labelInput";
            this.labelInput.Size = new System.Drawing.Size(35, 15);
            this.labelInput.TabIndex = 3;
            this.labelInput.Text = "Input";
            // 
            // labelOutput
            // 
            this.labelOutput.AutoSize = true;
            this.labelOutput.Location = new System.Drawing.Point(250, 7);
            this.labelOutput.Name = "labelOutput";
            this.labelOutput.Size = new System.Drawing.Size(45, 15);
            this.labelOutput.TabIndex = 4;
            this.labelOutput.Text = "Output";
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(12, 297);
            this.buttonStart.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(102, 32);
            this.buttonStart.TabIndex = 5;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // plotViewWave
            // 
            this.plotViewWave.Location = new System.Drawing.Point(14, 65);
            this.plotViewWave.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.plotViewWave.Name = "plotViewWave";
            this.plotViewWave.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotViewWave.Size = new System.Drawing.Size(332, 220);
            this.plotViewWave.TabIndex = 6;
            this.plotViewWave.Text = "plotViewWave";
            this.plotViewWave.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotViewWave.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotViewWave.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // plotViewSpectrogram
            // 
            this.plotViewSpectrogram.Location = new System.Drawing.Point(366, 65);
            this.plotViewSpectrogram.Name = "plotViewSpectrogram";
            this.plotViewSpectrogram.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotViewSpectrogram.Size = new System.Drawing.Size(322, 220);
            this.plotViewSpectrogram.TabIndex = 7;
            this.plotViewSpectrogram.Text = "plotViewSpectrogram";
            this.plotViewSpectrogram.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotViewSpectrogram.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotViewSpectrogram.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 338);
            this.Controls.Add(this.plotViewSpectrogram);
            this.Controls.Add(this.plotViewWave);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.labelOutput);
            this.Controls.Add(this.labelInput);
            this.Controls.Add(this.comboBoxOutput);
            this.Controls.Add(this.comboBoxInput);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
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
        private OxyPlot.WindowsForms.PlotView plotViewSpectrogram;
    }
}