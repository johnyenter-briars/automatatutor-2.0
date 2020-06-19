using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.Automata
{
    public class ThreeTuple<T1, T2, T3>
    {
        public T1 first { get; set; }
        public T2 second { get; set; }
        public T3 third { get; set; }

        public ThreeTuple(T1 first_in, T2 second_in, T3 third_in)
        {
            first = first_in;
            second = second_in;
            third = third_in;
        }

        public override bool Equals(object obj)
        {
            var other = (ThreeTuple<T1, T2, T3>)obj;

            return this.first.Equals(other.first) && this.second.Equals(other.second) && this.third.Equals(other.third);
        }

        public override int GetHashCode()
        {
            return first.GetHashCode() + second.GetHashCode() + third.GetHashCode();
        }
    }
}
