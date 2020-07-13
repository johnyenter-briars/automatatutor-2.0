using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AutomataPDL.CFG;

namespace ProblemGeneration
{
    public class FindDerivationGenerator : Generator<FindDerivationProblem>
    {
        private static Random rnd = new Random();
        private static FindDerivationGenerator singleton = null;

        //singleton
        private FindDerivationGenerator() { } //private constructor
        public static FindDerivationGenerator GetInstance()
        {
            if (singleton == null) singleton = new FindDerivationGenerator();
            return singleton;
        }

        override
        public FindDerivationProblem Generate(int targetLevel)
        {
            ContextFreeGrammar grammar = GrammarGenerator.GenerateRandom();
            var productions = grammar.GetProductions();
            var toAdd = new List<Production>();

            //make sure every varaible has a resolution (production with #NT = 0) and an continuation (production with #NT > 0)
            foreach (Nonterminal nt in grammar.Variables)
            {
                var nt_prods = grammar.GetProductions(nt);
                //resolution check
                var p = nt_prods.ToList().Find(pp => pp.ContainsNoVariables);
                if (p == null) {
                    GrammarSymbol t = Choose(grammar.GetNonVariableSymbols()); 
                    if (t == null) t = new Exprinal<char>('x', "x");
                    p = new Production(nt, new GrammarSymbol[] { t });
                    toAdd.Add(p);
                }
                //continuation check
                p = nt_prods.ToList().Find(pp => Derivation.countNT(pp.Rhs) >= 1);
                if (p == null)
                {
                    //resolution check
                    GrammarSymbol t = Choose(grammar.Variables);
                    p = new Production(nt, new GrammarSymbol[] { Choose(grammar.Variables), Choose(grammar.Variables) });
                    toAdd.Add(p);
                }
            }
            //add new productions (for resolution and continuation)
            if (toAdd.Count != 0) grammar = new ContextFreeGrammar(grammar.StartSymbol, grammar.GetProductions().Concat(toAdd));
            productions = grammar.GetProductions();

            //type
            bool shouldBeAny = true;
            if (targetLevel == Generation.ANY) shouldBeAny = (rnd.Next(0,9) < 3); // 30%
            if (targetLevel == Generation.MEDIUM) shouldBeAny = (rnd.Next(0, 9) < 2); // 20%
            if (targetLevel == Generation.HIGH) shouldBeAny = (rnd.Next(0, 9) < 9); // 90%
            int type = Derivation.DERIVATION_ALL;
            if (!shouldBeAny)
            {
                type = Derivation.DERIVATION_LEFTMOST;
                if (rnd.Next(0, 9) < 3) type = Derivation.DERIVATION_RIGHTMOST;
            }

            //nr of steps
            int steps = rnd.Next(5, 10);
            if (targetLevel == Generation.LOW) steps = rnd.Next(3, 6);
            if (targetLevel == Generation.MEDIUM) steps = rnd.Next(7, 9);
            if (targetLevel == Generation.HIGH) steps = rnd.Next(10, 12);

            List<GrammarSymbol[]> derivation = new List<GrammarSymbol[]>(steps + 1);
            var cur = new GrammarSymbol[] { grammar.StartSymbol };
            derivation.Add(cur);
            for(int i = 0; i < steps; i++)
            {
                //get possible next steps
                IEnumerable<GrammarSymbol[]> next = Derivation.findAllDerivations(grammar.GetProductions(), cur, type);

                if (Derivation.countNT(cur) <= 1 && i != steps -1) //don't end yet!
                {
                    next = next.Where(pw => Derivation.countNT(pw) >= 1);
                }

                //try not to repeat
                var next_part = next.Except(derivation);
                if (next_part.Count() > 0) next = next_part;

                cur = Choose(next);
                //cut out repeat
                if (derivation.Contains(cur))
                {
                    int r_i = derivation.IndexOf(cur);
                    derivation = derivation.GetRange(0, r_i);
                }

                //add next step
                derivation.Add(cur);
            }
            //replace all NTs
            while (Derivation.countNT(cur) > 0)
            {
                //get possible next steps
                IEnumerable<GrammarSymbol[]> next = Derivation.findAllDerivations(grammar.GetProductions(), cur, type);

                //filter
                int curNTs = Derivation.countNT(cur);
                next = next.Where(pw => Derivation.countNT(pw) < curNTs);

                cur = Choose(next);
                derivation.Add(cur);
            }

            String word = Derivation.partialWordToString(cur);

            return new FindDerivationProblem(grammar, word, type, derivation);
        }

        public override string TypeName()
        {
            return "Find Derivation";
        }

        public static T Choose<T>(IEnumerable<T> possibilities)
        {
            if (possibilities == null) return default(T);
            if (possibilities.Count() == 0) return default(T);

            int i = rnd.Next(0, possibilities.Count() - 1);
            return possibilities.ElementAt(i);
        }
    }

    public class FindDerivationProblem : Problem {

        private ContextFreeGrammar grammar;
        private String word;
        private int derivationType;
        private List<GrammarSymbol[]> derivation;

        public FindDerivationProblem(ContextFreeGrammar grammar, string word, int derivationType, List<GrammarSymbol[]> derivation)
        {
            this.grammar = grammar;
            this.word = word;
            this.derivationType = derivationType;
            this.derivation = derivation;
        }

        public override int Difficulty()
        {
            int grammarPoints = 20;
            if (grammar.Variables.Count <= 2) grammarPoints -= 5;
            if (grammar.Variables.Count <= 3) grammarPoints -= 5;
            int nr_prod = grammar.GetProductions().Count();
            if (nr_prod <= 8) grammarPoints -= 5;
            if (nr_prod <= 12) grammarPoints -= 5;

            int typePoints = 20;
            if (derivationType == Derivation.DERIVATION_ALL) typePoints = 0;

            int derivationPoints = 60;
            if (derivation.Count <= 5) derivationPoints = 0;
            else derivationPoints = Math.Min(60, 4 * (derivation.Count - 5));

            return grammarPoints + typePoints + derivationPoints;
        }

        public override double Quality()
        {
            if (grammar == null || word == null || derivation == null) return 0;
            double q = 1.0;

            //to short
            if (derivation.Count <= 2) q *= 0.5;

            //too long
            if (derivation.Count >= 18) q *= 0.8;
            if (derivation.Count >= 22) q *= 0.5;

            //word too long
            if (word.Length >= 20) q *= 0.8;
            if (word.Length >= 25) q *= 0.8;
            if (word.Length >= 30) q *= 0.8;
            if (word.Length >= 35) q *= 0.8;

            return q;
        }

        override
        public String ToString()
        {
            return toXML().ToString();
        }

        protected override XElement toXML()
        {
            return new XElement("FindDerivationProblem",
                    new XElement("Grammar", grammar),
                    new XElement("Word", word),
                    new XElement("DerivationType", derivationType)
                );
        }

        public override Generator GetGenerator()
        {
            return FindDerivationGenerator.GetInstance();
        }
    }
}
