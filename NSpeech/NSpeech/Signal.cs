﻿using System;
using System.Collections.Generic;
using System.Linq;
using NSpeech.DSPAlgorithms.Basic;
using NSpeech.DSPAlgorithms.Filters.Butterworth;
using NSpeech.DSPAlgorithms.SpeechFeatures;
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
        /// Provides access to the basic signal operations
        /// </summary>
        private readonly BasicOperations _operations;

        /// <summary>
        /// Creates signal
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="sampleRate"></param>
        public Signal(float[] samples, int sampleRate)
        {
            Samples = samples;
            SignalFormat = new Format(sampleRate);
            _operations = new BasicOperations();
        }

        /// <summary>
        /// Creates signal with some format info
        /// </summary>
        /// <param name="samples"></param>
        /// <param name="signalFormat"></param>
        public Signal(float[] samples, Format signalFormat)
        {
            SignalFormat = signalFormat;
            Samples = samples;
            _operations = new BasicOperations();
        }

        /// <summary>
        /// Calculates energy of the whole signal
        /// </summary>
        /// <returns>Energy value</returns>
        public double GetEnergy()
        {
            return _operations.Energy(Samples);
        }

        /// <summary>
        /// Calulates correlation of the whole signal with provided delay
        /// </summary>
        /// <param name="delay">Correlation delay in samples</param>
        /// <returns>Corellation coefficient value</returns>
        public double GetCorrelation(int delay = 1)
        {
            return _operations.Correlation(delay, Samples);
        }

        /// <summary>
        /// Calculates signal's complex spectrum
        /// </summary>
        /// <param name="size">Furier transform size</param>
        /// <returns>Frequency domain signal</returns>
        public ComplexSignal GetSpectrum(int size  = 1024)
        {
            var furierTransform = new FastFurierTransform(Samples);
            return new ComplexSignal(furierTransform.PerformForwardTransform(size), SignalFormat);
        }

        /// <summary>
        /// Converts amplitude spectrum into time domain signal
        /// </summary>
        /// <param name="size">Furier transform size</param>
        /// <returns>Time domain signal</returns>
        public Signal PerformBackwardFurierTransform(int size = 1024)
        {
            var furierTansform = new FastFurierTransform(Samples);

            return new Signal(furierTansform.PerformBackwardTransform(size), SignalFormat);
        }

        /// <summary>
        /// Apply additive noise to signal with specifed level.
        /// </summary>
        /// <param name="noiseLevel">Noise level</param>
        /// <param name="snr">Resulting Signal-to-Noise Raito</param>
        /// <param name="maxEnergyStart">Start of the maximum energy signal interval</param>
        /// <param name="maxEnergyStop">End of the maximum energy signal interval</param>
        /// <returns>Noised signal</returns>
        public Signal ApplyNoise(float noiseLevel, out double snr, int maxEnergyStart = 0, int maxEnergyStop = -1)
        {
            return new Signal(_operations.ApplyNoise(maxEnergyStart, maxEnergyStop, Samples, noiseLevel, out snr), SignalFormat);
        }

        /// <summary>
        /// Calculates signal's autocorrelation 
        /// </summary>
        /// <returns>Autocorrelational signal</returns>
        public Signal GetAutocorrelation()
        {
            return new Signal(_operations.CalcAutocorrelation(Samples), SignalFormat);
        }

        /// <summary>
        /// Returns linear prediction coefficients for the signal
        /// </summary>
        /// <param name="numberOfCoefficients">Number of the coefficients to calculate</param>
        /// <returns>Array of the coefficients</returns>
        public float[] GetLinearPredictCoefficients(int numberOfCoefficients)
        {
            var lpc = new LinearPrediction(Samples);
            return lpc.GetCoefficients(numberOfCoefficients);
        }

        /// <summary>
        /// Makes copy of the signal from <paramref name="startPosition"/> sample with length of <paramref name="length"/>
        /// </summary>
        /// <param name="startPosition">First sample to copy</param>
        /// <param name="length">Number of samples to copy</param>
        /// <returns>Part of the original signal</returns>
        public Signal ExtractAnalysisInterval(int startPosition, int length)
        {
            var interval = new float[length];
            Array.Copy(Samples, startPosition, interval, 0, length);
            return new Signal(interval, SignalFormat.SampleRate);
        }

        /// <summary>
        /// Limit samples by amplitude value. Replace all small samples by zero
        /// </summary>
        /// <param name="level">Percents value from maximal sample to limit the signal</param>
        /// <returns>Limited signal</returns>
        public Signal ApplyCentralLimitation(double level)
        {
            var maxSignal = Samples.Max(x => Math.Abs(x))*level;
            return new Signal(Samples.Select(x => Math.Abs(x) > maxSignal ? x : 0.0f).ToArray(), SignalFormat.SampleRate);
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

                intervals.Add(new Signal(windowFunction(intervalSamples), SignalFormat.SampleRate));
            }
            return intervals.ToArray();
        }


        /// <summary>
        /// Apply window function to signal's samples
        /// </summary>
        /// <param name="window">Window function type</param>
        /// <returns>Windowed signal</returns>
        public Signal ApplyWindowFunction(WindowFunctions window)
        {
            var windowFunction = WindowFunctionSelector.SelectWindowFunction(window);
            return new Signal(windowFunction(Samples), SignalFormat);
        }

        /// <summary>
        /// Apply low-pass filter to signal
        /// </summary>
        /// <param name="borderFrequency">Cut frequency</param>
        /// <returns>Filtred signal</returns>
        public Signal ApplyLowPassFiltration(float borderFrequency)
        {
            var lpf = new LowPassFilter(borderFrequency, SignalFormat.SampleRate);
            return new Signal(lpf.Filter(Samples), SignalFormat);
        }

        /// <summary>
        /// Apply high-pass filter to signal
        /// </summary>
        /// <param name="borderFrequency">Cut frequency</param>
        /// <returns>Filtred signal</returns>
        public Signal ApplyHighPassFiltration(float borderFrequency)
        {
            var hpf = new HighPassFilter(borderFrequency, SignalFormat.SampleRate);
            return new Signal(hpf.Filter(Samples), SignalFormat);
        }

        /// <summary>
        /// Apply band-pass filter to signal
        /// </summary>
        /// <param name="lowerBoder">Lower cut frequency</param>
        /// <param name="upperBorder">Upper cut frequency</param>
        /// <returns>Filtred signal</returns>
        public Signal ApplyBandPassFiltration(float lowerBoder, float upperBorder)
        {
            var bpf = new BandPassFilter(lowerBoder, upperBorder, SignalFormat.SampleRate);
            return new Signal(bpf.Filter(Samples), SignalFormat);
        }
    }
}
