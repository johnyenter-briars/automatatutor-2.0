using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomataPDL.PDA.Utils;
using AutomataPDL.PDA.DPDA;
using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.DPDA.DPDAEquivalence;

namespace AutomataPDL.PDA.SDA
{
    /// <summary>
    /// a determinised SDA: all transitions with the same read symbol and the same read stack symbol are combined to one transition
    /// </summary>
    /// <typeparam name="A">type of the alphabet symbols</typeparam>
    /// <typeparam name="S">type of the stack alphabet symbols</typeparam>
    public class DeterminisedSDA<A, S> : IEquatable<DeterminisedSDA<A, S>> where A : IEquatable<A> where S : IEquatable<S>
    {
        private List<DeterminisedSDATransition<A, S>> transitions = new List<DeterminisedSDATransition<A, S>>();
        internal IEnumerable<S> AllStackSymbols { private set; get; }
        public Dictionary<S, IEnumerable<A>> ShortestWordsOfStackSymbols { get; private set; } = new Dictionary<S, IEnumerable<A>>();

        const string InvalidStackSymbolError = "one of the stack symbols is not in the stack symbol list of this pda";

        public DeterminisedSDA(IEnumerable<S> allStackSymbols)
        {
            AllStackSymbols = allStackSymbols;
        }

        public static DeterminisedSDA<A, S> FromSDAInNormalForm(SDA<A, S> sda)
        {
            var res = new DeterminisedSDA<A, S>(sda.AllStackSymbols);
            var origTransitions = sda.Transitions.GroupBy(t => new { t.SymbolIn, t.StackSymbolIn }).ToList();
            foreach(var tGroup in origTransitions)
            {
                var stackSymbolsSetWritten = tGroup.Select(t => t.StackSymbolsWritten).Select(s => new StackSymbolSequence<S>(s)).ToList().Distinct().ToList();
                res.AddTransition(tGroup.Key.SymbolIn, tGroup.Key.StackSymbolIn, new StackSymbolSequenceSet<S>(stackSymbolsSetWritten));
            }

            res.CalculateShortestWordsOfStackSymbols();

            return res;
        }

        public StackSymbolSequenceSet<S> ApplySymbolToStackSymbolSequenceSet(StackSymbolSequenceSet<S> stackSequenceSet, A symbol)
        {
            var res = stackSequenceSet.StackSequenceSet.Select(stackSequence =>
            {
                if (stackSequence.IsEpsilon())
                {
                    return new StackSymbolSequenceSet<S>();
                }

                var first = stackSequence.StackSequence.First();
                var enterableTransitions = transitions.Where(t => t.CanBeEntered(symbol, first));

                Assertion.Assert(enterableTransitions.Count() <= 1, "in a determinised SDA, only one transition can be entered for a specific configuration");

                if (enterableTransitions.Count() == 0)
                {
                    return new StackSymbolSequenceSet<S>();
                }

                var stackSymbolsSetWritten = enterableTransitions.First().StackSymbolsSetWritten;
                var remainingStackSequence = new StackSymbolSequence<S>(stackSequence.StackSequence.Skip(1).ToList());
                return stackSymbolsSetWritten.Multiply(remainingStackSequence);
            });
            return StackSymbolSequenceSet<S>.Flatten(res);
        }

        public StackSymbolSequenceSet<S> ApplyWordToStackSymbolSequenceSet(StackSymbolSequenceSet<S> stackSymbolSequenceSet, IEnumerable<A> word)
        {
            return word.Aggregate(stackSymbolSequenceSet, (acc, symbol) => ApplySymbolToStackSymbolSequenceSet(acc, symbol));
        }

        public StackSymbolSequenceSet<S> ApplyWordToStackSymbolSequence(StackSymbolSequence<S> stackSymbolSequence, IEnumerable<A> word)
        {
            return ApplyWordToStackSymbolSequenceSet(new StackSymbolSequenceSet<S>(stackSymbolSequence), word);
        }

        public void CalculateShortestWordsOfStackSymbols()
        {
            int norm = 1;
            while (ShortestWordsOfStackSymbols.Count < AllStackSymbols.Count())
            {
                var remainingSymbols = AllStackSymbols.Except(ShortestWordsOfStackSymbols.Keys).ToList();

                foreach(var remainingSymbol in remainingSymbols)
                {
                    var transitionsWithCurrentStackSymbol = transitions.Where(t => t.StackSymbolIn.Equals(remainingSymbol)).ToList();

                    Assertion.Assert(transitionsWithCurrentStackSymbol.Count > 0, "a stack symbol without possible transitions is redundant and should therefore have been removed");

                    var transitionAndStackSequenceFullfillingNorm = transitionsWithCurrentStackSymbol.Select(t =>
                    {
                        var stackSymbolSequences = t.StackSymbolsSetWritten.StackSequenceSet.Where(stackSymbolsSequence =>
                        {
                            var allStackSymbolsInSequenceAlreadyHaveNorm = stackSymbolsSequence.StackSequence.All(s => ShortestWordsOfStackSymbols.ContainsKey(s));
                            if (allStackSymbolsInSequenceAlreadyHaveNorm)
                            {
                                var normSum = stackSymbolsSequence.StackSequence.Sum(s => ShortestWordsOfStackSymbols[s].Count());
                                return normSum + 1 == norm;
                            }
                            else
                            {
                                return false;
                            }
                        }).ToList();

                        return new
                        {
                            transition = t,
                            stackSymbolSequences
                        };

                    }).Where(t => t.stackSymbolSequences.Count > 0).ToList();

                    if (transitionAndStackSequenceFullfillingNorm.Count > 0)
                    {
                        var t = transitionAndStackSequenceFullfillingNorm.First();
                        var firstSymbol = t.transition.SymbolIn;
                        var stackSequence = t.stackSymbolSequences.First();
                        var word = (new A[] { firstSymbol }).Concat(stackSequence.StackSequence.SelectMany(s => ShortestWordsOfStackSymbols[s])).ToList();
                        ShortestWordsOfStackSymbols.Add(remainingSymbol, word);
                    }
                }

                norm++;
            }
        }

        public void AddTransition(A symbolIn, S stackSymbolIn, StackSymbolSequenceSet<S> stackSymbolsSetWritten)
        {
            Assertion.Assert(AllStackSymbols.Any(s => s.Equals(stackSymbolIn)), InvalidStackSymbolError);
            Assertion.Assert(stackSymbolsSetWritten.StackSequenceSet.All(s => s.StackSequence.All(symbol => AllStackSymbols.Any(t => t.Equals(symbol)))), InvalidStackSymbolError);
            Assertion.Assert(stackSymbolsSetWritten.StackSequenceSet.All(s => s.StackSequence.Length <= 2), "as a determinised SDA is in normal form, only two stack symbols can be pushed");

            Assertion.Assert(!transitions.Any(t => t.SymbolIn.Equals(symbolIn) && t.StackSymbolIn.Equals(stackSymbolIn)), "the new transition violates the determinism property of the determinised SDA");

            transitions.Add(new DeterminisedSDATransition<A, S>(symbolIn, stackSymbolIn, stackSymbolsSetWritten));
        }

        public bool Equals(DeterminisedSDA<A, S> other)
        {
            var part1 = AllStackSymbols.OrderBy(s => s.ToString()).SequenceEqual(other.AllStackSymbols.OrderBy(s => s.ToString()));
            var part2 = transitions.OrderBy(t => t.Id).SequenceEqual(other.transitions.OrderBy(s => s.Id));
            return part1 && part2;
        }
    }
}
