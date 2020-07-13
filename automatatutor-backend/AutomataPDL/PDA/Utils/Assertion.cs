using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.PDA.Utils
{
    internal class Assertion
    {
        internal static void Assert(bool condition, string errorDescription)
        {
            if (!condition)
            {
                throw new Exception("Illegal state: " + errorDescription);
            }
        }

        internal static void Assert(bool condition, Func<Exception> getException)
        {
            if (!condition)
            {
                throw getException();
            }
        }
    }
}
