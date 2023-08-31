using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Generic;
using Accord.Math;
using System.Linq;

using System.IO;

using System.Text.Json;

namespace DeepLearningNlp
{
    class Pipeline: IPipeline
    {
        // name of different files
        private NeuralNetwork model;
        private Preprocessor preprocessor;
        private string name = "pipeline.json";

        public Pipeline()
        {
            // string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        private void TrainTestSplit(double[][] X, int[] y, double testSize = 0.2, int randomSeed = 42)
        {
            int numSamples = X.Length;
            int numTestSamples = (int)(numSamples * testSize);

            var combinedData = X.Zip(y, (xVal, yVal) => new { X = xVal, Y = yVal }).ToList();
            combinedData.Shuffle(new Random(randomSeed));

            double[][] XShuffled = combinedData.Select(item => item.X).ToArray();
            int[] yShuffled = combinedData.Select(item => item.Y).ToArray();

            double[][] XTrain = XShuffled.Take(XShuffled.Length - numTestSamples).ToArray();
            int[] yTrain = yShuffled.Take(yShuffled.Length - numTestSamples).ToArray();
            double[][] XTest = XShuffled.Skip(XShuffled.Length - numTestSamples).ToArray();
            int[] yTest = yShuffled.Skip(yShuffled.Length - numTestSamples).ToArray();
        }

        public (double accuracy, double[,] matrix, double[] precision, double[] recall, double[] f1) EvaluateModel(double[][] XTest, int[] yTest)
        {
            int correct = 0;
            int total = XTest.Length;
            int numClasses = 5;
            double[,] matrix = new double[total, numClasses];
            for (int i = 0; i < XTest.Length; i++)
            {
                double[] probabilities = model.Predict(XTest[i]);
              
                int predictedLabel = probabilities.IndexOf(probabilities.Max());
                int trueLabel = yTest[i];
                matrix[trueLabel, predictedLabel] += 1;
                if (predictedLabel == trueLabel)
                {
                    correct++;
                }
            }


            //matrix = matrix.Divide(matrix.Sum(1, true));
            double accuracy = 1.0;//(double)correct / total;
            double[] precision, recall, f1;
            ComputePrecisionRecall(matrix, out precision, out recall, out f1);

            return (accuracy, matrix, precision, recall, f1);
        }

        private void ComputePrecisionRecall(double[,] confusionMatrix, out double[] precision, out double[] recall, out double[] f1)
        {
            int numClasses = confusionMatrix.GetLength(0);
            precision = new double[numClasses];
            recall = new double[numClasses];
            f1 = new double[numClasses];

            for (int i = 0; i < numClasses; i++)
            {
                double truePositive = confusionMatrix[i, i];
                double falsePositive = confusionMatrix.GetColumn(i).Sum() - truePositive;
                double falseNegative = confusionMatrix.GetRow(i).Sum() - truePositive;

                precision[i] = truePositive / (truePositive + falsePositive);
                recall[i] = truePositive / (truePositive + falseNegative);
                f1[i] = 2 * (precision[i] * recall[i]) / (precision[i] + recall[i]);
            }
        }

        public bool TrainModel(int nSentences, int nFeatures, int hiddenSize, int numEpochs, double learningRate)
        {
            string[] filesToRemove = { "vocabulary.json", "pca.json", "pipeline.json" };
            foreach (string file in filesToRemove)
            {
                if (File.Exists(file))
                {
                    File.Delete(file);
                    Console.WriteLine($"{file} removed...");
                }
            }

            preprocessor = new Preprocessor(nSentences, nFeatures);
            
            double[][] x; double[][] y;
            preprocessor.FitTransform(out x, out y);
            Console.WriteLine("Attempting to preprocess input");
            
            //Xtrain, ytrain, Xtest, ytest = TrainTestSplit(x, y, testSize: 0.2, randomSeed: 42);

            int inputSize = nFeatures;
            int outputSize = 5;
            

            //var (accuracy, matrix, precision, recall, f1) = EvaluateModel(x, y);
            Serialize();
            return true;
        }

        public void Serialize()
        {
            string json = JsonSerializer.Serialize(this);
            File.WriteAllText(name, json);
        }

        public Pipeline Deserialize()
        {

            // set all the default values , model not trained etc
            if (File.Exists(name))
            {
                string json = File.ReadAllText(name);
                return JsonSerializer.Deserialize<Pipeline>(json);
            }
            return null;
        }

        public int MakePrediction(string rawText)
        {
            double[] x = preprocessor.Transform(rawText);
            //double[] probabilities = model.Predict(x);
            return 0; //probabilities.IndexOf(probabilities.Max());
        }
    }

    static class Extensions
    {
        private static Random rng = new Random();

        public static void Shuffle<T>(this IList<T> list, Random random)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        public static int IndexOf<T>(this IEnumerable<T> collection, T item)
        {
            int index = 0;
            foreach (var element in collection)
            {
                if (EqualityComparer<T>.Default.Equals(element, item))
                    return index;
                index++;
            }
            return -1;
        }
    }
}

 



