﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web.Script.Serialization;
using System.Reflection;

namespace cbboc
{
    public sealed class ProblemClass
    {
		public TrainingCategory trainingCategory;
		public readonly List<ObjectiveFn> training = new List<ObjectiveFn> ();
		public readonly List<ObjectiveFn> testing = new List<ObjectiveFn> ();

        ///////////////////////////////

        private static List<string> readInstances(string testingInventoryName) //File testingFilesTxt) //(Java) throws IOException
        {
            //(Java) if (!testingFilesTxt.exists())
            if (!File.Exists(testingInventoryName))
                throw new FileNotFoundException(testingInventoryName + " not found"); //(Java) IllegalArgumentException();

            //(Java) InputStream is = new FileInputStream(testingFilesTxt);
            //(Java) LineNumberReader r = new LineNumberReader(new BufferedReader(new InputStreamReader( is )));

            //(Java) string line = r.readLine();
            //(Java) Scanner scanner = new Scanner(line);
            //(Java) readonly
            //(Java) int numInstances;
            //(Java) try
            //(Java) {
            //(Java) numInstances = scanner.nextInt();
            //(Java) }
            //(Java) finally
            //(Java) {
            //(Java) scanner.close();
            //(Java) }

            //(Java) try
            int numInstances;
            List<string> result = new List<string>();
            using (StreamReader sr = File.OpenText(testingInventoryName))
            {
                bool parsed = Int32.TryParse(sr.ReadLine(), out numInstances);

                if (parsed)
                {
                    for (int i = 0; i < numInstances; ++i)
                        result.Add(sr.ReadLine());

                    Debug.Assert(result.Count == numInstances);
                }
                else
                {
                    throw new IOException();
                }
            }
            return result;
            //(Java) finally
            //(Java) {
            //(Java) r.close();
            //(Java) }
        }

        ///////////////////////////////	

        public ProblemClass(string root, TrainingCategory trainingCategory) //(Java) throws IOException
        {

            this.trainingCategory = trainingCategory;
            //(Java) File trainingFilesInventory = new File(root + "/trainingFiles.txt");
            //(Java) if (!trainingFilesInventory.exists())
            //(Java)    throw new RuntimeException("Fatal problem class file error: cannot find " + trainingFilesInventory);

            //(Java) File testingFilesInventory = new File(root + "/testingFiles.txt");
            //(Java) if (!testingFilesInventory.exists())
            //(Java)    throw new RuntimeException("Fatal problem class file error: cannot find " + testingFilesInventory);

            string trainingFilesInventory = root + "/trainingFiles.txt";
            string testingFilesInventory = root + "/testingFiles.txt";

            List<string> trainingFiles = readInstances(trainingFilesInventory);
            List<string> testingFiles = readInstances(testingFilesInventory);

            ///////////////////////////

            List<ProblemInstance> trainingInstances = new List<ProblemInstance>();
            switch (trainingCategory)
            {
                case TrainingCategory.NONE: // Intentionally Empty
                    break;
                case TrainingCategory.SHORT:
                case TrainingCategory.LONG:
                    foreach (string s in trainingFiles)
                        trainingInstances.Add(new ProblemInstance(File.OpenText(root + "/" + s))); //(Java) new FileInputStream(root + "/" + s)));
                    break;
            }

            int totalTrainingEvaluations = 0;
            foreach (ProblemInstance p in trainingInstances)
                totalTrainingEvaluations += p.getMaxEvalsPerInstance();

            totalTrainingEvaluations *= (int)trainingCategory;

            ///////////////////////////

            long sharedTrainingEvaluations = totalTrainingEvaluations;
            foreach (ProblemInstance p in trainingInstances)
                training.Add(new ObjectiveFn(p, ObjectiveFn.TimingMode.TRAINING, sharedTrainingEvaluations));

            ///////////////////////////

            List<ProblemInstance> testingInstances = new List<ProblemInstance>();
            foreach (string f in testingFiles)
                testingInstances.Add(new ProblemInstance(File.OpenText(root + "/" + f))); //(Java) new FileInputStream(root + "/" + f)));

            foreach (ProblemInstance p in testingInstances)
            {
                long individualTestingEvaluations = p.getMaxEvalsPerInstance();
                testing.Add(new ObjectiveFn(p, ObjectiveFn.TimingMode.TESTING, individualTestingEvaluations));
            }
        }

        ///////////////////////////////

        public TrainingCategory getTrainingCategory() { return trainingCategory; }
        public List<ObjectiveFn> getTrainingInstances() { return training; }
        public List<ObjectiveFn> getTestingInstances() { return testing; }

        ///////////////////////////////	

        public override string ToString()
        {
            //(Java) return ToStringBuilder.reflectionToString(this, ToStringStyle.MULTI_LINE_STYLE);
			//return (new JavaScriptSerializer().Serialize(this));

            StringBuilder sb = new StringBuilder();
		    
			foreach (System.Reflection.FieldInfo field in this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
			{
                sb.Append(field.Name);
                sb.Append(": ");

				var value = field.GetValue (this);
				if (value is List<ObjectiveFn>) {
					sb.Append("["+string.Join(", ", (List<ObjectiveFn>)value)+"]");
				} else {
					sb.Append(field.GetValue(this));
				}

                sb.Append(Environment.NewLine);
            }

            return sb.ToString();
        }
    }
}
