namespace WinMMVCClient
{
    public class SinGenerator
    {
        int specsLength;
        float[] denseFactors;
        int[] upsampleScales;
        int delayFrames;
        int sampleRate;
        int hopSize;
        float sineAmp;
        float noiseAmp;
        float[] sin;
        float[][] d;
        SignalGenerator signalGenerator;

        public SinGenerator(int specsLength, float[] denseFactors, int[] upsampleScales, int delayFrames, int sampleRate, int hopSize, float sineAmp, float noiseAmp)
        {
            this.sampleRate = sampleRate;
            this.hopSize = hopSize;
            this.sineAmp = sineAmp;
            this.noiseAmp = noiseAmp;
            this.delayFrames = delayFrames;
            this.specsLength = specsLength;
            this.denseFactors = denseFactors;
            this.upsampleScales = upsampleScales;
            this.signalGenerator = new SignalGenerator(delayFrames);

            d = new float[this.upsampleScales.Length][];
            int upsampleScale = 1;
            for (int i = 0; i < this.upsampleScales.Length; i++)
            {
                upsampleScale *= this.upsampleScales[i];
                d[i] = new float[specsLength * upsampleScale];
            }
            sin = new float[specsLength * upsampleScale];
        }

        public (float[] sin, float[] d0, float[] d1, float[] d2, float[] d3) MakeSinD(ReadOnlySpan<float> f0)
        {
            /*
            def make_sin_d(self, f0):
                # f0 から sin と d を作成
                # f0 : [b, 1, t]
                # sin : [b, 1, t]
                # d : [4][b, 1, t]
                prod_upsample_scales = np.cumprod(self.upsample_scales)
                dfs_batch = []
                for df, us in zip(self.dense_factors, prod_upsample_scales):
                  dilated_tensor = dilated_factor(f0, self.sample_rate, df)
                  #result += [torch.repeat_interleave(dilated_tensor, us, dim=1)]
                  result = [torch.stack([dilated_tensor for _ in range(us)], -1).reshape(dilated_tensor.shape[0], -1)]
                  dfs_batch.append(torch.cat(result, dim = 0).unsqueeze(1))
                in_batch = self.signal_generator(f0)

                return in_batch, dfs_batch
            */
            int upsampleScale = 1;
            for (int i = 0; i < upsampleScales.Length; i++)
            {
                upsampleScale *= upsampleScales[i]; // x8, x32, x64, x128
                StretchedDilatedFactor(f0, sampleRate, denseFactors[i], upsampleScale).CopyTo(d[i]);
            }
            signalGenerator.GenerateSignal(f0).CopyTo(sin);
            return (sin, d[0], d[1], d[2], d[3]);
        }

        public static ReadOnlySpan<float> StretchedDilatedFactor(ReadOnlySpan<float> f0, int sampleRate, float denseFactor, int stretchFactor)
        {
            float[] result = new float[f0.Length * stretchFactor];
            for (int i = 0; i < f0.Length; i++)
            {
                var dilatedFactor = (f0[i] == 0) ? 1 : sampleRate / denseFactor / f0[i];
                for (int j = 0; j < stretchFactor; j++)
                {
                    result[i * stretchFactor + j] = dilatedFactor;
                }
            }
            return result;
        }

        public static ReadOnlySpan<float> DilatedFactor(ReadOnlySpan<float> f0, int sampleRate, float denseFactor)
        {
            /*
            def dilated_factor(batch_f0, fs, dense_factor):
                """Pitch-dependent dilated factor

                Args:
                    batch_f0 (ndarray): the f0 sequence (T)
                    fs (int): sampling rate
                    dense_factor (int): the number of taps in one cycle

                Return:
                    dilated_factors(np array):
                        float array of the pitch-dependent dilated factors (T)

                """
                batch_f0[batch_f0 == 0] = fs / dense_factor
                dilated_factors = torch.ones_like(batch_f0) * fs / dense_factor / batch_f0
                #assert np.all(dilated_factors > 0)
                return dilated_factors
             */
            var dilatedFactors = new float[f0.Length];
            for (int i = 0; i < f0.Length; i++)
            {
                if (f0[i] != 0)
                {
                    dilatedFactors[i] = sampleRate / denseFactor / f0[i];
                }
                else
                {
                    dilatedFactors[i] = 1;
                }
            }

            return dilatedFactors;
        }

