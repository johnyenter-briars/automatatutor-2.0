using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.Automata
{
    public enum Directions { Left, Non, Right }

    public class TMCB<C, S> : AbstractAutomaton<C, S>
    {
        private bool timeout = false;

        public State<S> q0;
        public HashSet<C> Gamma;
        public C blank;
        public Dictionary<State<S>, List<Transition<C, S>>> Delta;
        public Dictionary<State<S>, Transition<C, S>> DefaultTransitions;
        //returns true iff the last run on this TM timed out (did not accept in maximumNumSteps steps)
        public bool Timeout
        {
            get { return timeout; }
        }

        private const int tapeExpansionLength = 10;
        private const int maximumNumSteps = 1000;

        public TMCB()
        {
            this.Q = new HashSet<State<S>>();
            this.F = new HashSet<State<S>>();
            this.Sigma = new HashSet<C>();
            this.Gamma = new HashSet<C>();
            this.Delta = new Dictionary<State<S>, List<Transition<C, S>>>();
            this.DefaultTransitions = new Dictionary<State<S>, Transition<C, S>>();
        }

        public TMCB(HashSet<State<S>> Q, HashSet<State<S>> F, HashSet<C> Sigma, HashSet<C> Gamma, C blank)
        {
            this.Q = Q;
            this.F = F;
            this.Sigma = Sigma;
            this.Gamma = Gamma;
            this.blank = blank;
            this.Delta = new Dictionary<State<S>, List<Transition<C, S>>>();
            this.DefaultTransitions = new Dictionary<State<S>, Transition<C, S>>();
        }

        public State<S> CreateState(S label)
        {
            State<S> q = new State<S>(label);
            Q.Add(q);
            return q;
        }

        public void AddTransition(State<S> source, Transition<C, S> t)
        {
            List<Transition<C, S>> transitions;
            if (Delta.TryGetValue(source, out transitions))
            {
                transitions.Add(t);
            }
            else
            {
                transitions = new List<Transition<C, S>>();
                transitions.Add(t);
                Delta.Add(source, transitions);
            }
        }

        public void AddDefaultTransition(State<S> source, Transition<C, S> t)
        {
            if (DefaultTransitions.ContainsKey(source))
            {
                DefaultTransitions.Remove(source);
            }
            DefaultTransitions.Add(source, t);
        }

        public C[][] Run(int numTapes, out bool accepts)
        {
            C[][] tapes = new C[numTapes][];
            return this.Run(tapes, out accepts);
        }

        public C[][] Run(C[][] tapes_in, out bool accepts)
        {

            //terminate if there is no initial state
            if (q0 == null)
            {
                this.timeout = false;
                accepts = false;
                return tapes_in;
            }

            //create copy of tapes_in
            //initialize tapes that are null, expand short tapes to minimum length
            C[][] tapes = new C[tapes_in.Length][];
            for(int i=0; i<tapes.Length; ++i)
            {
                if (tapes_in[i] == null)
                {
                    tapes[i] = new C[tapeExpansionLength];
                    for (int j = 0; j < tapeExpansionLength; ++j)
                    {
                        tapes[i][j] = blank;
                    }
                }
                else
                {
                    if (tapes_in[i].Length < tapeExpansionLength)
                    {
                        C[] newTape = new C[tapeExpansionLength];
                        for (int j = 0; j < tapeExpansionLength; ++j)
                        {
                            newTape[j] = blank;
                        }
                        tapes_in[i].CopyTo(newTape, 0);
                        tapes[i] = newTape;
                    }
                    else
                    {
                        tapes_in[i].CopyTo(tapes[i], 0);
                    }
                }

            }

            State<S> currentState = q0;
            int[] headPositions = new int[tapes.Length];
            int step = 0;

            for (step = 0; step < maximumNumSteps; ++step)
            {
                C[] tapeLetters = getTapeLetters(tapes, headPositions);
                //get enabled transition if one exists
                Transition<C, S> transition = getEnabledTransition(currentState, tapeLetters);
                //if it does not exist, terminate
                if (transition == null)
                {
                    break;
                }
                else
                {
                    //execute transition
                    //write to tapes that exist
                    foreach (KeyValuePair<int, C> writeAction in transition.Write)
                    {
                        int tapeIndex = writeAction.Key;
                        if (tapeIndex < tapes.Length)
                        {
                            tapes[writeAction.Key][headPositions[writeAction.Key]] = writeAction.Value;
                        }
                    }
                    //move on tapes that exist
                    foreach (KeyValuePair<int, Directions> moveAction in transition.Move)
                    {
                        int tapeIndex = moveAction.Key;
                        if (tapeIndex < tapes.Length)
                        {
                            headPositions[tapeIndex] += (int)moveAction.Value - 1; //-1 shifts {Left, Non, Right} -> {-1, 0, 1}
                                                                                   //expand tape if needed and adjust headPosition
                            if (headPositions[tapeIndex] < 0)
                            {
                                C[] newTape = new C[tapes[tapeIndex].Length + tapeExpansionLength];
                                tapes[tapeIndex].CopyTo(newTape, tapeExpansionLength);
                                tapes[tapeIndex] = newTape;
                                headPositions[tapeIndex] += tapeExpansionLength;
                                for (int j = 0; j < tapeExpansionLength; ++j)
                                {
                                    tapes[tapeIndex][j] = blank;
                                }
                            }
                            if (headPositions[tapeIndex] >= tapes[tapeIndex].Length)
                            {
                                C[] newTape = new C[tapes[tapeIndex].Length + tapeExpansionLength];
                                tapes[tapeIndex].CopyTo(newTape, 0);
                                tapes[tapeIndex] = newTape;
                                for (int j = tapeExpansionLength; j < newTape.Length; ++j)
                                {
                                    tapes[tapeIndex][j] = blank;
                                }
                            }
                        }
                    }
                    //update state
                    if(transition.TargetState != null)
                    {
                        currentState = transition.TargetState;
                    }
                }
            }
            accepts = F.Contains(currentState);
            this.timeout = step == maximumNumSteps && !accepts;
            return tapes;
        }
        
        private Transition<C, S> getEnabledTransition(State<S> currentState, C[] tapeLetters)
        {
            List<Transition<C, S>> transitions;
            if (Delta.TryGetValue(currentState, out transitions))
            {
                foreach (Transition<C, S> transition in transitions)
                {
                    if (transition.IsEnabled(tapeLetters))
                    {
                        return transition;
                    }
                }
            }
            Transition<C, S> defaultTransition;
            if (DefaultTransitions.TryGetValue(currentState, out defaultTransition))
            {
                return defaultTransition;
            }
            else
            {
                return null;
            }
        }

        private C[] getTapeLetters(C[][] tapes, int[] headPositions)
        {
            C[] rv = new C[tapes.Length];
            for (int i = 0; i < tapes.Length; i++)
            {
                rv[i] = tapes[i][headPositions[i]];
            }
            return rv;
        }

        //only for single-tape TMs
        public IEnumerable<C[]> GetLanguageRep(int maxCount)
        {
            int count = 0;
            Permutation<C> perm = new Permutation<C>(Sigma);
            while(count < maxCount)
            {
                C[] currentWord = perm.Next;
                bool accepts;
                Run(1, out accepts);
                if (accepts)
                {
                    yield return currentWord;
                    ++count;
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"q0: {q0.label}");
            sb.Append("Sigma:");
            foreach (C letter in Sigma)
            {
                sb.Append($" {letter.ToString()}");
            }
            sb.AppendLine();
            sb.Append("Gamma:");
            foreach (C letter in Gamma)
            {
                sb.Append($" {letter.ToString()}");
            }
            sb.AppendLine();
            sb.AppendLine($"blank: {blank}");
            sb.AppendLine("Delta:");
            foreach(KeyValuePair<State<S>, List<Transition<C, S>>> transitions in Delta)
            {
                var state = transitions.Key;
                var list = transitions.Value;
                sb.AppendLine($"State {state.label}");
                foreach(var t in list)
                {
                    sb.AppendLine(t.ToString());
                }
                Transition<C, S> defaultT;
                if(DefaultTransitions.TryGetValue(state, out defaultT))
                {
                    sb.AppendLine("default:");
                    sb.AppendLine(defaultT.ToString());
                }
            }
            return sb.ToString();
        }
    }

    public class Transition<C, S>
    {
        //---read---
        //Each item is a condition. If all conditions are met, the transition can fire.
        //Each condition consists of a tape index and an array of letters.
        //A condition is met if the indicated tape reads one of the letters in the array or if the array is empty/null
        public Dictionary<int, C[]> Read;
        public Dictionary<int, C> Write;
        public Dictionary<int, Directions> Move;
        public State<S> TargetState;

        public Transition()
        {
            this.Read = new Dictionary<int, C[]>();
            this.Write = new Dictionary<int, C>();
            this.Move = new Dictionary<int, Directions>();
        }
        
        public void AddReadCondition(int tape, params C[] letters)
        {
            AddReadCondition(false, tape, letters);
        }

        //reset == true : override possibly existing entry
        //reset == false: add letters to possibly existing entry
        public void AddReadCondition(bool reset, int tape, params C[] letters)
        {
            C[] oldLetters;
            if(Read.TryGetValue(tape, out oldLetters))
            {
                if (reset)
                {
                    Read[tape] = letters;
                }
                else
                {
                    C[] newLetters = new C[oldLetters.Length + letters.Length];
                    oldLetters.CopyTo(newLetters, 0);
                    letters.CopyTo(newLetters, oldLetters.Length);
                    Read[tape] = newLetters;
                }
            }
            else
            {
                Read.Add(tape, letters);
            }
        }

        public void AddWriteAction(int tape, C letter)
        {
            Write[tape] = letter;
        }

        public void AddMoveAction(int tape, Directions dir)
        {
            Move[tape] = dir;
        }

        //tapeLetters are the letters currently read from the tapes
        public bool IsEnabled(C[] tapeLetters)
        {
            foreach (KeyValuePair<int, C[]> condition in Read)
            {
                int tapeIndex = condition.Key;
                //check if the tape exists
                if(tapeIndex < tapeLetters.Length)
                {
                    C[] letters = condition.Value;
                    //condition is NOT met if
                    if (letters.Length != 0 && letters != null //array of letters is not empty/null
                        && !letters.Contains(tapeLetters[tapeIndex])) //and letters does not contain the letter on the tape
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("read:");
            foreach(KeyValuePair<int, C[]> condition in Read)
            {
                sb.Append($"{condition.Key}: ");
                foreach(C letter in condition.Value)
                {
                    sb.Append($"{letter}, ");
                }
                sb.Append(Environment.NewLine);
            }
            sb.AppendLine("write:");
            foreach (KeyValuePair<int, C> action in Write)
            {
                sb.AppendLine($"{action.Key}: {action.Value}");
            }
            sb.AppendLine("move:");
            foreach (KeyValuePair<int, Directions> action in Move)
            {
                sb.AppendLine($"{action.Key}: {action.Value}");
            }
            if(TargetState != null)
            {
                sb.AppendLine($"state: {TargetState.label}");
            }

            return sb.ToString();
        }
    }

    class Permutation<C>
    {
        public C[] Next
        {
            get
            {
                permutate();
                return current;
            }
        }

        private C[] sigma;
        private C[] current;
        private List<byte> numPerm;

        public Permutation(HashSet<C> sigma)
        {
            if(sigma.Count > 0)
            {
                this.sigma = sigma.ToArray();
            }
            
        }

        //modifies current
        //interprets letters as digits, current as number in LSBF-format
        //"increments" current, expanding the array if necessary
        private void permutate()
        {
            if (sigma != null)
            {
                if(current == null)
                {
                    this.current = new[] { this.sigma[0] };
                    this.numPerm = new List<byte>(new byte[] { 0 });
                }
                else
                {
                    int count = sigma.Length;

                    for (int i = 0; i < numPerm.Count; ++i)
                    {
                        byte result = (byte)((numPerm[i] + 1) % count);
                        numPerm[i] = result;
                        current[i] = sigma[result];
                        if (i == numPerm.Count - 1 && result == 0)
                        {
                            numPerm.Add(0);
                            C[] next = new C[current.Length + 1];
                            current.CopyTo(next, 0);
                            next[next.Length] = sigma[0];
                            current = next;
                        }
                        else
                        {
                            if (result != 0)
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}
