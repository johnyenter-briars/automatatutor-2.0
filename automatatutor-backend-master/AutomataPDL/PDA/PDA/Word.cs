using AutomataPDL.PDA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.PDA
{
    public class Word<A> where A : IEquatable<A>
    {
        internal IEnumerable<A> Symbols { private set; get; }

        public Word(IEnumerable<A> symbols)
        {
            Symbols = symbols;
        }

        public static Word<A> ApplyTransitionToWord<S>(Transition<A, S> transition, Word<A> wordBefore) where S : IEquatable<S>
        {
            if (transition.SymbolIn.IsEmpty())
            {
                return new Word<A>(wordBefore.Symbols.ToList());
            }
            else
            {
                Assertion.Assert(!wordBefore.IsEmpty(), "The symbol-in has to epsilon if the remining-word is empty");
                Assertion.Assert(transition.SymbolIn.GetSymbol().Equals(wordBefore.Symbols.First()), 
                    "The symbol-in of the transition has to be the same as the first symbol of the remaining word");
                return new Word<A>(wordBefore.Symbols.Skip(1).ToList());
            }
        }

        internal bool IsEmpty()
        {
            return Symbols.Count() == 0;
        }

        public override string ToString()
        {
            return String.Join("", Symbols);
        }
    }
}
