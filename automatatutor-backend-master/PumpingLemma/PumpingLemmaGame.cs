using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutomataPDL.Automata;

namespace PumpingLemma
{

    public class PumpingLemmaGame
    {

        //constant used for regular word choice;
        private static int lowestNRest = -20;

        /*
         * ----------------------------------------------------------------------------------------------------
         *                                       Helper Methods
         * ----------------------------------------------------------------------------------------------------
         */


        private static Random rand = null;
        private static int randInt(int minValue, int maxValue)
        {
            if (rand == null)
                rand = new Random();
            return rand.Next(minValue, maxValue);
        }

        private static string wordError()
        {
            return "<error>No word possible.</error>";
        }

        private static int divideConstant(int constant, int coefficient)
        {
            double divided = (double)constant / coefficient;
            //check wether integer
            if (Math.Abs(divided % 1) > (Double.Epsilon * 100))
                throw new PumpingLemmaException("Not satisfiable for integers");
            return Convert.ToInt32(divided);
        }

        private static bool isInt(double d)
        {
            return Math.Abs(d % 1) <= (Double.Epsilon * 100);
        }

        private static UnaryComparison compExprToUnaryComp(ComparisonExpression expr)
        {
            if (!expr.isUnary()) return null;

            if (expr.arithmetic_operand1.isConstant())
            {
                LinearIntegerExpression tmp = expr.arithmetic_operand1;
                expr.arithmetic_operand1 = expr.arithmetic_operand2;
                expr.arithmetic_operand2 = tmp;
                switch (expr.comparison_operator)
                {
                    case ComparisonExpression.ComparisonOperator.GEQ:
                        expr.comparison_operator = ComparisonExpression.ComparisonOperator.LEQ;
                        break;
                    case ComparisonExpression.ComparisonOperator.GT:
                        expr.comparison_operator = ComparisonExpression.ComparisonOperator.LT;
                        break;
                    case ComparisonExpression.ComparisonOperator.LEQ:
                        expr.comparison_operator = ComparisonExpression.ComparisonOperator.GEQ;
                        break;
                    case ComparisonExpression.ComparisonOperator.LT:
                        expr.comparison_operator = ComparisonExpression.ComparisonOperator.GT;
                        break;
                    default:
                        break;
                }
            }

            VariableType variable = expr.GetVariables().First();
            int coefficient = expr.arithmetic_operand1.coefficients[variable];
            int constant = expr.arithmetic_operand2.constant;
            double divided = (double)constant / (double)coefficient;

            switch (expr.comparison_operator)
            {
                case ComparisonExpression.ComparisonOperator.EQ:
                case ComparisonExpression.ComparisonOperator.NEQ:
                    if (!isInt(divided)) return null;
                    else
                    {
                        expr.arithmetic_operand2 = LinearIntegerExpression.Constant(Convert.ToInt32(divided));
                    }
                    break;
                case ComparisonExpression.ComparisonOperator.GEQ:
                    expr.arithmetic_operand2 = LinearIntegerExpression.Constant(Convert.ToInt32(Math.Ceiling(divided)));
                    break;
                case ComparisonExpression.ComparisonOperator.GT:
                    expr.arithmetic_operand2 = LinearIntegerExpression.Constant(Convert.ToInt32(Math.Floor(divided)));
                    break;
                case ComparisonExpression.ComparisonOperator.LEQ:
                    expr.arithmetic_operand2 = LinearIntegerExpression.Constant(Convert.ToInt32(Math.Floor(divided)));
                    break;
                case ComparisonExpression.ComparisonOperator.LT:
                    expr.arithmetic_operand2 = LinearIntegerExpression.Constant(Convert.ToInt32(Math.Ceiling(divided)));
                    break;
                default:
                    throw new ArgumentException();
            }

            if (expr.comparison_operator == ComparisonExpression.ComparisonOperator.GEQ)
            {
                expr.comparison_operator = ComparisonExpression.ComparisonOperator.GT;
                expr.arithmetic_operand2.constant--;
            }
            if (expr.comparison_operator == ComparisonExpression.ComparisonOperator.LEQ)
            {
                expr.comparison_operator = ComparisonExpression.ComparisonOperator.LT;
                expr.arithmetic_operand2.constant++;
            }

            switch (expr.comparison_operator)
            {
                case ComparisonExpression.ComparisonOperator.EQ:
                    return UnaryComparison.equal(variable, expr.arithmetic_operand2.constant);
                case ComparisonExpression.ComparisonOperator.GT:
                    return UnaryComparison.greater(variable, expr.arithmetic_operand2.constant, new HashSet<int>());
                case ComparisonExpression.ComparisonOperator.LT:
                    return UnaryComparison.between(variable, -1, expr.arithmetic_operand2.constant, new HashSet<int>());
                default:
                    return null;
            }
        }

