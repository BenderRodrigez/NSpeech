using System;

namespace NSpeech.Clustering.Metrics
{
    /// <summary>
    /// Genetate specific metric selected by user (or not) with enumerable.
    /// </summary>
    internal static class MetricGenerator
    {
        /// <summary>
        /// Look at enumerable value and return specific object
        /// </summary>
        /// <param name="selectedMetric">An metric</param>
        /// <returns>Metric calculator class</returns>
        internal static IMetric GetMetric(Metrics selectedMetric)
        {
            switch (selectedMetric)
            {
                case Metrics.Euclidian:
                    return new EuclidianDistance();
                case Metrics.Manhattan:
                    return new ManhattanDistance();
                default:
                    throw new ArgumentException("This metric is unknown.");
            }
        }
    }
}
