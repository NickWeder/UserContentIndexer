namespace UserContentIndexer.Interfaces
{
    public interface IVideoSummaryService
    {
        Task<string> GenerateVideoSummaryAsync(string llamaModelPath, List<string> videoResults, string whisperResult);
    }
}