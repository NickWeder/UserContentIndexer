namespace UserContentIndexerAPI.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Controllers.Models;
    using Microsoft.Extensions.Logging;
    using UserContentIndexerAPI.Controllers.Interfaces;
    using UserContentIndexerAPI.Controllers.Services;
    using UserContentIndexerAPI.Controllers.Utilities;

    [ApiController]
    [Route("[controller]")]
    public class UserContentIndexerController : ControllerBase
    {
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
        private readonly ILogger<UserContentIndexerController> _logger;

        public UserContentIndexerController(ILogger<UserContentIndexerController> logger)
        {
            this._logger = logger;
        }

        [HttpGet(Name = "GetUserContent")]
        public async Task<JsonStructure> Get([FromHeader] string videoPath)
        {
            try
            {
                var serviceProvider = ConfigureServices();
                var programProcessor = serviceProvider.GetService<ProgramProcessor>();
                return await programProcessor.Start(videoPath);
            }
            catch (Exception)
            {
                throw;
            }

        }
    }
}
