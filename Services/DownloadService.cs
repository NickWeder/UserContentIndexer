using System.Net.Http;
using UserContentIndexer.Interfaces;

namespace UserContentIndexer.Services
{
    public class DownloadService : IDownloadService
    {
        public async Task<string> DownloadModelAsync(string modelType, string modelSize = "small")
        {
            var requestUri = "";
            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Configure the request to follow redirects and handle large files
                    httpClient.DefaultRequestHeaders.Add("User-Agent", "WhisperNetDownloader");

                    switch (modelType)
                    {
                        case "whisper":
                            requestUri = $"https://huggingface.co/sandrohanea/whisper.net/resolve/main/classic/ggml-{modelSize}.bin";
                            break;
                        case "llama":
                            //requestUri = $"https://huggingface.co/cjpais/llava-v1.6-vicuna-7b-gguf/resolve/main/llava-v1.6-vicuna-7b.Q4_K_M.gguf";
                            requestUri = $"https://huggingface.co/mys/ggml_llava-v1.5-7b/resolve/main/ggml-model-q4_k.gguf";
                            break;
                        case "llava":
                            //requestUri = $"https://huggingface.co/cjpais/llava-v1.6-vicuna-7b-gguf/resolve/main/mmproj-model-f16.gguf";
                            requestUri = $"https://huggingface.co/mys/ggml_llava-v1.5-7b/resolve/main/mmproj-model-f16.gguf";
                            break;
                    }

                    var localFilePath = Path.Combine("Models/", requestUri.Split('/').Last());

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
            }
            catch (HttpRequestException ex)
            {
                // Log network-related errors
                Console.WriteLine($"Network error downloading model {modelType}: {ex.Message}");
                throw;
            }
            catch (IOException ex)
            {
                // Log file system-related errors
                Console.WriteLine($"File system error saving model {modelType}: {ex.Message}");
                throw;
            }
        }
    }
}