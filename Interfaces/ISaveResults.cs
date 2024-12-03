using UserContentIndexer.Models;

namespace UserContentIndexer.Interfaces
{
    internal interface ISaveResults
    {
        public void SaveResultsJson(string videodescription, string audiodescription, List<Tags> tags, string filename);
    }
}
