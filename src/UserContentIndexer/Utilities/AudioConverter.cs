namespace UserContentIndexer.Utilities
{
    using NAudio.Wave;
    using NAudio.Lame;
    using UserContentIndexer.Interfaces;
    using Microsoft.Extensions.Logging;

    public class AudioConverter : IAudioConverter
    {
        private readonly ILogger<AudioConverter> logger;

        public AudioConverter(ILogger<AudioConverter> logger)
        {
            this.logger = logger;
        }

        public void DeleteWav(string filePath)
        {
            File.Delete(filePath);
        }
        public string ConvertMp4ToWav(string inputFilePath)
        {
            this.logger.LogInformation("Convert .Mp4 to .Wav");
            var outputFilePath = inputFilePath.Split('.')[0] + ".wav";
            try
            {
                if (!File.Exists(inputFilePath))
                {
                    this.logger.LogError("File not found:", inputFilePath);
                }

                var tempMp3Path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".mp3");

                using (var reader = new MediaFoundationReader(inputFilePath))
                using (var writer = new LameMP3FileWriter(tempMp3Path, reader.WaveFormat, 128))
                {
                    reader.CopyTo(writer);
                }

                using (var mp3Reader = new Mp3FileReader(tempMp3Path))
                {
                    var wavFormat = new WaveFormat(16000, 16, 1);
                    using (var resampler = new WaveFormatConversionStream(wavFormat, mp3Reader))
                    {
                        WaveFileWriter.CreateWaveFile(outputFilePath, resampler);
                    }
                }

                File.Delete(tempMp3Path);

                this.logger.LogInformation($"Conversion succesful: {outputFilePath}");
            }
            catch (Exception ex)
            {
                this.logger.LogError($"Exception: {ex}");
                throw;
            }
            this.logger.LogError($"Model saved in: {outputFilePath}");
            return outputFilePath;
        }
    }
}
