using System;

namespace NSpeech.DSPAlgorithms.WindowFunctions
{
    /// <summary>
    /// Select specific window function
    /// </summary>
    internal static class WindowFunctionSelector
    {
        /// <summary>
        /// Look at enumerable and return specific object
        /// </summary>
        /// <param name="selectedFunction">Selected window function type</param>
        /// <returns>Window function</returns>
        internal static Func<float[], float[]> SelectWindowFunction(WindowFunctions selectedFunction)
        {
            switch (selectedFunction)
            {
                case WindowFunctions.Rectangular:
                    return (signal) =>
                    {
                        var x = new float[signal.Length];
                        Array.Copy(signal, x, signal.Length);
                        return x;
                    };
                case WindowFunctions.Blackman:
                    return (signal) =>
                    {
                        const double a = 0.16;
                        const double b = (1.0 - a)/2.0;
                        const double c = 0.5;
                        const double d = a/2.0;
                        var x = new float[signal.Length];
                        for (var i = 0; i < x.Length; i++)
                        {
                            x[i] = signal[i]*
                                   (float)
                                       (b - c*Math.Cos(2.0*Math.PI*i/signal.Length) +
                                        d*Math.Cos(4.0*Math.PI*i/signal.Length));
                        }
                        return x;
                    };
                case WindowFunctions.Hamming:
                    return (signal) =>
                    {
                        var x = new float[signal.Length];
                        for (int i = 0; i < signal.Length/2; i++)
                        {
                            x[i] = (float) (signal[i]*(0.54 - 0.46*Math.Cos(2.0*Math.PI*i/signal.Length)));
                        }
                        return x;
                    };
                default:
                    throw new ArgumentOutOfRangeException("selectedFunction", "Selected value and WindowFunction enumerable mismatch.");
            }
        }
    }
}
