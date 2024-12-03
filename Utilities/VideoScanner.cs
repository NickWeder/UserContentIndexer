using System.Collections.Generic;
using UserContentIndexer.Interfaces;

namespace UserContentIndexer.Utilities
{
    public class VideoScanner
    {
        private readonly IVideoService _videoService;

        public VideoScanner(IVideoService videoService)
        {
            _videoService = videoService;
        }

        public async Task<List<string>> AnalyzeVideoAsync(string llamaModelPath, string llavaModelPath, List<string> imagePaths, string transcription)
        {
            return await _videoService.AnalyzeVideoAsync(llamaModelPath, llavaModelPath, imagePaths, transcription);
        }
    }
}