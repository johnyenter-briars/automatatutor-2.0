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
    public static class ConstructionProblemGrader
    {
        const int maximumWordLengthTested = 9;
        const int maximumNumberOfWords = 100000;
        const int maximumMilliSeconds = 10000;

        const string ErrorWordFalselyExcluded = "the word \"{0}\" is in the given language, but not recognized by your pda";
        const string HintSuperset = "your pda recognizes a superset of the given language";
        const string ErrorWordFalselyIncluded = "the word \"{0}\" is not in the given language, but it is recognized by your pda";
        const string HintSubset = "your pda recognizes a subset of the given language";
        const string ErrorToManyStates = "your pda has {0} states, but is possible to use only {1}";
        const string ErrorToManyStackSymbols = "your pda has {0} stack-symbols, but it is possible to use only {1}";
        const string ErrorInconsistentAcceptanceCondition = "your pda is not consistent concerning its acceptence condition: the word \"{0}\" has acceptance {1} for final state, but {2} for empty stack";

        /// <summary>
        /// calculates the grade of the given pda-constuction-problem attempt
        /// </summary>
        /// <param name="xmlPdaCorrect">the correct pda that defines the language</param>
        /// <param name="xmlPdaAttempt">attempt pda created by the solver</param>
        /// <param name="xmlGiveStackAlphabet">whether the stack-alphabet was given or the solver could define it himself</param>
        /// <param name="xmlMaxGrade">maximum reachable grade</param>
        /// <returns>grade of the problem attempt and a detailed feedback</returns>
        public static XElement GradeConstructionProblem(XElement xmlPdaCorrect, XElement xmlPdaAttempt, XElement xmlGiveStackAlphabet, XElement xmlMaxGrade)
        {
            int timeOut = 10000;
            try
            {
                return TimeGuard<XElement>.Run(token => TryToGradeConstructionProblem(xmlPdaCorrect, xmlPdaAttempt, xmlGiveStackAlphabet, xmlMaxGrade, token), timeOut);
            }
            catch (TimeoutException)
            {
                return Grader.CreateXmlFeedback(0, new List<string>() { "Timeout - your inputs seem to be too big" });
            }
        }

        private static XElement TryToGradeConstructionProblem(XElement xmlPdaCorrect, XElement xmlPdaAttempt, XElement xmlGiveStackAlphabet, XElement xmlMaxGrade, CancellationToken token)
        {
            var pdaCorrect = PDA<char, char>.FromXml(xmlPdaCorrect);
            pdaCorrect.CreateRunner(token);
            var pdaAttempt = PDA<char, char>.FromXml(xmlPdaAttempt);
            pdaAttempt.CreateRunner(token);
            var giveStackAlphabet = bool.Parse(xmlGiveStackAlphabet.Value);
            var maxGrade = int.Parse(xmlMaxGrade.Value);

            var alphabet = PDAXmlParser.ParseAlphabetFromXmlPDA(xmlPdaCorrect);
            Assertion.Assert(Enumerable.SequenceEqual(alphabet, PDAXmlParser.ParseAlphabetFromXmlPDA(xmlPdaAttempt)), "the alphabets of the given PDAs are not the same");

            var stackAlphabetCorrect = PDAXmlParser.ParseStackAlphabetFromXmlPDA(xmlPdaCorrect);
            var stackAlphabetAttempt = PDAXmlParser.ParseStackAlphabetFromXmlPDA(xmlPdaAttempt);
            Assertion.Assert(!giveStackAlphabet || Enumerable.SequenceEqual(stackAlphabetCorrect, stackAlphabetAttempt), "if the stack-alphabet is given for the solver, the attempt pda has to have the same stack-alphabet");

            int statesDiff = pdaAttempt.States.Count - pdaCorrect.States.Count;
            int stackDiff = stackAlphabetAttempt.Count - stackAlphabetCorrect.Count;
            PDAEqualityResult<char, char> pdaEqualityResult;
            pdaEqualityResult = new PDAEqualityResult<char, char>(pdaCorrect, pdaAttempt, alphabet, maximumWordLengthTested, maximumNumberOfWords, maximumMilliSeconds);
            if (pdaEqualityResult.PdaCorrectIsinconsistent)
            {
                return Grader.CreateXmlFeedback(maxGrade, new List<string>() { "Oops! Seems like your tutor created an inconsistent pda...therefore, you get the full grade ;)" });
            }

            int joinSize = pdaEqualityResult.NumberOfWordsAcceptedByAtLeastOne;
            double proportion = joinSize == 0 ? 1 : (double)pdaEqualityResult.NumberOfWordsAcceptedByBoth / joinSize;
            int grade = (int) (maxGrade * proportion);

            var feedback = new List<string>();

            if (!pdaEqualityResult.AreEqual)
            {
                if (pdaEqualityResult.WordsInPdaCorrectButNotInPdaAttempt.Count() == 0 && pdaEqualityResult.WordsInPdaAttemptButNotInPdaCorrect.Count() > 0)
                {
                    feedback.Add(string.Format(HintSuperset));
                }
                else if (pdaEqualityResult.WordsInPdaAttemptButNotInPdaCorrect.Count() == 0 && pdaEqualityResult.WordsInPdaCorrectButNotInPdaAttempt.Count() > 0)
                {
                    feedback.Add(string.Format(HintSubset));
                }

                if (pdaEqualityResult.WordsInPdaCorrectButNotInPdaAttempt.Count() > 0)
                {
                    feedback.Add(string.Format(ErrorWordFalselyExcluded, FindWordWithMinimumLength(pdaEqualityResult.WordsInPdaCorrectButNotInPdaAttempt)));
                }

                if (pdaEqualityResult.WordsInPdaAttemptButNotInPdaCorrect.Count() > 0)
                {
                    feedback.Add(string.Format(ErrorWordFalselyIncluded, FindWordWithMinimumLength(pdaEqualityResult.WordsInPdaAttemptButNotInPdaCorrect)));
                }
            }

            if (pdaEqualityResult.InconsistentWordsInPdaAttempt.Count() > 0)
            {
                grade = Math.Max(grade - 1, 0);
                var tuple = FindElementWithMinimumProperty(pdaEqualityResult.InconsistentWordsInPdaAttempt, t => t.Item1.Length);
                feedback.Add(string.Format(ErrorInconsistentAcceptanceCondition, tuple.Item1, tuple.Item2, !tuple.Item2));
            }

            if (statesDiff > 0 || stackDiff > 0)
            {
                grade = Math.Max(0, grade - 1);

                if (statesDiff > 0)
                {
                    feedback.Add(string.Format(ErrorToManyStates, pdaAttempt.States.Count, pdaCorrect.States.Count));
                }
                if (stackDiff > 0)
                {
                    feedback.Add(string.Format(ErrorToManyStackSymbols, stackAlphabetAttempt.Count, stackAlphabetCorrect.Count));
                }
            }

            if (grade == maxGrade)
            {
                feedback.Add("Correct!");
            }
            else if (pdaEqualityResult.AreEqual && pdaEqualityResult.InconsistentWordsInPdaAttempt.Count() == 0)
            {
                feedback.Add("Almost correct!");
            }

            return Grader.CreateXmlFeedback(grade, feedback);
        }

        private static T FindElementWithMinimumProperty<T>(IEnumerable<T> list, Func<T, int> getProperty)
        {
            return list.Aggregate((elementWithMinSoFar, el) => (elementWithMinSoFar == null || getProperty(el) < getProperty(elementWithMinSoFar)) ? el : elementWithMinSoFar);
        }

        private static string FindWordWithMinimumLength(IEnumerable<string> words)
        {
            return FindElementWithMinimumProperty<string>(words, w => w.Length);
        }
    }
}
