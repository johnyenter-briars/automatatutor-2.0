using Microsoft.Automata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutomataPDL
{
    public static class EquivalencyClasses
    {
        private static HashSet<char> parseAlphabet(XElement xAlphabet)
        {
            xAlphabet = DFAUtilities.RemoveJustNamespaces(xAlphabet);
            HashSet<char> alphabet = new HashSet<char>();
            foreach (var child in xAlphabet.Elements("symbol"))
            {
                char element = Convert.ToChar(child.Value.Trim());
                alphabet.Add(element);
            }
            return alphabet;
        }

        private static string regexConverterHelper(string reg, string between)
        {
            string ret = "";
            for (int i = 0; i < reg.Length; i++)
            {
                char c = reg[i];
                if (c == '(')
                {
                    int opened = 1;
                    int len = 0;
                    for (int j = i + 1; j < reg.Length; j++)
                    {
                        char d = reg[j];
                        if (d == '(')
                            opened++;
                        if (d == ')')
                            opened--;
                        if (opened == 0)
                        {
                            len = j - i + 1;
                            break;
                        }
                    }
                    string childret = regexConverterHelper(reg.Substring(i + 1, len - 2), "");
                    if(childret.Length > 1)
                        childret = '(' + childret +')';
                    ret += childret;
                    i += len - 1;
                }
                else if (c == '[')
                {
                    int opened = 1;
                    int len = 0;
                    for (int j = i + 1; j < reg.Length; j++)
                    {
                        char d = reg[j];
                        if (d == '[')
                            opened++;
                        if (d == ']')
                            opened--;
                        if (opened == 0)
                        {
                            len = j - i + 1;
                            break;
                        }
                    }
                    string childRet = regexConverterHelper(reg.Substring(i + 1, len - 2), "|");
                    childRet = '(' + childRet.Substring(0, childRet.Length - 1) + ')';
                    ret += childRet;
                    i += len - 1;
                }
                else
                    ret += c + between;
            }
            return ret;
        }

        private static string regexToTraditional(string sharp)
        {
            var trimmed = sharp.Substring(1, sharp.Length - 2);
            string ret = regexConverterHelper(trimmed, "");
            if(ret.Length > 2)
                ret = ret.Substring(1, ret.Length - 2);
            if (ret.Length == 0)
                return "the empty language";
            return ret;
        }

        public static XElement getTwoWordsInstructorFeedback(XElement regex, XElement xAlphabet, XElement first, XElement second)
        {
            string firstString = (XElement.Parse(DFAUtilities.RemoveAllNamespaces(first.ToString()))).Value.Trim();
            string secondString = (XElement.Parse(DFAUtilities.RemoveAllNamespaces(second.ToString()))).Value.Trim();
            HashSet<char> alphabet = parseAlphabet(xAlphabet);

            CharSetSolver solver = new CharSetSolver(BitWidth.BV64);
            var dfaPair = DFAUtilities.parseRegexFromXML(regex, xAlphabet, solver);
            var dfa = dfaPair.Second.Determinize(solver).Minimize(solver);

            firstString = firstString.decodeEpsilon();
            secondString = secondString.decodeEpsilon();
            int firstState = DFAUtilities.GetStateAfterString(dfa.InitialState, firstString, dfa, solver);
            int secondState = DFAUtilities.GetStateAfterString(dfa.InitialState, secondString, dfa, solver);
            bool areEquivalent = (firstState == secondState);

            if (areEquivalent)
            {
                var suffixDfa = Automaton<BDD>.Create(firstState, dfa.GetFinalStates(), dfa.GetMoves());
                var suffixRegex = solver.ConvertToRegex(suffixDfa);
                suffixRegex = regexToTraditional(suffixRegex);
                return XElement.Parse(String.Format("<div><feedback> The words '{0}' and '{1}' are equivalent. The language of suffixes is '{2}'</feedback></div>", firstString, secondString, suffixRegex));
            }
            else
            {
                var shortestDiff = DFAUtilities.GetDifferentiatingWord(firstState, secondState, dfa, alphabet, solver).Second;
                return XElement.Parse(String.Format("<div><feedback> The words '{0}' and '{1}' are NOT equivalent. The shortest differentiating word is '{2}'</feedback></div>", firstString, secondString, shortestDiff));
            }
        }

        public static XElement getSameFeedback(XElement regex, XElement xAlphabet, XElement first, XElement second, XElement notEquivalent, XElement reason, XElement maxGrade)
        {
            string firstString = (XElement.Parse(DFAUtilities.RemoveAllNamespaces(first.ToString()))).Value.Trim().decodeEpsilon();
            string secondString = (XElement.Parse(DFAUtilities.RemoveAllNamespaces(second.ToString()))).Value.Trim().decodeEpsilon();
            string reasonString = (XElement.Parse(DFAUtilities.RemoveAllNamespaces(reason.ToString()))).Value.Trim();
            bool areEquivalentAttempt = int.Parse(notEquivalent.Value) == 1 ? false : true;
            int maxG = int.Parse(maxGrade.Value);
            HashSet<char> alphabet = parseAlphabet(xAlphabet);

            CharSetSolver solver = new CharSetSolver(BitWidth.BV64);
            var dfaPair = DFAUtilities.parseRegexFromXML(regex, xAlphabet, solver);
            var dfa = dfaPair.Second.Minimize(solver);

            int firstState = DFAUtilities.GetStateAfterString(dfa.InitialState, firstString, dfa, solver);
            int secondState = DFAUtilities.GetStateAfterString(dfa.InitialState, secondString, dfa, solver);
            bool areEquivalent = (firstState == secondState);

            if(areEquivalent != areEquivalentAttempt)
            {
                return XElement.Parse(string.Format("<div><grade>0</grade><feedback>Wrong equivalency assesment!</feedback></div>"));
            }
            else if (areEquivalent)
            {
                try
                {
                    var suffixDfa = Automaton<BDD>.Create(firstState, dfa.GetFinalStates(), dfa.GetMoves());
                    var reasonDfaPair = DFAUtilities.parseRegexFromXML(reason, xAlphabet, solver);
                    var dfaGradingFeedback = DFAGrading.GetGrade(suffixDfa, reasonDfaPair.Second, alphabet, solver, 1500, maxG, FeedbackLevel.Minimal, false, false, true);
                    var feedString = "<div>";
                    foreach (var feed in dfaGradingFeedback.Second)
                        feedString += string.Format("{0}<br />", feed);
                    feedString += "</div>";
                    if (maxG == dfaGradingFeedback.First)
                        return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>Correct!</feedback></div>", maxG));
                    else
                        return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>{1}</feedback></div>", Math.Max((int)(maxG / 3), dfaGradingFeedback.First), feedString));
                }
                catch (PDLException pdlex)
                {
                    return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>Parsing Error: {1}</feedback></div>", (int)(maxG / 3), pdlex.Message));
                }
            }
            else
            {
                // Finding a shortest differentiating word
                var shortestDiff = DFAUtilities.GetDifferentiatingWord(firstState, secondState, dfa, alphabet, solver).Second;
                reasonString = reasonString.decodeEpsilon();
                int endFirst = DFAUtilities.GetStateAfterString(firstState, reasonString, dfa, solver);
                int endSecond = DFAUtilities.GetStateAfterString(secondState, reasonString, dfa, solver);
                bool c1 = dfa.GetFinalStates().Contains(endFirst);
                bool c2 = dfa.GetFinalStates().Contains(endSecond);
                if (c1 ^ c2)
                    return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>Correct!</feedback></div>", maxG));
                else
                {
                    string feedString = string.Format("The words '{0}' and '{2}' are both {1} by the language", (firstString + reasonString).emptyToEpsilon(), c1 ? "accepted" : "not accepted", (secondString + reasonString).emptyToEpsilon());
                    return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>{1}</feedback></div>", (int)(maxG / 3), feedString));
                }
            }
        }

        public static XElement getShortestFeedback(XElement regex, XElement xAlphabet, XElement representative, XElement attemptShortest, XElement maxGrade)
        {
            int maxG = int.Parse(maxGrade.Value);
            CharSetSolver solver = new CharSetSolver(BitWidth.BV64);
            var dfaPair = DFAUtilities.parseRegexFromXML(regex, xAlphabet, solver);
            var dfa = dfaPair.Second.Minimize(solver);

            string representativeString = (XElement.Parse(DFAUtilities.RemoveAllNamespaces(representative.ToString()))).Value.Trim();
            representativeString = representativeString.decodeEpsilon();
            string attemptShortestString = (XElement.Parse(DFAUtilities.RemoveAllNamespaces(attemptShortest.ToString()))).Value.Trim();
            attemptShortestString = attemptShortestString.decodeEpsilon();

            int representativeState = DFAUtilities.GetStateAfterString(dfa.InitialState, representativeString, dfa, solver);
            int attemptShortestState = DFAUtilities.GetStateAfterString(dfa.InitialState, attemptShortestString, dfa, solver);

            xAlphabet = XElement.Parse(DFAUtilities.RemoveAllNamespaces(xAlphabet.ToString()));
            HashSet<char> alphabet = parseAlphabet(xAlphabet);

            string correctShortest = DFAUtilities.GetRepresentative(representativeState, dfa, alphabet, solver);
            if (correctShortest.Equals(attemptShortestString))
            {
                return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>Correct!</feedback></div>", maxG));
            }
            else if(attemptShortestState == representativeState)
            {
                if (representativeString.Equals(attemptShortestString))
                    return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>Please provide a different word than the given one</feedback></div>", 0));
                else if(attemptShortestString.Length > correctShortest.Length)
                    return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>The word '{1}' is in the same equivalence class as '{2}' but there exists a shorter one</feedback></div>", (int)(maxG * 2 / 3), attemptShortestString.emptyToEpsilon(), representativeString.emptyToEpsilon()));
                else
                    return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>The word '{1}' is in the same equivalence class as '{2}' but there exists a <strong>lexicographically smaller</strong> one</feedback></div>", (int)(maxG * 4 / 5), attemptShortestString.emptyToEpsilon(), representativeString.emptyToEpsilon()));
            }
            else
            {
                string feedString = "Incorrect!";//"The Correct Answer is '" + correctShortest + "'";
                return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>{1}</feedback></div>", 0, feedString));
            }
        }

        private static string alphabetToString(HashSet<char> alphabet)
        {
            string alphabetString = "{";
            foreach (char c in alphabet)
                alphabetString += c + ",";
            if (alphabetString.Length > 1)
                alphabetString = alphabetString.Remove(alphabetString.Length - 1);
            return alphabetString + "}";
        }

        public static XElement getWordsFeedback(XElement regex, XElement xAlphabet, XElement representative, XElement wordsIn, XElement maxGrade)
        {
            //read inputs
            int maxG = int.Parse(maxGrade.Value);
            List<string> wordsInList = new List<String>();
            HashSet<string> usedWords = new HashSet<string>();
            foreach (var wordElement in wordsIn.Elements())
            {
                string w = wordElement.Value;
                if (w.Length > 75) w = w.Substring(0, 75); //limit word length
                w = w.decodeEpsilon();
                wordsInList.Add(w);
            }

            HashSet<char> alphabet = parseAlphabet(xAlphabet);
            string alphabetString = alphabetToString(alphabet);
            CharSetSolver solver = new CharSetSolver(BitWidth.BV64);
            var dfaPair = DFAUtilities.parseRegexFromXML(regex, xAlphabet, solver);
            var dfa = dfaPair.Second.Minimize(solver);

            string representativeString = (XElement.Parse(DFAUtilities.RemoveAllNamespaces(representative.ToString()))).Value.Trim();
            representativeString = representativeString.decodeEpsilon();
            int representativeState = DFAUtilities.GetStateAfterString(dfa.InitialState, representativeString, dfa, solver);

            string feedString = "<ul>";
            int correct = 0;
            int total = wordsInList.Count;
            usedWords.Add(representativeString);
            foreach (string s in wordsInList)
            {
                //TODO: Check string only contains valid characters
                if (usedWords.Contains(s))
                {
                    if(s.Equals(representativeString))
                        feedString += String.Format("<li> The word '{0}' was provided by the instructor </li>", s.emptyToEpsilon());
                    else
                        feedString += String.Format("<li> The word '{0}' was already used </li>", s.emptyToEpsilon());
                    continue;
                }
                bool overValidAlphabet = true;
                foreach (char c in s)
                    if (!alphabet.Contains(c))
                        overValidAlphabet = false;
                if (!overValidAlphabet)
                {
                    feedString += String.Format("<li> The word '{0}' is not over the alphabet {1} </li>", s.emptyToEpsilon(), alphabetString);
                    continue;
                }
                usedWords.Add(s);
                int wordState = DFAUtilities.GetStateAfterString(dfa.InitialState, s, dfa, solver);
                if (wordState == representativeState)
                    correct++;
                else
                    feedString += String.Format("<li> The word '{0}' is not in the same equivalence class as '{1}' </li>", s.emptyToEpsilon(), representativeString.emptyToEpsilon());
            }

            feedString += "</ul>";
            var grade = 1.0 * int.Parse(maxGrade.Value) * correct / total;
            if (grade == maxG)
                feedString = "Correct!";
            return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>{1}</feedback></div>", (int)(Math.Ceiling(grade)), feedString));
        }
    }
}
