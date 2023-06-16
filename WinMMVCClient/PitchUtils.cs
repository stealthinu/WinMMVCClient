using System;

namespace WinMMVCClient
{
    public static class PitchUtils
    {
        // A4 (440Hz) as a reference pitch.
        private static readonly float ReferenceFrequency = 440.0f;

        // The twelfth root of two.
        private static readonly float HalfStepRatio = MathF.Pow(2.0f, 1.0f / 12.0f);

        public static int GetSemitonesDifference(float sourceFrequency, float targetFrequency)
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
    }
}
