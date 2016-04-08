using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using cbboc;

/**
 * Baseline learning strategy: Simulated Annealing hyper-heuristic 
 * which learns the start and end values for the annealing schedule.  
 */

public sealed class SAHHCompetitor : Competitor
{
    private double saScheduleLowerBound = 0.0;
    private double saScheduleUpperBound = Double.MaxValue;
    private static readonly Random rand = new Random();

    ///////////////////////////////

    public SAHHCompetitor(TrainingCategory trainingCategory) : base(trainingCategory)
    {
        Debug.Assert(invariant());
    }

    ///////////////////////////////

    private static Tuple<double, double>
    WhiteTemperatureRangeForSA(List<double> fitnessTrajectory)
    {
        if (fitnessTrajectory.Count == 0)
            throw new ArgumentException();

        /**
         * @see:
         * @inproceedings{white:1984,
         *  address = {Port Chester, NY},
         *  author = {White, S. R.},
         *  booktitle = {Proceeedings of the IEEE International Conference on Computer Design (ICCD) '84},
         *  pages = {646--651},
         *  title = {Concepts of Scale in Simulated Annealing},
         *  year = {1984}
         * }
         */
        double minDifference = Double.MaxValue;
        double[] asArray = new double[fitnessTrajectory.Count];
        asArray[0] = fitnessTrajectory[0];
        for (int i = 1; i < fitnessTrajectory.Count; ++i)
        {
            asArray[i] = fitnessTrajectory[i];

            double delta = Math.Abs(fitnessTrajectory[i] - fitnessTrajectory[i - 1]);
            if (delta < minDifference)
                minDifference = delta;
        }

        double avg = asArray.Average();
        double variance = (asArray.Sum(d => (d - avg) * (d - avg))) / asArray.Length;
        return Tuple.Create(minDifference, Math.Sqrt(variance));
    }

    ///////////////////////////////

    private static bool[] randomHamming1Neighbour(bool[] incumbent)
    {
        bool[] neighbour = (bool[])incumbent.Clone();
        int randomIndex = rand.Next(neighbour.Length);
        neighbour[randomIndex] = !neighbour[randomIndex];
        return neighbour;
    }

    ///////////////////////////////

    private static List<double>
    fitnessTrajectoryOfRandomWalk(ObjectiveFn f, long numSteps)
    {
        bool[] incumbent = randomBitvector(f.getNumGenes());

        List<double> result = new List<double>();
        for (int i = 0; i < numSteps; ++i)
        {
            bool[] incoming = randomHamming1Neighbour(incumbent);
            result.Add(f.value(incoming));

            incumbent = incoming;
        }
        return result;
    }

    ///////////////////////////////	

    public override void train(List<ObjectiveFn> trainingSet, long maxTimeInMilliseconds)
    {
        long evalPerCase = trainingSet[0].getRemainingEvaluations() / trainingSet.Count;

        // ^ `remaining evaluations' for training are shared across all instances.
        //		int totalEvaluations = 0;
        //		for( int i=0; i<trainingSet.size(); ++i )
        //			totalEvaluations += trainingSet.get( i ).getRemainingEvaluations();

        double[] saScheduleLowerBounds = new double[trainingSet.Count];
        double[] saScheduleUpperBounds = new double[trainingSet.Count];
        for (int i = 0; i < trainingSet.Count; ++i)
        {
            Tuple<double, double> saScheduleBounds = WhiteTemperatureRangeForSA(fitnessTrajectoryOfRandomWalk(trainingSet[i], evalPerCase));
            saScheduleLowerBounds[i] = saScheduleBounds.Item1;
            saScheduleUpperBounds[i] = saScheduleBounds.Item2;
        }

        saScheduleLowerBound = saScheduleLowerBounds.Average();
        saScheduleUpperBound = saScheduleUpperBounds.Average();
        Debug.Assert(invariant());
    }

    ///////////////////////////////

    static double minDiffDivT = Double.PositiveInfinity;
    static double maxDiffDivT = 0.0;

    private static bool SAAccept(double lastValue, double currentValue, double temperature)
    {
        if (Double.IsNaN(temperature) || temperature < 0)
            throw new ArgumentException("Expected non-negative temperature, found:" + temperature);

        // assumes maximising...
        if (currentValue > lastValue)
            return true;
        else if (temperature == 0)
            return currentValue >= lastValue;
        else {
            Debug.Assert(currentValue <= lastValue);
            Debug.Assert(temperature > 0);

            double diffDivT = (currentValue - lastValue) / temperature;
            // assert( diffDivT >= 0 );

            if (diffDivT < minDiffDivT)
                minDiffDivT = diffDivT;
            if (diffDivT > maxDiffDivT)
                maxDiffDivT = diffDivT;

            double p = Math.Exp(diffDivT);
            Debug.Assert(!Double.IsNaN(p));
            return rand.NextDouble() < p;
        }
    }

    ///////////////////////////////	

    public override void test(ObjectiveFn testCase, long maxTimeInMilliseconds)
    {
        bool[] incumbent = randomBitvector(testCase.getNumGenes());
        double lastValue = testCase.value(incumbent);

        long numEvaluations = testCase.getRemainingEvaluations();
        for (int i = 0; i < numEvaluations; ++i)
        {

            bool[] incoming = randomHamming1Neighbour(incumbent);
            double value = testCase.value(incoming);

            // linear annealing schedule...
			double temperature = ((1.0 - (i / (double) (numEvaluations - 1)))*(saScheduleUpperBound-saScheduleLowerBound))+saScheduleLowerBound;
            if (SAAccept(lastValue, value, temperature))
            {
                incumbent = incoming;
                lastValue = value;
            }
        }
    }

    ///////////////////////////////

    private static bool[] randomBitvector(int length)
    {
        bool[] result = new bool[length];
        for (int i = 0; i < length; ++i)
            result[i] = (rand.NextDouble() < 0.5);

        return result;
    }

    public bool invariant()
    {
        return saScheduleLowerBound < saScheduleUpperBound;
    }

    ///////////////////////////////	

    public static void Main(String[] args) //(Java) throws IOException
    {

        Competitor competitor = new SAHHCompetitor(TrainingCategory.SHORT);
        CBBOC.run(competitor);

        Console.Error.WriteLine("minDiffDivT: " + minDiffDivT + "exp:" + Math.Exp(minDiffDivT));
        Console.Error.WriteLine("maxDiffDivT:" + maxDiffDivT + "exp:" + Math.Exp(maxDiffDivT));


        Console.WriteLine("All done.");
    }
}
