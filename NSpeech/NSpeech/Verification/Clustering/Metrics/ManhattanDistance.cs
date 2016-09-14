using System;
using System.Linq;

namespace NSpeech.Clustering.Metrics
{
    /// <summary>
    /// Realizing Manhattan distance metric calculation
    /// </summary>
    class ManhattanDistance: IMetric
    {
        /// <summary>
        /// Calculates Manhattan distance betwen two points
        /// </summary>
        /// <returns>The distance</returns>
        /// <param name="pointA">First point</param>
        /// <param name="pointB">Second point</param>
        public double CalcDistanceBetween(double[] pointA, double[] pointB)
        {
            //d=total_sum(abs(a-b))
            if (pointA.Length != pointB.Length) throw new ArgumentException("Points have a different number of dimensions");

            var distanceBetween = pointA.Select((t, i) => Math.Abs(t - pointB[i])).Sum();
            return distanceBetween;
        }
    }
}
