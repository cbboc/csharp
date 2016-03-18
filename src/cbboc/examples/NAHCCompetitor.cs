using System;
using System.Collections.Generic;
using cbboc;

/**
 * Baseline metaheuristic strategy: Next Ascent Hillclimbing.  
 *
 */

public sealed class NAHCCompetitor : Competitor
{
    private static readonly Random rand = new Random();

    public NAHCCompetitor() : base(TrainingCategory.NONE) { }

    public override void train(List<ObjectiveFn> trainingSet, long maxTimeInMilliseconds)
    {
        // no training because we're in TrainingCategory.NONE 
        throw new InvalidOperationException(); //(Java) UnsupportedOperationException();
    }

    ///////////////////////////////	
    public override void test(ObjectiveFn testCase, long maxTimeInMilliseconds)
    {
        bool[] incumbent = randomBitvector(testCase.getNumGenes());
        double bestValue = testCase.value(incumbent);
        bool improved = false;
        do
        {
            List<bool[]> neighbors = hamming1Neighbours(incumbent);
            foreach (bool[] neighbor in neighbors)
            {
                //(Java) readonly 
                double value = testCase.value(neighbor);
                if (value > bestValue)
                {
                    improved = true;
                    incumbent = neighbor;
                    bestValue = value;
                }
            }

        } while (improved && testCase.getRemainingEvaluations() > 0);
    }

    ///////////////////////////////

    private static List<bool[]> hamming1Neighbours(bool[] incumbent)
    {
        List<bool[]> result = new List<bool[]>();
        for (int i = 0; i < incumbent.Length; ++i)
        {
            bool[] neighbour = (bool[])incumbent.Clone();
            neighbour[i] = !neighbour[i];
            result.Add(neighbour);
        }

        return result;
    }

    ///////////////////////////////

    private static bool[] randomBitvector(int length)
    {
        bool[] result = new bool[length];
        for (int i = 0; i < length; ++i)
            result[i] = (rand.NextDouble() >= 0.5); //(Java) RNG.get().nextBoolean();

        return result;
    }

    ///////////////////////////////	

    public static void main(String[] args) //(Java) throws IOException
    {

        Competitor competitor = new NAHCCompetitor();
        CBBOC2016.run(competitor);

        Console.WriteLine("All done.");
    }
}
