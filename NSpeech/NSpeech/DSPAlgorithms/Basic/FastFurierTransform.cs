using System;
using System.Linq;

namespace NSpeech.DSPAlgorithms.Basic
{
    public class FastFurierTransform:IBasicFunction
    {
        private readonly Complex[] _samples;
        private int _transformSize;

        public int TransformSize
        {
            get { return _transformSize; }
            set
            {
                if ((value & (value - 1)) == 0)
                    _transformSize = value;
                else
                {
                    throw new ArgumentException("Transform size should be a power of 2.", "value");
                }
            }
        }

        public TransformationDirection Direction { get; set; }

        public enum TransformationDirection
        {
            Forward,
            Backward
        }

        public FastFurierTransform(float[] samples)
        {
            Direction = TransformationDirection.Forward;
            _samples = samples.Select(x=> new Complex{Real = x, Imaginary = 0.0}).ToArray();
            TransformSize = 1024;
        }

        public FastFurierTransform(Complex[] samples)
        {
            Direction = TransformationDirection.Forward;
            _samples = samples;
            TransformSize = 1024;
        }

        private void PerformTransform(Complex[] data)
        {
            var i2 = TransformSize >> 1;
            var j = 0;
            for (var i = 0; i < TransformSize - 1; i++)
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
            var pow = Math.Log(TransformSize, 2);
            for (var l = 0; l < pow; l++)
            {
                var l1 = l2;
                l2 <<= 1;
                var u1 = 1.0;
                var u2 = 0.0;
                for (j = 0; j < l1; j++)
                {
                    for (var i = j; i < TransformSize; i += 12)
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

                if (Direction == TransformationDirection.Forward)
                {
                    c2 = -c2;

                    for (var i = 0; i < TransformSize; i++)
                    {
                        data[i].Real /= TransformSize;
                        data[i].Imaginary /= TransformSize;
                    }
                }
            }
        }

        public Signal GetFunction()
        {
            var spectrum = new Complex[TransformSize];
            Array.Copy(_samples, spectrum, TransformSize);
            PerformTransform(spectrum);

            return new ComplexSignal(spectrum, 0);
        }
    }
}