using System.Text.Json;
using UserContentIndexer.Interfaces;
using UserContentIndexer.Models;

namespace UserContentIndexer.Services
{
    internal class SaveResults : ISaveResults
    {
        public void SaveResultsJson(string videodescription, string audiodescription, List<Tags> tags, string filename) 
        {
            var file = $"{filename}.json";
            var results = new JsonStructure()
            {
                Videodescription = videodescription,
                Audiodescription = audiodescription,
                Tags = tags,
            };

            var json = JsonSerializer.Serialize(results);
            if (!File.Exists(file))
            {
                File.Create(file);
            }
            File.WriteAllText(file, json);
        }

    }
}
