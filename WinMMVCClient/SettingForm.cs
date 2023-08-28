using Newtonsoft.Json.Linq;

namespace WinMMVCClient
{
    public class ComboBoxItem<T>
    {
        public string DisplayName { get; set; }
        public T Value { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
    }

    public partial class SettingForm : Form
    {
        public JObject conf;
        public String confFilePath;

        public SettingForm(JObject conf, String confFilePath)
        {
            InitializeComponent();
            initializeComboBoxItems();
            this.conf = conf;
            this.confFilePath = confFilePath;
            configFileTextBox.Text = conf["path"]["json"].Value<String>();
            correspondenceFileTextBox.Text = conf["path"]["correspondence"].Value<String>();
            modelFileTextBox.Text = conf["path"]["model"].Value<String>();  
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

        }

        private void initializeComboBoxItems()
        {
            shareModeComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "Shared", Value = 0 });
            shareModeComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "Exclusive", Value = 1 });
            shareModeComboBox.SelectedIndex = 0;
            gpuIdComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "0", Value = 0 });
            gpuIdComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "1", Value = 1 });
            gpuIdComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "2", Value = 2 });
            gpuIdComboBox.SelectedIndex = 0;
            delayFramesComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "1024", Value = 1024 });
            delayFramesComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "1664", Value = 1664 });
            delayFramesComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "2048", Value = 2048 });
            delayFramesComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "4096", Value = 4096 });
            delayFramesComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "8192", Value = 8192 });
            delayFramesComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "16384", Value = 16384 });
            delayFramesComboBox.SelectedIndex = 1;
            overlapComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "512", Value = 512 });
            overlapComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "768", Value = 768 });
            overlapComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "1024", Value = 1024 });
            overlapComboBox.SelectedIndex = 0;
            disposeSpecsComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "0", Value = 0 });
            disposeSpecsComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "2", Value = 2 });
            disposeSpecsComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "4", Value = 4 });
            disposeSpecsComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "8", Value = 8 });
            disposeSpecsComboBox.SelectedIndex = 2;
            latencyComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "10", Value = 10 });
            latencyComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "20", Value = 20 });
            latencyComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "50", Value = 50 });
            latencyComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "100", Value = 100 });
            latencyComboBox.Items.Add(new ComboBoxItem<int>() { DisplayName = "200", Value = 200 });
            latencyComboBox.SelectedIndex = 2;
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
