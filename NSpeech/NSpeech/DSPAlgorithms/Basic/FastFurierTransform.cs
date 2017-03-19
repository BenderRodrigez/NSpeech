using System;
using System.Linq;

namespace NSpeech.DSPAlgorithms.Basic
{
    public class FastFurierTransform
    {
        private void PerformTransform(Complex[] data, bool forwardDirection, int transformSize)
        {
            var i2 = transformSize >> 1;
            var j = 0;
            for (var i = 0; i < transformSize - 1; i++)
            {
                if (i < j)
                {
                    var real = data[i].Real;
                    var imaginary = data[i].Imaginary;
                    data[i].Real = data[j].Real;
                    data[i].Imaginary = data[j].Imaginary;
                    data[j].Real = real;
                    data[j].Imaginary = imaginary;
                }

                var k = i2;

                while (k <= j)
                {
                    j -= k;
                    k >>= 1;
                }
                j += k;
            }

            var c1 = -1.0;
            var c2 = 0.0;
            var l2 = 1;
            var pow = Math.Log(transformSize, 2);
            for (var l = 0; l < pow; l++)
            {
                var l1 = l2;
                l2 <<= 1;
                var u1 = 1.0;
                var u2 = 0.0;
                for (j = 0; j < l1; j++)
                {
                    for (var i = j; i < transformSize; i += l2)
                    {
                        var i1 = i + l1;
                        var real = u1*data[i1].Real - u2*data[i1].Imaginary;
                        var imaginary = u1*data[i1].Imaginary + u2*data[i1].Real;
                        data[i1].Real = data[i].Real - real;
                        data[i1].Imaginary = data[i].Imaginary - imaginary;
                        data[i].Real += real;
                        data[i].Imaginary += imaginary;
                    }

                    var z = u1*c1 - u2*c2;
                    u2 = u1*c2 + u2*c1;
                    u1 = z;
                }

                c2 = Math.Sqrt((1.0 - c1)/2.0);

                c1 = Math.Sqrt((1.0 + c1)/2.0);

                if (forwardDirection)
                {
                    c2 = -c2;

                    for (var i = 0; i < transformSize; i++)
                    {
                        data[i].Real /= transformSize;
                        data[i].Imaginary /= transformSize;
                    }
                }
            }
        }

        public Complex[] PerformForwardTransform(float[] samples, int transformSize)
        {
            return PerformForwardTransform(samples.Select(x => new Complex {Real = x, Imaginary = 0.0}).ToArray(),
                transformSize);
        }

        public Complex[] PerformForwardTransform(Complex[] samples, int transformSize)
        {
            if ((transformSize & (transformSize - 1)) != 0)
                throw new ArgumentException("Transform size should be a power of 2.", "transformSize");

            var spectrum = new Complex[transformSize];
            Array.Copy(samples, spectrum, samples.Length > transformSize ? transformSize : samples.Length);
            PerformTransform(spectrum, true, transformSize);

            return spectrum;
        }

        public double[] PerformBackwardTransform(float[] samples, int transformSize)
        {
            return PerformBackwardTransform(samples.Select(x => new Complex(x)).ToArray(), transformSize);
        }

        public double[] PerformBackwardTransform(Complex[] samples, int transformSize)
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