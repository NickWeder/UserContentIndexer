namespace UserContentIndexer
{

    internal class Downloader
    {
        public static async Task<string> DownloadModel(string modelType, string modelSize)
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
                            // Use SendAsync with HttpCompletionOption to start downloading immediately
                            using (var response = await httpClient.GetAsync(requestUri, HttpCompletionOption.ResponseHeadersRead))
                            {
                                response.EnsureSuccessStatusCode();

                                // Determine local file path for saving the model
                                var localFilePath = Path.Combine("Models/", $"ggml-{modelType}.bin");

                                // Ensure directory exists
                                Directory.CreateDirectory(Path.GetDirectoryName(localFilePath));

                                // Download and save the file
                                using (var contentStream = await response.Content.ReadAsStreamAsync())
                                using (var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                                {
                                    await contentStream.CopyToAsync(fileStream);
                                }

                                return localFilePath;
                            }                           
                        case "llama":
                            requestUri = $"https://huggingface.co/sandrohanea/whisper.net/resolve/main/classic/ggml-{modelSize}.bin";
                            return "localFilePath";
                        case "llava":
                            requestUri = $"https://huggingface.co/sandrohanea/whisper.net/resolve/main/classic/ggml-{modelSize}.bin";
                            return "localFilePath";
                    }
                }
                return null;
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
