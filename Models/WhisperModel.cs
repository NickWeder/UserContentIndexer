using Whisper.net.Ggml;

namespace UserContentIndexer.Models
{
    public class WhisperModel : Model
    {
        public GgmlType GgmlType { get; set; }
        public string Encoder { get; set; }
    }
}