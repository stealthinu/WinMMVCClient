using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WinMMVCClient
{
    public partial class SettingForm : Form
    {
        public IConfiguration conf;

        public SettingForm(IConfiguration conf)
        {
            InitializeComponent();
            this.conf = conf;
            configFileTextBox.Text = conf["path:json"];
            correspondenceFileTextBox.Text = conf["path:correspondence"];
            modelFileTextBox.Text = conf["path:model"];
        }

        private void configFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                configFileTextBox.Text = openFileDialog.FileName;
            }
        }

        private void modelFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                modelFileTextBox.Text = openFileDialog.FileName;
            }
        }

        private void correspondenceFileButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                correspondenceFileTextBox.Text = openFileDialog.FileName;
            }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            conf["path:json"] = configFileTextBox.Text;
            conf["path:correspondence"] = correspondenceFileTextBox.Text;
            conf["path:model"] = modelFileTextBox.Text;
            // close Dialog
        }

        /*
        "delay_flames": 1664
        "overlap": 512
        "dispose_conv1d_specs": 4
        "source_id": 0
        "mic_volume_adjust": 0
        "pitch_adjust": 0
        "latency": 50
        "gpu_id": 0
         */
    }
}
