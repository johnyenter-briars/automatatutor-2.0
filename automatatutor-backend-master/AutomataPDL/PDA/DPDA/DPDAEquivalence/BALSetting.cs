using AutomataPDL.PDA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.DPDA.DPDAEquivalence
{
    internal class BALSetting<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        internal Node<A, S> Premise { get; }
        internal List<StackSymbolSequenceSet<S>> OwnHeads { get; }
        internal IEnumerable<S> PremiseHeads { get; }

        public BALSetting(Node<A, S> premise, List<StackSymbolSequenceSet<S>> ownHeads, IEnumerable<S> premiseHeads)
        {
            Assertion.Assert(ownHeads.Count == premiseHeads.Count(), "the own heads have to contain as many elements as the premise heads do");

            Premise = premise;
            OwnHeads = ownHeads;
            PremiseHeads = premiseHeads;
        }
    }
}
