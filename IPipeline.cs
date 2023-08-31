using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepLearningNlp
{
    interface IPipeline
    {
        bool TrainModel(int nSentences, int nFeatures, int hiddenSize, int numEpochs, double learningRate);
        int MakePrediction(string rawText);
        (double accuracy, double[,] matrix, double[] precision, double[] recall, double[] f1) EvaluateModel(double[][] XTest, int[] yTest);
    }
}
