namespace UserContentIndexer.Interfaces
{
    using UserContentIndexer.Models;
    public interface IDownloadService
    {
        Task<string> DownloadModelAsync(ModelType modelType, string modelSize = "small");
    }
}
