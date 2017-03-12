namespace NSpeech.Verification.Solvers
{
    /// <summary>
    ///     Desccribes main methods for any solver for verification task
    /// </summary>
    public interface ISolver
    {
        SolutionState MakeDecision(double feature);
    }
}