        private static int wordLength(SymbolicString symbStr, Dictionary<VariableType, int> assignment)
        {
            int sum;
            switch (symbStr.expression_type)
            {
                case SymbolicString.SymbolicStringType.Symbol:
                    return 1;
                case SymbolicString.SymbolicStringType.Concat:
                    sum = 0;
                    foreach (SymbolicString s in symbStr.sub_strings)
                        sum += wordLength(s, assignment);
                    return sum;
                case SymbolicString.SymbolicStringType.Repeat:
                    if (symbStr.repeat.isConstant())
                        return wordLength(symbStr.root, assignment) * symbStr.repeat.constant;
                    sum = 0;
                    foreach (var v in symbStr.repeat.coefficients)
                    {
                        sum += v.Value * assignment[v.Key] * wordLength(symbStr.root, assignment);
                    }
                    return sum;
                default:
                    throw new ArgumentException();
            }
        }

        private static string pumpedWord(SymbolicString symbStr, Dictionary<VariableType, int> assignment)
        {
            StringBuilder sb = new StringBuilder();
            switch (symbStr.expression_type)
            {
                case SymbolicString.SymbolicStringType.Symbol:
                    return symbStr.atomic_symbol;
                case SymbolicString.SymbolicStringType.Concat:
                    foreach (SymbolicString s in symbStr.sub_strings)
                        sb.Append(pumpedWord(s, assignment));
                    return sb.ToString();
                case SymbolicString.SymbolicStringType.Repeat:
                    for (int i = 0; i < symbStr.repeat.constant; i++)
                    {
                        sb.Append(pumpedWord(symbStr.root, assignment));
                    }
                    foreach (var v in symbStr.repeat.coefficients)
                    {
                        for (int i = 0; i < v.Value * assignment[v.Key]; i++)
                            sb.Append(pumpedWord(symbStr.root, assignment));
                    }
                    return sb.ToString();
                default:
                    throw new ArgumentException();
            }
        }

        // returns start and end index (both inclusive) of mid word
        private static TwoTuple<int, int> detectLoopFromState(StringDFA dfa, string currentState, string word, int n)
        {
            List<string> statesVisited = new List<string>();

            if (!dfa.Accepts(word))
            {
                throw new PumpingLemmaException("Automaton does not accept this word!");
            }

            statesVisited.Add(currentState);
            int currentLetter = 0;

            while (currentLetter <= n && currentLetter < word.Length)
            {
                string newState = dfa.delta[new TwoTuple<string, string>(currentState, word.ElementAt(currentLetter) + "")];
                if (statesVisited.Contains(newState))
                {
                    int index = statesVisited.IndexOf(newState);
                    return new TwoTuple<int, int>(index, currentLetter);
                }
                else
                {
                    currentLetter++;
                    currentState = newState;
                    statesVisited.Add(currentState);
                }

            }
            return null;
        }

        private static IEnumerable<Tuple<string, string, string>> possibleSplits(string word, int n)
        {

            for (int splitPos2 = 1; splitPos2 <= n; splitPos2++)
            {
                for (int splitPos1 = 0; splitPos1 < splitPos2; splitPos1++)
                {
                    yield return splitTuple(word, splitPos1, splitPos2);
                }
            }
        }

