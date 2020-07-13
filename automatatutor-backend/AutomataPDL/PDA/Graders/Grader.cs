using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AutomataPDL.PDA.Graders
{
    /// <summary>
    /// class containing common functionality of the graders
    /// </summary>
    internal static class Grader
    {
        /// <summary>
        /// creates an XML element out of the grade and the feedback
        /// </summary>
        internal static XElement CreateXmlFeedback(int grade, IEnumerable<string> feedback)
        {
            var feedbackString = "<ul>" + feedback.Aggregate("", (acc, el) => acc + string.Format("<li>{0}</li>", el)) + "</ul>";
            return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>{1}</feedback></div>", grade, feedbackString));
        }
    }
}
