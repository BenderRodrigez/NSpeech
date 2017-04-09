using System;

namespace NSpeech
{
    /// <summary>
    ///     Complex number
    /// </summary>
    public struct Complex
    {
        public Complex(double real) : this()
        {
            Real = real;
        }

        /// <summary>
        ///     Real part
        /// </summary>
        public double Real;

        /// <summary>
        ///     Imaginary part
        /// </summary>
        public double Imaginary;

        /// <summary>
        ///     Represents the zero
        /// </summary>
        public static Complex Zero
        {
            get { return new Complex {Real = 0.0, Imaginary = 0.0}; }
        }

        /// <summary>
        ///     Represents the imaginary one.
        /// </summary>
        public static Complex ImaginaryOne
        {
            get { return new Complex {Real = 0.0, Imaginary = -1.0}; }
        }

        /// <summary>
        ///     Complex square
        /// </summary>
        /// <returns>Value of the square</returns>
        public double ComlexSqr()
        {
            return Math.Pow(Real, 2) + Math.Pow(Imaginary, 2);
        }

        /// <summary>
        ///     Complex square
        /// </summary>
        /// <returns>Value of the square in complex number format</returns>
        public Complex ComlexSqr2()
        {
            Real = Math.Pow(Real, 2) + Math.Pow(Imaginary, 2);
            Imaginary = 0;
            return this;
        }
    }
}