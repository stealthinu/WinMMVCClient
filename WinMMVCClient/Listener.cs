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

namespace WindowsMMVCClient
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

        public float[] Infer(float[] wav)
        {
            // specsだから4096を128毎で257chのspec作らないといけなかった
            // 複素数データに変換
            int segmentSize = 4096;
            int hopSize = 128;
            int winSize = 512;
            int paddingSize = winSize / hopSize / 2; // 2
            int specNum = segmentSize / hopSize - paddingSize * 2 + 1; // 29
            var window = Window.Hamming(winSize);

            float[] tempArray = ArrayPool<float>.Shared.Rent(winSize);
            var temp = tempArray.AsSpan()[..winSize];
            float[] windowedWav = ArrayPool<float>.Shared.Rent(winSize);  
            for (int specN = paddingSize; specN < segmentSize / hopSize - paddingSize; specN++)
            {
                for (int i = specN * hopSize; i < specN * hopSize + 1; i++)
                {
                    wav.AsSpan(i, winSize).CopyTo(temp);
                    /*
                    var windowedWav = wav.Select((v, i) => v * (float)window[i]).ToList();
                    Complex[] complexWav = windowedWav.Select(v => new Complex(v, 0.0)).ToArray();
                    Fourier.Forward(complexWav, FourierOptions.Matlab);
                    float[] specs = new float[specNum];
                    */
                }
            }
            ArrayPool<float>.Shared.Return(windowedWav);
            //ArrayPool<float>.Shared.Return(temp);

            //var windowedWav = wav.Select((v, i) => v * (float)window[i]).ToList();
            Complex[] complexWav = windowedWav.Select(v => new Complex(v, 0.0)).ToArray();
            Fourier.Forward(complexWav, FourierOptions.Matlab);
            float[] specs = new float[specNum];
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

            //Debug.Assert(specs.Length != 257 * 4096);
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
        private readonly List<double> audio = new List<double>();
        public int SamplesInMemory { get { return audio.Count; } }
        public int SampleRate { get; private set; }
        public int SpeakerLatency { get; private set; }
        public int SegmentSize { get; private set; }
        public int bytesPerSample = 2;
        public int maxSample = 32768;

        public Converter(MMDevice mic, MMDevice speaker, WaveFormat waveFormat, int segmentSize=4096, int speakerLatency=100)
        {
            onnxConverter = new OnnxConverter();

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
                var convertedBytes = FloatToBytesArray(audio);
                speakerWaveProvider.AddSamples(convertedBytes, 0, convertedBytes.Length);
            }
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
