using AutomataPDL.PDA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.PDA
{
    public class CurrentStack<S> where S : IEquatable<S>
    {
        internal IEnumerable<S> StackSymbols { private set; get; } //the first element is the top-most one of the stack

        public CurrentStack(IEnumerable<S> stackSymbols)
        {
            StackSymbols = stackSymbols;
        }

        internal bool IsEmpty()
        {
            return StackSymbols.Count() == 0;
        }

        public static CurrentStack<S> WithSingleSymbol(S symbol)
        {
            return new CurrentStack<S>(new List<S>
            {
                symbol
            });
        }

        public bool HasSameTopMostSymbolsLike(CurrentStack<S> otherStack, int number)
        {
            if (StackSymbols.Count() < number || otherStack.StackSymbols.Count() < number)
            {
                return false;
            }
            return StackSymbols.Take(number).SequenceEqual(otherStack.StackSymbols.Take(number));
        }

        public static CurrentStack<S> ApplyTransitionToStack<A>(Transition<A, S> transition, CurrentStack<S> stackBefore) where A : IEquatable<A>
        {
            Assertion.Assert(!stackBefore.IsEmpty(), "a transition cannot be applied to an empty stack");
            Assertion.Assert(transition.StackSymbolIn.Equals(stackBefore.StackSymbols.First()), "the input-stack-symbol of the transition has to be the same as the first one of current stack");
            return new CurrentStack<S>(transition.StackSymbolsWritten.Concat(stackBefore.StackSymbols.Skip(1)).ToList());
        }

        public override string ToString()
        {
            return string.Join("", StackSymbols);
        }
    }
}
