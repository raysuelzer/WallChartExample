using System;
using System.Collections.Generic;
using System.Text;

namespace WallChartExample.Aggregators
{
    public class TagAggregatorQueryParams
    {
        public string CampaignId { get; set; }
        public string TagCatgegoryId { get; set; }
    }
    public class TagAggregator : IAggregator
    {
        private readonly TagAggregatorQueryParams _queryParams;

        public TagAggregator(TagAggregatorQueryParams queryParams)
        {
            this._queryParams = queryParams;
        }

        // SEE ASSESSMENT AGGREGATOR FOR COMMENTS
        public IEnumerable<IAggregatorOutput> FetchResults()
        {
            // EXECUTE SQL QUERY
            var sql = $@"
            --ENTITY TAGS-- -
                SELECT tl.campaign_id, t.tag_category_id, tl.tag_id, array_agg(tl.taggable_id) AS entity_ids
            FROM taggable_logbook tl
                JOIN tags t ON tl.tag_id = t.id
            WHERE tl.available = true
                -- FURTHER FILTER TO TAG CATEGORY AND CAMPAIGN ID TO SPEED UP--
                AND t.tag_category_id = {_queryParams.TagCatgegoryId}
                AND tl.campaign_id = {_queryParams.CampaignId}
            AND tl.taggable_type = 'Entity'
            GROUP BY tl.campaign_id, t.tag_category_id, tl.tag_id
            ";

            // Mock the return data
            // Tag ID is the grouping value (NOTE: Could sub with tag name here)

            return new List<IAggregatorOutput>()
            {
                new AggregatorOutput
                {
                    GroupingValue = "39",
                    EntityIds = new List<int> {9, 11, 12, 22, 25, 27, 28, 29, 30, 31, 33, 3}
                },
                new AggregatorOutput
                {
                    GroupingValue = "40",
                    EntityIds = new List<int> {6, 2, 3, 5, 8, 18, 19, 10, 4, 9}
                }
            };
        }
    }
}
