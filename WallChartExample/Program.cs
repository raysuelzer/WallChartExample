using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using WallChartExample.Aggregators;

namespace WallChartExample
{
    class Program
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Ready, press any key run");


            var cs = "PG_CONNECTION_STRING_HERE";

            var sqlReader = new SQLReader(cs, "dflcaucus");

            // Mock assuming assessment level is set to 4 on our org
            var assessmentLevels = 4;

            var campaignId = "2";

            var stopwatch = Stopwatch.StartNew();

            // Get a list of all the entity ids for testing purposes.
            var sql = $@"select array_agg(entity_id) AS entity_ids from campaigns_entities where campaign_id = {campaignId}";
            var result = sqlReader.RunQuery(sql, (r) =>
            {
                return r.GetFieldValue<long[]>(0);
            });
            var entityIds = result.First();

            Console.Write($"{entityIds.LongLength} entities retrieved");


            // Build agg composer service, with the query results
            var aggComposer = new AggregatorComposer(entityIds);

            // Make an assessment aggregator to split all entities into their most recent 
            // assessment level. This is always needed
            var assessmentAggregator = new AssessmentAggregator(sqlReader,
                new AssessmentQueryParamsInput { CampaignId = campaignId },
                assessmentLevels);

            // Make an tag aggregator
            var tagAggregator = new TagAggregator(
                sqlReader,
                entityIds,
                new TagAggregatorQueryParams
                {
                    CampaignId = campaignId,
                    TagCatgegoryId = "14"
                });


            var tagAggregator2 =
                new TagAggregator(
                    sqlReader, entityIds, 
                    new TagAggregatorQueryParams
                    {
                        CampaignId = campaignId,
                        TagCatgegoryId = "18"
                    }
               );
            // Make an EntityConnection aggregator
            //var entityConnectionAggregator = new EntityConnectionAggregator(new EntityConnectionAggQueryParams()
            //{
            //    CampaignId = campaignId,
            //    EntityType = "1"
            //});

            // Put the two aggregators together in a list, to be grouped in order
            var aggregators = new List<IAggregator>() { tagAggregator };

            // Run the aggregators
            // This will return the aggregated result
            var aggResult = aggComposer.RunAggregators(
                assessmentAggregator,
                aggregators
                );


            // Write results to console as json
            Console.WriteLine($"Done in {stopwatch.ElapsedMilliseconds}");
            //File.WriteAllText(@"C:\Users\rsuel\Desktop\test.json", JsonSerializer.Serialize(aggResult, new JsonSerializerOptions { WriteIndented = true }));


            //if (args.Count() == 0)
            //{
            //    Console.WriteLine("Starting Second Run");
            //    Main(new List<string> { "done" }.ToArray());
            //}
            //else
            //{
            //    Console.ReadKey();
            //}           

            Main(null);

        }
    }

}
