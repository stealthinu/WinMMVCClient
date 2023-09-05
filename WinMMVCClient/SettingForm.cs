using Newtonsoft.Json.Linq;
using System;
using System.Management;

namespace WinMMVCClient
{
    public class ComboBoxItem<T>
    {
        public string DisplayName { get; set; }
        public T Value { get; set; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }

    public partial class SettingForm : Form
    {
        public JObject conf;
        public string confFilePath;
        private bool isInitialized = false;

        public SettingForm(JObject conf, string confFilePath)
        {
            InitializeComponent();
            initializeComboBoxItems(conf);
            this.conf = conf;
            this.confFilePath = confFilePath;
            configFileTextBox.Text = conf["path"]["json"].Value<string>();
            correspondenceFileTextBox.Text = conf["path"]["correspondence"].Value<string>();
            modelFileTextBox.Text = conf["path"]["model"].Value<string>();
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
            // pathの空チェックする
            conf["path"]["json"] = configFileTextBox.Text;
            conf["path"]["correspondence"] = correspondenceFileTextBox.Text;
            conf["path"]["model"] = modelFileTextBox.Text;
            File.WriteAllText(confFilePath, conf.ToString());
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void shareModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isInitialized) return;
            conf["device"]["share_mode"] = shareModeComboBox.SelectedItem.ToString();
        }

        private void gpuIdComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isInitialized) return;
            conf["device"]["gpu_id"] = gpuIdComboBox.SelectedItem.ToString();
        }

        private void delayFramesComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isInitialized) return;
            conf["vc_conf"]["delay_frames"] = delayFramesComboBox.SelectedItem.ToString();
        }

        private void overlapComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isInitialized) return;
            conf["vc_conf"]["overlap"] = overlapComboBox.SelectedItem.ToString();
        }

        private void disposeSpecsComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isInitialized) return;
            conf["vc_conf"]["dispose_specs"] = disposeSpecsComboBox.SelectedItem.ToString();
        }

        private void latencyComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isInitialized) return;
            conf["vc_conf"]["latency"] = latencyComboBox.SelectedItem.ToString();
        }

        private void initializeComboBoxItems(JObject conf)
        {
            shareModeComboBox.Items.Add(new ComboBoxItem<string>() { DisplayName = "Shared", Value = "Shared" });
            shareModeComboBox.Items.Add(new ComboBoxItem<string>() { DisplayName = "Exclusive", Value = "Exclusive" });
            SetSelectedString(shareModeComboBox, conf["device"]["share_mode"].Value<string>());

            var gpuCount = getGpuCount();
            for (int i = 0; i < gpuCount; i++)
            {
                gpuIdComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = i.ToString(), Value = i });
            }
            SetSelectedValue(gpuIdComboBox, conf["device"]["gpu_id"].Value<int>());

            delayFramesComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "1024", Value = 1024 });
            delayFramesComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "1664", Value = 1664 });
            delayFramesComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "2048", Value = 2048 });
            delayFramesComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "4096", Value = 4096 });
            delayFramesComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "8192", Value = 8192 });
            delayFramesComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "16384", Value = 16384 });
            SetSelectedValue(delayFramesComboBox, conf["vc_conf"]["delay_frames"].Value<int>());

            overlapComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "512", Value = 512 });
            overlapComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "768", Value = 768 });
            overlapComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "1024", Value = 1024 });
            SetSelectedValue(overlapComboBox, conf["vc_conf"]["overlap"].Value<int>());

            disposeSpecsComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "0", Value = 0 });
            disposeSpecsComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "2", Value = 2 });
            disposeSpecsComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "4", Value = 4 });
            disposeSpecsComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "8", Value = 8 });
            SetSelectedValue(disposeSpecsComboBox, conf["vc_conf"]["dispose_specs"].Value<int>());

            latencyComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "10", Value = 10 });
            latencyComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "20", Value = 20 });
            latencyComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "50", Value = 50 });
            latencyComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "100", Value = 100 });
            latencyComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "200", Value = 200 });
            SetSelectedValue(latencyComboBox, conf["vc_conf"]["latency"].Value<int>());

            isInitialized = true;
        }

        private void SetSelectedValue(ComboBox comboBox, int value)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                ComboBoxItem<int> item = comboBox.Items[i] as ComboBoxItem<int>;
                if (item != null && item.Value == value)
                {
                    comboBox.SelectedIndex = i;
                    break;
                }
            }
        }

        private void SetSelectedString(ComboBox comboBox, string value)
        {
            for (int i = 0; i < comboBox.Items.Count; i++)
            {
                ComboBoxItem<string> item = comboBox.Items[i] as ComboBoxItem<string>;
                if (item != null && item.Value == value)
                {
                    comboBox.SelectedIndex = i;
                    break;
                }
            }
        }

        private static int getGpuCount()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController");
            return searcher.Get().Count;
        }

        /*
        "delay_frames": 1664
        "overlap": 512
        "dispose_specs": 4
        "source_id": 0
        "mic_volume_adjust": 0
        "pitch_adjust": 0
        "latency": 50
        "gpu_id": 0
         */
    }
}