        public static ReadOnlySpan<float> StretchArray(ReadOnlySpan<float> array, int repeatTimes)
        {
            float[] result = new float[array.Length * repeatTimes];
            for (int i = 0; i < array.Length; i++)
            {
                for (int j = 0; j < repeatTimes; j++)
                {
                    result[i * repeatTimes + j] = array[i];
                }
            }
            return result;
        }
    }

    public class SignalGenerator
    {
        /*
            Initialize WaveNetResidualBlock module.

            Args:
                sample_rate(int) : Sampling rate.
                hop_size(int): Hop size of input F0.
                sine_amp(float): Sine amplitude for NSF-based sine generation.
                noise_amp(float): Noise amplitude for NSF-based sine generation.
        */

        int lastIndex = -1; // Index to get the lastPhase
        int sampleRate = 24000;
        int hopSize = 128;
        float sineAmp = 0.1f;
        float noiseAmp = 0.003f;
        float lastPhase = 0.0f;

        public SignalGenerator(int _lastIndex = -1, int _sampleRate = 24000, int _hopSize = 128, float _sineAmp = 0.1f, float _noiseAmp = 0.003f)
        {
            lastIndex = _lastIndex;
            sampleRate = _sampleRate;
            hopSize = _hopSize;
            sineAmp = _sineAmp;
            noiseAmp = _noiseAmp;
        }

        public ReadOnlySpan<float> GenerateSignal(ReadOnlySpan<float> f0)
        {
            // return self.sinusoid(f0) * f0_scale
            var signal = Sinusoid(f0);
            return signal;
        }

        public ReadOnlySpan<float> Sinusoid(ReadOnlySpan<float> f0, float? phase = null)
        {
            int outputLength = f0.Length * hopSize;
            float[] sine = new float[outputLength];
            if (phase != null)
            {
                lastPhase = (float)phase;
            }
            var startPhase = lastPhase;
            var _lastIndex = lastIndex >= 0 ? lastIndex : outputLength + lastIndex;

            float radious = 0;
            for (int i = 0; i < outputLength; i++)
            {
                var (vuv, rad) = GetInterpolatedVuvAndRadious(f0, i);
                radious += rad;
                sine[i] = vuv * MathF.Sin((radious + startPhase) * 2.0f * MathF.PI) * sineAmp;
                if (noiseAmp > 0)
                {
                    var noise = Randn() * (vuv * noiseAmp + (1.0f - vuv) * noiseAmp / 3.0f);
                    sine[i] += noise;
                }

                // Save the last phase for the next generation
                if (i == _lastIndex)
                {
                    lastPhase = (radious + startPhase) % 1;
                }
            }

            return sine;
        }

        private (float vuv, float rad) GetInterpolatedVuvAndRadious(ReadOnlySpan<float> f0, int i)
        {
            float originalPosition = (float)i / hopSize;
            int index = (int)Math.Floor(originalPosition);
            int nextIndex = index < f0.Length - 1 ? index + 1 : index;
            float t = originalPosition - index;

            var vuv1 = f0[index] > 0 ? 1 : 0;
            var rad1 = f0[index] / sampleRate;
            var vuv2 = f0[nextIndex] > 0 ? 1 : 0;
            var rad2 = f0[nextIndex] / sampleRate;

            var vuv = vuv1 * (1 - t) + vuv2 * t;
            var rad = (rad1 * (1 - t) + rad2 * t) % 1;

            return (vuv, rad);
        }

        public static float Randn()
        {
            // standard normal distribution random number
            Random rand = new Random();
            float result = (float)(Math.Sqrt(-2.0 * Math.Log(rand.NextDouble())) * Math.Sin(2.0 * Math.PI * rand.NextDouble()));
            return result;
        }

        public ReadOnlySpan<float> SinusoidArray(ReadOnlySpan<float> f0)
        {
            /*
            """Calculate sine signals.

            Args:
                f0(Tensor) : F0 tensor(B, 1, T // hop_size).

            Returns:
                Tensor: Sines generated following NSF (B, 1, T).

            """
            B, _, T = f0.size()
            vuv = interpolate((f0 > 0) * torch.ones_like(f0), T * self.hop_size)
            radious = (interpolate(f0, T * self.hop_size) / self.sample_rate) % 1
            sine = vuv * torch.sin(torch.cumsum(radious, dim=2) * 2 * np.pi) * self.sine_amp
            if self.noise_amp > 0:
                noise_amp = vuv * self.noise_amp + (1.0 - vuv) * self.noise_amp / 3.0
                noise = torch.randn((B, 1, T * self.hop_size), device=f0.device) * noise_amp
                sine = sine + noise

            return sine
             */
            var vuv = InterpolateLinear(GreaterThanZero(f0), hopSize);
            var radious = DivByScalarAndGetFraction(InterpolateLinear(f0, hopSize), sampleRate);
            var sine = MulWithScalar(MulArrays(ApplySin(MulWithScalar(CumSum(radious), 2.0f * MathF.PI)), vuv), sineAmp);

            if (noiseAmp > 0)
            {
                var noiseAmpAdjusted = AddArrays(MulWithScalar(vuv, noiseAmp), MulWithScalar(SubFromScalar(1.0f, vuv), noiseAmp / 3.0f));
                var noise = MulArrays(RandnArray(f0.Length * hopSize), noiseAmpAdjusted);
                sine = AddArrays(sine, noise);
            }

            return sine;
        }