        private static string pumpMid(string start, string mid, string end, int i)
        {
            StringBuilder fullWord = new StringBuilder(start);
            for (int j = 0; j < i; j++)
            {
                fullWord.Append(mid);
            }
            fullWord.Append(end);
            return fullWord.ToString();
        }

        private static Tuple<string, string, string> splitTuple(string word, int splitPos1, int splitPos2)
        {
            string word1 = word.Substring(0, splitPos1);
            string word2 = word.Substring(splitPos1, splitPos2 - splitPos1);
            string word3 = word.Substring(splitPos2);

            return new Tuple<string, string, string>(word1, word2, word3);
        }

        private static SymbolicString splitToSymbStr(List<string> alphabet, string start, string mid, string end)
        {
            VariableType freshVar = VariableType.FreshVariable();
            List<SymbolicString> strings = new List<SymbolicString>();
            if (!start.Equals(""))
                strings.Add(SymbolicString.FromTextDescription(alphabet, start));
            SymbolicString pumpedMid = SymbolicString.Repeat(SymbolicString.FromTextDescription(alphabet, mid), LinearIntegerExpression.Variable(freshVar.ToString()));
            strings.Add(pumpedMid);
            if (!end.Equals(""))
                strings.Add(SymbolicString.FromTextDescription(alphabet, end));

            SymbolicString matchingString = SymbolicString.Concat(strings);
            return matchingString;
        }

        private static string shortestAcceptingWord(StringDFA dfa, string state)
        {
            Dictionary<string, int> stateDistance = new Dictionary<string, int>();
            Dictionary<string, string> statePredecessor = new Dictionary<string, string>();
            HashSet<string> allStates = new HashSet<string>();
            foreach (string s in dfa.states)
            {
                stateDistance.Add(s, Int32.MaxValue);
                statePredecessor.Add(s, null);
                allStates.Add(s);
            }
            stateDistance[state] = 0;
            while(allStates.Any())
            {
                HashSet<KeyValuePair<string, int>> minStates = new HashSet<KeyValuePair<string, int>>();
                var ordered = stateDistance.OrderBy(x => x.Value);
                int i = 1;
                minStates.Add(ordered.First());
                int minVal = ordered.First().Value;
                while(i < ordered.Count() && minVal == ordered.ElementAt(i).Value)
                {
                    minStates.Add(ordered.ElementAt(i));
                    i++;
                }
                i = randInt(0, minStates.Count());
                var nextState = minStates.ToArray()[i].Key;
                allStates.Remove(nextState);

                HashSet<string> neighbours = new HashSet<string>();
                foreach(var letter in dfa.alphabet)
                {
                    string value = null;
                    if (dfa.delta.TryGetValue(new TwoTuple<string, string>(nextState, letter), out value))
                    {
                        neighbours.Add(value);
                    }
                }
                foreach(string neighbour in neighbours)
                {
                    if (allStates.Contains(neighbour))
                    {
                        int alt = stateDistance[nextState] + 1;
                        if (alt < stateDistance[neighbour])
                        {
                            stateDistance[neighbour] = alt;
                            statePredecessor[neighbour] = nextState;
                        }
                    }
                }
            }

            HashSet<string> shortestWords = new HashSet<string>();
            foreach(var acceptingState in dfa.F)
            {
                StringBuilder sb = new StringBuilder();
                string lastState = acceptingState;
                while(statePredecessor[lastState] != null)
                {
                    string letter = dfa.delta.Where(x =>
                        x.Key.first == statePredecessor[lastState]
                        && x.Value == lastState).First().Key.second;
                    sb.Insert(0, letter);
                    lastState = statePredecessor[lastState];
                }
                shortestWords.Add(sb.ToString());
            }

            HashSet<string> minWords = new HashSet<string>();
            var orderedWords = shortestWords.OrderBy(x => x.Length);
            int j = 1;
            minWords.Add(orderedWords.First());
            int minL = orderedWords.First().Length;
            while (j < orderedWords.Count() && minL == orderedWords.ElementAt(j).Length)
            {
                minWords.Add(orderedWords.ElementAt(j));
                j++;
            }
            j = randInt(0, minWords.Count());
            return minWords.ToArray()[j];
        }

