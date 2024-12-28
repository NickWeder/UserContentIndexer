namespace UserContentIndexer.Interfaces
{
    using UserContentIndexer.Models;
    public interface IImageAnalyzeService
    {
        Task<IList<ImageDescription>> AnalyzeImageAsync(IList<SceneInfo> imagePaths, ModelType modelType);
    }
}
