using System;
using System.Linq;

namespace NSpeech.DSPAlgorithms.Basic
{
    internal sealed class BasicOperations
    {
        public double Energy(float[] signal)
        {
            return signal.Sum(x => Math.Pow(x, 2)) / signal.Length;
        }

        public double Correlation(int delay, float[] signal)
        {
            var energy = 0.0;
            var corellation = 0.0;
            for (int j = 0; j < signal.Length - delay; j++)
            {
                energy += Math.Pow(signal[j], 2);
                corellation += signal[j] * signal[j + delay];
            }
            corellation = (float)(corellation / energy);

            return corellation;
        }

        /// <summary>
        /// Mix noise and signal
        /// </summary>
        /// <param name="maxEnergyIntervalStart"></param>
        /// <param name="maxEnergyIntervalStop"></param>
        /// <param name="signal"></param>
        /// <param name="noiseLevel"></param>
        /// <param name="snr">Signal to noise raito</param>
        /// <returns>Noised signal</returns>
        public float[] ApplyNoise(int maxEnergyIntervalStart, int maxEnergyIntervalStop, float[] signal, float noiseLevel, out double snr)
        {
            var signalEnergyLog = 0.0;
            for (int i = maxEnergyIntervalStart; i < maxEnergyIntervalStop; i++)
            {
                signalEnergyLog += Math.Pow(signal[i], 2.0);
            }
            signalEnergyLog = 20.0 * Math.Log10(signalEnergyLog);

            var noise = new float[signal.Length];
            var rand = new Random();
            var energy = 0.0;
            for (int i = 0; i < signal.Length; i++)
            {
                noise[i] = (float)((rand.NextDouble() * 2.0 - 1.0) * noiseLevel);
                energy += Math.Pow(noise[i], 2.0);
            }
            energy = 20.0 * Math.Log10(energy);
            snr = signalEnergyLog - energy;

            var noisedSignal = new float[signal.Length];
            for (int i = 0; i < noisedSignal.Length; i++)
            {
                noisedSignal[i] = signal[i] + noise[i];
            }
            return noisedSignal;
        }

        /// <summary>
        /// Calculates autocorrelation from samples
        /// </summary>
        /// <returns>autocorrelation signal samples</returns>
        public float[] CalcAutocorrelation(float[] signal)
        {
            var complexData = Array.ConvertAll(signal, input => new Complex { Real = input, Imaginary = 0.0 });


            var nearestSize = Math.Ceiling(Math.Log(signal.Length, 2));
            var newSize = (int)nearestSize + 1;
            var doubleSized = new float[(int)Math.Pow(2, newSize)];
            Array.Copy(signal, doubleSized, signal.Length);

            var furier = new FastFurierTransform(doubleSized);

            var tmp = furier.PerformForwardTransform(doubleSized.Length).Select(x => (float)x.ComlexSqr()).ToArray();

            var backFurier = new FastFurierTransform(tmp);
            doubleSized = backFurier.PerformBackwardTransform(tmp.Length);
            Array.Copy(doubleSized, signal, signal.Length);



            var result = new double[signal.Length];
            Array.Copy(complexData.Select(x => x.Real).ToArray(), result, signal.Length);
            var k = result[0];
            return result.Select(x => (float)(x / k)).ToArray();
        }
    }
}
