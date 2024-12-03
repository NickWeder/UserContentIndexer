using LLama.Common;
using LLama;
using LLama.Sampling;

namespace UserContentIndexer
{
    internal class SummeryVideo
    {
        public async Task SumVideoContext(string llama, List<string> videoResults, string whisperResult)
        {

            // Hier kannst du deinen Prompt eingeben
            var prompt = "You are an Assistant for summerizing described images of a video.\nSummerize the following Videoresults.\nGive me a description of the Audiotranscription.\nProvide me relavent Tags of the summerized videoinformations.";

            // Hier wird der Beschribene Videocontent hinzugefügt
            prompt += "\n\nVideoresults:\n";
            for (int i = 0; i < videoResults.Count; i++)
            {
                prompt += videoResults[i].Replace("Note:", "").Trim() + "\n\n";
            }
            prompt += "Audiotranscription:\n" + whisperResult;
            


            var parameters = new ModelParams(llama)
            {
                GpuLayerCount = -1,
            };
            using var model = await LLamaWeights.LoadFromFileAsync(parameters);

            using var context = model.CreateContext(parameters);
            var ex = new InteractiveExecutor(context);

            var inferenceParams = new InferenceParams
            {
                AntiPrompts = new List<string> { "User:" },
                MaxTokens = 1024,

                SamplingPipeline = new DefaultSamplingPipeline
                {
                    Temperature = 0.6f
                }
            };

            await foreach (var text in ex.InferAsync(prompt, inferenceParams))
            {
                Console.Write(text);
            }
        }
    }
}
