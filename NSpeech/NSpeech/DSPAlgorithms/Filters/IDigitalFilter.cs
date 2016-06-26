namespace NSpeech.DSPAlgorithms.Filters
{
    /// <summary>
    /// Describes the main methods of the digital filter
    /// </summary>
    public interface IDigitalFilter
    {
        /// <summary>
        /// Initialize filter parameters and coeficients
        /// </summary>
        void InitFilter();

        /// <summary>
        /// Apply filter to signal
        /// </summary>
        /// <param name="signal">Some signal</param>
        /// <returns>Filtred signal</returns>
        float[] Filter(float[] signal);
        /// <summary>
        /// Apply filter to signal
        /// </summary>
        /// <param name="signal">Some signal</param>
        /// <returns>Filtred signal</returns>
        double[] Filter(double[] signal);
        /// <summary>
        /// Apply filter to signal
        /// </summary>
        /// <param name="signal">Some signal</param>
        /// <returns>Filtred signal</returns>
        short[] Filter(short[] signal);
    }
}
