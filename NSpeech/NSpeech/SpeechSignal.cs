using System;
using System.Collections.Generic;

namespace NSpeech
{
    class SpeechSignal:Signal
    {
        public SpeechSignal(float[] samples, int sampleRate) : base(samples, sampleRate)
        {
        }

        public List<Tuple<int, int>> GetVoicedSpeechIntervals()
        {
            return null;//TODO: implemet this
        }
    }
}
