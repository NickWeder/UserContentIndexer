namespace UserContentIndexer.Models
{
    public class JsonStructure
    {
        public string Videoname { get; set; }
        public IList<ImageDescription>? ImageDescriptions { get; set; }
        public IList<WhisperResult>? WhisperResults { get; set; }
    }
    public class WhisperResult
    {
        public string Text { get; set; }
        public TimeSpan Start { get; set; }
        public TimeSpan End { get; set; }
    }

    public class ImageDescription
    {
        public string? Videodescription { get; set; }
        public IList<Tags>? Tags { get; set; }
        public string? PreviewImage { get; set; }
        public TimeSpan? StartOfScene { get; set; }
        public TimeSpan? EndOfScene { get; set; }
    }
    public class Tags
    {
        public string? PrimarySubjectTags { get; set; }
        public string? AtmosphereTags { get; set; }
        public string? StyleTags { get; set; }
        public string? CameraAngleTags { get; set; }
    }
}

