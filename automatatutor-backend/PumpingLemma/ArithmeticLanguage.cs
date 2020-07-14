using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AutomataPDL.Automata;


namespace PumpingLemma
{
    public class ArithmeticLanguage
    {
        public IEnumerable<String> alphabet;
        public SymbolicString symbolic_string;
        public BooleanExpression constraint;

        public ArithmeticLanguage(IEnumerable<String> _alphabet, SymbolicString _symbolic_string, BooleanExpression _constraint)
        {
            this.alphabet = _alphabet;
            this.symbolic_string = _symbolic_string;
            this.constraint = _constraint;
        }

        public static ArithmeticLanguage FromTextDescriptions(List<String> alphabet, string symbolicStringText, string constraintText)
        {
            var symbolPattern = new Regex(@"^[a-zA-Z0-9]$");
            var illegalSymbols = alphabet.FindAll(s => !symbolPattern.IsMatch(s));
            if (illegalSymbols.Count > 0)
            {
                var message = string.Format(
                    "Found illegal symbols {0} in alphabet. Symbols should match [a-zA-Z0-9]", 
                    string.Join(", ", illegalSymbols)
                );
                throw new PumpingLemmaException(message);
            }

            // Parse the language
            var ss = PumpingLemma.Parser.parseSymbolicString(symbolicStringText, alphabet);
            if (ss == null)
                throw new PumpingLemmaException("Unable to parse language");

            // Parse the constraintDesc
            var constraint = PumpingLemma.Parser.parseCondition(constraintText);
            if (constraint == null)
                throw new PumpingLemmaException("Unable to parse constraint");

            // Make sure all the variables are bound
            var boundVariables = ss.GetIntegerVariables();
            var constraintVariables = constraint.GetVariables();
            // Console.WriteLine("Bound variables: " + String.Join(", ", boundVariables));
            // Console.WriteLine("Constriant variables: " + String.Join(", ", constraintVariables));
            foreach (var consVar in constraintVariables)
            {
                if (!boundVariables.Contains(consVar))
                    throw new PumpingLemmaException(
                        string.Format("Constraint variable {0} not bound", consVar));
            }

            // Add constraints saying that all variables are >= 0
            BooleanExpression defaultConstraint = LogicalExpression.True();
            foreach (var consVar in constraintVariables)
            {
                defaultConstraint = LogicalExpression.And(
                    defaultConstraint,
                    ComparisonExpression.GreaterThanOrEqual(
                        LinearIntegerExpression.SingleTerm(1, consVar),
                        LinearIntegerExpression.Constant(0)
                        )
                    );
            }

            return new ArithmeticLanguage(alphabet, ss, constraint);
        }
        public static ArithmeticLanguage FromTextDescriptions(string alphabetText, string symbolicStringText, string constraintText)
        {
            // Split alphabet and ensure that the symbols are valid 
            // and don't contain special characters
            var whiteSpacePattern = new Regex(@"^\s*$");
            List<String> alphabet = alphabetText
                .Split(new char[] { ' ' })
                .Where(s => !whiteSpacePattern.IsMatch(s))
                .ToList();
            return FromTextDescriptions(alphabet, symbolicStringText, constraintText);
        }

        public static HashSet<ComparisonExpression> getComparisonExpressions(BooleanExpression constraint)
        {
            HashSet<ComparisonExpression> set = new HashSet<ComparisonExpression>();
            if (constraint.boolean_expression_type == BooleanExpression.OperatorType.Comparison)
            {
                set.Add((ComparisonExpression)constraint);
                return set;
            }
            else if (constraint.boolean_expression_type == BooleanExpression.OperatorType.Quantifier)
            {
                throw new PumpingLemmaException("Use of Quantifiers is not allowed!");
            }
            else
            {
                LogicalExpression lexpr = (LogicalExpression)constraint;
                if (lexpr.logical_operator != LogicalExpression.LogicalOperator.And)
                {
                    throw new PumpingLemmaException("Use only the logical operator AND (&)!");
                }
                set.UnionWith(getComparisonExpressions(lexpr.boolean_operand1));
                set.UnionWith(getComparisonExpressions(lexpr.boolean_operand2));
                return set;
            }
        }


