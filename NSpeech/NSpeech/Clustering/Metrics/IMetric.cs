namespace NSpeech.Clustering.Metrics
{
    public interface IMetric
    {
        double CalcDistanceBetween(double[] pointA, double[] pointB);
    }
}
