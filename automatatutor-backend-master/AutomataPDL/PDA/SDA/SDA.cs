using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.Utils;

namespace AutomataPDL.PDA.SDA
{
    /// <summary>
    /// a PDA with only one state and no epsilon-transitions, which is in normal form 
    /// (that means, that a transition can only push two stack symbols and no stack symbol is redundant)
    /// </summary>
    /// <typeparam name="A">type of the alphabet symbols</typeparam>
    /// <typeparam name="S">type of the stack alphabet symbols</typeparam>
    public class SDA<A, S> : IEquatable<SDA<A, S>> where A : IEquatable<A> where S : IEquatable<S>
    {
        private List<SDATransition<A, S>> transitions = new List<SDATransition<A, S>>();
        internal IEnumerable<S> AllStackSymbols { private set; get; }

        const string InvalidStackSymbolError = "one of the stack symbols is not in the stack symbol list of this pda";

        public SDA(IEnumerable<S> allStackSymbols)
        {
            AllStackSymbols = allStackSymbols;
        }

        public void AddTransition(A symbolIn, S stackSymbolIn, IEnumerable<S> stackSymbolsWritten)
        {
            Assertion.Assert(AllStackSymbols.Any(s => s.Equals(stackSymbolIn)), InvalidStackSymbolError);
            Assertion.Assert(stackSymbolsWritten.All(s => AllStackSymbols.Any(t => t.Equals(s))), InvalidStackSymbolError);
            Assertion.Assert(stackSymbolsWritten.Count() <= 2, "as an SDA is in normal form, only two stack symbols can be pushed");

            transitions.Add(new SDATransition<A, S>(symbolIn, stackSymbolIn, stackSymbolsWritten.ToArray()));
        }

        public void AddTransition(A symbolIn, S stackSymbolIn)
        {
            AddTransition(symbolIn, stackSymbolIn, new S[] { });
        }

        internal IEnumerable<SDATransition<A, S>> Transitions
        {
            get
            {
                return transitions;
            }
        }

        internal void RemoveStackSymbols(IEnumerable<S> stackSymbolsToRemove)
        {
            transitions = transitions.Where(t => !t.StackSymbolsWritten.Any(s => stackSymbolsToRemove.Contains(s))).ToList();
            AllStackSymbols = AllStackSymbols.Except(stackSymbolsToRemove);
        }

        public PDA<A, S> ToPDA(S firstStackSymbol)
        {
            Assertion.Assert(AllStackSymbols.Any(s => s.Equals(firstStackSymbol)), "the given first stack symbol is not a valid stack symbol for this SDA");

            var res = new PDA<A, S>(new AcceptanceCondition.EmptyStack(), false, firstStackSymbol, false, AllStackSymbols);
            foreach(var t in transitions)
            {
                res.AddTransition().From(0).To(0).Read(t.SymbolIn).Pop(t.StackSymbolIn).Push(t.StackSymbolsWritten);
            }
            return res;
        }

        public bool Equals(SDA<A, S> other)
        {
            var part1 = AllStackSymbols.OrderBy(s => s.ToString()).SequenceEqual(other.AllStackSymbols.OrderBy(s => s.ToString()));
            var part2 = Transitions.OrderBy(t => t.Id).SequenceEqual(other.Transitions.OrderBy(s => s.Id));
            return part1 && part2;
        }
    }
}
