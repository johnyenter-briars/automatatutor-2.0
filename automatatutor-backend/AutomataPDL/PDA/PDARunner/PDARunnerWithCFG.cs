using AutomataPDL.CFG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.CFGUtils;
using AutomataPDL.PDA.Simulation;
using System.Threading;

namespace AutomataPDL.PDA.PDARunner
{
    internal class PDARunnerWithCFG<A, S> : IPDARunner<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        Dictionary<string, ContextFreeGrammar> contextFreeGrammarsInCNF = new Dictionary<string, ContextFreeGrammar>();

        private bool noWordIsAccepted = false;

        internal PDARunnerWithCFG(PDA<A, S> pda) : this(pda, new CancellationTokenSource().Token)
        {
        }

        internal PDARunnerWithCFG(PDA<A, S> pda, CancellationToken token)
        {
            if (pda.AcceptanceCondition.IsEmptyStack())
            {
                AddCFGOfPDA(pda, AcceptanceCondition.EmptyStack.GetId(), token);
            }
            if (pda.AcceptanceCondition.IsFinalState())
            {
                AddCFGOfPDA(PDATransformer<A, S>.ToPDAWithEmptyStack(pda), AcceptanceCondition.FinalState.GetId(), token);
            }
        }

        private void AddCFGOfPDA(PDA<A, S> pda, string id, CancellationToken token)
        {
            var cfg = PDAToCFGConverter<A, S>.ToCFG(pda, token);
            var cleanedCfg = CFGCleaner.RemoveUselessSymbols(cfg, token);
            if (cleanedCfg == null)
            {
                noWordIsAccepted = true;
            }
            else
            {
                var cfgInCNF = GrammarUtilities.getEquivalentCNF(cleanedCfg);
                contextFreeGrammarsInCNF.Add(id, cfgInCNF);
            }
        }

        public AcceptanceResult<S> IsWordAccepted(IEnumerable<A> word)
        {
            var wordAsString = string.Join("", word.Select(w => w.ToString()));
            if (noWordIsAccepted)
            {
                return AcceptanceResult<S>.NoAcceptance();
            }
            var res = contextFreeGrammarsInCNF.Select(cfg => new KeyValuePair<string, bool>(cfg.Key, cfg.Value != null && GrammarUtilities.isWordInGrammar(cfg.Value, wordAsString))).ToList();
            return new AcceptanceResult<S>(res.ToDictionary(k => k.Key, k => k.Value));
        }
    }
}
