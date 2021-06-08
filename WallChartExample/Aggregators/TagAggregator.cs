using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WallChartExample.Aggregators
{
    public class TagAggregatorQueryParams
    {
        public string CampaignId { get; set; }
        public string TagCatgegoryId { get; set; }
    }

    public class TagQuerySQLItem
    {
        public string CampaignId { get; set; }
        public string TagCategoryId { get; set; }
        public string TagId { get; set; }
        public long[] EntityIds { get; set; }

    }
    public class TagAggregator : IAggregator
    {
        private readonly SQLReader sqlReader;

        private readonly TagAggregatorQueryParams _queryParams;

        private long[] SupersetIds { get; }

        public TagAggregator(
            SQLReader sqlReader,
            long[] entityIds,
            TagAggregatorQueryParams queryParams)
        {
            this.SupersetIds = entityIds;
            this.sqlReader = sqlReader;
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
            var sqlResults = this.sqlReader.RunQuery<TagQuerySQLItem>(sql, ToSQLModel);

            return sqlResults.ToList().Select(sr =>
            {
                return new AggregatorOutput { EntityIds = sr.EntityIds, GroupingValue = sr.TagId };
            });            
        }

        public TagQuerySQLItem ToSQLModel(NpgsqlDataReader reader)
        {
            return new TagQuerySQLItem
            {
                CampaignId = reader.GetFieldValue<long>(0).ToString(),
                TagCategoryId = reader.GetFieldValue<int>(1).ToString(),
                TagId = reader.GetFieldValue<int>(2).ToString(),
                EntityIds = reader.GetFieldValue<long[]>(3)
            };
        }
    }
}
