using LLama.Common;
using LLama.Sampling;
using UserContentIndexer.Builders;
using UserContentIndexer.Interfaces;


namespace UserContentIndexer.Services
{
    internal class AudioSummaryService : IAudioSummaryService
    {
        private readonly ILanguageModelService _languageModelService;
        private readonly PromptBuilder _promptBuilder;

        public AudioSummaryService(ILanguageModelService languageModelService, PromptBuilder promptBuilder)
        {
            _languageModelService = languageModelService;
            _promptBuilder = promptBuilder;
        }

        public async Task<string> GenerateAudioSummaryAsync(string llamaModelPath, string trascription)
        {
            var prompt = _promptBuilder.BuildAudioSummaryPrompt(trascription);

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
