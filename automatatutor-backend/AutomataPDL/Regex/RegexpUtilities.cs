using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL
{
    // Handles both backslash and double backslash
    public static class RegexpUtilities
    {
        // Translates Regex to .net syntax
        public static string toDotNet(this string regex)
        {
            return regex
                .Replace("\\\\epsilon", "(ε?)")
                .Replace("\\\\emptyset", "[^\\w\\W]")
                .Replace("\\\\emp", "[^\\w\\W]")
                .Replace("\\\\eps", "(ε?)")
                .Replace("\\\\e", "(ε?)")
                .Replace("\\epsilon", "(ε?)")
                .Replace("\\emptyset", "[^\\w\\W]")
                .Replace("\\emp", "[^\\w\\W]")
                .Replace("\\eps", "(ε?)")
                .Replace("\\e", "(ε?)");
        }
        public static string toConventional(this string regex)
        {
            return regex
                .Replace("\\\\epsilon", "ε")
                .Replace("\\\\emptyset", "∅")
                .Replace("\\\\emp", "∅")
                .Replace("\\\\eps", "ε")
                .Replace("\\\\e", "ε")
                .Replace("\\epsilon", "ε")
                .Replace("\\emptyset", "∅")
                .Replace("\\emp", "∅")
                .Replace("\\eps", "ε")
                .Replace("\\e", "ε");
        }
        public static string decodeEpsilon(this string word)
        {
            return word
                .Replace("\\\\epsilon", "")
                .Replace("\\\\eps", "")
                .Replace("\\\\e", "")
                .Replace("\\epsilon", "")
                .Replace("\\eps", "")
                .Replace("\\e", "");
        }
        public static string emptyToEpsilon(this string word)
        {
            return word.Length <= 0 ? "\\e" : word;
        }
    }
}
