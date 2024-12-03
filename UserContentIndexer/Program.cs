using NAudio.Wave.SampleProviders;
using System.Diagnostics;

namespace UserContentIndexer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();
            var sd = new SceneDetector();
            var video = @"C:\Users\nickx\Downloads\testvideo.mp4";
            var videoResults = new List<string>();
            var images = sd.ProcessVideo(video);

            var audioscanner = new AudioScanner();
            video = AudioConverter.ConvertMp4ToWav(video);
            var whisperModelPath = await Downloader.DownloadModel("whisper", "small");
            var whisperResult = await audioscanner.Whisper(video, whisperModelPath);

            var llavaModelPath = await Downloader.DownloadModel("llava");
            var llamaModelPath = await Downloader.DownloadModel("llama");
            if (images != null)
            {

                videoResults = await VideoScanner.Llava(llamaModelPath, llavaModelPath, images);

            }
            sw.Stop();
            sd.DeleteChache();
            SummeryVideo summeryVideo = new SummeryVideo();
            await summeryVideo.SumVideoContext(llamaModelPath, videoResults, whisperResult);

            TimeSpan ts = sw.Elapsed;

            // Format and display the TimeSpan value.
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
                ts.Hours, ts.Minutes, ts.Seconds,
                ts.Milliseconds / 10);
            Console.WriteLine("RunTime " + elapsedTime);
        }
    }
}
