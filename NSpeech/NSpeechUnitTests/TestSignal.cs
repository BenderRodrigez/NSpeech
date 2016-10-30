using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NAudio.Wave;
using NSpeech;
using NSpeech.DSPAlgorithms.WindowFunctions;
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
                res &= Math.Abs(a.Samples[i] - b.Samples[i]) < 0.0001;
            }
            return res;
        }

        private bool Equals(float[] a, float[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            var res = true;
            for (int i = 0; i < a.Length; i++)
            {
                res &= Math.Abs(a[i] - b[i]) < 0.0001;
            }
            return res;
        }

        private bool Equals(List<int> a, List<int> b)
        {
            if (a.Count != b.Count)
            {
                return false;
            }

            var res = false;
            for (int i = 0; i < a.Count; i++)
            {
                res = Math.Abs(a[i] - b[i]) < 0.0001;
            }
            return res;
        }

        private static double ExtractFrequency(int sectrumPosition, int sampleRate, int spectrumSize)
        {
            return sampleRate * sectrumPosition / (double)spectrumSize;
        }

        private List<int> GetExtremums(float[] signal)
        {
            var res = new List<int>();
            for (int i = 1; i < signal.Length/2 -1; i++)
            {
                if(signal[i] < 0.002) continue;

                if(signal[i] > signal[i - 1] && signal[i] > signal[i + 1])
                    res.Add(i);
                else if(signal[i] < signal[i - 1] && signal[i] < signal[i + 1])
                    res.Add(i);
            }
            return res;
        }

        private float[] DiffSignal(float[] a, float[] b)
        {
            var res = new float[a.Length];
            for (int i = 0; i < a.Length; i++)
            {
                res[i] = a[i] - b[i];
            }
            return res;
        }

        private void Dump(float[] data)
        {
            using (var writer = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "dump.txt")))
            {
                foreach (var f in data)
                {
                    writer.WriteLine(f);
                }
            }
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
            var initialSpectrum = _fixedSpectrumSignal.Normalize().GetSpectrum(1024).Normalize();
            var modifiedSpectrum = filteredSignal.Normalize().GetSpectrum(1024).Normalize();

            //Assert.IsFalse(Equals(initialSpectrum, modifiedSpectrum), "Equals(initialSpectrum, modifiedSpectrum)");

            var modifiedExtr = GetExtremums(modifiedSpectrum.Samples).Select(x => ExtractFrequency(x, _fixedSpectrumSignal.SignalFormat.SampleRate, 1024));
            var intitExtr = GetExtremums(initialSpectrum.Samples).Select(x => ExtractFrequency(x, _fixedSpectrumSignal.SignalFormat.SampleRate, 1024));

            Assert.IsFalse(modifiedExtr.Any(x => x > 100 && x < 3000), "modifiedExtr.Any(x => x > 100 && x < 3000)");
        }

        [TestMethod]
        public void ApplyCentralLimitationTest()
        {
            var limitedSignal = _fixedSpectrumSignal.Normalize().ApplyCentralLimitation(0.3);
            Assert.IsFalse(limitedSignal.Samples.All(x=> Math.Abs(x) >= 0.3 || Math.Abs(x) < 0.0001));

            limitedSignal = _fixedSpectrumSignal.Normalize().ApplyCentralLimitation(1.0);
            Assert.IsTrue(Equals(limitedSignal, _fixedSpectrumSignal.Normalize()), "Signal shouldn't be changed");

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

            Assert.IsFalse(Equals(filteredSignal, _fixedSpectrumSignal), "Equals(filteredSignal, _fixedSpectrumSignal)");

            //should compare sectrums
            var initialSpectrum = _fixedSpectrumSignal.Normalize().GetSpectrum(1024);
            var modifiedSpectrum = filteredSignal.GetSpectrum(1024);

            var diffSpectrum = DiffSignal(((Signal) initialSpectrum).Samples, ((Signal) modifiedSpectrum).Samples);

            Assert.IsFalse(Equals(GetExtremums(((Signal)initialSpectrum).Samples), GetExtremums(diffSpectrum)));

            //Test to pass higher frequencies
            filteredSignal = _fixedSpectrumSignal.Normalize().ApplyHighPassFiltration(100);

            Assert.IsFalse(Equals(filteredSignal, _fixedSpectrumSignal), "Equals(filteredSignal, _fixedSpectrumSignal)");

            modifiedSpectrum = filteredSignal.GetSpectrum(1024);

            Assert.IsTrue(Equals(initialSpectrum, modifiedSpectrum));
        }

        [TestMethod]
        public void ApplyLowPassFiltrationTest()
        {
            //Test to filter higher frequencies
            var filteredSignal = _fixedSpectrumSignal.Normalize().ApplyHighPassFiltration(100);

            Assert.IsFalse(Equals(filteredSignal, _fixedSpectrumSignal), "Equals(filteredSignal, _fixedSpectrumSignal)");

            //should compare sectrums
            var initialSpectrum = _fixedSpectrumSignal.Normalize().GetSpectrum(1024);
            var modifiedSpectrum = filteredSignal.GetSpectrum(1024);

            var diffSpectrum = DiffSignal(((Signal)initialSpectrum).Samples, ((Signal)modifiedSpectrum).Samples);

            Assert.IsFalse(Equals(GetExtremums(((Signal)initialSpectrum).Samples), GetExtremums(diffSpectrum)));

            //Test to pass lower frequencies
            filteredSignal = _fixedSpectrumSignal.Normalize().ApplyHighPassFiltration(3000);

            Assert.IsFalse(Equals(filteredSignal, _fixedSpectrumSignal), "Equals(filteredSignal, _fixedSpectrumSignal)");

            modifiedSpectrum = filteredSignal.GetSpectrum(1024);

            Assert.IsTrue(Equals(initialSpectrum, modifiedSpectrum));
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

            var extremums = GetExtremums(spectrum.Normalize().Samples);
            Assert.IsTrue(extremums.Count == 1);
            Assert.IsTrue(Math.Abs(ExtractFrequency(extremums[0], _fixedSpectrumSignal.SignalFormat.SampleRate, 1024) - 1500.0) < 10.0);
        }

        [TestMethod]
        public void PerformBackwardFurierTransformTest()
        {
            
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
