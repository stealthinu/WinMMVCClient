using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.Json.Nodes;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace WinMMVCClient
{
    public partial class SettingForm : Form
    {
        public IConfiguration conf;
        public Newtonsoft.Json.Linq.JObject jsonObject;

        public SettingForm(IConfiguration conf, Newtonsoft.Json.Linq.JObject jsonObject)
        {
            InitializeComponent();
            this.conf = conf;
            this.jsonObject = jsonObject;
            configFileTextBox.Text = conf["path:json"];
            correspondenceFileTextBox.Text = conf["path:correspondence"];
            modelFileTextBox.Text = conf["path:model"];
            this.jsonObject = jsonObject;
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
            jsonObject["path"]["json"] = configFileTextBox.Text;
            saveSetting();
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void saveSetting()
        {
            var dict = new Dictionary<string, string>();
            foreach (var pair in conf.AsEnumerable())
            {
                dict.Add(pair.Key, pair.Value);
            }
            var jsonString = System.Text.Json.JsonSerializer.Serialize(dict);
            File.WriteAllText("test_settings_file.json", jsonString);
            var outputJsonText = jsonObject.ToString();
            File.WriteAllText("test_settings.json", outputJsonText);
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
