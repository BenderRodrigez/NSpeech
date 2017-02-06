using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSpeech;
using NSpeech.Verification;
using NSpeech.Verification.Clustering.Metrics;
using NSpeechUnitTests.Properties;

namespace NSpeechUnitTests
{
    [TestClass]
    public class TestDictor
    {
        private Signal _someSpeech;

        [TestInitialize]
        public void Init()
        {
            var sppechFileName = Path.Combine(Environment.CurrentDirectory, "Files", Settings.Default.Speech);
            int sampleRate;
            _someSpeech = new Signal(Helpers.ReadFile(sppechFileName, out sampleRate), sampleRate);
        }

        [TestMethod]
        public void DictorCreationTest()
        {
            var dictor = new Dictor();
            dictor.Name = "Test dictor";
            dictor.Speech = _someSpeech;
            
            Assert.IsNotNull(dictor.Name);
            Assert.AreEqual("Test dictor", dictor.Name);
            Assert.IsNotNull(dictor.Speech);
            Assert.IsNull(dictor.Key);
        }

        [TestMethod]
        public void VoiceKeyGenerationLinearPredictionTest()
        {
            var dictor = new Dictor();
            dictor.Name = "Test dictor";
            dictor.Speech = _someSpeech;

            dictor.GenerateVoiceKey(64, VoiceFeature.LinearPrediction, Metrics.Euclidian);
            Assert.IsNotNull(dictor.Key);
        }

        [TestMethod]
        public void VoiceKeyGenerationPitchTest()
        {
            var dictor = new Dictor();
            dictor.Name = "Test dictor";
            dictor.Speech = _someSpeech;

            dictor.GenerateVoiceKey(64, VoiceFeature.Pitch, Metrics.Euclidian);
            Assert.IsNotNull(dictor.Key);
        }

        [TestMethod]
        public void VoiceKeyGenerationLinearPredictionAndPicthTest()
        {
            var dictor = new Dictor();
            dictor.Name = "Test dictor";
            dictor.Speech = _someSpeech;

            dictor.GenerateVoiceKey(64, VoiceFeature.PitchAndLP, Metrics.Euclidian);
            Assert.IsNotNull(dictor.Key);
        }

    }
}
