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
        internal static IWindowFunction SelectWindowFunction(WindowFunctions selectedFunction)
        {
            switch (selectedFunction)
            {
                case WindowFunctions.Rectangular:
                    return new RectangularWindow();
                case WindowFunctions.Blackman:
                    return new BlackmanWindow();
                case WindowFunctions.Hamming:
                    return new HammingWindow();
                default:
                    throw new ArgumentOutOfRangeException("selectedFunction", "Selected value and WindowFunction enumerable mismatch.");
            }
        }
    }
}
