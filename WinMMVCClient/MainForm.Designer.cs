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
            this.InputComboBox = new System.Windows.Forms.ComboBox();
            this.OutputComboBox = new System.Windows.Forms.ComboBox();
            this.InputLabel = new System.Windows.Forms.Label();
            this.OutputLabel = new System.Windows.Forms.Label();
            this.StartButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.TargetListBox = new System.Windows.Forms.ListBox();
            this.MicVolumeAdjustTrackBar = new System.Windows.Forms.TrackBar();
            this.MicVolumeAdjustLabel = new System.Windows.Forms.Label();
            this.PitchAdjustLabel = new System.Windows.Forms.Label();
            this.PitchAdjustTrackBar = new System.Windows.Forms.TrackBar();
            this.SettingButton = new System.Windows.Forms.Button();
            this.MicVolumeAdjustTextBox = new System.Windows.Forms.TextBox();
            this.PitchAdjustTextBox = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.MicVolumeAdjustTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.PitchAdjustTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // InputComboBox
            // 
            this.InputComboBox.FormattingEnabled = true;
            this.InputComboBox.Location = new System.Drawing.Point(63, 45);
            this.InputComboBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.InputComboBox.Name = "InputComboBox";
            this.InputComboBox.Size = new System.Drawing.Size(210, 23);
            this.InputComboBox.TabIndex = 0;
            // 
            // OutputComboBox
            // 
            this.OutputComboBox.FormattingEnabled = true;
            this.OutputComboBox.Location = new System.Drawing.Point(63, 71);
            this.OutputComboBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.OutputComboBox.Name = "OutputComboBox";
            this.OutputComboBox.Size = new System.Drawing.Size(210, 23);
            this.OutputComboBox.TabIndex = 1;
            // 
            // InputLabel
            // 
            this.InputLabel.AutoSize = true;
            this.InputLabel.Location = new System.Drawing.Point(10, 48);
            this.InputLabel.Name = "InputLabel";
            this.InputLabel.Size = new System.Drawing.Size(35, 15);
            this.InputLabel.TabIndex = 3;
            this.InputLabel.Text = "Input";
            // 
            // OutputLabel
            // 
            this.OutputLabel.AutoSize = true;
            this.OutputLabel.Location = new System.Drawing.Point(10, 73);
            this.OutputLabel.Name = "OutputLabel";
            this.OutputLabel.Size = new System.Drawing.Size(45, 15);
            this.OutputLabel.TabIndex = 4;
            this.OutputLabel.Text = "Output";
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(10, 385);
            this.StartButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(120, 32);
            this.StartButton.TabIndex = 5;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.Location = new System.Drawing.Point(153, 385);
            this.StopButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(120, 31);
            this.StopButton.TabIndex = 7;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // TargetListBox
            // 
            this.TargetListBox.FormattingEnabled = true;
            this.TargetListBox.ItemHeight = 15;
            this.TargetListBox.Location = new System.Drawing.Point(10, 97);
            this.TargetListBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TargetListBox.Name = "TargetListBox";
            this.TargetListBox.Size = new System.Drawing.Size(263, 214);
            this.TargetListBox.TabIndex = 8;
            this.TargetListBox.SelectedIndexChanged += new System.EventHandler(this.TargetListBox_SelectedIndexChanged);
            // 
            // MicVolumeAdjustTrackBar
            // 
            this.MicVolumeAdjustTrackBar.Location = new System.Drawing.Point(10, 335);
            this.MicVolumeAdjustTrackBar.Maximum = 20;
            this.MicVolumeAdjustTrackBar.Minimum = -20;
            this.MicVolumeAdjustTrackBar.Name = "MicVolumeAdjustTrackBar";
            this.MicVolumeAdjustTrackBar.Size = new System.Drawing.Size(99, 45);
            this.MicVolumeAdjustTrackBar.TabIndex = 9;
            this.MicVolumeAdjustTrackBar.Scroll += new System.EventHandler(this.MicVolumeAdjustTrackBar_Scroll);
            // 
            // MicVolumeAdjustLabel
            // 
            this.MicVolumeAdjustLabel.AutoSize = true;
            this.MicVolumeAdjustLabel.Location = new System.Drawing.Point(16, 317);
            this.MicVolumeAdjustLabel.Name = "MicVolumeAdjustLabel";
            this.MicVolumeAdjustLabel.Size = new System.Drawing.Size(104, 15);
            this.MicVolumeAdjustLabel.TabIndex = 10;
            this.MicVolumeAdjustLabel.Text = "Mic volume adjust";
            // 
            // PitchAdjustLabel
            // 
            this.PitchAdjustLabel.AutoSize = true;
            this.PitchAdjustLabel.Location = new System.Drawing.Point(169, 317);
            this.PitchAdjustLabel.Name = "PitchAdjustLabel";
            this.PitchAdjustLabel.Size = new System.Drawing.Size(69, 15);
            this.PitchAdjustLabel.TabIndex = 12;
            this.PitchAdjustLabel.Text = "Pitch adjust";
            // 
            // PitchAdjustTrackBar
            // 
            this.PitchAdjustTrackBar.Location = new System.Drawing.Point(151, 335);
            this.PitchAdjustTrackBar.Maximum = 20;
            this.PitchAdjustTrackBar.Minimum = -20;
            this.PitchAdjustTrackBar.Name = "PitchAdjustTrackBar";
            this.PitchAdjustTrackBar.Size = new System.Drawing.Size(99, 45);
            this.PitchAdjustTrackBar.TabIndex = 11;
            this.PitchAdjustTrackBar.Scroll += new System.EventHandler(this.PitchAdjustTrackBar_Scroll);
            // 
            // settingButton
            // 
            this.SettingButton.Location = new System.Drawing.Point(191, 6);
            this.SettingButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.SettingButton.Name = "settingButton";
            this.SettingButton.Size = new System.Drawing.Size(82, 32);
            this.SettingButton.TabIndex = 13;
            this.SettingButton.Text = "Setting";
            this.SettingButton.UseVisualStyleBackColor = true;
            this.SettingButton.Click += new System.EventHandler(this.settingButton_Click);
            // 
            // micVolumeAdjustTextBox
            // 
            this.MicVolumeAdjustTextBox.Location = new System.Drawing.Point(108, 338);
            this.MicVolumeAdjustTextBox.Name = "micVolumeAdjustTextBox";
            this.MicVolumeAdjustTextBox.Size = new System.Drawing.Size(26, 23);
            this.MicVolumeAdjustTextBox.TabIndex = 14;
            // 
            // pitchAdjustTextBox
            // 
            this.PitchAdjustTextBox.Location = new System.Drawing.Point(247, 338);
            this.PitchAdjustTextBox.Name = "pitchAdjustTextBox";
            this.PitchAdjustTextBox.Size = new System.Drawing.Size(26, 23);
            this.PitchAdjustTextBox.TabIndex = 15;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 431);
            this.Controls.Add(this.PitchAdjustTextBox);
            this.Controls.Add(this.MicVolumeAdjustTextBox);
            this.Controls.Add(this.SettingButton);
            this.Controls.Add(this.PitchAdjustLabel);
            this.Controls.Add(this.PitchAdjustTrackBar);
            this.Controls.Add(this.MicVolumeAdjustLabel);
            this.Controls.Add(this.MicVolumeAdjustTrackBar);
            this.Controls.Add(this.TargetListBox);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.OutputLabel);
            this.Controls.Add(this.InputLabel);
            this.Controls.Add(this.OutputComboBox);
            this.Controls.Add(this.InputComboBox);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "MainForm";
            this.Text = "MMVC Client";
            ((System.ComponentModel.ISupportInitialize)(this.MicVolumeAdjustTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.PitchAdjustTrackBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ComboBox InputComboBox;
        private ComboBox OutputComboBox;
        private Label InputLabel;
        private Label OutputLabel;
        private Button StartButton;
        private Button StopButton;
        private ListBox TargetListBox;
        private TrackBar MicVolumeAdjustTrackBar;
        private Label MicVolumeAdjustLabel;
        private Label PitchAdjustLabel;
        private TrackBar PitchAdjustTrackBar;
        private Button SettingButton;
        private TextBox MicVolumeAdjustTextBox;
        private TextBox PitchAdjustTextBox;
    }
}