        public static float[] InterpolateLinear(ReadOnlySpan<float> input, int scale)
        {
            // floatの配列をscale倍して間を線形補間する
            int inputLength = input.Length;
            int outputLength = inputLength * scale;

            float[] output = new float[outputLength];

            for (int i = 0; i < outputLength; i++)
            {
                float originalPosition = (float)i / scale;
                int index = (int)Math.Floor(originalPosition);
                float t = originalPosition - index;
                if (index < inputLength - 1)
                {
                    output[i] = input[index] * (1 - t) + input[index + 1] * t;
                }
                else
                {
                    output[i] = input[index];
                }
            }

            return output;
        }

        public static float[] ApplySin(ReadOnlySpan<float> array)
        {
            float[] result = new float[array.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = MathF.Sin(array[i]);
            }

            return result;
        }

        public static float[] RandnArray(int length)
        {
            float[] result = new float[length];
            Random rand = new Random();
            for (int i = 0; i < length; i++)
            {
                result[i] = (float)(Math.Sqrt(-2.0 * Math.Log(rand.NextDouble())) * Math.Sin(2.0 * Math.PI * rand.NextDouble()));
            }
            return result;
        }

        public static float[] AddArrays(ReadOnlySpan<float> array1, ReadOnlySpan<float> array2)
        {
            if (array1.Length != array2.Length)
            {
                throw new ArgumentException("The arrays must have the same length.");
            }

            float[] result = new float[array1.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = array1[i] + array2[i];
            }

            return result;
        }

        public static float[] MulArrays(ReadOnlySpan<float> array1, ReadOnlySpan<float> array2)
        {
            if (array1.Length != array2.Length)
            {
                throw new ArgumentException("The arrays must have the same length.");
            }

            float[] result = new float[array1.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = array1[i] * array2[i];
            }

            return result;
        }

        public static float[] MulWithScalar(ReadOnlySpan<float> array, float scalar)
        {
            float[] result = new float[array.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = array[i] * scalar;
            }

            return result;
        }

        public static float[] CumSum(ReadOnlySpan<float> array)
        {
            float[] result = new float[array.Length];
            float sum = 0;

            for (int i = 0; i < result.Length; i++)
            {
                sum += array[i];
                result[i] = sum;
            }

            return result;
        }

        public static float[] DivByScalarAndGetFraction(ReadOnlySpan<float> array, int scalar)
        {
            float[] result = new float[array.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = array[i] / scalar - (int)(array[i] / scalar);
            }

            return result;
        }

        public static float[] SubFromScalar(float scalar, ReadOnlySpan<float> array)
        {
            float[] result = new float[array.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = scalar - array[i];
            }

            return result;
        }

        public static float[] OnesLike(ReadOnlySpan<float> array)
        {
            float[] result = new float[array.Length];

            for (int i = 0; i < result.Length; i++)
            {
                result[i] = 1.0f;
            }

            return result;
        }

        public static float[] GreaterThanZero(ReadOnlySpan<float> array)
        {
            float[] result = new float[array.Length];

            for (int i = 0; i < array.Length; i++)
            {
                result[i] = array[i] > 0 ? 1.0f : 0;
            }

            return result;
        }
    }

