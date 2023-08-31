using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Accord.Math;

using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;

namespace DeepLearningNlp
{
    class Preprocessor : IProcessor
    {
        private int minWordCount = 32;
        private int aveWordCount = 109;
        private int maxWordCount = 224;
        private string corpusPath = "\\data\\complaints_processed.csv";
        private PCA pca;

        public int NFeatures { get; private set; }
        public int NSentences { get; private set; }

        public Preprocessor(int nSentences = 200, int nFeatures = 100)
        {
            NFeatures = nFeatures;
            NSentences = nSentences;
            pca = new PCA(nFeatures);
        }

        private List<string> WordTokenize(string text)
        {
            string wordPattern = @"\b\w+\b";
            List<string> words = new List<string>();
            MatchCollection matches = Regex.Matches(text.ToLower(), wordPattern);
            foreach (Match match in matches)
            {
                words.Add(match.Value);
            }
            return words;
        }

        private List<List<string>> TokenizeStem(List<string> sentences)
        {
            PorterStemmer stemmer = new PorterStemmer();
            HashSet<string> stopwordsSet = new HashSet<string>();

            if (File.Exists("stopwords.json"))
            {
                string file = File.ReadAllText("stopwords.json");
                stopwordsSet = JsonSerializer.Deserialize<HashSet<string>>(file);
            }

            List<List<string>> tokenizedSentences = new List<List<string>>();
            foreach (string sentence in sentences)
            {
                List<string> words = WordTokenize(sentence);
                List<string> stemmedWords = new List<string>();
                foreach (string word in words)
                {
                    if (!stopwordsSet.Contains(word))
                    {
                        stemmedWords.Add(stemmer.Stem(word));
                    }
                }
                tokenizedSentences.Add(stemmedWords);
            }
            return tokenizedSentences;
        }


        public string CategorizeNWords(int nWords)
        {
            if (1 < nWords && nWords <= minWordCount)
            {
                return "1-32";
            }
            else if (minWordCount < nWords && nWords <= aveWordCount)
            {
                return "32-109";
            }
            else if (aveWordCount < nWords && nWords <= maxWordCount)
            {
                return "109-224";
            }
            return "outlier";
        }

        private double[,] ComputeBagOfWords(List<List<string>> sentences)
        {
            List<string> vocabulary = ComputeVocabulary(sentences);

            int numSentences = sentences.Count;
            int numWords = vocabulary.Count;
            double[,] encoded = new double[numSentences, numWords];

            Dictionary<string, int> word2index = new Dictionary<string, int>();
            Dictionary<int, string> index2word = new Dictionary<int, string>();
            for (int i = 0; i < vocabulary.Count; i++)
            {
                word2index[vocabulary[i]] = i;
                index2word[i] = vocabulary[i];
            }

            for (int i = 0; i < numSentences; i++)
            {
                foreach (string word in sentences[i])
                {
                    if (word2index.ContainsKey(word))
                    {
                        int wordIndex = word2index[word];
                        encoded[i, wordIndex]++;
                    }
                }
            }

            double[] norms = (Matrix.Sum(encoded.Pow(2), 1)).Sqrt();
            for (int i = 0; i < numSentences; i++)
            {
                for (int j = 0; j < numWords; j++)
                {
                    encoded[i, j] /= norms[i];
                }
            }

            return encoded;
        }

        private List<string> ComputeVocabulary(List<List<string>> sentences)
        {
            List<string> vocabulary = new List<string>();

            if (File.Exists("vocabulary.json"))
            {

                string file = File.ReadAllText("vocabulary.json");
                vocabulary = JsonSerializer.Deserialize<List<string>>(file);
            }
            else
            {
                foreach (List<string> sentence in sentences)
                {
                    vocabulary.AddRange(sentence);
                }
                vocabulary = vocabulary.Distinct().ToList();
                string json = JsonSerializer.Serialize(this);
                File.WriteAllText("vocabulary.json", json);
            }

            return vocabulary;
        }


        public void FitTransform(out double[][] x, out double[][] y)
        {
            LoadCorpus(out List<string> productLabels, out List<string> narratives);

            
            CleanDataset(ref productLabels, ref narratives);
            Console.WriteLine($"{productLabels[0]} {narratives[0]}");
            List<List<string>> tokenizedSentences = TokenizeStem(narratives);
            double[][] bagOfWords = ComputeBagOfWords(tokenizedSentences).ToJagged();
            Console.WriteLine($"{ba[0]} {narratives[0]}");
            pca.Fit(bagOfWords);


            string json = JsonSerializer.Serialize(pca);
            File.WriteAllText("pca.json", json);
           

            double[][] transformedData = pca.Transform(bagOfWords);
            y = EncodeLabels(productLabels);
            x = transformedData;
        }

        private void LoadCorpus(out List<string> productLabels, out List<string> narratives)
        {
            productLabels = new List<string>();
            narratives = new List<string>();
            string filePath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, $"..\\..\\..{corpusPath}"));
            using (var reader = new StreamReader(filePath))
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)))
            {
                csv.Read(); // Skip header line
                csv.ReadHeader(); // Read header
                while (csv.Read())
                {
                    string productLabel = csv.GetField<string>("product");
                    string narrative = csv.GetField<string>("narrative");

                    productLabels.Add(productLabel);
                    narratives.Add(narrative);
                }
            }
        }

        private void CleanDataset(ref List<string> productLabels, ref List<string> narratives)
        {
            List<int> indicesToRemove = new List<int>();
            for (int i = 0; i < productLabels.Count; i++)
            {
                int nWords = WordTokenize(narratives[i]).Count;
                if (CategorizeNWords(nWords) != "32-109")
                {
                    indicesToRemove.Add(i);
                }
            }

            foreach (int index in indicesToRemove.OrderByDescending(i => i))
            {
                productLabels.RemoveAt(index);
                narratives.RemoveAt(index);
            }
        }

        private double[][] EncodeLabels(List<string> productLabels)
        {
            LabelEncoder labelEncoder = new LabelEncoder();
            //productLabels.Select(p => Array.IndexOf(labelEncoder.getClasses(), p)).ToArray()
            int[,] encodedLabels = labelEncoder.FitTransform(productLabels.ToArray());
            int numClasses = labelEncoder.getClasses().Length;
            int numSamples = encodedLabels.Length;

            double[][] y = new double[numSamples][];
            //for (int i = 0; i < numSamples; i++)
            //{
                //y[i] = new double[numClasses];
                //y[i][encodedLabels[i]] = 1.0;
            //}
            return y;
        }

        public double[] Transform(string rawText)
        {
            throw new NotImplementedException();
        }
    }
}