using Whisper.net.Ggml;

namespace UserContentIndexer.Models
{
    internal class WhisperModel : Model
    {
        public GgmlType Type { get; set; }
        public string Encoder { get; set; }
    }


}
