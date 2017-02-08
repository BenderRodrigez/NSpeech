using System;
using System.Collections.Generic;
using System.Linq;

namespace NSpeech.DSPAlgorithms.SpeechFeatures
{
    public class VoicedSeechFeature: ISpeechFeature
    {
        private readonly Signal _signal;
        private readonly double _windowSize;
        private readonly double _overlapping;

        public int LowPassFilterBorder { get; set; }
        public float AdditiveNoiseLevel { get; set; }

        public VoicedSeechFeature(Signal speechSignal, double windowSize, double overlapping)
        {
            _signal = speechSignal;
            _windowSize = windowSize;
            _overlapping = overlapping;
            InitVariables();
        }

        public Signal GetFeature()
        {
            var energy = GetEnergy(_windowSize, _overlapping);
            var corellation = GetCorellation(_windowSize, _overlapping);
            return
                new Signal(
                    GenerateGeneralFeature((int) Math.Round(_windowSize*_signal.SignalFormat.SampleRate), energy,
                        corellation).Select(x => (float) x).ToArray(), _signal.SignalFormat.SampleRate);
        }

        private void InitVariables()
        {
            LowPassFilterBorder = 300;
            AdditiveNoiseLevel = 0.2f;
        }

        private double[] GetEnergy(double windowSize, double overlapping)
        {
            var file = _signal.ApplyLowPassFiltration(LowPassFilterBorder);

            return
                file.Split(windowSize, overlapping, WindowFunctions.WindowFunctions.Rectangular)
                    .Select(x => x.GetEnergy()*file.SignalFormat.SampleRate).ToArray();
        }

        private double[] GetCorellation(double windowSize, double overlapping)
        {
            double snr;
            return 
                _signal.Split(windowSize, overlapping, WindowFunctions.WindowFunctions.Rectangular)
                    .Select(x => x.ApplyNoise(AdditiveNoiseLevel, out snr).GetCorrelation())
                    .ToArray();
        }

        private double[] GenerateGeneralFeature(int windowSize, IReadOnlyList<double> energy, IReadOnlyList<double> corellation)
        {
            var tmp = new List<double>(energy.Count + windowSize / 2);
            tmp.AddRange(new double[windowSize / 2]);
            for (int i = 0; i < energy.Count && i < corellation.Count; i++)
            {
                tmp.Add(corellation[i] * energy[i]);
            }
            return tmp.ToArray();
        }

        /// <summary>
        /// Returns voiced speech beginning and finish samples of the signal
        /// </summary>
        /// <param name="border">Solution border</param>
        /// <returns>Returns start (Item1) and stop (Item2) positions in signal</returns>
        public Tuple<int, int> GetVoicedSpeechBorder(double border = 5.0)
        {
            var feature = GetFeature();
            var start = -1;
            var stop = -1;
            for (int i = 0; i < feature.Samples.Length && feature.Samples.Length - i > -1; i++)
            {
                if (feature.Samples[i] > border)
                {
                    if (start <= -1)
                    {
                        start = i;
                    }
                    if (stop <= -1)
                    {
                        stop = feature.Samples.Length - i - 1;
                    }
                }
            }

            if (start == -1)
                start = 0;
            if (stop == -1)
                stop = feature.Samples.Length - 1;
            return new Tuple<int, int>(start, stop);
        }

        /// <summary>
        /// Returns voiced speech borders in signal
        /// </summary>
        /// <param name="border">Solution border</param>
        /// <returns>Returns list of start (Item1) and stop (Item2) positions in signal</returns>
        public List<Tuple<int, int>> GetVoicedSpeechMarkers(double border = 5.0)
        {
            var feature = GetFeature();
            var marks = new List<Tuple<int, int>>();
            var start = -1;
            for (int i = 0; i < feature.Samples.Length; i++)
            {
                if (feature.Samples[i] > border)
                {
                    if (start > -1)
                    {
                        marks.Add(new Tuple<int, int>(start, i));
                        start = -1;
                    }
                    else
                    {
                        start = i;
                    }
                }
            }

            if (start > -1)
            {
                marks.Add(new Tuple<int, int>(start, feature.Samples.Length - 1));
            }
            return marks;
        }
    }
}
