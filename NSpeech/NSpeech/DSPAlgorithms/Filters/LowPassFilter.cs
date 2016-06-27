using System;

namespace NSpeech.DSPAlgorithms.Filters
{
    /// <summary>
    /// Implements Butterworth 4th order LowPassFilter  
    /// </summary>
    public class LowPassFilter:IDigitalFilter
    {
        /// <summary>
        /// Signal sampling rate
        /// </summary>
        private readonly int _sampleRate;

        private const int FilterOrder = 4;

        private double[] _a;
        private double[] _b;
        private double[] _c;
        private double _d;
        private double[] _w0;
        private double[] _w1;
        private double[] _w2;

        /// <summary>
        /// Creates new filter
        /// </summary>
        /// <param name="cutFrequency">Filter's cut frequency</param>
        /// <param name="sampleRate">Signal sampling rate</param>
        public LowPassFilter(float cutFrequency, int sampleRate)
        {
            _sampleRate = sampleRate;
            InitFilter(cutFrequency);
        }

        /// <summary>
        /// Init filter parameters
        /// </summary>
        /// <param name="cutFreq">Filter's cut frequency</param>
        private void InitFilter(float cutFreq)
        {
            _d = 0.0;
            _a = new double[FilterOrder];
            _b = new double[FilterOrder];
            _c = new double[FilterOrder];

            var tangens = (float)(2.0 * Math.Sin(Math.PI * cutFreq * (1.0 / _sampleRate)) / Math.Cos(Math.PI * cutFreq * (1.0 / _sampleRate)));
            _d = (float)Math.Pow(tangens, 2);
            for (int i = 0; i < FilterOrder; i++)
            {
                var sinus = tangens * Math.Sin(Math.PI * (0.5 + (2.0 * (i + 1) - 1.0) / (2.0 * FilterOrder)));
                var cosinus = tangens * Math.Cos(Math.PI * (0.5 + (2.0 * (i + 1) - 1.0) / (2.0 * FilterOrder)));
                _a[i] = (float)(4.0 + 4.0 * cosinus + Math.Pow(cosinus, 2) + Math.Pow(sinus, 2));
                _b[i] = (float)(-8.0 + 2.0 * (Math.Pow(cosinus, 2) + Math.Pow(sinus, 2)));
                _c[i] = (float)(4.0 - 4.0 * cosinus + Math.Pow(cosinus, 2) + Math.Pow(sinus, 2));
            }
        }

        /// <summary>
        /// K-th filter chain
        /// </summary>
        /// <param name="k">Chain order</param>
        /// <param name="x">Input sample</param>
        /// <returns>Output sample</returns>
        private double FilterElementPass(int k, double x)
        {
            _w0[k] = (1.0f * x - (_a[k] * _w2[k]) - (_b[k] * _w1[k])) / _c[k];
            var y = _d * (_w0[k] + _w2[k] + (2.0f * _w1[k]));
            _w2[k] = _w1[k];
            _w1[k] = _w0[k];
            return y;
        }

        /// <summary>
        /// Apply filter for some signal
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <returns>Filtred signal</returns>
        public float[] Filter(float[] signal)
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
                    x = FilterElementPass(k, x);
                }

                resSignal[i] = (float)x;
            }
            return resSignal;
        }

        /// <summary>
        /// Apply filter for some signal
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <returns>Filtred signal</returns>
        public double[] Filter(double[] signal)
        {
            _w0 = new double[FilterOrder];
            _w1 = new double[FilterOrder];
            _w2 = new double[FilterOrder];
            var resSignal = new double[signal.Length];
            for (int i = 0; i < signal.Length; i++)
            {
                var x = signal[i];
                for (int k = 0; k < FilterOrder; k++)
                {
                    //iterative filter chain input
                    x = FilterElementPass(k, x);
                }

                resSignal[i] = x;
            }
            return resSignal;
        }

        /// <summary>
        /// Apply filter for some signal
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <returns>Filtred signal</returns>
        public short[] Filter(short[] signal)
        {
            _w0 = new double[FilterOrder];
            _w1 = new double[FilterOrder];
            _w2 = new double[FilterOrder];
            var resSignal = new short[signal.Length];
            for (int i = 0; i < signal.Length; i++)
            {
                var x = (double)signal[i];
                for (int k = 0; k < FilterOrder; k++)
                {
                    //iterative filter chain input
                    x = FilterElementPass(k, x);
                }

                resSignal[i] = (short)Math.Round(x);
            }
            return resSignal;
        }
    }
}
