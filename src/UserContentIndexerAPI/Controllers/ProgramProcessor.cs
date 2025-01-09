namespace UserContentIndexerAPI.Controllers
{
    using System.Text.Json;
    using System.Diagnostics;
    using Microsoft.Extensions.Logging;
    using UserContentIndexerAPI.Controllers.Interfaces;
    using UserContentIndexerAPI.Controllers.Models;
    using UserContentIndexerAPI.Controllers.Utilities;

    internal class ProgramProcessor
    {
        private readonly ILogger<ProgramProcessor> logger;
        private readonly IAudioService audioService;
        private readonly SceneDetector sceneDetector;

        private readonly IImageAnalyzeService imageAnalyzeService;


        public ProgramProcessor(ILogger<ProgramProcessor> logger, IAudioService audioService, SceneDetector sceneDetector, IImageAnalyzeService imageAnalyzeService)
        {
            this.logger = logger;
            this.audioService = audioService;
            this.sceneDetector = sceneDetector;
            this.imageAnalyzeService = imageAnalyzeService;
        }

        public async Task<JsonStructure> Start(string videoPath)
        {
            this.logger.LogInformation("ProgramProcessor - Start");
            var sw1 = new Stopwatch();
            var sw2 = new Stopwatch();
            var sw3 = new Stopwatch();

            var jsonStructure = new JsonStructure();
            // Split video into relavent images
            sw1.Start();
            var images = this.sceneDetector.ProcessVideo(videoPath);
            sw1.Stop();

            // AI Processes
            jsonStructure.Videoname = videoPath;
            sw2.Start();
            jsonStructure.WhisperResults = await this.audioService.TranscribeAsync(videoPath);
            sw2.Stop();
            sw3.Start();
            jsonStructure.ImageDescriptions = await this.imageAnalyzeService.AnalyzeImageAsync(images, ModelType.Llava_ggml);
            sw3.Stop();

            this.logger.LogInformation($"ProgramProcessor - Time: {sw1.Elapsed + sw2.Elapsed + sw3.Elapsed}");
            this.logger.LogInformation($"Split in Scnees - Time: {sw1.Elapsed}");
            this.logger.LogInformation($"Audio transcibation - Time: {sw2.Elapsed}");
            this.logger.LogInformation($"Image Description - Time: {sw3.Elapsed}");
            this.logger.LogInformation("ProgramProcessor - End");

            return jsonStructure;
        }

    }
}
