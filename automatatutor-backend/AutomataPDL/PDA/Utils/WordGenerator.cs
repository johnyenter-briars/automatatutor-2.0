using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.Utils
{
    internal class WordGenerator
    {
        internal static IEnumerable<string> GenerateWordsUntilLength(int length, IEnumerable<char> alphabet)
        {
            Assertion.Assert(length >= 0, "the length of a word cannot be negative");
            IEnumerable<string> allWords = new List<string>() { string.Empty };
            IEnumerable<string> lastWords = new List<string>() { string.Empty };
            for (int i = 1; i <= length; i++)
            {
                lastWords = lastWords.SelectMany(word => alphabet.Select(k => word + k));
                allWords = allWords.Concat(lastWords);
            }
            return allWords.ToList();
        }
    }
}
