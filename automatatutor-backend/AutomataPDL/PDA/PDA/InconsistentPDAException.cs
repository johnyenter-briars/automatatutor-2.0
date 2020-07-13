using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.PDA
{
    /// <summary>
    /// thrown if a PDA occurs that has <seealso cref="AutomataPDL.PDA.PDA.AcceptanceCondition.FinalStateAndEmptyStack"/>, 
    /// but accepts a word by final state and not by empty stack, or the other way round
    /// </summary>
    class InconsistentPDAException : Exception
    {
        public InconsistentPDAException(string message) : base(message)
        {
        }
    }
}
