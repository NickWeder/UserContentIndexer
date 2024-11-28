using System;

namespace UserContentIndexer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var video = @"C:\Users\nickx\Downloads\ImmortalityKilledTheLich.wav";
            var audioscanner = new AudioScanner();
            video = AudioConverter.ConvertMp4ToWav(video);
            var modelPath = await Downloader.DownloadModel("small");
            var result = await audioscanner.Whisper(video, modelPath);
            Console.WriteLine(result);
        }
    }
    interface IVideoScanner { }
    internal class VideoScanner : IVideoScanner { 
    
    
    
    
    }


}
