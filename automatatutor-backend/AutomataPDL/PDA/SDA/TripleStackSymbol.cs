using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.SDA
{
    public class TripleStackSymbol<S> : IEquatable<TripleStackSymbol<S>> where S : IEquatable<S>
    {
        internal int State1 { get; }
        internal S StackSymbol { get; }
        internal int State2 { get; }

        public TripleStackSymbol(int state1, S stackSymbol, int state2)
        {
            State1 = state1;
            StackSymbol = stackSymbol;
            State2 = state2;
        }

        public bool Equals(TripleStackSymbol<S> other)
        {
            return State1 == other.State1
                && StackSymbol.Equals(other.StackSymbol)
                && State2 == other.State2;
        }

        /// <summary>
        /// source: https://stackoverflow.com/a/263416
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + State1.GetHashCode();
                hash = hash * 23 + StackSymbol.GetHashCode();
                hash = hash * 23 + State2.GetHashCode();
                return hash;
            }            
        }

        internal static string GetKey(int state1, S stackSymbol, int state2)
        {
            return state1 + stackSymbol.ToString() + state2;
        }

        internal string GetKey()
        {
            return GetKey(State1, StackSymbol, State2);
        }

        public override string ToString()
        {
            return State1 + StackSymbol.ToString() + State2;
        }
    }
}
