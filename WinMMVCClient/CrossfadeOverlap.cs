namespace WinMMVCClient
{
    class CrossfadeOverlap
    {
        private int overlapSize;
        private float[] prevWav;

        public CrossfadeOverlap(int overlapSize)
        {
            this.overlapSize = overlapSize; // solaSearchFrameSize * 2 = 512
            prevWav = new float[overlapSize];
        }

        public ReadOnlySpan<float> Merge(ReadOnlySpan<float> wav)
        {
            if (prevWav.Length > wav.Length)
            {
                throw new ArgumentException("prevWav.Length > curWav.Length");
            }

            var crossfadeSize = prevWav.Length;
            float[] overlappedWav = wav.ToArray();

            for (int i = 0; i < crossfadeSize; i++)
            {
                var percent = i / (float)crossfadeSize;
                var curStrength = MathF.Pow(MathF.Sin(percent * 0.5f * MathF.PI), 2); // sin(i * PI/2) ** 2
                var prevStrength = 1 - curStrength;
                overlappedWav[i] = prevWav[i] * prevStrength + wav[i] * curStrength;
            }

            var outputWav = overlappedWav.AsSpan()[ .. ^overlapSize]; // SegmentSizeに切り出し
            wav[^overlapSize .. ].CopyTo(prevWav); // 音声の最後をMerge用にprevWavとして保持

            return outputWav;
        }

    }
}
