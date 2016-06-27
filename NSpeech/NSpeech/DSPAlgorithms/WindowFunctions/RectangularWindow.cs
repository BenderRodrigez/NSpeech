namespace NSpeech.DSPAlgorithms.WindowFunctions
{
    /// <summary>
    /// Implements application a rectangular window function
    /// </summary>
    class RectangularWindow:IWindowFunction
    {
        /// <summary>
        /// Apply Rectangular function for signal
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <returns>Wheighted wignal</returns>
        public float[] ApplyWindowFunction(float[] signal)
        {
            //Rectangular window == no window.
            return signal;
        }
    }
}
