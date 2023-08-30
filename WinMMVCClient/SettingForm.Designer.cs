namespace WinMMVCClient
{
    partial class SettingForm
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
            configFileButton = new Button();
            configFileTextBox = new TextBox();
            modelFileTextBox = new TextBox();
            modelFileButton = new Button();
            correspondenceFileTextBox = new TextBox();
            correspondenceFileButton = new Button();
            VcGroupBox = new GroupBox();
            latencyComboBox = new ComboBox();
            latencyLabel = new Label();
            disposeSpecsComboBox = new ComboBox();
            disposeSpecsLabel = new Label();
            overlapComboBox = new ComboBox();
            overlapLabel = new Label();
            delayFramesComboBox = new ComboBox();
            delayFramesLabel = new Label();
            gpuIdComboBox = new ComboBox();
            gpuIdLabel = new Label();
            filePathGroupBox = new GroupBox();
            okButton = new Button();
            SystemGroupBox = new GroupBox();
            shareModeComboBox = new ComboBox();
            shareModeLabel = new Label();
            VcGroupBox.SuspendLayout();
            filePathGroupBox.SuspendLayout();
            SystemGroupBox.SuspendLayout();
            SuspendLayout();
            // 
            // configFileButton
            // 
            configFileButton.Location = new Point(632, 29);
            configFileButton.Name = "configFileButton";
            configFileButton.Size = new Size(118, 29);
            configFileButton.TabIndex = 0;
            configFileButton.Text = "config";
            configFileButton.UseVisualStyleBackColor = true;
            configFileButton.Click += configFileButton_Click;
            // 
            // configFileTextBox
            // 
            configFileTextBox.Location = new Point(17, 28);
            configFileTextBox.Name = "configFileTextBox";
            configFileTextBox.Size = new Size(607, 27);
            configFileTextBox.TabIndex = 1;
            // 
            // modelFileTextBox
            // 
            modelFileTextBox.Location = new Point(17, 64);
            modelFileTextBox.Name = "modelFileTextBox";
            modelFileTextBox.Size = new Size(607, 27);
            modelFileTextBox.TabIndex = 3;
            // 
            // modelFileButton
            // 
            modelFileButton.Location = new Point(632, 65);
            modelFileButton.Name = "modelFileButton";
            modelFileButton.Size = new Size(118, 29);
            modelFileButton.TabIndex = 2;
            modelFileButton.Text = "model";
            modelFileButton.UseVisualStyleBackColor = true;
            modelFileButton.Click += modelFileButton_Click;
            // 
            // correspondenceFileTextBox
            // 
            correspondenceFileTextBox.Location = new Point(17, 100);
            correspondenceFileTextBox.Name = "correspondenceFileTextBox";
            correspondenceFileTextBox.Size = new Size(607, 27);
            correspondenceFileTextBox.TabIndex = 5;
            // 
            // correspondenceFileButton
            // 
            correspondenceFileButton.Location = new Point(632, 101);
            correspondenceFileButton.Name = "correspondenceFileButton";
            correspondenceFileButton.Size = new Size(118, 29);
            correspondenceFileButton.TabIndex = 4;
            correspondenceFileButton.Text = "correspondence";
            correspondenceFileButton.UseVisualStyleBackColor = true;
            correspondenceFileButton.Click += correspondenceFileButton_Click;
            // 
            // VcGroupBox
            // 
            VcGroupBox.Controls.Add(latencyComboBox);
            VcGroupBox.Controls.Add(latencyLabel);
            VcGroupBox.Controls.Add(disposeSpecsComboBox);
            VcGroupBox.Controls.Add(disposeSpecsLabel);
            VcGroupBox.Controls.Add(overlapComboBox);
            VcGroupBox.Controls.Add(overlapLabel);
            VcGroupBox.Controls.Add(delayFramesComboBox);
            VcGroupBox.Controls.Add(delayFramesLabel);
            VcGroupBox.Location = new Point(14, 173);
            VcGroupBox.Margin = new Padding(3, 4, 3, 4);
            VcGroupBox.Name = "VcGroupBox";
            VcGroupBox.Padding = new Padding(3, 4, 3, 4);
            VcGroupBox.Size = new Size(279, 223);
            VcGroupBox.TabIndex = 6;
            VcGroupBox.TabStop = false;
            VcGroupBox.Text = "VC Settings";
            // 
            // latencyComboBox
            // 
            latencyComboBox.FormattingEnabled = true;
            latencyComboBox.Location = new Point(128, 129);
            latencyComboBox.Margin = new Padding(3, 4, 3, 4);
            latencyComboBox.Name = "latencyComboBox";
            latencyComboBox.Size = new Size(138, 28);
            latencyComboBox.TabIndex = 7;
            latencyComboBox.SelectedIndexChanged += latencyComboBox_SelectedIndexChanged;
            // 
            // latencyLabel
            // 
            latencyLabel.Location = new Point(7, 133);
            latencyLabel.Name = "latencyLabel";
            latencyLabel.Size = new Size(114, 24);
            latencyLabel.TabIndex = 6;
            latencyLabel.Text = "Latency";
            // 
            // disposeSpecsComboBox
            // 
            disposeSpecsComboBox.FormattingEnabled = true;
            disposeSpecsComboBox.Location = new Point(128, 93);
            disposeSpecsComboBox.Margin = new Padding(3, 4, 3, 4);
            disposeSpecsComboBox.Name = "disposeSpecsComboBox";
            disposeSpecsComboBox.Size = new Size(138, 28);
            disposeSpecsComboBox.TabIndex = 5;
            disposeSpecsComboBox.SelectedIndexChanged += disposeSpecsComboBox_SelectedIndexChanged;
            // 
            // disposeSpecsLabel
            // 
            disposeSpecsLabel.Location = new Point(7, 97);
            disposeSpecsLabel.Name = "disposeSpecsLabel";
            disposeSpecsLabel.Size = new Size(114, 24);
            disposeSpecsLabel.TabIndex = 4;
            disposeSpecsLabel.Text = "Dispose specs";
            // 
            // overlapComboBox
            // 
            overlapComboBox.FormattingEnabled = true;
            overlapComboBox.Location = new Point(128, 57);
            overlapComboBox.Margin = new Padding(3, 4, 3, 4);
            overlapComboBox.Name = "overlapComboBox";
            overlapComboBox.Size = new Size(138, 28);
            overlapComboBox.TabIndex = 3;
            overlapComboBox.SelectedIndexChanged += overlapComboBox_SelectedIndexChanged;
            // 
            // overlapLabel
            // 
            overlapLabel.Location = new Point(7, 61);
            overlapLabel.Name = "overlapLabel";
            overlapLabel.Size = new Size(114, 24);
            overlapLabel.TabIndex = 2;
            overlapLabel.Text = "Overlap";
            // 
            // delayFramesComboBox
            // 
            delayFramesComboBox.FormattingEnabled = true;
            delayFramesComboBox.Location = new Point(128, 21);
            delayFramesComboBox.Margin = new Padding(3, 4, 3, 4);
            delayFramesComboBox.Name = "delayFramesComboBox";
            delayFramesComboBox.Size = new Size(138, 28);
            delayFramesComboBox.TabIndex = 1;
            delayFramesComboBox.SelectedIndexChanged += delayFramesComboBox_SelectedIndexChanged;
            // 
            // delayFramesLabel
            // 
            delayFramesLabel.Location = new Point(7, 25);
            delayFramesLabel.Name = "delayFramesLabel";
            delayFramesLabel.Size = new Size(114, 24);
            delayFramesLabel.TabIndex = 0;
            delayFramesLabel.Text = "Delay frames";
            // 
            // gpuIdComboBox
            // 
            gpuIdComboBox.FormattingEnabled = true;
            gpuIdComboBox.Location = new Point(135, 21);
            gpuIdComboBox.Margin = new Padding(3, 4, 3, 4);
            gpuIdComboBox.Name = "gpuIdComboBox";
            gpuIdComboBox.Size = new Size(138, 28);
            gpuIdComboBox.TabIndex = 7;
            gpuIdComboBox.SelectedIndexChanged += gpuIdComboBox_SelectedIndexChanged;
            // 
            // gpuIdLabel
            // 
            gpuIdLabel.Location = new Point(14, 25);
            gpuIdLabel.Name = "gpuIdLabel";
            gpuIdLabel.Size = new Size(114, 24);
            gpuIdLabel.TabIndex = 6;
            gpuIdLabel.Text = "GPU ID";
            // 
            // filePathGroupBox
            // 
            filePathGroupBox.Controls.Add(configFileTextBox);
            filePathGroupBox.Controls.Add(configFileButton);
            filePathGroupBox.Controls.Add(correspondenceFileTextBox);
            filePathGroupBox.Controls.Add(modelFileButton);
            filePathGroupBox.Controls.Add(correspondenceFileButton);
            filePathGroupBox.Controls.Add(modelFileTextBox);
            filePathGroupBox.Location = new Point(14, 16);
            filePathGroupBox.Margin = new Padding(3, 4, 3, 4);
            filePathGroupBox.Name = "filePathGroupBox";
            filePathGroupBox.Padding = new Padding(3, 4, 3, 4);
            filePathGroupBox.Size = new Size(773, 149);
            filePathGroupBox.TabIndex = 7;
            filePathGroupBox.TabStop = false;
            filePathGroupBox.Text = "File path";
            // 
            // okButton
            // 
            okButton.Location = new Point(682, 393);
            okButton.Margin = new Padding(3, 4, 3, 4);
            okButton.Name = "okButton";
            okButton.Size = new Size(86, 31);
            okButton.TabIndex = 8;
            okButton.Text = "OK";
            okButton.UseVisualStyleBackColor = true;
            okButton.Click += okButton_Click;
            // 
            // SystemGroupBox
            // 
            SystemGroupBox.Controls.Add(shareModeComboBox);
            SystemGroupBox.Controls.Add(shareModeLabel);
            SystemGroupBox.Controls.Add(gpuIdComboBox);
            SystemGroupBox.Controls.Add(gpuIdLabel);
            SystemGroupBox.Location = new Point(308, 173);
            SystemGroupBox.Name = "SystemGroupBox";
            SystemGroupBox.Size = new Size(285, 223);
            SystemGroupBox.TabIndex = 9;
            SystemGroupBox.TabStop = false;
            SystemGroupBox.Text = "System Settings";
            // 
            // shareModeComboBox
            // 
            shareModeComboBox.FormattingEnabled = true;
            shareModeComboBox.Location = new Point(135, 61);
            shareModeComboBox.Margin = new Padding(3, 4, 3, 4);
            shareModeComboBox.Name = "shareModeComboBox";
            shareModeComboBox.Size = new Size(138, 28);
            shareModeComboBox.TabIndex = 9;
            shareModeComboBox.SelectedIndexChanged += shareModeComboBox_SelectedIndexChanged;
            // 
            // shareModeLabel
            // 
            shareModeLabel.Location = new Point(14, 65);
            shareModeLabel.Name = "shareModeLabel";
            shareModeLabel.Size = new Size(114, 24);
            shareModeLabel.TabIndex = 8;
            shareModeLabel.Text = "Share mode";
            // 
            // SettingForm
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 451);
            Controls.Add(SystemGroupBox);
            Controls.Add(okButton);
            Controls.Add(filePathGroupBox);
            Controls.Add(VcGroupBox);
            Name = "SettingForm";
            Text = "Setting";
            VcGroupBox.ResumeLayout(false);
            filePathGroupBox.ResumeLayout(false);
            filePathGroupBox.PerformLayout();
            SystemGroupBox.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button configFileButton;
        private TextBox configFileTextBox;
        private TextBox modelFileTextBox;
        private Button modelFileButton;
        private TextBox correspondenceFileTextBox;
        private Button correspondenceFileButton;
        private GroupBox VcGroupBox;
        private Label delayFramesLabel;
        private ComboBox delayFramesComboBox;
        private GroupBox filePathGroupBox;
        private Button okButton;
        private ComboBox disposeSpecsComboBox;
        private Label disposeSpecsLabel;
        private ComboBox overlapComboBox;
        private Label overlapLabel;
        private ComboBox gpuIdComboBox;
        private Label gpuIdLabel;
        private GroupBox SystemGroupBox;
        private ComboBox shareModeComboBox;
        private Label shareModeLabel;
        private ComboBox latencyComboBox;
        private Label latencyLabel;
    }
}