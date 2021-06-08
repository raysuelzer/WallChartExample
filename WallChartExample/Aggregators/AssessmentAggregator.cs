using Npgsql;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;

namespace WallChartExample.Aggregators
{
    public class AssessmentQueryParamsInput
    {
        public string CampaignId { get; set; }
    }

    public class AssessmentQuerySQLItem
    {
        public string LatestAssessmentLevel { get; set; }
        public string CampaignId { get; set; }
        public long[] EntityIds { get; set; }
    }

    public class AssessmentAggregator: IAggregator
    {
        private readonly SQLReader sqlReader;
        private readonly AssessmentQueryParamsInput _queryParams;
        private readonly List<string> levels;

        public AssessmentAggregator(
            SQLReader sqlReader,
            AssessmentQueryParamsInput queryParams, 
            int numberOfLevels)
        {
            this.sqlReader = sqlReader;
            // Params used on SQL Query
            _queryParams = queryParams;

            // Used to make a list of the possible assessment levels,
            // Used later to fill in 0 for where there was no level for a certain assessment
            // return from the SQL query
            levels = Enumerable.Range(0, numberOfLevels).ToList<int>().Select(i => i.ToString()).ToList();
            
        }
        public IEnumerable<IAggregatorOutput> FetchResults()
        {
            // EXECUTE SQL QUERY
            // This should be fast, as it returns a columnar format
            // and has few to no filters. We will filter this further
            // in the Aggregator Composer
            var sql = $@"
            SELECT  ce.campaign_id, 
                    ce.latest_assessment_level,                   
                   array_agg(ce.entity_id) AS entity_ids
                FROM campaigns_entities ce
                WHERE campaign_id = {_queryParams.CampaignId}
                GROUP BY ce.campaign_id,  ce.latest_assessment_level";


            var sqlResults = sqlReader.RunQuery<AssessmentQuerySQLItem>(sql, ToSQLModel);

            var aggOutputs = sqlResults.ToList().Select(r =>
            {
               return new AggregatorOutput
               {
                   GroupingValue = r.LatestAssessmentLevel,
                   EntityIds = r.EntityIds
               };
            });
            // I am hard coding sample results
            // GroupingValue here is latest_assessment_level

            // Here is some mock output
          
            // Ensure all levels have a result, this is needed to fill out any assessments which had no entityIds
            var resultWithMissingLevels = this.levels.Select(level =>
            {
                var aggOutput = aggOutputs.FirstOrDefault(t => t.GroupingValue == level);
                if (aggOutput != null)
                {
                    return aggOutput;
                }

                // No result, create an empty output for the level
                return new AggregatorOutput(level, new List<long>());
            });

            // Order it by assessment level (will be easier later)
            return resultWithMissingLevels.OrderBy(t => t.GroupingValue);
        }

        public AssessmentQuerySQLItem ToSQLModel(NpgsqlDataReader reader)
        {
            return new AssessmentQuerySQLItem
            {

                CampaignId = reader.GetFieldValue<long>(0).ToString(),
                LatestAssessmentLevel = reader.GetFieldValue<int>(1).ToString(),                
                EntityIds = reader.GetFieldValue<long[]>(2)

                //CampaignId = reader.GetFieldValue<long>(reader.GetOrdinal("campaign_id")).ToString(),
                //LatestAssessmentLevel = reader.GetFieldValue<int>(reader.GetOrdinal("latest_assessment_level")).ToString(),
                //EntityIds = reader.GetFieldValue<long[]>(reader.GetOrdinal("entity_ids"))
            };
        }
    }
}
