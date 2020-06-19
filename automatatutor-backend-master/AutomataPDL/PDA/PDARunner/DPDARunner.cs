using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.Simulation;
using AutomataPDL.PDA.Utils;

namespace AutomataPDL.PDA.PDARunner
{
    internal class DPDARunner<A, S> : IPDARunner<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        readonly PDA<A, S> dpda;
        private readonly CancellationToken token;

        public DPDARunner(PDA<A, S> dpda) : this(dpda, new CancellationTokenSource().Token)
        {

        }

        public DPDARunner(PDA<A, S> dpda, CancellationToken token)
        {
            Assertion.Assert(dpda.Deterministic, "the PDA is not deterministic as it is required");
            this.dpda = dpda;
            this.token = token;
        }

        public AcceptanceResult<S> IsWordAccepted(IEnumerable<A> word)
        {
            var res = new Dictionary<string, bool>();
            if (dpda.AcceptanceCondition.IsFinalState())
            {
                res[AcceptanceCondition.FinalState.GetId()] = DPDASimulationRunner<A, S>.RunSimulation(dpda, word.ToArray(), new AcceptanceCondition.FinalState(), token).WordAccepted;
            }
            if (dpda.AcceptanceCondition.IsEmptyStack())
            {
                res[AcceptanceCondition.EmptyStack.GetId()] = DPDASimulationRunner<A, S>.RunSimulation(dpda, word.ToArray(), new AcceptanceCondition.EmptyStack(), token).WordAccepted;
            }
            return new AcceptanceResult<S>(res);
        }
    }
}
