using Microsoft.Extensions.DependencyInjection;
using UserContentIndexer.Builders;
using UserContentIndexer.Interfaces;
using UserContentIndexer.Services;
using UserContentIndexer.Utilities;

namespace UserContentIndexer
{
    class Program
    {

        // Add here your Video, prefered MP4 format
        internal const string VideoPath = "";


        static async Task Main(string[] args)
        {
            var serviceProvider = ConfigureServices();

            var sceneDetector = serviceProvider.GetService<SceneDetector>();
            var audioService = serviceProvider.GetService<IAudioService>();
            var videoService = serviceProvider.GetService<IVideoService>();
            var downloadService = serviceProvider.GetService<IDownloadService>();
            var videoSummaryService = serviceProvider.GetService<IVideoSummaryService>();
            var audioSummaryService = serviceProvider.GetService<IAudioSummaryService>();
            var splitResults = serviceProvider.GetService<ISplitResults>();
            var saveResults = serviceProvider.GetService<ISaveResults>();

            var images = sceneDetector.ProcessVideo(VideoPath);

            var whisperModelPath = await downloadService.DownloadModelAsync("whisper", "small");
            var transcription = await audioService.TranscribeAsync(VideoPath, whisperModelPath);

            var llavaModelPath = await downloadService.DownloadModelAsync("llava");
            var llamaModelPath = await downloadService.DownloadModelAsync("llama");

            var analysisResults = await videoService.AnalyzeVideoAsync(llamaModelPath, llavaModelPath, images, transcription);

            var videoSummary = await videoSummaryService.GenerateVideoSummaryAsync(llamaModelPath, analysisResults);
            var audioSummary = await audioSummaryService.GenerateAudioSummaryAsync(llamaModelPath, transcription);
            var tags = splitResults.SplitTags(videoSummary);
            var description = splitResults.SplitDescription(videoSummary);
            saveResults.SaveResultsJson(description, audioSummary, tags, VideoPath);

            sceneDetector.DeleteCache();
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<SceneDetector>();
            services.AddTransient<IAudioService, AudioService>();
            services.AddTransient<IAudioSummaryService, AudioSummaryService>();
            services.AddTransient<IDownloadService, DownloadService>();
            services.AddTransient<IVideoService, VideoService>();
            services.AddTransient<IVideoSummaryService, VideoSummaryService>();
            services.AddTransient<ILanguageModelService, LanguageModelService>();
            services.AddTransient<ISaveResults, SaveResults>();
            services.AddTransient<ISplitResults, SplitResults>();
            services.AddTransient<PromptBuilder>();

            return services.BuildServiceProvider();
        }
    }
}