        /*
         * ----------------------------------------------------------------------------------------------------
         *                                       Decision Making
         * ----------------------------------------------------------------------------------------------------
         */

        public static string RegularCheckWord(StringDFA dfa, int n, string word)
        {
            if (word.Length < n) return "<false>Word is too short.</false>";

            foreach (char c in word)
            {
                if (!dfa.alphabet.Contains(c + ""))
                    return "<false>Word contains illegal letters.</false>";
            }

            if (dfa.Accepts(word)) return "<true></true>";

            return "<false>Word is not in the language.</false>";

        }

        public static int RegularGetN(StringDFA dfa)
        {
            var newDfa = dfa.ToDFA();
            return newDfa.Q.Count;
        }

        public static int RegularGetI(StringDFA dfa, string start, string mid, string end)
        {
            if (!dfa.Accepts(start + end)) return 0;
            if (RegularCheckSplit(dfa, start, mid, end))
            {
                return 1; //AI admits defeat
            }
            int i = 2;
            while (dfa.Accepts(pumpMid(start, mid, end, i)))
            {
                i++;
                //for debuugging purposes:
                if (i > 99)
                {
                    return 1;
                }
            }
            return i;
        }

        public static string RegularGetWord(StringDFA dfa, int n)
        {
            if (dfa.states.Count <= n)
            {
                //choose random word
                return RegularGetRandomWord(dfa, dfa.q_0, n);
            }

            //choose word that doesn't contain loop
            string word = RegularGetUnpumpableWord(dfa, dfa.q_0, new HashSet<string>(new string[] { dfa.q_0 }), n);
            if (word == null)
                return RegularGetRandomWord(dfa, dfa.q_0, n);
            return word;
        }

        private static bool RegularCheckSplit(StringDFA dfa, string start, string mid, string end)
        {
            string fullWord = pumpMid(start, mid, end, 1);
            HashSet<Tuple<string, string, string>> equivalentSplits = new HashSet<Tuple<string, string, string>>();
            equivalentSplits.Add(new Tuple<string, string, string>(start, mid, end));


            for (int k = 1; k <= mid.Length; k++)
            {
                string newEnd = String.Copy(end);
                string newStart = String.Copy(start);

                while (newEnd.Length >= k)
                {
                    string pre = mid.Length > k ? mid.Substring(k) : "";
                    if (pre + newEnd.Substring(0, k) == mid)
                    {
                        newEnd = newEnd.Substring(k);
                        newStart = newStart + mid.Substring(0, k);
                        equivalentSplits.Add(new Tuple<string, string, string>(newStart, mid, newEnd));
                    }
                    else break;
                }

                newEnd = String.Copy(end);
                newStart = String.Copy(start);
                while (newStart.Length >= k)
                {
                    string post = mid.Length > k ? mid.Substring(0, k) : "";
                    if (newStart.Substring(newStart.Length - k) + post == mid)
                    {
                        newEnd = mid.Substring(mid.Length - k) + newEnd;
                        newStart = newStart.Substring(0, newStart.Length - k);
                        equivalentSplits.Add(new Tuple<string, string, string>(newStart, mid, newEnd));
                    }
                    else break;
                }
            }

            foreach (var split in equivalentSplits)
            {
                //check whether mid is a loop
                string stateAfterStart = dfa.getStateFromState(split.Item1, dfa.q_0);
                if (stateAfterStart.Equals(dfa.getStateFromState(split.Item2, stateAfterStart)))
                {
                    return true;
                }
            }
            return false;
        }

