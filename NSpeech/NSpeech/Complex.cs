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
        public double Real { get; set; }

        /// <summary>
        ///     Imaginary part
        /// </summary>
        public double Imaginary { get; set; }

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
    }
}