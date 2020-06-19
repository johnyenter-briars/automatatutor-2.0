using System;
using System.Collections.Generic;

namespace AutomataPDL.CFG
{
    public class Exprinal<T> : GrammarSymbol
    {
        public T term;
        string name;

        public Exprinal(T term, string name)
        {
            this.term = term;
            this.name = name;
        }

        public override string Name
        {
            get { return name; }
        }

        public override bool Equals(object obj)
        {
            var exprinal = obj as Exprinal<T>;
            return exprinal != null &&
                   EqualityComparer<T>.Default.Equals(term, exprinal.term) &&
                   name == exprinal.name;
        }

        public override int GetHashCode()
        {
            var hashCode = -522383019;
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(term);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(name);
            return hashCode;
        }

        public override string ToString()
        {
            return name;
        }


    }
}
