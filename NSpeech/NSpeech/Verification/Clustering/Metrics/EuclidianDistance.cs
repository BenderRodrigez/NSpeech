using System;
using System.Linq;

namespace NSpeech.Clustering.Metrics
{
    /// <summary>
    /// Realizing Euclidian distance metric calculation
    /// </summary>
    public class EuclidianDistance : IMetric
    {
        /// <summary>
        /// Calculates Euclidian distance betwen two points
        /// </summary>
        /// <returns>The distance</returns>
        /// <param name="pointA">First point</param>
        /// <param name="pointB">Second point</param>
        public double CalcDistanceBetween(double[] pointA, double[] pointB)
        {
            //d=total_sum(a^2-b^2)
            if (pointA.Length != pointB.Length) throw new ArgumentException("Points have a different number of dimensions");

            var distanceBetween = pointA.Select((t, i) => Math.Pow(t - pointB[i], 2)).Sum();
            return distanceBetween;
        }
    }
}
