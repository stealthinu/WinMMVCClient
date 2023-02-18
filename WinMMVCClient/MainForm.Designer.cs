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
            this.inputComboBox = new System.Windows.Forms.ComboBox();
            this.outputComboBox = new System.Windows.Forms.ComboBox();
            this.inputLabel = new System.Windows.Forms.Label();
            this.outputLabel = new System.Windows.Forms.Label();
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.targetListBox = new System.Windows.Forms.ListBox();
            this.micVolumeAdjustTrackBar = new System.Windows.Forms.TrackBar();
            this.micVolumeAdjustLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.micVolumeAdjustTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // inputComboBox
            // 
            this.inputComboBox.FormattingEnabled = true;
            this.inputComboBox.Location = new System.Drawing.Point(63, 4);
            this.inputComboBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.inputComboBox.Name = "inputComboBox";
            this.inputComboBox.Size = new System.Drawing.Size(210, 23);
            this.inputComboBox.TabIndex = 0;
            // 
            // outputComboBox
            // 
            this.outputComboBox.FormattingEnabled = true;
            this.outputComboBox.Location = new System.Drawing.Point(63, 30);
            this.outputComboBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.outputComboBox.Name = "outputComboBox";
            this.outputComboBox.Size = new System.Drawing.Size(210, 23);
            this.outputComboBox.TabIndex = 1;
            // 
            // inputLabel
            // 
            this.inputLabel.AutoSize = true;
            this.inputLabel.Location = new System.Drawing.Point(10, 7);
            this.inputLabel.Name = "inputLabel";
            this.inputLabel.Size = new System.Drawing.Size(35, 15);
            this.inputLabel.TabIndex = 3;
            this.inputLabel.Text = "Input";
            // 
            // outputLabel
            // 
            this.outputLabel.AutoSize = true;
            this.outputLabel.Location = new System.Drawing.Point(10, 32);
            this.outputLabel.Name = "outputLabel";
            this.outputLabel.Size = new System.Drawing.Size(45, 15);
            this.outputLabel.TabIndex = 4;
            this.outputLabel.Text = "Output";
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(10, 344);
            this.startButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(120, 32);
            this.startButton.TabIndex = 5;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.buttonStart_Click);
            // 
            // stopButton
            // 
            this.stopButton.Location = new System.Drawing.Point(153, 344);
            this.stopButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(120, 31);
            this.stopButton.TabIndex = 7;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.buttonStop_Click);
            // 
            // targetListBox
            // 
            this.targetListBox.FormattingEnabled = true;
            this.targetListBox.ItemHeight = 15;
            this.targetListBox.Location = new System.Drawing.Point(10, 56);
            this.targetListBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.targetListBox.Name = "targetListBox";
            this.targetListBox.Size = new System.Drawing.Size(263, 214);
            this.targetListBox.TabIndex = 8;
            this.targetListBox.SelectedIndexChanged += new System.EventHandler(this.listBoxTarget_SelectedIndexChanged);
            // 
            // micVolumeAdjustTrackBar
            // 
            this.micVolumeAdjustTrackBar.Location = new System.Drawing.Point(10, 294);
            this.micVolumeAdjustTrackBar.Maximum = 20;
            this.micVolumeAdjustTrackBar.Minimum = -20;
            this.micVolumeAdjustTrackBar.Name = "micVolumeAdjustTrackBar";
            this.micVolumeAdjustTrackBar.Size = new System.Drawing.Size(132, 45);
            this.micVolumeAdjustTrackBar.TabIndex = 9;
            this.micVolumeAdjustTrackBar.ValueChanged += new System.EventHandler(this.trackBarMicVolumeAdjust_ValueChanged);
            // 
            // micVolumeAdjustLabel
            // 
            this.micVolumeAdjustLabel.AutoSize = true;
            this.micVolumeAdjustLabel.Location = new System.Drawing.Point(16, 276);
            this.micVolumeAdjustLabel.Name = "micVolumeAdjustLabel";
            this.micVolumeAdjustLabel.Size = new System.Drawing.Size(104, 15);
            this.micVolumeAdjustLabel.TabIndex = 10;
            this.micVolumeAdjustLabel.Text = "Mic volume adjust";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 385);
            this.Controls.Add(this.micVolumeAdjustLabel);
            this.Controls.Add(this.micVolumeAdjustTrackBar);
            this.Controls.Add(this.targetListBox);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.outputLabel);
            this.Controls.Add(this.inputLabel);
            this.Controls.Add(this.outputComboBox);
            this.Controls.Add(this.inputComboBox);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MainForm";
            this.Text = "MMVC Client";
            ((System.ComponentModel.ISupportInitialize)(this.micVolumeAdjustTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComboBox inputComboBox;
        private ComboBox outputComboBox;
        private Label inputLabel;
        private Label outputLabel;
        private Button startButton;
        private Button stopButton;
        private ListBox targetListBox;
        private TrackBar micVolumeAdjustTrackBar;
        private Label micVolumeAdjustLabel;
    }
}