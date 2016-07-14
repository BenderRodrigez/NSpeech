using System;

namespace NSpeech.DSPAlgorithms.Basic
{
    /// <summary>
    /// Apply some additive noise to digital signal with specifed aplitude
    /// </summary>
    public class AdditiveNoiseGenerator
    {
        private readonly Signal _signal;
        private readonly int _maxEnergyIntervalStart;
        private readonly int _maxEnergyIntervalStop;

        /// <summary>
        /// Noise amplitude value
        /// </summary>
        public double NoiseLevel { get; set; }

        /// <summary>
        /// Creates new noise generator
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <param name="maxEnergyIntervalStart">Start point of the maximum energy interval</param>
        /// <param name="maxEnergyIntervalStop">End point of the maximum energy interval</param>
        public AdditiveNoiseGenerator(Signal signal, int maxEnergyIntervalStart, int maxEnergyIntervalStop)
        {
            _signal = signal;
            NoiseLevel = 0.3;

            _maxEnergyIntervalStart = maxEnergyIntervalStart;
            _maxEnergyIntervalStop = maxEnergyIntervalStop;
        }

        /// <summary>
        /// Mix noise and signal
        /// </summary>
        /// <param name="snr">Signal to noise raito</param>
        /// <returns>Noised signal</returns>
        public Signal ApplyNoise(out double snr)
        {
            var signalEnergyLog = 0.0;
            for (int i = _maxEnergyIntervalStart; i < _maxEnergyIntervalStop; i++)
            {
                signalEnergyLog += Math.Pow(_signal.Samples[i], 2.0);
            }
            signalEnergyLog = 20.0 * Math.Log10(signalEnergyLog);

            var noise = new float[_signal.Samples.Length];
            var rand = new Random();
            var energy = 0.0;
            for (int i = 0; i < _signal.Samples.Length; i++)
            {
                noise[i] = (float)((rand.NextDouble() * 2.0 - 1.0) * NoiseLevel);
                energy += Math.Pow(noise[i], 2.0);
            }
            energy = 20.0 * Math.Log10(energy);
            snr = signalEnergyLog - energy;

            var noisedSignal = new float[_signal.Samples.Length];
            for (int i = 0; i < noisedSignal.Length; i++)
            {
                noisedSignal[i] = _signal.Samples[i] + noise[i];
            }
            return new Signal(noisedSignal, _signal.SignalFormat.SampleRate);
        }
    }
}