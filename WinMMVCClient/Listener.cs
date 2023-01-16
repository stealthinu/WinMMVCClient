using NAudio.CoreAudioApi;
using NAudio.Wave;
using Microsoft.ML;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System.Buffers;
using OxyPlot.Series;
using OxyPlot;
using NAudio.Dsp;

namespace WinMMVCClient
{
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

        public OnnxConverter()
        {
            var assetsRelativePath = @"..\..\..\..\..\assets";
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

        public float[] Infer(float[,] specs)
        {
            var specsLength = specs.GetLength(0); // Spectrogramの時間長
            var specsNum = specs.GetLength(1);  // Spectrogramの周波数チャンネル数 256
            var specsDims = session.InputMetadata["specs"].Dimensions; // 入力データ次元 [1, 257, length]
            specsDims[2] = specsLength;
            // ONNXに入れるためフラットな1次元の配列にする [1, 257, length] の順
            var flattedSpecs = new float[specsLength * 257]; // ONNXの入力は 257
            for (int fNum = 0; fNum < specsNum; fNum++) // FFTした結果は 256
            {
                for (int sNum = 0; sNum < specsLength; sNum++)
                {
                    flattedSpecs[fNum * specsLength + sNum] = specs[sNum, fNum];
                }
            }
            var specsTensor = new DenseTensor<float>(   //Microsoft.ML.OnnxRuntime.Tensors.DenseTensor
                flattedSpecs, // 音声データ(テンソル相当、floatの1次元配列)
                specsDims // 入力データ次元 [1, 257, length]
            );
            var lengths = new DenseTensor<Int64>(new[] { 1 });
            var sidSrc = new DenseTensor<Int64>(new[] { 1 });
            var sidTgt = new DenseTensor<Int64>(new[] { 1 });
            lengths[0] = specs.Length;
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

        public double AmplitudeFrac { get; private set; }
        public double TotalSamples { get; private set; }
        public double TotalTimeSec { get { return (double)TotalSamples / SampleRate; } }
        private readonly List<float> audio = new List<float>();
        public int SamplesInMemory { get { return audio.Count; } }
        public int SampleRate { get; private set; }
        public int SpeakerLatency { get; private set; }
        public int SegmentSize { get; private set; }
        public int bytesPerSample = 2;
        public int maxSample = 32768;
        public PlotModel _waveView;
        public LineSeries _waveLine;
        public PlotModel _spectrogram;
        public HeatMapSeries _heatmap;

        public Converter(MMDevice mic, MMDevice speaker, WaveFormat waveFormat, PlotModel waveView, LineSeries waveLine, int segmentSize=4096, int speakerLatency=100)
        {
            onnxConverter = new OnnxConverter();
            _waveView = waveView;
            _waveLine = waveLine;

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

        private void OnNewAudioData(object sender, WaveInEventArgs args)
        {
            int newSampleCount = args.BytesRecorded / bytesPerSample;
            float[] buffer = BytesToFloatArray(args.Buffer, newSampleCount);
            AmplitudeFrac = buffer.Max();
            TotalSamples += newSampleCount;
            audio.AddRange(buffer);
            if (audio.Count >= SegmentSize)
            {
                var wav = GetNewAudio();
                var specs = MakeSpectrogram(wav);
                var audio = onnxConverter.Infer(specs);
                var convertedBytes = FloatToWavArray(audio, 16384);
                speakerWaveProvider.AddSamples(convertedBytes, 0, convertedBytes.Length);
                ProcessSample(wav);
            }
        }

        public void ProcessSample(float[] sample)
        {
            _waveLine.Points.Clear();
            for (int i = 0; i < sample.Length; i++)
            {
                _waveLine.Points.Add(new DataPoint((double)i, sample[i]));
            }
            _waveView.InvalidatePlot(true);
        }

        public float[,] MakeSpectrogram(float[] wav)
        {
            // specsだから4096を128毎で257chのspec作らないといけなかった
            // 複素数データに変換
            int segmentSize = 4096;
            int hopSize = 128;
            int winSize = 512; // winSizeが512なので有効なのは256まで
            int m = 9; // winSizeの2のべき数(512=2^9)
            int truncationSize = winSize / hopSize / 2; // 2
            int specNum = segmentSize / hopSize; // 32

            // NAudioのFFTを利用する　そのためComplexもNAudioのものを利用
            //NAudio.Dsp.Complex[] complexWav = ArrayPool<NAudio.Dsp.Complex>.Shared.Rent(winSize);
            NAudio.Dsp.Complex[] complexWav = new NAudio.Dsp.Complex[winSize];
            //float[,] specs = ArrayPool<float[,]>.Shared.Rent(specNum - truncationSize * 2, winSize / 2);
            float[,] specs = new float[specNum - truncationSize * 2, winSize / 2];
            for (int n = 0; n < specNum - truncationSize * 2; n++) // 今回は窓に入らない部分はpaddingせず使わない
            {
                // Hann窓掛けてComplex化
                var start = n * hopSize;
                var end = start + winSize;
                var wavWin = wav.AsSpan()[start..end];
                for (int i = 0; i < winSize; i++)
                {
                    var w = wavWin[i];
                    var hw = (float)FastFourierTransform.HannWindow(i, winSize);
                    //complexWav[i].X = wavWin[i] * (float)FastFourierTransform.HannWindow(i, winSize);
                    complexWav[i].X = w * hw;
                    complexWav[i].Y = 0;
                }
                // STFT
                FastFourierTransform.FFT(true, m, complexWav);
                // STFT結果の大きさをスペクトログラムに保存
                // Python: spec = torch.sqrt(spec.pow(2).sum(-1) + 1e-6)
                for (int i = 0; i < winSize / 2; i++)
                {
                    specs[n, i] = (float)Math.Sqrt(complexWav[i].X * complexWav[i].X + complexWav[i].Y * complexWav[i].Y + 1e-6);
                }
            }

            return specs;
        }

        private float[] GetNewAudio()
        {
            var count = SegmentSize;
            if (audio.Count < SegmentSize)
                count = audio.Count;
            float[] values = new float[count];
            for (int i = 0; i < count; i++)
                values[i] = audio[i];
            audio.RemoveRange(0, count);
            return values;
        }

        private float[] BytesToFloatArray(byte[] bytesBuffer, int newSampleCount)
        {
            float[] buffer = new float[newSampleCount];
            for (int i = 0; i < newSampleCount; i++)
            {
                buffer[i] = BitConverter.ToInt16(bytesBuffer, i * bytesPerSample);
            }
            return buffer;
        }

        private byte[] FloatToWavArray(float[] floatArray, float amplify)
        {
            byte[] bytes = new byte[floatArray.Length * bytesPerSample];
            for (int i = 0; i < floatArray.Length; i++)
            {
                Int16 val = (Int16)(floatArray[i] * amplify);
                byte[] vals = BitConverter.GetBytes(val);
                bytes[i * 2 + 0] = vals[0];
                bytes[i * 2 + 1] = vals[1];
            }
            return bytes;
        }
    }
}
