using System;
using System.Collections.Generic;
using System.Text;

namespace WallChartExample.Aggregators
{
    public class EntityConnectionAggQueryParams
    {
        public string CampaignId { get; set; }
        public string EntityType { get; set; }
    }


    // SEE ASSESSMENT AGGREGATOR FOR COMMENTS
    public class EntityConnectionAggregator : IAggregator
    {

        private readonly EntityConnectionAggQueryParams _queryParams;

        public EntityConnectionAggregator(EntityConnectionAggQueryParams queryParams)
        {
            this._queryParams = queryParams;
        }

        public IEnumerable<IAggregatorOutput> FetchResults()
        {
            // EXECUTE SQL QUERY
            // NOte we would also filter on entity_type_id at this point (see tags aggregator)
           var sql = @"
            --ENTITY_CONNECTION_1--
                (SELECT ec.campaign_id, ec.from_entity_id AS connected_to_entity_id, e.entity_type_id, array_agg(ec.to_entity_id) AS entity_ids
            FROM entity_connections ec
                JOIN entities e ON e.id = ec.from_entity_id
            WHERE ec.status = 1
            GROUP BY ec.campaign_id, ec.from_entity_id, e.entity_type_id)
            UNION
                -- INVERSE AND UNION--
                (SELECT ec.campaign_id, ec.to_entity_id AS connected_to_entity_id, e.entity_type_id, array_agg(ec.from_entity_id) AS entity_ids
            FROM entity_connections ec
                JOIN entities e ON e.id = ec.to_entity_id
            WHERE ec.status = 1
            GROUP BY ec.campaign_id, ec.to_entity_id, e.entity_type_id)
            ";


            // Mock the return data
            // Connected_to_entity_id ID is the grouping value (NOTE: Could sub with entity name here)
            // This data is totally made up, the actual results from this query are correct
            return new List<IAggregatorOutput>()
            {
                new AggregatorOutput
                {
                    GroupingValue = "9",
                    EntityIds = new List<long> {9, 11, 12, 22, 25, 27, 28, 29, 30, 31, 33, 3}
                },
                new AggregatorOutput
                {
                    GroupingValue = "8",
                    EntityIds = new List<long> {6, 2, 3}
                }
            };
        }
    }
}
