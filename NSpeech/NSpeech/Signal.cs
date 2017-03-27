using System;
using System.Collections.Generic;
using System.Linq;
using NSpeech.DSPAlgorithms.Basic;
using NSpeech.DSPAlgorithms.Filters;
using NSpeech.DSPAlgorithms.Filters.Butterworth;
using NSpeech.DSPAlgorithms.WindowFunctions;

namespace NSpeech
{
    /// <summary>
    ///     Represent some signal
    /// </summary>
    public class Signal
    {
        /// <summary>
        ///     Creates signal
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="sampleRate"></param>
        public Signal(double[] samples, int sampleRate)
        {
            Samples = samples;
            SignalFormat = new Format(sampleRate);
        }

        /// <summary>
        ///     Creates signal with some format info
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="signalFormat"></param>
        public Signal(double[] samples, Format signalFormat)
        {
            SignalFormat = signalFormat;
            Samples = samples;
        }

        /// <summary>
        ///     Signal data
        /// </summary>
        public double[] Samples { get; private set; }

        /// <summary>
        ///     Format of the signal (curently only sampling rate)
        /// </summary>
        public Format SignalFormat { get; }

        /// <summary>
        ///     Calculates energy of the whole signal
        /// </summary>
        /// <returns>Energy value</returns>
        public double GetEnergy()
        {
            return BasicOperations.Energy(Samples);
        }

        /// <summary>
        ///     Normalize signal's samples to range from -1 to 1
        /// </summary>
        /// <returns>Normalized signal</returns>
        public Signal Normalize()
        {
            var max = Samples.Max(f => Math.Abs(f));

            Samples = Samples.Select(x => x/max).ToArray();
            return this;
        }

        /// <summary>
        ///     Calulates correlation of the whole signal with provided delay
        /// </summary>
        /// <param name="delay">Correlation delay in samples</param>
        /// <returns>Corellation coefficient value</returns>
        public double GetCorrelation(int delay = 1)
        {
            return BasicOperations.Correlation(delay, Samples);
        }

        /// <summary>
        ///     Calculates signal's complex spectrum
        /// </summary>
        /// <param name="size">Furier transform size</param>
        /// <returns>Frequency domain signal</returns>
        public ComplexSignal GetSpectrum(int size)
        {
            return new ComplexSignal(FastFurierTransform.PerformForwardTransform(Samples, size), SignalFormat);
        }

        /// <summary>
        ///     Converts amplitude spectrum into time domain signal
        /// </summary>
        /// <param name="size">Furier transform size</param>
        /// <returns>Time domain signal</returns>
        public Signal PerformBackwardFurierTransform(int size = 1024)
        {
            Samples = FastFurierTransform.PerformBackwardTransform(Samples, size).ToArray();
            return this;
        }

        /// <summary>
        ///     Apply additive noise to signal with specifed level.
        /// </summary>
        /// <param name="noiseLevel">Noise level</param>
        /// <param name="snr">Resulting Signal-to-Noise Raito</param>
        /// <param name="maxEnergyStart">Start of the maximum energy signal interval</param>
        /// <param name="maxEnergyStop">End of the maximum energy signal interval</param>
        /// <returns>Noised signal</returns>
        public Signal ApplyNoise(float noiseLevel, out double snr, int maxEnergyStart = 0, int maxEnergyStop = -1)
        {
            Samples = BasicOperations.ApplyNoise(maxEnergyStart, maxEnergyStop, Samples, noiseLevel, out snr);
            return this;
        }

        /// <summary>
        ///     Calculates signal's autocorrelation
        /// </summary>
        /// <returns>Autocorrelational signal</returns>
        public Signal GetAutocorrelation()
        {
            Samples = BasicOperations.CalcAutocorrelation(Samples);
            return this;
        }

        public Signal GetAutocorrelation(double centralLimitationLevel)
        {
            return ApplyCentralLimitation(centralLimitationLevel).GetAutocorrelation();
        }

        /// <summary>
        ///     Returns linear prediction coefficients for the signal
        /// </summary>
        /// <param name="numberOfCoefficients">Number of the coefficients to calculate</param>
        /// <returns>Array of the coefficients</returns>
        public double[] GetLinearPredictCoefficients(int numberOfCoefficients)
        {
            var lpc = new LinearPrediction(Samples);
            return lpc.GetCoefficients(numberOfCoefficients);
        }

        /// <summary>
        ///     Makes copy of the signal from <paramref name="startPosition" /> sample with length of <paramref name="length" />
        /// </summary>
        /// <param name="startPosition">First sample to copy</param>
        /// <param name="length">Number of samples to copy</param>
        /// <returns>Part of the original signal</returns>
        public Signal ExtractAnalysisInterval(int startPosition, int length)
        {
            var interval = new double[length];
            Array.Copy(Samples, startPosition, interval, 0, length+startPosition < Samples.Length ? length : Samples.Length - startPosition);
            return new Signal(interval, SignalFormat.SampleRate);
        }

        /// <summary>
        ///     Limit samples by amplitude value. Replace all small samples by zero
        /// </summary>
        /// <param name="level">Percents value from maximal sample to limit the signal</param>
        /// <returns>Limited signal</returns>
        public Signal ApplyCentralLimitation(double level)
        {
            if ((level < 0.0) || (level > 1.0))
                throw new ArgumentOutOfRangeException(nameof(level), level, "Value should be in range from 0.0 to 1.0");

            var maxSignal = Samples.Max(x => Math.Abs(x))*level;
            Samples = Samples.Select(x => Math.Abs(x) > maxSignal ? x : 0.0f).ToArray();
            return this;
        }

