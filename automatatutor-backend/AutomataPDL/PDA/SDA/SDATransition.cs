using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomataPDL.PDA.Utils;

namespace AutomataPDL.PDA.SDA
{
    internal class SDATransition<A, S> : IEquatable<SDATransition<A, S>> where A : IEquatable<A> where S : IEquatable<S>
    {
        internal A SymbolIn { get; }
        internal S StackSymbolIn { get; }
        internal S[] StackSymbolsWritten { get; private set; } //the first element is the top of the new stack

        public SDATransition(A symbolIn, S stackSymbolIn, S[] stackSymbolsWritten)
        {
            Assertion.Assert(stackSymbolsWritten.Length <= 2, "as an SDA is in normal form, a transition may only push at most two symbols");
            SymbolIn = symbolIn;
            StackSymbolIn = stackSymbolIn;
            StackSymbolsWritten = stackSymbolsWritten;
        }

        internal void RemoveEpsilonSymbol(S epsilonSymbol)
        {
            StackSymbolsWritten = StackSymbolsWritten.Where(s => !s.Equals(epsilonSymbol)).ToArray();
        }

        public string Id
        {
            get
            {
                return SymbolIn.ToString() + "," + StackSymbolIn.ToString() + "/" + String.Join(",", StackSymbolsWritten);
            }
        }

        public bool Equals(SDATransition<A, S> other)
        {
            return SymbolIn.Equals(other.SymbolIn)
                && StackSymbolIn.Equals(other.StackSymbolIn)
                && StackSymbolsWritten.SequenceEqual(other.StackSymbolsWritten);
        }
    }
}
