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
            this.SettingButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.MicVolumeAdjustTrackBar)).BeginInit();
            this.SuspendLayout();
            // 
            // InputComboBox
            // 
            this.InputComboBox.FormattingEnabled = true;
            this.InputComboBox.Location = new System.Drawing.Point(63, 4);
            this.InputComboBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.InputComboBox.Name = "InputComboBox";
            this.InputComboBox.Size = new System.Drawing.Size(210, 23);
            this.InputComboBox.TabIndex = 0;
            // 
            // OutputComboBox
            // 
            this.OutputComboBox.FormattingEnabled = true;
            this.OutputComboBox.Location = new System.Drawing.Point(63, 30);
            this.OutputComboBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.OutputComboBox.Name = "OutputComboBox";
            this.OutputComboBox.Size = new System.Drawing.Size(210, 23);
            this.OutputComboBox.TabIndex = 1;
            // 
            // InputLabel
            // 
            this.InputLabel.AutoSize = true;
            this.InputLabel.Location = new System.Drawing.Point(10, 7);
            this.InputLabel.Name = "InputLabel";
            this.InputLabel.Size = new System.Drawing.Size(35, 15);
            this.InputLabel.TabIndex = 3;
            this.InputLabel.Text = "Input";
            // 
            // OutputLabel
            // 
            this.OutputLabel.AutoSize = true;
            this.OutputLabel.Location = new System.Drawing.Point(10, 32);
            this.OutputLabel.Name = "OutputLabel";
            this.OutputLabel.Size = new System.Drawing.Size(45, 15);
            this.OutputLabel.TabIndex = 4;
            this.OutputLabel.Text = "Output";
            // 
            // StartButton
            // 
            this.StartButton.Location = new System.Drawing.Point(10, 344);
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
            this.StopButton.Location = new System.Drawing.Point(153, 344);
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
            this.TargetListBox.Location = new System.Drawing.Point(10, 56);
            this.TargetListBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.TargetListBox.Name = "TargetListBox";
            this.TargetListBox.Size = new System.Drawing.Size(263, 214);
            this.TargetListBox.TabIndex = 8;
            this.TargetListBox.SelectedIndexChanged += new System.EventHandler(this.TargetListBox_SelectedIndexChanged);
            // 
            // MicVolumeAdjustTrackBar
            // 
            this.MicVolumeAdjustTrackBar.Location = new System.Drawing.Point(10, 294);
            this.MicVolumeAdjustTrackBar.Maximum = 20;
            this.MicVolumeAdjustTrackBar.Minimum = -20;
            this.MicVolumeAdjustTrackBar.Name = "MicVolumeAdjustTrackBar";
            this.MicVolumeAdjustTrackBar.Size = new System.Drawing.Size(132, 45);
            this.MicVolumeAdjustTrackBar.TabIndex = 9;
            this.MicVolumeAdjustTrackBar.ValueChanged += new System.EventHandler(this.MicVolumeAdjustTrackBar_ValueChanged);
            // 
            // MicVolumeAdjustLabel
            // 
            this.MicVolumeAdjustLabel.AutoSize = true;
            this.MicVolumeAdjustLabel.Location = new System.Drawing.Point(16, 276);
            this.MicVolumeAdjustLabel.Name = "MicVolumeAdjustLabel";
            this.MicVolumeAdjustLabel.Size = new System.Drawing.Size(104, 15);
            this.MicVolumeAdjustLabel.TabIndex = 10;
            this.MicVolumeAdjustLabel.Text = "Mic volume adjust";
            // 
            // SettingButton
            // 
            this.SettingButton.Location = new System.Drawing.Point(197, 294);
            this.SettingButton.Name = "SettingButton";
            this.SettingButton.Size = new System.Drawing.Size(75, 23);
            this.SettingButton.TabIndex = 11;
            this.SettingButton.Text = "Setting";
            this.SettingButton.UseVisualStyleBackColor = true;
            this.SettingButton.Click += new System.EventHandler(this.SettingButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 385);
            this.Controls.Add(this.SettingButton);
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
        private Button SettingButton;
    }
}