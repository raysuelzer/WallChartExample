using System.Collections.Generic;

namespace WallChartExample.Aggregators
{
    public interface IAggregatorOutput
    {
        /**
         * Could be assessment id, tag name, tag id, city, state
         * etc
         */
        public string GroupingValue { get; set; }
        public IEnumerable<long> EntityIds { get; set; }

        public IAggregatorOutput ApplyEntityIdIntersection(IEnumerable<long> entityIdsSuperSet);
    }
}