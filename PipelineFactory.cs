using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;


namespace DeepLearningNlp
{
    class PipelineFactory
    {
        public static IPipeline createPipeline()
        {
            if (File.Exists("pipeline.json"))
            {
                string json = File.ReadAllText("pipeline.json");
                return JsonSerializer.Deserialize<Pipeline>(json);
            }
            else
            {
                return new Pipeline();
            }
        }

    }
}
