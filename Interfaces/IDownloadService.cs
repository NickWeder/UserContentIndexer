namespace UserContentIndexer.Interfaces
{
    public interface IDownloadService
    {
        Task<string> DownloadModelAsync(string modelType, string modelSize = "small");
    }
}