using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.Utils;

namespace AutomataPDL.PDA.PDARunnerDirect
{
    internal class Word<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        IEnumerable<A> symbols;

        internal Word(Transition<A, S> transition, Configuration<A, S> config)
        {
            if (transition.SymbolIn.IsEmpty())
            {
                symbols = config.RemainingWord.symbols.ToList();
            }
            else
            {
                Assertion.Assert(!config.RemainingWord.IsEmpty(), "The symbol-in has to epsilon if the remining-word is empty");
                Assertion.Assert(transition.SymbolIn.GetSymbol().Equals(config.RemainingWord.symbols.First()), "The symbol-in of the transition has to be the same as the first symbol of the remaining word");
                symbols = config.RemainingWord.symbols.Skip(1).ToList();
            }
        }

        internal Word(IEnumerable<A> symbols)
        {
            this.symbols = symbols;
        }

        internal bool IsEmpty()
        {
            return symbols.Count() == 0;
        }

        internal A First()
        {
            return symbols.First();
        }

        internal int Length
        {
            get { return symbols.Count(); }
        }

        public override string ToString()
        {
            return String.Join("", symbols);
        }
    }
}
