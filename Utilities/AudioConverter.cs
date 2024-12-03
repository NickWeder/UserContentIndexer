using NAudio.Wave;
using NAudio.Lame;
using UserContentIndexer.Interfaces;

namespace UserContentIndexer.Utilities
{
    public class AudioConverter : IAudioConverter
    {
        public void DeleteWav(string filePath)
        {
            File.Delete(filePath);
        }
        public string ConvertMp4ToWav(string inputFilePath)
        {
            var outputFilePath = inputFilePath.Split('.')[0]+".wav";
            try
            {
                if (!File.Exists(inputFilePath))
                {
                    throw new FileNotFoundException("Die Eingabedatei wurde nicht gefunden.", inputFilePath);
                }

                string tempMp3Path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".mp3");

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

                Console.WriteLine($"Konvertierung erfolgreich: {outputFilePath}");
            }
            catch (Exception)
            {
                throw;
            }
            return outputFilePath;
        }
    }
}