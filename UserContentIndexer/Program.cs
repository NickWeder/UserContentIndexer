namespace UserContentIndexer
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
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
            sd.DeleteChache();



            Console.WriteLine("VideoResults: ");
            foreach (var videoResult in videoResults)
            {
                Console.WriteLine(videoResult);
            }
            Console.WriteLine("WhisperResult: ");
            Console.WriteLine(whisperResult);

        }
    }
}
