using Microsoft.Extensions.Configuration;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using System.Diagnostics;

namespace WinMMVCClient
{
    public partial class MainForm : Form
    {
        private MMDeviceCollection inputs;
        private MMDeviceCollection outputs;
        private MMDevice inputDevice;
        private MMDevice outputDevice;
        private Converter listener;
        private IConfiguration conf;

        public PlotModel plotModelWave = new PlotModel();
        private LinearAxis _linearaxis1 = new LinearAxis
        {
            Position = AxisPosition.Bottom
        };
        private LinearAxis _linearaxis2 = new LinearAxis
        {
            Minimum = -32768.0,
            Maximum = 32768.0,
            Position = AxisPosition.Left
        };
        public LineSeries lineSeries = new LineSeries();

        public void InitPlotWave()
        {
            plotModelWave.Axes.Add(_linearaxis1);
            plotModelWave.Axes.Add(_linearaxis2);
            plotModelWave.Series.Add(lineSeries);
            this.plotViewWave.Model = plotModelWave;
        }

        public MainForm()
        {
            InitializeComponent();
            string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
            var confFilePath = Path.Combine(rootPath, @"..\..\..\..\appsettings.json");
            conf = new ConfigurationBuilder().AddJsonFile(confFilePath).Build();
            setupInputOutputComboBox();
            setupVoiceListBox();
            InitPlotWave();
        }

        private void start()
        {
            //if (!(comboBoxInput.SelectedItem is MMDevice && comboBoxInput.SelectedItem is MMDevice)) return;
            fixInputOutputComboBox();
            //inputDevice = (MMDevice)comboBoxInput.SelectedItem;
            //outputDevice = (MMDevice)comboBoxOutput.SelectedItem;

            listener?.Dispose();
            listener = new Converter((MMDevice)comboBoxInput.SelectedItem, (MMDevice)comboBoxOutput.SelectedItem, conf, plotViewWave.Model, lineSeries);
            listener.Start();
        }

        private void stop()
        {
            listener?.Dispose();
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
            string inputName = conf["input"];
            string outputName = conf["output"];
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

        private List<WaveInCapabilities> getWaveInCapabilities()
        {
            List<WaveInCapabilities> sources = new List<WaveInCapabilities>();

            for (int i = 0; i < WaveIn.DeviceCount; i++)
            {
                sources.Add(WaveIn.GetCapabilities(i));
            }
            return sources;
        }

        private List<WaveOutCapabilities> getWaveOutCapabilities()
        {
            List<WaveOutCapabilities> sources = new List<WaveOutCapabilities>();

            for (int i = 0; i < WaveOut.DeviceCount; i++)
            {
                sources.Add(WaveOut.GetCapabilities(i));
            }
            return sources;
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
            var list = conf.GetSection("voice_list");
            var arr = list.AsEnumerable().ToArray();
            foreach (var ary in list.AsEnumerable())
            {
                if (String.IsNullOrEmpty(ary.Value))
                    continue;
                var keys = ary.Key.Split(':');
                var index = Convert.ToInt32(keys[1]);
                var id = Convert.ToInt32(keys[2]);
                var name = ary.Value;
                voiceList.Add(new Voice(index, id, name));
            }
            voiceList.Sort((a, b) => a.Index - b.Index);
            listBoxTarget.DataSource = voiceList;
            var targetId = Convert.ToInt32(conf["target_id"]);
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
            listener?.setTargetId(targetVoice.ID);
        }
    }
}
