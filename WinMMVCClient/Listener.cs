using NAudio.CoreAudioApi;
using NAudio.Wave;
using Microsoft.ML;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Buffers;
using OxyPlot.Series;
using OxyPlot;
using NAudio.Dsp;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using NumSharp;

namespace WinMMVCClient
{
    public class OnnxConverter
    {
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

        public OnnxConverter(string modelFilePath, IConfiguration conf)
        {
            // ONNXオプション指定
            // DirectMLの場合はパッケージを「Microsoft.ML.OnnxRuntime.DirectML」を入れる
            // CUDAの場合はパッケージを「Microsoft.ML.OnnxRuntime.Gpu」を入れる
            // Pythonの時と同様「Microsoft.ML.OnnxRuntime」を入れるとCPUでの変換になってしまうので注意
            // ※下記はDirectML用のオプション指定
            //var opts = new SessionOptions();
            //opts.ExecutionMode = ExecutionMode.ORT_SEQUENTIAL;
            //opts.EnableMemoryPattern = false;
            //session = new InferenceSession(modelFilePath, opts);
            session = new InferenceSession(modelFilePath, SessionOptions.MakeSessionOptionWithCudaProvider(0));
        }

        public float[] Infer(float[,] specs, int srcId, int tgtId)
        {
            var specsLength = specs.GetLength(0); // Spectrogramの時間長
            var specsNum = specs.GetLength(1);  // Spectrogramの周波数チャンネル数 256
            var specsDims = session.InputMetadata["specs"].Dimensions; // 入力データ次元 [1, 257, length]
            specsDims[2] = specsLength;
            var specChannels = specsDims[1]; // ONNXのSpectrogramチャンネル数 257
            // ONNXに入れるためフラットな1次元の配列にする [1, 257, length] の順
            float[] flattedSpecs = new float[specsLength * specChannels]; // ArrayPool使うと入力テンソルサイズが違うと言われるので使わない AsSpanもダメ
            for (int fNum = 0; fNum < specsNum; fNum++) // FFTした結果は 256 でONNXのチャンネル数とは1違う
            {
                for (int sNum = 0; sNum < specsLength; sNum++)
                {
                    flattedSpecs[fNum * specsLength + sNum] = specs[sNum, fNum];
                }
            }
            var specsTensor = new DenseTensor<float>(
                flattedSpecs, // 音声データ(テンソル相当、floatの1次元配列)
                specsDims // 入力データ次元 [1, 257, length]
            );
            var lengths = new DenseTensor<Int64>(new[] { 1 });
            var sidSrc = new DenseTensor<Int64>(new[] { 1 });
            var sidTgt = new DenseTensor<Int64>(new[] { 1 });
            lengths[0] = specs.Length;
            sidSrc[0] = srcId;
            sidTgt[0] = tgtId;

            var namedOnnxValues = new List<NamedOnnxValue>
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
    }

    public class Converter : IDisposable
    {
        public int SidSrc { get; set; }
        public int SidTgt { get; set; }
        public double AmplitudeFrac { get; private set; }
        public double TotalSamples { get; private set; }
        public int SampleRate { get; private set; }
        public double TotalTimeSec { get { return (double)TotalSamples / SampleRate; } }
        public int SpeakerLatency { get; private set; }
        public int SegmentSize { get; private set; }
        public int HopSize { get; private set; }
        public int WinSize { get; private set; }
        public int BytesPerSample { get; private set; }
        public int MaxSample { get; private set; }

        private readonly WasapiCapture waveIn;
        private WasapiOut waveOut;
        private BufferedWaveProvider speakerWaveProvider;
        private OnnxConverter onnxConverter;
        public PlotModel _waveView;
        public LineSeries _waveLine;

        private readonly List<float> audio;
        private float[] oldWavBuffer;

