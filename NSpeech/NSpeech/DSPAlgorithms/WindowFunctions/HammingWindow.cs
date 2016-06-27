using System;

namespace NSpeech.DSPAlgorithms.WindowFunctions
{
    /// <summary>
    /// Implements Hamming window function
    /// </summary>
    class HammingWindow: IWindowFunction
    {
        /// <summary>
        /// Apply Hamming window function to signal
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <returns>Wheighted signal</returns>
        public float[] ApplyWindowFunction(float[] signal)
        {
            var x = new float[signal.Length];
            for (int i = 0; i < signal.Length / 2; i++)
            {
                x[i] = (float) (signal[i]*(0.54 - 0.46*Math.Cos(2.0*Math.PI*i/signal.Length)));
            }
            return x;
        }
    }
}
