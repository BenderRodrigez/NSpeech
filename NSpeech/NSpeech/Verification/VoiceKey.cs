using NSpeech.Verification.Clustering;
using NSpeech.Verification.Clustering.Metrics;
using NSpeech.Verification.Solvers;

namespace NSpeech.Verification
{
    public class VoiceKey
    {
        private readonly VectorQuantization _vq;

        public VoiceKey(int keySize, Metrics metric)
        {
            Key = new double[keySize][];
            _vq = new VectorQuantization(keySize, metric);
        }

        public double[][] Key { get; private set; }

        public void Generate(double[][] trainData)
        {
            Key = _vq.Learn(trainData[0].Length, trainData);
        }

        public SolutionState Verify(double[][] testData)
        {
            var solver = new FuzzySolver();
            return solver.MakeDecision(_vq.DistortionMeasureEnergy(testData, Key));
        }
    }
}