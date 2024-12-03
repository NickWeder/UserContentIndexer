using System.Text.Json;
using UserContentIndexer.Interfaces;
using UserContentIndexer.Models;

namespace UserContentIndexer.Services
{
    internal class SaveResults : ISaveResults
    {
        public void SaveResultsJson(string videodescription, string audiodescription, List<Tags> tags, string filename) 
        {
            var file = $"{filename.Split('.')[0]}.json";
            var results = new JsonStructure()
            {
                Videodescription = videodescription,
                Audiodescription = audiodescription,
                Tags = tags,
            };
            var json = JsonSerializer.Serialize(results);
            File.WriteAllText(file, json);
        }

    }
}
