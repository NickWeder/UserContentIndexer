namespace UserContentIndexer.Interfaces
{
    public interface IAudioService
    {
        Task<string> TranscribeAsync(string contentLink, string modelPath);
    }
}