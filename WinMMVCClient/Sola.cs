namespace WinMMVCClient
{
    class Sola
    {
        private int solaSearchFrame;
        private int crossfadeFrame;
        private int blockFrame;
        private float curStrength;
        private float prevStrength;
        private float[]? solaBuffer;

        public Sola(int solaSearchFrame, int crossfadeFrame, int blockFrame, float curStrength, float prevStrength)
        {
            this.solaSearchFrame = solaSearchFrame;
            this.crossfadeFrame = crossfadeFrame;
            this.blockFrame = blockFrame;
            this.curStrength = curStrength;
            this.prevStrength = prevStrength;
            solaBuffer = null;
        }

        public ReadOnlySpan<float> Convert(ReadOnlySpan<float> audio)
        {
            int audioOffset = -1 * (solaSearchFrame + crossfadeFrame + blockFrame);
            audio = audio[audioOffset .. ];

            float[] flippedSolaBuffer = solaBuffer.Reverse().ToArray();
            var audioSubset = audio[ .. ^(crossfadeFrame + solaSearchFrame)];
            var corNom = Convolve(audioSubset, flippedSolaBuffer);
            var corDen = CalculateRootEnergy(audioSubset, crossfadeFrame);
            int solaOffset = ArgMax(corNom);

            int solaEnd = solaOffset + blockFrame;
            var outputWav = audio[solaOffset .. solaEnd];
            CrossfadeOverlap(outputWav[ .. crossfadeFrame], solaBuffer, curStrength);

            if (solaOffset < solaSearchFrame)
            {
                int offset = -1 * (solaSearchFrame + crossfadeFrame - solaOffset);
                int end = -1 * (solaSearchFrame - solaOffset);
                MulWithScalar(audio[offset .. ^end], prevStrength).CopyTo(solaBuffer);
            }
            else
            {
                MulWithScalar(audio[^crossfadeFrame .. ], prevStrength).CopyTo(solaBuffer);
            }

            return outputWav;
        }

        public static ReadOnlySpan<float> CrossfadeOverlap(ReadOnlySpan<float> inputWav, ReadOnlySpan<float> crossWav, float strength)
        {
            float[] outputWav = new float[inputWav.Length];
            for (int i = 0; i < inputWav.Length; i++)
            {
                outputWav[i] = inputWav[i] * strength + crossWav[i];
            }
            return outputWav;
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

        private static float CalculateRootEnergy(ReadOnlySpan<float> a, int m)
        {
            int n = a.Length;

            float sum = 0;

            for (int i = 0; i < n - m + 1; i++)
            {
                for (int j = 0; j < m; j++)
                {
                    var squared = MathF.Pow(a[i + j], 2);
                    sum += squared;
                }
            }

            return MathF.Sqrt(sum + 1e-3f);
        }
    }
}
