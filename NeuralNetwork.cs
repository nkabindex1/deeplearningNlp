using System;
using Accord.Math;
using System.Linq;


namespace DeepLearningNlp
{
    class NeuralNetwork
    {
        private int input_size;
        private int hidden_size;
        private int output_size;
        private double[,] W1;
        private double[,] W2;
        private double[,] b1;
        private double[,] b2;
        private double[,] z1;
        private double[,] a1;
        private double[,] z2;
        private double[,] a2;

        public NeuralNetwork(int input_size, int hidden_size, int output_size)
        {
            this.input_size = input_size;
            this.hidden_size = hidden_size;
            this.output_size = output_size;

            // Initialize weights and biases
            this.W1 = Matrix.Random(input_size, hidden_size);
            this.b1 = new double [1, hidden_size];
            this.W2 = Matrix.Random(hidden_size, output_size);
            this.b2 = new double [1, output_size];
        }

        public double[,] Forward(double[,] X)
        {
            // Forward propagation
            double[,] dotProduct1 = Matrix.Dot(X, this.W1);
            this.z1 = dotProduct1.Add(this.b1); 
            this.a1 = Sigmoid(this.z1);
            double[,] dotProduct2 = Matrix.Dot(this.a1, this.W2);
            this.z2 = dotProduct2.Add(this.b2);
            this.a2 = Sigmoid(this.z2);
            return this.a2;
        }

        public void Backward(double[,] X, double[,] y, double learning_rate)
        {
            // Backward propagation
            int m = X.GetLength(0);
            double[,] dZ2 = this.a2.Subtract(y);
            double[,] dW2 = Matrix.Dot(this.a1.Transpose(), dZ2).Divide(m);
            double[,] sumResult = new double[1, dZ2.GetLength(1)]; // Creating a new 1xnumCols array

            for (int col = 0; col < dZ2.GetLength(1); col++)
            {
                double sum = 0;
                for (int row = 0; row < dZ2.GetLength(0); row++)
                {
                    sum += dZ2[row, col];
                }
                sumResult[0, col] = sum;
            }
            double[,] db2 = sumResult.Divide(m);
            double[,] dZ1 = Matrix.Dot(Matrix.Dot(dZ2, Matrix.Transpose(this.W2)), SigmoidDerivative(this.a1));
            double[,] dW1 = (dZ1.Dot(Matrix.Transpose(X))).Divide(m);
            double[,] sumResult2 = new double[1, dZ1.GetLength(1)]; // Creating a new 1xnumCols array

            for (int col = 0; col < dZ1.GetLength(1); col++)
            {
                double sum = 0;
                for (int row = 0; row < dZ1.GetLength(0); row++)
                {
                    sum += dZ1[row, col];
                }
                sumResult2[0, col] = sum;
            }
            double[,] db1 = sumResult.Divide(m);

            // Update weights and biases
            this.W2 = this.W2.Subtract(dW2.Multiply(learning_rate));
            this.b2 = this.b2.Subtract(db2.Multiply(learning_rate));
            this.W1 = this.W1.Subtract(dW1.Multiply(learning_rate));
            this.b1 = this.b1.Subtract(db1.Multiply(learning_rate));
        }

        public void Train(double[,] X, double[,] y, int num_epochs, double learning_rate)
        {
            for (int epoch = 0; epoch < num_epochs; epoch++)
            {
                // Forward propagation
                double[,] output = Forward(X);

                // Backward propagation
                Backward(X, y, learning_rate);

                // Print loss
                double loss = MeanSquaredError(output, y);
                if (epoch % 100 == 0)
                {
                    Console.WriteLine($"Epoch {epoch}: Loss = {loss}");
                }
            }
        }

        public double[] Predict(double[] X)
        {
            // Forward propagation for prediction
            double[,] matrix = new double[1, X.Length];
            for (int j = 0; j < X.Length; j++)
            {
                matrix[0, j] = X[j];
            }
            double[,] doutput = Forward(matrix);
            double[,] predictions = Matrix.Round(doutput);
            double[] output = new double[X.Length];

            for (int j = 0; j < X.Length; j++)
            {
                output[j] = predictions[0, j];
            }

            return output;
        }


        public double[,] Predict(double[,] X)
        {
            // Forward propagation for prediction
           
            double[,] output = Forward(X);
            double[,] predictions = Matrix.Round(output);
            return predictions;
        }

        private double[,] Sigmoid(double[,] x)
        {
            int numRows = x.GetLength(0);
            int numCols = x.GetLength(1);

            double[,] negatedMatrix = new double[numRows, numCols];
            double[,] resultMatrix = new double[numRows, numCols];

            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    negatedMatrix[i, j] = -x[i, j];
                    resultMatrix[i, j] = 1 / (1 + Math.Exp(negatedMatrix[i, j]));
                }
            }
            
            return resultMatrix;
        }

        private double[,] SigmoidDerivative(double[,] x)
        {
            return Matrix.Dot(x, x.Subtract(1));
        }

     
        private double MeanSquaredError(double[,] y_pred, double[,] y_true)
        {
            int numRows = y_pred.GetLength(0);
            int numCols = y_pred.GetLength(1);

            double sumSquaredDifferences = 0.0;
            for (int i = 0; i < numRows; i++)
            {
                for (int j = 0; j < numCols; j++)
                {
                    double difference = y_pred[i, j] - y_true[i, j];
                    sumSquaredDifferences += difference * difference;
                }
            }

            int totalElements = numRows * numCols;
            double meanSquaredError = sumSquaredDifferences / totalElements;
            return meanSquaredError;
        }
    }
}
