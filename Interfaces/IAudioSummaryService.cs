namespace UserContentIndexer.Interfaces
{
    internal interface IAudioSummaryService
    {
        public Task<string> GenerateAudioSummaryAsync(string llamaModelPath, string trascription);
    }
}
