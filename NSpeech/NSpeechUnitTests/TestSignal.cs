using System;
using System.IO;
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

        [TestInitialize]
        public void Init()
        {
            var fixedFreqFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Files", Settings.Default.SpectrumTest);

            int sampleRate;
            _fixedSpectrumSignal = new Signal(ReadFile(fixedFreqFileName, out sampleRate), sampleRate);


        }

        [TestMethod]
        public void ApplyBandPassFiltrationTest()
        {
            var filteredSignal = _fixedSpectrumSignal.ApplyBandPassFiltration(100, 3000);

            Assert.IsFalse(Equals(filteredSignal, _fixedSpectrumSignal), "Equals(filteredSignal, _fixedSpectrumSignal)");

            //should compare sectrums
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
