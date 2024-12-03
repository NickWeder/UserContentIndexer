namespace UserContentIndexer.Builders
{
    public class PromptBuilder
    {
        public string BuildVideoSummaryPrompt(List<string> videoResults)
        {
            var prompt = File.ReadAllText("./Prompts/SummaryzeVideoPrompt.txt");
            prompt += "\n\nVideoresults:\n\n";
            for(int i = 0; i < videoResults.Count; i++)
            {
                prompt += $"[VIDEOIMAGE]\n";
                prompt += videoResults[i] + "\n\n";
            }
            return prompt;
        }
        public string BuildAudioSummaryPrompt(string transcription)
        {
            var prompt = File.ReadAllText("./Prompts/SummaryzeAudioPrompt.txt");
            prompt += "\n\ntranscription:\n\n";
            prompt += transcription;
            return prompt;
        }
    }
}