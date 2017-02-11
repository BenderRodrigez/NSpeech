using System;
using System.Linq;

namespace NSpeech.DSPAlgorithms.Basic
{
    class LinearPrediction
    {
        private readonly float[] _signal;

        public LinearPrediction(float[] signal)
        {
            _signal = signal;
        }

        private double[] MakeInitialAutocorrelationalVector(int order)
        {
            var vector = new double[order+1];
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] = AutoCorrelationCoefficient(_signal, i + 1);
            }
            return vector;
        }

        /// <summary>
        /// Calculates autocorrelation coefficient in direct way
        /// </summary>
        /// <param name="inputSignal">Some signal to process</param>
        /// <param name="k">Latency factor</param>
        /// <returns>Returns k-th autocorrelation coefficient</returns>
        private double AutoCorrelationCoefficient(float[] inputSignal, int k)
        {
            var autoCorrelation = 0.0;
            var energy = 0.0;

            for (int j = 0; j < inputSignal.Length; j++)
            {
                if (j + k < inputSignal.Length)
                {
                    autoCorrelation += inputSignal[j] * inputSignal[j + k];
                    energy += inputSignal[j] * inputSignal[j];
                }
            }
            return energy > 0.0 ? autoCorrelation / energy : 0.0;
        }

        /// <summary>
        /// Calcs LPC coefficients by Durbin algorythm
        /// </summary>
        /// <param name="initialVector">Auto-correlation vector from 0 to N</param>
        /// <param name="order">Number of LPC coefficients</param>
        private double[] DurbinAlgLpcCoefficients(double[] initialVector, int order)
        {
            var tmp = new double[order];
            var lpcCoefficients = new double[order];

            var e = initialVector[0];

            for (int i = 0; i < order; i++)
            {
                var tmp0 = initialVector[i + 1];
                for (int j = 0; j < i; j++)
                    tmp0 -= lpcCoefficients[j] * initialVector[i - j];

                if (Math.Abs(tmp0) >= e) break;

                double pk;
                lpcCoefficients[i] = pk = tmp0 / e;
                e -= tmp0 * pk;

                for (int j = 0; j < i; j++)
                    tmp[j] = lpcCoefficients[j];

                for (int j = 0; j < i; j++)
                    lpcCoefficients[j] -= pk * tmp[i - j - 1];
            }
            return lpcCoefficients;
        }

        public float[] GetCoefficients(int order)
        {
            if(order < 1)
                throw new ArgumentException("Invalid order parameter! Should larger than 0.", "order");

            return
                DurbinAlgLpcCoefficients(MakeInitialAutocorrelationalVector(order), order)
                    .Select(x => (float) x)
                    .ToArray();
        }
    }
}
