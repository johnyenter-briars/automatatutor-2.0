using AutomataPDL.CFG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.CFGUtils
{
    /// <summary>
    /// extends a <see cref="ContextFreeGrammar"/> by useful methods to increase the performance for certain operations
    /// </summary>
    public class PerformanceCFG
    {
        internal ContextFreeGrammar Cfg {get;}
        private readonly Dictionary<GrammarSymbol, List<Production>> productionsByGrammarSymbolsInRightHandSide = new Dictionary<GrammarSymbol, List<Production>>();
        public List<Production> BinaryProductions { get; }

        public PerformanceCFG(ContextFreeGrammar cfg)
        {
            Cfg = cfg;
            InitIds();
            BinaryProductions = Cfg.GetProductions().Where(p => p.Rhs.Count() == 2).ToList();
            InitializeProductionsByGrammarSymbolsInRightHandSide();
        }

        private void InitIds()
        {
            var i = 1;
            foreach(var v in Cfg.Variables.Concat(Cfg.GetNonVariableSymbols()))
            {
                v.UnqiueId = i++;
            }
        }

        public IEnumerable<Production> GetProductionsWhereRightHandSideContains(GrammarSymbol symbol)
        {
            if (productionsByGrammarSymbolsInRightHandSide.ContainsKey(symbol))
            {
                return productionsByGrammarSymbolsInRightHandSide[symbol];
            }
            else
            {
                return new List<Production>();
            }
        }

        private static string GetKeyOfGrammarSymbols(IEnumerable<GrammarSymbol> rhs)
        {
            return string.Join(";", rhs);
        }

        private void InitializeProductionsByGrammarSymbolsInRightHandSide()
        {
            foreach(var v in Cfg.Variables.Concat(Cfg.GetNonVariableSymbols()).ToList())
            {
                productionsByGrammarSymbolsInRightHandSide[v] = new List<Production>();
            }

            foreach(var prod in Cfg.GetProductions())
            {
                foreach(var v in prod.Rhs.Distinct().ToList())
                {
                    productionsByGrammarSymbolsInRightHandSide[v].Add(prod);
                }
            }
        }
    }
}
