using NumSharp;

namespace WinMMVCClient
{
    public class SinGenerator
    {
        int sampleRate = 24000;
        float[] denseFactors = new float[] { 0.5f, 1.0f, 4.0f, 8.0f };
        int[] upsampleScales = new int[] { 8, 4, 2, 2 };
        SignalGenerator signalGenerator = new SignalGenerator();

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
            float[] sin = new float[42 * 8 * 4 * 2 * 2];
            float[] d0 = new float[42 * 8];
            float[] d1 = new float[42 * 8 * 4];
            float[] d2 = new float[42 * 8 * 4 * 2];
            float[] d3 = new float[42 * 8 * 4 * 2 * 2];
            for (int i = 0; i < denseFactors.Length; i++)
            {
                var dilatedTensor = DilatedFactor(f0, sampleRate, denseFactors[i]);
                //result = [torch.stack([dilated_tensor for _ in range(us)], -1).reshape(dilated_tensor.shape[0], -1)]
                //dfs_batch.append(torch.cat(result, dim = 0).unsqueeze(1))
            }
            signalGenerator.GenerateSignal(f0).CopyTo(sin);
            return (sin, d0, d1, d2, d3);
        }

        public float[] DilatedFactor(ReadOnlySpan<float> f0, int sampleRate, float denseFactor)
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
                var _f0 = f0[i];
                if (_f0 == 0)
                {
                    _f0 = sampleRate / denseFactor;
                }
                dilatedFactors[i] = sampleRate / denseFactor / _f0;
            }

            return dilatedFactors;
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

        int sampleRate = 24000;
        int hopSize = 128;
        float sineAmp = 0.1f;
        float noiseAmp = 0.003f;

        public SignalGenerator(int _sampleRate = 24000, int _hopSize = 128, float _sineAmp = 0.1f, float _noiseAmp = 0.003f)
        {
            sampleRate = _sampleRate;
            hopSize = _hopSize;
            sineAmp = _sineAmp;
            noiseAmp = _noiseAmp;
        }

        public ReadOnlySpan<float> GenerateSignal(ReadOnlySpan<float> f0, float f0Scale = 1.0f)
        {
            // return self.sinusoid(f0) * f0_scale
            var signal = Sinusoid(f0);
            for (var i = 0; i < signal.Length; i++)
            {
                signal[i] *= f0Scale;
            }
            return signal;
        }

        public float[] Sinusoid(ReadOnlySpan<float> f0)
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
            return new float[f0.Length];
        }

        public static float[] InterpolateLinear(float[] input, int scale)
        {
            /*
             * floatの配列をscale倍して間を線形補間する
             */
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
