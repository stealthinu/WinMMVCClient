using NAudio.CoreAudioApi;
using NAudio.Wave;
using NAudio.Dsp;
using Microsoft.ML;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using System.Buffers;
using Microsoft.Extensions.Configuration;
using NAudio.Utils;
using DotnetWorld.API.Structs;
using DotnetWorld.API;
using System.ComponentModel.DataAnnotations;
using System.Reflection.Emit;

namespace WinMMVCClient
{
    public class Converter : IDisposable
    {
        public int SidSrc { get; set; }
        public int SidTgt { get; set; }
        public int MicVolumeAdjustDB { get; set; }
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
        public int PitchAdjust { get; private set; }
        public int[] UpsampleRates { get; private set; }
        public float[] DenseFactors { get; private set; }

        private OnnxConverter onnxConverter;
        private SinGenerator sinGenerator;
        private Sola sola;
        private readonly WasapiCapture waveIn;
        private WasapiOut waveOut;
        //private readonly WaveIn waveIn;
        //private WaveOut waveOut;
        private BufferedWaveProvider speakerWaveProvider;
        private IConfiguration? hps;
        private Dictionary<int, Correspondence> correspondenceDict;

        private readonly List<float> audioBuffer;
        private float[] newWavBuffer;
        private float[] prevWavBuffer;
        private float[] prevTransWav;
        private float[] wavBuffer;
        private float[] disposedWav;
        private float[] overlappedWav;
        private float[,] specs;
        private int truncationSpecs;
        private int? segmentSpecs;
        private int prevStftWavSize;
        private int stftWavSize;
        private int stftSpecs;
        private int disposeConv1dSize;
        private int stftM;

