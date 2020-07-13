using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.Simulation;
using AutomataPDL.PDA.Simulation.DirectSimulation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestPDL.PDATest
{
    [TestClass]
    public class SimulationRunnerTest
    {
        const string xmlFileName1 = @"PDATest\XmlPath.xml";
        const string xmlFileName2 = @"PDATest\XmlPathCFG.xml";
        const string xmlFileName3 = @"PDATest\XmlPathCFG2.xml";
        const string xmlFileName4 = @"PDATest\XmlPathCFG3.xml";
        const string xmlFileName5 = @"PDATest\XmlPathCFG4.xml";
        const string xmlFileName6 = @"PDATest\XmlPathCFG5.xml";

        [TestMethod]
        [DeploymentItem(xmlFileName1)]
        public void RunSimulationTest()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "ZAB");
            pda.AddState(1, false);

            pda.AddTransition().From(0).To(0).Read('a').Pop('Z').Push("AZ");
            pda.AddTransition().From(0).To(0).Read('a').Pop('A').Push("AA");
            pda.AddTransition().From(0).To(0).Read('a').Pop('B').Push("AB");
            pda.AddTransition().From(0).To(0).Read('b').Pop('Z').Push("BZ");
            pda.AddTransition().From(0).To(0).Read('b').Pop('A').Push("BA");
            pda.AddTransition().From(0).To(0).Read('b').Pop('B').Push("BB");

            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(0).To(1).Read().Pop('A').Push("A");
            pda.AddTransition().From(0).To(1).Read().Pop('B').Push("B");
            pda.AddTransition().From(0).To(1).Read('a').Pop('Z').Push("Z");
            pda.AddTransition().From(0).To(1).Read('a').Pop('A').Push("A");
            pda.AddTransition().From(0).To(1).Read('a').Pop('B').Push("B");
            pda.AddTransition().From(0).To(1).Read('b').Pop('Z').Push("Z");
            pda.AddTransition().From(0).To(1).Read('b').Pop('A').Push("A");
            pda.AddTransition().From(0).To(1).Read('b').Pop('B').Push("B");

            pda.AddTransition().From(1).To(1).Read('a').Pop('A').Push();
            pda.AddTransition().From(1).To(1).Read('b').Pop('B').Push();
            pda.AddTransition().From(1).To(1).Read().Pop('Z').Push();

            pda.CreateRunner();

            var path = DirectSimulationRunner<char, char>.RunSimulation(pda, "abbababba".ToCharArray());

            var act = path.ToXml();

            var reader = new StreamReader(@"..\..\..\TestPDL\PDATest\XmlPath.xml");
            XmlDocument expXmlDoc = new XmlDocument();
            expXmlDoc.Load(reader);
            XDocument xDoc = XDocument.Load(new XmlNodeReader(expXmlDoc));
            var exp = xDoc.Root;

            Assert.IsTrue(XNode.DeepEquals(exp, act));
        }

        [TestMethod]
        public void RunDirectSimulationForProblemPda()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "Z");
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(0).Read('a').Pop('Z').Push();
            pda.CreateRunner();

            var s = new Stopwatch();
            s.Start();
            //adding one 'a' to the word leads to a System.OutOfMemoryException for the DirectSimulationRunner
            var path = DirectSimulationRunner<char, char>.RunSimulation(pda, "aaaaaaaaaaaa".ToCharArray());
            //var path = CFGSimulationRunner<char>.RunSimulation(pda, "aaaaaaaaaaaaaaa".ToCharArray());
            s.Stop();

            Console.WriteLine(s.ElapsedMilliseconds);
        }

        [TestMethod]
        [DeploymentItem(xmlFileName2)]
        public void RunSimulationCFGTest()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "Z");
            pda.AddState(1, false);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(0).To(1).Read('a').Pop('Z').Push();
            pda.AddTransition().From(1).To(1).Read('a').Pop('Z').Push();

            pda.CreateRunner();

            var path = CFGSimulationRunner<char>.RunSimulation(pda, "aaaa".ToCharArray());

            var act = path.ToXml();

            var reader = new StreamReader(@"..\..\..\TestPDL\PDATest\XmlPathCFG.xml");
            XmlDocument expXmlDoc = new XmlDocument();
            expXmlDoc.Load(reader);
            XDocument xDoc = XDocument.Load(new XmlNodeReader(expXmlDoc));
            var exp = xDoc.Root;

            Assert.IsTrue(XNode.DeepEquals(exp, act));
        }

        [TestMethod]
        [DeploymentItem(xmlFileName3)]
        public void RunSimulationCFGTest2()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "Z");
            pda.AddState(1, false);
            pda.AddState(2, false);
            pda.AddState(3, false);
            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(1).To(1).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(1).To(2).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(2).To(3).Read().Pop('Z').Push("ZZ");
            pda.AddTransition().From(3).To(3).Read('a').Pop('Z').Push();
            pda.AddTransition().From(3).To(0).Read().Pop('Z').Push("ZZ");

            pda.CreateRunner();

            var path = CFGSimulationRunner<char>.RunSimulation(pda, "aaaaaa".ToCharArray());

            var act = path.ToXml();

            var reader = new StreamReader(@"..\..\..\TestPDL\PDATest\XmlPathCFG2.xml");
            XmlDocument expXmlDoc = new XmlDocument();
            expXmlDoc.Load(reader);
            XDocument xDoc = XDocument.Load(new XmlNodeReader(expXmlDoc));
            var exp = xDoc.Root;

            Assert.IsTrue(XNode.DeepEquals(exp, act));
        }

        [TestMethod]
        [DeploymentItem(xmlFileName4)]
        public void RunSimulationCFGTest3()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.FinalState(), false, 'Z', false, "ZXY", 'V');
            pda.AddState(1, false);
            pda.AddState(2, true);
            pda.AddTransition().From(0).To(0).Read('a').Pop('Z').Push("XZ");
            pda.AddTransition().From(0).To(0).Read('a').Pop('X').Push("XX");
            pda.AddTransition().From(0).To(0).Read('a').Pop('Y').Push("XY");
            pda.AddTransition().From(0).To(0).Read('b').Pop('Z').Push("YZ");
            pda.AddTransition().From(0).To(0).Read('b').Pop('Y').Push("YY");
            pda.AddTransition().From(0).To(0).Read('b').Pop('X').Push("YX");
            pda.AddTransition().From(0).To(1).Read().Pop('X').Push("X");
            pda.AddTransition().From(0).To(1).Read().Pop('Y').Push("Y");
            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push("Z");
            pda.AddTransition().From(1).To(1).Read('a').Pop('X').Push();
            pda.AddTransition().From(1).To(1).Read('b').Pop('Y').Push();
            pda.AddTransition().From(1).To(2).Read().Pop('Z').Push("ZZ");

            pda.CreateRunner();

            var path = CFGSimulationRunner<char>.RunSimulation(pda, "abaabbaaba".ToCharArray());

            var act = path.ToXml();

            var reader = new StreamReader(@"..\..\..\TestPDL\PDATest\XmlPathCFG3.xml");
            XmlDocument expXmlDoc = new XmlDocument();
            expXmlDoc.Load(reader);
            XDocument xDoc = XDocument.Load(new XmlNodeReader(expXmlDoc));
            var exp = xDoc.Root;

            Assert.IsTrue(XNode.DeepEquals(exp, act));
        }

        [TestMethod]
        public void SimulationRunnerComparisonTest()
        {
            var pda = new PDA<char, char>(new AcceptanceCondition.EmptyStack(), false, 'Z', false, "Z");
            pda.AddState(1, false);
            pda.AddTransition().From(0).To(0).Read().Pop('Z').Push("ZZZZZZZZZZZZZZZZZZ");
            pda.AddTransition().From(0).To(1).Read().Pop('Z').Push();
            pda.AddTransition().From(1).To(1).Read('a').Pop('Z').Push();

            var word = "aaaaaaaaaaaaaaaaa".ToCharArray();

            var s = new Stopwatch();

            s.Start();
            var path1 = CFGSimulationRunner<char>.RunSimulation(pda, word);
            s.Stop();

            Console.WriteLine(s.ElapsedMilliseconds);

            s.Restart();
            pda.CreateRunner();
            var path2 = DirectSimulationRunner<char, char>.RunSimulation(pda, word);
            s.Stop();

            Console.WriteLine(s.ElapsedMilliseconds);
        }

        [TestMethod]
        [DeploymentItem(xmlFileName5)]
        [DeploymentItem(xmlFileName6)]
        public void DPDASimulationRunnerTest()
        {
            var dpda = new PDA<char, char>(new AcceptanceCondition.FinalStateAndEmptyStack(), true, 'Z', false, "ZXY");
            dpda.AddState(1, false);
            dpda.AddState(2, true);

            dpda.AddTransition().From(0).To(0).Read('a').Pop('Z').Push("XZ");
            dpda.AddTransition().From(0).To(0).Read('a').Pop('X').Push("XX");
            dpda.AddTransition().From(0).To(0).Read('a').Pop('Y').Push("XY");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('Z').Push("YZ");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('X').Push("YX");
            dpda.AddTransition().From(0).To(0).Read('b').Pop('Y').Push("YY");

            dpda.AddTransition().From(0).To(1).Read('v').Pop('Z').Push("Z");
            dpda.AddTransition().From(0).To(1).Read('v').Pop('X').Push("X");
            dpda.AddTransition().From(0).To(1).Read('v').Pop('Y').Push("Y");

            dpda.AddTransition().From(1).To(1).Read('a').Pop('X').Push();
            dpda.AddTransition().From(1).To(1).Read('b').Pop('Y').Push();
            dpda.AddTransition().From(1).To(2).Read().Pop('Z').Push("");

            var word = "aababbbavabbbabaa".ToCharArray();

            var watch = new Stopwatch();

            watch.Start();
            var path = DPDASimulationRunner<char, char>.RunSimulation(dpda, word, dpda.AcceptanceCondition);
            watch.Stop();

            Console.WriteLine(string.Format("For DPDA runner: {0}", watch.Elapsed));

            var act = path.ToXml();

            var reader = new StreamReader(@"..\..\..\TestPDL\PDATest\XmlPathCFG4.xml");
            XmlDocument expXmlDoc = new XmlDocument();
            expXmlDoc.Load(reader);
            XDocument xDoc = XDocument.Load(new XmlNodeReader(expXmlDoc));
            var exp = xDoc.Root;

            Assert.IsTrue(XNode.DeepEquals(exp, act));

            watch.Restart();
            path = CFGSimulationRunner<char>.RunSimulation(dpda, word);
            watch.Stop();

            Console.WriteLine(string.Format("For CFG runner: {0}", watch.Elapsed));

            Assert.IsTrue(XNode.DeepEquals(exp, act));

            path = DPDASimulationRunner<char, char>.RunSimulation(dpda, word.Concat("a").ToArray(), dpda.AcceptanceCondition);
            act = path.ToXml();

            reader = new StreamReader(@"..\..\..\TestPDL\PDATest\XmlPathCFG5.xml");
            expXmlDoc = new XmlDocument();
            expXmlDoc.Load(reader);
            xDoc = XDocument.Load(new XmlNodeReader(expXmlDoc));
            exp = xDoc.Root;

            Assert.IsTrue(XNode.DeepEquals(exp, act));
        }
    }
}
