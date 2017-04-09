using System;
using System.Linq;

namespace NSpeech.Verification.Clustering.Metrics
{
    /// <summary>
    ///     Select specific metric selected by user (or not) with enumerable.
    /// </summary>
    internal static class MetricSelector
    {
        /// <summary>
        ///     Look at enumerable value and return specific object
        /// </summary>
        /// <param name="selectedMetric">An metric</param>
        /// <returns>Metric calculator class</returns>
        internal static Func<double[], double[], double> GetMetric(Metrics selectedMetric)
        {
            switch (selectedMetric)
            {
                case Metrics.Euclidian:
                    return (a, b) =>
                    {
                        //d=total_sum((a-b)^2)
                        if (a.Length != b.Length)
                            throw new ArgumentException("Points have a different number of dimensions");

                        return a.Select((t, i) => Math.Pow(t - b[i], 2)).Sum();
                    };
                case Metrics.Manhattan:
                    return (a, b) =>
                    {
                        //d=total_sum(abs(a-b))
                        if (a.Length != b.Length)
                            throw new ArgumentException("Points have a different number of dimensions");

                        return a.Select((t, i) => Math.Abs(t - b[i])).Sum();
                    };
                default:
                    throw new ArgumentException("This metric is unknown.");
            }
        }
    }
}