using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using Microsoft.ML;
using Microsoft.ML.Data;
using Microsoft.ML.Transforms.Onnx;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using NAudio.Utils;
using static System.Collections.Specialized.BitVector32;
using System.Windows.Forms;
using System.Threading.Channels;
using System.Diagnostics;
using System.Numerics;
using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using System.Configuration;
using System.Windows.Forms.Design;
using System.Buffers;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot;
using static System.Runtime.InteropServices.JavaScript.JSType;
using NAudio.Dsp;

namespace WinMMVCClient
{
    /*
    public sealed class MMVCInput
    {
        public TensorFloat specs; // shape(1,257,-1)
        public TensorInt64Bit lengths; // shape(1)
        public TensorInt64Bit sid_src; // shape(1)
        public TensorInt64Bit sid_tgt; // shape(1)
    }

    public sealed class MMVCOutput
    {
        public TensorFloat audio; // shape(1,1,-1)
    }

    public sealed class MMVCModel
    {
        private LearningModel model;
        private LearningModelSession session;
        private LearningModelBinding binding;
        public static async Task<G_50000Model> CreateFromStreamAsync(IRandomAccessStreamReference stream)
        {
            MMVCModel learningModel = new MMVCModel();
            learningModel.model = await LearningModel.LoadFromStreamAsync(stream);
            learningModel.session = new LearningModelSession(learningModel.model);
            learningModel.binding = new LearningModelBinding(learningModel.session);
            return learningModel;
        }
        public async Task<MMVCOutput> EvaluateAsync(MMVCInput input)
        {
            binding.Bind("specs", input.specs);
            binding.Bind("lengths", input.lengths);
            binding.Bind("sid_src", input.sid_src);
            binding.Bind("sid_tgt", input.sid_tgt);
            var result = await session.EvaluateAsync(binding, "0");
            var output = new MMVCOutput();
            output.audio = result.Outputs["audio"] as TensorFloat;
            return output;
        }
    }
    */

    public class OnnxConverter
    {
        private MLContext mlContext;
        private InferenceSession session;

        /*
        INPUTS
            name: specs
            type: float32[1,257,length]

            name: lengths
            type: int64[1]

            name: sid_src
            type: int64[1]

            name: sid_tgt
            type: int64[1]

        OUTPUTS
            name: audio
            type: float32[1,1,Tanhaudio_dim_2]
         */

        public class OnnxInput
        {
            [ColumnName("specs")]
            [VectorType(257)]
            public float[] Specs { get; set; }

            [ColumnName("lengths"), OnnxMapType(typeof(Int64), typeof(Single))]
            public Int64 Lengths { get; set; }

            [ColumnName("sid_src"), OnnxMapType(typeof(Int64), typeof(Single))]
            public Int64 SidSrc { get; set; }

            [ColumnName("sid_tgt"), OnnxMapType(typeof(Int64), typeof(Single))]
            public Int64 SidTgt { get; set; }
        }

        public class OnnxOutput
        {
            [ColumnName("audio")]
            [VectorType(1)]
            public float[] Audio { get; set; }
        }

        public OnnxConverter()
        {
            var assetsRelativePath = @"..\..\..\..\assets";
            string assetsPath = GetAbsolutePath(assetsRelativePath);
            var modelFilePath = Path.Combine(assetsPath, "G_50000.onnx");
            var outputFolder = Path.Combine(assetsPath, "output");

            mlContext = new MLContext();

            // ONNXオプション指定
            var opts = new SessionOptions();
            opts.ExecutionMode = ExecutionMode.ORT_PARALLEL;

            Console.WriteLine($"Read model: {modelFilePath}");
            session = new InferenceSession(modelFilePath, opts);
        }

