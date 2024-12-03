using UserContentIndexer.Interfaces;
using UserContentIndexer.Builders;
using LLama.Common;
using LLama.Sampling;

namespace UserContentIndexer.Services
{
    public class VideoSummaryService : IVideoSummaryService
    {
        private readonly ILanguageModelService _languageModelService;
        private readonly PromptBuilder _promptBuilder;

        public VideoSummaryService(ILanguageModelService languageModelService, PromptBuilder promptBuilder)
        {
            _languageModelService = languageModelService;
            _promptBuilder = promptBuilder;
        }

        public async Task<string> GenerateVideoSummaryAsync(string llamaModelPath, List<string> videoResults, string whisperResult)
        {
            var prompt = _promptBuilder.BuildVideoSummaryPrompt(videoResults, whisperResult);

            var inferenceParams = new InferenceParams
            {
                AntiPrompts = new List<string> { "User:" },
                MaxTokens = 2048,
                SamplingPipeline = new DefaultSamplingPipeline
                {
                    Temperature = 0.6f
                }
            };

            var summary = await _languageModelService.GenerateTextAsync(prompt, inferenceParams, llamaModelPath);
            return summary;
        }
    }
}