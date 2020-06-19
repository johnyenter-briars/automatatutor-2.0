using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomataPDL.PDA.Utils;

namespace AutomataPDL.PDA.PDA
{
    /// <summary>
    /// stores the result of an approximative equality test of a <see cref="PDA{char, S}"/> and a <see cref="PDA{char, T}"/>
    /// </summary>
    /// <typeparam name="S">type of stack symbols of the first pda</typeparam>
    /// <typeparam name="T">type of stack symbols of the second pda</typeparam>
    public class PDAEqualityResult<S, T> where S : IEquatable<S> where T : IEquatable<T>
    {
        private PDA<char, S> pdaCorrect;
        private PDA<char, T> pdaAttempt;
        private readonly IEnumerable<char> alphabet;

        internal IEnumerable<string> WordsInPdaCorrectButNotInPdaAttempt { private set; get; }
        internal IEnumerable<string> WordsInPdaAttemptButNotInPdaCorrect { private set; get; }
        //the bool contains whether the pda accepts for final state
        internal IEnumerable<Tuple<string, bool>> InconsistentWordsInPdaAttempt { private set; get; }
        public bool AreEqual { private set; get; } = false;

        internal int NumberOfWordsAcceptedByAtLeastOne { get; private set; }
        internal int NumberOfWordsAcceptedByBoth { get; private set; }

        volatile bool pdaCorrectIsinconsistent = false;

        internal bool PdaCorrectIsinconsistent => pdaCorrectIsinconsistent;

        /// <summary>
        /// runs an approximative equality test of a <see cref="pdaCorrect"/> and a <see cref="pdaAttempt"/>
        /// </summary>
        /// <param name="pdaCorrect">PDA assumed to be the correct PDA</param>
        /// <param name="pdaAttempt">PDA to be testet if it equals the <see cref="pdaCorrect"/></param>
        /// <param name="alphabet">alphabet of the both PDAs</param>
        /// <param name="maximumWordLengthTested">the maximum length of words out of letters of the <see cref="alphabet"/>, which is tested for, if the PDAs accept them</param>
        /// <param name="maximumNumberOfWords">an upper bound for number of words to be tested</param>
        /// <param name="maximumMilliSeconds">an upper bound for the time the test should need</param>
        public PDAEqualityResult(PDA<char, S> pdaCorrect, PDA<char, T> pdaAttempt, IEnumerable<char> alphabet, int maximumWordLengthTested, int maximumNumberOfWords, int maximumMilliSeconds)
        {
            this.pdaCorrect = pdaCorrect;
            this.pdaAttempt = pdaAttempt;
            this.alphabet = alphabet;
            CheckWordsUntilLength(maximumWordLengthTested, maximumNumberOfWords, maximumMilliSeconds);
        }

        void CheckWordsUntilLength(int length, int maximumNumberOfWords, int maximumMilliSeconds)
        {
            var wordsInPdaCorrectButNotInPdaAttempt = new ConcurrentBag<string>();
            var wordsInPdaAttemptButNotInPdaCorrect = new ConcurrentBag<string>();
            var inconsistentWordsInPdaAttempt = new ConcurrentBag<Tuple<string, bool>>();
            var words = WordGenerator.GenerateWordsUntilLength(length, alphabet).Take(maximumNumberOfWords);

            var watch = new Stopwatch();
            watch.Start();

            var res = words.AsParallel().Select(word => {
                if (watch.ElapsedMilliseconds > maximumMilliSeconds)
                {
                    return new Tuple<int, int>(0, 0);
                }

                AcceptanceResult<S> isInPdaCorrect;
                try
                {
                    isInPdaCorrect = pdaCorrect.AcceptsWord(word);
                }
                catch(InconsistentPDAException)
                {
                    pdaCorrectIsinconsistent = true;
                    return new Tuple<int, int>(0, 0);
                }
                var isInPdaAttempt = pdaAttempt.AcceptsWordOrInconsistent(word);
                if (isInPdaAttempt.IsInconsistent())
                {
                    Assertion.Assert(pdaAttempt.AcceptanceCondition.IsEmptyStack() && pdaAttempt.AcceptanceCondition.IsFinalState(), "an inconsistent acceptance of a word can only occur when using final-state and empty stack as acceptance condition");
                    inconsistentWordsInPdaAttempt.Add(new Tuple<string, bool>(word, isInPdaAttempt.AcceptedByAcceptedCondition[AcceptanceCondition.FinalState.GetId()]));
                }

                if (isInPdaCorrect.Accepts() && !isInPdaAttempt.Accepts())
                {
                    wordsInPdaCorrectButNotInPdaAttempt.Add(word);
                }
                else if (!isInPdaCorrect.Accepts() && isInPdaAttempt.Accepts())
                {
                    wordsInPdaAttemptButNotInPdaCorrect.Add(word);
                }

                return new Tuple<int, int>(isInPdaAttempt.Accepts() || isInPdaCorrect.Accepts() ? 1 : 0, isInPdaAttempt.Accepts() && isInPdaCorrect.Accepts() ? 1 : 0);
            }).ToList();

            watch.Stop();

            NumberOfWordsAcceptedByAtLeastOne = res.Sum(t => t.Item1);
            NumberOfWordsAcceptedByBoth = res.Sum(t => t.Item2);

            WordsInPdaCorrectButNotInPdaAttempt = wordsInPdaCorrectButNotInPdaAttempt;
            WordsInPdaAttemptButNotInPdaCorrect = wordsInPdaAttemptButNotInPdaCorrect;
            InconsistentWordsInPdaAttempt = inconsistentWordsInPdaAttempt;
            AreEqual = WordsInPdaAttemptButNotInPdaCorrect.Count() == 0 && WordsInPdaCorrectButNotInPdaAttempt.Count() == 0;
        }
    }
}
