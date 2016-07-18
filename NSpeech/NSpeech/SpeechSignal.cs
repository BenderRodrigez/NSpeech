using System;
using System.Collections.Generic;
using NSpeech.DSPAlgorithms.SpeechFeatures;

namespace NSpeech
{
    public class SpeechSignal:Signal
    {
        public SpeechSignal(float[] samples, int sampleRate) : base(samples, sampleRate)
        {
        }

        public Tuple<int, int> GetVoicedSpeechBorder(double windowSize = 0.04, double overlapping = 0.95, double border = 5.0)
        {
            var voicedSpechFeature = new VoicedSeechFeature(this, windowSize, overlapping);

            var feature = voicedSpechFeature.GetFeature();
            var start = -1;
            var stop = -1;
            for (int i = 0; i < feature.Samples.Length && feature.Samples.Length - i > -1; i++)
            {
                if (feature.Samples[i] > border)
                {
                    if (start <= -1)
                    {
                        start = i;
                    }
                    if (stop <= -1)
                    {
                        stop = feature.Samples.Length - i - 1;
                    }
                }
            }

            if (start == -1)
                start = 0;
            if (stop == -1)
                stop = feature.Samples.Length - 1;
            return new Tuple<int, int>(start, stop);
        }

        public List<Tuple<int, int>> GetVoicedSpeechMarkers(double windowSize = 0.04, double overlapping = 0.95, double border = 5.0)
        {
            var voicedSpechFeature = new VoicedSeechFeature(this, windowSize, overlapping);

            var feature = voicedSpechFeature.GetFeature();
            var marks = new List<Tuple<int,int>>();
            var start = -1;
            for (int i = 0; i < feature.Samples.Length; i++)
            {
                if (feature.Samples[i] > border)
                {
                    if (start > -1)
                    {
                        marks.Add(new Tuple<int, int>(start, i));
                        start = -1;
                    }
                    else
                    {
                        start = i;
                    }
                }
            }

            if (start > -1)
            {
                marks.Add(new Tuple<int, int>(start, feature.Samples.Length - 1));
            }
            return marks;
        }
    }
}