    /*
    def dilated_factor(batch_f0, fs, dense_factor):
        """Pitch-dependent dilated factor

        Args:
            batch_f0 (ndarray): the f0 sequence (T)
            fs (int): sampling rate
            dense_factor (int): the number of taps in one cycle

        Return:
            dilated_factors(np array):
                float array of the pitch-dependent dilated factors (T)

        """
        batch_f0[batch_f0 == 0] = fs / dense_factor
        dilated_factors = torch.ones_like(batch_f0) * fs / dense_factor / batch_f0
        #assert np.all(dilated_factors > 0)
        return dilated_factors


    class SignalGenerator:
        """Input signal generator module."""

        def __init__(
            self,
            sample_rate=24000,
            hop_size=120,
            sine_amp=0.1,
            noise_amp=0.003,
            signal_types=["sine", "noise"],
        ):
            """Initialize WaveNetResidualBlock module.

            Args:
                sample_rate (int): Sampling rate.
                hop_size (int): Hop size of input F0.
                sine_amp (float): Sine amplitude for NSF-based sine generation.
                noise_amp (float): Noise amplitude for NSF-based sine generation.
                signal_types (list): List of input signal types for generator.

            """
            self.sample_rate = sample_rate
            self.hop_size = hop_size
            self.signal_types = signal_types
            self.sine_amp = sine_amp
            self.noise_amp = noise_amp

            for signal_type in signal_types:
                if not signal_type in ["noise", "sine", "sines", "uv"]:
                    logger.info(f"{signal_type} is not supported type for generator input.")
                    sys.exit(0)
            #logger.info(f"Use {signal_types} for generator input signals.")

        @torch.no_grad()
        def __call__(self, f0, f0_scale = 1.0):
            signals = []
            for typ in self.signal_types:
                if "noise" == typ:
                    signals.append(self.random_noise(f0))
                if "sine" == typ:
                    signals.append(self.sinusoid(f0))
                if "sines" == typ:
                    signals.append(self.sinusoids(f0))
                if "uv" == typ:
                    signals.append(self.vuv_binary(f0))

            input_batch = signals[0]
            for signal in signals[1:]:
                input_batch = torch.cat([input_batch, signal], axis=1)

            return input_batch * f0_scale

        @torch.no_grad()
        def random_noise(self, f0):
            """Calculate noise signals.

            Args:
                f0 (Tensor): F0 tensor (B, 1, T // hop_size).

            Returns:
                Tensor: Gaussian noise signals (B, 1, T).

            """
            B, _, T = f0.size()
            noise = torch.randn((B, 1, T * self.hop_size), device=f0.device)

            return noise

        @torch.no_grad()
        def sinusoid(self, f0):
            """Calculate sine signals.

            Args:
                f0 (Tensor): F0 tensor (B, 1, T // hop_size).

            Returns:
                Tensor: Sines generated following NSF (B, 1, T).

            """
            B, _, T = f0.size()
            vuv = interpolate((f0 > 0) * torch.ones_like(f0), T * self.hop_size)
            radious = (interpolate(f0, T * self.hop_size) / self.sample_rate) % 1
            sine = vuv * torch.sin(torch.cumsum(radious, dim=2) * 2 * np.pi) * self.sine_amp
            if self.noise_amp > 0:
                noise_amp = vuv * self.noise_amp + (1.0 - vuv) * self.noise_amp / 3.0
                noise = torch.randn((B, 1, T * self.hop_size), device=f0.device) * noise_amp
                sine = sine + noise

            return sine

        @torch.no_grad()
        def sinusoids(self, f0):
            """Calculate sines.

            Args:
                f0 (Tensor): F0 tensor (B, 1, T // hop_size).

            Returns:
                Tensor: Sines generated following NSF (B, 1, T).

            """
            B, _, T = f0.size()
            vuv = interpolate((f0 > 0) * torch.ones_like(f0), T * self.hop_size)
            f0 = interpolate(f0, T * self.hop_size)
            sines = torch.zeros_like(f0, device=f0.device)
            harmonics = 5  # currently only fixed number of harmonics is supported
            for i in range(harmonics):
                radious = (f0 * (i + 1) / self.sample_rate) % 1
                sines += torch.sin(torch.cumsum(radious, dim=2) * 2 * np.pi)
            sines = self.sine_amp * sines * vuv / harmonics
            if self.noise_amp > 0:
                noise_amp = vuv * self.noise_amp + (1.0 - vuv) * self.noise_amp / 3.0
                noise = torch.randn((B, 1, T * self.hop_size), device=f0.device) * noise_amp
                sines = sines + noise

            return sines

        @torch.no_grad()
        def vuv_binary(self, f0):
            """Calculate V/UV binary sequences.

            Args:
                f0 (Tensor): F0 tensor (B, 1, T // hop_size).

            Returns:
                Tensor: V/UV binary sequences (B, 1, T).

            """
            _, _, T = f0.size()
            uv = interpolate((f0 > 0) * torch.ones_like(f0), T * self.hop_size)

            return uv
    */
}
