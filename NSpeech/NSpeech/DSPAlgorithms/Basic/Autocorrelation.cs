using System;
using System.Linq;

namespace NSpeech.DSPAlgorithms.Basic
{
    /// <summary>
    /// Calculates basic autocorrelation function from signal
    /// </summary>
    public class Autocorrelation
    {
        private readonly float[] _signal;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        public Autocorrelation(float[] signal)
        {
            _signal = signal;
        }

        /// <summary>
        /// Calculates autocorrelation from samples
        /// </summary>
        /// <returns>autocorrelation signal samples</returns>
        private float[] CalcAutocorrelation()
        {
            var complexData = Array.ConvertAll(_signal, input => new Complex { Real = input, Imaginary = 0.0 });


            var nearestSize = Math.Ceiling(Math.Log(_signal.Length, 2));
            var newSize = (int)nearestSize + 1;
            var doubleSized = new float[(int)Math.Pow(2, newSize)];
            Array.Copy(_signal, doubleSized, _signal.Length);

            var furier = new FastFurierTransform(doubleSized);

            var tmp = furier.PerformForwardTransform(doubleSized.Length).Select(x => (float)x.ComlexSqr()).ToArray();

            var backFurier = new FastFurierTransform(tmp);
            doubleSized = backFurier.PerformBackwardTransform(tmp.Length);
            Array.Copy(doubleSized, _signal, _signal.Length);



            var result = new double[_signal.Length];
            Array.Copy(complexData.Select(x => x.Real).ToArray(), result, _signal.Length);
            var k = result[0];
            return result.Select(x => (float)(x / k)).ToArray();
        }

        /// <summary>
        /// Calculates autocorrelation function
        /// </summary>
        /// <returns>Autocorrelation signal</returns>
        public float[] GetFunction()
        {
            return CalcAutocorrelation();
        }
    }
}