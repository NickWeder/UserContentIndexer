namespace UserContentIndexerAPI.Controllers.Interfaces
{
    using UserContentIndexerAPI.Controllers.Models;

    public interface IDownloadService
    {
        Task<string> DownloadModelAsync(ModelType modelType, string modelSize = "small");
    }
}
