using NAudio.Wave;

namespace ExperimentalProcessing
{
    public class FileExporter
    {
        public int SampleRate { get; set; }
        public float[] Data { get; set; }

        public void SaveAsWav(string fileName)
        {
            using (var writer = new WaveFileWriter(fileName, WaveFormat.CreateIeeeFloatWaveFormat(SampleRate, 1)))
            {
                writer.WriteSamples(Data, 0, Data.Length);
            }
        }
    }
}
