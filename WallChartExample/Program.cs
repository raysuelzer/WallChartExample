using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using WallChartExample.Aggregators;

namespace WallChartExample
{
    class Program
    {
        static void Main(string[] args)
        {
            // Mock assuming assessment level is set to 4 on our org
            var assessmentLevels = 4;

            // Mock some query that returns entity ids 1 through 200.
            var mockEntityIdQueryResults = Enumerable.Range(1, 200).ToArray();

            // Build agg composer service, with the query results
            var aggComposer = new AggregatorComposer(mockEntityIdQueryResults);

            // Make an assessment aggregator to split all entities into their most recent 
            // assessment level. This is always needed
            var assessmentAggregator = new AssessmentAggregator(new AssessmentQueryParamsInput()
            {
                CampaignId = "1",
            }, assessmentLevels);

            // Make an tag aggregator
            var tagAggregator = new TagAggregator(
                new TagAggregatorQueryParams
                {
                    CampaignId = "1",
                    TagCatgegoryId = "3"
                });

            // Make an EntityConnection aggregator
            var entityConnectionAggregator = new EntityConnectionAggregator(new EntityConnectionAggQueryParams()
            {
                CampaignId = "1",
                EntityType = "1"
            });

            // Put the two aggregators together in a list, to be grouped in order
            var aggregators = new List<IAggregator>() { tagAggregator, entityConnectionAggregator } ;

            // Run the aggregators
            // This will return the aggregated result
            var aggResult = aggComposer.RunAggregators(
                assessmentAggregator, 
                aggregators
                );

            // Write results to console as json
            Console.WriteLine(JsonSerializer.Serialize(aggResult, new JsonSerializerOptions {WriteIndented = true }));

            Console.ReadKey();
        }
    }
}