        public ArithmeticLanguage CheckUnaryConstraints()
        {
            if (!constraint.isSatisfiable())
            {
                throw new PumpingLemmaException("Constraints of Arithmentic Language are not satisfiable");
            }

            //makes all constraints unary if possible and returns the resulting language
            //throws PumpingLemmaException if not possible
            HashSet<ComparisonExpression> allExpr = getComparisonExpressions(constraint);
            HashSet<VariableType> VarsSeen = new HashSet<VariableType>();
            HashSet<string> variables = new HashSet<string>();

            //enumerate all variables
            foreach (ComparisonExpression constr in allExpr)
            {
                HashSet<VariableType> varTypeSet = new HashSet<VariableType>(constr.arithmetic_operand1.GetVariables());
                varTypeSet.UnionWith(constr.arithmetic_operand1.GetVariables());
                foreach (VariableType t in varTypeSet)
                {
                    variables.Add(t.ToString());
                }

                /*
                if (constr.arithmetic_operand1.GetVariables().Count == 1 || constr.arithmetic_operand2.GetVariables().Count > 1)
                {

                }
                */

                if (constr.arithmetic_operand1.GetVariables().Count > 1 || constr.arithmetic_operand2.GetVariables().Count > 1)
                {
                    throw new PumpingLemmaException("Cannot make constraints unary - several variables on one side of the comparison");
                }

                //if there it is an comparison with a constant, we are done
                if (!constr.isUnary())
                {
                    VariableType v1 = constr.arithmetic_operand1.GetVariables().First();
                    VariableType v2 = constr.arithmetic_operand2.GetVariables().First();
                    IEnumerable<ComparisonExpression> comparisonsContainingSecondVar = allExpr.Where(comp => comp.containsVariable(v2));

                    //try to substitute second variable
                    bool substituted = false;
                    foreach (ComparisonExpression cexp in comparisonsContainingSecondVar)
                    {
                        if (cexp.comparison_operator == ComparisonExpression.ComparisonOperator.EQ && cexp.isUnary())
                        {
                            //unfinished
                        }
                    }
                }
            }

            foreach (string v in variables)
            {

            }

            throw new NotImplementedException();
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

        public Dictionary<VariableType, UnaryComparison> getReducedUnaryConstraints()
        {
            if (!getComparisonExpressions(constraint).All(expr => expr.isUnary()))
                throw new PumpingLemmaException("The Language doesn't contain only unary constraints.");

            if (!constraint.isSatisfiable())
                throw new PumpingLemmaException("The Language constraint is not satisfiable.");

            Dictionary<VariableType, UnaryComparison> comparisonsToReturn = new Dictionary<VariableType, UnaryComparison>();

            IEnumerable<ComparisonExpression> allExpr = getComparisonExpressions(constraint);

            //delete non-constraints
            allExpr = allExpr.Except(allExpr.Where(e => e.arithmetic_operand1.isConstant() && e.arithmetic_operand2.isConstant()));

            //put variable on left side
            //divide, so that coefficient = 1
            HashSet<ComparisonExpression> ToDelete = new HashSet<ComparisonExpression>();
            var exprToAdd = new HashSet<ComparisonExpression>();
            foreach (ComparisonExpression expr in allExpr)
            {
                if (expr.arithmetic_operand1.isConstant())
                {
                    switch(expr.comparison_operator)
                    {
                        case ComparisonExpression.ComparisonOperator.GEQ:
                            exprToAdd.Add(ComparisonExpression.LessThanOrEqual(expr.arithmetic_operand2, expr.arithmetic_operand1));
                            break;
                        case ComparisonExpression.ComparisonOperator.GT:
                            exprToAdd.Add(ComparisonExpression.LessThan(expr.arithmetic_operand2, expr.arithmetic_operand1));
                            break;
                        case ComparisonExpression.ComparisonOperator.LEQ:
                            exprToAdd.Add(ComparisonExpression.GreaterThanOrEqual(expr.arithmetic_operand2, expr.arithmetic_operand1));
                            break;
                        case ComparisonExpression.ComparisonOperator.LT:
                            exprToAdd.Add(ComparisonExpression.GreaterThan(expr.arithmetic_operand2, expr.arithmetic_operand1));
                            break;
                        default:
                            break;
                    }
                }
            }

            ToDelete = new HashSet<ComparisonExpression>(allExpr.Where(e => e.arithmetic_operand1.isConstant()));
            allExpr = allExpr.Except(ToDelete);
            allExpr = allExpr.Union(exprToAdd);
            ToDelete = new HashSet<ComparisonExpression>();
            exprToAdd = new HashSet<ComparisonExpression>();

            foreach (var expr in allExpr)
            {
                foreach (VariableType variable in expr.GetVariables())
                {
                    int coefficient = expr.arithmetic_operand1.coefficients[variable];
                    int constant = expr.arithmetic_operand2.constant;
                    double divided = (double)constant / (double)coefficient;

                    switch (expr.comparison_operator)
                    {
                        case ComparisonExpression.ComparisonOperator.EQ:
                        case ComparisonExpression.ComparisonOperator.NEQ:
                            if (!isInt(divided)) ToDelete.Add(expr);
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
                }
            }
            
            allExpr = allExpr.Except(ToDelete);
            allExpr = allExpr.Union(exprToAdd);
            ToDelete = new HashSet<ComparisonExpression>();
            exprToAdd = new HashSet<ComparisonExpression>();


            //reduce if equal constraint is found
            HashSet<string> varsToDelete = new HashSet<string>();
            foreach (ComparisonExpression expr in allExpr)
            {
                if (expr.comparison_operator == ComparisonExpression.ComparisonOperator.EQ)
                {
                    VariableType variable = expr.arithmetic_operand1.GetVariables().First();
                    int coefficient = expr.arithmetic_operand1.coefficients[variable];
                    int constant = expr.arithmetic_operand2.constant;
                    int divided = divideConstant(constant, coefficient);
                    comparisonsToReturn.Add(variable, UnaryComparison.equal(variable, divided));
                    varsToDelete.Add(variable.ToString());
                }
            }
            IEnumerable<ComparisonExpression> exprToDelete = allExpr.Where(e => varsToDelete.Contains(e.arithmetic_operand1.GetVariables().First().ToString()));
            allExpr = allExpr.Except(exprToDelete);

            //GEQ -> GT, LEQ -> LT 
            exprToDelete = new HashSet<ComparisonExpression>();
            exprToAdd = new HashSet<ComparisonExpression>();
            foreach (ComparisonExpression expr in allExpr)
            {
                if (expr.comparison_operator == ComparisonExpression.ComparisonOperator.GEQ)
                {
                    ComparisonExpression newExpr = ComparisonExpression.GreaterThan(expr.arithmetic_operand1, expr.arithmetic_operand2.constant - 1);
                    exprToAdd.Add(newExpr);
                }
                if (expr.comparison_operator == ComparisonExpression.ComparisonOperator.LEQ)
                {
                    ComparisonExpression newExpr = ComparisonExpression.LessThan(expr.arithmetic_operand1, expr.arithmetic_operand2.constant + 1);
                    exprToAdd.Add(newExpr);
                }
            }
            exprToDelete = allExpr.Where(e => e.comparison_operator == ComparisonExpression.ComparisonOperator.LEQ
                                           || e.comparison_operator == ComparisonExpression.ComparisonOperator.GEQ);
            allExpr = allExpr.Except(exprToDelete);
            allExpr = allExpr.Union(exprToAdd);

            HashSet<VariableType> allVars = new HashSet<VariableType>();
            foreach (ComparisonExpression expr in allExpr)
            {
                allVars.Add(expr.arithmetic_operand1.GetVariables().First());
            }

            foreach (VariableType variable in allVars)
            {
                IEnumerable<ComparisonExpression> varExpr = allExpr.Where(e => variable.ToString().Equals(e.GetVariables().First().ToString()));

                //reduce to single constraints
                IEnumerable<ComparisonExpression> gtExpr = varExpr.Where(e => e.comparison_operator == ComparisonExpression.ComparisonOperator.GT);
                int gthan = -1;
                foreach (ComparisonExpression expr in gtExpr)
                {
                    int coefficient = expr.arithmetic_operand1.coefficients[variable];
                    int constant = expr.arithmetic_operand2.constant;
                    //floor
                    int divided = constant / coefficient;
                    if (constant > gthan) gthan = constant;
                }

                IEnumerable<ComparisonExpression> ltExpr = varExpr.Where(e => e.comparison_operator == ComparisonExpression.ComparisonOperator.LT);
                int lthan = Int32.MaxValue;
                foreach (ComparisonExpression expr in ltExpr)
                {
                    int coefficient = expr.arithmetic_operand1.coefficients[variable];
                    int constant = expr.arithmetic_operand2.constant;
                    if (constant < lthan) lthan = constant;
                }

                varExpr = varExpr.Except(gtExpr);
                varExpr = varExpr.Except(ltExpr);

                //rest should be NEQ
                HashSet<int> neqInts = new HashSet<int>();
                foreach (ComparisonExpression expr in varExpr)
                {
                    if (expr.comparison_operator != ComparisonExpression.ComparisonOperator.NEQ)
                    {
                        throw new PumpingLemmaException("The programmer is stupid.");
                    }
                    int coefficient = expr.arithmetic_operand1.coefficients[variable];
                    int constant = expr.arithmetic_operand2.constant;
                    double divided = (double)constant / coefficient;
                    //if not int, discard
                    if (Math.Abs(divided % 1) <= (Double.Epsilon * 100))
                    {
                        int divided_int = (int)divided;
                        neqInts.Add(divided_int);
                    }
                }

                if (gthan > -1 && lthan < Int32.MaxValue)
                {
                    comparisonsToReturn.Add(variable, UnaryComparison.between(variable, gthan, lthan, neqInts));
                }
                else if (gthan > -1 && lthan == Int32.MaxValue)
                {
                    comparisonsToReturn.Add(variable, UnaryComparison.greater(variable, gthan, neqInts));
                }
                else if (gthan == -1 && lthan < Int32.MaxValue)
                {
                    comparisonsToReturn.Add(variable, UnaryComparison.between(variable, -1, lthan, neqInts));
                }
                else
                {
                    comparisonsToReturn.Add(variable, UnaryComparison.greater(variable, -1, neqInts));
                }
            }

            return comparisonsToReturn;
        }

        private static int currentId;

        private static HashSet<VariableType> varsSeen;
        private bool checkVariableOnce(SymbolicString symbString)
        {
            switch(symbString.expression_type)
            {
                case SymbolicString.SymbolicStringType.Symbol:
                    return true;
                case SymbolicString.SymbolicStringType.Concat:
                    foreach (SymbolicString s in symbString.sub_strings)
                    {
                        if (!checkVariableOnce(s)) return false;
                    }
                    return true;
                case SymbolicString.SymbolicStringType.Repeat:
                    HashSet<VariableType> vars = symbString.repeat.GetVariables();
                    if (varsSeen.Intersect(vars).Any()) return false;
                    varsSeen.UnionWith(vars);
                    return checkVariableOnce(symbString.root);
                default:
                    throw new ArgumentException();
            }
        }

        private static State<string> createState()
        {
            var state = new State<string>(currentId, "" + currentId);
            currentId++;
            return state;
        }

        private static Tuple<NFA<string, string>, HashSet<TwoTuple<State<string>, State<string>>>> concatAutomata(LinkedList<Tuple<NFA<string, string>, HashSet<TwoTuple<State<string>, State<string>>>>> listOfNFAs)
        {
            var states = new HashSet<State<string>>();
            var delta = new Dictionary<TwoTuple<State<string>, string>, HashSet<State<string>>>();
            var Q_0 = new HashSet<State<string>>();
            var F = new HashSet<State<string>>();
            var epsilonTransitions = new HashSet<TwoTuple<State<string>, State<string>>>();
            var alphabet = new HashSet<string>();

            for (int i = 0; i < listOfNFAs.Count; i++)
            {
                var nfaTuple = listOfNFAs.ElementAt(i);
                states.UnionWith(nfaTuple.Item1.Q);
                alphabet.UnionWith(nfaTuple.Item1.Sigma);
                //dictionary union
                foreach (var keyValuePair in nfaTuple.Item1.delta)
                {
                    HashSet<State<string>> val = new HashSet<State<string>>();
                    if (delta.TryGetValue(keyValuePair.Key, out val))
                    {
                        val.UnionWith(keyValuePair.Value);
                    }
                    else
                    {
                        delta[keyValuePair.Key] = keyValuePair.Value;
                    }
                }
                epsilonTransitions.UnionWith(nfaTuple.Item2);

                if (i == 0)
                {
                    Q_0 = listOfNFAs.ElementAt(i).Item1.Q_0;
                }
                else
                {
                    foreach (var state in F)
                    {
                        epsilonTransitions.Add(new TwoTuple<State<string>, State<string>>(state, listOfNFAs.ElementAt(i).Item1.Q_0.First()));
                    }
                }

                F = listOfNFAs.ElementAt(i).Item1.F;
            }
            var nfa = new NFA<string, string>(states, alphabet, delta, Q_0, F);
            return Tuple.Create(nfa, epsilonTransitions);
        }

        private static Tuple<NFA<string, string>, HashSet<TwoTuple<State<string>, State<string>>>> epsNFA(SymbolicString symbolicString, HashSet<string> alphabet, Dictionary<VariableType, UnaryComparison> comparisons)
        {
            var states = new HashSet<State<string>>();
            var delta = new Dictionary<TwoTuple<State<string>, string>, HashSet<State<string>>>();
            var Q_0 = new HashSet<State<string>>();
            var F = new HashSet<State<string>>();
            var epsilonTransitions = new HashSet<TwoTuple<State<string>, State<string>>>();
            var nfa = new NFA<string, string>(states, alphabet, delta, Q_0, F);

            switch (symbolicString.expression_type)
            {
                case SymbolicString.SymbolicStringType.Symbol:
                    var symbol = symbolicString.atomic_symbol;                    
                    var q_0 = createState();
                    Q_0.Add(q_0);
                    states.Add(q_0);
                    var q_1 = createState();
                    states.Add(q_1);
                    F.Add(q_1);
                    var twoTuple = new TwoTuple<State<string>, string>(q_0, symbol);
                    var to = new HashSet<State<string>>();
                    to.Add(q_1);
                    delta.Add(twoTuple, to);
                    nfa = new NFA<string, string>(states, alphabet, delta, Q_0, F);
                    return Tuple.Create(nfa, epsilonTransitions);
                case SymbolicString.SymbolicStringType.Concat:
                    var listOfNFAs = new LinkedList<Tuple<NFA<string, string>, HashSet<TwoTuple<State<string>, State<string>>>>>();
                    foreach(SymbolicString str in symbolicString.sub_strings)
                    {
                        listOfNFAs.AddLast(epsNFA(str, alphabet, comparisons));
                    }
                    return concatAutomata(listOfNFAs);

                case SymbolicString.SymbolicStringType.Repeat:
                    LinearIntegerExpression lie = symbolicString.repeat;
                    var lisNFAs = new LinkedList<Tuple<NFA<string, string>, HashSet<TwoTuple<State<string>, State<string>>>>>();
                    for (int i = 0; i < lie.constant; i++)
                    {
                        lisNFAs.AddLast(epsNFA(symbolicString.root, alphabet, comparisons));
                    }
                    foreach(var coeff in lie.coefficients)
                    {
                        var compar = comparisons[coeff.Key];
                        switch(compar.comparisonType)
                        {
                            case UnaryComparison.ComparisonType.EQUAL:
                                for (int i = 0; i < coeff.Value * compar.constant; i++)
                                {
                                    lisNFAs.AddLast(epsNFA(symbolicString.root, alphabet, comparisons));
                                }
                                break;
                            case UnaryComparison.ComparisonType.GREATER:
                                for (int i = 0; i < coeff.Value * (compar.min+1) - 1; i++)
                                {
                                    lisNFAs.AddLast(epsNFA(symbolicString.root, alphabet, comparisons));
                                }
                                var autom = epsNFA(symbolicString.root, alphabet, comparisons);
                                var autom_start = autom.Item1.Q_0.First();
                                var newEpsTrans = new HashSet<TwoTuple<State<string>, State<string>>>();
                                newEpsTrans.UnionWith(autom.Item2);
                                foreach(var acceptingState in autom.Item1.F)
                                {
                                    newEpsTrans.Add(new TwoTuple<State<string>, State<string>>(acceptingState, autom_start));
                                }
                                lisNFAs.AddLast(Tuple.Create(autom.Item1, newEpsTrans));
                                break;
                            case UnaryComparison.ComparisonType.BETWEEN:
                                for (int i = 0; i < coeff.Value * compar.min; i++)
                                {
                                    lisNFAs.AddLast(epsNFA(symbolicString.root, alphabet, comparisons));
                                }
                                var rememberedStates = new HashSet<State<string>>();

                                int upperbound = 0;
                                if (compar.min == -1 || compar.min == 0)
                                {
                                    upperbound = compar.max - 1;
                                }
                                else
                                {
                                    upperbound = compar.max - compar.min - 1;
                                }
                                
                                for (int i = 0; i < coeff.Value * upperbound; i++)
                                {
                                    var aut = epsNFA(symbolicString.root, alphabet, comparisons);
                                    foreach(var q_f in aut.Item1.F)
                                    {
                                        rememberedStates.Add(q_f);
                                    }
                                    lisNFAs.AddLast(aut);
                                }
                                if (compar.min == -1)
                                {
                                    rememberedStates.Add(lisNFAs.First.Value.Item1.Q_0.First());
                                }
                                
                                var epsTrans = new HashSet<TwoTuple<State<string>, State<string>>>();
                                var lastAut = lisNFAs.Last.Value;
                                foreach(var state in rememberedStates)
                                {
                                    foreach(var q_f in lastAut.Item1.F)
                                    {
                                        epsTrans.Add(new TwoTuple<State<string>, State<string>>(state, q_f));
                                    }
                                }
                                lisNFAs.RemoveLast();
                                lisNFAs.AddLast(Tuple.Create(lastAut.Item1, new HashSet<TwoTuple<State<string>, State<string>>>(epsTrans.Union(lastAut.Item2))));
                                break;
                        }
                       
                    }
                    
                    return concatAutomata(lisNFAs);

                default:
                    throw new ArgumentException();

            }
        }

        public DFA<string, HashSet<State<Set<State<string>>>>> ToDFA()
        {
            //check if every variable appears only once
            varsSeen = new HashSet<VariableType>();
            if (!checkVariableOnce(symbolic_string))
            {
                throw new PumpingLemmaException("Each variable must only appear once!");
            }

            NFA<string, string> nfa = null;
            var epsilonTransitions = new HashSet<TwoTuple<State<string>, State<string>>>();

            ArithmeticLanguage unaryLanguage = this;
            var comparisons = unaryLanguage.getReducedUnaryConstraints();

            currentId = 0;

            //only epsilon
            if (symbolic_string.isEpsilon())
            {
                var startState = new State<string>(0, "0");
                var finalState = new State<string>(1, "1");
                var states = new HashSet<State<string>>();
                states.Add(startState);
                states.Add(finalState);
                var Q_0 = new HashSet<State<string>>();
                Q_0.Add(startState);
                var F = new HashSet<State<string>>();
                F.Add(startState);
                var delta = new Dictionary<TwoTuple<State<string>, string>, HashSet<State<string>>>();
                foreach(var letter in alphabet)
                {
                    delta.Add(new TwoTuple<State<string>, string>(startState, letter), new HashSet<State<string>>(new State<string>[] { finalState }));
                }
                nfa = new NFA<string, string>(states, new HashSet<string>(alphabet), delta, Q_0, F);
            }
            else
            {
                var tuple = epsNFA(symbolic_string, new HashSet<string>(alphabet), comparisons);
                nfa = tuple.Item1;
                epsilonTransitions = tuple.Item2;
            }
            
            NFA<string, string> nfaCollected = StringDFA.collectEpsilonTransitions(nfa.Q, new HashSet<string>(alphabet), nfa.delta, nfa.Q_0, nfa.F, epsilonTransitions);

            var dfa = nfaCollected.NFAtoDFA();
            var dfa_min = dfa.MinimizeHopcroft();

            return dfa_min;
        }

        
    }

    public class PumpingLemmaException : Exception
    {
        public PumpingLemmaException() : base() { }
        public PumpingLemmaException(string message) : base(message) { }
        public PumpingLemmaException(string message, System.Exception inner) : base(message, inner) { }
    }
}
