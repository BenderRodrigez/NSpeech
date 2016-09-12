﻿namespace NSpeech.DSPAlgorithms.Filters
{
    internal abstract class ButterworthFilter
    {
        public ButterworthFilter()
        {
            FilterOrder = 4;
        }

        public int FilterOrder { get; set; }

        /// <summary>
        /// Init filter parameters
        /// </summary>
        protected abstract void Init();

        /// <summary>
        /// K-th filter chain
        /// </summary>
        /// <param name="k">Chain order</param>
        /// <param name="x">Input sample</param>
        /// <returns>Output sample</returns>
        protected abstract double PassFilter(int k, double x);
    }
}