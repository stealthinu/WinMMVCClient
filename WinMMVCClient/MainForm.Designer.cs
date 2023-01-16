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
            this.comboBoxInput.Location = new System.Drawing.Point(11, 35);
            this.comboBoxInput.Name = "comboBoxInput";
            this.comboBoxInput.Size = new System.Drawing.Size(268, 28);
            this.comboBoxInput.TabIndex = 0;
            // 
            // comboBoxOutput
            // 
            this.comboBoxOutput.FormattingEnabled = true;
            this.comboBoxOutput.Location = new System.Drawing.Point(286, 35);
            this.comboBoxOutput.Name = "comboBoxOutput";
            this.comboBoxOutput.Size = new System.Drawing.Size(298, 28);
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
            this.labelOutput.Location = new System.Drawing.Point(286, 9);
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
            this.plotViewWave.Location = new System.Drawing.Point(16, 87);
            this.plotViewWave.Name = "plotViewWave";
            this.plotViewWave.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotViewWave.Size = new System.Drawing.Size(379, 293);
            this.plotViewWave.TabIndex = 6;
            this.plotViewWave.Text = "plotViewWave";
            this.plotViewWave.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotViewWave.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotViewWave.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // plotViewSpectrogram
            // 
            this.plotViewSpectrogram.Location = new System.Drawing.Point(418, 87);
            this.plotViewSpectrogram.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.plotViewSpectrogram.Name = "plotViewSpectrogram";
            this.plotViewSpectrogram.PanCursor = System.Windows.Forms.Cursors.Hand;
            this.plotViewSpectrogram.Size = new System.Drawing.Size(368, 293);
            this.plotViewSpectrogram.TabIndex = 7;
            this.plotViewSpectrogram.Text = "plotViewSpectrogram";
            this.plotViewSpectrogram.ZoomHorizontalCursor = System.Windows.Forms.Cursors.SizeWE;
            this.plotViewSpectrogram.ZoomRectangleCursor = System.Windows.Forms.Cursors.SizeNWSE;
            this.plotViewSpectrogram.ZoomVerticalCursor = System.Windows.Forms.Cursors.SizeNS;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 451);
            this.Controls.Add(this.plotViewSpectrogram);
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
        private OxyPlot.WindowsForms.PlotView plotViewSpectrogram;
    }
}