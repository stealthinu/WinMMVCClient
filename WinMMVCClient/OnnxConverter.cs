using Microsoft.Extensions.Configuration;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using Newtonsoft.Json.Linq;

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

        f0 = (f0 * f0_scale).unsqueeze(0).unsqueeze(0)
        if spec_lengths.numpy() != fixed_length: # 固定長に足りない場合は0パディング
            spec_padding_size = (1, spec.size(1), fixed_length - spec.size(2))
            spec_zero_padding = torch.zeros(spec_padding_size)
            spec = torch.cat([spec, spec_zero_padding], dim=2)
            f0_padding_size = (1, 1, fixed_length - f0.size(2))
            f0_zero_padding = torch.zeros(f0_padding_size)
            f0 = torch.cat([f0, f0_zero_padding], dim=2)
            spec_lengths = torch.tensor([spec.size(2)])
        sin, d = net_g.make_sin_d(f0)
        (d0, d1, d2, d3) = d
        audio = ort_session.run(
            ["audio"],
            {
                "specs": spec.numpy(),
                "lengths": spec_lengths.numpy(),
                "sin": sin.numpy(),
                "d0": d0.numpy(),
                "d1": d1.numpy(),
                "d2": d2.numpy(),
                "d3": d3.numpy(),
                "sid_src": sid_src.numpy(),
                "sid_tgt": sid_target.numpy()
            })[0][0,0]
         */

        public OnnxConverter(string modelFilePath, JObject conf)
        {
            // ONNXオプション指定
            // DirectMLの場合はパッケージを「Microsoft.ML.OnnxRuntime.DirectML」を入れる
            // CUDAの場合はパッケージを「Microsoft.ML.OnnxRuntime.Gpu」を入れる
            // Pythonの時と同様「Microsoft.ML.OnnxRuntime」を入れるとCPUでの変換になってしまうので注意
            // ※下記はDirectML用の指定
            var opts = new SessionOptions();
            opts.AppendExecutionProvider_DML(conf["device"]["gpu_id"].Value<int>()); // DirectMLでGPU_ID=0指定
            opts.ExecutionMode = ExecutionMode.ORT_SEQUENTIAL;
            opts.EnableMemoryPattern = false;
            // ※下記はCUDA用の指定
            //var opts = SessionOptions.MakeSessionOptionWithCudaProvider(0); // CUDAでGPU_ID=0指定
            // ※下記はOpenVINO(JS.OnnxRuntime.OpenVINO)用の指定
            //var opts = new SessionOptions();
            ////opts.AppendExecutionProvider_OpenVINO("CPU_FP32"); // 'CPU_FP32', 'GPU_FP32', 'GPU_FP16', 'MYRIAD_FP16', 'VAD-M_FP16', 'VAD-F_FP16'
            session = new InferenceSession(modelFilePath, opts);
        }

        public float[] Infer(float[,] specs, float[] sin, float[] d0, float[] d1, float[] d2, float[] d3, int srcId, int tgtId)
        {
            var specsLength = specs.GetLength(0); // Spectrogramの時間長
            var specsNum = specs.GetLength(1);  // Spectrogramの周波数チャンネル数 256
            var specsDims = session.InputMetadata["specs"].Dimensions; // 入力データ次元 [1, 257, length]
            var specChannels = specsDims[1]; // ONNXのSpectrogramチャンネル数 257
            var sinDims = session.InputMetadata["sin"].Dimensions; // 入力データ次元 [1, 1, length * 8 * 4 * 2 * 2]
            var d0Dims = session.InputMetadata["d0"].Dimensions; // 入力データ次元 [1, 1, length * 8]
            var d1Dims = session.InputMetadata["d1"].Dimensions; // 入力データ次元 [1, 1, length * 8 * 4]
            var d2Dims = session.InputMetadata["d2"].Dimensions; // 入力データ次元 [1, 1, length * 8 * 4 * 2]
            var d3Dims = session.InputMetadata["d3"].Dimensions; // 入力データ次元 [1, 1, length * 8 * 4 * 2 * 2]
            // ONNXに入れるためフラットな1次元の配列にする [1, 257, length] の順
            float[] flattedSpecs = new float[specsLength * specChannels]; // ArrayPool使うと入力テンソルサイズが違うと言われるので使わない AsSpanもダメ
            for (int fNum = 0; fNum < specsNum; fNum++) // FFTした結果は 256 でONNXのチャンネル数とは1違う
            {
                for (int sNum = 0; sNum < specsLength; sNum++)
                {
                    flattedSpecs[fNum * specsLength + sNum] = specs[sNum, fNum];
                }
            }
            var specsTensor = new DenseTensor<float>(flattedSpecs, specsDims);
            var sinTensor = new DenseTensor<float>(sin, sinDims);
            var d0Tensor = new DenseTensor<float>(d0, d0Dims);
            var d1Tensor = new DenseTensor<float>(d1, d1Dims);
            var d2Tensor = new DenseTensor<float>(d2, d2Dims);
            var d3Tensor = new DenseTensor<float>(d3, d3Dims);
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
                NamedOnnxValue.CreateFromTensor("sin", sinTensor),
                NamedOnnxValue.CreateFromTensor("d0", d0Tensor),
                NamedOnnxValue.CreateFromTensor("d1", d1Tensor),
                NamedOnnxValue.CreateFromTensor("d2", d2Tensor),
                NamedOnnxValue.CreateFromTensor("d3", d3Tensor),
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
