using System;
using System.Collections.Generic;
using AutomataPDL.CFG;

namespace ProblemGeneration
{
    public class GrammarGenerator
    {

        static Random rnd = new Random();

        public static ContextFreeGrammar GenerateRandom()
        {
            Nonterminal startS = new Nonterminal("S");
            List<Nonterminal> nts = new List<Nonterminal>();
            nts.Add(startS);

            //nonterminals
            int ntcount = rnd.Next(2, 6);
            while (nts.Count < ntcount)
            {
                var newNt = new Nonterminal("" + (char)('A' + rnd.Next(26))); //random NT name
                if (!nts.Contains(newNt)) nts.Add(newNt);
            }

            //productions
            List<Production> prods = new List<Production>();
            prods.Add(generateRandomProduction(startS, nts, 0, 1, 0.45)); //add a production for Start with only NTs
            foreach (Nonterminal nt in nts)
            {
                prods.Add(generateRandomProduction(nt, nts)); //at least 1 prod per NT
                while (rnd.NextDouble() < 0.5) prods.Add(generateRandomProduction(nt, nts)); //generate more prods
            }

            return new ContextFreeGrammar(startS, prods);
        }

        private static Production generateRandomProduction(Nonterminal lhs, List<Nonterminal> nts, double epsylonProb = 0.2, double nontermProb = 0.4, double lengthIncreaseProb = 0.5)
        {
            List<GrammarSymbol> rhs = new List<GrammarSymbol>();

            //epsylon
            if (rnd.NextDouble() < epsylonProb) return new Production(lhs, rhs.ToArray());

            do
            {
                if (rnd.NextDouble() < nontermProb) rhs.Add(nts[rnd.Next(nts.Count)]); // --> nonterminal 
                else
                { // --> terminal
                    char term = (char)('a' + rnd.Next(10));
                    rhs.Add(new Exprinal<String>("" + term, "" + term));
                }
            } while (rnd.NextDouble() < lengthIncreaseProb); //length of right hand side 

            return new Production(lhs, rhs.ToArray());
        }

        public static ContextFreeGrammar GenerateRandomCNF()
        {
            Nonterminal startS = new Nonterminal("S");
            List<Nonterminal> nts = new List<Nonterminal>();
            nts.Add(startS);

            //nonterminals
            int ntcount = rnd.Next(3, 6);
            while (nts.Count < ntcount)
            {
                var newNt = new Nonterminal("" + (char)('A' + rnd.Next(26))); //random NT name
                if (!nts.Contains(newNt)) nts.Add(newNt);
            }

            //terminals
            int tcount = rnd.Next(3, 6);
            List<GrammarSymbol> ts = new List<GrammarSymbol>();
            while (ts.Count < tcount)
            {
                String name = "" + (char)('a' + rnd.Next(26));
                var newT = new Exprinal<String>(name, name);
                if (!ts.Contains(newT)) ts.Add(newT);
            }

            //productions
            List<Production> prods = new List<Production>();
            prods.Add(generateRandomCNFProduction(startS, nts, ts)); //add a production for Start with only NTs
            foreach (Nonterminal nt in nts)
            {
                prods.Add(generateRandomCNFProduction(nt, nts, ts)); //at least 1 prod per NT
                while (rnd.NextDouble() < 0.6) prods.Add(generateRandomCNFProduction(nt, nts, ts)); //generate more prods
            }

            return new ContextFreeGrammar(startS, prods);
        }

        private static Production generateRandomCNFProduction(Nonterminal lhs, List<Nonterminal> nts, List<GrammarSymbol> ts, double nontermProb = 0.5)
        {
            if (rnd.NextDouble() < nontermProb)
            {
                var rhs = new GrammarSymbol[] { nts[rnd.Next(nts.Count)] , nts[rnd.Next(nts.Count)] };
                return new Production(lhs, rhs);
            }
            else
            {
                var rhs = new GrammarSymbol[] { ts[rnd.Next(ts.Count)]};
                return new Production(lhs, rhs);
            }
        }

    }
}
