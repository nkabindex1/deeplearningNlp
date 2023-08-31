using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;
using Microsoft.VisualBasic.FileIO;

namespace DeepLearningNlp
{
    class DataHandler
    {
        public static DataTable LoadCorpus()
        {
            DataTable corpus = new DataTable();
            corpus.Columns.Add("product", typeof(string));
            corpus.Columns.Add("narrative", typeof(string));

            string path = "./data/complaints_processed.csv";
            if (File.Exists(path))
            {
                using (TextFieldParser parser = new TextFieldParser(path))
                {
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    // Skip header
                    if (!parser.EndOfData)
                    {
                        parser.ReadLine();
                    }

                    while (!parser.EndOfData)
                    {
                        string[] fields = parser.ReadFields();
                        if (fields.Length >= 2 && !string.IsNullOrEmpty(fields[0]) && !string.IsNullOrEmpty(fields[1]))
                        {
                            corpus.Rows.Add(fields[0], fields[1]);
                        }
                    }
                }

                Downsample(ref corpus);
            }

            return corpus;
        }

        private static void Downsample(ref DataTable corpus)
        {
            DataRow[] creditReportingRows = corpus.Select("product = 'credit_reporting'");
            int creditReportingRowCount = creditReportingRows.Length;
            int otherRowCount = corpus.Rows.Count - creditReportingRowCount;
            int sampleSize = (int)Math.Round((otherRowCount - creditReportingRowCount) / 4.0);

            Random random = new Random();
            List<int> sampledIndices = new List<int>();
            for (int i = 0; i < sampleSize; i++)
            {
                int randomIndex = random.Next(creditReportingRowCount);
                sampledIndices.Add(creditReportingRows[randomIndex].Table.Rows.IndexOf(creditReportingRows[randomIndex]));
            }

            List<DataRow> rowsToKeep = new List<DataRow>();
            rowsToKeep.AddRange(corpus.Select($"product <> 'credit_reporting'"));
            foreach (int index in sampledIndices)
            {
                rowsToKeep.Add(corpus.Rows[index]);
            }

            DataTable downsampledCorpus = corpus.Clone();
            foreach (DataRow row in rowsToKeep)
            {
                downsampledCorpus.ImportRow(row);
            }

            corpus = downsampledCorpus;
        }
    }

}