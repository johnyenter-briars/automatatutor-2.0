using AutomataPDL.CFG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.CFGUtils.CYKTable
{
    public class CYKTable<A> where A: IEquatable<A>
    {
        /// <summary>
        /// the bottom left entry in the cyk-table (see https://www7.in.tum.de/um/courses/theo/ss2018/exercises/theo18-07-solution.pdf) 
        /// corresponds to the [0][0]-entry in this table; the indices are defined like this: CykTable[row][column]
        /// </summary>
        public TableField<A>[][] CykTable { get; private set; }

        readonly A[] word;
        readonly PerformanceCFG cfg;

        Dictionary<int, DerivationNode<A>> epsilonSymbols = new Dictionary<int, DerivationNode<A>>();

        /// <summary>
        /// builds a filled CYK-table out of the cfg
        /// </summary>
        /// <param name="word"></param>
        /// <param name="cfg">cfg in 2NF</param>
        public CYKTable(A[] word, ContextFreeGrammar cfg)
        {
            this.word = word;
            this.cfg = new PerformanceCFG(cfg);
            InitializeCykTable();
            CalculateEpsilonSymbols();
            if (word.Length > 0)
            {
                FillTable();
            }
        }

        public CYKTable(A[] word, PerformanceCFG cfg)
        {
            this.word = word;
            this.cfg = cfg;
            InitializeCykTable();
            CalculateEpsilonSymbols();
            if (word.Length > 0)
            {
                FillTable();
            }
        }

        public bool AcceptsWord()
        {
            if (word.Length > 0)
            {
                return CykTable[word.Length - 1][0].Entries.ContainsKey(cfg.Cfg.StartSymbol.UnqiueId);
            }
            else
            {
                return epsilonSymbols.ContainsKey(cfg.Cfg.StartSymbol.UnqiueId);
            }
        }

        public DerivationNode<A> GetStartDerivationNode()
        {
            if (word.Length > 0)
            {
                return CykTable[word.Length - 1][0].Entries[cfg.Cfg.StartSymbol.UnqiueId];
            }
            else
            {
                return epsilonSymbols[cfg.Cfg.StartSymbol.UnqiueId];
            }
        }

        private void InitializeCykTable()
        {
            CykTable = new TableField<A>[word.Length][];

            for(int i = 0; i < word.Length; i++)
            {
                CykTable[i] = new TableField<A>[word.Length - i];
            }
        }

        private void CalculateEpsilonSymbols()
        {
            AddDirectEpsilonSymbols();
            CFGChainExtender<A>.AddChainEntries(epsilonSymbols, cfg.Cfg);
        }

        private void AddDirectEpsilonSymbols()
        {
            foreach (var prod in cfg.Cfg.GetProductions().Where(p => p.Rhs.Count() == 0))
            {
                epsilonSymbols.Add(prod.Lhs.UnqiueId, new DerivationNode<A>(prod, new List<DerivationNode<A>>()));
            }
        }

        private void FillTable()
        {
            FillBasicRow();
            FillOtherRows();
        }

        private void FillOtherRows()
        {
            for (int row = 1; row < word.Length; row++)
            {
                for (int column = 0; column < word.Length - row; column++)
                {
                    CykTable[row][column] = TableField<A>.CreateTableFieldForCYKTable(CykTable, row, column, epsilonSymbols, cfg);
                }
            }
        }

        private void FillBasicRow()
        {
            for (int i = 0; i < word.Length; i++)
            {
                CykTable[0][i] = TableField<A>.WithTerminal((Exprinal<A>) cfg.Cfg.GetNonVariableSymbols().First(a => a.Name.Equals(word[i].ToString())), epsilonSymbols, cfg);
            }
        }
    }
}
