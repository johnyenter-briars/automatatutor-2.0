using System;
using System.Collections.Generic;

namespace AutomataPDL.CFG
{
    public class Nonterminal : GrammarSymbol
    {
        string name;

        public override string Name
        {
            get { return name; }
        }

        public Nonterminal(string name)
        {
            this.name = name;
        }

        public Nonterminal(int id)
        {
            this.name = "<" + id + ">";
        }

        public override bool Equals(object obj)
        {
            var nonterminal = obj as Nonterminal;
            return nonterminal != null &&
                   name == nonterminal.name;
        }

        public override int GetHashCode()
        {
            return 363513814 + EqualityComparer<string>.Default.GetHashCode(name);
        }
    }
}
