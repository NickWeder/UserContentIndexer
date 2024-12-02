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
            var whisperModelPath = await Downloader.DownloadModel("whisper", "small");
            var llavaModelPath = await Downloader.DownloadModel("llava");
            var llamaModelPath = await Downloader.DownloadModel("llama");
            var result = await audioscanner.Whisper(video, whisperModelPath);
            Console.WriteLine(result);
        }
    }
    interface IVideoScanner { }
    internal class VideoScanner : IVideoScanner { 
    
    
    
    
    }


}
