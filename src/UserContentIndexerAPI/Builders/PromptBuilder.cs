namespace UserContentIndexer.Builders
{
    public class PromptBuilder
    {
        public static string BuildLlavaImageAnalyzingPrompt(string image)
        {
            var prompt = $"{{{image}}}\nUSER:\n" +
                File.ReadAllText("./Prompts/ImageAnalyzePrompt.txt") +
                "\nASSISTANT:\n";
            return prompt;
        }
    }
}