        public Converter(MMDevice mic, MMDevice speaker, WaveFormat waveFormat, IConfiguration conf, PlotModel waveView, LineSeries waveLine, int segmentSize=4096, int hopSize=128, int winSize=512, int speakerLatency=100)
        {
            var rootPath = conf["root_path"];
            var modelFile = conf["model_file"];
            var modelFilePath = Path.Combine(rootPath, modelFile);
            var outputFolder = Path.Combine(rootPath, "output");

            onnxConverter = new OnnxConverter(modelFilePath, conf);
            _waveView = waveView;
            _waveLine = waveLine;

            SegmentSize = segmentSize;
            HopSize = hopSize;
            WinSize = winSize;
            oldWavBuffer = Enumerable.Repeat<float>(0.0f, WinSize / 2).ToArray(); // スペクトログラム作成用に過去のwavをWinSize半分だけ保持
            BytesPerSample = 2;
            MaxSample = 32768;
            SpeakerLatency = speakerLatency;
            SampleRate = waveFormat.SampleRate;
            audio = new List<float>();
            speakerWaveProvider = new BufferedWaveProvider(waveFormat);
            speakerWaveProvider.DiscardOnBufferOverflow = true;
            waveOut = new WasapiOut(speaker, AudioClientShareMode.Shared, useEventSync: true, latency: speakerLatency);
            waveOut.Init(speakerWaveProvider);
            waveIn = new WasapiCapture(mic);
            waveIn.WaveFormat = waveFormat;
            BytesPerSample = waveFormat.BitsPerSample / 8;
            MaxSample = (1 << (BytesPerSample * 8 - 1));
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
            int newSampleCount = args.BytesRecorded / BytesPerSample;
            float[] newWavBuffer = BytesToFloatArray(args.Buffer, newSampleCount);
            float[] wavBuffer = new float[oldWavBuffer.Length + newWavBuffer.Length]; // oldWavBufferとnewWavBufferを繋げたものを作る
            Array.Copy(oldWavBuffer, wavBuffer, oldWavBuffer.Length);
            Array.Copy(newWavBuffer, 0, wavBuffer, oldWavBuffer.Length, newWavBuffer.Length);
            Array.Copy(newWavBuffer, newWavBuffer.Length - oldWavBuffer.Length, oldWavBuffer, 0, oldWavBuffer.Length); // newWavBufferの最後をoldWavBufferとして保持する
            AmplitudeFrac = wavBuffer.Max();
            TotalSamples += newSampleCount;
            audio.AddRange(wavBuffer);
            if (audio.Count >= SegmentSize)
            {
                var wav = GetNewAudio(SegmentSize);
                var specs = MakeSpectrogram(wav);
                var audio = onnxConverter.Infer(specs, 101, 102);
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

        public float[,] MakeSpectrogram(float[] wav, int segmentSize=4096, int hopSize=128, int winSize=512, float amplify=0.10f)
        {
            // 4096を128毎で257chのspectrogramを作る
            int m = (int)Math.Log((double)winSize, 2); // winSizeの2のべき数(512=2^9)
            int truncationSize = winSize / hopSize / 2; // 2
            int specNum = segmentSize / hopSize; // 32

            // NAudioのFFTを利用する　そのためComplexもNAudioのものを利用
            //NAudio.Dsp.Complex[] complexWav = new NAudio.Dsp.Complex[winSize];
            NAudio.Dsp.Complex[] complexWav = ArrayPool<NAudio.Dsp.Complex>.Shared.Rent(winSize);
            float[,] specs = new float[specNum - truncationSize * 2, winSize / 2]; // FFTの結果はwinSizeの半分だけ有効
            for (int n = 0; n < specNum - truncationSize * 2; n++) // 今回は窓に入らない部分はpaddingせず使わない
            {
                // Hann窓掛けてComplex化
                var start = n * hopSize;
                var end = start + winSize;
                var wavWin = wav.AsSpan()[start..end];
                for (int i = 0; i < winSize; i++)
                {
                    complexWav[i].X = wavWin[i] * (float)FastFourierTransform.HannWindow(i, winSize);
                    complexWav[i].Y = 0;
                }
                // STFT
                FastFourierTransform.FFT(true, m, complexWav);
                // STFT結果の大きさをスペクトログラムに保存 winSizeが512だと有効なのは半分の256
                // Python: spec = torch.sqrt(spec.pow(2).sum(-1) + 1e-6)
                for (int i = 0; i < winSize / 2; i++)
                {
                    specs[n, i] = (float)Math.Sqrt(complexWav[i].X * complexWav[i].X + complexWav[i].Y * complexWav[i].Y + 1e-6) * amplify;
                }
            }

            return specs;
        }

        private float[] GetNewAudio(int segmentSize)
        {
            var count = segmentSize;
            if (audio.Count < segmentSize)
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
                buffer[i] = BitConverter.ToInt16(bytesBuffer, i * BytesPerSample);
            }
            return buffer;
        }

        private byte[] FloatToWavArray(float[] floatArray, float amplify)
        {
            byte[] bytes = new byte[floatArray.Length * BytesPerSample];
            for (int i = 0; i < floatArray.Length; i++)
            {
                Int16 val = (Int16)(floatArray[i] * amplify);
                byte[] vals = BitConverter.GetBytes(val);
                bytes[i * 2 + 0] = vals[0];
                bytes[i * 2 + 1] = vals[1];
            }
            return bytes;
        }

        string GetAbsolutePath(string relativePath)
        {
            string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(rootPath, relativePath);

            return fullPath;
        }
    }
}
