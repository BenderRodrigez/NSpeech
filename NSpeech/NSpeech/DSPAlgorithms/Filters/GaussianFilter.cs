using System;

namespace NSpeech.DSPAlgorithms.Filters
{
    /// <summary>
    /// Implements one dimensional Gaussian Blur
    /// </summary>
    internal class GaussianFilter: IDigitalFilter
    {
        private readonly int _blurDiameter;
        private double _sigma;
        private double _delta;
        private double _sum;

        /// <summary>
        /// Creates new Gaussian Filter with selected blur diameter
        /// </summary>
        /// <param name="blurDiameter"></param>
        public GaussianFilter(int blurDiameter)
        {
            _blurDiameter = blurDiameter;
            InitFilter();
        }

        /// <summary>
        /// Initialize filter parameter
        /// </summary>
        private void InitFilter()
        {
            _sigma = (_blurDiameter - 1.0)/6.0;
            _delta = Math.Floor(_blurDiameter/2.0);

            _sum = 0.0;
            for (int j = (int)-_delta; j <= _delta; j++)
            {
                _sum += Math.Exp(-Math.Pow(j, 2) / (2.0 * Math.Pow(_sigma, 2))) / (Math.Sqrt(2 * Math.PI) * _sigma);
            }
        }

        /// <summary>
        /// Apply filter for some signal
        /// </summary>
        /// <param name="signal">Input signal</param>
        /// <returns>Blured signal</returns>
        public float[] Filter(float[] signal)
        {
            var data = new float[signal.Length];
            for (int i = (int)_delta; i < data.Length - _delta; i++)
            {
                var res = 0.0;
                for (int j = (int)-_delta; j <= _delta; j++)
                {
                    res += signal[i + j] * (Math.Exp(-Math.Pow(j, 2) / (2.0 * Math.Pow(_sigma, 2))) / (Math.Sqrt(2 * Math.PI) * _sigma)) / _sum;
                }
                data[i] = (float)res;
            }
            return data;
        }
    }
}
