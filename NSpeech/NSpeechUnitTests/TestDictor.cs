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
        private Signal _someSpeech;
        private Signal _sameDictorSpeech;
        private Signal _anotherDictorSpeech;

        [TestInitialize]
        public void Init()
        {
            var sppechFileName = Path.Combine(Environment.CurrentDirectory, "Files", Settings.Default.Speech);
            var sameDictorSpeechFileName = Path.Combine(Environment.CurrentDirectory, "Files", Settings.Default.Speech_DifferentPhrase);
            var anotherDictorSpeechFileName = Path.Combine(Environment.CurrentDirectory, "Files", Settings.Default.Speech_DifferentDictor);
            int sampleRate;
            _someSpeech = new Signal(Helpers.ReadFile(sppechFileName, out sampleRate), sampleRate);
            _sameDictorSpeech = new Signal(Helpers.ReadFile(sameDictorSpeechFileName, out sampleRate), sampleRate);
            _anotherDictorSpeech = new Signal(Helpers.ReadFile(anotherDictorSpeechFileName, out sampleRate), sampleRate);
        }

        [TestMethod]
        public void DictorCreationTest()
        {
            var dictor = new Dictor("Test dictor", _someSpeech, VoiceFeature.LinearPrediction, KeySize.Standart);
            
            Assert.IsNotNull(dictor.Name);
            Assert.AreEqual("Test dictor", dictor.Name);
            Assert.IsNotNull(dictor.Speech);
            Assert.IsNotNull(dictor.Key);
        }

        [TestMethod]
        public void VoiceKeyGenerationLinearPredictionTest()
        {
            var dictor = new Dictor("Test dictor", _someSpeech, VoiceFeature.LinearPrediction, KeySize.Standart);

            Assert.IsNotNull(dictor.Key);
        }

        [TestMethod]
        public void VoiceKeyGenerationPitchTest()
        {
            var dictor = new Dictor("Test dictor", _someSpeech, VoiceFeature.Pitch, KeySize.Standart);

            Assert.IsNotNull(dictor.Key);
        }

        [TestMethod]
        public void VoiceKeyGenerationLinearPredictionAndPicthTest()
        {
            var dictor = new Dictor("Test dictor", _someSpeech, VoiceFeature.PitchAndLP, KeySize.Standart);

            Assert.IsNotNull(dictor.Key);
        }

        [TestMethod]
        public void VerificationLinearPredictionTest()
        {
            var dictor = new Dictor("Test dictor", _someSpeech, VoiceFeature.LinearPrediction, KeySize.Standart);

            var sameDictor = new Dictor(dictor.Name, _sameDictorSpeech, VoiceFeature.LinearPrediction, KeySize.Standart);

            var anotherDictor = new Dictor("Not the same dictor", _anotherDictorSpeech, VoiceFeature.LinearPrediction, KeySize.Standart);
            
            Assert.AreEqual(SolutionState.Verified, dictor.Verify(sameDictor.Speech));
            Assert.AreEqual(SolutionState.Verified, sameDictor.Verify(dictor.Speech));

            Assert.AreEqual(SolutionState.Blocked, dictor.Verify(anotherDictor.Speech));
            Assert.AreEqual(SolutionState.Blocked, anotherDictor.Verify(sameDictor.Speech));
        }

        [TestMethod]
        public void VerificationPitchTest()
        {
            var dictor = new Dictor("Test dictor", _someSpeech, VoiceFeature.Pitch, KeySize.Standart);

            var sameDictor = new Dictor(dictor.Name, _sameDictorSpeech, VoiceFeature.Pitch, KeySize.Standart);

            var anotherDictor = new Dictor("Not the same dictor", _anotherDictorSpeech, VoiceFeature.Pitch, KeySize.Standart);

            Assert.AreEqual(SolutionState.Verified, dictor.Verify(sameDictor.Speech));
            Assert.AreEqual(SolutionState.Verified, sameDictor.Verify(dictor.Speech));

            Assert.AreEqual(SolutionState.Blocked, dictor.Verify(anotherDictor.Speech));
            Assert.AreEqual(SolutionState.Blocked, anotherDictor.Verify(sameDictor.Speech));
        }

        [TestMethod]
        public void VerificationLinearPredictionAndPicthTest()
        {
            var dictor = new Dictor("Test dictor", _someSpeech, VoiceFeature.PitchAndLP, KeySize.Standart);

            var sameDictor = new Dictor(dictor.Name, _sameDictorSpeech, VoiceFeature.PitchAndLP, KeySize.Standart);

            var anotherDictor = new Dictor("Not the same dictor", _anotherDictorSpeech, VoiceFeature.PitchAndLP, KeySize.Standart);

            Assert.AreEqual(SolutionState.Verified, dictor.Verify(sameDictor.Speech));
            Assert.AreEqual(SolutionState.Verified, sameDictor.Verify(dictor.Speech));

            Assert.AreEqual(SolutionState.Blocked, dictor.Verify(anotherDictor.Speech));
            Assert.AreEqual(SolutionState.Blocked, anotherDictor.Verify(sameDictor.Speech));
        }
    }
}
