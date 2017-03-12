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
            Key = new float[keySize][];
            _vq = new VectorQuantization(keySize, metric);
        }

        public float[][] Key { get; private set; }

        public void Generate(float[][] trainData)
        {
            Key = _vq.Learn(trainData[0].Length, trainData);
        }

        public SolutionState Verify(float[][] testData)
        {
            var solver = new FuzzySolver();
            return solver.MakeDecision(_vq.DistortionMeasureEnergy(testData, Key));
        }
    }
}