        public float[] Infer(float[] wavSegment)
        {
            // specsだから4096を128毎で257chのspec作らないといけなかった
            // 複素数データに変換
            int segmentSize = 4096;
            int hopSize = 128;
            int winSize = 512;
            int paddingSize = winSize / hopSize / 2;
            int truncationSize = winSize / hopSize / 2; // 2
            int specNum = segmentSize / hopSize; // 32
            float[] window = Array.ConvertAll(Window.Hann(winSize), d => (float)d);

            //Complex[] complexWav = ArrayPool<Complex>.Shared.Rent(winSize);
            Complex[] complexWav = new Complex[winSize];
            float[] specs = ArrayPool<float>.Shared.Rent(specNum - truncationSize * 2);
            for (int n = 0; n < specNum - truncationSize * 2; n++) // 今回は窓に入らない部分はpaddingせず使わない
            {
                var start = n * hopSize;
                var end = start + winSize;
                var wav = wavSegment.AsSpan()[start..end];
                for (int i = 0; i < winSize; i++)
                {
                    complexWav[i] = wav[i] * window[i];
                }
                Fourier.Forward(complexWav, FourierOptions.Matlab);
            }

            /*
            for (int specIndex = 0; specIndex < specNum; specIndex++)
            {
                double spec = 0;
                for (int i = 0; i < windowSize; i++)
                {
                    var len = Complex.Abs(x[i]);
                    spec += len;
                }

                specs[specIndex] = (float) spec;
            }
            */

            /*
            if (specs.Length != 257 * segmentSize)
            {
                Debug.WriteLine($"specs.Length: {specs.Length}");
            }
            var specsDims = session.InputMetadata["specs"].Dimensions;
            specsDims[2] = segmentSize;
            var specsTensor = new DenseTensor<float>(   //Microsoft.ML.OnnxRuntime.Tensors.DenseTensor
                specs, // 音声データ(テンソル相当、floatの一次元配列)
                specsDims // 入力データ次元 [1, 257, length]
            );
            var lengths = new DenseTensor<Int64>(new[] { 1 });
            var sidSrc = new DenseTensor<Int64>(new[] { 1 });
            var sidTgt = new DenseTensor<Int64>(new[] { 1 });
            lengths[0] = segmentSize;
            sidSrc[0] = 101;
            sidTgt[0] = 102;

            var namedOnnxValues = new List<NamedOnnxValue>   // Name と Value のリスト
            {
                NamedOnnxValue.CreateFromTensor("specs",   specsTensor),
                NamedOnnxValue.CreateFromTensor("lengths", lengths),
                NamedOnnxValue.CreateFromTensor("sid_src", sidSrc),
                NamedOnnxValue.CreateFromTensor("sid_tgt", sidTgt)
            };

            // 推論の実行
            var results = session.Run(namedOnnxValues);
            var floatArray = results.First().AsEnumerable<float>().ToArray();
            */
            float[] floatArray = new float[specNum];

            return floatArray;
        }

        string GetAbsolutePath(string relativePath)
        {
            string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;

            string fullPath = Path.Combine(rootPath, relativePath);

            return fullPath;
        }
    }

    public class Converter : IDisposable
    {
        private readonly WasapiCapture waveIn;
        private WasapiOut waveOut;
        private BufferedWaveProvider speakerWaveProvider;
        private OnnxConverter onnxConverter;
        private PlotModel plot;

        public double AmplitudeFrac { get; private set; }
        public double TotalSamples { get; private set; }
        public double TotalTimeSec { get { return (double)TotalSamples / SampleRate; } }
        private readonly List<double> audio = new List<double>();
        public int SamplesInMemory { get { return audio.Count; } }
        public int SampleRate { get; private set; }
        public int SpeakerLatency { get; private set; }
        public int SegmentSize { get; private set; }
        public int bytesPerSample = 2;
        public int maxSample = 32768;
        public PlotModel _plot;
        public LineSeries _line;
        public PlotModel _spectrogram;
        public HeatMapSeries _heatmap;
        public const int fftnum = 512;
        public float[,] Data = new float[100, fftnum / 2];

        public Converter(MMDevice mic, MMDevice speaker, WaveFormat waveFormat, PlotModel plot, LineSeries line, PlotModel spectrogram, HeatMapSeries heatmap, int segmentSize =4096, int speakerLatency=100)
        {
            onnxConverter = new OnnxConverter();
            _plot = plot;
            _line = line;
            _spectrogram = spectrogram;
            _heatmap = heatmap;

            SegmentSize = segmentSize;
            SpeakerLatency = speakerLatency;
            SampleRate = waveFormat.SampleRate;
            speakerWaveProvider = new BufferedWaveProvider(waveFormat);
            speakerWaveProvider.DiscardOnBufferOverflow = true;
            waveOut = new WasapiOut(speaker, AudioClientShareMode.Shared, useEventSync: true, latency: speakerLatency);
            waveOut.Init(speakerWaveProvider);
            waveIn = new WasapiCapture(mic);
            waveIn.WaveFormat = waveFormat;
            bytesPerSample = waveFormat.BitsPerSample / 8;
            maxSample = (1 << (bytesPerSample * 8 - 1));
            waveIn.DataAvailable += OnNewAudioData;
        }

        private void OnNewAudioData(object sender, WaveInEventArgs args)
        {
            int newSampleCount = args.BytesRecorded / bytesPerSample;
            double[] buffer = BytesToDoubleArray(args.Buffer, newSampleCount); // new double[newSampleCount];
            AmplitudeFrac = buffer.Max();
            TotalSamples += newSampleCount;
            var buffer4 = Array.ConvertAll(buffer, n => n * 4.0); // 音量4倍にしてみる
            audio.AddRange(buffer4);
            if (audio.Count >= SegmentSize)
            {
                var convertAudio = GetNewAudio();
                //var convertedBytes = DoubleToBytesArray(convertAudio);
                var specs = new float[257 * 4096 / 128];
                var wav = new float[4096];
                for (int i = 0; i < convertAudio.Length; i++)
                {
                    wav[i] = (float)convertAudio[i];
                }
                var audio = onnxConverter.Infer(wav); // テスト
                ProcessSample(wav);
                ProcessSpectrogram(sample);
                var convertedBytes = FloatToBytesArray(audio);
                speakerWaveProvider.AddSamples(convertedBytes, 0, convertedBytes.Length);
            }
        }

