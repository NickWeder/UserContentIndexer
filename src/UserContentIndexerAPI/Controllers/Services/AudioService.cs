namespace UserContentIndexerAPI.Controllers.Services
{
    using System.Text;
    using Microsoft.Extensions.Logging;
    using UserContentIndexerAPI.Controllers.Interfaces;
    using UserContentIndexerAPI.Controllers.Models;
    using Whisper.net.Ggml;

    public class AudioService : IAudioService
    {
        private readonly IAudioConverter audioConverter;
        private readonly IModelManager modelManager;
        private readonly ILogger<AudioService> logger;

        public AudioService(IAudioConverter audioConverter, IModelManager modelManager, ILogger<AudioService> logger)
        {
            this.audioConverter = audioConverter;
            this.modelManager = modelManager;
            this.logger = logger;
        }

        public async Task<IList<WhisperResult>> TranscribeAsync(string contentLink)
        {

            var whisperResults = new List<WhisperResult>();
            this.logger.LogInformation("Loading Whisper Model");
            var processor = await this.modelManager.LoadWhisperModelAsync(GgmlType.Medium);

            if (!contentLink.EndsWith(".wav"))
            {
                this.logger.LogInformation("Convert file to .Wav");
                contentLink = this.audioConverter.ConvertMp4ToWav(contentLink);
                this.logger.LogInformation("Convert succesful");
            }

            using var fileStream = File.OpenRead(contentLink);

            try
            {
                this.logger.LogInformation("Start Transcribing:");
                await foreach (var result in processor.ProcessAsync(fileStream))
                {
                    var whisperResult = new WhisperResult();
                    if (!result.Text.Contains('[') || !result.Text.Contains(']'))
                    {
                        whisperResult.Text = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(result.Text.Trim())).Replace("\u0027", "'").Replace("\\n", "").Replace("USER:", "").Replace("\u0022", "\"");
                        whisperResult.Start = result.Start;
                        whisperResult.End = result.End;
                    }
                    whisperResults.Add(whisperResult);
                    this.logger.LogInformation($"Text: {whisperResult.Text} Start time: {whisperResult.Start} End time: {whisperResult.End}");
                }
                this.logger.LogInformation("Transcribing succesful");
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Transcribing failed with exception: {ex}");
            }
            fileStream.Close();
            this.audioConverter.DeleteWav(contentLink);
            this.modelManager.UnloadWhisperModel();

            return whisperResults;
        }
    }
}
