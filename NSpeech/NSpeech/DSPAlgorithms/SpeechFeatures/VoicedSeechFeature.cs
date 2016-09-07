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
                    .Select(x => x.GetEnergy()).ToArray();
        }

        private double[] GetCorellation(double windowSize, double overlapping)
        {
            return 
                _signal.Split(windowSize, overlapping, WindowFunctions.WindowFunctions.Rectangular)
                    .Select(x => x.ApplyNoise(AdditiveNoiseLevel).GetCorrelation())
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
    }
}
