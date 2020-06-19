using AutomataPDL.CFG;
using AutomataPDL.PDA.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.CFGUtils
{
    public static class CFGCleaner
    {
        private static void InitIds(ContextFreeGrammar cfg)
        {
            int i = 1;
            foreach(var v in cfg.Variables.Concat(cfg.GetNonVariableSymbols()).ToList())
            {
                v.UnqiueId = i++;
            }
        }

        public static ContextFreeGrammar RemoveUselessSymbols(ContextFreeGrammar cfg)
        {
            return RemoveUselessSymbols(cfg, new CancellationTokenSource().Token);
        }

        public static ContextFreeGrammar RemoveUselessSymbols(ContextFreeGrammar cfg, CancellationToken token)
        {
            var cfg1 = RemoveNonGeneratingSymbols(cfg, token);
            return cfg1 == null ? null : RemoveNonReachableSymbols(cfg1, token);
        }

        private static ContextFreeGrammar RemoveNonGeneratingSymbols(ContextFreeGrammar cfg, CancellationToken token)
        {
            InitIds(cfg);

            var res = new HashSet<int>(cfg.GetNonVariableSymbols().Select(v => v.UnqiueId));

            var remainingProductions = new Dictionary<int, List<Production>>();
            foreach(var s in cfg.Variables)
            {
                remainingProductions[s.UnqiueId] = new List<Production>();
            }
            foreach(var prod in cfg.GetProductions())
            {
                remainingProductions[prod.Lhs.UnqiueId].Add(prod);
            }

            int oldLength;

            do
            {
                token.ThrowIfCancellationRequested();

                oldLength = res.Count;

                var newSymbols = new HashSet<int>();

                foreach(var e in remainingProductions)
                {
                    token.ThrowIfCancellationRequested();

                    if (e.Value.Any(p => p.Rhs.All(s => res.Contains(s.UnqiueId))))
                    {
                        res.Add(e.Key);
                        newSymbols.Add(e.Key);
                    }
                }

                foreach(var s in newSymbols)
                {
                    token.ThrowIfCancellationRequested();

                    remainingProductions.Remove(s);
                }
            }
            while (res.Count > oldLength);

            if (!res.Contains(cfg.StartSymbol.UnqiueId))
            {
                return null;
            }
            else
            {
                var prods = FilterProductions(res, cfg.GetProductions());
                return new ContextFreeGrammar(cfg.StartSymbol, prods);
            }
        }

        private static IEnumerable<Production> FilterProductions(HashSet<int> symbols, IEnumerable<Production> productions)
        {
            return productions.Where(p => symbols.Contains(p.Lhs.UnqiueId) && p.Rhs.All(s => symbols.Contains(s.UnqiueId))).ToList();
        }

        private static ContextFreeGrammar RemoveNonReachableSymbols(ContextFreeGrammar cfg, CancellationToken token)
        {
            InitIds(cfg);

            var productions = cfg.productionMap;

            var res = new HashSet<int>() { cfg.StartSymbol.UnqiueId };

            var queue = new Queue<Nonterminal>();
            queue.Enqueue(cfg.StartSymbol);

            while (queue.Count > 0)
            {
                token.ThrowIfCancellationRequested();

                var next = queue.Dequeue();
                var prods = productions[next];
                foreach (var prod in prods)
                {
                    token.ThrowIfCancellationRequested();

                    foreach (var rhs in prod.Rhs)
                    {
                        if (res.Add(rhs.UnqiueId) && rhs is Nonterminal)
                        {
                            queue.Enqueue((Nonterminal) rhs);
                        }
                    }
                }
            }

            return new ContextFreeGrammar(cfg.StartSymbol, FilterProductions(res, cfg.GetProductions()));
        }
    }
}
