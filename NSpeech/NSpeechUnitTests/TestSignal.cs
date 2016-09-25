using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;
using NSpeech;
using NSpeechUnitTests.Properties;

namespace NSpeechUnitTests
{
    [TestClass]
    public class TestSignal
    {
        private Signal _fixedSpectrumSignal;

        private float[] ReadFile(string fileName, out int sampleRate)
        {
            float[] file;
            using (var reader = new WaveFileReader(fileName))
            {
                var sampleProvider = reader.ToSampleProvider();
                file = new float[reader.SampleCount];
                sampleProvider.Read(file, 0, (int)reader.SampleCount);
                sampleRate = reader.WaveFormat.SampleRate;
            }
            return file;
        }

        private bool Equals(Signal a, Signal b)
        {
            var res = a.SignalFormat.SampleRate == b.SignalFormat.SampleRate;

            if (a.Samples.Length != b.Samples.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Samples.Length ; i++)
            {
                res = Math.Abs(a.Samples[i] - b.Samples[i]) < 0.0001;
            }
            return res;
        }

        private static double ExtractFrequency(int sectrumPosition, int sampleRate, int spectrumSize)
        {
            return sampleRate * sectrumPosition / (double)spectrumSize;
        }

        private List<int> GetExtremums(Signal signal)
        {
            var res = new List<int>();
            for (int i = 1; i < signal.Samples.Length -1; i++)
            {
                if(signal.Samples[i] < 0.002) continue;

                if(signal.Samples[i] > signal.Samples[i - 1] && signal.Samples[i] > signal.Samples[i + 1])
                    res.Add(i);
                else if(signal.Samples[i] < signal.Samples[i - 1] && signal.Samples[i] < signal.Samples[i + 1])
                    res.Add(i);
            }
            return res;
        }

        [TestInitialize]
        public void Init()
        {
            var fixedFreqFileName = Path.Combine(Environment.CurrentDirectory, "Files", Settings.Default.SpectrumTest);
            
            int sampleRate;
            _fixedSpectrumSignal = new Signal(ReadFile(fixedFreqFileName, out sampleRate), sampleRate);


        }

        [TestMethod]
        public void ApplyBandPassFiltrationTest()
        {
            var filteredSignal = _fixedSpectrumSignal.Normalize().ApplyBandPassFiltration(100, 3000);

            Assert.IsFalse(Equals(filteredSignal, _fixedSpectrumSignal), "Equals(filteredSignal, _fixedSpectrumSignal)");

            //should compare sectrums
            var initialSpectrum = _fixedSpectrumSignal.Normalize().GetSpectrum().Normalize();
            var modifiedSpectrum = filteredSignal.Normalize().GetSpectrum().Normalize();

            //Assert.IsFalse(Equals(initialSpectrum, modifiedSpectrum), "Equals(initialSpectrum, modifiedSpectrum)");

            var modifiedExtr = GetExtremums(modifiedSpectrum).Select(x => ExtractFrequency(x, _fixedSpectrumSignal.SignalFormat.SampleRate, 1024));
            var intitExtr = GetExtremums(initialSpectrum).Select(x => ExtractFrequency(x, _fixedSpectrumSignal.SignalFormat.SampleRate, 1024));

            Assert.IsFalse(modifiedExtr.Any(x => x > 100 && x < 3000), "modifiedExtr.Any(x => x > 100 && x < 3000)");
        }

        [TestMethod]
        public void ApplyCentralLimitationTest()
        {
            
        }

        [TestMethod]
        public void ApplyHighPassFiltrationTest()
        {
            
        }

        [TestMethod]
        public void ApplyLowPassFiltrationTest()
        {
            
        }

        [TestMethod]
        public void ApplyNoiseTest()
        {
            
        }

        [TestMethod]
        public void ApplyWindowFunctionTest()
        {
            
        }

        [TestMethod]
        public void ExtractAnalysisIntervalTest()
        {
            
        }

        [TestMethod]
        public void GetAutocorrelationTest()
        {
            
        }

        [TestMethod]
        public void GetCorrelationTest()
        {
            
        }

        [TestMethod]
        public void GetEnergyTest()
        {
            
        }

        [TestMethod]
        public void GetLinearPredictCoefficientsTest()
        {
            
        }

        [TestMethod]
        public void GetSpectrumTest()
        {
            var spectrum = _fixedSpectrumSignal.Normalize().GetSpectrum(1024);

            Assert.IsTrue(spectrum.Samples.Length == 1024);

            var extremums = GetExtremums(spectrum.Normalize());
            Assert.IsTrue(extremums.Count == 2);
            Assert.IsTrue(Math.Abs(ExtractFrequency(extremums[0], _fixedSpectrumSignal.SignalFormat.SampleRate, 1024) - 1500.0) < 10.0);
        }

        [TestMethod]
        public void PerformBackwardFurierTransformTest()
        {
            
        }

        [TestMethod]
        public void SplitTest()
        {
            
        }
    }
}
