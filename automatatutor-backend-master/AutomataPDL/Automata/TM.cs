using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutomataPDL.Automata
{
    public class TM<C, S> : AbstractAutomaton<C, S> //Tahiti
    {
        public Dictionary<TwoTuple<State<S>, string>, ThreeTuple<State<S>, string, string>> delta { get; set; } //Änderung nötig
        public State<S> q_0 { get; set; }
        public HashSet<C> gamma { get; set; }
        //new Dictionary<TwoTuple<State<string>, string>, ThreeTuple<State<string>, string, string>>();
        public char blank;

        public TM(HashSet<State<S>> Q_in, HashSet<C> Sigma_in, HashSet<C> Gamma_in, Dictionary<TwoTuple<State<S>, string>, ThreeTuple<State<S>, string, string>> delta_in, State<S> q_0_in,  char blank_in, HashSet<State<S>> F_in)
        {
            Q = Q_in;
            Sigma = Sigma_in;
            gamma = Gamma_in;
            delta = delta_in;
            q_0 = q_0_in;
            blank = blank_in;
            F = F_in;

            //TODO: Test for "q_0 \in Q" and "F \subseteq Q"
        }

        public ThreeTuple<bool, string[], StringBuilder[]> ComputeTapes( string[] tapeList, int maxSteps)
        {
            State<S> current = q_0;
            ThreeTuple<State<S>, string, string> resolutionList = new ThreeTuple<State<S>, string, string>(current, "", "");
            int[] tapeHeadPosition = new int[tapeList.Length];
            StringBuilder[] strB = new StringBuilder[tapeList.Length];
            int i = 0;
            for(i = 0; i < tapeList.Length; i++)
            {
                tapeHeadPosition[i] = 0;
                strB[i] = new StringBuilder(tapeList[i]);
            }
            bool acceptState;

            i = 0;
            for (i = 0; i < maxSteps; i++)
            {
                string readingSymbols = "";// = ""; //tapeList[tapeHeadPostiotions[]] + tapeList[] + tapeList[]);
                for (int j = 0; j < tapeHeadPosition.Length; j++)
                {
                    //readingSymbols = readingSymbols + tapeList[j][tapeHeadPosition[j]];
                    readingSymbols = readingSymbols + strB[j][tapeHeadPosition[j]];

                }
                TwoTuple<State<S>, string> tuple = new TwoTuple<State<S>, string>(current, readingSymbols);
                if (!delta.TryGetValue(tuple, out resolutionList))
                    break;
                for (int k = 0; k < tapeHeadPosition.Length; k++)
                {
                    //tapeList[k][tapeHeadPosition[k]] = resolutionList.second.ToCharArray()[k];
                    strB[k][tapeHeadPosition[k]] = resolutionList.second.ToCharArray()[k];
                    current = resolutionList.first;
                    switch (resolutionList.third.ToCharArray()[k])
                    {
                        case 'R':
                            tapeHeadPosition[k]++;
                            if(tapeHeadPosition[k] == strB[k].Length)
                                strB[k].Append("?");
                            break;
                        case 'L':
                            if(tapeHeadPosition[k] == 0)
                            {
                                strB[k].Insert(0, "?");
                            }
                            else
                                tapeHeadPosition[k]--;
                            break;
                        case 'N':
                            break;
                    }
                }
            }

            if (i == maxSteps)
            {
                acceptState = false;
            }
            else
            {
                acceptState = F.Contains(current);
            }

            /*State<S> current = q_0;
            int[] tapeHeadPosition = new int[tapeList.Length];

            for (int i = 0; i < maxSteps; i++)
            {
                S[] readingSymbols = new S[tapeList.Length];// = ""; //tapeList[tapeHeadPostiotions[]] + tapeList[] + tapeList[]);
                for (int j = 0; j < tapeHeadPosition.Length; j++)
                {
                    readingSymbols[j] = tapeList[tapeHeadPosition[j]];
                    TwoTuple<State<S>, S> tuple = new TwoTuple<State<S>, S>(current, readingSymbols);
                    if (!delta.TryGetValue(tuple, out current))
                        break;// return false;
                }

                return F.Contains(current);*/

            //TODO List generation from tapeList
            //var resolutionStringList = new List<string>();
            var resolutionStringList = new string[tapeHeadPosition.Length];
            for (int j = 0; j < tapeHeadPosition.Length; j++)
            {
                string currentTapeWord = "";
                while (tapeHeadPosition[j] < strB[j].Length && strB[j][tapeHeadPosition[j]] != '?')
                {
                    currentTapeWord = currentTapeWord + strB[j][tapeHeadPosition[j]];
                    tapeHeadPosition[j]++;
                }
                resolutionStringList[j] = currentTapeWord;
            }
            return new ThreeTuple<bool, string[], StringBuilder[]>(acceptState, resolutionStringList, strB);
        }
    }
}
