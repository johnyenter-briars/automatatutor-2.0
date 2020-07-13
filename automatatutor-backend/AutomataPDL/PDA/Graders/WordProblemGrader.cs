using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.Utils;

namespace AutomataPDL.PDA.Graders
{
    public static class WordProblemGrader
    {
        const int maximumWordLength = 30;

        const string ErrorWordMoreThanOnce = "The word \"{0}\" was used more than once";
        const string ErrorWordNotInLanguage = "The word \"{0}\" is not in the language of the pda";
        const string ErrorWordInLanguage = "The word \"{0}\" is in the language of the pda";
        const string ErrorWordWithInvalidKeys = "The word \"{0}\" contains the keys \"{1}\" that are not in the alphabet";

        /// <summary>
        /// calculates the grade of the given pda-word-problem attempt
        /// </summary>
        /// <param name="xmlPda">pda in xml-representation</param>
        /// <param name="xmlWordsInLanguage">words in xml-format that should be in the language of the pda</param>
        /// <param name="xmlWordsNotInLanguage">words in xml-format that should not be in the language of the pda</param>
        /// <param name="xmlMaxGrade">maximum reachable grade</param>
        /// <returns>grade of the problem attempt and a detailed feedback</returns>
        public static XElement GradeWordProblemAsync(XElement xmlPda, XElement xmlWordsInLanguage, XElement xmlWordsNotInLanguage, XElement xmlMaxGrade)
        {
            int timeOut = 10000;
            try
            {
                return TimeGuard<XElement>.Run(token => TryToGradeWordProblem(xmlPda, xmlWordsInLanguage, xmlWordsNotInLanguage, xmlMaxGrade, token), timeOut);
            }
            catch(TimeoutException)
            {
                return Grader.CreateXmlFeedback(0, new List<string>() { "Timeout - your inputs seem to be too big" });
            }
        }

        private static XElement TryToGradeWordProblem(XElement xmlPda, XElement xmlWordsInLanguage, XElement xmlWordsNotInLanguage, XElement xmlMaxGrade, CancellationToken token)
        {
            var pda = PDA<char, char>.FromXml(xmlPda);
            pda.CreateRunner(token);
            var alphabet = PDAXmlParser.ParseAlphabetFromXmlPDA(xmlPda);
            var maxGrade = int.Parse(xmlMaxGrade.Value);
            var wordsIn = xmlWordsInLanguage.Elements().Select(xmlWord => string.Concat(xmlWord.Value)).ToList(); //.Take(maximumWordLength)
            var wordsNotIn = xmlWordsNotInLanguage.Elements().Select(xmlWord => string.Concat(xmlWord.Value)).ToList(); //.Take(maximumWordLength)

            var feedback = new List<string>();
            int numberOfCorrectWords;
            try
            {
                numberOfCorrectWords = CheckWordsInLanguage(wordsIn, alphabet, pda, feedback) + CheckWordsNotInLanguage(wordsNotIn, alphabet, pda, feedback);
            }
            catch (InconsistentPDAException)
            {
                return Grader.CreateXmlFeedback(maxGrade, new List<string>() { "Oops! Seems like your tutor created an inconsistent pda...therefore, you get the full grade ;)" });
            }

            int totalNumberOfWords = wordsIn.Count() + wordsNotIn.Count();
            int grade = (int) ((double) numberOfCorrectWords * maxGrade / totalNumberOfWords);

            if (grade == maxGrade)
            {
                feedback.Add("Correct!");
            }

            return Grader.CreateXmlFeedback(grade, feedback);
        }

        private static int CheckWordsInLanguage(IEnumerable<string> words, HashSet<char> alphabet, PDA<char, char> pda, List<string> feedback)
        {
            return CheckWords(words, alphabet, word => pda.AcceptsWord(word).Accepts(), feedback, ErrorWordNotInLanguage);
        }

        private static int CheckWordsNotInLanguage(IEnumerable<string> words, HashSet<char> alphabet, PDA<char, char> pda, List<string> feedback)
        {
            return CheckWords(words, alphabet, word => !pda.AcceptsWord(word).Accepts(), feedback, ErrorWordInLanguage);
        }

        private static int CheckWords(IEnumerable<string> words, HashSet<char> alphabet, Func<string, bool> checker, List<string> feedback, string errorMessage)
        {
            int numberOfCorrectWords = 0;
            var wordsSoFar = new HashSet<string>();

            foreach (var word in words)
            {
                var invalidKeys = word.Where(k => !alphabet.Contains(k)).Distinct();
                if (invalidKeys.Count() > 0)
                {
                    feedback.Add(string.Format(ErrorWordWithInvalidKeys, word, string.Join(", ", invalidKeys)));
                }
                else if (wordsSoFar.Contains(word))
                {
                    feedback.Add(string.Format(ErrorWordMoreThanOnce, word));
                }
                else
                {
                    wordsSoFar.Add(word);
                    if (checker(word))
                    {
                        numberOfCorrectWords++;
                    }
                    else
                    {
                        feedback.Add(string.Format(errorMessage, word));
                    }
                }
            }

            return numberOfCorrectWords;
        }
    }
}
