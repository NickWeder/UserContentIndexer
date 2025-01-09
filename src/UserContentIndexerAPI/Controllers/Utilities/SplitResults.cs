namespace UserContentIndexerAPI.Controllers.Utilities
{
    using UserContentIndexerAPI.Controllers.Models;

    public class SplitResults
    {
        public static string SplitDescription(string videoResult)
        {
            var description = videoResult.Trim().Replace("Description:\r\n", "").Split("\r\nTags:")[0];
            return description;
        }

        public static IList<Tags> SplitTags(string videoResult)
        {
            IList<Tags> tagsList = [];
            try
            {
                var tagsSections = videoResult.Split("\nTags:");

                if (tagsSections.Length == 2)
                {
                    var tagSection = tagsSections[1];
                    var tagsLines = tagSection.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

                    var tagsObject = new Tags();
                    foreach (var line in tagsLines)
                    {
                        if (line.StartsWith("- Primary Subject Tags: "))
                        {
                            tagsObject.PrimarySubjectTags = line.Replace("- Primary Subject Tags: ", "").Trim();
                        }
                        else if (line.StartsWith("- Atmosphere Tags: "))
                        {
                            tagsObject.AtmosphereTags = line.Replace("- Atmosphere Tags: ", "").Trim();
                        }
                        else if (line.StartsWith("- Style Tags: "))
                        {
                            tagsObject.StyleTags = line.Replace("- Style Tags: ", "").Trim();
                        }
                        else if (line.StartsWith("- Camera Angle: "))
                        {
                            tagsObject.CameraAngleTags = line.Replace("- Camera Angle: ", "").Trim();
                        }
                    }
                    tagsList.Add(tagsObject);
                }
            }
            catch (Exception) { throw; }
            return tagsList;
        }
    }
}
