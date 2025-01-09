namespace UserContentIndexerAPI.Controllers.Services
{
    using Microsoft.Extensions.Logging;
    using UserContentIndexerAPI.Controllers.Interfaces;
    using UserContentIndexerAPI.Controllers.Models;

    public class DownloadService : IDownloadService
    {
        private readonly ILogger<DownloadService> logger;
        public DownloadService(ILogger<DownloadService> logger)
        {
            this.logger = logger;
        }

        private async Task<string> Downloadmodel(string requestUri, HttpClient httpClient)
        {
            var localFilePath = Directory.GetCurrentDirectory() + Path.Combine("Models/", requestUri.Split('/').Last());

            if (!File.Exists(localFilePath))
            {
                // Use SendAsync with HttpCompletionOption to start downloading immediately
                using (var response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead))
                {
                    response.EnsureSuccessStatusCode();

                    // Ensure directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(localFilePath));

                    // Download and save the file
                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                    {
                        await contentStream.CopyToAsync(fileStream);
                    }
                }
            }
            return localFilePath;
        }

        public async Task<string> DownloadModelAsync(ModelType modelType, string modelSize = "small")
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Configure the request to follow redirects and handle large files
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "WhisperNetDownloader");

                    switch (modelType)
                    {
                        case ModelType.Whisper:
                            this.logger.LogInformation($"Download Whisper Model {modelSize}");
                            var requestUri = $"https://huggingface.co/sandrohanea/whisper.net/resolve/main/classic/ggml-{modelSize.ToLower()}.bin";
                            return await this.Downloadmodel(requestUri, httpClient);
                        case ModelType.Llava_ggml:
                            this.logger.LogInformation($"Download Llava-Ggml Model");
                            //requestUri = $"https://huggingface.co/cjpais/llava-v1.6-vicuna-7b-gguf/resolve/main/llava-v1.6-vicuna-7b.Q4_K_M.gguf";
                            requestUri = $"https://huggingface.co/mys/ggml_llava-v1.5-7b/resolve/main/ggml-model-q4_k.gguf";
                            return await this.Downloadmodel(requestUri, httpClient);
                        case ModelType.Llava_mmproj:
                            this.logger.LogInformation($"Download Llava-Mmproj Model");
                            //requestUri = $"https://huggingface.co/cjpais/llava-v1.6-vicuna-7b-gguf/resolve/main/mmproj-model-f16.gguf";
                            requestUri = $"https://huggingface.co/mys/ggml_llava-v1.5-7b/resolve/main/mmproj-model-f16.gguf";
                            return await this.Downloadmodel(requestUri, httpClient);
                    }
                }
            }
            catch (HttpRequestException ex)
            {
                // Log network-related errors
                this.logger.LogError($"Network error downloading model {modelType}: {ex.Message}");
                throw;
            }
            catch (IOException ex)
            {
                // Log file system-related errors
                this.logger.LogError($"File system error saving model {modelType}: {ex.Message}");
                throw;
            }
            return default;
        }
    }
}
