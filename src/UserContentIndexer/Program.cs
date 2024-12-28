namespace UserContentIndexer
{
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using UserContentIndexer.Interfaces;
    using UserContentIndexer.Services;
    using UserContentIndexer.Utilities;
    public class Program
    {
        //TODO: change cache to full local path

        static async Task Main(string[] args)
        {
            // implementation of the services
            try
            {
                var serviceProvider = ConfigureServices();
                var programProcessor = serviceProvider.GetService<ProgramProcessor>();
                await programProcessor.Start(args[0]);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            _ = services.AddLogging(loggerBuilder =>
            {
                _ = loggerBuilder.ClearProviders();
                _ = loggerBuilder.AddConsole();
            });

            // Register services
            services.AddTransient<SceneDetector>();
            services.AddTransient<IAudioService, AudioService>();
            services.AddTransient<IDownloadService, DownloadService>();
            services.AddTransient<IImageAnalyzeService, ImageAnalyzeService>();
            services.AddTransient<IModelManager, ModelManager>();
            services.AddTransient<IAudioConverter, AudioConverter>();

            services.AddSingleton<ProgramProcessor>();

            return services.BuildServiceProvider();
        }
    }
}
