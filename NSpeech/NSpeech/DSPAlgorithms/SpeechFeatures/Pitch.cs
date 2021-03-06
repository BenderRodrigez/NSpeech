﻿using System;
using System.Collections.Generic;
using System.Linq;
using NSpeech.DSPAlgorithms.Filters;

namespace NSpeech.DSPAlgorithms.SpeechFeatures
{
    internal class Pitch : ISpeechFeature
    {
        private readonly Signal _signal;

        private readonly IEnumerable<Tuple<int, int>> _speechMarks;

        public Pitch(Signal signal, IEnumerable<Tuple<int, int>> speechMarks)
        {
            _signal = signal.Clone().Normalize();
            _speechMarks = speechMarks;

            MaxFrequencyJumpPercents = 0.15;
            BlurDiameter = 9;
            CentralLimitation = 0.3;
            HighPassFilterBorder = 60.0f;
            LowPassFilterBorder = 600.0f;
            MinimalVoicedSpeechLength = 0.04;
            AnalysisInterval = 0.04;
            WindowFunction = WindowFunctions.WindowFunctions.Blackman;
            Overlapping = 0.95;
        }

        public double AnalysisInterval { get; set; } //in seconds

        public double Overlapping { get; set; } //in percents

        public float HighPassFilterBorder { get; set; }

        public float LowPassFilterBorder { get; set; }

        public WindowFunctions.WindowFunctions WindowFunction { get; set; }

        public double CentralLimitation { get; set; }

        public int BlurDiameter { get; set; }

        public double MinimalVoicedSpeechLength { get; set; }

        public double MaxFrequencyJumpPercents { get; set; }

        public Signal GetFeature()
        {
            return new Signal(TrackPitch().Select(x=> x != 0.0?_signal.SignalFormat.SampleRate/x:0.0).ToArray(), _signal.SignalFormat);
        }

        private double[] TrackPitch()
        {
            //preprocessing
            var filtredSignal = _signal.Clone().ApplyHighPassFiltration(HighPassFilterBorder);
            filtredSignal = filtredSignal.ApplyLowPassFiltration(LowPassFilterBorder);

            //analysis variables
            var size = (int) Math.Round(AnalysisInterval*_signal.SignalFormat.SampleRate);
            var offset = size*(1.0 - Overlapping);
            var jump = (int) Math.Round(offset);
            var furieSize = (int) Math.Pow(2, Math.Ceiling(Math.Log(size, 2) + 1));
            var resultImg = new List<double>(_signal.Samples.Length);
            var prevStop = 0;
            var lower = (int) Math.Round(_signal.SignalFormat.SampleRate/60.0); //60 Hz in ACF values array border
            var higher = (int) Math.Round(_signal.SignalFormat.SampleRate/600.0); //600 Hz in ACF values array border
            var globalCandidates = new List<List<Tuple<double, double>>>();
            var gaussianFilter = new GaussianFilter(BlurDiameter);

            foreach (var curentMark in _speechMarks)
            {
                for (var i = prevStop; i < curentMark.Item1; i++)
                    if (i%jump == 0)
                    {
                        resultImg.Add(0.0);
                        globalCandidates.Add(new List<Tuple<double, double>>());
                    }

                var acfSamples = filtredSignal.Split(AnalysisInterval, Overlapping, WindowFunction, curentMark.Item1,
                    curentMark.Item2);
                var acfsSamples = _signal.Split(AnalysisInterval, Overlapping, WindowFunction, curentMark.Item1,
                    curentMark.Item2);

                for (int sample = 0; sample < acfSamples.Length && sample < acfsSamples.Length; sample++)
                {
                    var acf = acfSamples[sample].GetAutocorrelation(CentralLimitation).Samples;
                    var acfsSample = acfsSamples[sample].GetSpectrumAutocorrelation(furieSize, gaussianFilter).Samples;

                    var candidates = new List<Tuple<double, double>>(); //int = position, double = amplitude

                    //extract candidates
                    var acfsCandidates = new List<Tuple<int, double>>();
                    for (var i = 1; i < acfsSample.Length - 1; i++)
                        if ((acfsSample[i] > acfsSample[i - 1]) &&
                            (acfsSample[i] > acfsSample[i + 1]))
                            acfsCandidates.Add(new Tuple<int, double>(i, acfsSample[i]));

                    for (var i = higher; (i < acf.Length) && (i < lower); i++)
                        if ((acf[i - 1] > acf[i - 2]) && (acf[i - 1] > acf[i]))
                            candidates.Add(new Tuple<double, double>(i - 1.0, acf[i - 1]));
                                //add each maximum of function from 60 to 600 Hz

                    var aproximatedPosition = acfsCandidates.Any() ? acfsCandidates[0].Item1 : -1;
                    var freqPosition = _signal.SignalFormat.SampleRate/(double)furieSize*aproximatedPosition;
                        //aproximated frequency value

                    if ((aproximatedPosition > -1) && (freqPosition > 60) && (freqPosition < 600))
                    {
                        var acfPosition = _signal.SignalFormat.SampleRate/freqPosition; //aproximated time value

                        resultImg.Add(acfPosition);
                        globalCandidates.Add(candidates);
                    }
                    else
                    {
                        resultImg.Add(0.0);
                        globalCandidates.Add(candidates);
                    }
                }
                prevStop = curentMark.Item2 + 1;
            }
            ExtractPitch(resultImg, globalCandidates, _signal.SignalFormat.SampleRate, furieSize, jump);

            return resultImg.ToArray();
        }

