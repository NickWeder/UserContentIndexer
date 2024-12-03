namespace UserContentIndexer.Interfaces
{
    public interface IVideoService
    {
        Task<List<string>> AnalyzeVideoAsync(string llamaModelPath, string llavaModelPath, List<string> imagePaths, string transcription);
    }
}