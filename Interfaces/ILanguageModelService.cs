using LLama.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserContentIndexer.Interfaces
{
    public interface ILanguageModelService
    {
        Task<string> GenerateTextAsync(string prompt, InferenceParams inferenceParams, string modelPath);
    }
}
