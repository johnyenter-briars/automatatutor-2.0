using System;
using System.Collections.Generic;
using System.Linq;
using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.Utils;

namespace AutomataPDL.PDA.PDARunnerDirect
{
    public class PDARunner<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        public static bool IsWordAccepted(PDA<A, S> pda, A[] word)
        {
            if (pda.AcceptanceCondition.IsEmptyStack() && pda.AcceptanceCondition.IsFinalState())
            {
                var acceptedByFinalState = IsWordAccepted(pda, word, new AcceptanceCondition.FinalState());
                var acceptedByEmptyStack = IsWordAccepted(pda, word, new AcceptanceCondition.EmptyStack());
                Assertion.Assert(acceptedByFinalState == acceptedByEmptyStack, () => new InconsistentPDAException(String.Format("the acceptance conditions of the pda are final-state and empty-stack, but the acceptance of \"{0}\" is inconsistent: for final-state is {1}, for empty stack is {2}", word, acceptedByFinalState, acceptedByEmptyStack)));
                return acceptedByFinalState;
            }
            return IsWordAccepted(pda, word, pda.AcceptanceCondition);
        }

        internal static bool IsWordAccepted(PDA<A, S> pda, A[] word, AcceptanceCondition acceptanceCondition)
        {
            Assertion.Assert(!(acceptanceCondition.IsFinalState() && acceptanceCondition.IsEmptyStack()) , "you can only check the acceptance of a word for a single condition");
            var numberOfTransitions = pda.States.Sum(s => s.Value.Transitions.Count());
            var startNode = new Node<A, S>(new Configuration<A, S>(pda.InitialState, new Word<A, S>(word), new Stack<A, S>(pda.FirstStackSymbol)), acceptanceCondition, numberOfTransitions + 1);

            IEnumerable<Node<A, S>> frontChain = new List<Node<A, S>> { startNode };
            var transitionsEnteredSoFar = new HashSet<Transition<A, S>>();
            int minimumRemainingWordLengthSoFar = word.Length;

            bool resetCurrentMaximumNumberOfSteps = false;

            while (frontChain.Count() > 0)
            {
                minimumRemainingWordLengthSoFar = frontChain.Min(n => n.Config.RemainingWord.Length);
                var nodesAcceptedWord = frontChain.Where(node => node.HasAcceptedWord).ToList();
                if (nodesAcceptedWord.Count() > 0)
                {
                    return true;
                }
                var result = frontChain.Select(node => node.DoStep(resetCurrentMaximumNumberOfSteps)).ToList();
                frontChain = result.SelectMany(r => r.Item1).ToList();
                var enteredTransitions = result.SelectMany(r => r.Item2);
                if (enteredTransitions.Any(t => !transitionsEnteredSoFar.Contains(t) || frontChain.Any(n => n.Config.RemainingWord.Length < minimumRemainingWordLengthSoFar)))
                {
                    resetCurrentMaximumNumberOfSteps = true;
                }
                else
                {
                    resetCurrentMaximumNumberOfSteps = false;
                }
                transitionsEnteredSoFar.UnionWith(enteredTransitions);
            }

            return false;
        } 
    }
}
