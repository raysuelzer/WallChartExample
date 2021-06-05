using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace WallChartExample.Models
{
    public class AggregationResultType
    {
        public string Label { get; set; }
        public IEnumerable<AggregationMatchedType> Matches { get; set; }
        public IEnumerable<AggregationResultType> GroupBy { get; set; }
    }
}
