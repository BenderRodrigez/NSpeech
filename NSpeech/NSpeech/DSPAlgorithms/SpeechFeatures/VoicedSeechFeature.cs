﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace NSpeech.DSPAlgorithms.SpeechFeatures
{
    public class VoicedSeechFeature : ISpeechFeature
    {
        private readonly double _overlapping;
        private readonly Signal _signal;
        private readonly double _windowSize;

        public VoicedSeechFeature(Signal speechSignal, double windowSize, double overlapping)
        {
            _signal = speechSignal.Clone().Normalize();
            _windowSize = windowSize;
            _overlapping = overlapping;
            InitVariables();
        }

        public int LowPassFilterBorder { get; set; }
        public float AdditiveNoiseLevel { get; set; }

        public Signal GetFeature()
        {
            var energy = GetEnergy(_windowSize, _overlapping);
            var corellation = GetCorellation(_windowSize, _overlapping);
            return
                new Signal(
                    GenerateGeneralFeature((int) Math.Round(_windowSize*_signal.SignalFormat.SampleRate), _overlapping,
                        energy,
                        corellation).ToArray(), _signal.SignalFormat.SampleRate);
        }

        private void InitVariables()
        {
            LowPassFilterBorder = 300;
            AdditiveNoiseLevel = 0.2f;
        }

        private double[] GetEnergy(double windowSize, double overlapping)
        {
            var file = _signal.Clone().ApplyLowPassFiltration(LowPassFilterBorder);

            return
                file.Split(windowSize, overlapping, WindowFunctions.WindowFunctions.Rectangular)
                    .Select(x => x.GetEnergy()*x.Samples.Length).ToArray();
        }

        private double[] GetCorellation(double windowSize, double overlapping)
        {
            double snr;
            return
                _signal.Clone().Split(windowSize, overlapping, WindowFunctions.WindowFunctions.Rectangular)
                    .Select(x => x.ApplyNoise(AdditiveNoiseLevel, out snr).GetCorrelation())
                    .ToArray();
        }

        private double[] GenerateGeneralFeature(int windowSize, double overlapping, IReadOnlyList<double> energy,
            IReadOnlyList<double> corellation)
        {
            var tmp = new List<double>(energy.Count + windowSize/2);
            tmp.AddRange(new double[windowSize/2]);
            for (var i = 0; (i < energy.Count) && (i < corellation.Count); i++)
            {
                var value = corellation[i]*Math.Pow(energy[i], 2);
                for (var j = 0; j < windowSize*(1.0 - overlapping); j++)
                    tmp.Add(value);
            }
            return tmp.ToArray();
        }

        /// <summary>
        ///     Returns voiced speech beginning and finish samples of the signal
        /// </summary>
        /// <param name="border">Solution border</param>
        /// <returns>Returns start (Item1) and stop (Item2) positions in signal</returns>
        public Tuple<int, int> GetVoicedSpeechBorder(double border = 5.0)
        {
            var feature = GetFeature();
            var start = -1;
            var stop = -1;
            for (var i = 0; (i < feature.Samples.Length) && (feature.Samples.Length - i > -1); i++)
            {
                if (start <= -1 && feature.Samples[i] > border)
                {
                    start = i;
                }
                if (stop <= -1 && feature.Samples[feature.Samples.Length - i - 1] > border)
                {
                    stop = feature.Samples.Length - i - 1;
                }
                if(start > -1 && stop > -1)
                    return new Tuple<int, int>(start, stop);
            }

            if (start == -1)
                start = 0;
            if (stop == -1)
                stop = feature.Samples.Length - 1;
            return new Tuple<int, int>(start, stop);
        }

        /// <summary>
        ///     Returns voiced speech borders in signal
        /// </summary>
        /// <param name="border">Solution border</param>
        /// <returns>Returns list of start (Item1) and stop (Item2) positions in signal</returns>
        public List<Tuple<int, int>> GetVoicedSpeechMarkers(double border = 5.0)
        {
            var feature = GetFeature();
            var marks = new List<Tuple<int, int>>();
            var start = -1;
            for (var i = 0; i < feature.Samples.Length; i++)
                    if (feature.Samples[i] < border && start > -1)
                    {
                        marks.Add(new Tuple<int, int>(start, i));
                        start = -1;
                    }
                    else if(feature.Samples[i] > border && start < 0)
                    {
                        start = i;
                    }

            if (start > -1)
                marks.Add(new Tuple<int, int>(start, feature.Samples.Length - 1));
            return marks;
        }

        private void Dump(Signal signal)
        {
            using (var writer = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "dump.txt")))
            {
                foreach (var sample in signal.Samples)
                {
                    writer.WriteLine(sample);
                }
            }
        }

        private void Dump(double[] signal)
        {
            using (var writer = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "dump.txt")))
            {
                foreach (var sample in signal)
                {
                    writer.WriteLine(sample);
                }
            }
        }
    }
}