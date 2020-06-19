using System;
using System.Collections.Generic;
using System.Linq;
using AutomataPDL.PDA.PDA;

namespace AutomataPDL.PDA.PDARunnerDirect
{
    internal class Node<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        IEnumerable<Node<A, S>> children = new List<Node<A, S>>();

        internal Configuration<A, S> Config { get; }

        AcceptanceCondition acceptanceCondition;
        internal bool HasAcceptedWord { get; private set; } = false;

        readonly int maximumNumberOfSteps;
        int currentMaximumNumberOfSteps;

        internal void ResetCurrentMaximumNumberOfSteps()
        {
            currentMaximumNumberOfSteps = maximumNumberOfSteps;
        }

        Node(AcceptanceCondition acceptanceCondition, Configuration<A, S> config, int maximumNumberOfSteps)
        {
            this.acceptanceCondition = acceptanceCondition;
            Config = config;
            this.maximumNumberOfSteps = maximumNumberOfSteps;
            CheckIfWordIsAccepted();
        }

        Node(Node<A, S> parent, Transition<A, S> transitionToHere, AcceptanceCondition acceptanceCondition, int maximumNumberOfSteps) : this(acceptanceCondition, new Configuration<A, S>(parent.Config, transitionToHere), maximumNumberOfSteps)
        {
            if (Config.RemainingWord.Length < parent.Config.RemainingWord.Length)
            {
                currentMaximumNumberOfSteps = maximumNumberOfSteps;
            }
            else if (Config.Stack.Length < parent.Config.Stack.Length)
            {
                currentMaximumNumberOfSteps = parent.currentMaximumNumberOfSteps;
            }
            else
            {
                currentMaximumNumberOfSteps = parent.currentMaximumNumberOfSteps - 1;
            }
        }

        internal Node(Configuration<A, S> config, AcceptanceCondition acceptanceCondition, int maximumNumberOfSteps) : this(acceptanceCondition, config, maximumNumberOfSteps)
        {
            currentMaximumNumberOfSteps = maximumNumberOfSteps;
        }

        private void CheckIfWordIsAccepted()
        {
            if (Config.RemainingWord.IsEmpty())
            {
                if (Config.State.Final && acceptanceCondition.IsFinalState())
                {
                    HasAcceptedWord = true;
                }
                else if (Config.Stack.IsEmpty() && acceptanceCondition.IsEmptyStack())
                {
                    HasAcceptedWord = true;
                }
            }
        }

        internal Tuple<IEnumerable<Node<A, S>>, IEnumerable<Transition<A, S>>> DoStep(bool resetCurrentNumberOfSteps)
        {
            if (resetCurrentNumberOfSteps)
            {
                ResetCurrentMaximumNumberOfSteps();
            }
            if (currentMaximumNumberOfSteps == 0)
            {
                return new Tuple<IEnumerable<Node<A, S>>, IEnumerable<Transition<A, S>>>(new List<Node<A, S>>(), new List<Transition<A, S>>());
            }
            var transitionsToEnter = Config.GetEnterableTransitions();
            children = transitionsToEnter.Select(t => new Node<A, S>(this, t, acceptanceCondition, maximumNumberOfSteps)).ToList();
            return new Tuple<IEnumerable<Node<A, S>>, IEnumerable<Transition<A, S>>>(children, transitionsToEnter);
        }
    }
}
