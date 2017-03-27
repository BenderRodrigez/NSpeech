using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NAudio.Wave;
using NSpeech;

namespace NSpeechUnitTests
{
    internal static class Helpers
    {
        internal static double[] ReadFile(string fileName, out int sampleRate)
        {
            float[] file;
            using (var reader = new WaveFileReader(fileName))
            {
                var sampleProvider = reader.ToSampleProvider();
                file = new float[reader.SampleCount];
                sampleProvider.Read(file, 0, (int) reader.SampleCount);
                sampleRate = reader.WaveFormat.SampleRate;
            }
            return file.Select(x=> (double)x).ToArray();
        }

        internal static bool Equals(Signal a, Signal b)
        {
            var res = a.SignalFormat.SampleRate == b.SignalFormat.SampleRate;

            if (a.Samples.Length != b.Samples.Length)
                return false;

            for (var i = 0; i < a.Samples.Length; i++)
                res &= Math.Abs(a.Samples[i] - b.Samples[i]) < 0.0001;
            return res;
        }

        internal static bool Equals(float[] a, float[] b)
        {
            if (a.Length != b.Length)
                return false;

            var res = true;
            for (var i = 0; i < a.Length; i++)
                res &= Math.Abs(a[i] - b[i]) < 0.0001;
            return res;
        }

        internal static bool Equals(List<int> a, List<int> b)
        {
            if (a.Count != b.Count)
                return false;

            var res = false;
            for (var i = 0; i < a.Count; i++)
                res = Math.Abs(a[i] - b[i]) < 0.0001;
            return res;
        }

        internal static double ExtractFrequency(int sectrumPosition, int sampleRate, int spectrumSize)
        {
            return sampleRate*sectrumPosition/(double) spectrumSize;
        }

        internal static List<int> GetExtremums(double[] signal)
        {
            var res = new List<int>();
            for (var i = 1; i < signal.Length/2 - 1; i++)
            {
                if (signal[i] < 0.002) continue;

                if ((signal[i] > signal[i - 1]) && (signal[i] > signal[i + 1]))
                    res.Add(i);
                else if ((signal[i] < signal[i - 1]) && (signal[i] < signal[i + 1]))
                    res.Add(i);
            }
            return res;
        }

        internal static double[] DiffSignal(double[] a, double[] b)
        {
            var res = new double[a.Length];
            for (var i = 0; i < a.Length; i++)
                res[i] = a[i] - b[i];
            return res;
        }

        internal static void Dump(double[] data)
        {
            using (
                var writer =
                    new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                        "dump.txt")))
            {
                foreach (var f in data)
                    writer.WriteLine(f);
            }
        }
    }
}