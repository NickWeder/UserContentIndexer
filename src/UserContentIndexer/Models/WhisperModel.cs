namespace UserContentIndexer.Models
{
    using Whisper.net.Ggml;
    public class WhisperModel
    {
        public GgmlType GgmlType { get; set; }
        public string Name { get; set; }
        public string Encoder { get; set; }
    }
}
