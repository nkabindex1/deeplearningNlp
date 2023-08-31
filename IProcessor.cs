using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeepLearningNlp
{
    interface IProcessor
    {
        void FitTransform(out double[][] x, out double[][] y);
        double[] Transform(string rawText);
    }
}
