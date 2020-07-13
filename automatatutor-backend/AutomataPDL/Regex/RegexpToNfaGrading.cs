using Microsoft.Automata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutomataPDL
{
    
    public static class RegexpToNfaGrading
    {
        private static HashSet<string> expressions;
        private static HashSet<string> wrong;

        private static bool IsValidRegex(string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return false;
            if (pattern[0] == '|' || pattern[pattern.Length - 1] == '|')
                return false;
            if (pattern[0] == '(' && pattern[pattern.Length - 1] == ')')
                return false;

            try
            {
                System.Text.RegularExpressions.Regex.Match("", pattern);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        public static HashSet<String> GetSubexpressions(string regex)
        {
            var ret = new HashSet<String>();
            for (int i = 0; i < regex.Length; i++)
                for (int j = i; j < regex.Length; j++)
                {
                    var pattern = string.Format(@"^({0})$", regex.Substring(i, j - i + 1));
                    if (IsValidRegex(regex.Substring(i, j - i + 1)))
                        ret.Add(regex.Substring(i, j - i + 1));
                }
            return ret;
        }

        public static bool compareNfaToRegex(XElement regex, XElement alphabet, XElement attemptNfa)
        {            
            CharSetSolver solver = new CharSetSolver(BitWidth.BV64);
            var dfaCorrectPair = DFAUtilities.parseRegexFromXML(regex, alphabet, solver);
            var attempt = DFAUtilities.parseBlockFromXML(attemptNfa, alphabet, solver);

            var same = dfaCorrectPair.Second.IsEquivalentWith(attempt.Second, solver);
            return same;
        }

        public static Tuple<int, int, string> recursiveHelper(XElement regex, XElement alphabet, XElement attemptRoot)
        {
            var cleanRegex = XElement.Parse(DFAUtilities.RemoveAllNamespaces(regex.ToString())).Value.Trim();
            var subexpressions = GetSubexpressions(cleanRegex);

            int total = 1, correct = 0;
            string feedString = "";
            if (!compareNfaToRegex(regex, alphabet, attemptRoot))
            {
                if(!wrong.Contains(cleanRegex))
                    feedString += String.Format("<li> Incorrect definition of '{0}' </li>", cleanRegex);
                wrong.Add(cleanRegex);
            }
            else if (!expressions.Contains(cleanRegex))
                correct++;

            expressions.Add(cleanRegex);

            var states = attemptRoot.Element("stateSet");
            foreach (var s in states.Elements())
                if(s.Name == "block")
                {
                    var subreg = s.Attribute("regex").Value.Trim();
                    if (subexpressions.Contains(subreg))
                    {
                        var tup = recursiveHelper(XElement.Parse("<div>" + subreg + "</div>"), alphabet, s);
                        correct += tup.Item1;
                        total += tup.Item2;
                        feedString += tup.Item3;
                    }
                    else
                        feedString += String.Format("<li> '{0}' is not a subexpression of '{1}' </li>", subreg, cleanRegex);
                }
            return Tuple.Create(correct, total, feedString);
        }
        
        public static XElement getFeedback(XElement regex, XElement alphabet, XElement attemptNfa, XElement maxGrade)
        {
            var cleanAttempt = DFAUtilities.RemoveJustNamespaces(attemptNfa).Element("block");
            expressions = new HashSet<string>();
            wrong = new HashSet<string>();

            var feedString = "<ul>";
            var tup = recursiveHelper(regex, alphabet, cleanAttempt);
            feedString += tup.Item3 + "</ul>";

            var grade = 1.0 * int.Parse(maxGrade.Value) * tup.Item1 / expressions.Count;
            if (grade == int.Parse(maxGrade.Value))
                feedString = "Correct!";
            return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>{1}</feedback></div>", (int)(Math.Floor(grade)), feedString));
        }
    }
}
