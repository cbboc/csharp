﻿using System;

namespace cbboc
{
    public sealed class ObjectiveFn
    {
        public readonly ProblemInstance instance;
        public readonly TimingMode timingMode;
        public long remainingEvaluations;

        private Tuple<long, double> remainingEvaluationsAtBestValue = null; // Pair.of( -1L, Double.NaN );

        ///////////////////////////////

        public enum TimingMode
        {
            TRAINING, TESTING
        };

        public ObjectiveFn(ProblemInstance instance, TimingMode timingMode, long remainingEvaluations)
        {

            this.instance = instance;
            this.timingMode = timingMode;
            // this.remainingEvaluations = instance.getMaxEvalsPerInstance();
            this.remainingEvaluations = remainingEvaluations;
        }

        ///////////////////////////////

        public double value(bool[] candidate)
        {

            //(Java) readonly 
            long trainTimeNow = CBBOC2016.trainTime.ElapsedMilliseconds;
            long testTimeNow = CBBOC2016.testTime.ElapsedMilliseconds;

            if (timingMode == TimingMode.TRAINING)
            {
                if (trainTimeNow > CBBOC2016.trainingEndTime)
                    throw new CBBOC2016.TimeExceededException();
            }
            else if (timingMode == TimingMode.TESTING)
            {
                if (testTimeNow > CBBOC2016.testingEndTime)
                    throw new CBBOC2016.TimeExceededException();
            }
            else
            {
                throw new InvalidOperationException(); //(Java) IllegalStateException();
            }

            ///////////////////////////

            if (remainingEvaluations <= 0)
            {
                throw new CBBOC2016.EvaluationsExceededException();
            }
            else
            {
                //(Java) readonly
                double value = instance.value(candidate);
                //(Java) remainingEvaluations.setValue(remainingEvaluations.getValue() - 1);
                remainingEvaluations--;

                // We are maximizing...
                if (remainingEvaluationsAtBestValue == null || value > remainingEvaluationsAtBestValue.Item2)
                    remainingEvaluationsAtBestValue = Tuple.Create(getRemainingEvaluations(), value);

                return value;
            }
        }

        ///////////////////////////////

        public Tuple<long, double> getRemainingEvaluationsAtBestValue()
        {
            if (remainingEvaluationsAtBestValue == null)
                return Tuple.Create(-1L, -1.0);
            else
                return remainingEvaluationsAtBestValue;
        }

        public int getNumGenes() { return instance.getNumGenes(); }
        public long getRemainingEvaluations() { return remainingEvaluations; }
        public long getMaxEvalsPerInstance() { return instance.getMaxEvalsPerInstance(); }

        ///////////////////////////////	
        
        public override string ToString()
        {
            // return ToStringBuilder.reflectionToString( this );
            string result = "ObjectiveFn(numGenes:" + getNumGenes();
            result += ",remainingEvaluations: " + getRemainingEvaluations();
            if (remainingEvaluationsAtBestValue == null)
            {
                result += ",remainingEvaluationsAtBestValue: -1";
                result += ",bestValue: -1.0";
            }
            // if( remainingEvaluationsAtBestValue != null ) 
            else {
                result += ",remainingEvaluationsAtBestValue: " + remainingEvaluationsAtBestValue.Item1;
                result += ",bestValue: " + remainingEvaluationsAtBestValue.Item2;
            }
            result += ",timingMode: " + timingMode + ")";
            return result;
        }
    }
}
