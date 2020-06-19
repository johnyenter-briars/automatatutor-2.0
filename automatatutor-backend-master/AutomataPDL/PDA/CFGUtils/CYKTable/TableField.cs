using AutomataPDL.CFG;
using AutomataPDL.PDA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.CFGUtils.CYKTable
{
    /// <summary>
    /// represents an entry field in the CYK-table; when create a new TableField with some symbols, they are automatically expanded to all 
    /// nodes reachable by chain productions
    /// </summary>
    /// <typeparam name="A"></typeparam>
    public class TableField<A> where A: IEquatable<A>
    {
        public Dictionary<int, DerivationNode<A>> Entries { private set; get; }

        private TableField(Dictionary<int, DerivationNode<A>> entries, Dictionary<int, DerivationNode<A>> epsilonSymbols, PerformanceCFG cfg)
        {
            Entries = entries;
            CFGChainExtender<A>.AddChainAndEpsilonEntries(entries, cfg, epsilonSymbols);
        }

        /// <summary>
        /// creates an entry with a single terminal
        /// </summary>
        /// <param name="terminal"></param>
        /// <param name="epsilonSymbols"></param>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public static TableField<A> WithTerminal(Exprinal<A> terminal, Dictionary<int, DerivationNode<A>> epsilonSymbols, PerformanceCFG cfg)
        {
            var entries = new Dictionary<int, DerivationNode<A>>
            {
                { terminal.UnqiueId, new DerivationNode<A>(terminal) }
            };
            return new TableField<A>(entries, epsilonSymbols, cfg);
        }

        private static void CombineTwoTableFields(Dictionary<int, DerivationNode<A>> entries, TableField<A> left, TableField<A> right, Dictionary<int, DerivationNode<A>> epsilonSymbols, PerformanceCFG cfg)
        {
            foreach(var prod in cfg.BinaryProductions)
            {
                if (left.Entries.TryGetValue(prod.Rhs[0].UnqiueId, out DerivationNode<A> leftNode))
                {
                    if (right.Entries.TryGetValue(prod.Rhs[1].UnqiueId, out DerivationNode<A> rightNode))
                    {
                        if (!entries.ContainsKey(prod.Lhs.UnqiueId))
                        {
                            entries.Add(prod.Lhs.UnqiueId, new DerivationNode<A>(prod, new List<DerivationNode<A>>() { leftNode, rightNode }));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// creats an entry for the given CYK-table at the defined position
        /// </summary>
        /// <param name="cykTable"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="epsilonSymbols"></param>
        /// <param name="cfg"></param>
        /// <returns></returns>
        public static TableField<A> CreateTableFieldForCYKTable(TableField<A>[][] cykTable, int row, int column, Dictionary<int, DerivationNode<A>> epsilonSymbols, PerformanceCFG cfg)
        {
            var entries = new Dictionary<int, DerivationNode<A>>();

            for (int i = 0; i < row; i++)
            {
                CombineTwoTableFields(entries, cykTable[i][column], cykTable[row - i - 1][column + i + 1], epsilonSymbols, cfg);
            }
            return new TableField<A>(entries, epsilonSymbols, cfg);
        }
    }
}
