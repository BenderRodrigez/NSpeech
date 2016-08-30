namespace NSpeech
{
    /// <summary>
    /// Signal's format parameters
    /// </summary>
    public class Format
    {
        /// <summary>
        /// Signal sampling rate
        /// </summary>
        public int SampleRate { get; set; }

        /// <summary>
        /// Creates new signal's format
        /// </summary>
        /// <param name="sampleRate">Signal's sampling rate</param>
        public Format(int sampleRate)
        {
            SampleRate = sampleRate;
        }
    }
}