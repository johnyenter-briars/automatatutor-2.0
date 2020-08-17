using System;
using System.Xml.Linq;

namespace ProblemGeneration
{
    public abstract class Problem
    {
        abstract public int Difficulty();
        abstract public double Quality();
        abstract protected XElement toXML();
        abstract public Generator GetGenerator();

        // Needed just for Grammar exercises, since they may be unvalid
        public virtual bool isValid()
        {
            return true;
        }

        public String TypeName()  //returns the type string used by the frontend
        {
            return GetGenerator().TypeName();
        }

        public virtual String LongDescription()
        {
            return "Difficulty: " + Difficulty() + "/100 & " + "Quality: " + ((int)Math.Round(Quality() * 100)) + "%";
        }

        public XElement Export()
        {
            return new XElement("problem",
                    new XAttribute("quality", Quality()),
                    new XAttribute("difficulty", Difficulty()),
                    new XElement("typeName", TypeName()),
                    new XElement("name", "Gen " + DateTime.Now.ToString("yyMMdd HH:mm:ss")),
                    new XElement("description", LongDescription()),
                    new XElement("specificProblem", toXML())
                );
        }
    }
}
