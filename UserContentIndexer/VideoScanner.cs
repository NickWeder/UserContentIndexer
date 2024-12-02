using LLama.Common;
using LLama.Native;
using LLama.Sampling;
using LLama;
using System.Text.RegularExpressions;

namespace UserContentIndexer
{
    internal class VideoScanner
    {
        private const string Prompt = @"You are an advanced AI image analysis assistant. Your task is to provide a comprehensive and detailed analysis of the uploaded image(s). 

For each image, provide a structured analysis with the following components:

1. Detailed Description
   - Primary subjects and key objects
   - Spatial relationships between elements
   - Environmental context and setting
   - Lighting characteristics
   - Color palette and mood
   - Significant actions or events
   - Composition and visual flow

3. Tags
   - Most relevant descriptive tags
   - Categorical tags
   - Style and mood tags
   - Photographic elements (camera angle, perspective)
   - Image style and potential artistic techniques
   - Apparent photography or editing techniques

Output Format:
```markdown
Description:
[Comprehensive, detailed description]

Tags:
- Primary Subject Tags: [comma-separated tags]
- Mood/Atmosphere Tags: [comma-separated tags]
- Style Tags: [comma-separated tags]
- Camera Angle: [comma-separated tags]
- Style: [comma-separated tags]";


        public static async Task<List<string>> Llava(string llama, string llava, List<string> images)
        {
            List<string> results = new List<string>();

            var parameters = new ModelParams(llama);
            using var model = await LLamaWeights.LoadFromFileAsync(parameters);
            using var clipModel = await LLavaWeights.LoadFromFileAsync(llava);

            foreach (var image in images)
            {
                var result = "";
                var prompt = $"{{{image}}}\nUSER:\n{Prompt}\nASSISTANT:\n";


                using var context = model.CreateContext(parameters);



                var ex = new InteractiveExecutor(context, clipModel);

                var inferenceParams = new InferenceParams
                {
                    SamplingPipeline = new DefaultSamplingPipeline
                    {
                        Temperature = 0.1f
                    },

                    AntiPrompts = new List<string> { "\nUSER:", "Image:", "Note:"},
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
                results.Add(result);
                
            }
            return results;
        }


    }


}
