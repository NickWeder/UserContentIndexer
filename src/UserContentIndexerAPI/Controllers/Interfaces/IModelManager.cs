namespace UserContentIndexerAPI.Controllers.Interfaces
{
    using LLama;
    using Whisper.net;
    using Whisper.net.Ggml;
    public interface IModelManager
    {
        // rename to Context
        public Task<WhisperProcessor> LoadWhisperModelAsync(GgmlType ggmlType);
        public Task<LLavaWeights> LoadLlava();

        public void UnloadWhisperModel();


        public Task<LLamaContext> CreateContextAsync(int gpuLayerCount = -1);


        public Task<string> GenerateTextAsync(string prompt, string modelPath, int maxTokens = 1024, float temprature = 0.1f);
    }
}
