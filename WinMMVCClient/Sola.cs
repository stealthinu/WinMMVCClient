namespace WinMMVCClient
{
    class Sola
    {
        private int blockFrameSize; // input audio blockFrameSize
        private int crossfadeSize;  // crossfadeSize = SOLA search region size
        private int solaSearchFrameSize;
        private float[] solaPrevBuffer;

        public Sola(int SamplingRate, int blockFrameSize, int crossfadeSize)
        {
            this.blockFrameSize = blockFrameSize;
            this.crossfadeSize = crossfadeSize;
            this.solaSearchFrameSize = (int)(0.01 * SamplingRate); // Convolution size = 100Hz
            solaPrevBuffer = new float[crossfadeSize];
        }

        public ReadOnlySpan<float> Convert(ReadOnlySpan<float> audio)
        {
            //int audioOffset =  crossfadeFrameSize + solaSearchFrameSize + blockFrameSize;
            //var audio = audioIn[^audioOffset .. ];

            var solaSearchRegion = audio[ .. (crossfadeSize + solaSearchFrameSize)];
            var corNom = Convolve(solaSearchRegion, solaPrevBuffer);
            var corDen = CalculateRootEnergy(solaSearchRegion, crossfadeSize);
            int solaOffset = CalculateSolaOffset(corNom, corDen);

            var solaExtractedWav = audio[solaOffset .. (solaOffset + blockFrameSize)];
            //var outputWav = CrossfadeOverlap(solaExtractedWav, solaPrevBuffer);
            audio[^crossfadeSize .. ].CopyTo(solaPrevBuffer);

            return solaExtractedWav;
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
