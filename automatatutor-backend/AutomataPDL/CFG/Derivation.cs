using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.CFG
{
    public class Derivation
    {
        public const int DERIVATION_ALL = 0;
        public const int DERIVATION_LEFTMOST = 1;
        public const int DERIVATION_RIGHTMOST = 2;
        public static readonly DerivationComparator comparator = new DerivationComparator();

        public static HashSet<GrammarSymbol[]> findAllDerivations(IEnumerable<Production> productions, GrammarSymbol[] partialWord, int type = DERIVATION_ALL)
        {
            var result = new HashSet<GrammarSymbol[]>(comparator);

            //find first and last NT
            int first_NT = -1; 
            int last_NT = -1;
            for (int i = 0; i < partialWord.Length; i++)
            {
                if (partialWord[i] is Nonterminal && first_NT == -1) first_NT = i;
                if (partialWord[i] is Nonterminal) last_NT = i;
            }

            //find all next steps
            for (int sym_i = 0; sym_i < partialWord.Length; sym_i++)
            {
                if (!(partialWord[sym_i] is Nonterminal)) continue; //not a NT
                if (type == DERIVATION_LEFTMOST && sym_i != first_NT) continue; //should be leftmost derivatation 
                if (type == DERIVATION_RIGHTMOST && sym_i != last_NT) continue; //should be rightmost derivation

                Nonterminal nt = (Nonterminal)partialWord[sym_i];
                foreach (Production p in productions)
                {
                    if (!p.Lhs.Equals(nt)) continue;

                    //build new partial word
                    var npw = applyPrduction(partialWord, sym_i, p);

                    result.Add(npw);
                }
            }

            return result;
        }

        public static GrammarSymbol[] applyPrduction(GrammarSymbol[] partialWord, int pos, Production p)
        {
            if (partialWord == null || p == null) return null; //null as parameter!

            if (pos < 0 || pos > partialWord.Length) return null; //pos out of bounds
            if (!partialWord[pos].Equals(p.Lhs)) return null; //can't apply

            var npw = new GrammarSymbol[partialWord.Length + p.Rhs.Length - 1];
            for (int i = 0; i < pos; i++) npw[i] = partialWord[i];
            for (int i = 0; i < p.Rhs.Length; i++) npw[pos + i] = p.Rhs[i];
            for (int i = pos + 1; i < partialWord.Length; i++) npw[p.Rhs.Length - 1 + i] = partialWord[i];

            return npw;
        }

        public static bool isValidDerivationStep(IEnumerable<Production> productions, GrammarSymbol[] start, GrammarSymbol[] end, int type = DERIVATION_ALL)
        {
            return findAllDerivations(productions, start, type).Contains(end, new DerivationComparator());
        }

        public static int countNT(GrammarSymbol[] pw)
        {
            return pw.Count(gs => (gs is Nonterminal));
        }

        public static string partialWordToString(GrammarSymbol[] pw)
        {
            if (pw == null || pw.Length == 0) return "_";
            string res = pw[0].ToString();
            bool wasnt = pw[0] is Nonterminal;
            for (int i = 1; i < pw.Length; i++)
            {
                if (pw[i] is Nonterminal)
                {
                    if (wasnt) res += " ";
                    wasnt = true;
                }
                else wasnt = false;
                res += pw[i].ToString();
            }
            return res;
        }

        public static string derivationToString(List<GrammarSymbol[]> d)
        {
            if (d == null || d.Count == 0) return "";
            string res = partialWordToString(d[0]);
            for (int i = 1; i < d.Count; i++) res += "\n" + partialWordToString(d[i]);
            return res;
        }
    }

    public class DerivationComparator : IEqualityComparer<GrammarSymbol[]>
    {
        public bool Equals(GrammarSymbol[] x, GrammarSymbol[] y)
        {
            return Enumerable.SequenceEqual(x, y);
        }

        public int GetHashCode(GrammarSymbol[] obj)
        {
            if (obj == null) return 0;
            unchecked
            {
                int hash = 19;
                foreach (var gs in obj)
                {
                    hash = hash * 23 + ((gs != null) ? gs.GetHashCode() : 0);
                }
                return hash;
            }
        }
    }
}
