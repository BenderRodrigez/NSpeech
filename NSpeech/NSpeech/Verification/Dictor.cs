using System;
using System.Linq;
using NSpeech.DSPAlgorithms.SpeechFeatures;
using NSpeech.DSPAlgorithms.WindowFunctions;
using NSpeech.Verification.Clustering.Metrics;
using NSpeech.Verification.Solvers;

namespace NSpeech.Verification
{
    public class Dictor
    {
        private VoiceKey _key;

        public Dictor(string name, Signal speech, VoiceFeature speechFeature = VoiceFeature.PitchAndLP, KeySize size = KeySize.Standart)
        {
            Name = name;
            Speech = speech;
            UsedSpeechFeature = speechFeature;
            VoicePrintKeySize = size;
        }

        public string Name { get; private set; }

        public Signal Speech { get; }

        public VoiceFeature UsedSpeechFeature { get; }

        public KeySize VoicePrintKeySize { get; }

        public VoiceKey Key
        {
            get
            {
                if (_key == null)
                    GenerateVoiceKey((int) VoicePrintKeySize, VoiceFeature.PitchAndLP, Metrics.Euclidian);
                return _key;
            }
            private set { _key = value; }
        }

        private void GenerateVoiceKey(int keySize, VoiceFeature featureSet, Metrics metric)
        {
            //here we use VQ to generate code book
            _key = new VoiceKey(keySize, metric);

            //get voice feature
            var feature = GetVoiceFeature(featureSet, Speech);

            _key.Generate(feature);
        }

        private float[][] GetVoiceFeature(VoiceFeature feature, Signal speech)
        {
            var voicedSpeech = new VoicedSeechFeature(speech, 0.04, 0.95);
            switch (feature)
            {
                case VoiceFeature.Pitch:
                    var pitch = new Pitch(speech, voicedSpeech.GetVoicedSpeechMarkers());
                    return pitch.GetFeature().Samples.Select(x => new[] {x}).ToArray();
                case VoiceFeature.LinearPrediction:
                    var borders = voicedSpeech.GetVoicedSpeechBorder();
                    return
                        speech.ExtractAnalysisInterval(borders.Item1, borders.Item2 - borders.Item1)
                            .Split(0.04, 0.95, WindowFunctions.Blackman)
                            .Select(x => x.GetLinearPredictCoefficients(10))
                            .ToArray();
                case VoiceFeature.PitchAndLP:
                    var pitchTrack =
                        new Pitch(speech, voicedSpeech.GetVoicedSpeechMarkers()).GetFeature()
                            .Samples.Select(x => new[] {x})
                            .ToArray();

                    var bordersVoicedSpeech = voicedSpeech.GetVoicedSpeechBorder();
                    var lpc =
                        speech.ExtractAnalysisInterval(bordersVoicedSpeech.Item1,
                                bordersVoicedSpeech.Item2 - bordersVoicedSpeech.Item1)
                            .Split(0.04, 0.95, WindowFunctions.Blackman)
                            .Select(x => x.GetLinearPredictCoefficients(10))
                            .ToArray();
                    var fullLpc = new float[pitchTrack.Length][];
                    Array.Copy(lpc, 0, fullLpc, bordersVoicedSpeech.Item1, lpc.Length);
                    for (var i = 0; i < pitchTrack.Length; i++)
                    {
                        if (fullLpc[i] == null) fullLpc[i] = new float[10];
                        fullLpc[i] = Combine(pitchTrack[i], fullLpc[i]);
                    }
                    return fullLpc;
                default:
                    throw new ArgumentOutOfRangeException(nameof(feature), feature, null);
            }
        }

        private float[] Combine(float[] a, float[] b)
        {
            var res = new float[a.Length + b.Length];
            Array.Copy(a, res, a.Length);
            Array.Copy(b, 0, res, a.Length, b.Length);
            return res;
        }

        public SolutionState Verify(Signal speech)
        {
            var speechFeature = GetVoiceFeature(UsedSpeechFeature, speech);

            return Key.Verify(speechFeature);
        }
    }
}