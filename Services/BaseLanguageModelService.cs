using LLama.Common;
using LLama;

namespace UserContentIndexer.Services
{
    public abstract class BaseLanguageModelService
    {
        protected async Task<LLamaContext> CreateContextAsync(string modelPath, int gpuLayerCount = -1)
        {
            var parameters = new ModelParams(modelPath)
            {
                GpuLayerCount = gpuLayerCount,
            };
            using var model = await LLamaWeights.LoadFromFileAsync(parameters);
            return model.CreateContext(parameters);
        }
    }
}