        private void ExtractPitch(List<double> img, IReadOnlyList<List<Tuple<double, double>>> globalCandidates,
            int sampleRate, double furieSize, int jumpSize)
        {
            if (img.Count == 0)
                return;

            var searchWindow = Math.Ceiling(sampleRate*1.2/furieSize);
            for (var i = 0; i < img.Count; i++) //find value by approximation
                if ((img[i] > 0.0) && globalCandidates[i].Any(x => Math.Abs(x.Item1 - img[i]) < searchWindow))
                {
                    var nearest =
                        globalCandidates[i]
                            .Where(x => Math.Abs(x.Item1 - img[i]) < searchWindow)
                            .Max(x => x.Item2);
                    img[i] =
                        globalCandidates[i]
                            .Where(x => Math.Abs(x.Item1 - img[i]) < searchWindow)
                            .First(x => x.Item2 >= nearest)
                            .Item1;
                }

            var prevVal = 0.0;
            for (var i = BlurDiameter/2; i < img.Count - BlurDiameter/2; i++) //use median filter to cath the errors
            {
                var itemsToSort = new List<double>(BlurDiameter);
                for (var j = -BlurDiameter/2; j <= BlurDiameter/2; j++)
                    itemsToSort.Add(img[i + j]);
                var arr = itemsToSort.ToArray();
                Array.Sort(arr);
                if ((Math.Abs(arr[BlurDiameter/2] - prevVal)/Math.Max(prevVal, arr[BlurDiameter/2]) >
                     MaxFrequencyJumpPercents) && (prevVal > 0.0))
                    for (var j = i; (j < i + sampleRate*MinimalVoicedSpeechLength/jumpSize) && (j < img.Count); j++)
                        img[j] = 0.0;
                else
                    img[i] = arr[BlurDiameter/2];
                prevVal = img[i];
            }

            for (var i = img.Count - BlurDiameter/2; i < img.Count; i++)
                //use median filter to cath the errors in the last points
            {
                var itemsToSort = new List<double>(BlurDiameter);
                for (var j = -BlurDiameter/2; j <= BlurDiameter/2; j++)
                    itemsToSort.Add(i + j < img.Count ? img[i + j] : 0.0);
                var arr = itemsToSort.ToArray();
                Array.Sort(arr);
                if ((Math.Abs(arr[BlurDiameter/2] - prevVal)/Math.Max(prevVal, arr[BlurDiameter/2]) >
                     MaxFrequencyJumpPercents) && (prevVal > 0.0))
                    for (var j = i; (j < i + sampleRate*MinimalVoicedSpeechLength/jumpSize) && (j < img.Count); j++)
                        img[j] = 0.0;
                else
                    img[i] = arr[BlurDiameter/2];
                prevVal = img[i];
            }

            for (var i = 0; i < img.Count; i++)
                //remove too small pitch intervals, if they shorter than it's physicaly possible
                if (img[i] > 0.0)
                {
                    var size = 0;
                    while ((i + size < img.Count) && (img[i + size] > 0.0))
                        size++;

                    if (size < sampleRate*MinimalVoicedSpeechLength/jumpSize)
                        for (var k = 0; (k < size) && (k + i < img.Count); k++)
                            img[i + k] = 0.0;
                    i += size;
                }
        }
    }
}