        private static string RegularGetUnpumpableWord(StringDFA dfa, string currentState, HashSet<string> statesUsed, int nRest)
        {
            if (currentState.Equals("-1"))
                return null;
            if (dfa.F.Contains(currentState) && nRest <= 0)
            {
                return "";
            }

            if (dfa.states.Count == statesUsed.Count)
                return null;

            string[] alphArr = new string[dfa.alphabet.Count];
            dfa.alphabet.CopyTo(alphArr, 0);
            HashSet<string> letters = new HashSet<string>(alphArr);

            while (letters.Any())
            {
                int i = randInt(0, letters.Count);
                string letter = letters.ToArray()[i];
                letters.Remove(letter);

                string state = dfa.getStateFromState(letter, currentState);
                if (nRest > 0)
                {
                    if (!statesUsed.Contains(state))
                    {
                        bool contin = true;
                        foreach (string l in alphArr)
                        {
                            if (statesUsed.Contains(dfa.getStateFromState(l, state)))
                            {
                                contin = false;
                                break;
                            }
                        }
                        if (contin)
                        {
                            HashSet<string> newStatesUsed = new HashSet<string>(statesUsed);
                            newStatesUsed.Add(state);
                            string returnedWord = RegularGetUnpumpableWord(dfa, state, newStatesUsed, nRest - 1);
                            if (returnedWord != null)
                                return letter + returnedWord;
                        }

                    }
                }
                else
                {
                    string returnedWord = RegularGetUnpumpableWord(dfa, state, statesUsed, nRest - 1);
                    if (returnedWord != null)
                        return letter + returnedWord;
                }
            }
            return null;
        }

        //chooses a random word with at least n
        private static string RegularGetRandomWord(StringDFA dfa, string currentState, int nRest)
        {
            if (currentState.Equals("-1"))
                return null;

            if (dfa.F.Contains(currentState) && nRest <= 0)
            {
                return "";
            }

            if (nRest < lowestNRest)
            {
                return shortestAcceptingWord(dfa, currentState);
            }

            string[] alphArr = new string[dfa.alphabet.Count];
            dfa.alphabet.CopyTo(alphArr, 0);
            HashSet<string> letters = new HashSet<string>(alphArr);

            while (letters.Any())
            {
                int i = randInt(0, letters.Count);
                string letter = letters.ToArray()[i];
                letters.Remove(letter);
                string state = dfa.getStateFromState(letter, currentState);
                string returnedWord = RegularGetRandomWord(dfa, state, nRest - 1);
                if (returnedWord != null)
                    return letter + returnedWord;
            }
            return null;
        }

        public static bool RegularCheckI(StringDFA dfa, string start, string mid, string end, int i)
        {
            return dfa.Accepts(pumpMid(start, mid, end, i));
        }

        public static Tuple<string, string, string> RegularGetSplit(StringDFA dfa, string word, int n)
        {
            TwoTuple<int, int> midIndex = detectLoopFromState(dfa, dfa.q_0, word, n);
            if (midIndex != null)
            {
                string start = word.Substring(0, midIndex.first);
                string mid = word.Substring(midIndex.first, midIndex.second - midIndex.first + 1);
                string end = word.Substring(midIndex.second + 1);
                return Tuple.Create(start, mid, end);
            }
            return null;
        }

        public static string NonRegularGetRandomWord(ArithmeticLanguage language, int n)
        {
            HashSet<Dictionary<VariableType, int>> usedAssigments = new HashSet<Dictionary<VariableType, int>>();
            HashSet<VariableType> vars = language.constraint.GetVariables();

            for (int i = 0; i < Math.Pow(n, vars.Count); i++)
            {
                Dictionary<VariableType, int> newAssigment = new Dictionary<VariableType, int>();
                foreach (VariableType v in vars)
                {
                    newAssigment.Add(v, randInt(0, n));
                }
                if (usedAssigments.Contains(newAssigment))
                {
                    i--;
                }
                else
                {
                    usedAssigments.Add(newAssigment);
                    //check sat
                    HashSet<BooleanExpression> ops = new HashSet<BooleanExpression>();
                    ops.Add(language.constraint);
                    foreach (var entry in newAssigment)
                    {
                        ops.Add(ComparisonExpression.Equal(LinearIntegerExpression.Variable(entry.Key.ToString()), entry.Value));
                    }
                    BooleanExpression expr = LogicalExpression.And(ops);
                    if (expr.isSatisfiable())
                    {
                        return pumpedWord(language.symbolic_string, newAssigment);
                    }
                }
            }
            return wordError();
        }

