using LLama.Common;

namespace UserContentIndexer.Interfaces
{
    public interface ILanguageModelService
    {
        Task<string> GenerateTextAsync(string prompt, InferenceParams inferenceParams, string modelPath);
    }
}
