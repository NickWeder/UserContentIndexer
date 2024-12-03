namespace UserContentIndexer.Builders
{
    public class PromptBuilder
    {
        public string BuildVideoSummaryPrompt(List<string> videoResults, string whisperResult)
        {
            var prompt = File.ReadAllText("./Prompts/SummaryzeTextPrompt.txt");
            prompt += "\n\nVideoresults:\n\n";
            for(int i = 0; i < videoResults.Count; i++)
            {
                prompt += $"Videoimage {i}:\n";
                prompt += videoResults[i] + "\n\n";
            }
            prompt += "Audiotranscription:\n" + whisperResult;
            return prompt;
        }
    }
}