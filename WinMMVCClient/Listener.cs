using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Dsp;
using Microsoft.ML;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Buffers;
using Microsoft.Extensions.Configuration;
using NAudio.Utils;
using TensorFlowLite;
using Microsoft.ML.Transforms;
using System;
using System.Reflection;
using TensorFlowLite.Delegates;

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
        public int Latency { get; private set; }
        public int SegmentSize { get; private set; }
        public int HopSize { get; private set; }
        public int WinSize { get; private set; }
        public float MaxWavValue { get; private set; }
        public int SpecChannels { get; private set; }
        public int OverlapSize { get; private set; }
        public int DisposeConv1dSpecs { get; private set; }
        public int BytesPerSample { get; private set; }
        public int MaxSample { get; private set; }
        public double MicVolumeAdjust { get; private set; }

        private OnnxConverter onnxConverter;
        private TFLiteConverter tfliteConverter;
        private readonly WasapiCapture waveIn;
        private WasapiOut waveOut;
        //private readonly WaveIn waveIn;
        //private WaveOut waveOut;
        private BufferedWaveProvider speakerWaveProvider;

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
        private int stftWavSize;
        private int stftSpecs;
        private int disposeConv1dSize;
        private int stftM;

        public Converter(MMDevice mic, MMDevice speaker, IConfiguration conf, IConfiguration hps)
        {
            SampleRate = hps.GetValue<int>("data:sampling_rate");
            HopSize = hps.GetValue<int>("data:hop_length");
            WinSize = hps.GetValue<int>("data:win_length");
            MaxWavValue = hps.GetValue<float>("data:max_wav_value");

            SidSrc = conf.GetValue<int>("vc_conf:source_id");
            SidTgt = conf.GetValue<int>("vc_conf:target_id");
            var micVolumeAdjustDB = conf.GetValue<double>("vc_conf:mic_volume_adjust");
            MicVolumeAdjust = Math.Pow(10.0, micVolumeAdjustDB / 20.0); // dB値を倍率に変換
            SegmentSize = conf.GetValue<int>("vc_conf:delay_flames");
            SpecChannels = WinSize / 2; // STFT結果の大きさをスペクトログラムに保存 winSizeが512だと有効なのは半分の256
            OverlapSize = conf.GetValue<int>("vc_conf:overlap");
            truncationSpecs = WinSize / HopSize / 2; // 2 FFTするときに端で計算できないサイズ
            segmentSpecs = SegmentSize / HopSize; // 32 スペクトログラムの時間方向の数
            stftM = (int)Math.Log((double)WinSize, 2); // winSizeの2のべき数(512=2^9)
            prevStftWavSize = (((WinSize / HopSize) / 2) + 1) * HopSize; // スペクトログラム作成用に過去のwavを、WinSize半分ぶんのspecsに+1した長さだけ保持
            DisposeConv1dSpecs = conf.GetValue<int>("vc_conf:dispose_conv1d_specs");
            disposeConv1dSize = DisposeConv1dSpecs * HopSize;
            BytesPerSample = 2;
            Latency = conf.GetValue<int>("vc_conf:latency");
            WaveFormat waveFormat = new WaveFormat(SampleRate, 1); // 24K mono
            BytesPerSample = waveFormat.BitsPerSample / 8;

            newWavBuffer = Enumerable.Repeat<float>(0.0f, SegmentSize).ToArray();
            prevWavBuffer = Enumerable.Repeat<float>(0.0f, prevStftWavSize + disposeConv1dSize * 2 + OverlapSize).ToArray();
            prevTransWav = Enumerable.Repeat<float>(0.0f, disposeConv1dSize * 2 + OverlapSize).ToArray();
            stftWavSize = SegmentSize + prevStftWavSize + disposeConv1dSize * 2 + OverlapSize;
            stftSpecs = (SegmentSize + disposeConv1dSize * 2 + OverlapSize) / HopSize; // 出てくるspecsはprevStftWavSize分だけ減る
            wavBuffer = new float[stftWavSize];
            specs = new float[stftSpecs, SpecChannels];
            disposedWav = new float[SegmentSize + OverlapSize];
            overlappedWav = new float[SegmentSize];
            audioBuffer = new List<float>(); // TODO: 溢れないためListにしているけど固定長バッファにして溢れたら捨てるようにしたほうがよさそう

            var modelFilePath = conf["path:model"];
            onnxConverter = new OnnxConverter(modelFilePath, conf);
            tfliteConverter = new TFLiteConverter();

            speakerWaveProvider = new BufferedWaveProvider(waveFormat);
            speakerWaveProvider.DiscardOnBufferOverflow = true;
            waveOut = new WasapiOut(speaker, AudioClientShareMode.Exclusive, true, Latency);
            waveOut.Init(speakerWaveProvider);
            waveIn = new WasapiCapture(mic, true, Latency);
            waveIn.WaveFormat = waveFormat;
            //waveOut = new WaveOut();
            //waveOut.DeviceNumber = speakerId;
            //waveIn = new WaveIn();
            //waveIn.DeviceNumber = micId;

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

        public void SetTargetId(int id)
        {
            SidTgt = id;
        }

        public void SetMicVolumeAdjust(double volume)
        {
            var micVolumeAdjustDB = volume;
            MicVolumeAdjust = Math.Pow(10.0, micVolumeAdjustDB / 20.0);
        }

        private void OnNewAudioData(object sender, WaveInEventArgs args)
        {
            int newSampleCount = args.BytesRecorded / BytesPerSample;
            audioBuffer.AddRange(ConvertAndScaleBytesToFloatArray(args.Buffer, newSampleCount, (float)MicVolumeAdjust));
            if (TryGetNewAudio(audioBuffer, newWavBuffer))
            {
                // Debug.WriteLine(DateTime.Now.ToString("ss.fff") + $" {audioBuffer.Count} {newWavBuffer.Length} ");
                prevWavBuffer.AsSpan().CopyTo(wavBuffer); // prevWavBufferとnewWavBufferをつなげてwavBufferを作る
                newWavBuffer.AsSpan().CopyTo(wavBuffer.AsSpan()[prevWavBuffer.Length..]);
                newWavBuffer.AsSpan()[^prevWavBuffer.Length..].CopyTo(prevWavBuffer); // newWavBufferの最後をprevWavBufferとして保持する
                //MakeSpectrogram(wavBuffer, specs);
                //var transWav = onnxConverter.Infer(specs, SidSrc, SidTgt);
                //transWav.AsSpan()[disposeConv1dSize..^disposeConv1dSize].CopyTo(disposedWav); // 前後の劣化してる部分を削除
                var transWav = tfliteConverter.Convert(wavBuffer, SidSrc, SidTgt);
                transWav[disposeConv1dSize..^disposeConv1dSize].CopyTo(disposedWav); // 前後の劣化してる部分を削除
                OverlapMerge(disposedWav, prevTransWav, overlappedWav); // 頭をオーバーラップして最後を削って返す
                disposedWav.AsSpan()[^prevTransWav.Length..].CopyTo(prevTransWav); // 変換後音声の最後をOverlapMerge用にprevTransWavとして保持する
                var convertedBytes = FloatToWavArray(overlappedWav, MaxWavValue);
                speakerWaveProvider.AddSamples(convertedBytes, 0, convertedBytes.Length);
                AmplitudeFrac = wavBuffer.Max();
                TotalSamples += wavBuffer.Length;
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

        public void MakeSpectrogram(float[] wav, float[,] specs, float amplify = 0.0390f)
        {
            // 4096を128毎で257chのspectrogramを作る
            // NAudioのFFTを利用する　そのためComplexもNAudioのものを利用
            NAudio.Dsp.Complex[] complexWav = ArrayPool<NAudio.Dsp.Complex>.Shared.Rent(WinSize);
            var wavSpecs = wav.Length / HopSize;
            var trancatedWavSpecs = wavSpecs - truncationSpecs * 2 + 1; // paddingせずにSpec算出できる数
            for (int n = 0; n < trancatedWavSpecs; n++)
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

        private float[] ConvertAndScaleBytesToFloatArray(byte[] bytesBuffer, int newSampleCount, float scale)
        {
            float[] buffer = new float[newSampleCount];
            for (int i = 0; i < newSampleCount; i++)
            {
                buffer[i] = BitConverter.ToInt16(bytesBuffer, i * BytesPerSample) * scale;
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
            // ※下記はDirectML用の指定
            var opts = new SessionOptions();
            opts.AppendExecutionProvider_DML(conf.GetValue<int>("device:gpu_id")); // DirectMLでGPU_ID=0指定
            opts.ExecutionMode = ExecutionMode.ORT_SEQUENTIAL;
            opts.EnableMemoryPattern = false;
            // ※下記はCUDA用の指定
            //var opts = SessionOptions.MakeSessionOptionWithCudaProvider(0); // CUDAでGPU_ID=0指定
            session = new InferenceSession(modelFilePath, opts);
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

    public class TFLiteConverter
    {
        readonly Model model;
        Interpreter interpreter;
        readonly SpectrogramGenerator spectrogramGenerator;

        public int SampleRate { get; }

        public TFLiteConverter(String modelName = "G_140000_fix42_float32.tflite", int sampleRate = 24000)
        {
            model = ModelLoader.Load(modelName);
            var xnnPackDelegate = TFLiteDelegate.Create();
            using var options = InterpreterOptions.Create();
            options.AddDelegate(xnnPackDelegate);
            interpreter = new Interpreter(model, options);
            interpreter.AllocateTensors().ThrowExceptionForStatus();
            spectrogramGenerator = new SpectrogramGenerator();

            SampleRate = sampleRate;
        }

        public ReadOnlySpan<float> Convert(ReadOnlySpan<float> audio, int src, int tgt)
        {
            var specs = spectrogramGenerator.Generate(audio); // tensorは[1, 42, 257]をフラットに並べたもの
            var specsLengths = new long[] { specs.Length };
            var sidSrc = new long[] { src };
            var sidTgt = new long[] { tgt };

            // 入力テンソルのセット
            interpreter.GetInputTensor(0).CopyFromBuffer(specs).ThrowExceptionForStatus();
            interpreter.GetInputTensor(1).CopyFromBuffer(specsLengths.AsSpan()).ThrowExceptionForStatus();
            interpreter.GetInputTensor(2).CopyFromBuffer(sidSrc.AsSpan()).ThrowExceptionForStatus();
            interpreter.GetInputTensor(3).CopyFromBuffer(sidTgt.AsSpan()).ThrowExceptionForStatus();
            interpreter.Invoke().ThrowExceptionForStatus();
            var data = interpreter.GetOutputTensor(0).GetData<float>();

            return data;
        }
    }

    static class TFLiteDelegate
    {
        public static TensorFlowLiteDelegate Create()
        {
            var xnnpackDelegateOptions = XNNPackDelegate.Options.Default;
            xnnpackDelegateOptions.Flags |= XNNPackDelegate.Flags.Qu8;
            xnnpackDelegateOptions.ThreadCount = 4;
            var xnnpackDelegate = XNNPackDelegate.Create(xnnpackDelegateOptions);
            xnnpackDelegate.Flags |= TensorFlowLite.Native.TfLiteDelegateFlags.AllowDynamicTensors;
            return xnnpackDelegate;
        }
    }

    class SpectrogramGenerator
    {
        public int HopSize { get; private set; }
        public int WinSize { get; private set; }
        public float MaxWavValue { get; private set; }
        public int SpecChannels { get; private set; }
        public int SpecLength { get; private set; }
        private int truncationSpecs;
        private int stftM;
        private float[] specs; // TODO: 最終的にTFLiteに渡すためフラットなarrayで生成するから名前変えるべき？

        public SpectrogramGenerator()
        {
            HopSize = 128;
            WinSize = 512;
            MaxWavValue = 32768.0f;
            SpecChannels = WinSize / 2 + 1; // STFT結果の大きさをスペクトログラムに保存 winSizeが512だと有効なのは半分の256。なのだがなぜかvitsは+1している
            SpecLength = 42;
            truncationSpecs = WinSize / HopSize / 2; // 2 FFTするときに端で計算できないサイズ
            stftM = (int)Math.Log((double)WinSize, 2); // winSizeの2のべき数(512=2^9)
            specs = new float[SpecLength * SpecChannels];
        }

        public ReadOnlySpan<float> Generate(ReadOnlySpan<float> wav, float amplify = 0.0390f)
        {
            // 4096を128毎で257chのspectrogramを作る
            // NAudioのFFTを利用する　そのためComplexもNAudioのものを利用
            NAudio.Dsp.Complex[] complexWav = ArrayPool<NAudio.Dsp.Complex>.Shared.Rent(WinSize);
            var wavSpecs = wav.Length / HopSize;
            var trancatedWavSpecs = wavSpecs - truncationSpecs * 2 + 1; // paddingせずにSpec算出できる数
            for (int n = 0; n < trancatedWavSpecs; n++)
            {
                // Hann窓掛けてComplex化
                var start = n * HopSize;
                var end = start + WinSize;
                var wavWin = wav[start..end];
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
                    // TFLiteに入れるためフラットな1次元の配列にする TFLiteは[1, length, 257]順になってることに注意！
                    specs[n * SpecChannels + i] = MathF.Sqrt((float)complexWav[i].X * (float)complexWav[i].X + (float)complexWav[i].Y * (float)complexWav[i].Y + 1e-6f) * amplify;
                }
            }
            return specs;
        }
    }

    static class ModelLoader
    {
        static byte[] LoadBinary(string name)
        {
            using var resourceStream = typeof(ModelLoader).Assembly.GetManifestResourceStream(typeof(ModelLoader), "Models." + name);
            using var memoryStream = new MemoryStream();
            resourceStream.CopyTo(memoryStream);
            return memoryStream.ToArray();
        }
        public static Model Load(string name)
        {
            return new Model(LoadBinary(name));
        }
    }
}
