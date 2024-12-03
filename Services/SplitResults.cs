using UserContentIndexer.Interfaces;
using UserContentIndexer.Models;

namespace UserContentIndexer.Services
{
    internal class SplitResults : ISplitResults
    {
        public string SplitDescription(string videoResult)
        {
            var description = videoResult.Trim().Replace("Description:\r\n", "").Split("\r\nTags:")[0];
            return description;
        }


        public List<Tags> SplitTags(string videoResults)
        {
            string[] videoImageBlocks = videoResults.Split(new string[] { "[VIDEOIMAGE]" }, StringSplitOptions.RemoveEmptyEntries);

            List<Tags> tagsList = new List<Tags>();

            foreach (string block in videoImageBlocks)
            {
                string tagsSection = block.Split(new string[] { "Tags:\r\n" }, StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                string[] tagsLines = tagsSection.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                Tags tagsObject = new Tags();
                foreach (string line in tagsLines)
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

            return tagsList;
        }
    }
}
