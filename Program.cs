using System;
using System.Diagnostics;

namespace DeepLearningNlp
{
    class Program
    {
        static void Main(string[] args)
        {
            initProgram();
        }

        static void initProgram()
        {

            bool is_running = true;
            IPipeline nlpPipeline = PipelineFactory.createPipeline();
            bool is_trained = isModelTrained();

            while (is_running)
            {
                Console.Clear();
                Console.WriteLine("nlp.ai - NLP Machine Learning Console App");
                Console.WriteLine("1. Train Model");
                Console.WriteLine("2. Make Prediction");
                Console.WriteLine("3. Evaluate Model");
                Console.WriteLine("4. Exit");
                Console.Write("Select an option: ");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        if (is_trained)
                        {
                            Console.WriteLine("A trained Model exists. Continue training? Y/N");
                            string responce = Console.ReadLine();
                            if (!string.Equals(responce, "Y", StringComparison.OrdinalIgnoreCase))
                            {
                                break;
                            }
                        }
                        Console.WriteLine("Training");
                        is_trained = nlpPipeline.TrainModel(20, 100, 100, 1000, 0.1);
                        break;
                    case "2":
                        if (!is_trained)
                        {
                            Console.WriteLine("Train a model before making a prediction. Press any key to continue...");
                            break;
                        }
                        Console.WriteLine("Provide sentence.");
                        string response = Console.ReadLine();
                        nlpPipeline.MakePrediction(response);
                        break;
                    case "3":
                        if (!is_trained)
                        {
                            Console.WriteLine("Train a model before evaluating a Model. Press any key to continue...");
                            break;
                        }
                        //nlpPipeline.EvaluateModel();
                        break;
                    case "4":
                        Console.WriteLine("Exiting...");
                        return;
                    default:
                        Console.WriteLine("Invalid choice. Press any key to continue...");
                        Console.ReadKey();
                        break;
                }
            }
        }

        
        static bool isModelTrained()
        {
            return false;
        }
    }



}