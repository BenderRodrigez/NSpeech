using System;

namespace NSpeech.DSPAlgorithms.Basic
{
    /// <summary>
    /// Calulates signal's correlation coefficient with provided delay
    /// </summary>
    class SignalCorrelation:IBasicFeature
    {
        /// <summary>
        /// Signal's samples
        /// </summary>
        private readonly float[] _samples;

        /// <summary>
        /// Correlation delay in samples
        /// </summary>
        public int Delay { get; set; }

        public SignalCorrelation(float[] samples)
        {
            _samples = samples;
            Delay = 1;
        }

        /// <summary>
        /// Calulates the correlation with provided delay
        /// </summary>
        /// <returns>Corellation coefficient</returns>
        public double CalculateFeature()
        {
            var energy = 0.0;
            var corellation = 0.0;
            for (int j = 0; j < _samples.Length - Delay; j++)
            {
                energy += Math.Pow(_samples[j], 2);
                corellation += _samples[j] * _samples[j + Delay];
            }
            corellation = (float)(50.0f * (corellation / energy));

            return corellation;
        }
    }
}
