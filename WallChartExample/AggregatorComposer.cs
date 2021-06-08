using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using WallChartExample.Aggregators;
using WallChartExample.Models;

namespace WallChartExample
{
    public class AggregatorComposer
    {
        private readonly IEnumerable<long> _baseEntityQueryResults;

        /**
        *  NOTE: This class expects an input of the superset of entity ids
        *  in reality, we MAY want to handle the case where the superset of the results
        *  (i.e. the query results) are blank which would indicate to apply no filters.
        *
        *  It may be worthwhile to check if there is an performance boost by passing
        *  the entityId to IAggregator as a param on IAggregator.fetch
        *  to narrow the aggregator SQL query before it is run.
        *  This would be easy to do, but not sure of the net benefit on performance.
        *  It would likely help the most cases where the base entity query is small and the
        *  size of the campaign is huge. But it may make the SQL query take more time to run.
        */
        public AggregatorComposer(IEnumerable<long> baseEntityQueryResults)
        {
            _baseEntityQueryResults = baseEntityQueryResults;
        }
      
        public IList<AggregationResultType> RunAggregators(
            AssessmentAggregator assessmentAgg,
            IEnumerable<IAggregator> aggregators 
        )
        {
            // We will always need an assessment level aggregator
            var assessmentAggResults = assessmentAgg.FetchResults().ToList();

            // Execute all the aggs. This could be done in parallel. 
            // Additional optimizations for recursive filtering maybe potential here
            var aggResults = aggregators.Select(agg => agg.FetchResults().ToList());

            // NOTE: Using a linked list here, but really this could fixed to only two aggregators
            // or a more basic loop.  We only use two agg in real life, but this can be infinitely nested
            var ll = new LinkedList<List<IAggregatorOutput>>(aggResults);

            return CombineResults(
                assessmentAggResults,
                ll.First,
                this._baseEntityQueryResults.ToList()
            );
        }

        private List<AggregationResultType> CombineResults(
            IList<IAggregatorOutput> assessmentAggResults,
            LinkedListNode<List<IAggregatorOutput>> currentNode,
            IList<long> entityIdSuperset
            )
        {
            if (currentNode == null)
            {
                return null;
            }

            var currentNodeList = currentNode.Value?.ToList() ?? new List<IAggregatorOutput>();

            // The first thing we need to do is to add a aggregatorOutput
            // which contains any entityIds that are in the entityIdSuperset
            // but not in the outputs from the SQL command. (See function for more comments)
            currentNodeList.Add(
                MakeNoneGrouping(currentNodeList, entityIdSuperset)
                );

            // Will use this later for sub grouping
            var nextNode = currentNode.Next;

            // Loop through the first set of results from the SQL Query
            var result = currentNodeList.Select(item =>
            {
                // Only the entityIds in the superset and the item
                var intersectedEntityIds = item.EntityIds.Intersect(entityIdSuperset).ToArray();


                // AggregationResult is the ultimate return type of the API
                // served by GraphQL
                var aggResult = new AggregationResultType()
                {
                    // NOTE: There will need to be extra code here (or elsewhere) to get a human friendly value for a label.
                    // This may be the tag name or entity name this code implementation, right now it's always going to be an id. 
                    Label = item.GroupingValue,
                    Matches = SplitIntoAssessmentLevels(assessmentAggResults, intersectedEntityIds),

                    // If there is a second level that was passed, recursively call back to this function to 
                    // generate the nested level. While filtering the second level EntityIds to only those also
                    // in the first level
                    GroupBy = nextNode == null ? null : CombineResults(
                            assessmentAggResults,
                            nextNode,
                            intersectedEntityIds
                            )
                        .ToList()

                };

                return aggResult;
            }).ToList();


            return result?.ToList();
        }

        // We need to add a "NONE" grouping result
        // This will include any entityIds in the superset that were not in the subset
        private IAggregatorOutput MakeNoneGrouping(
            IList<IAggregatorOutput> aggregatorOutputs,
            IList<long> entityIdSuperset)
        {
            // This is a flatMap uniq function
            // It combines all the entityIds from the returned SQL Counts
            var matchedEntityIds = aggregatorOutputs.SelectMany(ao => ao.EntityIds).Distinct().ToList();

            // Then we need to get the entityIds from our base input (maybe it's an entire campaign)
            // and put any of these Ids that didn't have a result in the aggreagtor into a "none" or "missing" 
            // group.  For example, if our aggregator was for "city" this would include entityIds in the input
            // that did not have a city. 

            var entityIdsNotInSubset = entityIdSuperset.Where(id => !matchedEntityIds.Contains(id)).ToList();

            return new AggregatorOutput()
            {
                GroupingValue = "NONE",
                EntityIds = entityIdsNotInSubset
            };

            
        }

        private static IList<AggregationMatchedType> SplitIntoAssessmentLevels(
            IList<IAggregatorOutput> assessmentAggResults,
            IEnumerable<long> entityIdsSubset)
        {           

            var result = assessmentAggResults.Select(agr =>
            {
                return new AggregationMatchedType
                {
                    // the assessment level is the grouping value
                    AssessmentLevel = agr.GroupingValue,
                    // Filter those entityIds to those in the subset provided
                    EntityIds = agr.EntityIds.Intersect(entityIdsSubset)
                       
                };
            }).ToList();
            return result;
        }
    }
}
