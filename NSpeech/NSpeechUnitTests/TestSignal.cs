using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSpeech;
using NSpeech.DSPAlgorithms.WindowFunctions;
using NSpeechUnitTests.Properties;

namespace NSpeechUnitTests
{
    [TestClass]
    public class TestSignal
    {
        private Signal _fixedSpectrumSignal;
        private Signal _immutableFixedSpectrumSignal;
        private Signal _silenceSignal;
        private Signal _immutableSilenceSignal;

        [TestInitialize]
        public void Init()
        {
            var fixedFreqFileName = Path.Combine(Environment.CurrentDirectory, "Files", Settings.Default.SpectrumTest);
            var silenceFileName = Path.Combine(Environment.CurrentDirectory, "Files", Settings.Default.Silence);
            
            int sampleRate;
            _fixedSpectrumSignal = new Signal(Helpers.ReadFile(fixedFreqFileName, out sampleRate), sampleRate);
            _immutableFixedSpectrumSignal = new Signal(Helpers.ReadFile(fixedFreqFileName, out sampleRate), sampleRate);
            _silenceSignal = new Signal(Helpers.ReadFile(silenceFileName, out sampleRate), sampleRate);
            _immutableSilenceSignal = new Signal(Helpers.ReadFile(silenceFileName, out sampleRate), sampleRate);

        }

        [TestMethod]
        public void ApplyBandPassFiltrationTest()
        {
            var filteredSignal = _fixedSpectrumSignal.Normalize().ApplyBandPassFiltration(100, 3000);

            Assert.IsFalse(Helpers.Equals(filteredSignal, _immutableFixedSpectrumSignal), "Equals(filteredSignal, _fixedSpectrumSignal)");

            //should compare sectrums
            var initialSpectrum = _fixedSpectrumSignal.Normalize().GetSpectrum(1024);
            var modifiedSpectrum = filteredSignal.GetSpectrum(1024);

            var diffSpectrum = Helpers.DiffSignal(((Signal)initialSpectrum).Samples, ((Signal)modifiedSpectrum).Samples);

            Assert.IsFalse(Helpers.Equals(Helpers.GetExtremums(((Signal)initialSpectrum).Samples), Helpers.GetExtremums(diffSpectrum)));
        }

        [TestMethod]
        public void ApplyCentralLimitationTest()
        {
            var limitedSignal = _fixedSpectrumSignal.Normalize().ApplyCentralLimitation(0.3);
            Assert.IsTrue(limitedSignal.Samples.All(x=> Math.Abs(x) >= 0.3 || Math.Abs(x) < 0.0001));

            limitedSignal = _fixedSpectrumSignal.Normalize().ApplyCentralLimitation(0.0);
            Assert.IsTrue(Helpers.Equals(limitedSignal, _fixedSpectrumSignal.Normalize()), "Signal shouldn't be changed");

            try
            {
                _fixedSpectrumSignal.Normalize().ApplyCentralLimitation(-0.1);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOfType(ex, typeof(ArgumentOutOfRangeException), "Incorrect parameter value accepted");
            }
        }

        [TestMethod]
        public void ApplyHighPassFiltrationTest()
        {
            //Test to filter lower frequencies
            var filteredSignal = _fixedSpectrumSignal.Normalize().ApplyHighPassFiltration(3000);

            Assert.IsFalse(Equals(filteredSignal, _immutableFixedSpectrumSignal), "Equals(filteredSignal, _fixedSpectrumSignal)");

            //should compare sectrums
            var initialSpectrum = _fixedSpectrumSignal.Normalize().GetSpectrum(1024);
            var modifiedSpectrum = filteredSignal.GetSpectrum(1024);

            var diffSpectrum = Helpers.DiffSignal(((Signal) initialSpectrum).Samples, ((Signal) modifiedSpectrum).Samples);

            Assert.IsFalse(Helpers.Equals(Helpers.GetExtremums(((Signal)initialSpectrum).Samples), Helpers.GetExtremums(diffSpectrum)));

            //Test to pass higher frequencies
            filteredSignal = _fixedSpectrumSignal.Normalize().ApplyHighPassFiltration(100);

            Assert.IsFalse(Helpers.Equals(filteredSignal, _immutableFixedSpectrumSignal), "Equals(filteredSignal, _fixedSpectrumSignal)");

            modifiedSpectrum = filteredSignal.GetSpectrum(1024);

            Assert.IsTrue(Helpers.Equals(initialSpectrum, modifiedSpectrum));
        }

