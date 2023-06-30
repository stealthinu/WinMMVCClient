namespace WinMMVCClient
{
    class Sola
    {
        private int overlapSize;
        private int solaSearchFrameSize;
        private float[] prevWav;

        public Sola(int overlapSize = 512, int solaSearchFrameSize = 256)
        {
            this.overlapSize = overlapSize; // solaSearchFrameSize * 2 = 512
            this.solaSearchFrameSize = solaSearchFrameSize;  // 24000Hz / 100Hz = 240　~= 256
            prevWav = new float[overlapSize];
        }

        public ReadOnlySpan<float> Merge(ReadOnlySpan<float> wav)
        {
            var solaSearchRegion = wav[ .. solaSearchFrameSize];
            var corNom = Convolve(prevWav, solaSearchRegion);
            var corDen = CalculateRootEnergy(prevWav, solaSearchFrameSize);
            int solaOffset = CalculateSolaOffset(corNom, corDen);

            var prevSolaMatchRegion = prevWav[solaOffset .. (solaOffset + solaSearchFrameSize)];
            var crossfadeWav = Crossfade(solaSearchRegion, prevSolaMatchRegion);
            var solaMergedWav = ConcatSpan(prevWav[ .. solaOffset], crossfadeWav, wav[solaSearchFrameSize .. ]);
            var outputWav = solaMergedWav[ .. ^(solaOffset + overlapSize)];
            wav[^(overlapSize + solaOffset) .. ^solaOffset].CopyTo(prevWav);

            return outputWav;
        }

        public static int CalculateSolaOffset(ReadOnlySpan<float> corNom, ReadOnlySpan<float> corDen)
        {
            // ArgMax(corNom / corDen)

            if (corNom.Length != corDen.Length)
            {
                throw new ArgumentException("corNom.Length != corDen.Length");
            }

            int idx = 0; // return 0 if not match
            float max = float.MinValue;

            for (int i = 0; i < corNom.Length; i++)
            {
                float scaledValue = corNom[i] / corDen[i];
                if (scaledValue > max)
                {
                    max = scaledValue;
                    idx = i;
                }
            }

            return idx;
        }

        public static ReadOnlySpan<float> CalculateRootEnergy(ReadOnlySpan<float> a, int len)
        {
            // corDen = Convolve(Pow(solaSearchRegion, 2), ones(solaSearchRegion));

            int n = a.Length;
            if (len == 0 || n < len)
            {
                throw new ArgumentException("Input arrays have incompatible sizes.");
            }

            float[] result = new float[n - len + 1];

            for (int i = 0; i < result.Length; i++)
            {
                float sum = 0;
                for (int j = 0; j < len; j++)
                {
                    sum += MathF.Pow(a[i + j], 2) * 1;
                }
                result[i] = sum; // MathF.Sqrt(sum)
            }

            return result;
        }

        public static ReadOnlySpan<float> Convolve(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
        {
            int n = a.Length;
            int m = b.Length;
            if (m == 0 || n < m)
            {
                throw new ArgumentException("Input arrays have incompatible sizes.");
            }

            float[] result = new float[n - m + 1];

            for (int i = 0; i < result.Length; i++)
            {
                float sum = 0;
                for (int j = 0; j < m; j++)
                {
                    sum += a[i + j] * b[j];
                }
                result[i] = sum;
            }

            return result;
        }

        public static ReadOnlySpan<float> ConcatSpan(ReadOnlySpan<float> array1, ReadOnlySpan<float> array2, ReadOnlySpan<float> array3)
        {
            int totalLength = array1.Length + array2.Length + array3.Length;
            float[] tempArray = new float[totalLength];

            array1.CopyTo(tempArray);
            array2.CopyTo(tempArray.AsSpan(array1.Length));
            array3.CopyTo(tempArray.AsSpan(array1.Length + array2.Length));

            return tempArray;
        }

        public static ReadOnlySpan<float> Crossfade(ReadOnlySpan<float> curWav, ReadOnlySpan<float> prevWav)
        {
            if (prevWav.Length != curWav.Length)
            {
                throw new ArgumentException("prevWav.Length != curWav.Length");
            }

            var crossfadeSize = prevWav.Length;
            float[] outputWav = curWav.ToArray();

            for (int i = 0; i < crossfadeSize; i++)
            {
                var percent = i / (float)crossfadeSize;
                var prevStrength = MathF.Pow(MathF.Cos(percent * 0.5f * MathF.PI), 2);
                var curStrength = MathF.Pow(MathF.Cos((1 - percent) * 0.5f * MathF.PI), 2);
                outputWav[i] = prevWav[i] * prevStrength + curWav[i] * curStrength;
            }

            return outputWav;
        }
    }
}
