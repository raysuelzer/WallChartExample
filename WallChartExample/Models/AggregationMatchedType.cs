using System.Collections.Generic;
using System.Linq;
using WallChartExample.Aggregators;

namespace WallChartExample.Models
{
    public class AggregationMatchedType
    {
        public string AssessmentLevel { get; set; }
        public IEnumerable<long> EntityIds { get; set; }
    }
}