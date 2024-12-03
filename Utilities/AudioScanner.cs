using System.Threading.Tasks;
using UserContentIndexer.Interfaces;

namespace UserContentIndexer.Utilities
{
    public class AudioScanner
    {
        private readonly IAudioService _audioService;

        public AudioScanner(IAudioService audioService)
        {
            _audioService = audioService;
        }

        public async Task<string> TranscribeAudioAsync(string contentLink, string modelPath)
        {
            return await _audioService.TranscribeAsync(contentLink, modelPath);
        }
    }
}