        [TestMethod]
        public void ApplyLowPassFiltrationTest()
        {
            //Test to filter higher frequencies
            var filteredSignal = _fixedSpectrumSignal.Normalize().ApplyHighPassFiltration(100);

            Assert.IsFalse(Equals(filteredSignal, _immutableFixedSpectrumSignal), "Equals(filteredSignal, _fixedSpectrumSignal)");

            //should compare sectrums
            var initialSpectrum = _fixedSpectrumSignal.Normalize().GetSpectrum(1024);
            var modifiedSpectrum = filteredSignal.GetSpectrum(1024);

            var diffSpectrum = Helpers.DiffSignal(((Signal)initialSpectrum).Samples, ((Signal)modifiedSpectrum).Samples);

            Assert.IsFalse(Equals(Helpers.GetExtremums(((Signal)initialSpectrum).Samples), Helpers.GetExtremums(diffSpectrum)));

            //Test to pass lower frequencies
            filteredSignal = _fixedSpectrumSignal.Normalize().ApplyHighPassFiltration(3000);

            Assert.IsFalse(Helpers.Equals(filteredSignal, _immutableFixedSpectrumSignal), "Equals(filteredSignal, _fixedSpectrumSignal)");

            modifiedSpectrum = filteredSignal.GetSpectrum(1024);

            Assert.IsTrue(Helpers.Equals(initialSpectrum, modifiedSpectrum));
        }

        [TestMethod]
        public void ApplyNoiseTest()
        {
            double snr;
            var noisedSignal = _silenceSignal.ApplyNoise(0.1f, out snr);
            Assert.IsFalse(Helpers.Equals(noisedSignal, _immutableSilenceSignal));
            Assert.AreNotEqual(0.0, snr);
        }

        [TestMethod]
        public void ApplyWindowFunctionTest()
        {
            var windowedSignal = _fixedSpectrumSignal.ApplyWindowFunction(WindowFunctions.Rectangular);
            Assert.IsTrue(Helpers.Equals(windowedSignal, _immutableFixedSpectrumSignal), "Signals should be equals with and witout rectangular window function");

            windowedSignal = _fixedSpectrumSignal.Normalize().ApplyWindowFunction(WindowFunctions.Hamming);
            var normalizedSignal = _immutableFixedSpectrumSignal.Clone().Normalize();
            for (int i = 0; i < windowedSignal.Samples.Length; i++)
            {
                Assert.AreEqual(HammingWindow(normalizedSignal.Samples[i], i, normalizedSignal.Samples.Length), windowedSignal.Samples[i], 0.00001, "Unexpected data starting from "+i);
            }
        }

        private float HammingWindow(float x, int i, int length)
        {
            return (float)(x * (0.54 - 0.46 * Math.Cos(2.0 * Math.PI * i / length)));
        }

        [TestMethod]
        public void ExtractAnalysisIntervalTest()
        {
            var interval = _fixedSpectrumSignal.ExtractAnalysisInterval(0, 0);
            Assert.IsTrue(interval.Samples.Length == 0);

            interval = _fixedSpectrumSignal.ExtractAnalysisInterval(0, 100);
            Assert.IsTrue(interval.Samples.Length == 100);

            interval = _fixedSpectrumSignal.ExtractAnalysisInterval(100, 100);
            Assert.IsTrue(interval.Samples.Length == 100);

            interval = _fixedSpectrumSignal.ExtractAnalysisInterval(0, _fixedSpectrumSignal.Samples.Length + 1);
            Assert.IsTrue(interval.Samples.Length == _fixedSpectrumSignal.Samples.Length + 1);
        }

