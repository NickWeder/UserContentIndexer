using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UserContentIndexer.Builders;
using UserContentIndexer.Interfaces;
using UserContentIndexer.Services;
using UserContentIndexer.Utilities;

namespace UserContentIndexer
{
    class Program
    {
        internal const string VideoPath = "";
        static async Task Main(string[] args)
        {
            var serviceProvider = ConfigureServices();

            var sceneDetector = serviceProvider.GetService<SceneDetector>();
            var audioService = serviceProvider.GetService<IAudioService>();
            var videoService = serviceProvider.GetService<IVideoService>();
            var downloadService = serviceProvider.GetService<IDownloadService>();
            var videoSummaryService = serviceProvider.GetService<IVideoSummaryService>();

            var images = sceneDetector.ProcessVideo(VideoPath);

            var whisperModelPath = await downloadService.DownloadModelAsync("whisper", "small");
            var transcription = await audioService.TranscribeAsync(VideoPath, whisperModelPath);

            var llavaModelPath = await downloadService.DownloadModelAsync("llava");
            var llamaModelPath = await downloadService.DownloadModelAsync("llama");

            var analysisResults = await videoService.AnalyzeVideoAsync(llamaModelPath, llavaModelPath, images, transcription);

            var summary = await videoSummaryService.GenerateVideoSummaryAsync(llamaModelPath, analysisResults, transcription);

            foreach (var txt in analysisResults)
            {
                Console.WriteLine(txt);
            }
            Console.WriteLine(transcription);
            Console.WriteLine("Video Summary:\n" + summary);

            sceneDetector.DeleteCache();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<SceneDetector>();
            services.AddTransient<IAudioService, AudioService>();
            services.AddTransient<IDownloadService, DownloadService>();
            services.AddTransient<IVideoService, VideoService>();
            services.AddTransient<IVideoSummaryService, VideoSummaryService>();
            services.AddTransient<ILanguageModelService, LanguageModelService>();
            services.AddTransient<PromptBuilder>();

            return services.BuildServiceProvider();
        }
    }
}
