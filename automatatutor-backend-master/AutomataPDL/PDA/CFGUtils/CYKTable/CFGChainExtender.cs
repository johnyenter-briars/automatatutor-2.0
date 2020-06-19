using AutomataPDL.CFG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.CFGUtils.CYKTable
{
    internal static class CFGChainExtender<A> where A : IEquatable<A>
    {
        /// <summary>
        /// adds to the given dictionary recursively all entries, that have a production with all right-hand-side-symbols on the dictionary
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="cfg"></param>
        internal static void AddChainEntries(Dictionary<int, DerivationNode<A>> entries, ContextFreeGrammar cfg)
        {
            bool newProductionsFound = true;
            while (newProductionsFound)
            {
                var productions = cfg.GetProductions().Where(prod => prod.Rhs.All(s => entries.ContainsKey(s.UnqiueId))).ToList();
                var newProductions = productions.Where(prod => !entries.ContainsKey(prod.Lhs.UnqiueId)).ToList();
                if (newProductions.Count > 0)
                {
                    foreach (var prod in newProductions)
                    {
                        if (!entries.ContainsKey(prod.Lhs.UnqiueId))
                        {
                            entries.Add(prod.Lhs.UnqiueId, new DerivationNode<A>(prod, prod.Rhs.Select(s => entries[s.UnqiueId]).ToList()));
                        }
                    }
                }
                else
                {
                    newProductionsFound = false;
                }
            }
        }

        /// <summary>
        /// adds to the given dictionary recursively all entries, that have a production with only epsilon-symbols on the right hand side, except exactly one of the symbols in entries
        /// </summary>
        /// <param name="entries"></param>
        /// <param name="cfg"></param>
        /// <param name="epsilonSymbols">symbols, that can reach epsilon without derivate a letter</param>
        internal static void AddChainAndEpsilonEntries(Dictionary<int, DerivationNode<A>> entries, PerformanceCFG cfg, Dictionary<int, DerivationNode<A>> epsilonSymbols)
        {
            var newEntries = new Queue<DerivationNode<A>>(entries.Values);
            while (newEntries.Count > 0)
            {
                AddChainAndEpsilonEntriesForNextEntry(entries, cfg, epsilonSymbols, newEntries);
            }
        }

        private static void AddChainAndEpsilonEntriesForNextEntry(Dictionary<int, DerivationNode<A>> entries, PerformanceCFG cfg, Dictionary<int, DerivationNode<A>> epsilonSymbols, Queue<DerivationNode<A>> newEntries)
        {
            var next = newEntries.Dequeue();
            var productionsWithNextOnRightHandsSide = cfg.GetProductionsWhereRightHandSideContains(next.Symbol);
            foreach (var prod in productionsWithNextOnRightHandsSide)
            {
                //only add new entries; so maybe not all derivations are found, but only one is needed
                if (!entries.ContainsKey(prod.Lhs.UnqiueId))
                {
                    AddChainAndEpsilonEntriesForProduction(entries, epsilonSymbols, newEntries, next, prod);
                }
            }
        }

        private static void AddChainAndEpsilonEntriesForProduction(Dictionary<int, DerivationNode<A>> entries, Dictionary<int, DerivationNode<A>> epsilonSymbols, Queue<DerivationNode<A>> newEntries, DerivationNode<A> next, Production prod)
        {
            if (prod.Rhs.Count() == 1)
            {
                var newEntry = new DerivationNode<A>(prod, new List<DerivationNode<A>>() { next });
                newEntries.Enqueue(newEntry);
                entries.Add(newEntry.Symbol.UnqiueId, newEntry);
            }
            else
            {
                //every rhs has length 2
                if (!prod.Rhs.All(s => s.Equals(next.Symbol)) && epsilonSymbols.ContainsKey(prod.Rhs.First(s => !s.Equals(next.Symbol)).UnqiueId))
                {
                    var newEntry = new DerivationNode<A>(prod, prod.Rhs.Select(s =>
                    {
                        if (s.Equals(next.Symbol))
                        {
                            return next;
                        }
                        else
                        {
                            return epsilonSymbols[s.UnqiueId];
                        }
                    }).ToList());
                    newEntries.Enqueue(newEntry);
                    entries.Add(newEntry.Symbol.UnqiueId, newEntry);
                }
            }
        }
    }
}
