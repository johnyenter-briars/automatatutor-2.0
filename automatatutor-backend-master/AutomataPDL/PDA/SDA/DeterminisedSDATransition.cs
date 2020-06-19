using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomataPDL.PDA.Utils;
using AutomataPDL.PDA.DPDA;
using AutomataPDL.PDA.DPDA.DPDAEquivalence;

namespace AutomataPDL.PDA.SDA
{
    internal class DeterminisedSDATransition<A, S> : IEquatable<DeterminisedSDATransition<A, S>> where A : IEquatable<A> where S : IEquatable<S>
    {
        internal A SymbolIn { get; }
        internal S StackSymbolIn { get; }
        internal StackSymbolSequenceSet<S> StackSymbolsSetWritten { get; private set; } //the first element of a stack sequence is the top of the new stack
        string KeySelector(StackSymbolSequence<S> s) => s.Id;

        public DeterminisedSDATransition(A symbolIn, S stackSymbolIn, StackSymbolSequenceSet<S> stackSymbolsSetWritten)
        {
            Assertion.Assert(stackSymbolsSetWritten.StackSequenceSet.All(s => s.StackSequence.Length <= 2), "as a determinised SDA is in normal form, a transition may only push at most two symbols");
            SymbolIn = symbolIn;
            StackSymbolIn = stackSymbolIn;
            StackSymbolsSetWritten = stackSymbolsSetWritten;
        }

        public bool CanBeEntered(A symbol, S stackSymbol)
        {
            return SymbolIn.Equals(symbol) && StackSymbolIn.Equals(stackSymbol);
        }

        public string Id
        {
            get
            {
                return SymbolIn.ToString() + "," + StackSymbolIn.ToString() + "/" + StackSymbolsSetWritten.ToString();
            }
        }

        public bool Equals(DeterminisedSDATransition<A, S> other)
        {
            return SymbolIn.Equals(other.SymbolIn)
                && StackSymbolIn.Equals(other.StackSymbolIn)
                && StackSymbolsSetWritten.Equals(other.StackSymbolsSetWritten);
        }
    }
}
