using System;
using System.Linq;

namespace NSpeech.DSPAlgorithms.Basic
{
    internal static class BasicOperations
    {
        public static double Energy(double[] signal)
        {
            return signal.Sum(x => Math.Pow(x, 2))/signal.Length;
        }

        public static double Correlation(int delay, double[] signal)
        {
            var energy = 0.0;
            var corellation = 0.0;
            for (var j = 0; j < signal.Length - delay; j++)
            {
                energy += Math.Pow(signal[j], 2);
                corellation += signal[j]*signal[j + delay];
            }
            corellation = corellation/energy;

            return corellation;
        }

        /// <summary>
        ///     Mix noise and signal
        /// </summary>
        /// <param name="maxEnergyIntervalStart"></param>
        /// <param name="maxEnergyIntervalStop"></param>
        /// <param name="signal"></param>
        /// <param name="noiseLevel"></param>
        /// <param name="snr">Signal to noise raito</param>
        /// <returns>Noised signal</returns>
        public static double[] ApplyNoise(int maxEnergyIntervalStart, int maxEnergyIntervalStop, double[] signal,
            float noiseLevel, out double snr)
        {
            var signalEnergyLog = 0.0;
            for (var i = maxEnergyIntervalStart; i < maxEnergyIntervalStop; i++)
                signalEnergyLog += Math.Pow(signal[i], 2.0);
            signalEnergyLog = 20.0*Math.Log10(signalEnergyLog);

            var noise = new double[signal.Length];
            var rand = new Random();
            var energy = 0.0;
            for (var i = 0; i < signal.Length; i++)
            {
                noise[i] = (rand.NextDouble()*2.0 - 1.0)*noiseLevel;
                energy += Math.Pow(noise[i], 2.0);
            }
            energy = 20.0*Math.Log10(energy);
            snr = signalEnergyLog - energy;

            for (var i = 0; i < signal.Length; i++)
                signal[i] += noise[i];
            return signal;
        }

        /// <summary>
        ///     Calculates autocorrelation from samples
        /// </summary>
        /// <returns>autocorrelation signal samples</returns>
        public static double[] CalcAutocorrelation(double[] signal)
        {
            var complexData = Array.ConvertAll(signal, input => new Complex(input));

            var nearestSize = (int) Math.Ceiling(Math.Log(signal.Length, 2) + 1);
            var doubleSized = new Complex[(int) Math.Pow(2, nearestSize)];
            Array.Copy(complexData, doubleSized, complexData.Length);
            for (var i = complexData.Length; i < doubleSized.Length; i++)
                doubleSized[i] = Complex.Zero;

            var resultedSignal =
                FastFurierTransform.PerformBackwardTransform(
                    FastFurierTransform.PerformForwardTransform(doubleSized, doubleSized.Length)
                        .Select(x => x.ComlexSqr2())
                        .ToArray(), doubleSized.Length);

            var result = new double[signal.Length];
            Array.Copy(resultedSignal, result, result.Length);

            var k = result[0];
            return result.Select(x => x/k).ToArray();
        }
    }
}