namespace UserContentIndexer.Services
{
    using LLama.Common;
    using LLama.Native;
    using LLama.Sampling;
    using LLama;
    using System.Text.RegularExpressions;
    using UserContentIndexer.Interfaces;
    using UserContentIndexer.Models;
    using UserContentIndexer.Utilities;
    using UserContentIndexer.Builders;
    using System.Text;

    public class ImageAnalyzeService : IImageAnalyzeService
    {
        private readonly IModelManager modelManager;

        public ImageAnalyzeService(IModelManager modelManager)
        {
            this.modelManager = modelManager;
        }

        public async Task<IList<ImageDescription>> AnalyzeImageAsync(IList<SceneInfo> imagePaths, ModelType modelType)
        {
            using var clipModel = await this.modelManager.LoadLlava();
            using var context = await this.modelManager.CreateContextAsync();

            var imagedescriptions = new List<ImageDescription>();
            var results = new List<string>();

            foreach (var image in imagePaths)
            {
                var imagedescription = new ImageDescription();

                var prompt = PromptBuilder.BuildLlavaImageAnalyzingPrompt(image.ImagePath);

                var ex = new InteractiveExecutor(context, clipModel);

                var inferenceParams = new InferenceParams
                {
                    SamplingPipeline = new DefaultSamplingPipeline
                    {
                        Temperature = 0.4f
                    },

                    AntiPrompts = ["\nUSER:"],
                    MaxTokens = 1024

                };


                var imageMatches = Regex.Matches(prompt, "{([^}]*)}").Select(m => m.Value);
                var imageCount = imageMatches.Count();
                var hasImages = imageCount > 0;

                if (hasImages)
                {
                    var imagePathsWithCurlyBraces = Regex.Matches(prompt, "{([^}]*)}").Select(m => m.Value);
                    var imagePath = Regex.Matches(prompt, "{([^}]*)}").Select(m => m.Groups[1].Value).ToList();

                    List<byte[]> imageBytes;
                    try
                    {
                        imageBytes = imagePath.Select(File.ReadAllBytes).ToList();
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

                    var index = 0;
                    foreach (var path in imagePathsWithCurlyBraces)
                    {
                        prompt = prompt.Replace(path, index++ == 0 ? "<image>" : "");
                    }

                    foreach (var frame in imagePath)
                    {
                        ex.Images.Add(await File.ReadAllBytesAsync(frame));
                    }
                }
                var result = "";
                await foreach (var text in ex.InferAsync(prompt, inferenceParams))
                {
                    result += text;
                }
                results.Add(Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(result.Replace("\nUSER:", "").Replace("Note:", ""))));

                imagedescription.Tags = SplitResults.SplitTags(result);
                imagedescription.Videodescription = SplitResults.SplitDescription(result).Replace("\u0027", "'").Replace("\\n", "").Replace("USER:", "").Replace("\u0022", "\"");
                imagedescription.PreviewImage = image.ImagePath;
                imagedescription.StartOfScene = image.StartTime;
                imagedescription.EndOfScene = image.EndTime;

                imagedescriptions.Add(imagedescription);
            }

            return imagedescriptions;
        }
    }
}
