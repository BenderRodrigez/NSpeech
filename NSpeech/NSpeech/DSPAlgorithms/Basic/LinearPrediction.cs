using System;
using System.Linq;

namespace NSpeech.DSPAlgorithms.Basic
{
    class LinearPrediction:IBasicFunction
    {
        private Signal _signal;

        public int Order { get; set; }

        public LinearPrediction(Signal signal)
        {
            _signal = signal;
            Order = 10;
        }

        private double[] MakeInitialAutocorrelationalVector()
        {
            var vector = new double[Order+1];
            var autocorr = _signal.GetAutocorrelation();
            Array.Copy(autocorr.Samples, vector, Order + 1);//TODO: possible time loss for unnecessary calculations
            return vector;
        }

        /// <summary>
        /// Calcs LPC coefficients by Durbin algorythm
        /// </summary>
        /// <param name="initialVector">Auto-correlation vector from 0 to N</param>
        /// <param name="lpcCoefficients">Output LPC values vector</param>
        private double[] DurbinAlgLpcCoefficients(double[] initialVector)
        {
            var tmp = new double[Order];
            var lpcCoefficients = new double[Order];

            var e = initialVector[0];

            for (int i = 0; i < Order; i++)
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

        public Signal GetFunction()
        {
            return
                new Signal(
                    DurbinAlgLpcCoefficients(MakeInitialAutocorrelationalVector()).Select(x => (float) x).ToArray(),
                    _signal.SignalFormat.SampleRate);
        }
    }
}