        [TestMethod]
        public void GetAutocorrelationTest()
        {
            PerformBackwardFurierTransformTest();

            var autocorrelation = _fixedSpectrumSignal.ExtractAnalysisInterval(0, 256).Normalize().GetAutocorrelation();

            foreach (var sample in autocorrelation.Samples)
            {
                Assert.IsFalse(float.IsNaN(sample) || float.IsInfinity(sample), "Samples can't be NaN or Infinity");
            }

            var firstExtremumPosition = -1;
            for (int i = 1; i < autocorrelation.Samples.Length-1; i++)
            {
                if (autocorrelation.Samples[i - 1] < autocorrelation.Samples[i]
                    && autocorrelation.Samples[i + 1] < autocorrelation.Samples[i])
                {
                    firstExtremumPosition = i;
                    break;
                }
            }

            Assert.AreNotEqual(-1, firstExtremumPosition);
            Assert.AreEqual(0.0006, firstExtremumPosition/(double)autocorrelation.SignalFormat.SampleRate, 0.0001);
        }

        [TestMethod]
        public void GetCorrelationTest()
        {
            var correlation = _fixedSpectrumSignal.GetCorrelation(0);
            Assert.AreEqual(1.0, correlation, 0.0001);

            correlation = _fixedSpectrumSignal.GetCorrelation();
            Assert.AreNotEqual(_fixedSpectrumSignal.GetEnergy(), correlation, 0.0001);
        }

        [TestMethod]
        public void GetEnergyTest()
        {
            var energy = _fixedSpectrumSignal.Normalize().GetEnergy();
            Assert.AreEqual(_fixedSpectrumSignal.Normalize().Samples.Sum(x=>Math.Pow(x, 2))/_fixedSpectrumSignal.Samples.Length, energy, 0.0001);
        }

        [TestMethod]
        public void GetLinearPredictCoefficientsTest()
        {
            var lpc = _fixedSpectrumSignal.ExtractAnalysisInterval(0, 512).GetLinearPredictCoefficients(10);
            Assert.AreEqual(10, lpc.Length);
            Assert.IsFalse(lpc.All(f => float.IsNaN(f) || float.IsInfinity(f)), "Values shuld never be NaN or infinity.");
        }

        [TestMethod]
        public void GetSpectrumTest()
        {
            var spectrum = _fixedSpectrumSignal.Normalize().GetSpectrum(1024);

            Assert.IsTrue(spectrum.Samples.Length == 1024);

            var extremums = Helpers.GetExtremums(spectrum.Normalize().Samples);
            Assert.IsTrue(extremums.Count == 1);
            Assert.IsTrue(Math.Abs(Helpers.ExtractFrequency(extremums[0], _fixedSpectrumSignal.SignalFormat.SampleRate, 1024) - 1500.0) < 10.0);
        }

        [TestMethod]
        public void PerformBackwardFurierTransformTest()
        {
            var spectrum = _fixedSpectrumSignal.Normalize().GetSpectrum(2048);
            var signal = spectrum.PerformBackwardFurierTransform(2048).Normalize();

            Assert.IsTrue(Helpers.Equals(_fixedSpectrumSignal.Normalize().ExtractAnalysisInterval(0, 2048), signal));
        }

        [TestMethod]
        public void SplitTest()
        {
            var splited = _fixedSpectrumSignal.Split(0.01, 0.0, WindowFunctions.Rectangular);
            var windowSizeInSamples = (int)(_fixedSpectrumSignal.SignalFormat.SampleRate * 0.01);
            var asumedNumberOfElements = (int)Math.Floor((double)_fixedSpectrumSignal.SignalFormat.SampleRate/windowSizeInSamples);

            Assert.IsTrue(splited.Length == asumedNumberOfElements);//assert what we have same number of elements as expected
            Assert.IsTrue(splited[0].Samples.Length == windowSizeInSamples);

            splited = _fixedSpectrumSignal.Split(0.01, 0.99, WindowFunctions.Rectangular);
            asumedNumberOfElements = (int)Math.Ceiling((double)(_fixedSpectrumSignal.SignalFormat.SampleRate - windowSizeInSamples) / (int)(windowSizeInSamples*0.01));

            Assert.IsTrue(splited.Length == asumedNumberOfElements);//assert what we have same number of elements as expected
            Assert.IsTrue(splited[0].Samples.Length == windowSizeInSamples);
        }
    }
}
