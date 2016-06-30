using System;
using System.Linq;
using NSpeech.DSPAlgorithms.Basic;

namespace NSpeech
{
    public class ComplexSignal: Signal
    {
        public new Complex[] Samples { get; private set; }

        public ComplexSignal(float[] samples, int sampleRate) : base(samples, sampleRate)
        {
            Samples = samples.Select(x => new Complex {Real = x, Imaginary = 0.0}).ToArray();
        }

        public ComplexSignal(Complex[] samples, int sampleRate):base(samples.Select(x=> (float)Math.Sqrt(x.ComlexSqr())).ToArray(), sampleRate)
        {
            Samples = samples;
        }

        public new Signal PerformBackwardFurierTransform(int size = 1024)
        {
            var furierTansform = new FastFurierTransform(Samples)
            {
                TransformSize = size,
                Direction = FastFurierTransform.TransformationDirection.Backward
            };

            return furierTansform.GetFunction();
        }
    }
}
