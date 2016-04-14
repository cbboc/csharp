using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.IO;

namespace cbboc
{
    public sealed class CBBOC
    {
        private readonly static long BASE_TIME_PER_INSTANCE_IN_MILLIS = 250 * 1000L;

        ////////////////////////////////

        public static long trainingEndTime = -1;
        public static long testingEndTime = -1;
        public static Stopwatch trainTime = new Stopwatch();
        public static Stopwatch testTime = new Stopwatch();

        ////////////////////////////////

        public sealed class EvaluationsExceededException : Exception
        {
            private static readonly long serialVersionUID = 1L;
        }

        public sealed class TimeExceededException : Exception
        {
            private static readonly long serialVersionUID = 1L;
        }

        ////////////////////////////////

        private static Boolean allSameN(List<ObjectiveFn> p)
        {
            int N = p[0].getNumGenes();
            for (int i = 1; i < p.Count; ++i)
                if (p[i].getNumGenes() != N)
                    return false;

            return true;
        }

        ////////////////////////////////	

        private static long trainClient(Competitor client, List<ObjectiveFn> p)
        {
            Debug.Assert(allSameN(p));

            trainTime = Stopwatch.StartNew();
            long maxTime = BASE_TIME_PER_INSTANCE_IN_MILLIS * p.Count * (long)client.getTrainingCategory();
			trainingEndTime = maxTime;

            try
            {
                client.train(p, maxTime);
            }
            catch (TimeExceededException)
            {
                // Intentionally Empty
            }
            catch (EvaluationsExceededException)
            {
                // Intentionally Empty
            }

            trainTime.Stop();
            return trainTime.ElapsedMilliseconds;
        }

        ////////////////////////////////

        private static long testClient(Competitor client, List<ObjectiveFn> fns)
        {
            testTime = Stopwatch.StartNew();

            foreach (ObjectiveFn fn in fns)
            {
                try
                {
                    long maxTime = BASE_TIME_PER_INSTANCE_IN_MILLIS;

					testingEndTime = testTime.ElapsedMilliseconds + maxTime;
                    client.test(fn, maxTime);
                }
                catch (TimeExceededException)
                {
                    // Intentionally Empty
                }
                catch (EvaluationsExceededException)
                {
                    // Intentionally Empty
                }
            }

            testTime.Stop();
            return testTime.ElapsedMilliseconds;
        }

        ////////////////////////////////

        private sealed class OutputResults
        {
            public readonly string competitorName;
            public readonly string competitorLanguage = "C#";
            public readonly string problemClassName;
            // readonly string trainingCategory;
            public readonly TrainingCategory trainingCategory;
            public readonly string datetime;
            public readonly List<Result> trainingResults = new List<Result>();
            public readonly long trainingWallClockUsage;
            public readonly List<Result> testingResults = new List<Result>();
            public readonly long testingWallClockUsage;

            ///////////////////////////

            //(Java) static final
            public sealed class Result
            {
                public readonly long remainingEvaluations;
                public readonly long remainingEvaluationsWhenBestReached;
                public readonly double bestValue;

                ///////////////////////////

                public Result(long remainingEvaluations, long remainingEvaluationsWhenBestReached, double bestValue)
                {
                    this.remainingEvaluations = remainingEvaluations;
                    this.remainingEvaluationsWhenBestReached = remainingEvaluationsWhenBestReached;
                    this.bestValue = bestValue;
                }
            }

            ///////////////////////////

            public OutputResults(string competitorName, string datetime,
                    string problemClassName, ProblemClass problemClass, long trainingWallClockUsage, long testingWallClockUsage)
            {

                this.competitorName = competitorName;
                this.problemClassName = problemClassName;
                this.datetime = datetime;
				Console.WriteLine(problemClass.ToString());
				Console.WriteLine(problemClass.getTrainingCategory());
                this.trainingCategory = problemClass.getTrainingCategory();
                foreach (ObjectiveFn o in problemClass.getTrainingInstances())
                {
                    Tuple<long, double> p = o.getRemainingEvaluationsAtBestValue();
                    this.trainingResults.Add(new Result(o.getRemainingEvaluations(), p.Item1, p.Item2));
                }
                this.trainingWallClockUsage = trainingWallClockUsage;

                foreach (ObjectiveFn o in problemClass.getTestingInstances())
                {
                    Tuple<long, double> p = o.getRemainingEvaluationsAtBestValue();
                    this.testingResults.Add(new Result(o.getRemainingEvaluations(), p.Item1, p.Item2));
                }
                this.testingWallClockUsage = testingWallClockUsage;
            }

            ///////////////////////////

            public string toJSonString()
            {
                var json = new JavaScriptSerializer().Serialize(this);
				return json.Replace(",",",\n\t");
            }
        }

        ////////////////////////////////	

        public static void run(Competitor client) //throws IOException
        {

            string problemClassName;

            // read in root for problem class from classFolder.txt
            string path = null;
            string problemClassFile = Directory.GetCurrentDirectory() + "/resources/classFolder.txt";
            using (StreamReader sr = File.OpenText(problemClassFile))
            {
                problemClassName = sr.ReadLine();
                path = Directory.GetCurrentDirectory() + "/resources/" + problemClassName;
            }
            
            ProblemClass problemClass = new ProblemClass(Path.GetFullPath(path), client.getTrainingCategory());

            long actualTrainingTime = 0;
            long actualTestingTime = 0;

            switch (client.getTrainingCategory())
            {
                case TrainingCategory.NONE:
                    {
                        actualTestingTime = testClient(client, problemClass.getTestingInstances());
                        Console.Error.WriteLine("actualTestingTime:" + actualTestingTime);
                    }
                    break;
                case TrainingCategory.SHORT:
                case TrainingCategory.LONG:
                    {
                        actualTrainingTime = trainClient(client, problemClass.getTrainingInstances());
                        Console.Error.WriteLine("actualTrainingTime:" + actualTrainingTime);

                        actualTestingTime = testClient(client, problemClass.getTestingInstances());
                        Console.Error.WriteLine("actualTestingTime:" + actualTestingTime);
                    }
                    break;
                default:
                    throw new InvalidOperationException(); //(Java) IllegalStateException();
            }

            ///////////////////////////

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            OutputResults results = new OutputResults(client.GetType().FullName,
                    timestamp, problemClassName, problemClass, actualTrainingTime, actualTestingTime);

            string outputPath = path + "/results/" + "CBBOCresults-" + client.GetType().Name + "-" + problemClassName + "-" + timestamp + ".json";
            using (StreamWriter sw = new StreamWriter(outputPath))
            {
                sw.WriteLine(results.toJSonString());
            }

            ///////////////////////////		

            Console.WriteLine(results.toJSonString());
        }
    }
}
