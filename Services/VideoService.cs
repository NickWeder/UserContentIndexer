using LLama.Common;
using LLama.Native;
using LLama.Sampling;
using LLama;
using System.Text.RegularExpressions;
using UserContentIndexer.Interfaces;

namespace UserContentIndexer.Services
{
    public class VideoService : BaseLanguageModelService, IVideoService
    {
        public async Task<List<string>> AnalyzeVideoAsync(string llamaModelPath, string llavaModelPath, List<string> images, string transcription)
        {
            var userPrompt = File.ReadAllText("./Prompts/ImageAnalyzePrompt.txt");

            List<string> results = new List<string>();

            var parameters = new ModelParams(llamaModelPath)
            {
                GpuLayerCount = -1,
            };
            using var model = await LLamaWeights.LoadFromFileAsync(parameters);
            using var clipModel = await LLavaWeights.LoadFromFileAsync(llavaModelPath);

            foreach (var image in images)
            {
                var result = "";
                var prompt = $"{{{image}}}\nUSER:\n{userPrompt}\nASSISTANT:\n";


                using var context = model.CreateContext(parameters);



                var ex = new InteractiveExecutor(context, clipModel);

                var inferenceParams = new InferenceParams
                {
                    SamplingPipeline = new DefaultSamplingPipeline
                    {
                        Temperature = 0.1f
                    },

                    AntiPrompts = new List<string> { "\nUSER:", "\nNote:"},
                    MaxTokens = 1024

                };


                var imageMatches = Regex.Matches(prompt, "{([^}]*)}").Select(m => m.Value);
                var imageCount = imageMatches.Count();
                var hasImages = imageCount > 0;

                if (hasImages)
                {
                    var imagePathsWithCurlyBraces = Regex.Matches(prompt, "{([^}]*)}").Select(m => m.Value);
                    var imagePaths = Regex.Matches(prompt, "{([^}]*)}").Select(m => m.Groups[1].Value).ToList();

                    List<byte[]> imageBytes;
                    try
                    {
                        imageBytes = imagePaths.Select(File.ReadAllBytes).ToList();
                    }
                    catch (IOException exception)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write(
                            $"Could not load your {(imageCount == 1 ? "image" : "images")}:");
                        Console.Write($"{exception.Message}");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine("Please try again.");

                    }

                    ex.Context.NativeHandle.KvCacheRemove(LLamaSeqId.Zero, -1, -1);

                    int index = 0;
                    foreach (var path in imagePathsWithCurlyBraces)
                    {
                        prompt = prompt.Replace(path, index++ == 0 ? "<image>" : "");
                    }

                    foreach (var frame in imagePaths)
                    {
                        ex.Images.Add(await File.ReadAllBytesAsync(frame));
                    }
                }

                await foreach (var text in ex.InferAsync(prompt, inferenceParams))
                {
                    result += text;
                }
                results.Add(result.Replace("USER:","").Replace("Note:",""));

            }
            return results;
        }
    }
}