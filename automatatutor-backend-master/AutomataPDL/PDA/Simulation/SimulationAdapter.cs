using AutomataPDL.PDA.PDA;
using AutomataPDL.PDA.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutomataPDL.PDA.Simulation
{
    public static class SimulationAdapter
    {
        private const string ns = "http://automatagrader.com/";

        public static XElement RunSimulationAsync(XElement xmlPda, XElement xmlWord)
        {
            int timeOut = 10000;
            try
            {
                //var task = TimeGuard<XElement>.RunAsync(token => TryToRunSimulation(xmlPda, xmlWord, token), timeOut);
                return TimeGuard<XElement>.Run(token => TryToRunSimulation(xmlPda, xmlWord, token), timeOut);
                //task.Wait();
                //return task.Result;
            }
            catch (TimeoutException)
            {
                return Error("Timeout - your inputs seem to be too big");
            }
            /*catch (AggregateException)
            {
                return Error("Timeout - your inputs seem to be too big");
            }*/
        }

        private static XElement TryToRunSimulation(XElement xmlPda, XElement xmlWord, CancellationToken token)
        {
            var pda = PDA<char, char>.FromXml(xmlPda);
            var word = xmlWord.Value;

            try
            {
                SimulationPath<char, char> path;

                if (pda.Deterministic)
                {
                    path = DPDASimulationRunner<char, char>.RunSimulation(pda, word.ToArray(), pda.AcceptanceCondition, token);
                }
                else
                {
                    path = CFGSimulationRunner<char>.RunSimulation(pda, word.ToArray(), token);
                }
                XNamespace xNamespace = @ns;
                return new XElement(xNamespace + "div", path.ToXml());
            }
            catch(InconsistentPDAException ex)
            {
                return Error(ex.Message);
            }
            catch(NoAcceptanceException ex)
            {
                return Error(ex.Message);
            }
        }

        private static XElement Error(string errorMsg)
        {
            XNamespace xNamespace = @ns;
            return new XElement(xNamespace + "div", new XElement("error", errorMsg));
        }
    }
}
