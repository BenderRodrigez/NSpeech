namespace NSpeech.Clustering.Metrics
{
    /// <summary>
    /// Describes what the therm Metric is.
    /// </summary>
    public interface IMetric
    {
        /// <summary>
        /// Calulates the distance between two points in dimentionality size of N.
        /// </summary>
        /// <param name="pointA">First point</param>
        /// <param name="pointB">Second point</param>
        /// <returns>Distance between two points</returns>
        double CalcDistanceBetween(double[] pointA, double[] pointB);
    }
}
