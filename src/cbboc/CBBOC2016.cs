using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.IO;

namespace cbboc
{
    public sealed class CBBOC2016
    {
        private readonly static long BASE_TIME_PER_INSTANCE_IN_MILLIS = 250 * 1000L;
        //(Java) public static Logger LOGGER = Logger.getLogger(CBBOC2016.class.getName() );

        //(Java) public static readonly Boolean LOGGING_ENABLED = false;
        /*(Java) static {
            if( !LOGGING_ENABLED )
                LogManager.getLogManager().reset();
        }*/

        ////////////////////////////////

        public static long trainingEndTime = -1;
        public static long testingEndTime = -1;
        public static Stopwatch trainTime = new Stopwatch();
        public static Stopwatch testTime = new Stopwatch();

        ////////////////////////////////
        //(Java) static
        public sealed class EvaluationsExceededException : Exception
        {
            private static readonly long serialVersionUID = 1L;
        }

        //(Java) static
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

            //(Java) readonly long startTime = System.currentTimeMillis();
            trainTime = Stopwatch.StartNew();
            //(Java) readonly
            long maxTime = BASE_TIME_PER_INSTANCE_IN_MILLIS * p.Count * (long)client.getTrainingCategory();
            //(Java) trainingEndTime = startTime + maxTime;
            trainingEndTime = maxTime;

            try
            {
                client.train(p, maxTime);
            }
            //(Java) catch (TimeExceededException | EvaluationsExceededException ex ) {
            catch (TimeExceededException)
            {
                // Intentionally Empty
            }
            catch (EvaluationsExceededException)
            {
                // Intentionally Empty
            }

            //(Java) readonly long endTime = System.currentTimeMillis();
            trainTime.Stop();
            return trainTime.ElapsedMilliseconds;
            //(Java) return endTime - startTime;
        }

        ////////////////////////////////

        private static long testClient(Competitor client, List<ObjectiveFn> fns)
        {

            //(Java) readonly long startTime = System.currentTimeMillis();
            testTime = Stopwatch.StartNew();

            foreach (ObjectiveFn fn in fns)
            {
                try
                {
                    //(Java) readonly long now = System.currentTimeMillis();
                    long maxTime = BASE_TIME_PER_INSTANCE_IN_MILLIS;

                    //(Java) testingEndTime = now + maxTime;
                    testingEndTime = maxTime;
                    client.test(fn, maxTime);
                }
                //(Java) catch (TimeExceededException | EvaluationsExceededException ex ) {
                catch (TimeExceededException)
                {
                    // Intentionally Empty
                }
                catch (EvaluationsExceededException)
                {
                    // Intentionally Empty
                }
            }

            //(Java) readonly long endTime = System.currentTimeMillis();
            testTime.Stop();
            return testTime.ElapsedMilliseconds;
            //(Java) return endTime - startTime;
        }

        ////////////////////////////////

        //(Java) static final
        private sealed class OutputResults
        {
            //(Java) JUST readonly ON ALL
            public readonly string competitorName;
            public readonly string competitorLanguage = "C#";
            public readonly string problemClassName;
            // readonly string trainingCategory;
            public readonly TrainingCategory trainingCategory;
            public readonly string datetime;
            public readonly List<Result> trainingResults = new List<Result>(); //(Java) new ArrayList<Result>();
            public readonly long trainingWallClockUsage;
            public readonly List<Result> testingResults = new List<Result>(); //(Java) new ArrayList<Result>();
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
                    this.trainingResults.Add(new Result(o.getRemainingEvaluations(), p.Item1, p.Item2)); //(Java) p.getLeft(), p.getRight()));
                }
                this.trainingWallClockUsage = trainingWallClockUsage;

                foreach (ObjectiveFn o in problemClass.getTestingInstances())
                {
                    Tuple<long, double> p = o.getRemainingEvaluationsAtBestValue();
                    this.testingResults.Add(new Result(o.getRemainingEvaluations(), p.Item1, p.Item2)); //(Java) p.getLeft(), p.getRight()));
                }
                this.testingWallClockUsage = testingWallClockUsage;
            }

            ///////////////////////////

            public string toJSonString()
            {
                //(Java) Gson gson = new Gson();
                //(Java) string result = gson.toJson(this);
                //(Java) result = result.replaceAll(",", ",\n\t");
                //(Java) return result;
                var json = new JavaScriptSerializer().Serialize(this);
				return json.Replace(",",",\n\t");
            }
        }

        ////////////////////////////////	

        public static void run(Competitor client) //throws IOException
        {

            string problemClassName;

            // read in root for problem class from classFolder.txt
            //(Java) BufferedReader reader = null;
            string path = null;
            //(Java) try
            //(Java) {
            string problemClassFile = Directory.GetCurrentDirectory() + "/resources/classFolder.txt";
            using (StreamReader sr = File.OpenText(problemClassFile))
            {
                //(Java) reader = new BufferedReader(new FileReader(problemClassFile));
                problemClassName = sr.ReadLine();
                path = Directory.GetCurrentDirectory() + "/resources/" + problemClassName;
            }
            //(Java) finally
            //(Java) {
            //(Java)    if (reader != null)
            //(Java)        reader.close();
            //(Java) }

            // String relativePathToProblem = "/resources/test/toy/";
            // String path = root + "/resources/test";
            // String path = root + relativePathToProblem;
            ProblemClass problemClass = new ProblemClass(Path.GetFullPath(path), client.getTrainingCategory());

            long actualTrainingTime = 0;
            long actualTestingTime = 0;

            switch (client.getTrainingCategory())
            {
                case TrainingCategory.NONE:
                    {
                        actualTestingTime = testClient(client, problemClass.getTestingInstances());
                        Console.Error.WriteLine("actualTestingTime:" + actualTestingTime); //(Java) LOGGER (C# does not seem to have an equivalent, so Error chosen instead)
                    }
                    break;
                case TrainingCategory.SHORT:
                case TrainingCategory.LONG:
                    {
                        actualTrainingTime = trainClient(client, problemClass.getTrainingInstances());
                        Console.Error.WriteLine("actualTrainingTime:" + actualTrainingTime); //(Java) LOGGER (C# does not seem to have an equivalent, so Error chosen instead)

                        actualTestingTime = testClient(client, problemClass.getTestingInstances());
                        Console.Error.WriteLine("actualTestingTime:" + actualTestingTime); //(Java) LOGGER (C# does not seem to have an equivalent, so Error chosen instead)
                    }
                    break;
                default:
                    throw new InvalidOperationException(); //(Java) IllegalStateException();
            }

            ///////////////////////////

            //(Java) DateFormat dateFormat = new SimpleDateFormat("yyyy-MM-dd-HH-mm-ss");
            //(Java) String timestamp = dateFormat.format(new Date());
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            OutputResults results = new OutputResults(client.GetType().FullName,
                    timestamp, problemClassName, problemClass, actualTrainingTime, actualTestingTime);

            string outputPath = path + "/results/" + "CBBOC2015results-" + client.GetType().Name + "-" + problemClassName + "-" + timestamp + ".json";
            //(Java) PrintWriter pw = new PrintWriter(new FileOutputStream(new File(outputPath)));
            using (StreamWriter sw = new StreamWriter(outputPath))
            {
                //(Java) pw.println(results.toJSonString());
                sw.WriteLine(results.toJSonString());
            }
            //(Java) pw.close();

            ///////////////////////////		

            //(Java) System.out.println(results.toJSonString());
            Console.WriteLine(results.toJSonString());
        }
    }
}
