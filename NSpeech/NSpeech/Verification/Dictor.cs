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
            Speech = speech;
            UsedSpeechFeature = speechFeature;
            VoicePrintKeySize = size;
        }

        public string Name { get; private set; }

        public Signal Speech { get; }

        public VoiceFeature UsedSpeechFeature { get; }

        public KeySize VoicePrintKeySize { get; }

#if DEBUG
        public double[][] VoiceFeatureArray
        {
            get
            {
                double[][] acf;
                double[][] acfs;
                var res = GetVoiceFeature(UsedSpeechFeature, Speech.Clone(), out acf, out acfs);
                AcfFeature = acf;
                AscfFeature = acfs;
                return res;
            }
        }

        public double[][] AcfFeature { get; private set; }
        public double[][] AscfFeature { get; private set; }
#endif

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
            var voicedSpeech = new VoicedSeechFeature(speech.Clone().Normalize(), 0.04, 0.95);
            switch (feature)
            {
                case VoiceFeature.Pitch:
                    var pitch = new Pitch(speech, voicedSpeech.GetVoicedSpeechMarkers());
                    return pitch.GetFeature().Samples.Select(x => new[] { x }).ToArray();
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
                            .Samples.Select(x => new[] { x })
                            .ToArray();

                    var bordersVoicedSpeech = voicedSpeech.GetVoicedSpeechBorder();
                    var lpc =
                        speech.ExtractAnalysisInterval(bordersVoicedSpeech.Item1,
                                bordersVoicedSpeech.Item2 - bordersVoicedSpeech.Item1)
                            .Split(0.04, 0.95, WindowFunctions.Blackman)
                            .Select(x => x.GetLinearPredictCoefficients(10))
                            .ToArray();
                    return MixFeatures(lpc, pitchTrack, bordersVoicedSpeech.Item1, (int)Math.Round(0.05 * (0.04 * speech.SignalFormat.SampleRate)));
                default:
                    throw new ArgumentOutOfRangeException(nameof(feature), feature, null);
            }
        }

        private double[][] GetVoiceFeature(VoiceFeature feature, Signal speech, out double[][] acf, out double[][] acfs)
        {
            var voicedSpeech = new VoicedSeechFeature(speech.Clone().Normalize(), 0.04, 0.95);
            switch (feature)
            {
                case VoiceFeature.Pitch:
                    var pitch = new Pitch(speech, voicedSpeech.GetVoicedSpeechMarkers());
                    var resPitch = pitch.GetFeature().Samples.Select(x => new[] {x}).ToArray();
                    acf = pitch.Acf;
                    acfs = pitch.Acfs;
                    return resPitch;
                case VoiceFeature.LinearPrediction:
                    var borders = voicedSpeech.GetVoicedSpeechBorder();
                    acf = null;
                    acfs = null;
                    return
                        speech.ExtractAnalysisInterval(borders.Item1, borders.Item2 - borders.Item1)
                            .Split(0.04, 0.95, WindowFunctions.Blackman)
                            .Select(x => x.GetLinearPredictCoefficients(10))
                            .ToArray();
                case VoiceFeature.PitchAndLP:
                    var pitch2 =
                        new Pitch(speech, voicedSpeech.GetVoicedSpeechMarkers());
                        var pitchTrack = pitch2.GetFeature()
                            .Samples.Select(x => new[] {x})
                            .ToArray();

                    var bordersVoicedSpeech = voicedSpeech.GetVoicedSpeechBorder();
                    var lpc =
                        speech.ExtractAnalysisInterval(bordersVoicedSpeech.Item1,
                                bordersVoicedSpeech.Item2 - bordersVoicedSpeech.Item1)
                            .Split(0.04, 0.95, WindowFunctions.Blackman)
                            .Select(x => x.GetLinearPredictCoefficients(10))
                            .ToArray();

                    acf = pitch2.Acf;
                    acfs = pitch2.Acfs;

                    return MixFeatures(lpc, pitchTrack, bordersVoicedSpeech.Item1, (int)Math.Round(0.05 * (0.04 * speech.SignalFormat.SampleRate)));
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

        private double[] Combine(double[] a, double[] b)
        {
            var res = new double[a.Length + b.Length];
            Array.Copy(a, res, a.Length);
            Array.Copy(b, 0, res, a.Length, b.Length);
            return res;
        }

        public SolutionState Verify(Signal speech)
        {
            var speechFeature = GetVoiceFeature(UsedSpeechFeature, speech.Clone());

            return Key.Verify(speechFeature);
        }
    }
}