namespace UserContentIndexer
{
    interface IAudioScanner
    {

        public Task<string> Whisper(string ContentLink, string modelPath);

    }


}
