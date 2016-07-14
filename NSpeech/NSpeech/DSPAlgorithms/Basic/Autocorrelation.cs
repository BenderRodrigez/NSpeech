using System;
using System.Linq;

namespace NSpeech.DSPAlgorithms.Basic
{
    /// <summary>
    /// Calculates basic autocorrelation function from signal
    /// </summary>
    public class Autocorrelation:IBasicFunction
    {
        private readonly Signal _signal;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        public Autocorrelation(Signal signal)
        {
            _signal = signal;
        }

        /// <summary>
        /// Calculates autocorrelation from samples
        /// </summary>
        /// <returns>autocorrelation signal samples</returns>
        private float[] CalcAutocorrelation()
        {
            var complexData = Array.ConvertAll(_signal.Samples, input => new Complex { Real = input, Imaginary = 0.0 });


            var nearestSize = Math.Ceiling(Math.Log(_signal.Samples.Length, 2));
            var newSize = (int)nearestSize + 1;
            var doubleSized = new float[(int)Math.Pow(2, newSize)];
            Array.Copy(_signal.Samples, doubleSized, _signal.Samples.Length);

            var furier = new FastFurierTransform(doubleSized)
            {
                TransformSize = doubleSized.Length,
                Direction = FastFurierTransform.TransformationDirection.Forward
            };

            var tmp = (furier.GetFunction() as ComplexSignal).Samples.Select(x => (float)x.ComlexSqr()).ToArray();

            var backFurier = new FastFurierTransform(tmp)
            {
                Direction = FastFurierTransform.TransformationDirection.Backward,
                TransformSize = tmp.Length
            };
            doubleSized = backFurier.GetFunction().Samples;
            Array.Copy(doubleSized, _signal.Samples, _signal.Samples.Length);



            var result = new double[_signal.Samples.Length];
            Array.Copy(complexData.Select(x => x.Real).ToArray(), result, _signal.Samples.Length);
            var k = result[0];
            return result.Select(x => (float)(x / k)).ToArray();
        }

        /// <summary>
        /// Calculates autocorrelation function
        /// </summary>
        /// <returns>Autocorrelation signal</returns>
        public Signal GetFunction()
        {
            return new Signal(CalcAutocorrelation(), _signal.SignalFormat.SampleRate);
        }
    }
}