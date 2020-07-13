using AutomataPDL.CFG;
using AutomataPDL.PDA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.CFGUtils
{
    public class CFGTo2NFConverter<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        public const string PrefixOfAddedNonterminals = "NEW";

        /// <summary>
        /// Converts the given cfg into a new one in 2NF, that means that every production has at most 2 symbols on the right hand side
        /// </summary>
        /// <param name="cfg">cfg</param>
        /// <returns>cfg in 2NF</returns>
        public static ContextFreeGrammar To2NF(ContextFreeGrammar cfg)
        {
            var productions = new List<Production>();

            int currentNumber = 0;

            var productionsToShorten = new Queue<Production>(cfg.GetProductions());
            while (productionsToShorten.Count() > 0)
            {
                var next = productionsToShorten.Dequeue();
                if (next.Rhs.Length <= 2)
                {
                    productions.Add(next);
                }
                else
                {
                    var newNonTerminal = new Nonterminal(PrefixOfAddedNonterminals + currentNumber);
                    currentNumber++;

                    Assertion.Assert(!cfg.Variables.Any(v => v.Name.Equals(newNonTerminal.Name)), 
                        string.Format("The nonterminal with id {0} already existed. Please ensure, that the CFG does not use ints as ids", newNonTerminal.Name));

                    productions.Add(new Production(next.Lhs, next.Rhs[0], newNonTerminal));
                    productionsToShorten.Enqueue(new Production(newNonTerminal, next.Rhs.Skip(1).ToArray()));
                }
            }

            return new ContextFreeGrammar(cfg.StartSymbol, productions);
        }
    }
}
