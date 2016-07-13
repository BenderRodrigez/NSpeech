using System;
using System.Linq;

namespace NSpeech.DSPAlgorithms.Basic
{
    /// <summary>
    /// Calculates basic autocorrelation function from signal
    /// </summary>
    public class Autocorrelation:IBasicFunction
    {
        private readonly float[] _samples;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="signal"></param>
        public Autocorrelation(float[] signal)
        {
            _samples = signal;
        }

        /// <summary>
        /// Calculates autocorrelation from samples
        /// </summary>
        /// <returns>autocorrelation signal samples</returns>
        private double[] CalcAutocorrelation()
        {
            var complexData = Array.ConvertAll(data, input => new ComplexNumber(input));


            var nearestSize = Math.Ceiling(Math.Log(data.Length, 2));
            var newSize = (int)nearestSize + 1;
            var doubleSized = new float[(int)Math.Pow(2, newSize)];
            Array.Copy(_samples, doubleSized, _samples.Length);

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
            Array.Copy(doubleSized, data, data.Length);



            result = new double[size];
            Array.Copy(complexData.Select(x => x.RealPart).ToArray(), result, size);
            var k = result[0];
            result = result.Select(x => x / k).ToArray();
        }

        /// <summary>
        /// Calculates autocorrelation function
        /// </summary>
        /// <returns>Autocorrelation signal</returns>
        public Signal GetFunction()
        {
            throw new System.NotImplementedException();
        }
    }
}