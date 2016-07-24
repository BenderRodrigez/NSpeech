using System;

namespace NSpeech.DSPAlgorithms.WindowFunctions
{
    /// <summary>
    /// Implements Blackman window function
    /// </summary>
    class BlackmanWindow:IWindowFunction
    {
        private const double A = 0.16;
        private const double B = (1.0 - A) / 2.0;
        private const double C = 0.5;
        private const double D = A / 2.0;

        /// <summary>
        /// Apply Hamming window function to signal
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <returns>Wheighted signal</returns>
        public float[] ApplyWindowFunction(float[] signal)
        {
            var x = new float[signal.Length];
            for (var i = 0; i < x.Length; i++)
            {
                x[i] = signal[i] * (float)(B - C * Math.Cos(2.0 * Math.PI * i / signal.Length) + D * Math.Cos(4.0 * Math.PI * i / signal.Length));
            }
            return x;
        }

        public Signal ApplyWindowFunction(Signal signal)
        {
            return new Signal(ApplyWindowFunction(signal.Samples), signal.SignalFormat.SampleRate);
        }
    }
}
