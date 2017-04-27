using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSpeech;
using NSpeech.Verification;
using NSpeech.Verification.Solvers;

namespace NSpeechUnitTests
{
    [TestClass]
    public class VerficationTest
    {
        private const string BaseSamplesPath = "E:\\База образцов голоса\\Productive DB\\";
        private Dictionary<string, List<Dictor>> _speechRecords;

        [TestInitialize]
        public void Init()
        {
            _speechRecords = new Dictionary<string, List<Dictor>>();
            foreach (var phrase in Directory.GetDirectories(BaseSamplesPath))
            {
                foreach (var file in Directory.GetFiles(phrase))
                {
                    int sampleRate;
                    var signal = Helpers.ReadFile(file, out sampleRate);

                    var dictor = new Dictor(new FileInfo(file).Name.Substring(0, 3), new Signal(signal, sampleRate));
                    if (!_speechRecords.ContainsKey(dictor.Name))
                        _speechRecords.Add(dictor.Name, new List<Dictor>());
                    _speechRecords[dictor.Name].Add(dictor);
                }
            }
        }

        [TestMethod]
        public void RunTest()
        {
            var sameDictorFails = 0;
            var foriginDictorFails = 0;
            var success = 0;
            foreach (var trainDictor in _speechRecords.Values.SelectMany(x => x))
            {
                foreach (var testDictor in _speechRecords.Values.SelectMany(x => x))
                {
                    var result = trainDictor.Verify(testDictor.Speech);

                    if (trainDictor.Name == testDictor.Name)
                    {
                        if (result == SolutionState.Verified)
                            success++;
                        else
                        {
                            sameDictorFails++;
                        }
                    }
                    else
                    {
                        if (result == SolutionState.Blocked)
                            success++;
                        else
                        {
                            foriginDictorFails++;
                        }
                    }
                }
            }

            using (var writer = new StreamWriter(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "report.txt")))
            {
                var totalRecords = _speechRecords.Values.Sum(x => x.Count);
                writer.WriteLine("Same Dictor Fails: {0}", sameDictorFails);
                writer.WriteLine("Forigin Dictor Fails: {0}", foriginDictorFails);
                writer.WriteLine("Success: {0}", success);
                writer.WriteLine("Total experiments: {0}", totalRecords*(totalRecords-1));
            }
        }
    }
}
