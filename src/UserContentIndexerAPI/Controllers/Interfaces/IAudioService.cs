namespace UserContentIndexerAPI.Controllers.Interfaces
{
    using UserContentIndexerAPI.Controllers.Models;

    public interface IAudioService
    {
        public Task<IList<WhisperResult>> TranscribeAsync(string contentLink);
    }

}
