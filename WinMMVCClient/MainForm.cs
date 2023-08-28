using Microsoft.Extensions.Configuration;
using NAudio.CoreAudioApi;
using Newtonsoft.Json.Linq;

namespace WinMMVCClient
{
    public partial class MainForm : Form
    {
        private MMDeviceCollection? inputs;
        private MMDeviceCollection? outputs;
        private MMDevice? inputDevice;
        private MMDevice? outputDevice;
        private Converter? converter;
        public JObject conf;
        public String confFilePath;
        public JObject hps;
        private Dictionary<int, Correspondence> correspondenceDict;
        private int TargetId;
        private int MicVolumeAdjust;
        private int PitchAdjust;

        public MainForm()
        {
            InitializeComponent();
            this.Load += MainForm_Load;
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                confFilePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
                var confJson = File.ReadAllText(confFilePath);
                conf = JObject.Parse(confJson);
                var hpsFilePath = conf["path"]["json"].Value<String>();
                var correspondenceFilePath = conf["path"]["correspondence"].Value<String>();
                var modelFilePath = conf["path"]["model"].Value<String>();
                if (string.IsNullOrWhiteSpace(hpsFilePath) || string.IsNullOrWhiteSpace(correspondenceFilePath) || string.IsNullOrWhiteSpace(modelFilePath))
                {
                    // Open setting form
                    var settingsForm = new SettingForm(conf, confFilePath);
                    if (settingsForm.ShowDialog() == DialogResult.OK)
                    {
                        conf = settingsForm.conf;
                        hpsFilePath = conf["path"]["json"].Value<String>();
                        correspondenceFilePath = conf["path"]["correspondence"].Value<String>();
                        modelFilePath = conf["path"]["model"].Value<String>();
                    }
                }
                hps = JObject.Parse(File.ReadAllText(hpsFilePath));
                var sidSrc = conf["vc_conf"]["source_id"].Value<int>();
                correspondenceDict = CorrespondenceDictReader.ReadDataFromFile(correspondenceFilePath, sidSrc); // òbé“ñàÇÃâπíˆï‚ê≥ílÇéÊìæ

                converter = new Converter(conf, hps, correspondenceDict);
                SetupInputOutputComboBox();
                SetupVoiceListBox();
                SetupAdjustTrackBar();
                StartButton.Enabled = true;
                StopButton.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Initialized error: {ex.Message}", "Error");
                Application.Exit();
            }
        }

        private void Start()
        {
            try
            {
                if (!(InputComboBox.SelectedItem is MMDevice && InputComboBox.SelectedItem is MMDevice)) return;
                FixInputOutputComboBox();
                inputDevice = (MMDevice)InputComboBox.SelectedItem;
                outputDevice = (MMDevice)OutputComboBox.SelectedItem;
                converter.InitWaveDevice(inputDevice, outputDevice);
                converter.SetMicVolumeAdjust(MicVolumeAdjust);
                converter.SetTargetId(TargetId);
                converter.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Convert error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Stop()
        {
            converter.DisposeWaveDevice();
            UnfixInputOutputComboBox();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            StartButton.Enabled = false;
            StopButton.Enabled = true;
            Start();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            Stop();
            StartButton.Enabled = true;
            StopButton.Enabled = false;
        }

        private void SetupInputOutputComboBox()
        {
            string? inputName = conf["device"]["input_device1"].Value<string>();
            string? outputName = conf["device"]["output_device"].Value<string>();
            inputs = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            var inputsArray = inputs.ToArray();
            outputs = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            var outputsArray = outputs.ToArray();
            InputComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            InputComboBox.Items.Clear();
            InputComboBox.Items.AddRange(inputsArray);
            OutputComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            OutputComboBox.Items.Clear();
            OutputComboBox.Items.AddRange(outputsArray);
            for (int i = 0; i < inputsArray.Length; i++)
            {
                if (inputsArray[i].FriendlyName == inputName)
                    InputComboBox.SelectedIndex = i;
            }
            for (int i = 0; i < outputsArray.Length; i++)
            {
                if (outputsArray[i].FriendlyName == outputName)
                    OutputComboBox.SelectedIndex = i;
            }
        }

        private void FixInputOutputComboBox()
        {
            InputComboBox.Enabled = false;
            OutputComboBox.Enabled = false;
        }

        private void UnfixInputOutputComboBox()
        {
            InputComboBox.Enabled = true;
            OutputComboBox.Enabled = true;
        }

        private record Voice
        {
            public int Index { get; set; }
            public int ID { get; set; }
            public string Label { get; set; }

            public Voice(int index, int id, string label)
            {
                Index = index;
                ID = id;
                Label = label;
            }

            public override string ToString()
            {
                return Label;
            }
        }

        private void SetupVoiceListBox()
        {
            TargetId = conf["vc_conf"]["target_id"].Value<int>();
            TargetListBox.Items.Clear();
            var voiceList = new List<Voice>();
            var index = 0;
            foreach (var (key, value) in correspondenceDict)
            {
                var id = value.Id;
                var name = value.Name;
                voiceList.Add(new Voice(index, id, name));
                index++;
            }
            voiceList.Sort((a, b) => a.Index - b.Index);
            TargetListBox.DataSource = voiceList;
            for (int i = 0; i < voiceList.Count; i++)
            {
                if (voiceList[i].ID == TargetId)
                {
                    TargetListBox.SelectedIndex = i;
                    break;
                }
            }
        }

        private void TargetListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            Voice? targetVoice = TargetListBox.SelectedItem as Voice;
            if (converter != null && targetVoice != null)
            {
                converter.SetTargetId(targetVoice.ID);
                SetPitchAdjust(converter.GetPitchAdjust());
            }
        }

        private void MicVolumeAdjustTrackBar_Scroll(object sender, EventArgs e)
        {
            // -20: -10dB, 20: +20dB
            SetMicVolumeAdjust(MicVolumeAdjustTrackBar.Value);
        }

        private void PitchAdjustTrackBar_Scroll(object sender, EventArgs e)
        {
            SetPitchAdjust(PitchAdjustTrackBar.Value);
        }

        private void SetupAdjustTrackBar()
        {
            var micVolumeAdjust = Convert.ToInt32(conf["vc_conf:mic_volume_adjust"]);
            SetMicVolumeAdjust(micVolumeAdjust);
            //var pitchAdjust = Convert.ToInt32(conf["vc_conf:pitch_adjust"]);
            //SetPitchAdjust(pitchAdjust);
        }

        private void SetMicVolumeAdjust(int adjustValue)
        {
            MicVolumeAdjust = adjustValue;
            MicVolumeAdjustTrackBar.Value = MicVolumeAdjust;
            MicVolumeAdjustTextBox.Text = MicVolumeAdjust.ToString();
            converter?.SetMicVolumeAdjust(MicVolumeAdjust);
        }

        private void SetPitchAdjust(int adjustValue)
        {
            PitchAdjust = adjustValue;
            PitchAdjustTrackBar.Value = PitchAdjust;
            PitchAdjustTextBox.Text = PitchAdjust.ToString();
            converter?.SetPitchAdjust(PitchAdjust);
        }

        private void SettingButton_Click(object sender, EventArgs e)
        {
            var settingsForm = new SettingForm(conf, confFilePath);
            if (settingsForm.ShowDialog() == DialogResult.OK)
            {
                conf = settingsForm.conf;
                converter = new Converter(conf, hps, correspondenceDict);
            }
        }
    }
}
