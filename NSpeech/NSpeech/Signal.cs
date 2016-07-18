using System;
using System.Collections.Generic;
using System.Linq;
using NSpeech.DSPAlgorithms.Basic;
using NSpeech.DSPAlgorithms.WindowFunctions;

namespace NSpeech
{
    /// <summary>
    /// Represent some signal
    /// </summary>
    public class Signal
    {
        /// <summary>
        /// Signal data
        /// </summary>
        public float[] Samples { get; private set; }

        /// <summary>
        /// Format of the signal (curently only sampling rate)
        /// </summary>
        public Format SignalFormat { get; private set; }

        /// <summary>
        /// Creates signal
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="sampleRate"></param>
        public Signal(float[] samples, int sampleRate)
        {
            Samples = samples;
            SignalFormat = new Format(sampleRate);
        }

        /// <summary>
        /// Calculates energy of the whole signal
        /// </summary>
        /// <returns>Energy value</returns>
        public double GetEnergy()
        {
            var energy = new SignalEnergy(Samples);
            return energy.CalculateFeature();
        }

        /// <summary>
        /// Calulates correlation of the whole signal with provided delay
        /// </summary>
        /// <param name="delay">Correlation delay in samples</param>
        /// <returns>Corellation coefficient value</returns>
        public double GetCorrelation(int delay = 1)
        {
            var correlation = new SignalCorrelation(Samples) {Delay = delay};
            return correlation.CalculateFeature();
        }

        /// <summary>
        /// Calculates signal's complex spectrum
        /// </summary>
        /// <param name="size">Furier transform size</param>
        /// <returns>Frequency domain signal</returns>
        public ComplexSignal GetSpectrum(int size  = 1024)
        {
            var furierTransform = new FastFurierTransform(Samples) {TransformSize = 1024};
            return furierTransform.GetFunction() as ComplexSignal;
        }

        /// <summary>
        /// Converts amplitude spectrum into time domain signal
        /// </summary>
        /// <param name="size">Furier transform size</param>
        /// <returns>Time domain signal</returns>
        public Signal PerformBackwardFurierTransform(int size = 1024)
        {
            var furierTansform = new FastFurierTransform(Samples)
            {
                TransformSize = size,
                Direction = FastFurierTransform.TransformationDirection.Backward
            };

            return furierTansform.GetFunction();
        }

        /// <summary>
        /// Apply additive noise to signal with specifed level.
        /// </summary>
        /// <param name="noiseLevel">Noise level</param>
        /// <param name="maxEnergyStart">Start of the maximum energy signal interval</param>
        /// <param name="maxEnergyStop">End of the maximum energy signal interval</param>
        /// <returns>Noised signal</returns>
        public Signal ApplyNoise(double noiseLevel, int maxEnergyStart = 0, int maxEnergyStop = -1)
        {
            var generator = new AdditiveNoiseGenerator(this, maxEnergyStart,
                maxEnergyStop > -1 ? maxEnergyStop : Samples.Length) {NoiseLevel = noiseLevel};
            double snr;
            return generator.ApplyNoise(out snr);
        }

        /// <summary>
        /// Calculates signal's autocorrelation 
        /// </summary>
        /// <returns>Autocorrelational signal</returns>
        public Signal GetAutocorrelation()
        {
            var autocorr = new Autocorrelation(this);
            return autocorr.GetFunction();
        }

        public double[] GetLinearPredictCoefficients(int numberOfCoefficients)
        {
            return null;//TODO: implement
        }

        /// <summary>
        /// Splits the signal on intervals of analysis
        /// </summary>
        /// <param name="intervalTime">Analysis interval time in seconds</param>
        /// <param name="overlap">Interval's overlap in percents</param>
        /// <param name="window">Window function placed on signal</param>
        /// <returns>Array of the analysis intervals</returns>
        public Signal[] Split(double intervalTime, double overlap, WindowFunctions window)
        {
            var intervalInSamples = (int)Math.Round(SignalFormat.SampleRate*intervalTime);//Convert time to samples count
            var displacements = (int) Math.Round(intervalInSamples*(1.0 - overlap));
            if (displacements < 1)
                displacements = 1;//we should have some displacement. Else we never finish this spliting.
            var intervals = new List<Signal>();
            for (var i = 0; i + intervalInSamples < Samples.Length; i += displacements)
            {
                var intervalSamples = Samples.Skip(i).Take(intervalInSamples).ToArray();
                var windowFunction = WindowFunctionSelector.SelectWindowFunction(window);

                intervals.Add(new Signal(windowFunction.ApplyWindowFunction(intervalSamples), SignalFormat.SampleRate));
            }
            return intervals.ToArray();
        }

        /// <summary>
        /// Signal's format parameters
        /// </summary>
        public struct Format
        {
            /// <summary>
            /// Signal sampling rate
            /// </summary>
            public int SampleRate { get; private set; }

            /// <summary>
            /// Creates new signal's format
            /// </summary>
            /// <param name="sampleRate">Signal's sampling rate</param>
            public Format(int sampleRate) : this()
            {
                SampleRate = sampleRate;
            }
        }
    }
}
