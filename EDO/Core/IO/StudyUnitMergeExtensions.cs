using EDO.Core.Model;
using EDO.Core.Util;
using System.Collections.Generic;

namespace EDO.Core.IO
{
    static class StudyUnitMergeExtensions
    {
        public static void MergeEvent(this StudyUnit curStudyUnit, StudyUnit newStudyUnit)
        {
            curStudyUnit.Events.AddRange(newStudyUnit.Events);
        }

        public static void MergeMember(this StudyUnit curStudyUnit, StudyUnit newStudyUnit)
        {
            curStudyUnit.Members.AddRange(newStudyUnit.Members);
            foreach (Organization organization in newStudyUnit.Organizations)
            {
                List<string> existNames = Organization.GetOrganizationNames(curStudyUnit.Organizations);
                organization.OrganizationName = EDOUtils.UniqueLabel(existNames, organization.OrganizationName);
                curStudyUnit.Organizations.Add(organization);
            }
        }

        public static void MergeAbstract(this StudyUnit curStudyUnit, StudyUnit newStudyUnit)
        {
            curStudyUnit.Abstract = newStudyUnit.Abstract;
        }

        public static void MergeCoverage(this StudyUnit curStudyUnit, StudyUnit newStudyUnit)
        {
            //Merge of the study range is different for each part
            Coverage newCoverage = newStudyUnit.Coverage;
            Coverage curCoverage = curStudyUnit.Coverage;

            //1. Topical Coverage
            CheckOption.Merge(newCoverage.Topics, curCoverage.Topics);

            //2. Keyword
            curCoverage.Keywords.AddRange(newCoverage.Keywords);

            //3. Temporal Coverage
            curCoverage.DateRange = newCoverage.DateRange;

            //4. Geographic Levels Covered
            CheckOption.Merge(newCoverage.Areas, curCoverage.Areas);

            //5. Description
            curCoverage.Memo = newCoverage.Memo;
        }

        public static void MergeFundingInfo(this StudyUnit curStudyUnit, StudyUnit newStudyUnit)
        {
            curStudyUnit.FundingInfos.AddRange(newStudyUnit.FundingInfos);
        }

        public static void MergeSampling(this StudyUnit curStudyUnit, StudyUnit newStudyUnit)
        {
            curStudyUnit.Samplings.AddRange(newStudyUnit.Samplings);
        }

        public static void MergeConcept(this StudyUnit curStudyUnit, StudyUnit newStudyUnit)
        {
            curStudyUnit.ConceptSchemes.AddRange(newStudyUnit.ConceptSchemes);
        }

        public static void MergeQuestion(this StudyUnit curStudyUnit, StudyUnit newStudyUnit)
        {
            curStudyUnit.Questions.AddRange(newStudyUnit.Questions);
        }

        public static void MergeCategory(this StudyUnit curStudyUnit, StudyUnit newStudyUnit)
        {
            curStudyUnit.CategorySchemes.AddRange(newStudyUnit.CategorySchemes);
        }

        public static void MergeCode(this StudyUnit curStudyUnit, StudyUnit newStudyUnit)
        {
            curStudyUnit.CodeSchemes.AddRange(newStudyUnit.CodeSchemes);
        }

        public static void MergeSequence(this StudyUnit curStudyUnit, StudyUnit newStudyUnit)
        {
            curStudyUnit.ControlConstructSchemes.AddRange(newStudyUnit.ControlConstructSchemes);
        }

        public static void MergeQuestionGroup(this StudyUnit curStudyUnit, StudyUnit newStudyUnit)
        {
            curStudyUnit.QuestionGroups.AddRange(newStudyUnit.QuestionGroups);
        }

        public static void MergeVariable(this StudyUnit curStudyUnit, StudyUnit newStudyUnit)
        {
            curStudyUnit.Variables.AddRange(newStudyUnit.Variables);
        }

        public static void MergeDataSet(this StudyUnit curStudyUnit, StudyUnit newStudyUnit)
        {
            if (newStudyUnit.DataSets.Count > 0 && newStudyUnit.DataSets[0].Title == EDOConstants.LABEL_ALL)
            {
                newStudyUnit.DataSets.RemoveAt(0);
            }
            curStudyUnit.DataSets.AddRange(newStudyUnit.DataSets);
        }

        public static void MergeDataFile(this StudyUnit curStudyUnit, StudyUnit newStudyUnit)
        {
            int upper = newStudyUnit.DataFiles.Count - 1;
            for (int i = upper; i >= 0; i--)
            {
                DataFile dataFile = newStudyUnit.DataFiles[i];
                if (newStudyUnit.FindDataSet(dataFile.DataSetId) == null)
                {
                    newStudyUnit.DataFiles.RemoveAt(i);
                }
            }
            curStudyUnit.DataFiles.AddRange(newStudyUnit.DataFiles);
        }

        public static void MergeBook(this StudyUnit curStudyUnit, StudyUnit newStudyUnit)
        {
            curStudyUnit.Books.AddRange(newStudyUnit.Books);
        }

        public static void MergeStatisticsInfos(this StudyUnit curStudyUnit, StudyUnit newStudyUnit)
        {
            curStudyUnit.StatisticsInfos.AddRange(newStudyUnit.StatisticsInfos);
        }
    }
}
