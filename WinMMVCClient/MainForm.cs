using Microsoft.Extensions.Configuration;
using NAudio.CoreAudioApi;
using NAudio.Wave;

namespace WinMMVCClient
{
    public partial class MainForm : Form
    {
        private MMDeviceCollection inputs;
        private MMDeviceCollection outputs;
        private MMDevice inputDevice;
        private MMDevice outputDevice;
        private Converter converter;
        public IConfiguration conf { get; private set; }
        public IConfiguration hps { get; private set; }

        public MainForm()
        {
            InitializeComponent();
            string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
            var confFilePath = Path.Combine(rootPath, @"..\conf\myprofile.conf");
            conf = new ConfigurationBuilder().AddJsonFile(confFilePath).Build();
            var hpsFilePath = conf["path:json"];
            hps = new ConfigurationBuilder().AddJsonFile(hpsFilePath).Build();
            setupInputOutputComboBox();
            setupVoiceListBox();
        }

        private void start()
        {
            if (!(comboBoxInput.SelectedItem is MMDevice && comboBoxInput.SelectedItem is MMDevice)) return;
            fixInputOutputComboBox();
            inputDevice = (MMDevice)comboBoxInput.SelectedItem;
            outputDevice = (MMDevice)comboBoxOutput.SelectedItem;

            converter?.Dispose();
            converter = new Converter(inputDevice, outputDevice, conf, hps);
            converter.Start();
        }

        private void stop()
        {
            converter?.Dispose();
            unfixInputOutputComboBox();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            start();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            stop();
        }

        private void setupInputOutputComboBox()
        {
            string inputName = conf["device:input_device1"];
            string outputName = conf["device:output_device"];
            inputs = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            var inputsArray = inputs.ToArray();
            outputs = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            var outputsArray = outputs.ToArray();
            comboBoxInput.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxInput.Items.Clear();
            comboBoxInput.Items.AddRange(inputsArray);
            comboBoxOutput.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxOutput.Items.Clear();
            comboBoxOutput.Items.AddRange(outputsArray);
            for (int i = 0; i < inputsArray.Length; i++)
            {
                if (inputsArray[i].FriendlyName == inputName)
                    comboBoxInput.SelectedIndex = i;
            }
            for (int i = 0; i < outputsArray.Length; i++)
            {
                if (outputsArray[i].FriendlyName == outputName)
                    comboBoxOutput.SelectedIndex = i;
            }
        }

        private void fixInputOutputComboBox()
        {
            comboBoxInput.Enabled = false;
            comboBoxOutput.Enabled = false;
        }

        private void unfixInputOutputComboBox()
        {
            comboBoxInput.Enabled = true;
            comboBoxOutput.Enabled = true;
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

        private void setupVoiceListBox()
        {
            listBoxTarget.Items.Clear();
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
            listBoxTarget.DataSource = voiceList;
            var targetId = Convert.ToInt32(conf["vc_conf:target_id"]);
            for (int i = 0; i < voiceList.Count; i++)
            {
                if (voiceList[i].ID == targetId)
                {
                    listBoxTarget.SelectedIndex = i;
                    break;
                }
            }
        }

        private void listBoxTarget_SelectedIndexChanged(object sender, EventArgs e)
        {
            Voice targetVoice = (Voice)listBoxTarget.SelectedItem;
            converter?.setTargetId(targetVoice.ID);
        }

        private void trackBarMicVolumeAdjust_ValueChanged(object sender, EventArgs e)
        {
            // -20: -10dB, 20: +20dB
            var adjustValue = trackBarMicVolumeAdjust.Value / 2.0;
            converter?.setMicVolumeAdjust(adjustValue);
        }
    }
}
