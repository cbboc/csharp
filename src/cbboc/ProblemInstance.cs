using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace cbboc
{
    public sealed class ProblemInstance
    {
        private int numGenes;
        private int maxEvalsPerInstance;
        private int K;

		// Change for 2016:
		// Instead of each file having N+1 rows (header and N functions), 
		// where N is the number of problem variables, each now has M+1 rows, 
		// with the value of M added to the end of the header.
		private int M;

        private List<Tuple<int[], double[]>> data = new List<Tuple<int[], double[]>>();

        ///////////////////////////////

        public ProblemInstance(StreamReader sr) //(Java) throws IOException
        {
            try
            {
                string line = sr.ReadLine();

                int[] instanceInfo = line.Split(null).Select(n => Convert.ToInt32(n)).ToArray();
                numGenes = instanceInfo[0];
                maxEvalsPerInstance = instanceInfo[1];
                K = instanceInfo[2];
				M = instanceInfo[3];

				int numRows = M;

                for (int i = 0; i < numRows; ++i)
                {
                    line = sr.ReadLine();
                    string[] tokens = line.Split(null);

                    int[] iarray = tokens.Take(K).Select(n => Convert.ToInt32(n)).ToArray();
                    double[] darray = tokens.Skip(K).Select(n => Convert.ToDouble(n)).ToArray();

                    data.Add(Tuple.Create(iarray, darray));
                }
            }
            finally
            {
                if (sr != null)
                    sr.Close();
            }


            Debug.Assert(invariant());
        }

        ///////////////////////////////

        public int getNumGenes() { return numGenes; }
        public int getMaxEvalsPerInstance() { return maxEvalsPerInstance; }

        ///////////////////////////////

        public double value(bool[] candidate)
        {
            if (candidate.Length != getNumGenes())
                throw new ArgumentException("candidate of length " + getNumGenes() + " expected, found " + candidate.Length);

            double total = 0.0;
            for (int i = 0; i < getNumGenes(); ++i)
            {
                int[] varIndices = data[i].Item1;
                int fnTableIndex = 0;
                for (int j = 0; j < varIndices.Length; ++j)
                {
                    fnTableIndex <<= 1;
                    fnTableIndex |= candidate[varIndices[j]] ? 1 : 0;
                }

                total += data[i].Item2[fnTableIndex];
            }

            return total;
        }

        ///////////////////////////////	

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("ProblemInstance(");
            result.Append("numGenes=" + numGenes);
            result.Append(",maxEvalsPerInstance=" + maxEvalsPerInstance);
            result.Append(")");
            return result.ToString();
        }

        ///////////////////////////////	

        public string debugToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("ProblemInstance[");
            result.Append("numGenes=" + numGenes);
            result.Append(",maxEvalsPerInstance=" + maxEvalsPerInstance);
            result.Append(",K=" + K);

            result.Append(",data=[\n");
            for (int i = 0; i < data.Count; ++i)
            {
                result.Append("(" + data[i].Item1.ToString());
                result.Append("," + data[i].Item2.ToString());
                result.Append(")\n");
            }
            result.Append("]]");
            return result.ToString();
        }

        ///////////////////////////////

        private static bool allValidSize(List<Tuple<int[], double[]>> data, int k)
        {
            foreach (Tuple<int[], double[]> p in data)
                if (p.Item1.Length != k + 1 || p.Item2.Length != 1 << (k + 1))
                    return false;

            return true;
        }

        public bool invariant()
        {
            return getNumGenes() > 0 &&
                getMaxEvalsPerInstance() > 0 &&
                K > 0 &&
                data.Count == getNumGenes() &&
                allValidSize(data, K);
        }

        ///////////////////////////////	

        /*public static void Main(String[] args) //(Java) throws IOException
        {
            string root = Directory.GetCurrentDirectory(); //(Java) System.getProperty("user.dir");
            // String path = root + "/resources/" + "00000.txt";
            string path = root + "/resources/" + "toy.txt";

            StreamReader sr = File.OpenText(path);

            ProblemInstance prob = new ProblemInstance(sr);
            Console.WriteLine(prob);

            bool[] candidate = new bool[prob.getNumGenes()];
            Console.WriteLine(prob.value(candidate));

            Console.WriteLine("All done");
        }*/
    }
}
