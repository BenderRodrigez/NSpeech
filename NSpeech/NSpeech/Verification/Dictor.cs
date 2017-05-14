using System;
using System.Collections.Generic;
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
            Speech = speech.Clone();
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
                    GenerateVoiceKey((int) VoicePrintKeySize, UsedSpeechFeature, Metrics.Euclidian);
                return _key;
            }
            private set { _key = value; }
        }

        private void GenerateVoiceKey(int keySize, VoiceFeature featureSet, Metrics metric)
        {
            //here we use VQ to generate code book
            _key = new VoiceKey(keySize, metric);

            //get voice feature
            var feature = GetVoiceFeature(featureSet, Speech.Clone());

            _key.Generate(feature);
        }

        private double[][] GetVoiceFeature(VoiceFeature feature, Signal speech)
        {
            var voicedSpeech = new VoicedSeechFeature(speech, 0.04, 0.95);
            var marks = voicedSpeech.GetVoicedSpeechMarkers();
            var start = marks[0].Item1;
            var stop = marks[marks.Count - 1].Item2;
            switch (feature)
            {
                case VoiceFeature.Pitch:
                    var pitch = new Pitch(speech, marks);
                    return pitch.GetFeature().Samples.Select(x => new[] { x }).ToArray();
                case VoiceFeature.LinearPrediction:
                    return
                        speech.ExtractAnalysisInterval(start, stop - start)
                            .Split(0.04, 0.95, WindowFunctions.Blackman)
                            .Select(x => x.GetLinearPredictCoefficients(10))
                            .ToArray();
                case VoiceFeature.PitchAndLP:
                    var pitchTrack =
                        new Pitch(speech, marks).GetFeature()
                            .Samples.Select(x => new[] { x })
                            .ToArray();
                    
                    var lpc =
                        speech.ExtractAnalysisInterval(start, stop - start)
                            .Split(0.04, 0.95, WindowFunctions.Blackman)
                            .Select(x => x.GetLinearPredictCoefficients(10))
                            .ToArray();
                    return MixFeatures(lpc, pitchTrack, start, (int)Math.Round(0.05 * (0.04 * speech.SignalFormat.SampleRate)));
                default:
                    throw new ArgumentOutOfRangeException(nameof(feature), feature, null);
            }
        }

        private static double[][] MixFeatures(double[][] lpcFeature, double[][] pitchFeature, int speechStartPosition, int jumpSize)
        {
            var pitchSkiping = (int)Math.Round(speechStartPosition / (double)jumpSize);
            var newFeature = new List<double[]>();
            for (int i = 0; i - pitchSkiping < lpcFeature.Length || i < pitchFeature.Length; i++)
            {
                var feature = new List<double>();
                feature.AddRange(i - pitchSkiping < lpcFeature.Length && i - pitchSkiping > -1
                    ? lpcFeature[i - pitchSkiping]
                    : new double[lpcFeature[0].Length]);

                feature.AddRange(i < pitchFeature.Length ? pitchFeature[i] : new double[pitchFeature[0].Length]);

                newFeature.Add(feature.ToArray());
            }
            return newFeature.ToArray();
        }

        public SolutionState Verify(Signal speech)
        {
            var speechFeature = GetVoiceFeature(UsedSpeechFeature, speech.Clone());

            return Key.Verify(speechFeature);
        }
    }
}