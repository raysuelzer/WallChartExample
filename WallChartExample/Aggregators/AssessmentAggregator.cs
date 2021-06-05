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

    public class AssessmentAggregator: IAggregator
    {
        private readonly AssessmentQueryParamsInput _queryParams;
        private readonly List<string> levels;

        public AssessmentAggregator(AssessmentQueryParamsInput queryParams, int numberOfLevels)
        {
            // Params used on SQL Query
            _queryParams = queryParams;

            // Used to make a list of the possible assessment levels,
            // Used later to fill in 0 for where there was no level for a certain assessment
            // return from the SQL query
            levels = NumOfLevelsToList(numberOfLevels);
        }
        public IEnumerable<IAggregatorOutput> FetchResults()
        {
            // EXECUTE SQL QUERY
            // This should be fast, as it returns a columnar format
            // and has few to no filters. We will filter this further
            // in the Aggregator Composer
            var sql = $@"
            SELECT ce.latest_assessment_level, 
                   ce.campaign_id, 
                   array_agg(ce.entity_id) AS entity_ids
                FROM campaigns_entities ce
                WHERE campaign_id = {_queryParams.CampaignId}
                GROUP BY ce.campaign_id,  ce.latest_assessment_level";

			// MOCK SQL EXECUTION
			// assume campaign_id passed was 513, IT RETURNS BACK:
            /*
            latest_assessment_level | campaign_id | entity_ids			
            0   513 { 9,11,12,13,21,24,26,57,103,106,111,118,144}          
            2   513 { 6,2,3,5,8,18,19,10,22,25,27,28,29,30,31,33,34}
            1   513 { 4,37,59,39,40,32,107,109,110,108,145}            
            */

            // We would loop through the results and cast to an IAggregatorOutput
            
            // I am hard coding sample results
            // GroupingValue here is latest_assessment_level

            // Here is some mock output
            var mockSqlResult = new List<IAggregatorOutput>()
            {
                new AggregatorOutput
                {
                    GroupingValue = "0",
                    EntityIds = Enumerable.Range(1, 29)
                },
                new AggregatorOutput
                {
                    GroupingValue = "2",
                    EntityIds = Enumerable.Range(30, 29)
                },
                new AggregatorOutput
                {
                    GroupingValue = "1", EntityIds = Enumerable.Range(60, 210)
                }
            };

            // Ensure all levels have a result, this is needed to fill out any assessments which had no entityIds
            var resultWithMissingLevels = this.levels.Select(level =>
            {
                var sqlResult = mockSqlResult.FirstOrDefault(t => t.GroupingValue == level);
                if (sqlResult != null)
                {
                    return sqlResult;
                }

                // No result, create an empty output for the level
                return new AggregatorOutput(level, new List<int>());
            });

            // Order it by assessment level (will be easier later)
            return resultWithMissingLevels.OrderBy(t => t.GroupingValue);
        }

        // Will make a string array from 0 to the number given
        private static List<string> NumOfLevelsToList(int numOfLevels)
        {
            var levels = new List<string>();
            for (int i = 0; i <= numOfLevels; i++)
            { // print numbers from 1 to 5
                levels.Add(i.ToString());
            }

            return levels;
        }
    }
}
