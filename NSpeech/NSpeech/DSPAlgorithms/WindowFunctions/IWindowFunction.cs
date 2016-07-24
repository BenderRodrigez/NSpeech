namespace NSpeech.DSPAlgorithms.WindowFunctions
{
    /// <summary>
    /// Describes main functions of each WindowFunction type
    /// </summary>
    public interface IWindowFunction
    {
        float[] ApplyWindowFunction(float[] signal);

        Signal ApplyWindowFunction(Signal signal);
    }
}