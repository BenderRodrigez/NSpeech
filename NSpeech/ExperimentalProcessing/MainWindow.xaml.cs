﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using ExperimentalProcessing.Annotations;
using Microsoft.Win32;
using NSpeech;
using NSpeech.Verification;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.Wpf;
using HeatMapSeries = OxyPlot.Series.HeatMapSeries;
using LinearAxis = OxyPlot.Axes.LinearAxis;
using LinearColorAxis = OxyPlot.Axes.LinearColorAxis;
using LineSeries = OxyPlot.Series.LineSeries;

namespace ExperimentalProcessing
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : INotifyPropertyChanged
    {
        private bool _useHz;
        public PlotModel PitchPlotModel { get; private set; }
        public PlotModel AcfPlotModel { get; private set; }
        public PlotModel AcfsPlotModel { get; private set; }
        public PlotModel AcfPreview { get; private set; }
        public PlotModel AcfsPreview { get; private set; }
        public Cursor WindowCursor { get; private set; }
        public string MaxSize { get; private set; }
        public string FileName { get; private set; }

        public bool UseNoise { get; set; }

        public double NoiseAmplitude
        {
            get { return _noiseAmplitude; }
            set { _noiseAmplitude = value; }
        }

        public string SignalNoiseRaito { get; private set; }

        private Tuple<int, int> _maxEnergyInterval = new Tuple<int, int>(0,0);

        public string MaxEnergyInterval
        {
            get { return _maxEnergyInterval.Item1 + "-" + _maxEnergyInterval.Item2; }
            set
            {
                var vals = value.Split('-');
                int start;
                int stop;
                if (vals.Length == 2 && int.TryParse(vals[0], out start) && int.TryParse(vals[1], out stop))
                {
                    _maxEnergyInterval = new Tuple<int, int>(start, stop);
                }
            }
        }

        public float HighPassFilterBorder { get; set; }

        public bool SimulatePhoneCnanel { get; set; }

        public float LowPassFilterBorder { get; set; }

        public double EnergyLineBorder
        {
            get; set; }

        public int FilterDiameter
        {
            get; set;
        }

        public double CentralLimit
        {
            get; set; }

        public double MaxFreqJumps
        {
            get; set; }

        public float AdditveNoiseLevel
        {
            get; set; }

        public double TonalSpeechSelectorBorder
        {
            get; set; }

        public double MinimalVoicedSpeechLength
        {
            get; set; }


        public bool UseHz
        {
            get
            {
                return _useHz;
            }
            set
            {
                _useHz = value;
                if(_pitch != null) PlotPitch(_pitch);
            }
        }

        private int _samplePos;
        public int SamplePosition
        {
            get { return _samplePos*_jump; }
            set
            {
                if (_acf != null && _acfs != null && value > -1 && value < _acf.Length*_jump)
                {
                    _samplePos = (int)Math.Round(value/(double)_jump);
                    PlotAcfSample(_samplePos);
                    PlotAcfsSample(_samplePos);
                    OnPropertyChanged();
                }
            }
        }

        double[][] _acf;
        double[][] _acfs;
        double[][] _pitch;
        double _sampleFreq;
        int _jump = 22;
        int _windowSize = 441;
        private Signal _inputFile;
        private double _noiseAmplitude = 0.02;
        private Result _results;

        public MainWindow()
        {
            _useHz = true;
            InitializeComponent();

            PitchPlotModel = new PlotModel { Title = "Трек ОТ", TitleFontSize = 10.0};
            PitchPlotModel.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1));
            OnPropertyChanged("PitchPlotModel");

            AcfPlotModel = new PlotModel { Title = "АКФ", TitleFontSize = 10.0 };
            AcfPlotModel.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1));
            OnPropertyChanged("AcfPlotModel");

            AcfsPlotModel = new PlotModel { Title = "АКФС", TitleFontSize = 10.0 };
            AcfsPlotModel.Series.Add(new FunctionSeries(Math.Cos, 0, 10, 0.1));
            OnPropertyChanged("AcfsPlotModel");

            AcfPreview = new PlotModel {Title = "Коррелограмма АКФ", TitleFontSize = 10.0 };
            var linearColorAxis = new LinearColorAxis
            {
                HighColor = OxyColors.White,
                LowColor = OxyColors.Black,
                Position = AxisPosition.Right,
                Palette = OxyPalettes.Hot(200),
            };
            AcfPreview.Axes.Add(linearColorAxis);
            AcfPreview.Series.Add(new FunctionSeries(Math.Cosh, 0, 10, 0.1));
            OnPropertyChanged("AcfPreview");
            AcfsPreview = new PlotModel { Title = "Коррелограмма АКФС", TitleFontSize = 10.0 };
            var linearColorAxis1 = new LinearColorAxis
            {
                HighColor = OxyColors.White,
                LowColor = OxyColors.Black,
                Position = AxisPosition.Right,
                Palette = OxyPalettes.Hot(200),
            };
            AcfsPreview.Axes.Add(linearColorAxis1);
            AcfsPreview.Series.Add(new FunctionSeries(Math.Cosh, 0, 10, 0.1));
            OnPropertyChanged("AcfsPreview");

            if (PitchPlotView.TrackerDefinitions.Count == 0)
            {
                PitchPlotView.TrackerDefinitions.Add(new TrackerDefinition { TrackerKey = "signal", TrackerTemplate = null });
                PitchPlotView.TrackerDefinitions.Add(new TrackerDefinition { TrackerKey = "", TrackerTemplate = PitchPlotView.DefaultTrackerTemplate });
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        internal void OpenFileButton()
        {
            var openFileDlg = new OpenFileDialog();
            openFileDlg.FileOk += OpenFileDlgOnFileOk;
            openFileDlg.ShowDialog(this);
        }

        internal void TestAlogorithm()
        {
            var openFileDlg = new OpenFileDialog();
            openFileDlg.FileOk += (sender, a) =>
            {
                var fileName = ((OpenFileDialog)sender).FileName;
                var fileInfo = new FileInfo(fileName);
                FileName = fileName;
                var args = new string[2];
                if (fileInfo.Extension.ToLower() == ".lst")
                {
                    args[0] = fileName.ToLower().Replace(".lst", ".dat");
                    args[1] = fileName;
                }
                else
                {
                    args[0] = fileName;
                    args[1] = fileName.ToLower().Replace(".dat", ".lst");
                }

                var task = new Task(OpenEtalonFile, args);
                task.Start();
                OnPropertyChanged("FileName");
            };
            openFileDlg.ShowDialog(this);
        }

        public void GenerateReport()
        {
            Result.MakeReport();
        }

        private void OpenEtalonFile(object fileName)
        {
            WindowCursor = Cursors.Wait;
            OnPropertyChanged("WindowCursor");
            var args = (string[]) fileName;
            _results = new Result();
            var expDataReader = new ExperimentalDataParser(args[0],args[1]);
            var samples = new float[expDataReader.SignalData.Length];
            Array.Copy(expDataReader.SignalData, samples, expDataReader.SignalData.Length);
            _sampleFreq = expDataReader.SampleRate;
            _inputFile = new Signal(samples.Select(x=> (double)x).ToArray(), expDataReader.SampleRate);

            _inputFile = _inputFile.Normalize();

            if (SimulatePhoneCnanel)
            {
                _inputFile.ApplyBandPassFiltration(300.0f, 3400.0f);
            }
            _results.IsPhoneChanel = SimulatePhoneCnanel;
            if (UseNoise)
            {
                double snr;
                _inputFile.ApplyNoise((float) NoiseAmplitude, out snr);
                SignalNoiseRaito = "ОСШ = " + snr.ToString("##0.#")+"дБ";
                _results.SignalToNoiseRaito = snr;
                OnPropertyChanged("SignalNoiseRaito");
            }
            _results.IsNoised = UseNoise;

//            var trainDataAcf = new Dictor("", _inputFile, VoiceFeature.Pitch).VoiceFeatureArray;
//            SamplePosition = 0;
//            _pitch = trainDataAcf;
//            var distortion = CalcDistortion(trainDataAcf, expDataReader.PitchTrajectory);
//            var results = string.Empty;
//            if (distortion.Length > 0)
//            {
//                var unimportantErrors = 100.0;
//                var smallErrors = 100.0;
//                var bigErrors = 100.0;
//                var voicedSpeechDetectionErrors = 100.0;
//
//                if (distortion.Any(x => (x >= 0.0 && x < 1.0)))
//                {
//                    var bothTonalIntervals = distortion.Where(x => (x >= 0.0 && x < 1.0)).Count();
//
//                    if (distortion.Any(x => x >= 0.0 && x <= 0.05))
//                    {
//                        unimportantErrors = distortion.Where(x => x >= 0.0 && x <= 0.05).Count()*100.0/bothTonalIntervals;
//                    }
//
//                    if (distortion.Any(x => x > 0.05 && x <= 0.15))
//                    {
//                        smallErrors = distortion.Where(x => x > 0.05 && x <= 0.15).Count()*100.0/
//                                                   bothTonalIntervals;
//                    }
//
//                    if (distortion.Any(x => x > 0.15 && x < 1.0))
//                    {
//                        bigErrors = distortion.Where(x => x > 0.15 && x < 1.0).Count()*100.0/bothTonalIntervals;
//                    }
//
//                    if (distortion.Any(x => x >= 1.0 && x < 2.0))
//                    {
//                        voicedSpeechDetectionErrors = distortion.Where(x => x >= 1.0 && x < 2.0).Count()*100.0/distortion.Length;
//                    }
//                }
//
//                _results.VoicedSpeechDetectorErrorRate = voicedSpeechDetectionErrors;
//                _results.SmallErrorsRate = smallErrors;
//                _results.BigErrorsRate = bigErrors;
//
//                results = string.Format(
//                    "{5}\r\n\r\nНесущественных ошибок: {0:#0.###}%\r\nМалых ошибок: {1:#0.###}%\r\nБольших ошибок: {2:#0.###}%\r\nСреднее: {3:#0.###}\r\nКоличество ошибка выделителя тональных участков: {4:#0.###}%\r\n",
//                    unimportantErrors, //unimpotant errors
//                    smallErrors, //small errors
//                    bigErrors, //big errors
//                    double.NaN, //total average
//                    voicedSpeechDetectionErrors,//voiced speech selecting error
//                    args[0]); //filename
//            }
//            Dispatcher.InvokeAsync(() =>
//            {
//                var resultsWindow = new ResultsWindow
//                {
//                    Results = results,
//                    Owner = this
//                };
//                resultsWindow.Show();
//            });
//            _results.FileName = FileName;
//            _results.IsNoised = UseNoise;
//            _results.IsPhoneChanel = SimulatePhoneCnanel;
//            _results.PitchData = _pitch.Select(x => x[0] > 0 ? _sampleFreq / x[0] : 0.0).ToArray();
//            _results.DistortionData = distortion;
//            _results.EthalonPitchData = expDataReader.PitchTrajectory;
//            _results.SampleRate = (int)_sampleFreq;
//            _results.DictorName = expDataReader.DictorName;
//            _results.Phrase = expDataReader.Phrase;
//            _results.SignalData = _inputFile.Samples;
//
//            PlotPitch(trainDataAcf, expDataReader.PitchTrajectory, distortion);
//            PlotAcfPreview();
//            PlotAcfsPreview();
            WindowCursor = Cursors.Arrow;
            OnPropertyChanged("WindowCursor");
        }

        public void SaveResults()
        {
            if (_results != null) _results.SaveToDb();
        }

        private double[] CalcDistortion(double[][] pitch, double[] etalon)
        {
            var distortion = new List<double>();
            for (int i = 0; i * _jump + _jump * 3 / 4 < etalon.Length; i++)
            {
                var etalon1 = etalon[i * _jump + _jump / 4];
                var etalon2 = etalon[i * _jump + _jump * 3 / 4];
                if (i >= pitch.Length)
                {
                    distortion.Add(etalon1 > 0.0 ? 1.0 : 2.0);
                    distortion.Add(etalon2 > 0.0 ? 1.0 : 2.0);
                    continue;
                }

                var bothAreTonal1 = etalon1 > 0.0 && pitch[i][0] > 0.0;
                var bothAreTonal2 = etalon2 > 0.0 && pitch[i][0] > 0.0;
                var lessOneIsTonal1 = etalon1 > 0.0 || pitch[i][0] > 0.0;
                var lessOneIsTonal2 = etalon2 > 0.0 || pitch[i][0] > 0.0;

                if (bothAreTonal1)
                {
                    distortion.Add(Math.Abs((pitch[i][0] > 0.0 ? _sampleFreq/pitch[i][0] : 0.0) - etalon1)/etalon1);
                }
                else if (lessOneIsTonal1)
                {
                    distortion.Add(1.0);
                }
                else
                {
                    distortion.Add(2.0);
                }

                if (bothAreTonal2)
                {
                    distortion.Add(Math.Abs((pitch[i][0] > 0.0 ? _sampleFreq / pitch[i][0] : 0.0) - etalon2) / etalon2);
                }
                else if (lessOneIsTonal2)
                {
                    distortion.Add(1.0);
                }
                else
                {
                    distortion.Add(2.0);
                }
            }
            return distortion.ToArray();
        }

        private void OpenFileDlgOnFileOk(object sender, CancelEventArgs cancelEventArgs)
        {
            var fileName = ((OpenFileDialog) sender).FileName;
            FileName = fileName;
            var task = new Task(OpenFile, fileName);
            task.Start();
            OnPropertyChanged("FileName");
        }

        private void OpenFile(object fileName)
        {
            WindowCursor = Cursors.Wait;
            OnPropertyChanged("WindowCursor");
            int signalFormat;
            var inputFile = FileReader.ReadFileNormalized(fileName.ToString(), out signalFormat);
            _inputFile = new Signal(inputFile.Select(x=> (double)x).ToArray(), signalFormat);
            _sampleFreq = signalFormat;

            if (SimulatePhoneCnanel)
            {
                _inputFile.ApplyBandPassFiltration(300.0f, 3400.0f);
            }
            if (UseNoise)
            {
//                if (_inputFile.Samples.Length > _maxEnergyInterval.Item2)
//                {
//                    var noise = new NoiseGenerator(_inputFile, _inputFile.Length, NoiseAmplitude, _maxEnergyInterval);
//                    SignalNoiseRaito = "ОСШ = " + noise.SNR.ToString("##0.#") + "дБ";
//                    OnPropertyChanged("SignalNoiseRaito");
//                    _inputFile = noise.ApplyNoise(_inputFile);
//                }
            }

            var dictor = new Dictor("", _inputFile, VoiceFeature.Pitch);
//            var trainDataAcf = dictor.VoiceFeatureArray;
//            _acf = dictor.AcfFeature;
//            _acfs = dictor.AscfFeature;
            SamplePosition = 0;
//            _pitch = trainDataAcf;
//            PlotPitch(trainDataAcf);
            PlotAcfPreview();
            PlotAcfsPreview();
            WindowCursor = Cursors.Arrow;
            OnPropertyChanged("WindowCursor");
        }

        private void PlotAcfsPreview()
        {
            if (_acfs != null && _acfs.Length > 0)
            {
                var heatMap = new HeatMapSeries
                {
                    Data = new double[_acfs.Length, _acfs[0].Length],
                    X0 = 0,
                    X1 = _acfs.Length*_jump,
                    Y0 = 0,
                    Y1 = _acfs[0].Length*(_sampleFreq/Math.Pow(2, Math.Ceiling(Math.Log(_windowSize, 2) + 1))),
                    Interpolate = false
                };
                for (int i = 0; i < _acfs.Length; i++)
                    for (int j = 0; j < _acfs[i].Length; j++)
                    {
                        heatMap.Data[i, j] = _acfs[i][j];
                    }
                AcfsPreview.Series.Clear();
                AcfsPreview.Series.Add(heatMap);
                AcfsPreview.InvalidatePlot(true);
                OnPropertyChanged("AcfsPreview");
            }
        }

        private void PlotAcfPreview()
        {
            if (_acf != null && _acf.Length > 0)
            {
                var heatMap = new HeatMapSeries
                {
                    Data = new double[_acf.Length, _acf[0].Length],
                    X0 = 0,
                    X1 = _acf.Length*_jump,
                    Y0 = 0,
                    Y1 = _acf[0].Length/_sampleFreq,
                    Interpolate = false
                };
                for (int i = 0; i < _acf.Length; i++)
                    for (int j = 0; j < _acf[i].Length; j++)
                    {
                        heatMap.Data[i, j] = _acf[i][j];
                    }
                AcfPreview.Series.Clear();
                AcfPreview.Series.Add(heatMap);
                AcfPreview.InvalidatePlot(true);
                OnPropertyChanged("AcfPreview");
            }
        }

//        private double[][] GetAcfImage(float[] speechFile, int sampleRate, Tuple<int, int>[] speechMarks,
//            out double[][] acf, out double[][] acfs)
//        {
//            double[][] trainDataAcf;
//            var windowSize = (int)Math.Round(sampleRate * 0.04);
//            acf = _corellation.Acf;
//            acfs = _corellation.Acfs;
//            _jump = (int)Math.Round(windowSize * 0.05f);
//            _windowSize = windowSize;
//            MaxSize = " из " + (speechFile.Length - 1);
//            OnPropertyChanged("MaxSize");
//            return trainDataAcf;
//        }

        private void PlotAcfSample(int pos)
        {
            var heatMap = new LineSeries();
            for (int i = 0; i < _acf[pos].Length; i++)
                heatMap.Points.Add(new DataPoint(i/_sampleFreq, _acf[pos][i]));
            heatMap.Color = OxyColors.Green;
            AcfPlotModel.Series.Clear();
            AcfPlotModel.Series.Add(heatMap);
            AcfPlotModel.InvalidatePlot(true);
        }

        private void PlotAcfsSample(int pos)
        {
            var heatMap = new LineSeries();
            for (int i = 0; i < _acfs[pos].Length; i++)
                heatMap.Points.Add(new DataPoint(i*(_sampleFreq/ Math.Pow(2, Math.Ceiling(Math.Log(_windowSize, 2) + 1))), _acfs[pos][i]));
            AcfsPlotModel.Series.Clear();
            AcfsPlotModel.Series.Add(heatMap);
            AcfsPlotModel.InvalidatePlot(true);
        }

        private void PlotPitch(double[][] featureSet)
        {
            var lineSeries = new LineSeries();
            for (int i = 0; i < featureSet.Length; i++)
            {
                lineSeries.Points.Add(new DataPoint(i*_jump, featureSet[i][0] > 0.0?featureSet[i][0]:0.0));
            }

            var signal = new LineSeries {Color = OxyColors.Aqua, Selectable = false, TrackerKey = "signal"};
            var signalYAxes = new LinearAxis {Key = "signalY", Position = AxisPosition.Right};
            signal.YAxisKey = "signalY";
            for (int i = 0; i < _inputFile.Samples.Length; i++)
            {
                signal.Points.Add(new DataPoint(i, _inputFile.Samples[i]));
            }

            if(PitchPlotModel.Axes.FirstOrDefault(x=> x.Key == "signalY") == null)
                PitchPlotModel.Axes.Add(signalYAxes);
            PitchPlotModel.Series.Clear();
            PitchPlotModel.Series.Add(signal);
            PitchPlotModel.Series.Add(lineSeries);
            PitchPlotModel.InvalidatePlot(true);
        }

        private void PlotPitch(double[][] featureSet, double[] etalonPitch, double[] distortion)
        {
            var lineSeries = new LineSeries();
            for (int i = 0; i < featureSet.Length; i++)
            {
                lineSeries.Points.Add(_useHz
                    ? new DataPoint(i * _jump, featureSet[i][0] > 0.0 ? _sampleFreq / featureSet[i][0] : 0.0)
                    : new DataPoint(i * _jump, featureSet[i][0] / _sampleFreq));
            }

            var etlonSeries = new LineSeries{TrackerKey = "etalon"};
            for (int i = 0; i < etalonPitch.Length; i++)
            {
                etlonSeries.Points.Add(new DataPoint(i, etalonPitch[i]));
            }

            var signal = new LineSeries { Color = OxyColors.Aqua, Selectable = false, TrackerKey = "signal" };
            var signalYAxes = new LinearAxis { Key = "signalY", Position = AxisPosition.Right };
            signal.YAxisKey = "signalY";
            for (int i = 0; i < _inputFile.Samples.Length; i++)
            {
                signal.Points.Add(new DataPoint(i, _inputFile.Samples[i]));
            }

            var distortionSeries = new LineSeries { Color = OxyColors.Red, Selectable = false, TrackerKey = "distortion" };
            var distortionYAxes = new LinearAxis { Key = "distortionY", Position = AxisPosition.None };
            distortionSeries.YAxisKey = "distortionY";
            for (int i = 0; i < distortion.Length; i++)
            {
                distortionSeries.Points.Add(new DataPoint(i*_jump/2+_jump/4, distortion[i]));
            }


            if (PitchPlotModel.Axes.FirstOrDefault(x => x.Key == "signalY") == null)
                PitchPlotModel.Axes.Add(signalYAxes);
            if (PitchPlotModel.Axes.FirstOrDefault(x => x.Key == "distortionY") == null)
                PitchPlotModel.Axes.Add(distortionYAxes);

            PitchPlotModel.Series.Clear();
            PitchPlotModel.Series.Add(signal);
            PitchPlotModel.Series.Add(lineSeries);
            PitchPlotModel.Series.Add(etlonSeries);
            PitchPlotModel.Series.Add(distortionSeries);
            PitchPlotModel.InvalidatePlot(true);
        }

        private void SampleNumberTextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Return:
                    int t;
                    if (int.TryParse(SampleNumberTextBox.Text, out t))
                        SamplePosition = t;
                    e.Handled = true;
                    break;
                case Key.Left:
                    SamplePosition-=_jump;
                    e.Handled = true;
                    break;
                case Key.Right:
                    SamplePosition+=_jump;
                    e.Handled = true;
                    break;
            }
        }

        private void GoForward_OnClick(object sender, RoutedEventArgs e)
        {
            if (SamplePosition < (_acf.Length - 1)*_jump) SamplePosition+=_jump;
        }

        private void GoBackward_OnClick(object sender, RoutedEventArgs e)
        {
            if (SamplePosition > 0) SamplePosition-=_jump;
        }

        private void ToEnd_OnClick(object sender, RoutedEventArgs e)
        {
            if(_acf != null)
                SamplePosition = (_acf.Length - 1) * _jump;
        }

        private void ToStart_OnClick(object sender, RoutedEventArgs e)
        {
            if (_acf != null)
                SamplePosition = 0;
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                SamplePosition+=_jump;
                e.Handled = true;
            }
            else if (e.Key == Key.Left)
            {
                SamplePosition-=_jump;
                e.Handled = true;
            }
        }

        internal void ResetButton()
        {
            AcfsPlotView.ResetAllAxes();
            AcfsSamplePlotView.ResetAllAxes();
            AcfPlotView.ResetAllAxes();
            AcfsSamplePlotView.ResetAllAxes();
            PitchPlotView.ResetAllAxes();
        }

        internal void RenewCalculations()
        {
            var args = new string[2];
            var fileInfo = new FileInfo(FileName);
            if (fileInfo.Extension.ToLower() == ".lst")
            {
                args[0] = FileName.ToLower().Replace(".lst", ".dat");
                args[1] = FileName;
            }
            else
            {
                args[0] = FileName;
                args[1] = FileName.ToLower().Replace(".dat", ".lst");
            }
            var task = (FileName.IndexOf(".wav", StringComparison.InvariantCultureIgnoreCase) > -1)
                ? new Task(OpenFile, FileName)
                : new Task(OpenEtalonFile, args);
            task.Start();
            OnPropertyChanged("FileName");
        }

        private void MainWindow_OnContentRendered(object sender, EventArgs e)
        {
            var settings = new SettingsWindow { Owner = this };
            settings.Show();
        }

        private void SavePitchPlotContextMenuButton_OnClick(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog { Filter = "PNG изображения|*.png" };
            saveDialog.FileOk += (o, args) =>
            {
                var dialog = o as SaveFileDialog;
                if (dialog == null) return;
                var fileName = dialog.FileName;
                PitchPlotView.SaveBitmap(fileName, (int) PitchPlotView.ActualWidth, (int) (PitchPlotView.ActualHeight*1.6), OxyColors.Transparent);
            };
            saveDialog.ShowDialog(this);
        }

        private void SaveAcfsPlotContextMenuButton_OnClick(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog { Filter = "PNG изображения|*.png" };
            saveDialog.FileOk += (o, args) =>
            {
                var dialog = o as SaveFileDialog;
                if (dialog == null) return;
                var fileName = dialog.FileName;
                AcfsSamplePlotView.SaveBitmap(fileName, (int)AcfsSamplePlotView.ActualWidth, (int)(AcfsSamplePlotView.ActualHeight), OxyColors.Transparent);
            };
            saveDialog.ShowDialog(this);
        }

        private void SaveAcfPlotContextMenuNutton_OnClick(object sender, RoutedEventArgs e)
        {
            var saveDialog = new SaveFileDialog { Filter = "PNG изображения|*.png" };
            saveDialog.FileOk += (o, args) =>
            {
                var dialog = o as SaveFileDialog;
                if (dialog == null) return;
                var fileName = dialog.FileName;
                AcfSamplePlotView.SaveBitmap(fileName, (int)AcfSamplePlotView.ActualWidth, (int)(AcfSamplePlotView.ActualHeight), OxyColors.Transparent);
            };
            saveDialog.ShowDialog(this);
        }

        internal void SaveFile()
        {
            var fileInfo = new FileInfo(FileName);
            var saveDialog = new SaveFileDialog {Filter = "WAV файл|*.wav", FileName = fileInfo.Name};
            saveDialog.FileOk += (sender, args) => {
                var dialog = sender as SaveFileDialog;
                if (dialog == null) return;
                var fileName = dialog.FileName;

                var exporter = new FileExporter {SampleRate = (int)_sampleFreq, Data = _inputFile.Samples.Select(x=> (float)x).ToArray()};
                exporter.SaveAsWav(fileName);
            };
            saveDialog.ShowDialog(this);
        }
    }
}
