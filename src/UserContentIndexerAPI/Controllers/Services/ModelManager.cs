namespace UserContentIndexerAPI.Controllers.Services
{
    using Whisper.net;
    using Whisper.net.Ggml;
    using LLama.Common;
    using LLama;
    using LLama.Sampling;
    using UserContentIndexerAPI.Controllers.Interfaces;
    using UserContentIndexerAPI.Controllers.Models;

    public class ModelManager : IModelManager
    {
        private WhisperFactory whisperFactory;
        private readonly IDownloadService downloadService;

        public ModelManager(IDownloadService downloadService)
        {
            this.downloadService = downloadService;
        }

        public async Task<LLavaWeights> LoadLlava()
        {
            var llavammproj = await this.downloadService.DownloadModelAsync(ModelType.Llava_mmproj);
            return await LLavaWeights.LoadFromFileAsync(llavammproj);
        }

        public async Task<LLamaWeights> LoadLlama(string modelPath, int gpuLayerCount = -1)
        {
            var parameters = new ModelParams(modelPath)
            {
                GpuLayerCount = gpuLayerCount,
            };
            return await LLamaWeights.LoadFromFileAsync(parameters);
        }

        public async Task<LLamaContext> CreateContextAsync(int gpuLayerCount = -1)
        {
            var modelPath = await this.downloadService.DownloadModelAsync(ModelType.Llava_ggml);
            var parameters = new ModelParams(modelPath)
            {
                GpuLayerCount = gpuLayerCount,
            };
            return (await this.LoadLlama(modelPath)).CreateContext(parameters);
        }

        public async Task<string> GenerateTextAsync(string prompt, string modelPath, int maxTokens = 1024, float temprature = 0.1f)
        {
            var inferenceParams = new InferenceParams
            {
                AntiPrompts = ["User:"],
                MaxTokens = maxTokens,
                SamplingPipeline = new DefaultSamplingPipeline
                {
                    Temperature = temprature
                }
            };

            using var context = await this.CreateContextAsync();
            var exe = new InteractiveExecutor(context);

            var generatedText = new System.Text.StringBuilder();
            await foreach (var text in exe.InferAsync(prompt, inferenceParams))
            {
                generatedText.Append(text);
            }
            return generatedText.ToString();
        }

        public async Task<WhisperProcessor> LoadWhisperModelAsync(GgmlType ggmlType)
        {
            if (this.whisperFactory == null)
            {
                var modelPath = await this.downloadService.DownloadModelAsync(ModelType.Whisper, ggmlType.ToString());
                this.whisperFactory = WhisperFactory.FromPath(modelPath);
            }
            return this.whisperFactory.CreateBuilder()
                    .WithLanguage("auto")
                    .Build();
        }

        public void UnloadWhisperModel()
        {
            this.whisperFactory?.Dispose();
            this.whisperFactory = null;
        }
    }
}
