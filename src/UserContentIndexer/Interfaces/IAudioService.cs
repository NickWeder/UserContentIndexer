namespace UserContentIndexer.Interfaces
{
    using UserContentIndexer.Models;

    public interface IAudioService
    {
        public Task<IList<WhisperResult>> TranscribeAsync(string contentLink);
    }

}
