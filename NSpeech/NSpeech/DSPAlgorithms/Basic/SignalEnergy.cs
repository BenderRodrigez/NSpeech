using System;
using System.Linq;

namespace NSpeech.DSPAlgorithms.Basic
{
    /// <summary>
    /// Calulates the energy of the signal
    /// </summary>
    public class SignalEnergy:IBasicFeature
    {
        /// <summary>
        /// Signal samples
        /// </summary>
        private readonly float[] _samples;

        public SignalEnergy(float[] samples)
        {
            _samples = samples;
        }

        /// <summary>
        /// Calculates the energy of the signal
        /// </summary>
        /// <returns>Signal's energy value</returns>
        public double CalculateFeature()
        {
            return _samples.Sum(x => Math.Pow(x, 2))/_samples.Length;
        }
    }
}
