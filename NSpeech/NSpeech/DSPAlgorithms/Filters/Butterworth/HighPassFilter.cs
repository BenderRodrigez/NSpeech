using System;

namespace NSpeech.DSPAlgorithms.Filters.Butterworth
{
    /// <summary>
    /// Implemets Butterworth 4th order HighPassFilter
    /// </summary>
    internal sealed class HighPassFilter : ButterworthFilter
    {
        /// <summary>
        /// Signal sampling rate
        /// </summary>
        private readonly int _sampleRate;

        public float CutFrequency { get; set; }

        private double[] _a;
        private double[] _b;
        private double[] _c;
        private double[] _w0;
        private double[] _w1;
        private double[] _w2;

        /// <summary>
        /// Creates new filter
        /// </summary>
        /// <param name="cutFrequency">Filter's cut frequency</param>
        /// <param name="sampleRate">Signal sampling rate</param>
        public HighPassFilter(float cutFrequency, int sampleRate)
        {
            _sampleRate = sampleRate;
            CutFrequency = cutFrequency;
            Init();
        }

        protected override void Init()
        {
            _a = new double[FilterOrder];
            _b = new double[FilterOrder];
            _c = new double[FilterOrder];
            var d = 2.0 * Math.Sin(Math.PI * CutFrequency * (1.0 / _sampleRate)) / Math.Cos(Math.PI * CutFrequency * (1.0 / _sampleRate));
            for (int i = 0; i < FilterOrder; i++)
            {
                var cos = Math.Cos(Math.PI * (0.5 + (2 * (i + 1) - 1) / (4.0 * FilterOrder)));
                _a[i] = d * d + 4.0 * d * cos + 4.0;
                _b[i] = -8.0 + 2.0 * d * d;
                _c[i] = d * d - 4.0 * d * cos + 4.0;
            }
        }

        /// <summary>
        /// K-th filter chain
        /// </summary>
        /// <param name="k">Chain order</param>
        /// <param name="x">Input sample</param>
        /// <returns>Output sample</returns>
        protected override double PassFilter(int k, double x)
        {
            _w0[k] = (1.0*x - _a[k]*_w2[k] - _b[k]*_w1[k])/_c[k];
            var y = (4.0f*(_w0[k] + _w2[k] - 2.0f*_w1[k]));
            _w2[k] = _w1[k];
            _w1[k] = _w0[k];
            return y;
        }

        /// <summary>
        /// Apply filter for some signal
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <returns>Filtred signal</returns>
        public override float[] Filter(float[] signal)
        {
            _w0 = new double[FilterOrder];
            _w1 = new double[FilterOrder];
            _w2 = new double[FilterOrder];
            var resSignal = new float[signal.Length];
            for (int i = 0; i < signal.Length; i++)
            {
                var x = (double)signal[i];
                for (int k = 0; k < FilterOrder; k++)
                {
                    //iterative filter chain input
                    x = PassFilter(k, x);
                }

                resSignal[i] = (float)x;
            }
            return resSignal;
        }
    }
}
