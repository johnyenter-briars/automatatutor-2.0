using System;
using System.Collections.Generic;

namespace ProblemGeneration
{
    public abstract class Generator<T> : Generator where T : Problem
    {
        public abstract T Generate(int targetDifficulty);
        public abstract String TypeName();

        Problem Generator.Generate(int targetDifficulty)
        {
            return Generate(targetDifficulty);
        }
    }


    public interface Generator
    {
        Problem Generate(int targetDifficulty);
        String TypeName(); //returns the type string used by the frontend
    }


    public static class Generation
    {
        //target difficulty levels to guide problem generation
        public static readonly int ANY = -1;
        public static readonly int HIGH = 2;
        public static readonly int MEDIUM = 1;
        public static readonly int LOW = 0;

        //normalizes values to be in [0;100] and handels values bigger than 100 in a good way. 
        //-inf -> 0
        // 0   -> 3
        // 20  -> 8
        // 40  -> 22
        // 50  -> 37
        // 80  -> 75
        // 90  -> 85
        // 100 -> 90
        // 120 -> 97
        // 160 -> 100
        public static int Normalization1(double d)
        {
            return (int)Math.Round((Math.Tanh((d - 60) * 0.03) + 1) * 50);
        }

        //generates a problem having at least quality <minQual>.
        //returns the first problem with the requested quality (or the problem with the highest quality)
        public static Problem generateWithMinQuality(Generator gen, int numberOfTries, double minQual) 
        {
            Problem bestSoFar = null;
            double qual = 0;
            for(int i = 0; i < numberOfTries; i++)
            {
                Console.WriteLine($"try #{i}");
                Problem cur = gen.Generate(ANY);
                double curQual = cur.Quality();
                if (curQual > minQual) return cur;
                if (bestSoFar == null || qual < curQual)
                {
                    bestSoFar = cur;
                    qual = curQual;
                }
            }
            return bestSoFar;
        }

        //generates the hardest problem possible while with at least quality minQual
        //returns the hardest problem (or the highest quality one if no good enough problem was found)
        public static Problem generateHardestWithMinQuality(Generator gen, int numberOfTries, double minQual) 
        {
            Problem bestSoFar = null;
            double qual = 0;
            int dif = 0;
            for (int i = 0; i < numberOfTries; i++)
            {
                Console.WriteLine($"try #{i}");
                Problem cur = gen.Generate(HIGH);
                double curQual = cur.Quality();
                int curDif = cur.Difficulty();
                if (bestSoFar == null || qual < minQual && qual < curQual || qual >= minQual && curQual >= minQual && curDif > dif)
                {
                    bestSoFar = cur;
                    qual = curQual;
                    dif = curDif;
                }
            }
            return bestSoFar;
        }

        private static int ProblemComparator(Problem a, Problem b)
        {
            return b.Difficulty() - a.Difficulty();
        }

        //generates a problem with a difficutly level between <minDif> and <maxDif>
        //returns the highest quality problem in that bound (or null if no such problem was found)
        //Christian: added diff and a third case, in which bestSoFar is not in the difficulty bounds but cur is.
        public static Problem generateBestWithDifficultyBounds(Generator gen, int numberOfTries, int minDif, int maxDif) 
        {
            Problem bestSoFar = null;
            double qual = 0;
            double diff = 0;
            List<Problem> problemList = new List<Problem>();
            for (int i = 0; i < numberOfTries; i++)
            {
                Problem cur = gen.Generate(ANY);
                double curQual = cur.Quality();
                int curDif = cur.Difficulty();
                if (minDif <= curDif && curDif <= maxDif && curQual >= 0.6)
                {
                    problemList.Add(cur);
                }
            }
            problemList.Sort(ProblemComparator);
            foreach(Problem prob in problemList){
                if (prob.isValid())
                    return prob;
            }
            // We didn't get any valid problem
            return generateBestWithDifficultyBounds(gen, numberOfTries, minDif, maxDif);
        }
    }
}
