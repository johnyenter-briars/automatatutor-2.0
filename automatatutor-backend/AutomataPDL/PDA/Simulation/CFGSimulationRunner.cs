using AutomataPDL.CFG;
using AutomataPDL.PDA.CFGUtils;
using AutomataPDL.PDA.CFGUtils.CYKTable;
using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.PDARunner;
using AutomataPDL.PDA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.Simulation
{
    public static class CFGSimulationRunner<A> where A : IEquatable<A>
    {
        private static readonly string wordNotAcceptedMessage = "The pda does not accept the word, therefore, a simulation is not possible. This is only possible for DPDAs.";

        public static SimulationPath<A, char> RunSimulation(PDA<A, char> pda, A[] word)
        {
            return RunSimulation(pda, word, new CancellationTokenSource().Token);
        }
        
        public static SimulationPath<A, char> RunSimulation(PDA<A, char> pda, A[] word, CancellationToken token)
        {
            if (pda.AcceptanceCondition.IsEmptyStack())
            {
                return RunSimulationWithCFGForEmptyStack(pda, word, token);
            }
            else if (pda.AcceptanceCondition.IsFinalState())
            {
                return RunSimulationWithCFGForFinalState(pda, word, token);
            }
            throw new Exception("This should never be reached, as each pda has to have one of the acceptance conditions");
        }

        private static SimulationPath<A, char> RunSimulationWithCFGForEmptyStack(PDA<A, char> pda, A[] word, CancellationToken token)
        {
            Assertion.Assert(pda.AcceptanceCondition.IsEmptyStack(), "the pda has not empty stack as acceptance condition");

            var cfg1 = PDAToCFGConverter<A, char>.ToCFG(pda, token);

            var cfg = CFGCleaner.RemoveUselessSymbols(cfg1, token);

            if (cfg == null)
            {
                throw new NoAcceptanceException(wordNotAcceptedMessage);
            }

            var cfgIn2NF = CFGTo2NFConverter<A, char>.To2NF(cfg);
            var cykTable = new CYKTable<A>(word, cfgIn2NF);

            if (!cykTable.AcceptsWord())
            {
                throw new NoAcceptanceException(wordNotAcceptedMessage);
            }

            var startDerivationNode = cykTable.GetStartDerivationNode();

            return startDerivationNode.ConvertToPDASimulationPath(pda, cfg.StartSymbol, word);
        }

        private static SimulationPath<A, char> RunSimulationWithCFGForFinalState(PDA<A, char> pda, A[] word, CancellationToken token)
        {
            Assertion.Assert(pda.AcceptanceCondition.IsFinalState(), "the pda has not final state as acceptance condition");

            var pdaWithEmptyStack = PDATransformer<A, char>.ToPDAWithEmptyStack(pda);
            var path = RunSimulationWithCFGForEmptyStack(pdaWithEmptyStack, word, token);
            return ConvertEmptyStackSimulationPathBackToFinalState(path, pda);
        }

        private static SimulationPath<A, char> ConvertEmptyStackSimulationPathBackToFinalState(SimulationPath<A, char> path, PDA<A, char> pdaWithFinalState)
        {
            var lastStateId = path.Nodes.Last().Config.State.Id;
            var newPath = path.Nodes
                .Skip(1)
                .TakeWhile(n => n.Config.State.Id != lastStateId)
                .Select((n, i) => Node<A, char>.NodeInEmptyStackPathBackToFinalStatePath(pdaWithFinalState, n, i == 0))
                .ToList();
            return new SimulationPath<A, char>(newPath);
        }
    }
}
