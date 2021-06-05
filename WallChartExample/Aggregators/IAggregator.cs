using System;
using System.Collections.Generic;
using System.Text;

namespace WallChartExample.Aggregators
{
    public interface IAggregator
    {
        public IEnumerable<IAggregatorOutput> FetchResults();
    }
}