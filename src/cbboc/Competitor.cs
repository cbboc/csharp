using System.Collections.Generic;

namespace cbboc
{
    public abstract class Competitor
    {
        private readonly TrainingCategory trainingCategory;

        ///////////////////////////////	

        public Competitor(TrainingCategory trainingCategory)
        {
            this.trainingCategory = trainingCategory;
        }

        ///////////////////////////////

        //(Java) final
        public TrainingCategory getTrainingCategory() { return trainingCategory; }

        public abstract void train(List<ObjectiveFn> trainingSet, long maxTimeInMilliseconds);

        public abstract void test(ObjectiveFn testCase, long maxTimeInMilliseconds);
    }
}
