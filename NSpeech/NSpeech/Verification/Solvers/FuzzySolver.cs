using System;

namespace NSpeech.Verification.Solvers
{
    /// <summary>
    ///     Implements speaker vrification descidion based on membership functions
    /// </summary>
    internal class FuzzySolver : ISolver
    {
        private const double BellFunctionCenter = 450.0;
        private const double BellFunctionSize = 450.0;
        private const double BellFunctionTilt = 5.0;

        private const double SigmoidalFunctionHalfPosition = 550.0;
        private const double SigmoidalFunctionFullPosition = 1100.0;

        /// <summary>
        ///     Init with basic parameters
        /// </summary>
        public FuzzySolver()
        {
            VerifyBorder = 0.5;
            BlockBorder = 0.2;
        }

        /// <summary>
        ///     An membership decision border for verified dictors set
        /// </summary>
        public double VerifyBorder { get; set; }

        /// <summary>
        ///     An membership decision border for blocked dictors set
        /// </summary>
        public double BlockBorder { get; set; }

        /// <summary>
        ///     Get an result of speaker verification
        /// </summary>
        /// <param name="feature">Value of decision criteria</param>
        /// <returns>Solution</returns>
        public SolutionState MakeDecision(double feature)
        {
            var ownVal = BellFunction(feature);
            var foreignVal = SigmoidalFunction(feature);

            if ((ownVal > VerifyBorder) && (ownVal > foreignVal))
                return SolutionState.Verified;
            if ((foreignVal > BlockBorder) && (foreignVal > ownVal))
                return SolutionState.Blocked;
            return SolutionState.NoParticularDescision;
        }

        /// <summary>
        ///     Generalized bell function
        /// </summary>
        /// <param name="x">Input feature</param>
        /// <returns>Membership value</returns>
        private double BellFunction(double x)
        {
            return 1.0/(Math.Pow((x - BellFunctionSize)/BellFunctionCenter, 2.0*BellFunctionTilt) + 1.0);
        }

        /// <summary>
        ///     S-shaped function
        /// </summary>
        /// <param name="x">Input feature</param>
        /// <returns>Membership value</returns>
        private double SigmoidalFunction(double x)
        {
            return 0.5*(Math.Tanh((x - SigmoidalFunctionHalfPosition)/SigmoidalFunctionFullPosition) + 1);
        }
    }
}