using System;
using System.Collections.Generic;
using System.Linq;

namespace DeepLearningNlp
{
    

    class LabelEncoder
    {
        private int[] labels;
        private string[] classes;

        public int[,] FitTransform(string[] labels)
        {
            //this.labels = labels; 
            this.classes = labels.Distinct().OrderBy(c => c).ToArray();
            int numClasses = classes.Length;

            int[] encodedLabels = Enumerable.Range(0, classes.Length).ToArray(); //[1,2,3,4,5]
            for (int i = 0; i < classes.Length; i++)
            {
                for (int j = 0; j < this.labels.Length; j++)
                {
                    if (labels[j] == this.classes[i])
                    {
                        this.labels[j] = encodedLabels[i];
                    }
                }
            }

            
            int[,] y = ToCategorical(this.labels, numClasses);
            return y;
        }

        private int[,] ToCategorical(int[] labels, int numClasses)
        {
            int numSamples = labels.Length;
            int[,] categoricalLabels = new int[numSamples, numClasses];

            for (int i = 0; i < numSamples; i++)
            {
                int label = labels[i];
                categoricalLabels[i, label] = 1;
            }
            return categoricalLabels;
        }


        public string[] getClasses()
        {
            return classes;
        }
    }



}
