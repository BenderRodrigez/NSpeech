using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NSpeech.Verification.Clustering
{
    /// <summary>
    /// Using the codebook to approximate multdimentional signal
    /// </summary>
    public class VectorQuantization
    {
        /// <summary>
        /// Code book size
        /// </summary>
        private readonly int _codeBookSize;

        /// <summary>
        /// Quantization distortion measure boreder to stop learning
        /// </summary>
        public double DistortionDelta { get; set; }

        public int KMeansIterationsBorder { get; set; }

        /// <summary>
        /// Init quantizer <see cref="VectorQuantization"/>.
        /// </summary>
        /// <param name="codeBookSize">Size of the codebook</param>
        public VectorQuantization(int codeBookSize)
        {
            _codeBookSize = codeBookSize;
            DistortionDelta = 0.05;
            KMeansIterationsBorder = 50;
        }

        /// <summary>
        /// Remove all doubled and not used codewords from code book
        /// </summary>
        /// <returns>Code book without garbage</returns>
        public float[][] ClearCodeBook(float[][] trainingSet, float[][] codeBook)
        {
            var effecivness = new int[codeBook.Length];
            foreach (var t in trainingSet)
            {
                effecivness[QuantazationIndex(t, codeBook)]++;
            }

            var clearCodeBook = new List<float[]>(codeBook.Length);
            for (int i = 0; i < effecivness.Length; i++)
            {
                if (effecivness[i] > 0)
                    clearCodeBook.Add(codeBook[i]);
            }
            return clearCodeBook.ToArray();
        }

        /// <summary>
        /// Generates new codebook
        /// </summary>
        public float[][] Learn(int vectorLength, float[][] trainingSet)
        {
            int iteration = 1;//current iteration
            var codeBook = new float[iteration][];
            codeBook[0] = new float[vectorLength];
            for (int j = 0; j < vectorLength; j++)
            {
                var j1 = j;
                var res = Parallel.For(0, trainingSet.Length, i =>
                    codeBook[0][j1] += trainingSet[i][j1]);//init codebook as average value 
                while (!res.IsCompleted)
                {

                }
                codeBook[0][j] /= trainingSet.Length;
            }
            var averageQuantError = AverageQuantizationError(trainingSet, codeBook);

            while (iteration < _codeBookSize)
            {
                var newCodeBook = new float[iteration * 2][];
                Parallel.For(0, codeBook.Length, cb =>
                {
                    var maxDistance = double.NegativeInfinity;
                    var centrOne = new float[vectorLength];
                    var centrTwo = new float[vectorLength];
                    for (int i = 0; i < trainingSet.Length - 1; i++)
                    {
                        if (QuantazationIndex(trainingSet[i], codeBook) != cb) continue;

                        for (int j = i + 1; j < trainingSet.Length; j++)
                        {
                            if (QuantazationIndex(trainingSet[j], codeBook) != cb) continue;

                            var distance = QuantizationError(trainingSet[i], trainingSet[j]);
                            if (distance > maxDistance)
                            {
                                centrOne = trainingSet[i];
                                centrTwo = trainingSet[j];
                                maxDistance = distance;
                            }
                        }
                    }

                    newCodeBook[(cb + 1) * 2 - 1] = new float[vectorLength];
                    newCodeBook[(cb + 1) * 2 - 2] = new float[vectorLength];

                    if (centrOne.Sum() == 0.0)
                    {
                        var rand = new Random();
                        for (int i = 0; i < vectorLength; i++)
                        {
                            newCodeBook[(cb + 1) * 2 - 1][i] = codeBook[cb][i] - codeBook[cb][i] * (float)rand.NextDouble() * 0.1f;
                        }
                    }
                    else
                    {
                        Array.Copy(centrOne, newCodeBook[(cb + 1) * 2 - 1], vectorLength);
                    }
                    if (centrTwo.Sum() == 0.0)
                    {
                        var rand = new Random();
                        for (int i = 0; i < vectorLength; i++)
                        {
                            newCodeBook[(cb + 1) * 2 - 2][i] = codeBook[cb][i] + codeBook[cb][i] * (float)rand.NextDouble() * 0.1f;
                        }
                    }
                    else
                    {
                        Array.Copy(centrTwo, newCodeBook[(cb + 1) * 2 - 2], vectorLength);
                    }
                });

                iteration *= 2;
                codeBook = newCodeBook;

                //D(m-1) - Dm > DistortionDelta?
                var averageQuantErrorOld = averageQuantError;
                averageQuantError = AverageQuantizationError(trainingSet, codeBook);
                var kMeansIntertionsCount = 0;
                while (Math.Abs(averageQuantErrorOld - averageQuantError) > DistortionDelta && kMeansIntertionsCount < KMeansIterationsBorder)//learning stop criteria
                {
                    //yi = total_sum(xi)/N
                    var tmpCodeBook = new float[codeBook.Length][];
                    var vectorsCount = new int[codeBook.Length];
                    var res = Parallel.For(0, trainingSet.Length, i =>
                    {
                        var codeBookIndex = QuantazationIndex(trainingSet[i], codeBook);
                        if (tmpCodeBook[codeBookIndex] == null)
                            tmpCodeBook[codeBookIndex] = new float[vectorLength];
                        for (int j = 0; j < vectorLength; j++)
                        {
                            tmpCodeBook[codeBookIndex][j] += trainingSet[i][j];
                        }
                        vectorsCount[codeBookIndex]++;
                    });

                    while (!res.IsCompleted)
                    {

                    }

                    var result = Parallel.For(0, tmpCodeBook.Length, i =>
                    {
                        if (tmpCodeBook[i] == null)
                        {
                            tmpCodeBook[i] = new float[vectorLength];
                            Array.Copy(codeBook[i], tmpCodeBook[i], vectorLength);
                        }
                        else if (vectorsCount[i] > 0)
                        {
                            for (int j = 0; j < tmpCodeBook[i].Length; j++)
                                tmpCodeBook[i][j] /= vectorsCount[i];
                        }
                    });
                    while (!result.IsCompleted)
                    {

                    }
                    codeBook = tmpCodeBook;
                    averageQuantErrorOld = averageQuantError;
                    averageQuantError = AverageQuantizationError(trainingSet, codeBook);
                    kMeansIntertionsCount++;
                }
            }
            return codeBook;
        }

        /// <summary>
        /// Calulates the average distortion measure for train set
        /// </summary>
        /// <returns>Average value</returns>
        private double AverageQuantizationError(float[][] trainingSet, float[][] codeBook)
        {//D=(total_sum(d(x, Q(x))))/N
            var errorRate = trainingSet.Sum(t => QuantizationError(t, Quantize(t, codeBook)));
            errorRate /= trainingSet.Length;
            return errorRate;
        }

        /// <summary>
        /// Vector quantization operator
        /// </summary>
        /// <param name="x">Input data vector</param>
        /// <param name="codeBook"></param>
        public float[] Quantize(float[] x, float[][] codeBook)
        {//Оператор квантования
            var minError = double.PositiveInfinity;
            int min = 0;
            for (int i = 0; i < codeBook.Length; i++)
            {
                var error = QuantizationError(x, codeBook[i]);
                if (error < minError)
                {
                    min = i;
                    minError = error;
                }
            }
            return codeBook[min];
        }

        /// <summary>
        /// Returns codeword position in code book
        /// </summary>
        /// <returns>Code word index in code book</returns>
        /// <param name="x">Test vector</param>
        /// <param name="codeBook">Code book</param>
        private int QuantazationIndex(float[] x, float[][] codeBook)
        {//same as Quantize, but returns index of codeword
            var minError = double.PositiveInfinity;
            int min = 0;
            for (int i = 0; i < codeBook.Length; i++)
            {
                var error = QuantizationError(x, codeBook[i]);

                if (!(error < minError)) continue;

                min = i;
                minError = error;
            }
            return min;
        }

        /// <summary>
        /// Distortion measure between two vectors
        /// </summary>
        /// <returns>The error</returns>
        /// <param name="a">The alpha component</param>
        /// <param name="b">The blue component</param>
        private static float QuantizationError(float[] a, float[] b)
        {//d=total_sum(a^2-b^2)
            if (a.Length == b.Length)
            {
                var error = (float)a.Select((t, i) => Math.Pow(t - b[i], 2)).Sum();
                return error;
            }
            throw new Exception("Вектора разной длины!");
        }

        /// <summary>
        /// Calculates energy of the distortion measure signal (quantization noise)
        /// </summary>
        /// <param name="testSet">Source signal</param>
        /// <param name="codeBook">Vector quantization codebook</param>
        /// <returns>Energy value</returns>
        public double DistortionMeasureEnergy(float[][] testSet, float[][] codeBook)
        {
            double res = 0;
            for (int i = 0; i < testSet.Length; i++)
            {
                res += Math.Pow(QuantizationError(testSet[i], Quantize(testSet[i], codeBook)), 2);
            }
            res /= testSet.Length;
            return res;
        }
    }
}