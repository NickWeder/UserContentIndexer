namespace UserContentIndexer.Interfaces
{
    public interface IAudioConverter
    {
        public string ConvertMp4ToWav(string inputFilePath);
        public void DeleteWav(string filePath);
    }
}
