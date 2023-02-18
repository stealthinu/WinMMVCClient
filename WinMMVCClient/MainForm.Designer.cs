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
            this.buttonStop = new System.Windows.Forms.Button();
            this.listBoxTarget = new System.Windows.Forms.ListBox();
            this.trackBarMicVolumeAdjust = new System.Windows.Forms.TrackBar();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.trackBarMicVolumeAdjust)).BeginInit();
            this.SuspendLayout();
            // 
            // comboBoxInput
            // 
            this.comboBoxInput.FormattingEnabled = true;
            this.comboBoxInput.Location = new System.Drawing.Point(63, 4);
            this.comboBoxInput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBoxInput.Name = "comboBoxInput";
            this.comboBoxInput.Size = new System.Drawing.Size(210, 23);
            this.comboBoxInput.TabIndex = 0;
            // 
            // comboBoxOutput
            // 
            this.comboBoxOutput.FormattingEnabled = true;
            this.comboBoxOutput.Location = new System.Drawing.Point(63, 30);
            this.comboBoxOutput.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.comboBoxOutput.Name = "comboBoxOutput";
            this.comboBoxOutput.Size = new System.Drawing.Size(210, 23);
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
            this.labelOutput.Location = new System.Drawing.Point(10, 32);
            this.labelOutput.Name = "labelOutput";
            this.labelOutput.Size = new System.Drawing.Size(45, 15);
            this.labelOutput.TabIndex = 4;
            this.labelOutput.Text = "Output";
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(10, 344);
            this.buttonStart.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(120, 32);
            this.buttonStart.TabIndex = 5;
            this.buttonStart.Text = "Start";
            this.buttonStart.UseVisualStyleBackColor = true;
            this.buttonStart.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // buttonStop
            // 
            this.buttonStop.Location = new System.Drawing.Point(153, 344);
            this.buttonStop.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.buttonStop.Name = "buttonStop";
            this.buttonStop.Size = new System.Drawing.Size(120, 31);
            this.buttonStop.TabIndex = 7;
            this.buttonStop.Text = "Stop";
            this.buttonStop.UseVisualStyleBackColor = true;
            this.buttonStop.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // listBoxTarget
            // 
            this.listBoxTarget.FormattingEnabled = true;
            this.listBoxTarget.ItemHeight = 15;
            this.listBoxTarget.Location = new System.Drawing.Point(10, 56);
            this.listBoxTarget.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.listBoxTarget.Name = "listBoxTarget";
            this.listBoxTarget.Size = new System.Drawing.Size(263, 214);
            this.listBoxTarget.TabIndex = 8;
            this.listBoxTarget.SelectedIndexChanged += new System.EventHandler(this.listBoxTarget_SelectedIndexChanged);
            // 
            // trackBarMicVolumeAdjust
            // 
            this.trackBarMicVolumeAdjust.Location = new System.Drawing.Point(10, 294);
            this.trackBarMicVolumeAdjust.Maximum = 20;
            this.trackBarMicVolumeAdjust.Minimum = -20;
            this.trackBarMicVolumeAdjust.Name = "trackBarMicVolumeAdjust";
            this.trackBarMicVolumeAdjust.Size = new System.Drawing.Size(132, 45);
            this.trackBarMicVolumeAdjust.TabIndex = 9;
            this.trackBarMicVolumeAdjust.ValueChanged += new System.EventHandler(this.trackBarMicVolumeAdjust_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 276);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 15);
            this.label1.TabIndex = 10;
            this.label1.Text = "Mic volume adjust";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 385);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.trackBarMicVolumeAdjust);
            this.Controls.Add(this.listBoxTarget);
            this.Controls.Add(this.buttonStop);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.labelOutput);
            this.Controls.Add(this.labelInput);
            this.Controls.Add(this.comboBoxOutput);
            this.Controls.Add(this.comboBoxInput);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MainForm";
            this.Text = "MMVC Client";
            ((System.ComponentModel.ISupportInitialize)(this.trackBarMicVolumeAdjust)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComboBox comboBoxInput;
        private ComboBox comboBoxOutput;
        private Label labelInput;
        private Label labelOutput;
        private Button buttonStart;
        private Button buttonStop;
        private ListBox listBoxTarget;
        private TrackBar trackBarMicVolumeAdjust;
        private Label label1;
    }
}