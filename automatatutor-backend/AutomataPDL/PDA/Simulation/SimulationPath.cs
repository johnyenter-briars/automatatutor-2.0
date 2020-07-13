using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutomataPDL.PDA.Simulation
{
    public class SimulationPath<A, S> where A : IEquatable<A> where S : IEquatable<S>
    {
        internal IEnumerable<Node<A, S>> Nodes { get; }
        /// <summary>
        /// if the word is not in the PDA, but a simulation is possible, this message defines why the simulation stopped
        /// </summary>
        internal string StopMessage { get; }
        internal bool WordAccepted { get; }

        public SimulationPath(IEnumerable<Node<A, S>> path)
        {
            Nodes = path;
            WordAccepted = true;
        }

        public SimulationPath(IEnumerable<Node<A, S>> path, string stopMessage) : this(path)
        {
            WordAccepted = false;
            StopMessage = stopMessage;
        }

        public XElement ToXml()
        {
            var nodes = Nodes.Select(node => node.ToXml());
            var name = "path";
            if (WordAccepted)
            {
                return new XElement(name, nodes);
            }
            else
            {
                return new XElement(name, new XAttribute("stop", StopMessage), nodes);
            }
        }
    }
}
