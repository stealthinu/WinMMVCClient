namespace WinMMVCClient
{
    class Sola
    {
        private int blockFrameSize; // input audio block frame size
        private int crossfadeSize;  // crossfade size
        private int solaSearchSize; // length which to search for a match in the SOLA
        private float[] solaBuffer;

        public Sola(int SamplingRate, int blockFrameSize, int crossfadeOverlapSize)
        {
            this.blockFrameSize = blockFrameSize;
            this.crossfadeSize = crossfadeOverlapSize;
            this.solaSearchSize = (int)(0.012 * SamplingRate);
            solaBuffer = new float[solaSearchSize];
        }

        public ReadOnlySpan<float> Convert(ReadOnlySpan<float> audio)
        {
            //int audioOffset =  crossfadeFrameSize + solaSearchFrameSize + blockFrameSize;
            //var audio = audioIn[^audioOffset .. ];

            var solaSearchRegion = audio[ .. solaSearchSize];
            var corNom = CalculateNominalCorrelation(solaSearchRegion, solaBuffer);
            var corDen = CalculateRootEnergy(solaSearchRegion, solaSearchSize);
            int solaOffset = CalculateSolaOffset(corNom, corDen);

            int frameEnd = solaOffset + blockFrameSize;
            var solaExtractedWav = audio[solaOffset .. frameEnd];
            var outputWav = CrossfadeOverlap(solaExtractedWav, solaBuffer);
            audio[^solaSearchSize .. ].CopyTo(solaBuffer);

            return outputWav;
        }

        public static ReadOnlySpan<float> CrossfadeOverlap(ReadOnlySpan<float> curWav, ReadOnlySpan<float> prevWav)
        {
            if (prevWav.Length > curWav.Length)
            {
                throw new ArgumentException("prevWav.Length > curWav.Length");
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

        public static int CalculateSolaOffset(ReadOnlySpan<float> corNom, ReadOnlySpan<float> corDen)
        {
            // ArgMax(corNom / corDen)

            if (corNom.Length != corDen.Length)
            {
                throw new ArgumentException("corNom.Length != corDen.Length");
            }

            int idx = -1;
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

        public static ReadOnlySpan<float> CalculateNominalCorrelation(ReadOnlySpan<float> a, ReadOnlySpan<float> b)
        {
            // flippedSolaBuffer = solaBuffer.Reverse().ToArray();
            // corNom = Convolve(crossfadeRegion, flippedSolaBuffer);

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
                    sum += a[i + j] * b[m - j - 1]; // Reversed b
                }
                result[i] = sum;
            }

            return result;
        }

        public static ReadOnlySpan<float> CalculateRootEnergy(ReadOnlySpan<float> a, int len)
        {
            // corDen = Convolve(Pow(crossfadeRegion, 2), ones(crossfadeRegion));

            int n = a.Length;
            int m = len;
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
                    sum += MathF.Pow(a[i + j], 2) * 1;
                }
                result[i] = sum;
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

        public static ReadOnlySpan<float> MulWithScalar(ReadOnlySpan<float> array, float scalar)
        {
            float[] result = new float[array.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = array[i] * scalar;
            }

            return result;
        }

        public static int ArgMax(ReadOnlySpan<float> data)
        {
            float max = float.MinValue;
            int idx = -1;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] > max)
                {
                    max = data[i];
                    idx = i;
                }
            }
            return idx;
        }

    }
}
