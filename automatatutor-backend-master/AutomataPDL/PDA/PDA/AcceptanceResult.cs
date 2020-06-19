using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomataPDL.PDA.Utils;

namespace AutomataPDL.PDA.PDA
{
    /// <summary>
    /// represents the result of the decision of a word problem for a specific word and a specific PDA
    /// </summary>
    /// <typeparam name="S">type of the stack symbols</typeparam>
    public class AcceptanceResult<S>
    {
        /// <summary>
        /// defines for one of the acceptance condition ids or for both, if the word is accepted by the PDA under this acceptance condition
        /// </summary>
        internal Dictionary<string, bool> AcceptedByAcceptedCondition { get; }

        public AcceptanceResult(Dictionary<string, bool> acceptedByAcceptedCondition)
        {
            Assertion.Assert(acceptedByAcceptedCondition.Count >= 1 && acceptedByAcceptedCondition.Count <= 2, "there are only two different elementary accepted conditions");
            AcceptedByAcceptedCondition = acceptedByAcceptedCondition;
        }

        public static AcceptanceResult<S> NoAcceptance()
        {
            return new AcceptanceResult<S>(new Dictionary<string, bool>()
            {
                {AcceptanceCondition.FinalState.GetId(), false },
                {AcceptanceCondition.EmptyStack.GetId(), false }
            });
        }

        /// <summary>
        /// whether the acceptance of the word is inconsistent, that means that the PDA has <see cref="AutomataPDL.PDA.PDA.AcceptanceCondition.FinalStateAndEmptyStack"/>, 
        /// but the word is accepted by empty stack and not by final state, or the other way round
        /// </summary>
        /// <returns></returns>
        public bool IsInconsistent()
        {
            return AcceptedByAcceptedCondition.Count == 2
                && AcceptedByAcceptedCondition[AcceptanceCondition.FinalState.GetId()] != AcceptedByAcceptedCondition[AcceptanceCondition.EmptyStack.GetId()];
        }

        public bool Accepts()
        {
            return AcceptedByAcceptedCondition.All(c => c.Value);
        }
    }
}
