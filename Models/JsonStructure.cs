namespace UserContentIndexer.Models
{
    internal class JsonStructure
    {
        public string? Videodescription { get; set; }
        public string? Audiodescription { get; set; }
        public List<Tags>? Tags { get; set; }
    }
    internal class Tags
    {
        public string? PrimarySubjectTags { get; set; }
        public string? AtmosphereTags { get; set; }
        public string? StyleTags { get; set; }
        public string? CameraAngleTags { get; set; }
    }
}

