using Microsoft.Extensions.Configuration;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.Text;

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
}
