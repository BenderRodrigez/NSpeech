using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSpeech;
using NSpeech.Verification;
using NSpeech.Verification.Solvers;
using NSpeechUnitTests.Properties;

namespace NSpeechUnitTests
{
    [TestClass]
    public class TestDictor
    {
        private Signal _anotherDictorSpeech;
        private Signal _sameDictorSpeech;
        private Signal _someSpeech;

        [TestInitialize]
        public void Init()
        {
            var sppechFileName = Path.Combine(Environment.CurrentDirectory, "Files", Settings.Default.Speech);
            var sameDictorSpeechFileName = Path.Combine(Environment.CurrentDirectory, "Files",
                Settings.Default.Speech_DifferentPhrase);
            var anotherDictorSpeechFileName = Path.Combine(Environment.CurrentDirectory, "Files",
                Settings.Default.Speech_DifferentDictor);
            int sampleRate;
            _someSpeech = new Signal(Helpers.ReadFile(sppechFileName, out sampleRate), sampleRate);
            _sameDictorSpeech = new Signal(Helpers.ReadFile(sameDictorSpeechFileName, out sampleRate), sampleRate);
            _anotherDictorSpeech = new Signal(Helpers.ReadFile(anotherDictorSpeechFileName, out sampleRate), sampleRate);
        }

        [TestMethod]
        public void DictorCreationTest()
        {
            var dictor = new Dictor("Test dictor", _someSpeech, VoiceFeature.LinearPrediction);

            Assert.IsNotNull(dictor.Name);
            Assert.AreEqual("Test dictor", dictor.Name);
            Assert.IsNotNull(dictor.Speech);
            Assert.IsNotNull(dictor.Key);
        }

        [TestMethod]
        public void VoiceKeyGenerationLinearPredictionTest()
        {
            var dictor = new Dictor("Test dictor", _someSpeech, VoiceFeature.LinearPrediction);

            Assert.IsNotNull(dictor.Key);
        }

        [TestMethod]
        public void VoiceKeyGenerationPitchTest()
        {
            var dictor = new Dictor("Test dictor", _someSpeech, VoiceFeature.Pitch);

            Assert.IsNotNull(dictor.Key);
        }

        [TestMethod]
        public void VoiceKeyGenerationLinearPredictionAndPicthTest()
        {
            var dictor = new Dictor("Test dictor", _someSpeech);

            Assert.IsNotNull(dictor.Key);
        }

        [TestMethod]
        public void VerificationLinearPredictionTest()
        {
            var dictor = new Dictor("Test dictor", _someSpeech, VoiceFeature.LinearPrediction);

            var sameDictor = new Dictor(dictor.Name, _sameDictorSpeech, VoiceFeature.LinearPrediction);

            var anotherDictor = new Dictor("Not the same dictor", _anotherDictorSpeech, VoiceFeature.LinearPrediction);

            Assert.AreEqual(SolutionState.Verified, dictor.Verify(sameDictor.Speech));
            Assert.AreEqual(SolutionState.Verified, sameDictor.Verify(dictor.Speech));

            Assert.AreEqual(SolutionState.Blocked, dictor.Verify(anotherDictor.Speech));
            Assert.AreEqual(SolutionState.Blocked, anotherDictor.Verify(sameDictor.Speech));
        }

        [TestMethod]
        public void VerificationPitchTest()
        {
            var dictor = new Dictor("Test dictor", _someSpeech, VoiceFeature.Pitch);

            var sameDictor = new Dictor(dictor.Name, _sameDictorSpeech, VoiceFeature.Pitch);

            var anotherDictor = new Dictor("Not the same dictor", _anotherDictorSpeech, VoiceFeature.Pitch);

            Assert.AreEqual(SolutionState.Verified, dictor.Verify(sameDictor.Speech));
            Assert.AreEqual(SolutionState.Verified, sameDictor.Verify(dictor.Speech));

            Assert.AreEqual(SolutionState.Blocked, dictor.Verify(anotherDictor.Speech));
            Assert.AreEqual(SolutionState.Blocked, anotherDictor.Verify(sameDictor.Speech));
        }

        [TestMethod]
        public void VerificationLinearPredictionAndPicthTest()
        {
            var dictor = new Dictor("Test dictor", _someSpeech);

            var sameDictor = new Dictor(dictor.Name, _sameDictorSpeech);

            var anotherDictor = new Dictor("Not the same dictor", _anotherDictorSpeech);

            Assert.AreEqual(SolutionState.Verified, dictor.Verify(sameDictor.Speech));
            Assert.AreEqual(SolutionState.Verified, sameDictor.Verify(dictor.Speech));

            Assert.AreEqual(SolutionState.Blocked, dictor.Verify(anotherDictor.Speech));
            Assert.AreEqual(SolutionState.Blocked, anotherDictor.Verify(sameDictor.Speech));
        }
    }
}