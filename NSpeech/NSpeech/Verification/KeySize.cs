namespace NSpeech.Verification
{
    /// <summary>
    ///     Represents the typical sizes of voice print key sizes. Values higher than 128 and lower than 16 is useless.
    /// </summary>
    public enum KeySize
    {
        /// <summary>
        ///     Fastest key generation, lowest accuracy
        /// </summary>
        Tiny = 16,

        /// <summary>
        ///     Fast enouth
        /// </summary>
        Small = 32,

        /// <summary>
        ///     Optilmized speed and accuracy
        /// </summary>
        Standart = 64,

        /// <summary>
        ///     May provide a little bit more accuracy, but at least 2x lower processing speed
        /// </summary>
        Large = 128
    }
}