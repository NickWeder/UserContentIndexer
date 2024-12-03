using LLama;
using LLama.Common;
using UserContentIndexer.Interfaces;

namespace UserContentIndexer.Services
{
    public class LanguageModelService : BaseLanguageModelService, ILanguageModelService
    {
        public async Task<string> GenerateTextAsync(string prompt, InferenceParams inferenceParams, string modelPath)
        {
            using var context = await CreateContextAsync(modelPath);
            var ex = new InteractiveExecutor(context);

            var generatedText = new System.Text.StringBuilder();
            await foreach (var text in ex.InferAsync(prompt, inferenceParams))
            {
                generatedText.Append(text);
            }
            return generatedText.ToString();
        }
    }
}