namespace DeepLearningNlp
{
    interface INlpModel
    {
        void TrainModel();
        void MakePrediction();
        void EvaluateModel();
    }
}