        /// <summary>
        ///     Splits the signal on intervals of analysis
        /// </summary>
        /// <param name="intervalTime">Analysis interval time in seconds</param>
        /// <param name="overlap">Interval's overlap in percents</param>
        /// <param name="window">Window function placed on signal</param>
        /// <returns>Array of the analysis intervals</returns>
        public Signal[] Split(double intervalTime, double overlap, WindowFunctions window)
        {
            var intervalInSamples = (int) Math.Round(SignalFormat.SampleRate*intervalTime);
                //Convert time to samples count
            var displacements = (int) Math.Round(intervalInSamples*(1.0 - overlap));
            if (displacements < 1)
                displacements = 1; //we should have some displacement. Else we never finish this spliting.
            var intervals = new List<Signal>();
            var windowFunction = WindowFunctionSelector.SelectWindowFunction(window);
            for (var i = 0; i + intervalInSamples < Samples.Length; i += displacements)
            {
                var intervalSamples = new double[intervalInSamples];
                Array.Copy(Samples, i, intervalSamples, 0, intervalInSamples);
                intervals.Add(new Signal(windowFunction(intervalSamples), SignalFormat.SampleRate));
            }
            return intervals.ToArray();
        }

        public Signal[] Split(double intervalTime, double overlap, WindowFunctions window, int from, int until)
        {
            var intervalInSamples = (int)Math.Round(SignalFormat.SampleRate * intervalTime);
            //Convert time to samples count
            var displacements = (int)Math.Round(intervalInSamples * (1.0 - overlap));
            if (displacements < 1)
                displacements = 1; //we should have some displacement. Else we never finish this spliting.
            var windowFunction = WindowFunctionSelector.SelectWindowFunction(window);
            var intervals = new List<Signal>();
            for (var i = from; i + intervalInSamples < Samples.Length && i + intervalInSamples < until; i += displacements)
            {
                var intervalSamples = new double[intervalInSamples];
                Array.Copy(Samples, i, intervalSamples, 0, intervalInSamples);
                intervals.Add(new Signal(windowFunction(intervalSamples), SignalFormat.SampleRate));
            }
            return intervals.ToArray();
        }


        /// <summary>
        ///     Apply window function to signal's samples
        /// </summary>
        /// <param name="window">Window function type</param>
        /// <returns>Windowed signal</returns>
        public Signal ApplyWindowFunction(WindowFunctions window)
        {
            var windowFunction = WindowFunctionSelector.SelectWindowFunction(window);
            Samples = windowFunction(Samples);
            return this;
        }

        /// <summary>
        ///     Apply low-pass filter to signal
        /// </summary>
        /// <param name="borderFrequency">Cut frequency</param>
        /// <returns>Filtred signal</returns>
        public Signal ApplyLowPassFiltration(float borderFrequency)
        {
            var lpf = new LowPassFilter(borderFrequency, SignalFormat.SampleRate);
            Samples = lpf.Filter(Samples);
            return this;
        }

        /// <summary>
        ///     Apply high-pass filter to signal
        /// </summary>
        /// <param name="borderFrequency">Cut frequency</param>
        /// <returns>Filtred signal</returns>
        public Signal ApplyHighPassFiltration(float borderFrequency)
        {
            var hpf = new HighPassFilter(borderFrequency, SignalFormat.SampleRate);
            Samples = hpf.Filter(Samples);
            return this;
        }

        /// <summary>
        ///     Apply band-pass filter to signal
        /// </summary>
        /// <param name="lowerBoder">Lower cut frequency</param>
        /// <param name="upperBorder">Upper cut frequency</param>
        /// <returns>Filtred signal</returns>
        public Signal ApplyBandPassFiltration(float lowerBoder, float upperBorder)
        {
            var bpf = new BandPassFilter(lowerBoder, upperBorder, SignalFormat.SampleRate);
            Samples = bpf.Filter(Samples);
            return this;
        }

        public Signal ApplyGaussianBlur(int diameter)
        {
            if (diameter%2 != 1)
                throw new ArgumentException("Diameter should be even value");
            var blur = new GaussianFilter(diameter);
            Samples = blur.Filter(Samples);
            return this;
        }

        /// <summary>
        ///     Makes new signal object with new samples and old signal format
        /// </summary>
        /// <returns>Shallow copy of the object</returns>
        public Signal Clone()
        {
            var newSamples = new double[Samples.Length];
            Array.Copy(Samples, newSamples, Samples.Length);
            return new Signal(newSamples, SignalFormat);
        }

        /// <summary>
        ///     Generates additive signal as product of sum of two signals
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static Signal operator +(Signal a, Signal b)
        {
            if (a == null) throw new ArgumentNullException(nameof(a));
            if (b == null) throw new ArgumentNullException(nameof(b));
            if (a.SignalFormat.SampleRate != b.SignalFormat.SampleRate)
                throw new ArgumentException("Signals should have the same Sample Rate");

            var shortest = a.Samples.Length < b.Samples.Length ? a.Samples : b.Samples;
            var longest = a.Samples.Length > b.Samples.Length ? a.Samples : b.Samples;

            var newSamples = new double[longest.Length];

            for (var i = 0; i < shortest.Length; i++)
                newSamples[i] = a.Samples[i] + b.Samples[i];

            for (var i = shortest.Length; i < longest.Length; i++)
                newSamples[i] = longest[i];

            return new Signal(newSamples, a.SignalFormat);
        }

        internal Signal GetSpectrumAutocorrelation(int spectrumSize, GaussianFilter gaussianFilter)
        {
            var spectrum = FastFurierTransform.PerformForwardTransform(Samples, spectrumSize);

            Samples =
                BasicOperations.CalcAutocorrelation(
                    gaussianFilter.Filter(spectrum.Select(x => Math.Sqrt(x.ComlexSqr())).ToArray()));
            return this;
        }
    }
}