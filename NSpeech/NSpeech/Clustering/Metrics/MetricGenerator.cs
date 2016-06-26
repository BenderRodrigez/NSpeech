using System;

namespace NSpeech.Clustering.Metrics
{
    public static class MetricGenerator
    {
        public static IMetric GetMetric(Metrics selectedMetric)
        {
            switch (selectedMetric)
            {
                case Metrics.Euclidian:
                    return new EuclidianMetric();
                default:
                    throw new ArgumentException("This metric is unknown.");
            }
        }
    }
}
