using System;
using System.Collections;
using System.Collections.Generic;
using cbboc;

public sealed class RandomCompetitor : Competitor
{
    private static readonly Random rand = new Random();

    public RandomCompetitor() : base(TrainingCategory.NONE) { }

    public override void train(List<ObjectiveFn> trainingSet, long maxTimeInMilliseconds)
    {
        // no training because we're in TrainingCategory.NONE 
        throw new InvalidOperationException();
    }

    public override void test(ObjectiveFn testCase, long maxTimeInMilliseconds)
    {
        //(Java) final long startTime = System.currentTimeMillis();
        while (true)
        {
            // final long elapsed = System.currentTimeMillis() - startTime;
            // if( elapsed > maxTimeInMilliseconds )
            //	break;
            // time budget reached when elapsed > maxTimeInMilliseconds 
            // could check as above, but loop will be terminated automatically 
            // when time or evaluation budget is exceeded... 

            bool[] candidate = randomBitvector(testCase.getNumGenes());
            double value = testCase.value(candidate);
            // Useful strategies will obviously care about value...
        }
    }

    ///////////////////////////////

    private static bool[] randomBitvector(int length)
    {
		//Trying to make generating random bit vectors faster
		int numBytes = (length - 1) / 8;
        bool[] result = new bool[length];
		byte[] bytes = new byte[numBytes];
		rand.NextBytes (bytes);
		BitArray bits = new BitArray (bytes);

		//CopyTo was faster than the for loop 
		//Not sure how to resize BitArray to length
		bits.CopyTo (result, 0);
		for (int i = bits.Count; i < length; ++i)
			result [i] = rand.NextDouble() >= 0.5;

        return result;
    }

    ///////////////////////////////	

    public static void Main(String[] args) //(Java) throws IOException
    {
        Competitor competitor = new RandomCompetitor();
        CBBOC.run( competitor );
	  }
}
