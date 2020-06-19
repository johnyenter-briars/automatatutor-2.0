using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.Utils;

namespace AutomataPDL.PDA.PDARunnerDirect
{
    internal class Stack<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        IEnumerable<S> stackSymbols; //the first element is the top-most one of the stack

        internal Stack(S firstSymbol)
        {
            stackSymbols = new List<S>
            {
                firstSymbol
            };
        }

        internal Stack(Transition<A, S> transition, Configuration<A, S> config)
        {
            Assertion.Assert(!config.Stack.IsEmpty(), "if the stack of the parent-configuration is empty, a further step cannot be done");
            Assertion.Assert(transition.StackSymbolIn.Equals(config.Stack.stackSymbols.First()), "the input-stack-symbol of the transition has to be the same as the first one of current stack");
            stackSymbols = transition.StackSymbolsWritten.Concat(config.Stack.stackSymbols.Skip(1)).ToList();
        }

        internal bool IsEmpty()
        {
            return stackSymbols.Count() == 0;
        }

        internal int Length
        {
            get
            {
                return stackSymbols.Count();
            }
        }

        internal S First()
        {
            return stackSymbols.First();
        }

        public override string ToString()
        {
            return String.Join("", stackSymbols);
        }
    }
}