        public Converter(MMDevice mic, MMDevice speaker, IConfiguration conf)
        {
            var hpsFilePath = conf["path:json"];
            var correspondenceFilePath = conf["path:correspondence"];
            var modelFilePath = conf["path:model"];
            if (hpsFilePath == null || correspondenceFilePath == null || modelFilePath == null)
            {
                throw new ArgumentException("Configuration file is not specified.");
            }

            hps = new ConfigurationBuilder().AddJsonFile(hpsFilePath).Build();

            SampleRate = hps.GetValue<int>("data:sampling_rate");
            HopSize = hps.GetValue<int>("data:hop_length");
            WinSize = hps.GetValue<int>("data:win_length");
            MaxWavValue = hps.GetValue<float>("data:max_wav_value");
            var upsampleRatesSection = hps.GetSection("model:upsample_rates");
            UpsampleRates = upsampleRatesSection.GetChildren().Select(x => int.Parse(x.Value)).ToArray();
            var denseFactorsSection = hps.GetSection("model:dense_factors");
            if (!denseFactorsSection.Exists())
            {
                // v1.5のTrainerではdense_factorsはコード決め打ちで設定ファイルになかったので特別処理　
                DenseFactors = new float[] { 0.5f, 1.0f, 4.0f, 8.0f };
            }
            else
            {
                DenseFactors = denseFactorsSection.GetChildren().Select(x => float.Parse(x.Value)).ToArray();
            }

            SidSrc = conf.GetValue<int>("vc_conf:source_id");
            SidTgt = conf.GetValue<int>("vc_conf:target_id");
            correspondenceDict = CorrespondenceDictReader.ReadDataFromFile(correspondenceFilePath, SidSrc); // 話者毎の音程補正値を取得

            MicVolumeAdjustDB = conf.GetValue<int>("vc_conf:mic_volume_adjust");
            MicVolumeAdjust = Math.Pow(10.0, MicVolumeAdjustDB / 20.0); // dB値を倍率に変換
            PitchAdjust = conf.GetValue<int>("vc_conf:pitch_adjust");
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
            WaveFormat? waveFormat = new WaveFormat(SampleRate, 1); // 24K mono
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

            onnxConverter = new OnnxConverter(modelFilePath, conf);
            sinGenerator = new SinGenerator(specsLength: stftSpecs, denseFactors: DenseFactors, upsampleScales: UpsampleRates, delayFrames: SegmentSize, sampleRate: SampleRate, hopSize: HopSize, sineAmp: 0.1f, noiseAmp: 0.003f);
            sola = new Sola(SampleRate, SegmentSize, OverlapSize);

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

        public void SetMicVolumeAdjust(int volume)
        {
            MicVolumeAdjustDB = volume;
            MicVolumeAdjust = Math.Pow(10.0, MicVolumeAdjustDB / 20.0);
        }

        public void SetPitchAdjust(int semitone)
        {
            correspondenceDict[SidTgt].AdjustSemitones = semitone;
        }

        public int GetPitchAdjust()
        {
            return correspondenceDict[SidTgt].AdjustSemitones;
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
                var f0Scale = PitchUtils.GetF0Scale(correspondenceDict[SidTgt].AdjustSemitones + PitchAdjust);
                var f0 = AdjustPitch(F0EstimationDio(wavBuffer), f0Scale);
                var (sin, d0, d1, d2, d3) = sinGenerator.MakeSinD(f0);
                MakeSpectrogram(wavBuffer, specs);
                //var transWav = onnxConverter.Infer(specs, sin, d0, d1, d2, d3, SidSrc, SidTgt);
                var transWav = wavBuffer[HopSize .. ^(HopSize * 2)]; // MakeSpectrogramで前後1+2個分減る
                for (int i = 0; i < transWav.Length; i++)
                {
                    transWav[i] = transWav[i] / MaxWavValue;
                }
                transWav.AsSpan()[disposeConv1dSize..^disposeConv1dSize].CopyTo(disposedWav); // 前後の劣化してる部分を削除
                sola.Convert(disposedWav).CopyTo(overlappedWav);
                //OverlapMerge(disposedWav, prevTransWav, overlappedWav); // 頭をオーバーラップして最後を削って返す
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
            NAudio.Dsp.Complex[]? complexWav = ArrayPool<NAudio.Dsp.Complex>.Shared.Rent(WinSize);
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
            float[]? buffer = new float[newSampleCount];
            for (int i = 0; i < newSampleCount; i++)
            {
                buffer[i] = BitConverter.ToInt16(bytesBuffer, i * BytesPerSample) * scale;
            }
            return buffer;
        }

        private byte[] FloatToWavArray(float[] floatArray, float amplify)
        {
            byte[]? bytes = new byte[floatArray.Length * BytesPerSample];
            for (int i = 0; i < floatArray.Length; i++)
            {
                Int16 val = (Int16)(floatArray[i] * amplify);
                byte[]? vals = BitConverter.GetBytes(val);
                bytes[i * 2 + 0] = vals[0];
                bytes[i * 2 + 1] = vals[1];
            }
            return bytes;
        }

        public ReadOnlySpan<float> F0EstimationDio(float[] floatWav, int sampleRate = 24000, int hopSize = 128)
        {
            // TODO: 関数内でのメモリアロケーションを無くす
            // TODO: Pitch推定をclassにして他の推定方法をすぐに試せるようにする
            double[]? wav = Array.ConvertAll(floatWav, x => (double)x);

            var wavSpecs = wav.Length / HopSize;
            var trancatedWavSpecs = wavSpecs - truncationSpecs * 2 + 1; // paddingせずにSpec算出できる数
            double framePeriod = (double)hopSize / (double)sampleRate * 1000.0; // 1要素大きくなるが先頭要素が必ず"0"になってしまうので1要素大きくてちょうど良い
            var option = new DioOption();
            Core.InitializeDioOption(option);
            option.frame_period = framePeriod;
            option.speed = 1;
            option.f0_floor = 71.0;
            option.allowed_range = 0.1;

            var f0Length = Core.GetSamplesForDIO(sampleRate, wav.Length, framePeriod);
            var f0 = new double[f0Length];
            var time_axis = new double[f0Length];
            double[]? refined_f0 = new double[f0Length];

            Core.Dio(wav, wav.Length, sampleRate, option, time_axis, f0);
            Core.StoneMask(wav, wav.Length, sampleRate, time_axis, f0, f0Length, refined_f0);

            float[]? floatF0 = Array.ConvertAll(refined_f0, x => (float)x);
            // 先頭要素が必ず"0"になる分(1) 前から(truncationSpecs - 1) 後から(truncationSpecs) 削る 例:46->42
            var sliceStart = 1 + truncationSpecs - 1; // 
            var sliceLength = floatF0.Length - sliceStart - truncationSpecs;
            ReadOnlySpan<float> trimedF0 = floatF0.AsSpan().Slice(sliceStart, sliceLength) ; 

            return trimedF0;
        }

        public static float[] AdjustPitch(ReadOnlySpan<float> f0, float f0Scale)
        {
            float[]? result = new float[f0.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = f0[i] * f0Scale;
            }

            return result;
        }
    }
}
