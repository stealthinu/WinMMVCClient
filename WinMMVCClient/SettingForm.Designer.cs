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
            this.configFileButton = new System.Windows.Forms.Button();
            this.configFileTextBox = new System.Windows.Forms.TextBox();
            this.modelFileTextBox = new System.Windows.Forms.TextBox();
            this.modelFileButton = new System.Windows.Forms.Button();
            this.correspondenceFileTextBox = new System.Windows.Forms.TextBox();
            this.correspondenceFileButton = new System.Windows.Forms.Button();
            this.VcGroupBox = new System.Windows.Forms.GroupBox();
            this.delayFramesComboBox = new System.Windows.Forms.ComboBox();
            this.DelayFramesLabel = new System.Windows.Forms.Label();
            this.filePathGroupBox = new System.Windows.Forms.GroupBox();
            this.okButton = new System.Windows.Forms.Button();
            this.VcGroupBox.SuspendLayout();
            this.filePathGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // configFileButton
            // 
            this.configFileButton.Location = new System.Drawing.Point(553, 22);
            this.configFileButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.configFileButton.Name = "configFileButton";
            this.configFileButton.Size = new System.Drawing.Size(103, 22);
            this.configFileButton.TabIndex = 0;
            this.configFileButton.Text = "config";
            this.configFileButton.UseVisualStyleBackColor = true;
            this.configFileButton.Click += new System.EventHandler(this.configFileButton_Click);
            // 
            // configFileTextBox
            // 
            this.configFileTextBox.Location = new System.Drawing.Point(15, 21);
            this.configFileTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.configFileTextBox.Name = "configFileTextBox";
            this.configFileTextBox.Size = new System.Drawing.Size(532, 23);
            this.configFileTextBox.TabIndex = 1;
            // 
            // modelFileTextBox
            // 
            this.modelFileTextBox.Location = new System.Drawing.Point(15, 48);
            this.modelFileTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.modelFileTextBox.Name = "modelFileTextBox";
            this.modelFileTextBox.Size = new System.Drawing.Size(532, 23);
            this.modelFileTextBox.TabIndex = 3;
            // 
            // modelFileButton
            // 
            this.modelFileButton.Location = new System.Drawing.Point(553, 49);
            this.modelFileButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.modelFileButton.Name = "modelFileButton";
            this.modelFileButton.Size = new System.Drawing.Size(103, 22);
            this.modelFileButton.TabIndex = 2;
            this.modelFileButton.Text = "model";
            this.modelFileButton.UseVisualStyleBackColor = true;
            this.modelFileButton.Click += new System.EventHandler(this.modelFileButton_Click);
            // 
            // correspondenceFileTextBox
            // 
            this.correspondenceFileTextBox.Location = new System.Drawing.Point(15, 75);
            this.correspondenceFileTextBox.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.correspondenceFileTextBox.Name = "correspondenceFileTextBox";
            this.correspondenceFileTextBox.Size = new System.Drawing.Size(532, 23);
            this.correspondenceFileTextBox.TabIndex = 5;
            // 
            // correspondenceFileButton
            // 
            this.correspondenceFileButton.Location = new System.Drawing.Point(553, 76);
            this.correspondenceFileButton.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.correspondenceFileButton.Name = "correspondenceFileButton";
            this.correspondenceFileButton.Size = new System.Drawing.Size(103, 22);
            this.correspondenceFileButton.TabIndex = 4;
            this.correspondenceFileButton.Text = "correspondence";
            this.correspondenceFileButton.UseVisualStyleBackColor = true;
            this.correspondenceFileButton.Click += new System.EventHandler(this.correspondenceFileButton_Click);
            // 
            // VcGroupBox
            // 
            this.VcGroupBox.Controls.Add(this.delayFramesComboBox);
            this.VcGroupBox.Controls.Add(this.DelayFramesLabel);
            this.VcGroupBox.Location = new System.Drawing.Point(12, 130);
            this.VcGroupBox.Name = "VcGroupBox";
            this.VcGroupBox.Size = new System.Drawing.Size(244, 167);
            this.VcGroupBox.TabIndex = 6;
            this.VcGroupBox.TabStop = false;
            this.VcGroupBox.Text = "VC Settings";
            // 
            // delayFramesComboBox
            // 
            this.delayFramesComboBox.FormattingEnabled = true;
            this.delayFramesComboBox.Location = new System.Drawing.Point(112, 16);
            this.delayFramesComboBox.Name = "delayFramesComboBox";
            this.delayFramesComboBox.Size = new System.Drawing.Size(121, 23);
            this.delayFramesComboBox.TabIndex = 1;
            // 
            // DelayFramesLabel
            // 
            this.DelayFramesLabel.Location = new System.Drawing.Point(6, 19);
            this.DelayFramesLabel.Name = "DelayFramesLabel";
            this.DelayFramesLabel.Size = new System.Drawing.Size(100, 23);
            this.DelayFramesLabel.TabIndex = 0;
            this.DelayFramesLabel.Text = "Delay Frames";
            // 
            // filePathGroupBox
            // 
            this.filePathGroupBox.Controls.Add(this.configFileTextBox);
            this.filePathGroupBox.Controls.Add(this.configFileButton);
            this.filePathGroupBox.Controls.Add(this.correspondenceFileTextBox);
            this.filePathGroupBox.Controls.Add(this.modelFileButton);
            this.filePathGroupBox.Controls.Add(this.correspondenceFileButton);
            this.filePathGroupBox.Controls.Add(this.modelFileTextBox);
            this.filePathGroupBox.Location = new System.Drawing.Point(12, 12);
            this.filePathGroupBox.Name = "filePathGroupBox";
            this.filePathGroupBox.Size = new System.Drawing.Size(676, 112);
            this.filePathGroupBox.TabIndex = 7;
            this.filePathGroupBox.TabStop = false;
            this.filePathGroupBox.Text = "File path";
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(597, 295);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 8;
            this.okButton.Text = "OK";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // SettingForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 338);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.filePathGroupBox);
            this.Controls.Add(this.VcGroupBox);
            this.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.Name = "SettingForm";
            this.Text = "Setting";
            this.VcGroupBox.ResumeLayout(false);
            this.filePathGroupBox.ResumeLayout(false);
            this.filePathGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Button configFileButton;
        private TextBox configFileTextBox;
        private TextBox modelFileTextBox;
        private Button modelFileButton;
        private TextBox correspondenceFileTextBox;
        private Button correspondenceFileButton;
        private GroupBox VcGroupBox;
        private Label DelayFramesLabel;
        private ComboBox delayFramesComboBox;
        private GroupBox filePathGroupBox;
        private Button okButton;
    }
}