        public void ProcessSample(float[] sample)
        {
            _line.Points.Clear();
            for (int i = 0; i < sample.Length; i++)
            {
                _line.Points.Add(new DataPoint((double)i, sample[i]));
            }
            _plot.InvalidatePlot(true);
        }

        public void ProcessSpectrogram(float[] sample)
        {
            SoundData[count] = sample * (float)FastFourierTransform.HammingWindow(count, 1024);
            count++;
            if (count == 1024)
            {
                MakeFFT(SoundData);
                plotModelFFT.InvalidatePlot(true);
                count = 0;
            }

            DataPointsRT.Add(new DataPoint(counttime, sample));
            if (DataPointsRT.Count > 1024) DataPointsRT.RemoveAt(0);
            plotModelRT.InvalidatePlot(true);
            counttime = counttime + 1.0 / 8000.0;
        }

        public void MakeFFT(float[] data)
        {
            var fft = ExecuteFFT(data);
            DataPointsFFT.Clear();
            for (int i = 0; i < 512; i++)
            {
                DataPointsFFT.Add(new DataPoint(i * 4000.0 / 512.0, fft[i]));
            }
        }

        public float[] ExecuteFFT(float[] data)
        {
            int len = data.Count();
            int m = (int)Math.Log((double)data.Count(), 2);
            var fftSample = data.Select(v => new Complex { X = v, Y = 0.0f }).ToArray();
            FastFourierTransform.FFT(true, m, fftSample);
            var ret = new float[len / 2];
            for (int i = 0; i < len / 2; i++)
            {
                ret[i] = (float)Math.Sqrt(fftSample[i].X * fftSample[i].X
                        + fftSample[i].Y * fftSample[i].Y) * 2.0f;
            }
            return ret;
        }
        
        // 振幅データをスペクトログラム配列に追加する
        private void AddSpectrogram(float[] data)
        {
            for (int i = 0; i < 99; i++)
            {
                for (int j = 0; j < fftnum / 2; j++)
                {
                    Data[i, j] = Data[i + 1, j];
                }
            }
            for (int j = 0; j < fftnum / 2; j++)
            {
                Data[99, j] = data[j];
            }

            var pldata = new double[100, fftnum / 2];
            Array.Copy(Data, pldata, Data.Length);
            _heatmap.Data = pldata;
        }

        public void Start()
        {
            waveOut.Play();
            waveIn.StartRecording();
        }

        public void Dispose()
        {
            waveIn?.StopRecording();
            waveIn?.Dispose();
            waveOut?.Stop();
            waveOut?.Dispose();
        }

        private double[] GetNewAudio()
        {
            var count = SegmentSize;
            if (audio.Count < SegmentSize)
                count = audio.Count;
            double[] values = new double[count];
            for (int i = 0; i < count; i++)
                values[i] = audio[i];
            audio.RemoveRange(0, count);
            return values;
        }

        private double[] BytesToDoubleArray(byte[] bytesBuffer, int newSampleCount)
        {
            double[] buffer = new double[newSampleCount];
            for (int i = 0; i < newSampleCount; i++)
            {
                buffer[i] = BitConverter.ToInt16(bytesBuffer, i * bytesPerSample) / (double)maxSample; // -1.0 .. 1.0
            }
            return buffer;
        }

        private byte[] DoubleToBytesArray(double[] doubleArray)
        {
            byte[] bytes = new byte[doubleArray.Length * bytesPerSample];
            for (int i = 0; i < doubleArray.Length; i++)
            {
                Int16 val = (Int16)(doubleArray[i] * (double)maxSample);
                byte[] vals = BitConverter.GetBytes(val);
                bytes[i * 2 + 0] = vals[0];
                bytes[i * 2 + 1] = vals[1];
            }
            return bytes;
        }

        private byte[] FloatToBytesArray(float[] floatArray)
        {
            byte[] bytes = new byte[floatArray.Length * bytesPerSample];
            for (int i = 0; i < floatArray.Length; i++)
            {
                Int16 val = (Int16)(floatArray[i] * (float)maxSample);
                byte[] vals = BitConverter.GetBytes(val);
                bytes[i * 2 + 0] = vals[0];
                bytes[i * 2 + 1] = vals[1];
            }
            return bytes;
        }
    }
}