        public static int NonRegularGetI(ArithmeticLanguage language, string start, string mid, string end)
        {
            SymbolicString matchingString = splitToSymbStr(language.alphabet.ToList(), start, mid, end);
            if (ProofChecker.checkContainment(matchingString, language, LogicalExpression.True()))
            {
                return 1; // AI surrenders
            }
            else
            {
                int i = 0;
                do
                {
                    string word = pumpMid(start, mid, end, i);
                    SymbolicString ss = SymbolicString.FromTextDescription(language.alphabet.ToList(), word);
                    if (!ProofChecker.checkContainment(ss, language, LogicalExpression.True()))
                    {
                        return i;
                    }
                    i++;
                } while (i < 99); //for debugging purposes
                return -1;
            }
        }

        public static string NonRegularGetUnpumpableWord(ArithmeticLanguage language, SymbolicString unpumpableWord, int n)
        {
            Dictionary<VariableType, int> assignment = new Dictionary<VariableType, int>();
            assignment.Add(VariableType.Variable("n"), n);
            string word = pumpedWord(unpumpableWord, assignment);
            SymbolicString ss = SymbolicString.FromTextDescription(language.alphabet.ToList(), word);
            while (!ProofChecker.checkContainment(ss, language, LogicalExpression.True()))
            {
                assignment[VariableType.Variable("n")]++;
                word = pumpedWord(unpumpableWord, assignment);
                ss = SymbolicString.FromTextDescription(language.alphabet.ToList(), word);
            }
            return word;
        }

        public static string NonRegularCheckWord(ArithmeticLanguage lang, int n, string word)
        {
            if (word.Length < n) return "<false>Word is too short.</false>";

            foreach (char c in word)
            {
                if (!lang.alphabet.Contains(c + ""))
                    return "<false>Word contains illegal letters.</false>";
            }

            if (lang.symbolic_string.isFlat())
            {
                SymbolicString wordSS = Parser.parseSymbolicString(word, lang.alphabet.ToList());
                if (ProofChecker.checkContainment(wordSS, lang, LogicalExpression.True()))
                {
                    return "<true></true>";
                }
                else
                {
                    return "<false>Word is not in the language.</false>";
                }
            }
            else
            {
                throw new PumpingLemmaException("Arithmetic Language must be flat!");
            }
        }

        public static bool NonRegularCheckI(ArithmeticLanguage language, string start, string mid, string end, int i)
        {
            string fullWord = pumpMid(start, mid, end, i);
            if (language.symbolic_string.isFlat())
            {
                SymbolicString wordSS = Parser.parseSymbolicString(fullWord.ToString(), language.alphabet.ToList());
                return ProofChecker.checkContainment(wordSS, language, LogicalExpression.True());
            }
            else
            {
                throw new PumpingLemmaException("Arithmetic Language must be flat!");
            }
        }

        public static Tuple<string, string, string> NonRegularGetPumpableSplit(ArithmeticLanguage language, string word, int n)
        {
            foreach (var split in possibleSplits(word, n))
            {
                SymbolicString str = splitToSymbStr(language.alphabet.ToList(), split.Item1, split.Item2, split.Item3);
                if (ProofChecker.checkContainment(str, language, LogicalExpression.True()))
                {
                    return split;
                }
            }
            return null;
        }

        public static Tuple<string, string, string> NonRegularGetRandomSplit(ArithmeticLanguage language, string word, int n)
        {
            int wordLength = word.Length;
            int splitPos1 = randInt(0, n-1); 
            // [0; pumpingVarValue-1]
            // splitPos1 + (splitPos2-splitPos1) <= pumpingVarValue
            // splitPos2 <= pumpingVarValue - 2 * splitPos1
            int splitPos2 = randInt(splitPos1 + 1, n);
            return splitTuple(word, splitPos1, splitPos2);
        }

    }
}
