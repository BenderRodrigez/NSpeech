namespace NSpeech.DSPAlgorithms.Filters
{
    /// <summary>
    /// Describes the main methods of the digital filter
    /// </summary>
    public interface IDigitalFilter
    {
        /// <summary>
        /// Apply filter to signal
        /// </summary>
        /// <param name="signal">Some signal</param>
        /// <returns>Filtred signal</returns>
        Signal Filter(Signal signal);
    }
}
