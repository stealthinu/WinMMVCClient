using DotnetWorld.API.Structs;
using DotnetWorld.API;
using System;

namespace WinMMVCClient
{
    public static class PitchUtils
    {
        // A4 (440Hz) as a reference pitch.
        private static readonly float ReferenceFrequency = 440.0f;

        // The twelfth root of two.
        private static readonly float HalfStepRatio = MathF.Pow(2.0f, 1.0f / 12.0f);

        public static int GetSemitoneDifference(float sourceFrequency, float targetFrequency)
        {
            var targetStepsFromRef = MathF.Log(targetFrequency / ReferenceFrequency, HalfStepRatio);
            var sourceStepsFromRef = MathF.Log(sourceFrequency / ReferenceFrequency, HalfStepRatio);
            int diff = (int)Math.Round(targetStepsFromRef - sourceStepsFromRef);
            return diff;
        }

        public static float GetF0Scale(int semitones)
        {
            // Raise the half-step ratio to the power of the semitone difference.
            return MathF.Pow(HalfStepRatio, semitones);
        }

        public static ReadOnlySpan<float> F0Estimation(float[] floatWav, int sampleRate = 24000, int hopSize = 128, int truncationSpecs = 2)
        {
            return F0EstimationDio(floatWav, sampleRate, hopSize, truncationSpecs);
        }

        public static ReadOnlySpan<float> F0EstimationDio(float[] floatWav, int sampleRate = 24000, int hopSize = 128, int truncationSpecs = 2)
        {
            // TODO: 関数内でのメモリアロケーションを無くす
            // TODO: Pitch推定をclassにして他の推定方法をすぐに試せるようにする
            double[]? wav = Array.ConvertAll(floatWav, x => (double)x);

            var wavSpecs = wav.Length / hopSize;
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
            ReadOnlySpan<float> trimedF0 = floatF0.AsSpan().Slice(sliceStart, sliceLength);

            return trimedF0;
        }
    }
}
