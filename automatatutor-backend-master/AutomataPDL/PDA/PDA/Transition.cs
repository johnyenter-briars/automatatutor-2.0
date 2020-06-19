using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutomataPDL.PDA.PDA
{
    public class Transition<A, S> : IEquatable<Transition<A, S>> where A : IEquatable<A> where S : IEquatable<S>
    {
        internal State<A, S> Origin { get; }
        internal State<A, S> Target { get; }
        internal Symbol<A> SymbolIn { get; }
        internal S StackSymbolIn { get; }
        /// <summary>
        /// the first element is the top most of the new stack
        /// </summary>
        internal S[] StackSymbolsWritten { get; }

        public Transition(State<A, S> origin, State<A, S> target, Symbol<A> symbolIn, S stackSymbolIn, S[] stackSymbolsWritten)
        {
            Origin = origin;
            Target = target;
            SymbolIn = symbolIn;
            StackSymbolIn = stackSymbolIn;
            StackSymbolsWritten = stackSymbolsWritten;
        }

        public string Id
        {
            get
            {
                return Origin.Id + "-" + Target.Id + "." + SymbolIn.ToString() + "," + StackSymbolIn.ToString() + "/" + String.Join(",", StackSymbolsWritten);
            }
        }

        public bool Equals(Transition<A, S> other)
        {
            return Origin.Id == other.Origin.Id
                && Target.Id == other.Target.Id
                && SymbolIn.Equals(other.SymbolIn)
                && StackSymbolIn.Equals(other.StackSymbolIn)
                && StackSymbolsWritten.SequenceEqual(other.StackSymbolsWritten);
        }

        internal static bool HasNormalForm(Symbol<A> symbolIn, S[] stackSymbolsWritten)
        {
            return (symbolIn.IsEmpty() && stackSymbolsWritten.Count() == 0)
                || (!symbolIn.IsEmpty() && stackSymbolsWritten.Count() <= 2);
        }

        internal XElement ToXml()
        {
            return new XElement("transition", SymbolIn.ToString() + "," + StackSymbolIn.ToString() + "/" + String.Join("", StackSymbolsWritten));
        }
    }
}
