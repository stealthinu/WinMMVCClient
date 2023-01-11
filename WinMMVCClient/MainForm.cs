using Microsoft.Extensions.Configuration;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using System.Diagnostics;

namespace WinMMVCClient
{
    public partial class MainForm : Form
    {
        //private WaveIn waveIn;
        //private WasapiCapture waveIn;
        private WasapiOut waveOut;
        //private WaveOut waveOut;
        //private DirectSoundOut waveOut;
        private MMDeviceCollection inputs;
        private MMDeviceCollection outputs;
        //private Converter listener;
        private readonly IConfiguration conf;

        public MainForm()
        {
            InitializeComponent();
            string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
            var confFilePath = Path.Combine(rootPath, @"..\..\..\..\appsettings.json");
            var conf = new ConfigurationBuilder().AddJsonFile(confFilePath).Build();
            Debug.WriteLine(conf["Input"]);
            Debug.WriteLine(conf["Output"]);
            setupInputOutput(conf["Input"], conf["Output"]);
        }

        private void setupInputOutput(string inputName, string outputName)
        {
            inputs = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            outputs = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            comboBoxInput.Items.Clear();
            foreach (var input in inputs)
            {
                comboBoxInput.Items.Add(input.DeviceFriendlyName);
            }
            comboBoxInput.SelectedItem = inputName;
            comboBoxOutput.Items.Clear();
            foreach (var output in outputs)
            {
                comboBoxOutput.Items.Add(output.DeviceFriendlyName);
            }
            comboBoxOutput.SelectedItem = outputName;
        }
    }
}