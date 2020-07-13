using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.DPDA.DPDAEquivalence
{
    public class HeadTailForm<S> where S : IEquatable<S>
    {
        public StackSymbolSequenceSet<S> Head { get; }
        public StackSymbolSequenceSet<S> Tail { get; }

        public HeadTailForm(StackSymbolSequenceSet<S> head, StackSymbolSequenceSet<S> tail)
        {
            Head = head;
            Tail = tail;
        }
    }
}
