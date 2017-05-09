using System;
using System.Collections.Generic;
using System.Linq;

namespace NSpeech.DSPAlgorithms.Basic
{
    public static class FastFurierTransform
    {
        private static void PerformTransform(Complex[] data, bool forwardDirection, int transformSize)
        {
            int n, i, i1, j, k, i2, l, l1, l2;
            double c1, c2, tx, ty, t1, t2, u1, u2, z;
            var pow = Math.Log(transformSize, 2);

            // Calculate the number of points
            n = 1;
            for (i = 0; i < pow; i++)
                n *= 2;

            // Do the bit reversal
            i2 = n >> 1;
            j = 0;
            for (i = 0; i < transformSize - 1; i++)
            {
                if (i < j)
                {
                    tx = data[i].Real;
                    ty = data[i].Imaginary;
                    data[i].Real = data[j].Real;
                    data[i].Imaginary = data[j].Imaginary;
                    data[j].Real = tx;
                    data[j].Imaginary = ty;
                }
                k = i2;

                while (k <= j)
                {
                    j -= k;
                    k >>= 1;
                }
                j += k;
            }

            // Compute the FFT 
            c1 = -1.0f;
            c2 = 0.0f;
            l2 = 1;
            for (l = 0; l < pow; l++)
            {
                l1 = l2;
                l2 <<= 1;
                u1 = 1.0f;
                u2 = 0.0f;
                for (j = 0; j < l1; j++)
                {
                    for (i = j; i < transformSize; i += l2)
                    {
                        i1 = i + l1;
                        t1 = u1 * data[i1].Real - u2 * data[i1].Imaginary;
                        t2 = u1 * data[i1].Imaginary + u2 * data[i1].Real;
                        data[i1].Real = data[i].Real - t1;
                        data[i1].Imaginary = data[i].Imaginary - t2;
                        data[i].Real += t1;
                        data[i].Imaginary += t2;
                    }
                    z = u1 * c1 - u2 * c2;
                    u2 = u1 * c2 + u2 * c1;
                    u1 = z;
                }
                c2 = (float)Math.Sqrt((1.0f - c1) / 2.0f);
                if (forwardDirection)
                    c2 = -c2;
                c1 = (float)Math.Sqrt((1.0f + c1) / 2.0f);
            }

            // Scaling for forward transform 
            if (forwardDirection)
            {
                for (i = 0; i < n; i++)
                {
                    data[i].Real /= n;
                    data[i].Imaginary /= n;
                }
            }
        }

        public static Complex[] PerformForwardTransform(double[] samples, int transformSize)
        {
            return PerformForwardTransform(samples.Select(x => new Complex {Real = x, Imaginary = 0.0}).ToArray(),
                transformSize);
        }

        public static Complex[] PerformForwardTransform(Complex[] samples, int transformSize)
        {
            if ((transformSize & (transformSize - 1)) != 0)
                throw new ArgumentException("Transform size should be a power of 2.", "transformSize");

            var spectrum = new Complex[transformSize];
            Array.Copy(samples, spectrum, samples.Length > transformSize ? transformSize : samples.Length);
            PerformTransform(spectrum, true, transformSize);

            return spectrum;
        }

        public static double[] PerformBackwardTransform(double[] samples, int transformSize)
        {
            return PerformBackwardTransform(samples.Select(x => new Complex(x)).ToArray(), transformSize);
        }

        public static double[] PerformBackwardTransform(Complex[] samples, int transformSize)
        {
            if ((transformSize & (transformSize - 1)) != 0)
                throw new ArgumentException("Transform size should be a power of 2.", "transformSize");

            var spectrum = new Complex[transformSize];
            Array.Copy(samples, spectrum, transformSize);

            PerformTransform(spectrum, false, transformSize);
            return spectrum.Select(x => x.Real).ToArray();
        }
    }
}