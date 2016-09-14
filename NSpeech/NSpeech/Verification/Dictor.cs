using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NSpeech.Solvers;

namespace NSpeech.Verification
{
    class Dictor
    {
        public string Name { get; set; }

        public Signal Speech { get; set; }

        public VoiceKey Key { get; set; }

        public void GenerateVoiceKey(int keySize, VoiceFeature featureSet)
        {
            //here we use VQ to generate code book
        }

        public SolutionState Verify(Signal speech)
        {
            //here we use the our key to compare two dictors voices
            return SolutionState.NoParticularDescision;
        }
    }
}
