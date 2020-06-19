using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.PDA
{
    /// <summary>
    /// represents a symbol that is read by the transition of a PDA, that means an input letter or epsilon
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public abstract class Symbol<A> : IEquatable<Symbol<A>> where A : IEquatable<A>
    {
        internal abstract bool IsEmpty();

        internal abstract A GetSymbol();

        internal static Symbol<A> EpsilonIn()
        {
            return new Epsilon();
        }

        internal static Symbol<A> SymbolIn(A symbol)
        {
            return new ExistingSymbol(symbol);
        }

        public abstract bool Equals(Symbol<A> other);

        private class Epsilon : Symbol<A>
        {
            internal override bool IsEmpty()
            {
                return true;
            }

            internal override A GetSymbol()
            {
                throw new Exception("epsilon has no symbol");
            }

            public override string ToString()
            {
                return "E";
            }

            public override bool Equals(Symbol<A> other)
            {
                return other.IsEmpty();
            }
        }

        private class ExistingSymbol : Symbol<A>
        {
            internal ExistingSymbol(A symbol)
            {
                this.symbol = symbol;
            }

            private A symbol;

            internal override bool IsEmpty()
            {
                return false;
            }

            internal override A GetSymbol()
            {
                return symbol;
            }

            public override string ToString()
            {
                return symbol.ToString();
            }

            public override bool Equals(Symbol<A> other)
            {
                return !other.IsEmpty() && symbol.Equals(other.GetSymbol());
            }
        }
    }
}
