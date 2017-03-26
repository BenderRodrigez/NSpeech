using System;
using System.Linq;
using NSpeech.DSPAlgorithms.Basic;

namespace NSpeech
{
    /// <summary>
    ///     Represent some signal with complex samples
    /// </summary>
    public class ComplexSignal : Signal
    {
        /// <summary>
        ///     Creates new complex signal from descreet signal with samples placed in reals
        /// </summary>
        /// <param name="samples">Real signal</param>
        /// <param name="sampleRate">Sample rate</param>
        public ComplexSignal(double[] samples, int sampleRate) : base(samples, sampleRate)
        {
            Samples = samples.Select(x => new Complex {Real = x, Imaginary = 0.0}).ToArray();
        }

        /// <summary>
        ///     Creates new instance of the complex signal
        /// </summary>
        /// <param name="samples">Signal's samples</param>
        /// <param name="sampleRate">Sample rate</param>
        public ComplexSignal(Complex[] samples, int sampleRate)
            : base(samples.Select(x =>  Math.Sqrt(x.ComlexSqr())).ToArray(), sampleRate)
        {
            Samples = samples;
        }

        /// <summary>
        ///     Creates new instance of the complex signal
        /// </summary>
        /// <param name="samples">Signal's samples</param>
        /// <param name="format">Signal's  format data</param>
        public ComplexSignal(Complex[] samples, Format format)
            : base(samples.Select(x =>  Math.Sqrt(x.ComlexSqr())).ToArray(), format)
        {
            Samples = samples;
        }

        /// <summary>
        ///     Signal's samples
        /// </summary>
        public new Complex[] Samples { get; }

        /// <summary>
        ///     Convert signal from frequency domain into time domain
        /// </summary>
        /// <param name="size">Furier transform size</param>
        /// <returns>Signal in time domain</returns>
        public new Signal PerformBackwardFurierTransform(int size = 1024)
        {
            return new Signal(FastFurierTransform.PerformBackwardTransform(Samples, size).Select(x => x).ToArray(),
                SignalFormat);
        }

        public Signal GetAplitudeSignal()
        {
            return new Signal(Samples.Select(x=> x.Real).ToArray(), SignalFormat);
        }
    }
}