using NAudio.Wave;
using NAudio.Lame;

namespace UserContentIndexer
{
    internal class AudioConverter
    {
        public static string ConvertMp4ToWav(string inputFilePath)
        {
            var outputFilePath = inputFilePath.Split('.')[0]+".wav";
            try
            {
                // Überprüfen, ob Eingabedatei existiert
                if (!File.Exists(inputFilePath))
                {
                    throw new FileNotFoundException("Die Eingabedatei wurde nicht gefunden.", inputFilePath);
                }

                // Temporäre MP3-Konvertierung als Zwischenschritt
                string tempMp3Path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".mp3");

                // MP4 zu MP3 konvertieren
                using (var reader = new MediaFoundationReader(inputFilePath))
                using (var writer = new LameMP3FileWriter(tempMp3Path, reader.WaveFormat, 128))
                {
                    reader.CopyTo(writer);
                }

                // MP3 zu WAV mit 16 kHz konvertieren
                using (var mp3Reader = new Mp3FileReader(tempMp3Path))
                {
                    var wavFormat = new WaveFormat(16000, 16, 1);
                    using (var resampler = new WaveFormatConversionStream(wavFormat, mp3Reader))
                    {
                        WaveFileWriter.CreateWaveFile(outputFilePath, resampler);
                    }
                }

                // Temporäre MP3-Datei löschen
                File.Delete(tempMp3Path);

                Console.WriteLine($"Konvertierung erfolgreich: {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Konvertierungsfehler: {ex.Message}");
            }
            return outputFilePath;
        }
    }


}
