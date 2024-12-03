using UserContentIndexer.Interfaces;
using UserContentIndexer.Models;
using UserContentIndexer.Utilities;
using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.Logger;

namespace UserContentIndexer.Services
{
    public class AudioService : IAudioService
    {
        public async Task<string> TranscribeAsync(string contentLink, string modelPath)
        {
            var audioConverter = new AudioConverter();
            if (!contentLink.Contains(".wav"))
            {
                contentLink = audioConverter.ConvertMp4ToWav(contentLink);
            }
            var whisper = new WhisperModel()
            {
                GgmlType = GgmlType.Small,
                Name = modelPath,
                Encoder = "ggml-small-encoder"
            };

            LogProvider.Instance.OnLog += (level, message) =>
            {
                Console.Write($"{level}: {message}");
            };

            if (!Directory.Exists(whisper.Encoder))
            {
                await WhisperGgmlDownloader.GetEncoderOpenVinoModelAsync(whisper.GgmlType)
                                           .ExtractToPath(whisper.Encoder);
            }

            using var whisperFactory = WhisperFactory.FromPath(whisper.Name);


            using var processor = whisperFactory.CreateBuilder()
                .WithLanguage("auto")
                .Build();

            using var fileStream = File.OpenRead(contentLink);
            var text = "";

            await foreach (var result in processor.ProcessAsync(fileStream))
            {
                if (!result.Text.Contains('[') || !result.Text.Contains(']'))
                {
                    text += result.Text;
                }
            }
            fileStream.Close();
            audioConverter.DeleteWav(contentLink);
            processor.Dispose();
            whisperFactory.Dispose();
            return text;
        }
    }
}