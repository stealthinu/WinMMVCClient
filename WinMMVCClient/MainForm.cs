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
        //private WaveIn waveIn;
        //private WasapiCapture waveIn;
        private WasapiOut waveOut;
        //private WaveOut waveOut;
        //private DirectSoundOut waveOut;
        private MMDeviceCollection inputs;
        private MMDeviceCollection outputs;
        private MMDevice inputDevice;
        private MMDevice outputDevice;
                private Converter listener;
        private readonly IConfiguration conf;

        public PlotModel plotmodel = new PlotModel();
        private LinearAxis _linearaxis1 = new LinearAxis
        {
            Position = AxisPosition.Bottom
        };
        private LinearAxis _linearaxis2 = new LinearAxis
        {
            Minimum = -1.0,
            Maximum = 1.0,
            Position = AxisPosition.Left
        };
        public LineSeries lineSeries = new LineSeries();

        public void InitPlot()
        {
            plotmodel.Axes.Add(_linearaxis1);
            plotmodel.Axes.Add(_linearaxis2);
            plotmodel.Series.Add(lineSeries);
            this.plotView1.Model = plotmodel;
        }

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
            foreach (var device in inputs)
            {
                comboBoxInput.Items.Add(device);
            }
            comboBoxInput.SelectedItem = inputName;
            comboBoxOutput.Items.Clear();
            foreach (var device in outputs)
            {
                comboBoxOutput.Items.Add(device);
            }
            comboBoxOutput.SelectedItem = outputName;
        }

        private void start()
        {
            if (!(comboBoxInput.SelectedItem is MMDevice && comboBoxInput.SelectedItem is MMDevice)) return;
            inputDevice = (MMDevice)comboBoxInput.SelectedItem;
            outputDevice = (MMDevice)comboBoxOutput.SelectedItem;

            WaveFormat waveFormat = new WaveFormat(24000, 1); // 24K mono

            //waveIn = new WasapiCapture(mics[micDeviceNumber]);
            //waveIn.WaveFormat = new WaveFormat(24000, 1); // 24K mono
            //waveIn = new WaveIn();
            //waveIn = new WaveInEvent();
            //waveIn.DeviceNumber = micDeviceNumber;
            //waveIn.NumberOfBuffers = 8192;

            //waveOut = new WasapiOut(speakers[speakerDeviceNumber], AudioClientShareMode.Shared, useEventSync: true, 100);
            //waveOut = new WaveOut();
            //waveOut = new DirectSoundOut();
            //waveOut.DeviceNumber = speakerDeviceNumber;

            var bufferedWaveProvider = new BufferedWaveProvider(waveFormat);
            bufferedWaveProvider.DiscardOnBufferOverflow = true;
            //var volumedWaveProvider = new VolumeWaveProvider16(bufferedWaveProvider) { Volume = 2.0f };
            var outputWaveProvider = new BufferedWaveProvider(waveFormat);
            outputWaveProvider.DiscardOnBufferOverflow = true;

            InitPlot();

            listener?.Dispose();
            listener = new Converter(inputDevice, outputDevice, waveFormat, plotView1.Model, lineSeries);
            listener.Start();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            start();
        }
    }
}