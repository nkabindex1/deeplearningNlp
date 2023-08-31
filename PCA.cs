using BaseLibS.Num;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Math;
using Accord.Math.Decompositions;

namespace DeepLearningNlp
{
    
    class PCA
    {
        private int n_components;
        private double[,] components;
        private double[] mean;

        public PCA(int n_components)
        {
            this.n_components = n_components;
            this.components = null;
            this.mean = null;
        }

        public void Fit(double[][] X)
        {
            //var matrix = Matrix.Create(X);
            int nSamples = X.Length;
            int nFeatures = X.GetRow(0).Length;

            // Compute mean of each feature
            this.mean = new double[nFeatures];
            for (int j = 0; j < nFeatures; j++)
            {
                this.mean[j] = X.GetColumn(j).Mean();
            }

            double[][] X_centered = new double[nSamples][];
            for (int i = 0; i < nSamples; i++)
            {
                X_centered[i] = X.GetRow(i).Subtract(this.mean);
            }

            // Compute covariance matrix
            var covariance = Matrix.Dot(X_centered.Transpose(), X_centered).Multiply(1.0 / (nSamples - 1));
            double[,] arrCovariance = new double[covariance.Length, covariance[0].Length];
            for (int i = 0; i < nSamples; i++)
            {
                for (int j = 0; j < nFeatures; j++)
                {
                    arrCovariance[i, j] = X[i][j];
                }
            }


            // Perform eigenvalue decomposition
            EigenvalueDecomposition eigenDecomposition = new EigenvalueDecomposition(arrCovariance) ;

            var eigenvalues = eigenDecomposition.RealEigenvalues;
            var eigenvectors = eigenDecomposition.Eigenvectors;

            // Sort eigenvalues and corresponding eigenvectors
            int[] indices = eigenvalues
                .Select((value, index) => new KeyValuePair<double, int>(value, index))
                .OrderByDescending(kv => kv.Key)
                .Select(kv => kv.Value)
                .ToArray();

            double[,] sorted_eigenvectors = eigenvectors.Get(0, nFeatures, indices);

            // Select top-k eigenvectors
            this.components = sorted_eigenvectors.Get(0, nFeatures, 0, n_components);
        }

        public double[][] Transform(double[][] X)
        {
            int nSamples = X.Length;
            int nFeatures = X[0].Length;

            // Center the data
            double[][] X_centered = new double[nSamples][];
            for (int i = 0; i < nSamples; i++)
            {
                X_centered[i] = X.GetRow(i).Subtract(this.mean);
            }
            // Project the data onto the principal components
            double[][] transformed = Matrix.Dot(X_centered, this.components);

            return transformed;
        }
    }


}

