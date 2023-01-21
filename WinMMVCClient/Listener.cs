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
using System.Configuration;

namespace WinMMVCClient
{
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
        public int SpecChannels { get; private set; }
        public int OverlapSize { get; private set; }
        public int DisposeConv1dSpecs { get; private set; }
        public int BytesPerSample { get; private set; }
        public int MaxSample { get; private set; }

        private OnnxConverter onnxConverter;
        private readonly WasapiCapture waveIn;
        private WasapiOut waveOut;
        private BufferedWaveProvider speakerWaveProvider;
        public PlotModel _waveView;
        public LineSeries _waveLine;

        private readonly List<float> audioBuffer;
        private float[] newWavBuffer;
        private float[] prevWavBuffer;
        private float[] prevTransWav;
        private float[] wavBuffer;
        private float[] disposedWav;
        private float[] overlappedWav;
        private float[,] specs;
        private int truncationSpecs;
        private int segmentSpecs;
        private int prevStftWavSize;
        private int disposeConv1dSize;
        private int stftM;

        public Converter(MMDevice mic, MMDevice speaker, WaveFormat waveFormat, IConfiguration conf, PlotModel waveView, LineSeries waveLine)
        {
            SidSrc = Convert.ToInt32(conf["source_id"]);
            SidTgt = Convert.ToInt32(conf["target_id"]);
            SegmentSize = Convert.ToInt32(conf["segment_size"]);
            HopSize = Convert.ToInt32(conf["hop_size"]);
            WinSize = Convert.ToInt32(conf["win_size"]);
            SpecChannels = WinSize / 2; // STFT結果の大きさをスペクトログラムに保存 winSizeが512だと有効なのは半分の256
            OverlapSize = Convert.ToInt32(conf["overlapSize"]);
            truncationSpecs = WinSize / HopSize / 2; // 2 FFTするときに端で計算できないサイズ
            segmentSpecs = SegmentSize / HopSize; // 32 スペクトログラムの時間方向の数
            stftM = (int)Math.Log((double)WinSize, 2); // winSizeの2のべき数(512=2^9)
            prevStftWavSize = (((WinSize / HopSize) / 2) + 1) * HopSize; // スペクトログラム作成用に過去のwavを、WinSize半分ぶんのspecsに+1した長さだけ保持
            DisposeConv1dSpecs = Convert.ToInt32(conf["dispose_conv1d_specs"]);
            disposeConv1dSize = DisposeConv1dSpecs * HopSize;
            BytesPerSample = 2;
            MaxSample = 32768;
            SpeakerLatency = Convert.ToInt32(conf["speaker_latency"]);
            SampleRate = waveFormat.SampleRate;
            BytesPerSample = waveFormat.BitsPerSample / 8;
            MaxSample = (1 << (BytesPerSample * 8 - 1));

            newWavBuffer = Enumerable.Repeat<float>(0.0f, SegmentSize).ToArray();
            prevWavBuffer = Enumerable.Repeat<float>(0.0f,  prevStftWavSize + disposeConv1dSize * 2 + OverlapSize).ToArray();
            prevTransWav = Enumerable.Repeat<float>(0.0f, disposeConv1dSize * 2 + OverlapSize).ToArray();
            wavBuffer = new float[SegmentSize + prevStftWavSize + disposeConv1dSize * 2 + OverlapSize]; // prevWavBufferとnewWavBufferを繋げたものが入る
            disposedWav = new float[SegmentSize + prevStftWavSize + OverlapSize];
            overlappedWav = new float[SegmentSize + prevStftWavSize];
            specs = new float[segmentSpecs - truncationSpecs * 2 + 1, SpecChannels];
            audioBuffer = new List<float>(); // TODO: 溢れないためListにしているけど固定長バッファにして溢れたら捨てるようにしたほうがよさそう

            var rootPath = conf["root_path"];
            var modelFile = conf["model_file"];
            var modelFilePath = Path.Combine(rootPath, modelFile);
            var outputFolder = Path.Combine(rootPath, "output");
            onnxConverter = new OnnxConverter(modelFilePath, conf);

            speakerWaveProvider = new BufferedWaveProvider(waveFormat);
            speakerWaveProvider.DiscardOnBufferOverflow = true;
            waveOut = new WasapiOut(speaker, AudioClientShareMode.Shared, useEventSync: true, latency: SpeakerLatency);
            waveOut.Init(speakerWaveProvider);
            waveIn = new WasapiCapture(mic);
            waveIn.WaveFormat = waveFormat;

            _waveView = waveView;
            _waveLine = waveLine;

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

        public void setTargetId(int id)
        {
            SidTgt = id;
        }

        private void OnNewAudioData(object sender, WaveInEventArgs args)
        {
            int newSampleCount = args.BytesRecorded / BytesPerSample;
            audioBuffer.AddRange(BytesToFloatArray(args.Buffer, newSampleCount));
            if (TryGetNewAudio(audioBuffer, newWavBuffer))
            {
                prevWavBuffer.AsSpan().CopyTo(wavBuffer); // prevWavBufferとnewWavBufferをつなげてwavBufferを作る
                newWavBuffer.AsSpan().CopyTo(wavBuffer.AsSpan()[prevWavBuffer.Length..]);
                newWavBuffer.AsSpan()[^prevWavBuffer.Length..].CopyTo(prevWavBuffer); // newWavBufferの最後をprevWavBufferとして保持する
                MakeSpectrogram(wavBuffer, specs);
                var transWav = onnxConverter.Infer(specs, SidSrc, SidTgt);
                transWav.AsSpan()[DisposeConv1dSpecs..^DisposeConv1dSpecs].CopyTo(disposedWav); // 前後の劣化してる部分を削除
                OverlapMerge(disposedWav, prevTransWav, overlappedWav); // 頭をオーバーラップして最後を削って返す
                disposedWav.AsSpan()[^prevTransWav.Length..].CopyTo(prevTransWav); // 変換後音声の最後をOverlapMerge用にprevTransWavとして保持する
                var convertedBytes = FloatToWavArray(overlappedWav, 32768);
                speakerWaveProvider.AddSamples(convertedBytes, 0, convertedBytes.Length);
                AmplitudeFrac = wavBuffer.Max();
                TotalSamples += wavBuffer.Length;
                ProcessSample(wavBuffer);
            }
        }

        private void OverlapMerge(float[] nowWav, float[] prevWav, float[] overlappedWav)
        {
            nowWav.AsSpan()[..^OverlapSize].CopyTo(overlappedWav); // 今回変換した音の最後オーバーラップ長分は次回オーバーラップして鳴らすので捨てる
            for (int i = 0; i < OverlapSize; i++)
            {
                var t1 = (float)i / (float)OverlapSize;
                var t2 = (float)(OverlapSize - i) / (float)OverlapSize;
                var now = nowWav[i];
                var prev = prevWav[i];
                var o = t1 * now + t2 * prev;
                overlappedWav[i] = o; 
            }
        }

        public void MakeSpectrogram(float[] wav, float[,] specs, float amplify=0.0390f)
        {
            // 4096を128毎で257chのspectrogramを作る
            // NAudioのFFTを利用する　そのためComplexもNAudioのものを利用
            NAudio.Dsp.Complex[] complexWav = ArrayPool<NAudio.Dsp.Complex>.Shared.Rent(WinSize);
            for (int n = 0; n <= segmentSpecs - truncationSpecs * 2; n++) // paddingせずに算出できる条件は n <= ... になる
            {
                // Hann窓掛けてComplex化
                var start = n * HopSize;
                var end = start + WinSize;
                var wavWin = wav.AsSpan()[start..end];
                for (int i = 0; i < WinSize; i++)
                {
                    complexWav[i].X = wavWin[i] * (float)FastFourierTransform.HannWindow(i, WinSize);
                    complexWav[i].Y = 0;
                }
                // STFT
                FastFourierTransform.FFT(true, stftM, complexWav);
                // Python: spec = torch.sqrt(spec.pow(2).sum(-1) + 1e-6)
                for (int i = 0; i < SpecChannels; i++)
                {
                    specs[n, i] = (float)Math.Sqrt(complexWav[i].X * complexWav[i].X + complexWav[i].Y * complexWav[i].Y + 1e-6) * amplify;
                }
            }
        }

        private bool TryGetNewAudio(List<float> audio, float[] buffer)
        {
            if (audio.Count < SegmentSize)
                return false;
            audio.GetRange(0, SegmentSize).CopyTo(buffer);
            audio.RemoveRange(0, SegmentSize);
            return true;
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

        public void ProcessSample(float[] sample)
        {
            _waveLine.Points.Clear();
            for (int i = 0; i < sample.Length; i++)
            {
                _waveLine.Points.Add(new DataPoint((double)i, sample[i]));
            }
            _waveView.InvalidatePlot(true);
        }

        string GetAbsolutePath(string relativePath)
        {
            string rootPath = System.AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(rootPath, relativePath);

            return fullPath;
        }
    }

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
            var opts = new SessionOptions();
            opts.ExecutionMode = ExecutionMode.ORT_SEQUENTIAL;
            opts.EnableMemoryPattern = false;
            session = new InferenceSession(modelFilePath, opts);
            //session = new InferenceSession(modelFilePath, SessionOptions.MakeSessionOptionWithCudaProvider(0));
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
}
