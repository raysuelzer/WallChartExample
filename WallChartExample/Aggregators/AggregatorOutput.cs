using System;
using System.Collections.Generic;
using System.Linq;

namespace WallChartExample.Aggregators
{
    public class AggregatorOutput: IAggregatorOutput
    {
   
        /**
         * Could be assessment id, tag name, tag id, city, state
         * etc
        */
        public string GroupingValue { get; set; }
        public IEnumerable<int> EntityIds { get; set; }

        public AggregatorOutput()
        {
        }

        public AggregatorOutput(string groupingValue, IEnumerable<int> entityIds)
        {
            this.GroupingValue = groupingValue;
            this.EntityIds = entityIds;
        }



        /**
         * Returns new output with the entity ids narrowed to the
         * to only those contained on the input list
         */
        public IAggregatorOutput ApplyEntityIdIntersection(IEnumerable<int> entityIdsSuperSet)
        {
            return new AggregatorOutput(this.GroupingValue, this.EntityIds.Where(entityIdsSuperSet.Contains));
        }
    }
}