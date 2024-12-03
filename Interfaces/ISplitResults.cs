using UserContentIndexer.Models;

namespace UserContentIndexer.Interfaces
{
    internal interface ISplitResults
    {
        public List<Tags> SplitTags(string videoResults);
        public string SplitDescription(string videoResults);
    }
}
