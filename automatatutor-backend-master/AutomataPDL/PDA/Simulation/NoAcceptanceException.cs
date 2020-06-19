using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.Simulation
{
    class NoAcceptanceException : Exception
    {
        public NoAcceptanceException(string message) : base(message)
        {
        }
    }
}
