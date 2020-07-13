using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.PDA
{
    public abstract class AcceptanceCondition : IEquatable<AcceptanceCondition>
    {
        const string finalState = "finalState";
        const string emptyStack = "emptyStack";
        const string finalStateAndEmptyStack = "finalStateAndEmptyStack";

        AcceptanceCondition()
        {
        }

        internal static AcceptanceCondition GetAcceptanceConditionById(string acceptanceConditionId)
        {
            switch(acceptanceConditionId)
            {
                case finalState: return new FinalState();
                case emptyStack: return new EmptyStack();
                case finalStateAndEmptyStack: return new FinalStateAndEmptyStack();
                default: throw new ArgumentException("the given acceptance condition id does not exist");
            }
        }

        public abstract bool IsFinalState();
        public abstract bool IsEmptyStack();
        public abstract AcceptanceCondition Clone();

        public abstract bool Equals(AcceptanceCondition other);

        public class EmptyStack : AcceptanceCondition
        {
            public EmptyStack()
            {
            }

            public override bool IsEmptyStack()
            {
                return true;
            }

            public override bool IsFinalState()
            {
                return false;
            }

            public static string GetId()
            {
                return emptyStack;
            }

            public override bool Equals(AcceptanceCondition other)
            {
                return other.IsEmptyStack() && !other.IsFinalState();
            }

            public override AcceptanceCondition Clone()
            {
                return AcceptanceCondition.GetAcceptanceConditionById(GetId());
            }
        }

        public class FinalState : AcceptanceCondition
        {
            public FinalState()
            {
            }

            public override bool IsEmptyStack()
            {
                return false;
            }

            public override bool IsFinalState()
            {
                return true;
            }

            public static string GetId()
            {
                return finalState;
            }

            public override bool Equals(AcceptanceCondition other)
            {
                return other.IsFinalState() && !other.IsEmptyStack();
            }
            public override AcceptanceCondition Clone()
            {
                return AcceptanceCondition.GetAcceptanceConditionById(GetId());
            }
        }

        public class FinalStateAndEmptyStack : FinalState
        {
            public FinalStateAndEmptyStack() : base()
            {
            }

            public override bool IsEmptyStack()
            {
                return true;
            }

            public override bool IsFinalState()
            {
                return true;
            }

            public override bool Equals(AcceptanceCondition other)
            {
                return other.IsEmptyStack() && other.IsFinalState();
            }
            public override AcceptanceCondition Clone()
            {
                return AcceptanceCondition.GetAcceptanceConditionById(GetId());
            }
        }
    }
}
