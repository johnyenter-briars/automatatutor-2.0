using System;
using System.Collections.Generic;
using AutomataPDL.CFG;
using AutomataPDL.PDA.CFGUtils.CYKTable;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestPDL.PDATest.CFG
{
    [TestClass]
    public class CFGTest
    {
        [TestMethod]
        public void TestCYKTable()
        {
            var startSymbol = new Nonterminal("0");
            var nonTerminals = new Dictionary<string, Nonterminal>()
            {
                {"0", startSymbol },
                {"A", new Nonterminal("A") },
                {"B", new Nonterminal("B") },
                {"C", new Nonterminal("C") },
                {"D", new Nonterminal("D") },
                {"E", new Nonterminal("E") },
                {"F", new Nonterminal("F") },
            };
            var terminals = new Dictionary<char, Exprinal<char>>()
            {
                { 'a', new Exprinal<char>('a', "a") },
                {'d', new Exprinal<char>('d', "d") }
            };
            var productions = new List<Production>()
            {
                new Production(startSymbol, nonTerminals["A"], nonTerminals["B"]),
                new Production(nonTerminals["A"], nonTerminals["A"]),
                new Production(nonTerminals["A"], nonTerminals["C"]),
                new Production(nonTerminals["C"], terminals['a']),
                new Production(nonTerminals["C"], nonTerminals["E"], nonTerminals["F"]),
                new Production(nonTerminals["B"], nonTerminals["D"], nonTerminals["E"]),
                new Production(nonTerminals["D"], terminals['d']),
                new Production(nonTerminals["E"]),
                new Production(nonTerminals["F"])
            };
            var cfg = new ContextFreeGrammar(startSymbol, productions);

            var cykTable1 = new CYKTable<char>("d".ToCharArray(), cfg);

            Assert.IsTrue(cykTable1.CykTable[0][0].Entries.ContainsKey(startSymbol.UnqiueId));

            var cykTable2 = new CYKTable<char>("ad".ToCharArray(), cfg);

            Assert.IsTrue(cykTable2.CykTable[1][0].Entries.ContainsKey(startSymbol.UnqiueId));

            var cykTable3 = new CYKTable<char>("add".ToCharArray(), cfg);

            Assert.IsFalse(cykTable3.CykTable[2][0].Entries.ContainsKey(startSymbol.UnqiueId));
        }
    }
}
