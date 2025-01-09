namespace UserContentIndexerAPI.Controllers.Interfaces
{
    using UserContentIndexerAPI.Controllers.Models;

    public interface IImageAnalyzeService
    {
        Task<IList<ImageDescription>> AnalyzeImageAsync(IList<SceneInfo> imagePaths, ModelType modelType);
    }
}
