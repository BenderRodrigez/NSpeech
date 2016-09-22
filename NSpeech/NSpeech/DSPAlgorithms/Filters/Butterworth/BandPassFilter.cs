using System;

namespace NSpeech.DSPAlgorithms.Filters.Butterworth
{
    /// <summary>
    /// Implemets BandPass Butterworth 4th order filter
    /// </summary>
    internal sealed class BandPassFilter: ButterworthFilter
    {
       /// <summary>
        /// Signal sampling rate
        /// </summary>
        private readonly int _sampleRate;

        public float CutFrequencyLow { get; set; }
        public float CutFrequencyHigh { get; set; }

        private double[] _a;
        private double[] _b;
        private double[] _c;
        private double[] _d;
        private double[] _e;
        private double _a1;
        private double[] _w0;
        private double[] _w1;
        private double[] _w2;
        private double[] _w3;
        private double[] _w4;

        /// <summary>
        /// Creates new filter
        /// </summary>
        /// <param name="cutFrequencyLow">Lowest pass frequency</param>
        /// <param name="cutFrequencyHigh">Highest pass frequency</param>
        /// <param name="sampleRate">Signal sampling rate</param>
        public BandPassFilter(float cutFrequencyLow, float cutFrequencyHigh, int sampleRate)
        {
            _sampleRate = sampleRate;
            CutFrequencyLow = cutFrequencyLow;
            CutFrequencyHigh = cutFrequencyHigh;
            Init();
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
            _w3 = new double[FilterOrder];
            _w4 = new double[FilterOrder];
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

        protected override void Init()
        {
            var lowFreq = 2.0 * (Math.Sin(Math.PI * CutFrequencyLow / _sampleRate)) / Math.Cos(Math.PI * CutFrequencyLow / _sampleRate);
            var highFreq = 2.0 * (Math.Sin(Math.PI * CutFrequencyHigh / _sampleRate)) / Math.Cos(Math.PI * CutFrequencyHigh / _sampleRate);

            _a = new double[FilterOrder];
            _b = new double[FilterOrder];
            _c = new double[FilterOrder];
            _d = new double[FilterOrder];
            _e = new double[FilterOrder];
            _a1 = Math.Pow(highFreq - lowFreq, 2.0);
            var c1 = 2.0 * highFreq * lowFreq + Math.Pow(highFreq - lowFreq, 2.0);
            var e1 = Math.Pow(highFreq * lowFreq, 2.0);

            for (int i = 0; i < FilterOrder; i++)
            {
                var cos = Math.Cos(Math.PI * (0.5 + (2.0 * (i + 1.0) - 1.0) / (4.0 * (FilterOrder + 1.0))));
                var b1 = -2.0 * cos;
                var d1 = -2.0 * highFreq * lowFreq * (highFreq - lowFreq) * cos;

                _a[i] = 16.0 - 8.0 * b1 + 4.0 * c1 - 2.0 * d1 + e1;
                _b[i] = -64.0 + 16.0 * b1 - 4.0 * d1 + 4.0 * e1;
                _c[i] = 96.0 - 8.0 * c1 + 6.0 * e1;
                _d[i] = -64.0 - 16.0 * b1 + 4.0 * d1 + 4.0 * e1;
                _e[i] = 16.0 + 8.0 * b1 + 4.0 * c1 + 2.0 * d1 + e1;
            }
        }

        protected override double PassFilter(int k, double x)
        {
            _w0[k] = (x - _a[k] * _w4[k] - _b[k] * _w3[k] - _c[k] * _w2[k] - _d[k] * _w1[k]) / _e[k];
            var y = ((_w0[k] - (2.0 * _w2[k]) + _w4[k]) * 4.0 * _a1);
            _w4[k] = _w3[k];
            _w3[k] = _w2[k];
            _w2[k] = _w1[k];
            _w1[k] = _w0[k];
            return y;
        }
    }
}
