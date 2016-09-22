using System;
using System.Linq;
using NSpeech.DSPAlgorithms.SpeechFeatures;
using NSpeech.DSPAlgorithms.WindowFunctions;
using NSpeech.Solvers;
using NSpeech.Verification.Clustering.Metrics;

namespace NSpeech.Verification
{
    public class Dictor
    {
        public string Name { get; set; }

        public Signal Speech { get; set; }

        public VoiceKey Key { get; set; }

        public void GenerateVoiceKey(int keySize, VoiceFeature featureSet, Metrics metric)
        {
            //here we use VQ to generate code book
            Key = new VoiceKey(keySize, metric);

            //get voice feature
            var feature = GetVoiceFeature(featureSet, Speech);

            Key.Generate(feature);
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
                        speech.ExtractAnalysisInterval(bordersVoicedSpeech.Item1, bordersVoicedSpeech.Item2 - bordersVoicedSpeech.Item1)
                            .Split(0.04, 0.95, WindowFunctions.Blackman)
                            .Select(x => x.GetLinearPredictCoefficients(10))
                            .ToArray();
                    var fullLpc = new float[pitchTrack.Length][];
                    Array.Copy(lpc, bordersVoicedSpeech.Item1, fullLpc, 0, lpc.Length);
                    for (int i = 0; i < pitchTrack.Length; i++)
                    {
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

        public SolutionState Verify(Signal speech, VoiceFeature feature)
        {
            var speechFeature = GetVoiceFeature(feature, speech);

            return Key.Verify(speechFeature);
        }
    }
}
