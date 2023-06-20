using Microsoft.Extensions.Configuration;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace WinMMVCClient
{
    public partial class MainForm : Form
    {
        private MMDeviceCollection? inputs;
        private MMDeviceCollection? outputs;
        private MMDevice? inputDevice;
        private MMDevice? outputDevice;
        private Converter? converter;
        public IConfiguration conf { get; private set; }

        public MainForm(IConfiguration _conf)
        {
            InitializeComponent();
            conf = _conf;
            SetupInputOutputComboBox();
            SetupVoiceListBox();
        }

        private void Start()
        {
            if (!(InputComboBox.SelectedItem is MMDevice && InputComboBox.SelectedItem is MMDevice)) return;
            FixInputOutputComboBox();
            inputDevice = (MMDevice)InputComboBox.SelectedItem;
            outputDevice = (MMDevice)OutputComboBox.SelectedItem;

            converter?.Dispose();
            converter = new Converter(inputDevice, outputDevice, conf);
            MicVolumeAdjustTrackBar.Value = converter.MicVolumeAdjustDB;
            converter.Start();
        }

        private void Stop()
        {
            converter?.Dispose();
            UnfixInputOutputComboBox();
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            Start();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            Stop();
        }

        private void SetupInputOutputComboBox()
        {
            string? inputName = conf["device:input_device1"];
            string? outputName = conf["device:output_device"];
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
            TargetListBox.Items.Clear();
            var voiceList = new List<Voice>();
            var list = conf.GetSection("others:voice_list");
            var arr = list.AsEnumerable().ToArray();
            foreach (var ary in list.AsEnumerable())
            {
                if (String.IsNullOrEmpty(ary.Value))
                    continue;
                var keys = ary.Key.Split(':');
                var index = Convert.ToInt32(keys[^2]);
                var id = Convert.ToInt32(keys[^1]);
                var name = ary.Value;
                voiceList.Add(new Voice(index, id, name));
            }
            voiceList.Sort((a, b) => a.Index - b.Index);
            TargetListBox.DataSource = voiceList;
            var targetId = Convert.ToInt32(conf["vc_conf:target_id"]);
            for (int i = 0; i < voiceList.Count; i++)
            {
                if (voiceList[i].ID == targetId)
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
                PitchAdjustTrackBar.Value = converter.GetPitchAdjust();
            }
        }

        private void PitchAdjustTrackBar_Scroll(object sender, EventArgs e)
        {
            var adjustValue = PitchAdjustTrackBar.Value;
            converter?.SetPitchAdjust(adjustValue);
        }

        public void SetMicVolumeAdjustTrackBar(int volume)
        {
            MicVolumeAdjustTrackBar.Value = volume;
        }

        private void MicVolumeAdjustTrackBar_Scroll(object sender, EventArgs e)
        {
            // -20: -10dB, 20: +20dB
            var adjustValue = MicVolumeAdjustTrackBar.Value;
            converter?.SetMicVolumeAdjust(adjustValue);
        }
    }
}
