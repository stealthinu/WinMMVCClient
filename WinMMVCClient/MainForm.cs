using Microsoft.Extensions.Configuration;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using System.Diagnostics;

namespace WinMMVCClient
{
    public class Conf
    {

    }
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
            setupInputOutput(conf["Input"], conf["Output"]);
        }

        private void start()
        {
            if (!(comboBoxInput.SelectedItem is MMDevice && comboBoxInput.SelectedItem is MMDevice)) return;
            fixInputOutput();
            inputDevice = (MMDevice)comboBoxInput.SelectedItem;
            outputDevice = (MMDevice)comboBoxOutput.SelectedItem;

            WaveFormat waveFormat = new WaveFormat(24000, 1); // 24K mono

            var bufferedWaveProvider = new BufferedWaveProvider(waveFormat);
            bufferedWaveProvider.DiscardOnBufferOverflow = true;
            var outputWaveProvider = new BufferedWaveProvider(waveFormat);
            outputWaveProvider.DiscardOnBufferOverflow = true;

            InitPlotWave();

            listener?.Dispose();
            listener = new Converter(inputDevice, outputDevice, waveFormat, plotViewWave.Model, lineSeries);
            listener.Start();
        }

        private void stop()
        {
            listener?.Dispose();
            unfixInputOutput();
        }

        private void buttonStart_Click(object sender, EventArgs e)
        {
            start();
        }

        private void buttonStop_Click(object sender, EventArgs e)
        {
            stop();
        }

        private void setupInputOutput(string inputName, string outputName)
        {
            inputs = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Capture, DeviceState.Active);
            outputs = new MMDeviceEnumerator().EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            comboBoxInput.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxInput.Items.Clear();
            comboBoxInput.Items.AddRange(inputs.ToArray());
            comboBoxInput.SelectedItem = inputName;
            comboBoxOutput.DropDownStyle = ComboBoxStyle.DropDownList;
            comboBoxOutput.Items.Clear();
            comboBoxOutput.Items.AddRange(outputs.ToArray());
            comboBoxOutput.SelectedItem = outputName;
        }

        private void fixInputOutput()
        {
            comboBoxInput.Enabled = false;
            comboBoxOutput.Enabled = false;
        }

        private void unfixInputOutput()
        {
            comboBoxInput.Enabled = true;
            comboBoxOutput.Enabled = true;
        }
    }
}