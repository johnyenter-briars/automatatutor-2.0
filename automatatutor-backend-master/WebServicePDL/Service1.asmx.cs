using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Services;
using System.Xml.Linq;
using System.Text;

using Microsoft.Automata;
using AutomataPDL;
using AutomataPDL.PDA.Graders;
using AutomataPDL.Automata;
using AutomataPDL.CFG;
using AutomataPDL.Utilities;
using ProblemGeneration;

using System.Diagnostics;
using AutomataPDL.PDA.Simulation;
using AutomataPDL.TM;

using PumpingLemma;
using System.Text.RegularExpressions;

namespace WebServicePDL
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://automatagrader.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class Service1 : System.Web.Services.WebService
    {
        //-------- Product Construction --------//

        [WebMethod]
        public XElement ComputeFeedbackProductConstruction(XElement dfaDescList, XElement dfaAttemptDesc, XElement booleanOperation, XElement maxGrade, XElement feedbackLevel, XElement enabledFeedbacks)
        {
            //TODO: Alphabet, Arbitrary Boolean Operations

            var DfaList = AutomataUtilities.ParseDFAListFromXML(dfaDescList);
            var attemptDfa = AutomataUtilities.ParseDFAFromXML(dfaAttemptDesc);
            var boolOp = BooleanOperation.parseBooleanOperationFromXML(booleanOperation);

            var feedbackGrade = AutomataFeedback.FeedbackForProductConstruction<char>(DfaList, boolOp, attemptDfa);
            var feedString = "<ul>";
            foreach (var feed in feedbackGrade.Item2)
            {
                feedString += string.Format("<li>{0}</li>", feed);
            }
            feedString += "</ul>";

            /*
            CharSetSolver solver = new CharSetSolver(BitWidth.BV64);
            //Read input
            var dfaPairList = DFAUtilities.parseDFAListFromXML(dfaDescList, solver);
            var alphabet = dfaPairList[0].First;
            for (int i = 1; i < dfaPairList.Count; i++) {
                foreach(char c in dfaPairList[i].First) {
                    alphabet.Add(c);
                }
            }
            //var dfaCorrect = dfaPairList[0].Second;
            //for (int i = 1; i < dfaPairList.Count; i++)
            //    dfaCorrect = dfaCorrect.Intersect(dfaPairList[i].Second, solver);

            var dfaList = new List<Automaton<BDD>>();
            for (int i = 0; i < dfaPairList.Count; i++) {
                dfaList.Add(dfaPairList[i].Second.Determinize(solver).Minimize(solver));
            }
            var dfaCorrect = boolOp.executeOperationOnAutomataList(dfaList, solver);

            var dfaAttemptPair = DFAUtilities.parseDFAFromXML(dfaAttemptDesc, solver);

            var level = FeedbackLevel.Hint;
            var maxG = int.Parse(maxGrade.Value);

            //Output
            var feedbackGrade = DFAGrading.GetGrade(dfaCorrect, dfaAttemptPair.Second, dfaPairList[0].First, solver, 1500, maxG, level);


            //Pretty print feedback
            var feedString = "<ul>";
            foreach (var feed in feedbackGrade.Second)
                feedString += string.Format("<li>{0}</li>", feed);
            feedString += "</ul>";*/

            return XElement.Parse(string.Format("<div><grade>{0}</grade><feedString>{1}</feedString></div>", feedbackGrade.Item1, feedString));
        }

        //-------- Minimization --------//

        [WebMethod]
        public XElement ComputeFeedbackMinimization(XElement dfaDesc, XElement minimizationTableAttempt, XElement dfaAttemptDesc, XElement maxGrade, XElement feedbackLevel, XElement enableFeedbacks)
        {
            var D = AutomataUtilities.ParseDFAFromXML(dfaDesc);
            var tableCorrect = D.GetMinimizationTable();
            var tableAttempt = AutomataUtilities.ParseMinimizationTableShortestWordsFromXML(minimizationTableAttempt);

            var feedbackGrade = AutomataFeedback.FeedbackForMinimizationTable(tableCorrect, tableAttempt, D);

            /*
            CharSetSolver solver = new CharSetSolver(BitWidth.BV64);

            //Read input
            var dfaPair = DFAUtilities.parseDFAFromXML(dfaDesc, solver);
            var dfaCorrect = dfaPair.Second.Minimize(solver);

            var dfaAttemptPair = DFAUtilities.parseDFAFromXML(dfaAttemptDesc, solver);
            var dfaAttempt = dfaAttemptPair.Second;

            var level = FeedbackLevel.Hint;
            var maxG = int.Parse(maxGrade.Value);
            
            //Output
            //TODO: ...
            var feedbackGrade = DFAGrading.GetGrade(dfaCorrect, dfaAttemptPair.Second, dfaPair.First, solver, 1500, maxG, level);
            */

            //Pretty print feedback
            var feedString = "<ul>";
            foreach (var feed in feedbackGrade.Item2)
                feedString += string.Format("<li>{0}</li>", feed);
            feedString += "</ul>";

            return XElement.Parse(string.Format("<div><grade>{0}</grade><feedString>{1}</feedString></div>", feedbackGrade.Item1, feedString));
        }

        //-------- DFA Construction --------//

        [WebMethod]
        public XElement ComputeFeedbackXML(XElement dfaCorrectDesc, XElement dfaAttemptDesc, XElement maxGrade, XElement feedbackLevel, XElement enabledFeedbacks)
        {
            #region Check if item is in cache
            StringBuilder key = new StringBuilder();
            key.Append("feed");
            key.Append(dfaCorrectDesc.ToString());
            key.Append(dfaAttemptDesc.ToString());
            key.Append(feedbackLevel.ToString());
            key.Append(enabledFeedbacks.ToString());
            string keystr = key.ToString();

            var cachedValue = HttpContext.Current.Cache.Get(key.ToString());
            if (cachedValue != null)
            {
                HttpContext.Current.Cache.Remove(keystr);
                HttpContext.Current.Cache.Add(keystr, cachedValue, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromDays(30), System.Web.Caching.CacheItemPriority.Normal, null);
                return (XElement)cachedValue;
            } 
            #endregion
            
            CharSetSolver solver = new CharSetSolver(BitWidth.BV64);

            //Read input 
            var dfaCorrectPair = DFAUtilities.parseBlockFromXML(dfaCorrectDesc, solver);
            var dfaAttemptPair = DFAUtilities.parseBlockFromXML(dfaAttemptDesc, solver);

            var level = (FeedbackLevel) Enum.Parse(typeof(FeedbackLevel), feedbackLevel.Value, true);
            var enabList = (enabledFeedbacks.Value).Split(',').ToList<String>();
            //bool dfaedit = enabList.Contains("dfaedit"), moseledit = enabList.Contains("moseledit"), density = enabList.Contains("density");
            bool dfaedit =true, moseledit = true, density = true;

            var maxG = int.Parse(maxGrade.Value);

            //Compute feedback
            var feedbackGrade = DFAGrading.GetGrade(dfaCorrectPair.Second, dfaAttemptPair.Second, dfaCorrectPair.First, solver, 1500, maxG, level, dfaedit, density, moseledit);

            //Pretty print feedback
            var feedString = "<ul>";
            foreach (var feed in feedbackGrade.Second)
            {
                feedString += string.Format("<li>{0}</li>", feed);
                break;
            }
            feedString += "</ul>";

            //var output = string.Format("<result><grade>{0}</grade><feedString>{1}</feedString></result>", feedbackGrade.First, feedString);
            var outXML = new XElement("result",  
                                    new XElement("grade", feedbackGrade.First),
                                    new XElement("feedString", XElement.Parse(feedString)));
            //XElement outXML = XElement.Parse(output);
            //Add this element to chace and return it
            HttpContext.Current.Cache.Add(key.ToString(), outXML, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromDays(30), System.Web.Caching.CacheItemPriority.Normal, null);

            return outXML;
        }

        
        //-------- NFA Construction --------//

        [WebMethod]
        public XElement ComputeFeedbackNFAXML(XElement nfaCorrectDesc, XElement nfaAttemptDesc, XElement maxGrade, XElement feedbackLevel, XElement enabledFeedbacks, XElement userId)
        {
            #region Check if item is in cache
            StringBuilder key = new StringBuilder();
            key.Append("feed");
            key.Append(nfaCorrectDesc.ToString());
            key.Append(nfaAttemptDesc.ToString());
            key.Append(feedbackLevel.ToString());
            key.Append(enabledFeedbacks.ToString());
            string keystr = key.ToString();

            var cachedValue = HttpContext.Current.Cache.Get(key.ToString());
            if (cachedValue != null)
            {
                HttpContext.Current.Cache.Remove(keystr);
                HttpContext.Current.Cache.Add(keystr, cachedValue, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromDays(30), System.Web.Caching.CacheItemPriority.Normal, null);
                return (XElement)cachedValue;
            }
            #endregion

            CharSetSolver solver = new CharSetSolver(BitWidth.BV64);

            //Read input 
            var nfaCorrectPair = DFAUtilities.parseBlockFromXML(nfaCorrectDesc, solver);
            var nfaAttemptPair = DFAUtilities.parseBlockFromXML(nfaAttemptDesc, solver);

            var level = (FeedbackLevel)Enum.Parse(typeof(FeedbackLevel), feedbackLevel.Value, true);
            var enabList = (enabledFeedbacks.Value).Split(',').ToList<String>();

            var maxG = int.Parse(maxGrade.Value);

            //Use this for generating 2 classes of feedback for reyjkiavik study
            var studentIdModule = int.Parse(userId.Value) % 2;
            //if (studentIdModule == 0)
            //    level = FeedbackLevel.Minimal;
            //else
            //    level = FeedbackLevel.Hint;
            //Give hints to everyone
            level = FeedbackLevel.Hint;

            //Compute feedback
            var feedbackGrade = NFAGrading.GetGrade(nfaCorrectPair.Second, nfaAttemptPair.Second, nfaCorrectPair.First, solver, 1500, maxG, level);

            //Pretty print feedback
            var feedString = "<ul>";
            foreach (var feed in feedbackGrade.Second)
                feedString += string.Format("<li>{0}</li>", feed);
            feedString += "</ul>";

            var output = string.Format("<div><grade>{0}</grade><feedString>{1}</feedString></div>", feedbackGrade.First, feedString);

            XElement outXML = XElement.Parse(output);
            //Add this element to chace and return it
            HttpContext.Current.Cache.Add(key.ToString(), outXML, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromDays(30), System.Web.Caching.CacheItemPriority.Normal, null);

            return outXML;
        }

        //-------- NFA to DFA --------//

        [WebMethod]
        public XElement ComputeFeedbackNfaToDfa(XElement nfaCorrectDesc, XElement dfaAttemptDesc, XElement maxGrade)
        {
            CharSetSolver solver = new CharSetSolver(BitWidth.BV64);
            var nfaPairZ3 = DFAUtilities.parseBlockFromXML(nfaCorrectDesc, solver);
            var nfa = DFAUtilities.toNFA(nfaPairZ3.Second, nfaPairZ3.First, solver);

            var attemptDfa = AutomataUtilities.ParseDFAFromXML(dfaAttemptDesc);

            var feedbackGrade = AutomataFeedback.FeedbackForPowersetConstruction<char>(nfa, attemptDfa);
            var feedString = "<ul>";
            foreach (var feed in feedbackGrade.Item2)
            {
                feedString += string.Format("<li>{0}</li>", feed);
            }
            feedString += "</ul>";

            return XElement.Parse(string.Format("<div><grade>{0}</grade><feedString>{1}</feedString></div>", feedbackGrade.Item1, feedString));
        }

        [WebMethod]
        public XElement ComputeFeedbackNfaToDfaOld(XElement nfaCorrectDesc, XElement dfaAttemptDesc, XElement maxGrade)
        {
            #region Check if item is in cache
            StringBuilder key = new StringBuilder();
            key.Append("feedNFADFA");
            key.Append(nfaCorrectDesc.ToString());
            key.Append(dfaAttemptDesc.ToString());
            string keystr = key.ToString();

            var cachedValue = HttpContext.Current.Cache.Get(key.ToString());
            if (cachedValue != null)
            {
                HttpContext.Current.Cache.Remove(keystr);
                HttpContext.Current.Cache.Add(keystr, cachedValue, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromDays(30), System.Web.Caching.CacheItemPriority.Normal, null);
                return (XElement)cachedValue;
            }
            #endregion

            CharSetSolver solver = new CharSetSolver(BitWidth.BV64);

            //Read input 
            var nfaCorrectPair = DFAUtilities.parseNFAFromXML(nfaCorrectDesc, solver);
            var dfaCorrect = nfaCorrectPair.Second.RemoveEpsilons(solver.MkOr).Determinize(solver).Minimize(solver);

            var dfaAttemptPair = DFAUtilities.parseDFAFromXML(dfaAttemptDesc, solver);

            var level = FeedbackLevel.Hint;

            var maxG = int.Parse(maxGrade.Value);            

            //Compute feedback
            var feedbackGrade = DFAGrading.GetGrade(dfaCorrect, dfaAttemptPair.Second, nfaCorrectPair.First, solver, 1500, maxG, level);

            //Pretty print feedback
            var feedString = "<ul>";
            foreach (var feed in feedbackGrade.Second)
                feedString += string.Format("<li>{0}</li>", feed);
            feedString += "</ul>";

            var output = string.Format("<div><grade>{0}</grade><feedString>{1}</feedString></div>", feedbackGrade.First, feedString);

            XElement outXML = XElement.Parse(output);
            //Add this element to chace and return it
            HttpContext.Current.Cache.Add(key.ToString(), outXML, null, System.Web.Caching.Cache.NoAbsoluteExpiration, TimeSpan.FromDays(30), System.Web.Caching.CacheItemPriority.Normal, null);

            return outXML;
        }

        //---------------------------
        // RegEx Methods
        //---------------------------
        [WebMethod]
        public XElement ComputeFeedbackDynamicRegexpEditDistance(XElement regexCorrectDesc, XElement equivRegex, XElement regexAttemptDesc, XElement alphabet, XElement maxGrade)
        {
            string correct = XElement.Parse(DFAUtilities.RemoveAllNamespaces(regexCorrectDesc.ToString())).Value.Trim();
            string[] equivalent = XElement.Parse(DFAUtilities.RemoveAllNamespaces(equivRegex.ToString())).Value.Trim().Split(new char[] {' ', '\n'});
            string attempt = XElement.Parse(DFAUtilities.RemoveAllNamespaces(regexAttemptDesc.ToString())).Value.Trim();


            //Checking if attempt is equivalent
            var feedString = "<ul>";
            var maxG = int.Parse(maxGrade.Value);
            try
            {
                CharSetSolver solver = new CharSetSolver(BitWidth.BV64);
                var dfaCorrectPair = DFAUtilities.parseRegexFromXML(regexCorrectDesc, alphabet, solver);
                var dfaAttemptPair = DFAUtilities.parseRegexFromXML(regexAttemptDesc, alphabet, solver);
                

                var feedbackGrade = DFAGrading.GetGrade(dfaCorrectPair.Second, dfaAttemptPair.Second, dfaCorrectPair.First, solver, 1500, maxG, FeedbackLevel.Minimal, false, false, true);
                
                foreach (var feed in feedbackGrade.Second)
                    feedString += string.Format("<li>{0}</li>", feed);
                feedString += "</ul>";

                if (feedbackGrade.First == maxG)
                    return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>Correct!</feedback></div>", feedbackGrade.First));
            }
            catch (PDLException pdlex)
            {
                return XElement.Parse(string.Format("<div>Error: {0} </div>", pdlex.Message));
            }
            // Don't forget to transform from encoded to convetional such that distance still makes sense
            var minimumDistance = LevenshteinDistance.Compute(attempt.toConventional(), correct.toConventional());
            for (var i = 0; i < equivalent.Length; i++)
                minimumDistance = Math.Min(minimumDistance, LevenshteinDistance.Compute(attempt.toConventional(), equivalent[i].toConventional()));

            return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>{1}</feedback></div>", (int)Math.Ceiling(Math.Max(maxG*(1 - 0.2*minimumDistance), 0.0)), feedString));
        }

        [WebMethod]
        public XElement CheckEquivalentRegexp(XElement correctRegex, XElement equivRegex, XElement alphabet)
        {
            try
            {
                CharSetSolver solver = new CharSetSolver(BitWidth.BV64);
                var dfaCorrectPair = DFAUtilities.parseRegexFromXML(correctRegex, alphabet, solver);
                var dfaAttemptPair = DFAUtilities.parseRegexFromXML(equivRegex, alphabet, solver);

                var same = dfaCorrectPair.Second.IsEquivalentWith(dfaAttemptPair.Second, solver);
                if (same)
                    return XElement.Parse(string.Format("<div>Equivalent</div>"));
                //Note we can also get the regex directly in scala
                else {
                    XElement Regex = XElement.Parse(DFAUtilities.RemoveAllNamespaces(equivRegex.ToString()));
                    string equiv = Regex.Value.Trim();
                    return XElement.Parse(string.Format("<div>{0} is not equivalent to the initial regular expression</div>", equiv));
                }
            }
            catch (PDLException pdlex)
            {
                return XElement.Parse(string.Format("<div>Error: {0} </div>", pdlex.Message));
            }
        }

        //---------------------------
        // Equivalence class (Regex)
        //---------------------------

        [WebMethod]
        public XElement ComputeEquivalentWordsFeedback(XElement regex, XElement alphabet, XElement representative, XElement wordsIn, XElement maxGrade)
        {
            return EquivalencyClasses.getWordsFeedback(regex, alphabet, representative, wordsIn, maxGrade);
        }
        [WebMethod]
        public XElement ComputeEquivalentShortestFeedback(XElement regex, XElement alphabet, XElement representative, XElement shortest, XElement maxGrade)
        {
            return EquivalencyClasses.getShortestFeedback(regex, alphabet, representative, shortest, maxGrade);
        }
        [WebMethod]
        public XElement ComputeSameEquivalencyClassFeedback(XElement regex, XElement alphabet, XElement firstWord, XElement secondWord, XElement notEquivalent, XElement reason, XElement maxGrade)
        {
            return EquivalencyClasses.getSameFeedback(regex, alphabet, firstWord, secondWord, notEquivalent, reason, maxGrade);  
        }
        [WebMethod]
        public XElement ComputeEquivalencyClassTwoWordsFeedback(XElement regex, XElement alphabet, XElement firstWord, XElement secondWord)
        {
            return EquivalencyClasses.getTwoWordsInstructorFeedback(regex, alphabet, firstWord, secondWord);
        }

        //---------------------------
        // Words in Regex
        //---------------------------

        [WebMethod]
        public XElement ComputeWordsInRegexpFeedback(XElement regEx, XElement wordsIn, XElement wordsOut, XElement maxGrade)
        {
            //read inputs
            int maxG = int.Parse(maxGrade.Value);
            List<String> wordsInList = new List<String>(), wordsOutList = new List<String>();
            foreach (var wordElement in wordsIn.Elements())
            {
                string w = wordElement.Value;
                if (w.Length > 75) w = w.Substring(0, 75); //limit word length
                w = w.decodeEpsilon();
                wordsInList.Add(w);
            }
            foreach (var wordElement in wordsOut.Elements())
            {
                string w = wordElement.Value;
                if (w.Length > 75) w = w.Substring(0, 75);  //limit word length
                w = w.decodeEpsilon();
                wordsOutList.Add(w);
            }

            var numberOfWords = wordsInList.Count + wordsOutList.Count;
            var correct = 0;
            var feedString = "<ul>";

            XElement regexXML = XElement.Parse(DFAUtilities.RemoveAllNamespaces(regEx.ToString()));
            string rexpr = regexXML.Value.Trim().toDotNet();
            var escapedRexpr = string.Format(@"^({0})$", rexpr);
            HashSet<string> hash = new HashSet<string>();
            HashSet<string> multi = new HashSet<string>();
            foreach (var word in wordsInList) {
                //Mind exceptions
                // w is used only for display, word is used in all the calculations and checks
                var w = word.emptyToEpsilon();
                if (Regex.Match(word, escapedRexpr, RegexOptions.None).Success && !hash.Contains(word) && !wordsOutList.Contains(word))
                    correct++;
                else if (wordsOutList.Contains(word) && !multi.Contains(word))
                {
                    feedString += String.Format("<li>'{0}' was used as both in and out of language</li>", w);
                    multi.Add(word);
                }
                else if (!hash.Contains(word))
                    feedString += String.Format("<li>'{0}' does not match</li>", w);
                else if (!multi.Contains(word))
                {
                    feedString += String.Format("<li>'{0}' was used multiple times</li>", w);
                    multi.Add(word);
                }
                
                hash.Add(word);
            }
            foreach (var word in wordsOutList) {
                var w = word.emptyToEpsilon();
                if (!Regex.Match(word, escapedRexpr, RegexOptions.None).Success && !hash.Contains(word) && !wordsInList.Contains(word))
                    correct++;
                else if (!hash.Contains(word))
                    feedString += String.Format("<li>'{0}' matches</li>", w);
                else if (!wordsInList.Contains(word) && !multi.Contains(word))
                {
                    feedString += String.Format("<li>'{0}' was used multiple times</li>", w);
                    multi.Add(word);
                }

                hash.Add(word);
            }
            feedString += "</ul>";
            var grade = Math.Round(1.0 * correct / numberOfWords * maxG);

            return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>{1}</feedback></div>", (int)grade, feedString));
        }

        //---------------------------
        // RegEx to epsilon-NFA
        //---------------------------
        [WebMethod]
        public XElement ComputeFeedbackRegexToNfa(XElement regex, XElement alphabet, XElement attemptNfa, XElement maxGrade)
        {
            try
            {
                return RegexpToNfaGrading.getFeedback(regex, alphabet, attemptNfa, maxGrade);
            }
            catch (PDLException pdlex)
            {
                return XElement.Parse(string.Format("<div><grade>0</grade><feedback>Parsing Error: {0}</feedback></div>", pdlex.Message));
            }
        }

        //---------------------------
        // Pumping lemma methods
        //---------------------------

        [WebMethod]
        public XElement ComputeFeedbackRegexp(XElement regexCorrectDesc, XElement regexAttemptDesc, XElement alphabet, XElement feedbackLevel, XElement enabledFeedbacks, XElement maxGrade)
        {
            CharSetSolver solver = new CharSetSolver(BitWidth.BV64);

            try
            {
                var dfaCorrectPair = DFAUtilities.parseRegexFromXML(regexCorrectDesc, alphabet, solver);
                var dfaAttemptPair = DFAUtilities.parseRegexFromXML(regexAttemptDesc, alphabet, solver);

                var level = (FeedbackLevel)Enum.Parse(typeof(FeedbackLevel), feedbackLevel.Value, true);

                var enabList = (enabledFeedbacks.Value).Split(',').ToList<String>();
                bool dfaedit = false, moseledit = false, density = true;
                int maxG = int.Parse(maxGrade.Value);

                var feedbackGrade = DFAGrading.GetGrade(dfaCorrectPair.Second, dfaAttemptPair.Second, dfaCorrectPair.First, solver, 1500, maxG, level, dfaedit, moseledit, density);

                var feedString = "<ul>";
                foreach (var feed in feedbackGrade.Second)
                    feedString += string.Format("<li>{0}</li>", feed);
                feedString += "</ul>";


                return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>{1}</feedback></div>", feedbackGrade.First, feedString));
            }
            catch (PDLException pdlex)
            {
                return XElement.Parse(string.Format("<div>Error: {0} </div>", pdlex.Message));
            }
        }

        [WebMethod]
        public XElement CheckRegexp(XElement regexDesc, XElement alphabet)
        {
            CharSetSolver solver = new CharSetSolver(BitWidth.BV64);

            try
            {
                var dfaCorrectPair = DFAUtilities.parseRegexFromXML(regexDesc, alphabet, solver);
                return XElement.Parse(string.Format("<div>CorrectRegex</div>"));
            }
            catch (PDLException pdlex)
            {
                XElement Regex = XElement.Parse(DFAUtilities.RemoveAllNamespaces(regexDesc.ToString()));
                string regex = Regex.Value.Trim();
                return XElement.Parse(string.Format("<div>Error for '{0}': {1} </div>", regex, pdlex.Message));
            }
        }

        //---------------------------
        // Grammar methods
        //---------------------------
        private static Func<char, char> terminalCreation = delegate (char x)
        {
            return x;
        };

        [WebMethod]
        public XElement CheckGrammar(XElement grammar)
        {
            try
            {
                var parsed = AutomataPDL.CFG.GrammarParser<char>.Parse(terminalCreation, grammar.Value);

                return XElement.Parse(string.Format("<parsing>CorrectGrammar</parsing>"));
            }
            catch (AutomataPDL.CFG.ParseException ex)
            {
                return XElement.Parse(string.Format("<div>Parsing Error: {0}</div>", ex.Message));
            }
        }

        [WebMethod]
        public XElement isCNF(XElement grammar)
        {
            try
            {
                var parsed = AutomataPDL.CFG.GrammarParser<char>.Parse(terminalCreation, grammar.Value);

                var feedString = "<ul>";
                bool allCNF = true;
                List<String> feedback = new List<String>();
                foreach (Production p in parsed.GetProductions())
                {
                    if (!p.IsCNF)
                    {
                        allCNF = false;
                        feedString += string.Format("<li>The production \"{0}\" is not in CNF...</li>", p);
                    }
                }
                feedString += "</ul>";

                if (allCNF) return XElement.Parse("<div><res>y</res><feedback></feedback></div>");

                return XElement.Parse(string.Format("<div><res>n</res><feedback>{0}</feedback></div>", feedString));
            }
            catch (AutomataPDL.CFG.ParseException ex)
            {
                return XElement.Parse(string.Format("<div><res>n</res><feedback>Parsing Error: {0}</feedback></div>", ex.Message));
            }
        }

        [WebMethod]
        public XElement ComputeWordsInGrammarFeedback(XElement grammar, XElement wordsIn, XElement wordsOut, XElement maxGrade)
        {
            //read inputs
            ContextFreeGrammar g;
            try
            {
                g = AutomataPDL.CFG.GrammarParser<char>.Parse(terminalCreation, grammar.Value);
            }
            catch (AutomataPDL.CFG.ParseException ex)
            {
                return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>Paring Error: {1}</feedback></div>", -1, ex.Message));
            }
            int maxG = int.Parse(maxGrade.Value);
            List<String> wordsInList = new List<String>(), wordsOutList = new List<String>();
            foreach (var wordElement in wordsIn.Elements())
            {
                string w = wordElement.Value;
                if (w.Length > 75) w = w.Substring(0, 75); //limit word length
                wordsInList.Add(w);
            }
            foreach (var wordElement in wordsOut.Elements())
            {
                string w = wordElement.Value;
                if (w.Length > 75) w = w.Substring(0, 75);  //limit word length
                wordsOutList.Add(w);
            }

            //grade
            var result = GrammarGrading.gradeWordsInGrammar(g, wordsInList, wordsOutList, maxG);

            //build return value
            var feedString = "<ul>";
            foreach (var feed in result.Item2)
                feedString += string.Format("<li>{0}</li>", feed);
            feedString += "</ul>";
            int grade = result.Item1;

            return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>{1}</feedback></div>", grade, feedString));
        }

        [WebMethod]
        public XElement ComputeGrammarEqualityFeedback(XElement solution, XElement attempt, XElement maxGrade, XElement checkEmptyWord)
        {
            var feedString = "<ul>";
            //read inputs
            ContextFreeGrammar sol, att;
            try
            {
                sol = AutomataPDL.CFG.GrammarParser<char>.Parse(terminalCreation, solution.Value);
                att = AutomataPDL.CFG.GrammarParser<char>.Parse(terminalCreation, attempt.Value);
            }
            catch (AutomataPDL.CFG.ParseException ex)
            {
                return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>Parsing Error: {1}</feedback></div>", -1, ex.Message));
            }
            int maxG = int.Parse(maxGrade.Value);
            bool checkEW = bool.Parse(checkEmptyWord.Value);

            //get warnings for useless variables
            foreach (var warning in GrammarUtilities.getGrammarWarnings(att))
                feedString += string.Format("<li>{0}</li>", warning);

            //ignore empty string?
            if (!checkEW)
            {
                att.setAcceptanceForEmptyString(sol.acceptsEmptyString());
            }

            //grade
            var result = GrammarGrading.gradeGrammarEquality(sol, att, maxG, 1000);

            //build return value
            foreach (var feed in result.Item2)
                feedString += string.Format("<li>{0}</li>", feed);

            feedString += "</ul>";
            int grade = result.Item1;

            return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>{1}</feedback></div>", grade, feedString));
        }

        [WebMethod]
        public XElement ComputeCYKFeedback(XElement grammar, XElement word, XElement attempt, XElement maxGrade)
        {
            //read inputs
            ContextFreeGrammar g;
            try
            {
                g = AutomataPDL.CFG.GrammarParser<char>.Parse(terminalCreation, grammar.Value);
            }
            catch (AutomataPDL.CFG.ParseException ex)
            {
                return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>{1}</feedback></div>", -1, ex.Message));
            }
            int maxG = int.Parse(maxGrade.Value);
            String w = word.Value;
            int n = w.Length;

            //parse cyk_table
            HashSet<Nonterminal>[][] cyk_table = new HashSet<Nonterminal>[n][];
            for (int i = 0; i < n; i++)
            {
                cyk_table[i] = new HashSet<Nonterminal>[n - i];
                for (int j = 0; j < n - i; j++) cyk_table[i][j] = new HashSet<Nonterminal>();
            }
            foreach (XElement cell in attempt.Elements())
            {
                //get start and end cell; cell is for substring (start-1)...(end-1)
                int start = int.Parse(cell.Attribute("start").Value);
                int end = int.Parse(cell.Attribute("end").Value);

                var set = cyk_table[end - start][start - 1];
                foreach (String nt in cell.Value.Split())
                {
                    if (nt.Length == 0) continue;
                    set.Add(new Nonterminal(nt));
                }
            }

            //grade
            var result = GrammarGrading.gradeCYK(g, w, cyk_table, maxG, 0);

            //build return value
            var feedString = "<ul>";
            foreach (var feed in result.Item2)
                feedString += string.Format("<li>{0}</li>", feed);
            feedString += "</ul>";
            int grade = result.Item1;

            return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>{1}</feedback></div>", grade, feedString));
        }

        [WebMethod]
        public XElement ComputeFindDerivationFeedback(XElement grammar, XElement word, XElement derivation, XElement maxGrade, XElement derivationType)
        {
            //read inputs
            ContextFreeGrammar g;
            try
            {
                g = AutomataPDL.CFG.GrammarParser<char>.Parse(terminalCreation, grammar.Value);
            }
            catch (AutomataPDL.CFG.ParseException ex)
            {
                return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>Grammer Parsing Error: {1}</feedback></div>", -1, ex.Message));
            }
            List<GrammarSymbol[]> d;
            try
            {
                d = AutomataPDL.CFG.DerivationParser<char>.Parse(terminalCreation, derivation.Value);
            }
            catch (AutomataPDL.CFG.ParseException ex)
            {
                return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>Derivation Parsing Error: {1}</feedback></div>", -1, ex.Message));
            }
            int maxG = int.Parse(maxGrade.Value);
            int t = Derivation.DERIVATION_ALL;
            if (int.Parse(derivationType.Value) == Derivation.DERIVATION_LEFTMOST) t = Derivation.DERIVATION_LEFTMOST;
            if (int.Parse(derivationType.Value) == Derivation.DERIVATION_RIGHTMOST) t = Derivation.DERIVATION_RIGHTMOST;
            string w = word.Value;

            //grade
            var result = GrammarGrading.gradeFindDerivation(g, w, d, maxG, t);

            //build return value
            var feedString = "<ul>";
            foreach (var feed in result.Item2)
                feedString += string.Format("<li>{0}</li>", feed);
            feedString += "</ul>";
            int grade = result.Item1;

            return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback>{1}</feedback></div>", grade, feedString));
        }

        //-------- Turing Machines (from while-programs) --------//

        [WebMethod]
        public XElement ComputeFeedbackWhileToTM(XElement correctProgram, XElement attemptTM, XElement maxGrade)
        {
            TMCB<int, string> attemptTm = TMXmlParser.ParseTMFromXml(attemptTM);

            //workaround for parsing correctProgram
            //TODO: find a clean solution
            string content = correctProgram.Value;
            content = content.Replace("&lt;", "<");
            content = content.Replace("&gt;", ">");
            content = AutomataPDL.WhileProgram.WhileUtilities.RemoveNamespacesFromString(content);
            correctProgram = XElement.Parse(content);
            
            AutomataPDL.WhileProgram.WExpr program = AutomataPDL.WhileProgram.WhileUtilities.ParseWhileProgramFromXML(correctProgram);
            int maxG = int.Parse(maxGrade.Value);

            TMCB<int, int> correctTm = program.ToTMCB(-1);

            var feedbackGrade = AutomataFeedback.FeedbackForWhileToTM(program, attemptTm, maxG);
            var feedString = "<ul>";
            foreach (var feed in feedbackGrade.Item2)
            {
                feedString += string.Format("<li>{0}</li>", feed);
            }
            feedString += "</ul>";
            
            //returns grade, feedback strings, and a sample input for the tape simulator
            return XElement.Parse(string.Format("<div><grade>{0}</grade><feedback><feedString>{1}</feedString><sampleInput>{2}</sampleInput></feedback></div>", feedbackGrade.Item1, feedString, feedbackGrade.Item3));
        }

        [WebMethod]
        public XElement CheckWhileFormat(XElement program)
        {
            string content = program.Value;
            content = content.Replace("&lt;", "<");
            content = content.Replace("&gt;", ">");
            content = AutomataPDL.WhileProgram.WhileUtilities.RemoveNamespacesFromString(content);
            program = XElement.Parse(content);

            AutomataPDL.WhileProgram.WExpr whileProgram = AutomataPDL.WhileProgram.WhileUtilities.ParseWhileProgramFromXML(program);

            return whileProgram.ProgramToXML();
        }

        //---------------------------
        // Problem Generation Methodes
        //---------------------------

        private readonly int TRIES = 100;
        private readonly Generator[] GENERATORS = new Generator[] {
            WordsInGrammarGenerator.GetInstance(),
            CYKGenerator.GetInstance(),
            GrammarToCNFGenerator.GetInstance(),
            WhileToTMGenerator.GetInstance(),
            FindDerivationGenerator.GetInstance()
        };
        private Generator GetMatchingGenerator(String typeS)
        {
            foreach (Generator gen in GENERATORS)
            {
                if (typeS.Equals(gen.TypeName())) return gen;
            }
            return null;
        }

        [WebMethod]
        public XElement GenerateProblem(XElement type, XElement minQual)
        {
            String typeS = type.Value;
            var gen = GetMatchingGenerator(typeS);
            double minQualD = double.Parse(minQual.Value);
            if (minQualD < 0) minQualD = 0.0;
            if (minQualD > 1) minQualD = 1;

            if (gen == null) return new XElement("error", "problem type not supported '" + typeS + "'");

            return Generation.generateWithMinQuality(gen, TRIES, minQualD).Export();
        }

        [WebMethod]
        public XElement GenerateProblemHardest(XElement type, XElement minQual)
        {
            String typeS = type.Value;
            var gen = GetMatchingGenerator(typeS);
            double minQualD = double.Parse(minQual.Value);
            if (minQualD < 0) minQualD = 0.0;
            if (minQualD > 1) minQualD = 1;

            if (gen == null) return new XElement("error", "problem type not supported '" + typeS + "'");

            return Generation.generateHardestWithMinQuality(gen,TRIES,minQualD).Export();
        }

        [WebMethod]
        public XElement GenerateProblemBestIn(XElement type, XElement minDiff, XElement maxDiff)
        {
            String typeS = type.Value;
            var gen = GetMatchingGenerator(typeS);
            int minDiffI = int.Parse(minDiff.Value);
            if (minDiffI < 0) minDiffI = 0;
            if (minDiffI > 100) minDiffI = 100;
            int maxDiffI = int.Parse(maxDiff.Value);
            if (maxDiffI < minDiffI) maxDiffI = minDiffI;
            if (maxDiffI > 100) maxDiffI = 100;

            if (gen == null) return new XElement("error", "problem type not supported '" + typeS + "'");

            return Generation.generateBestWithDifficultyBounds(gen, TRIES, minDiffI, maxDiffI).Export();
        }

        //---------------------------
        // Pumping lemma methods
        //---------------------------

        // Checks whether an arithmetic language description parses correctly
        [WebMethod]
        public XElement CheckArithLanguageDescription(
            XElement languageDesc,
            XElement constraintDesc, 
            XElement alphabet,
            XElement pumpingString)
        {

            // Please change this to return only if the pumping string
            // is a solution to the pumping problem
            try
            {
                // This is super shady 
                var symbols = alphabet.Descendants().Where(e => e.Name.LocalName == "symbol");
                List<string> alphabetList = symbols.Select(x => x.Value).ToList();
                var language = PumpingLemma.ArithmeticLanguage.FromTextDescriptions(alphabetList, languageDesc.Value, constraintDesc.Value);

                var pumpingSymString = PumpingLemma.SymbolicString.FromTextDescription(alphabetList, pumpingString.Value);
                if (pumpingSymString.GetIntegerVariables().Count > 1)
                    throw new PumpingLemma.PumpingLemmaException("Only one variable allowed in the pumping string!");

                return XElement.Parse(string.Format("<div>CorrectLanguageDescription</div>"));
                // if (PumpingLemma.ProofChecker.check(language, pumpingSymString))
                    // return XElement.Parse(string.Format("<div>CorrectLanguageDescription</div>"));
                // else
                    // throw new PumpingLemma.PumpingLemmaException("Unable to prove non-regularity of language using the pumping string!");
            }
            catch (PumpingLemma.PumpingLemmaException ex)
            {
                return XElement.Parse(string.Format("<div>Error: {0} </div>", ex.Message));
            }
            catch (Exception ex)
            {
                return XElement.Parse(string.Format("<div>Internal Error: {0} </div>", ex.ToString()));
            }
        }

        // Checks whether an arithmetic language description parses correctly
        [WebMethod]
        public XElement GenerateStringSplits(
            XElement languageDesc,
            XElement constraintDesc,
            XElement alphabet,
            XElement pumpingString)
        {
            try
            {
                var symbols = alphabet.Descendants().Where(e => e.Name.LocalName == "symbol");
                List<string> alphabetList = symbols.Select(x => x.Value).ToList();

                var language = PumpingLemma.ArithmeticLanguage.FromTextDescriptions(alphabetList, languageDesc.Value, constraintDesc.Value);

                var pumpingSymString = PumpingLemma.SymbolicString.FromTextDescription(alphabetList, pumpingString.Value);
                if (pumpingSymString.GetIntegerVariables().Count > 1)
                    throw new PumpingLemma.PumpingLemmaException("Only one variable allowed in the pumping string!");

                var pumpingLength = pumpingSymString.GetIntegerVariables().First();
                var pumpingLengthVariable = PumpingLemma.LinearIntegerExpression
                    .SingleTerm(1, pumpingLength);
                var additionalConstraint = PumpingLemma.ComparisonExpression.GreaterThanOrEqual(
                    pumpingLengthVariable,
                    PumpingLemma.LinearIntegerExpression.Constant(0)
                    );
                return pumpingSymString.SplitDisplayXML(pumpingLength, additionalConstraint);
            }
            catch (PumpingLemma.PumpingLemmaException e)
            {
                return XElement.Parse(string.Format("<error>Error: {0} </error>", e.Message));
            }
            catch (PDLException pdlex)
            {
                return XElement.Parse(string.Format("<error>Error: {0} </error>", pdlex.Message));
            }
                /*
            catch (Exception e)
            {
                return XElement.Parse(string.Format("<error>Internal Error: {0} </error>", e.Message));
            }
                 */
        }

        // Checks whether an arithmetic language description parses correctly
        [WebMethod]
        public XElement GetPumpingLemmaFeedback(
            XElement languageDesc,
            XElement constraintDesc,
            XElement alphabet,
            XElement pumpingString,
            XElement pumpingNumbers)
        {
            throw new NotImplementedException();

            //pumping numbers come as <pumps><pump>5</pump><pump>-1</pump><pump>p</pump></pumps>
            try
            {
                // Parse the language
                // Parse the constraintDesc
                // Make sure all the variables are bound

                // Verify using pumping string
                // Return whether the string is correct

                return XElement.Parse(string.Format("<result><grade>{0}</grade><feedback>{1}</feedback></result>", "10", "Try again"));
            }
            catch (PDLException pdlex)
            {
                return XElement.Parse(string.Format("<error>Error: {0} </error>", "you can only pump by a function of p"));
            }
        }

        /// <summary>
        /// computes the grade and feedback for the problem type "Words of PDA"; for the correpsonding format look at the invokes method
        /// </summary>
        /// <param name="xmlPda">PDA defined by the instructor</param>
        /// <param name="xmlWordsInLanguage">words entered by the student, that are in the language accepted by the PDA</param>
        /// <param name="xmlWordsNotInLanguage">words entered by the student, that are not in the language accepted by the PDA</param>
        /// <param name="xmlMaxGrade">the maximum grade that can be achieved in this problem</param>
        /// <returns></returns>
        [WebMethod]
        public XElement ComputeFeedbackPDAWordProblem(XElement xmlPda, XElement xmlWordsInLanguage, XElement xmlWordsNotInLanguage, XElement xmlMaxGrade)
        {
            return WordProblemGrader.GradeWordProblemAsync(xmlPda, xmlWordsInLanguage, xmlWordsNotInLanguage, xmlMaxGrade);
        }

        /// <summary>
        /// computes the grade and feedback for the problem type "English to PDA"; for the correpsonding format look at the invokes method
        /// </summary>
        /// <param name="xmlPdaCorrect">PDA defined by the instructor</param>
        /// <param name="xmlPdaAttempt">PDA build by the student</param>
        /// <param name="xmlGiveStackAlphabet">if the stack alphabet was predefined by the instructor</param>
        /// <param name="xmlMaxGrade">the maximum grade that can be achieved in this problem</param>
        /// <returns></returns>
        [WebMethod]
        public XElement ComputeFeedbackPDAConstruction(XElement xmlPdaCorrect, XElement xmlPdaAttempt, XElement xmlGiveStackAlphabet, XElement xmlMaxGrade)
        {
            return ConstructionProblemGrader.GradeConstructionProblem(xmlPdaCorrect, xmlPdaAttempt, xmlGiveStackAlphabet, xmlMaxGrade);
        }

        /// <summary>
        /// computes a path for the given word in the PDA; a path is a sequence of consecutive PDA-configurations, so that the first 
        /// configuration contains the given word and the last one is an accepting configuration
        /// </summary>
        /// <param name="xmlPda">PDA</param>
        /// <param name="xmlWord">word</param>
        /// <returns></returns>
        [WebMethod]
        public XElement SimulateWordInPDA(XElement xmlPda, XElement xmlWord)
        {
            return SimulationAdapter.RunSimulationAsync(xmlPda, xmlWord);
        }

        //---------------------------
        // Pumping Lemma Game
        //---------------------------


        [WebMethod]
        public XElement PLGRegularGetDFAFromSymbolicString(XElement alphabet, XElement symbolicString, XElement constraints)
        {
            ArithmeticLanguage lang;
            try
            {
                lang = ArithmeticLanguage.FromTextDescriptions(alphabet.Value, symbolicString.Value, constraints.Value);
            }
            catch (Exception e)
            {
                return PLGerror("Exception: " + e.Message);
            }

            StringDFA strDfa;
            try
            {
                strDfa = new StringDFA(lang.ToDFA());
            }
            catch (PumpingLemmaException e)
            {
                return PLGerror("Couldn't build DFA: " + e.Message);
            }

            return strDfa.ToXML(lang.alphabet.ToArray());
        }

        [WebMethod]
        public XElement PLGNonRegularCheckValidity(XElement alphabet, XElement symbolicString, XElement constraints, XElement unpumpableWord)
        {
            try
            {
                List<string> alphList = alphabet.Value.Split(' ').ToList();
                SymbolicString str = SymbolicString.FromTextDescription(alphList, unpumpableWord.Value);
                HashSet<VariableType> vars = str.GetIntegerVariables();
                vars.ExceptWith(new VariableType[] { VariableType.Variable("n") });
                if (vars.Any())
                {
                    throw new PumpingLemmaException("contains illegal integer variables");
                }
            }
            catch(Exception e)
            {
                return PLGerror("Could not parse unpumpable Word: " + e.Message);
            }

            try
            {
                ArithmeticLanguage lang = loadArithmeticLanguage(alphabet.Value, symbolicString.Value, constraints.Value);
                return XElement.Parse("<true></true>");
            }
            catch (Exception e)
            {
                return PLGerror("Could not parse language: " + e.Message);
            }
        }

        [WebMethod]
        public XElement PLGNfaToDfa(XElement automaton)
        {
            try
            {
                var strDFA = new StringDFA(automaton, false);
                return strDFA.ToXML(strDFA.alphabet.ToArray());
            }
            catch (Exception e)
            {
                return PLGerror("Couldn't build DFA: " + e.Message);
            }
        }

        //returns an n if the language is regular
        [WebMethod]
        public XElement PLGRegularGetN(XElement automaton)
        {
            try
            {
                int n = PumpingLemmaGame.RegularGetN(new StringDFA(automaton, true));
                return XElement.Parse("<n>" + n + "</n>");
            }
            catch (Exception e)
            {
                return XElement.Parse("<error>" + e.Message + "</error>");
            }
        }

        [WebMethod]
        public XElement PLGNonRegularGetN(XElement max)
        {
            int parsed = Int32.Parse(max.Value);
            int num = new Random().Next(1, parsed + 1);
            return XElement.Parse("<n>"+num+"</n>");
        }

        [WebMethod]
        public XElement PLGRegularGetWord(XElement automaton, XElement n)
        {
            int parsed = Int32.Parse(n.Value);
            StringDFA dfa = new StringDFA(automaton, true);
            string word = null;
            int tries = 0;
            while (word == null)
            {
                word = PumpingLemmaGame.RegularGetWord(dfa, parsed);
                if (tries > 5)
                {
                    throw new PumpingLemmaException("Could not find word.");
                }
            }
             
            if (word == null)
            {
                return PLGerror("no word could be found");
            }
            return XElement.Parse("<word>"+word+"</word>");
        }

        [WebMethod]
        public XElement PLGNonRegularGetWord(XElement alphabet, XElement symbolicString, XElement constraints, XElement n, XElement unpumpableWord)
        {
            ArithmeticLanguage lang = loadArithmeticLanguage(alphabet.Value, symbolicString.Value, constraints.Value);
            if (lang == null)
            {
                string msg = "Couldn't parse language.";
                return PLGerror(msg);
            }
            SymbolicString unpWord;
            try
            {
                unpWord = SymbolicString.FromTextDescription(lang.alphabet.ToList(), unpumpableWord.Value);
            }
            catch (Exception e)
            {
                return PLGerror("Couldn't parse unpumpable Word");
            }
            if (lang == null)
            {
                string msg = "Couldn't parse language.";
                return PLGerror(msg);
            }

            int n_num;
            try
            {
                n_num = Int32.Parse(n.Value);
                if (n_num < 1) return PLGerror("n must be greater 0");

            }
            catch (Exception e)
            {
                if (e is ArgumentNullException || e is FormatException || e is OverflowException)
                {
                    return XElement.Parse("<error>You must specify a number.</error>");
                }
                throw;
            }

            try
            {
                string word = PumpingLemmaGame.NonRegularGetUnpumpableWord(lang, unpWord, n_num);
                if (word != null)
                {
                    return XElement.Parse("<word>"+word+"</word>");
                }
                return PLGerror("Couldn't find word.");
            }
            catch (PumpingLemmaException e)
            {
                return XElement.Parse("<error>Malformed constraints of the language.</error>");
            }
        }

        [WebMethod]
        public XElement PLGRegularCheckI(XElement automaton, XElement start, XElement mid, XElement end, XElement i)
        {
            try
            {
                if (i.Value.Equals(""))
                    return XElement.Parse("<error>You must specify a number.</error>");
                int iParsed = Int32.Parse(i.Value);
                if (iParsed < 0)
                {
                    return XElement.Parse("<error>The number must be positive.</error>");
                }
                StringDFA stringDFA = new StringDFA(automaton, true);
                return PumpingLemmaGame.RegularCheckI(stringDFA, start.Value, mid.Value, end.Value, iParsed) ? XElement.Parse("<true></true>") : XElement.Parse("<false></false>");
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException || e is FormatException || e is OverflowException)
                {
                    return XElement.Parse("<error>You must specify a number.</error>");
                }
                throw;
            }
        }

        [WebMethod]
        public XElement PLGNonRegularCheckI(XElement alphabet, XElement symbolicString, XElement constraints, XElement start, XElement mid, XElement end, XElement i)
        {
            try
            {
                ArithmeticLanguage lang = loadArithmeticLanguage(alphabet.Value, symbolicString.Value, constraints.Value);
                if (lang == null)
                    return PLGerror("could not parse language");
                int iParsed = Int32.Parse(i.Value);
                if (iParsed < 0)
                {
                    return XElement.Parse("<error>The number must be positive.</error>");
                }
                return PumpingLemmaGame.NonRegularCheckI(lang, start.Value, mid.Value, end.Value, iParsed) ? XElement.Parse("<true></true>") : XElement.Parse("<false></false>");
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException || e is FormatException || e is OverflowException)
                {
                    return XElement.Parse("<error>You must specify a number.</error>");
                }
                throw;
            }
        }

        [WebMethod]
        public XElement PLGRegularCheckWordGetSplit(XElement automaton, XElement n, XElement word)
        {
            StringDFA strDFA = new StringDFA(automaton, true);
            string resp = PumpingLemmaGame.RegularCheckWord(strDFA, Int32.Parse(n.Value), word.Value);
            if (resp.StartsWith("<false>"))
            {
                return XElement.Parse(resp);
            }
            else
            {
                var split = PumpingLemmaGame.RegularGetSplit(strDFA, word.Value, Int32.Parse(n.Value));
                if (split != null)
                {
                    string s = "<split><start>" + split.Item1 + "</start><mid>" + split.Item2 + "</mid><end>" + split.Item3 + "</end></split>";
                    return XElement.Parse(s);
                }
                return PLGerror("Could not find loop");
            }
        }

        [WebMethod]
        public XElement PLGNonRegularCheckWordGetSplit(XElement alphabet, XElement symbolicString, XElement constraints, XElement n, XElement word)
        {
            ArithmeticLanguage lang = loadArithmeticLanguage(alphabet.Value, symbolicString.Value, constraints.Value);
            if (lang == null)
                return PLGerror("could not parse language");
            var feedback = PumpingLemmaGame.NonRegularCheckWord(lang, Int32.Parse(n.Value), word.Value);
            if (!feedback.StartsWith("<true>"))
            {
                return XElement.Parse(feedback);
            }
            var split = PumpingLemmaGame.NonRegularGetPumpableSplit(lang, word.Value, Int32.Parse(n.Value));
            if (split == null)
            {
                split = PumpingLemmaGame.NonRegularGetRandomSplit(lang, word.Value, Int32.Parse(n.Value));
            }
            return XElement.Parse("<split><start>" + split.Item1 + "</start><mid>" + split.Item2 + "</mid><end>" + split.Item3 + "</end></split>");
        }     

        [WebMethod]
        public XElement PLGRegularGetI(XElement automaton, XElement start, XElement mid, XElement end)
        {
            StringDFA dfa = new StringDFA(automaton, true);
            int i = PumpingLemmaGame.RegularGetI(dfa, start.Value, mid.Value, end.Value);
            if (i == -1)
            {
                return PLGerror("could not choose i");
            }
            if (i == 1)
                return XElement.Parse("<win>" + i + "</win>");
            return XElement.Parse("<loss>" + i + "</loss>");
        }

        [WebMethod]
        public XElement PLGNonRegularGetI(XElement alphabet, XElement symbolicString, XElement constraints, XElement start, XElement mid, XElement end)
        {
            ArithmeticLanguage lang;
            try
            {
                lang = loadArithmeticLanguage(alphabet.Value, symbolicString.Value, constraints.Value);
                int i = PumpingLemmaGame.NonRegularGetI(lang, start.Value, mid.Value, end.Value);
                if (i != -1)
                {
                    return XElement.Parse("<loss>" + i + "</loss>");
                }
                return XElement.Parse("<win>" + 1 + "</win>");
            }
            catch (PumpingLemmaException e)
            {
                string msg = "Couldn't parse language: " + e.ToString();
                return PLGerror(msg);
            }
            catch (Exception e)
            {
                string msg = "Exception: " + e.ToString();
                return PLGerror(msg);
            }
        }

        // Pumping Lemma Game helpers

        private ArithmeticLanguage loadArithmeticLanguage(string alphabet, string symbolicString, string constraints)
        {
            List<string> alphList = alphabet.Split(' ').ToList();
            SymbolicString str = SymbolicString.FromTextDescription(alphList, symbolicString);
            HashSet<VariableType> vars = str.GetIntegerVariables();
            StringBuilder additionalConstraints = new StringBuilder();
            foreach (VariableType v in vars)
            {
                additionalConstraints.Append(v.ToString() + " >= 0 &&");
            }
            additionalConstraints.Remove(additionalConstraints.Length - 3, 3);
            if (!constraints.Equals(""))
            {
                constraints += " && " + additionalConstraints.ToString();
            }
            else
            {
                constraints = additionalConstraints.ToString();
            }

            HashSet<VariableType> allowedVariables = str.GetIntegerVariables();

            BooleanExpression c = Parser.parseCondition(constraints);
            if (c == null)
                throw new PumpingLemmaException("invalid constraints");
            if (!c.isSatisfiable())
                throw new PumpingLemmaException("constraints are not satisfiable");
            if (!checkBooleanExpressionValidity(c, str.GetIntegerVariables()))
                throw new PumpingLemmaException("invalid constraints");

            ArithmeticLanguage lang = new ArithmeticLanguage(alphList, str, c);
            return lang;
        }

        private bool checkBooleanExpressionValidity(BooleanExpression c, HashSet<VariableType> allowedVariables)
        {
            switch (c.boolean_expression_type)
            {
                case BooleanExpression.OperatorType.Quantifier:
                    throw new PumpingLemmaException("do not use quantified expressions");
                case BooleanExpression.OperatorType.Logical:
                    LogicalExpression logicalExpression = (LogicalExpression)c;
                    switch (logicalExpression.logical_operator)
                    {
                        case LogicalExpression.LogicalOperator.Not:
                            throw new PumpingLemmaException("do not use negations");
                        case LogicalExpression.LogicalOperator.Or:
                            throw new PumpingLemmaException("do not use OR");
                        case LogicalExpression.LogicalOperator.And:
                            return checkBooleanExpressionValidity(logicalExpression.boolean_operand1, allowedVariables) && checkBooleanExpressionValidity(logicalExpression.boolean_operand2, allowedVariables);
                        case LogicalExpression.LogicalOperator.False:
                        case LogicalExpression.LogicalOperator.True:
                            return true;
                        default:
                            throw new ArgumentException();
                    }
                case BooleanExpression.OperatorType.Comparison:
                    ComparisonExpression comparisonExpression = (ComparisonExpression)c;
                    if (comparisonExpression.comparison_operator == ComparisonExpression.ComparisonOperator.NEQ)
                    {
                        throw new PumpingLemmaException("do not use NEQ");
                    }
                    if (!comparisonExpression.GetVariables().IsSubsetOf(allowedVariables))
                    {
                        throw new PumpingLemmaException("illegal variable in constraints");
                    }
                    return true;
                default:
                    throw new ArgumentException();
            }
        }

        private XElement PLGerror(string errorText)
        {
            return new XElement("error", errorText);
        }
    }
}
