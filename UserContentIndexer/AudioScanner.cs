using Whisper.net;
using Whisper.net.Ggml;
using Whisper.net.Logger;
using UserContentIndexer.Models;

namespace UserContentIndexer
{
    internal partial class AudioScanner : IAudioScanner
    {

        public async Task<string> Whisper(string ContentLink, string modelPath)
        {          
            var whisper = new WhisperModel()
            {
                Type = GgmlType.Small,
                Name = modelPath,
                Encoder = "ggml-small-encoder"
            };
          
            LogProvider.Instance.OnLog += (level, message) =>
            {
                Console.Write($"{level}: {message}");
            };

            if (!Directory.Exists(whisper.Encoder))
            {
                // Note: The encoder directory needs to be extracted
                await WhisperGgmlDownloader.GetEncoderOpenVinoModelAsync(whisper.Type)
                                           .ExtractToPath(whisper.Encoder);
            }

            // This section creates the whisperFactory object which is used to create the processor object.
            using var whisperFactory = WhisperFactory.FromPath(whisper.Name);

            // This section creates the processor object which is used to process the audio file, it uses language `auto` to detect the language of the audio file.
            using var processor = whisperFactory.CreateBuilder()
                .WithLanguage("auto")
                .Build();

            using var fileStream = File.OpenRead(ContentLink);
            var text = "";
            // This section processes the audio file and prints the results (start time, end time and text) to the console.
            await foreach (var result in processor.ProcessAsync(fileStream))
            {
                text += result.Text;
                //Console.WriteLine($"{result.Start}->{result.End}: {result.Text}");
            }
            processor.Dispose();
            whisperFactory.Dispose();
            return text;
        }
       
    }


}
