using AutomataPDL.PDA.PDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.PDARunner
{
    internal interface IPDARunner<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        AcceptanceResult<S> IsWordAccepted(IEnumerable<A> word);
    }
}
