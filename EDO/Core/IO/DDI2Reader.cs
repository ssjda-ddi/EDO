using System.Collections.Generic;
using System.Linq;
using EDO.Core.Model;
using System.Xml;
using EDO.Properties;
using System.Diagnostics;
using System.Xml.Linq;
using EDO.Core.Util;
using EDO.Core.Model.Statistics;
using EDO.Core.Util.Statistics;

namespace EDO.Core.IO
{
    public class DDI2Reader :DDI2IO, IDDIReader
    {
        private class ReaderContext
        {
            public ReaderContext()
            {
                SamplingMethods = new Dictionary<string, List<string>>();
                VarIds = new Dictionary<string, List<string>>();
                DataSetIds = new Dictionary<string, List<string>>();
            }

            // Tha pair og Sampling Id and Sampling Procedure(Multiple Sampling Procedure exist in each tab)
            public Dictionary<string, List<string>> SamplingMethods { get; set; }

            // The pair of conceptId and variableId(Multiple variables exist in each concept)
            public Dictionary<string, List<string>> VarIds {get; set; }

            public Dictionary<string, List<string>> DataSetIds { get; set; }


            public string FindConceptIdByVarId(string varId)
            {
                //Determine the Concept that belongs to variable
                foreach (KeyValuePair<string, List<string>> pair in VarIds)
                {
                    string conceptId = pair.Key;
                    List<string> ids = pair.Value;
                    if (ids.Contains(varId))
                    {
                        return conceptId;
                    }
                }
                return null;
            }
        }

        private class CategorySchemeItem
        {
            public CategorySchemeItem(CategoryScheme categoryScheme, CodeScheme codeScheme)
            {
                CategoryScheme = categoryScheme;
                CodeScheme = codeScheme;
            }
            public CategoryScheme CategoryScheme { get; set; }
            public CodeScheme CodeScheme { get; set; }
        }

        private class VariableItem
        {
            public VariableItem(Variable variable, Question question)
            {
                Variable = variable;
                Question = question;
            }
            public Variable Variable { get; set; }
            public Question Question { get; set; }
            public CategorySchemeItem CategorySchemeItem { get; set; }
            public StatisticsInfo StatisticsInfo { get; set; }
        }

        private class DataSetItem
        {
            public DataSetItem(DataSet dataSet, DataFile dataFile)
            {
                DataSet = dataSet;
                DataFile = dataFile;
            }
            public DataSet DataSet { get; set; }
            public DataFile DataFile { get; set; }
        }

        private static Member CreateMember(XElement memberElem, string roleName)
        {
            Member member = new Member();
            SetupMemberName(memberElem.Value, member);
            if (roleName == null)
            {
                string role = (string)memberElem.Attribute(ATTR_ROLE);
                roleName = role;
            }
            member.RoleName = roleName;
            return member;
        }

        private static Organization CreateOrganization(XElement elem, Member member, List<Organization> organizations)
        {
            string organizationName = (string)elem.Attribute(ATTR_AFFILIATION);
            if (string.IsNullOrEmpty(organizationName))
            {
                organizationName = Resources.UndefinedValue;
            }
            Organization organization = Organization.FindOrganizationByName(organizations, organizationName);
            if (organization == null)
            {
                organization = new Organization() { OrganizationName = organizationName };
                organizations.Add(organization);
            }
            if (member != null)
            {
                member.OrganizationId = organization.Id;
            }
            return organization;
        }

        private static void CreateMembers(XElement codebookElem, StudyUnit studyUnit)
        {
            List<Member> members = new List<Member>();
            XElement stdyDscrElem = codebookElem.Element(cb + TAG_STDY_DSCR);
            if (stdyDscrElem == null)
            {
                return;
            }
            XElement citationElem = stdyDscrElem.Element(cb + TAG_CITATION);
            if (citationElem == null)
            {
                return;
            }
            XElement rspStmtElem = citationElem.Element(cb + TAG_RSP_STMT);
            if (rspStmtElem == null)
            {
                return;
            }

            List<Organization> organizations = new List<Organization>();
            IEnumerable<XElement> authEntyElems = rspStmtElem.Elements(cb + TAG_AUTH_ENTY);
            foreach (XElement authEntyElem in authEntyElems)
            {
                Member member = CreateMember(authEntyElem, Member.FindRoleNameDaihyoSha());
                members.Add(member);
                CreateOrganization(authEntyElem, member, organizations);
            }
            IEnumerable<XElement> othIdElems = rspStmtElem.Elements(cb + TAG_OTH_ID);
            foreach (XElement othIdElem in othIdElems)
            {
                Member member = CreateMember(othIdElem, null);
                members.Add(member);
                CreateOrganization(othIdElem, member, organizations);
            }

            if (members.Count > 0)
            {
                studyUnit.Members = members;
            }

            if (organizations.Count > 0)
            {
                studyUnit.Organizations = organizations;
            }
        }

        private static void CreateAbstract(XElement codebookElem, StudyUnit studyUnit)
        {
            XElement stdyDscrElem = codebookElem.Element(cb + TAG_STDY_DSCR);
            if (stdyDscrElem == null)
            {
                return;
            }

            Abstract abstractModel = new Abstract();
            XElement citationElem = stdyDscrElem.Element(cb + TAG_CITATION);
            if (citationElem != null)
            {
                XElement titlElem = citationElem.Descendants(cb + TAG_TITL).FirstOrDefault();
                if (titlElem != null)
                {
                    abstractModel.Title = titlElem.Value;
                }
            }

            XElement stdyInfoElem = stdyDscrElem.Element(cb + TAG_STDY_INFO);
            if (stdyInfoElem != null)
            {
                abstractModel.Summary = (string)stdyInfoElem.Element(cb + TAG_ABSTRACT);
            }

            studyUnit.Abstract = abstractModel;
        }


        private static DateRange ReadDateRange(XElement rootElem, XName dateElemName)
        {
            IEnumerable<XElement> timePrdElems = rootElem.Descendants(dateElemName);
            DateUnit fromDate = new DateUnit();
            DateUnit toDate = new DateUnit();
            foreach (XElement timePrdElem in timePrdElems)
            {
                string dateStr = (string)timePrdElem.Attribute(ATTR_DATE);
                string eventStr = (string)timePrdElem.Attribute(ATTR_EVENT);
                if (eventStr == null || eventStr == "start")
                {
                    fromDate = DateParser.Parse(dateStr);
                }
                else if (eventStr == "end")
                {
                    toDate = DateParser.Parse(dateStr);
                }
            }
            if (fromDate == null)
            {
                return null;
            }
            return new DateRange(fromDate, toDate);
        }

        private static void CreateCoverage(XElement codebookElem, StudyUnit studyUnit)
        {
            XElement stdyDscrElem = codebookElem.Element(cb + TAG_STDY_DSCR);
            if (stdyDscrElem == null)
            {
                return;
            }

            //Topical Coverage
            Coverage coverageModel = Coverage.CreateDefault();
            XElement stdyInfoElem = stdyDscrElem.Element(cb + TAG_STDY_INFO);
            if (stdyInfoElem != null)
            {
                XElement subjectElem = stdyInfoElem.Element(cb + TAG_SUBJECT);
                if (subjectElem != null)
                {
                    List<string> labels = new List<string>();
                    IEnumerable<XElement> topcClasElems = subjectElem.Elements(cb + TAG_TOPC_CLAS);
                    foreach (XElement topcClasElem in topcClasElems)
                    {
                        labels.Add(topcClasElem.Value);
                    }
                    coverageModel.CheckTopics(labels);

                    //Keyword
                    IEnumerable<XElement> keywordElems = subjectElem.Elements(cb + TAG_KEYWORD);
                    foreach (XElement keywordElem in keywordElems)
                    {
                        Keyword keyword = new Keyword()
                        {
                            Content = keywordElem.Value
                        };
                        coverageModel.Keywords.Add(keyword);
                    }
                }

                XElement sumDscrElem = stdyInfoElem.Element(cb + TAG_SUM_DSCR);
                if (sumDscrElem != null)
                {
                    //Temporal Coverage
                    DateRange dateRange = ReadDateRange(sumDscrElem, cb + TAG_TIME_PRD);
                    if (dateRange != null)
                    {
                        coverageModel.DateRange = dateRange;
                    }


                    //Description
                    coverageModel.Memo = (string)sumDscrElem.Element(cb + TAG_GEOG_COVER);
                }

            }
            studyUnit.Coverage = coverageModel;
        }


        private static void CreateFundingInfos(XElement codebookElem, StudyUnit studyUnit)
        {
            XElement stdyDscrElem = codebookElem.Element(cb + TAG_STDY_DSCR);
            if (stdyDscrElem == null)
            {
                return;
            }

            XElement citationElem = stdyDscrElem.Element(cb + TAG_CITATION);
            if (citationElem == null)
            {
                return;
            }

            XElement prodStmtElem = citationElem.Element(cb + TAG_PROD_STMT);
            if (prodStmtElem == null)
            {
                return;
            }

            IEnumerable<XElement> fundAgElems = prodStmtElem.Elements(cb + TAG_FUND_AG);
            IEnumerable<XElement> grantNoElems = prodStmtElem.Elements(cb + TAG_GRANT_NO);


            List<FundingInfo> fundingInfos = new List<FundingInfo>();
            var fundAgEnumerator = fundAgElems.GetEnumerator();
            var grantNoEnumerator = grantNoElems.GetEnumerator();
            while (fundAgEnumerator.MoveNext() && grantNoEnumerator.MoveNext())
            {
                string organizationName = fundAgEnumerator.Current.Value;
                string number = grantNoEnumerator.Current.Value;

                FundingInfo fundingInfo = new FundingInfo();
                fundingInfo.Number = number;
                fundingInfo.Organization.OrganizationName = organizationName;
                fundingInfos.Add(fundingInfo);
            }

            if (fundingInfos.Count > 0)
            {
                studyUnit.FundingInfos = fundingInfos;
            }
        }

        private static List<DateRange> ReadDateRanges(IEnumerable<XElement> dateElems)
        {
            List<DateRange> dateRanges = new List<DateRange>();
            List<XElement> dateElemList = new List<XElement>(dateElems);

            //No event arribute=only start date
            //start=Start Date
            //end=End Date
            for (int i = 0; i < dateElemList.Count; i++)
            {
                XElement dateElem = dateElemList[i];
                string eventStr = (string)dateElem.Attribute(ATTR_EVENT);
                string dateStr = (string)dateElem.Attribute(ATTR_DATE);
                if (eventStr == "start" && i < dateElemList.Count - 1)
                {
                    //See the following elements if it is start date and it is not the last one
                    XElement nextDateElem = dateElemList[i + 1];
                    string nextEventStr = (string)nextDateElem.Attribute(ATTR_EVENT);
                    string nextDateStr = (string)nextDateElem.Attribute(ATTR_DATE);
                    if (nextEventStr == "end")
                    {
                        //Create data range using from and to if next element is end date
                        dateRanges.Add(DateParser.Parse(dateStr, nextDateStr)); //Can be null
                        i++;
                    }
                    else
                    {
                        //Create using only from if it is not end(data error?)
                        dateRanges.Add(DateParser.Parse(dateStr, null));//Can be null
                    }
                }
                else if (string.IsNullOrEmpty(eventStr))
                {
                    //In the case of valid start date
                    dateRanges.Add(DateParser.Parse(dateStr, null));//Can be null
                }

            }

            return dateRanges;
        }

        //private static Member CreateMemberForSampling(XElement memberElem)
        //{
        //    Member member = new Member();
        //    SetupMemberName(memberElem.Value, member);
        //    string role = (string)memberElem.Attribute(ATTR_ROLE);
        //    string roleCode = Options.RoleCodeByLabel(role);
        //    if (string.IsNullOrEmpty(role) || string.IsNullOrEmpty(roleCode))
        //    {
        //        return null;
        //    }
        //    member.RoleCode = roleCode;
        //    return member;
        //}

        //private static Organization CreateOrganizationForSampling(XElement elem, Member member, List<Organization> organizations)
        //{
        //    string organizationName = (string)elem.Attribute(ATTR_AFFILIATION);
        //    if (string.IsNullOrEmpty(organizationName))
        //    {
        //        organizationName = Resources.UndefinedValue;
        //    }
        //    Organization organization = Organization.FindOrganizationByName(organizations, organizationName);
        //    if (organization == null)
        //    {
        //        organization = new Organization() { OrganizationName = organizationName };
        //        organizations.Add(organization);
        //    }
        //    member.OrganizationId = organization.Id;
        //}

        private static void CreateSamplings(XElement codebookElem, StudyUnit studyUnit, ReaderContext context)
        {
            XElement stdyDscrElem = codebookElem.Element(cb + TAG_STDY_DSCR);
            if (stdyDscrElem == null)
            {
                return;
            }

            XElement methodElem = stdyDscrElem.Element(cb + TAG_METHOD);
            if (methodElem == null)
            {
                return;
            }

            List<Sampling> samplings = new List<Sampling>();
            IEnumerable<XElement> dataCollElems = methodElem.Elements(cb + TAG_DATA_COLL);
            foreach (XElement dataCollElem in dataCollElems)
            {
                Sampling sampling = new Sampling();
                samplings.Add(sampling);

                //Mode of Data Collection
                string method = (string)dataCollElem.Element(cb + TAG_COLL_MODE);
                sampling.MethodName = method;
                //Data Collection Situation
                sampling.Situation = (string)dataCollElem.Element(cb + TAG_COLL_SITU);
                //Data Collection Responsibility
                XElement dataCollectorElem = dataCollElem.Element(cb + TAG_DATA_COLLECTOR);
                if (dataCollectorElem != null)
                {
                    Member newMember = CreateMember(dataCollectorElem, null);
                    if (!string.IsNullOrEmpty(newMember.RoleName))
                    {
                        Member existMember = Member.FindByName(studyUnit.Members, newMember.LastName, newMember.FirstName);
                        if (existMember != null)
                        {
                            newMember = existMember;
                        }
                        else
                        {
                            CreateOrganization(dataCollectorElem, newMember, studyUnit.Organizations);
                            studyUnit.Members.Add(newMember);
                        }
                        sampling.MemberId = newMember.Id;
                    }
                    else
                    {
                        Organization organization = CreateOrganization(dataCollectorElem, null, studyUnit.Organizations);
                        sampling.MemberId = organization.Id;
                    }

                }

                //Read Sampling Procedure
                List<string> samplingMethodsPerTab = new List<string>();
                IEnumerable<XElement> sampProcElems = dataCollElem.Elements(cb + TAG_SAMP_PROC);
                foreach (XElement sampProcElem in sampProcElems)
                {
                    samplingMethodsPerTab.Add(sampProcElem.Value);
                }
                //Memorize Sampling Procedures in each tab(when reading Universe)
                context.SamplingMethods[sampling.Id] = samplingMethodsPerTab;
            }

            //YM of Data Collection
            XElement stdyInfoElem = stdyDscrElem.Element(cb + TAG_STDY_INFO);
            if (stdyInfoElem != null)
            {
                XElement sumDscrElem = stdyInfoElem.Element(cb + TAG_SUM_DSCR);
                if (sumDscrElem != null)
                {
                    List<DateRange> dateRanges = ReadDateRanges(sumDscrElem.Elements(cb + TAG_COLL_DATE));
                    for (int i = 0; i < samplings.Count && i < dateRanges.Count; i++ )
                    {
                        Sampling sampling = samplings[i];
                        DateRange dateRange = dateRanges[i];
                        if (dateRange != null) //dateRange may be null if its text is invalid so check needed
                        {
                            sampling.DateRange = dateRange;
                        }
                    }
                }
            }
            if (samplings.Count > 0)
            {
                studyUnit.Samplings = samplings;
            }
        }

        private static List<Universe> CreateUniversesPerTab(List<XElement> universeElemList, ref int index, List<string> samplingMethodsPerTab)
        {
            List<Universe> universes = new List<Universe>();
            foreach (string samplingMethod in samplingMethodsPerTab)
            {
                if (index > universeElemList.Count - 1)
                {
                    break;
                }
                XElement universeElem = universeElemList[index++];
                Universe universe = new Universe() { Method = samplingMethod };
                SetupUniverse(universeElem.Value, universe);
                universes.Add(universe);
            }
            return universes;
        }

        private static void CreateUniverses(XElement codebookElem, StudyUnit studyUnit, ReaderContext context)
        {
            XElement stdyDscrElem = codebookElem.Element(cb + TAG_STDY_DSCR);
            if (stdyDscrElem == null)
            {
                return;
            }
            XElement stdyInfoElem = stdyDscrElem.Element(cb + TAG_STDY_INFO);
            if (stdyInfoElem == null)
            {
                return;
            }
            XElement sumDscrElem = stdyInfoElem.Element(cb + TAG_SUM_DSCR);
            if (sumDscrElem == null)
            {
                return;
            }
            IEnumerable<XElement> universeElems = sumDscrElem.Elements(cb + TAG_UNIVERSE);
            List<XElement> universeElemList = new List<XElement>(universeElems);
            int index = 0;
            foreach (Sampling sampling in studyUnit.Samplings)
            {
                List<string> samplingMethodsPerTab = context.SamplingMethods[sampling.Id];
                List<Universe> universes = CreateUniversesPerTab(universeElemList, ref index, samplingMethodsPerTab);
                sampling.Universes = universes;
            }
        }

        private static void CreateConceptSchemes(XElement codebookElem, StudyUnit studyUnit, ReaderContext context)
        {
            XElement dataDscrElem = codebookElem.Element(cb + TAG_DATA_DSCR);
            if (dataDscrElem == null)
            {
                return;
            }

            Dictionary<string, List<string>> conceptIds = new Dictionary<string, List<string>>();

            List<ConceptScheme> allConceptSchemes = new List<ConceptScheme>();
            List<Concept> allConcepts = new List<Concept>();

            IEnumerable<XElement> varGrpElems = dataDscrElem.Elements(cb + TAG_VAR_GRP);
            foreach (XElement varGrpElem in varGrpElems)
            {
                string varGrpStr = (string)varGrpElem.Attribute(ATTR_VAR_GRP);
                if (!string.IsNullOrEmpty(varGrpStr))
                {
                    //ConceptScheme
                    ConceptScheme conceptScheme = new ConceptScheme();
                    allConceptSchemes.Add(conceptScheme);

                    conceptScheme.Title = (string)varGrpElem.Element(cb + TAG_CONCEPT);
                    conceptScheme.Memo = (string)varGrpElem.Element(cb + TAG_DEFNTH);

                    List<string> ids = SplitIds(varGrpStr);
                    conceptIds[conceptScheme.Id] = ids;
                }
                else
                {
                    string id = (string)varGrpElem.Attribute(ATTR_ID);
                    if (!string.IsNullOrEmpty(id))
                    {
                        //Concept
                        Concept concept = new Concept();
                        concept.Id = id;
                        concept.Title = (string)varGrpElem.Element(cb + TAG_CONCEPT);
                        concept.Content = (string)varGrpElem.Element(cb + TAG_DEFNTH);
                        allConcepts.Add(concept);

                        string varStr = (string)varGrpElem.Attribute(ATTR_VAR);
                        if (!string.IsNullOrEmpty(varStr))
                        {
                            List<string> ids = SplitIds(varStr);
                            context.VarIds[concept.Id] = ids; //Memorize Variable IDs in each Concept(when reading Variable it is used by setting conceptId)
                        }
                    }
                }
            }

            foreach (ConceptScheme conceptScheme in allConceptSchemes)
            {
                List<string> ids = conceptIds[conceptScheme.Id];
                conceptScheme.Concepts = Concept.FindAll(allConcepts, ids);
            }

            if (allConceptSchemes.Count > 0)
            {
                studyUnit.ConceptSchemes = allConceptSchemes;
            }
        }

        private static void SetupRange(XElement varElem, Response response)
        {
            XElement valrngElem = varElem.Element(cb + TAG_VALRNG);
            if (valrngElem == null) {
                return;
            }

            XElement rangeElem = valrngElem.Element(cb + TAG_RANGE);
            if (rangeElem == null)
            {
                return;
            }

            response.Min = (decimal?)rangeElem.Attribute(ATTR_MIN);
            response.Max = (decimal?)rangeElem.Attribute(ATTR_MAX);
        }

        private static Question CreateQuestion(XElement varElem)
        {
            string id = (string)varElem.Attribute(ATTR_ID);
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            XElement qstnElem = varElem.Element(cb + TAG_QSTN);
            if (qstnElem == null)
            {
                return null;
            }
            string type = (string)qstnElem.Attribute(ATTR_RESPONSE_DOMAIN_TYPE);
            Question question = null;
            if (type == "category")
            {
                //Category
                question = new Question();
                question.Response.TypeCode = Options.RESPONSE_TYPE_CHOICES_CODE;
            }
            else if (type == "numeric")
            {
                question = new Question();
                question.Response.TypeCode = Options.RESPONSE_TYPE_NUMBER_CODE;
            }
            else if (type == "text")
            {
                question = new Question();
                question.Response.TypeCode = Options.RESPONSE_TYPE_FREE_CODE;
            }
            else if (type == "datetime")
            {
                question = new Question();
                question.Response.TypeCode = Options.RESPONSE_TYPE_DATETIME_CODE;
            }
            else
            {
                return null;
            }
            question.Id = id;
            question.Title = (string)qstnElem.Attribute(ATTR_NAME);
            question.Text = (string)qstnElem.Element(cb + TAG_QSTN_LIT);
            return question;
        }

        private static bool IsMissingValue(string missingValue)
        {
            return DDI_Y == missingValue;
        }

        private static CategorySchemeItem CreateCategorySchemeItem(XElement varElem, Variable variable)
        {
            XElement catgryGrpElem = varElem.Element(cb + TAG_CATGRY_GRP);
            if (catgryGrpElem == null)
            {
                return null;
            }
            string title = (string)catgryGrpElem.Element(cb + TAG_LABL);
            if (string.IsNullOrEmpty(title))
            {
                return null;
            }
            string memo = (string)catgryGrpElem.Element(cb + TAG_TXT);
            CategoryScheme categoryScheme = new CategoryScheme()
            {
                Title = title,
                Memo = memo
            };
            CodeScheme codeScheme = new CodeScheme()
            {
                Title = title,
                Memo = memo
            };
            IEnumerable<XElement> catgryElems = varElem.Elements(cb + TAG_CATGRY);
            foreach (XElement catgryElem in catgryElems)
            {
                string missing = (string)catgryElem.Attribute(ATTR_MISSING);
                title = (string)catgryElem.Element(cb + TAG_LABL);
                memo = (string)catgryElem.Element(cb + TAG_TXT);
                string v = (string)catgryElem.Element(cb + TAG_CAT_VALU);

                Category category = new Category() {
                    Title = title,
                    Memo = memo
                };
                category.IsMissingValue = IsMissingValue(missing);
                category.CategorySchemeId = categoryScheme.Id;
                categoryScheme.Categories.Add(category);

                Code code = new Code()
                {
                    Value = v
                };
                code.CodeSchemeId = codeScheme.Id;
                code.CategoryId = category.Id;
                codeScheme.Codes.Add(code);
            }
            variable.Response.CodeSchemeId = codeScheme.Id;
            return new CategorySchemeItem(categoryScheme, codeScheme);
        }

        private static StatisticsInfo CreateNumberStatisticsInfo(XElement varElem, Variable variable)
        {
            StatisticsInfo statisticsInfo = new StatisticsInfo();
            statisticsInfo.Scale = variable.Response.Scale;
            statisticsInfo.VariableId = variable.Id;
            statisticsInfo.StatisticsType = StatisticsTypes.Number;

            IEnumerable<XElement> sumStats = varElem.Elements(cb + TAG_SUM_STAT);
            foreach (XElement sumStat in sumStats)
            {
                string typeName = (string)sumStat.Attribute(ATTR_TYPE);
                string value = sumStat.Value;
                if (typeName == DDI_MEAN)
                {
                    statisticsInfo.SummaryInfo.Mean = StatisticsUtils.ToDouble(value);
                }
                else if (typeName == DDI_MEDN)
                {
                    statisticsInfo.SummaryInfo.Median = StatisticsUtils.ToDouble(value);
                }
                else if (typeName == DDI_VALD)
                {
                    statisticsInfo.SummaryInfo.ValidCases = StatisticsUtils.ToInt(value);
                }
                else if (typeName == DDI_INVD)
                {
                    statisticsInfo.SummaryInfo.InvalidCases = StatisticsUtils.ToInt(value);
                }
                else if (typeName == DDI_MIN)
                {
                    statisticsInfo.SummaryInfo.Minimum = StatisticsUtils.ToDouble(value);
                }
                else if (typeName == DDI_MAX)
                {
                    statisticsInfo.SummaryInfo.Maximum = StatisticsUtils.ToDouble(value);
                }
                else if (typeName == DDI_STDEV)
                {
                    statisticsInfo.SummaryInfo.StandardDeviation = StatisticsUtils.ToDouble(value);
                }
            }
            return statisticsInfo;
        }

        private static CategoryInfo CreateSingleAnswerCategoryInfo(XElement catgry, List<Category> categories, List<Code> codes)
        {
            int frequency = 0;
            decimal percent = 0m;
            IEnumerable<XElement> catStats = catgry.Elements(cb + TAG_CAT_STAT);
            foreach (XElement catStat in catStats)
            {
                string typeName = (string)catStat.Attribute(ATTR_TYPE);
                string value = (string)catStat.Value;
                if (typeName == DDI_FREQ)
                {
                    frequency = StatisticsUtils.ToInt(value);
                }
                else if (typeName == DDI_PERCENT)
                {
                    percent = StatisticsUtils.ToDecimal(value);
                }
            }
            string codeValue = (string)catgry.Element(cb + TAG_CAT_VALU);
            Category category = DDIUtils.FindCategoryByCodeValue(categories, codes, codeValue);
            CategoryInfo categoryInfo = new CategoryInfo();
            categoryInfo.CategoryType = CategoryHelper.GeSingleAnswerCategoryType(codeValue, category);
            categoryInfo.Frequency = frequency;
            categoryInfo.Percent = percent;
            categoryInfo.CodeValue = codeValue;
            categoryInfo.CategoryTitle = CategoryHelper.GeSingleAnswerCategoryTitle(codeValue, category);
            return categoryInfo;
        }

        private static StatisticsInfo CreateSingleAnswerStatisticsInfo(XElement varElem, Variable variable, List<Category> categories, List<Code> codes)
        {
            StatisticsInfo statisticsInfo = new StatisticsInfo();
            statisticsInfo.Scale = variable.Response.Scale;
            statisticsInfo.VariableId = variable.Id;
            statisticsInfo.StatisticsType = StatisticsTypes.ChoicesSingleAnswer;

            IEnumerable<XElement> catgries = varElem.Elements(cb + TAG_CATGRY);
            foreach (XElement catgry in catgries)
            {
                CategoryInfo categoryInfo = CreateSingleAnswerCategoryInfo(catgry, categories, codes);
                statisticsInfo.CategoryInfos.Add(categoryInfo);
            }
            statisticsInfo.CalcPercents();
            return statisticsInfo;
        }

        private static CategoryInfo CreateDateTimeCategoryInfo(XElement catgry, List<MissingValue> missingValues)
        {
            int frequency = 0;
            decimal percent = 0m;
            IEnumerable<XElement> catStats = catgry.Elements(cb + TAG_CAT_STAT);
            foreach (XElement catStat in catStats)
            {
                string typeName = (string)catStat.Attribute(ATTR_TYPE);
                string value = (string)catStat.Value;
                if (typeName == DDI_FREQ)
                {
                    frequency = StatisticsUtils.ToInt(value);
                }
                else if (typeName == DDI_PERCENT)
                {
                    percent = StatisticsUtils.ToDecimal(value);
                }
            }
            string codeValue = (string)catgry.Element(cb + TAG_CAT_VALU);
            CategoryInfo categoryInfo = new CategoryInfo();
            categoryInfo.CategoryType = CategoryHelper.GetDateTimeCategoryType(codeValue, missingValues);
            categoryInfo.Frequency = frequency;
            categoryInfo.Percent = percent;
            categoryInfo.CodeValue = codeValue;
            categoryInfo.CategoryTitle = CategoryHelper.GetDateTimeCategoryTitle(codeValue);
            return categoryInfo;
        }

        private static StatisticsInfo CreateDateTimeStatisticsInfo(XElement varElem, Variable variable)
        {
            StatisticsInfo statisticsInfo = new StatisticsInfo();
            statisticsInfo.Scale = variable.Response.Scale;
            statisticsInfo.VariableId = variable.Id;
            statisticsInfo.StatisticsType = StatisticsTypes.DateTime;

            IEnumerable<XElement> catgries = varElem.Elements(cb + TAG_CATGRY);
            foreach (XElement catgry in catgries)
            {
                CategoryInfo categoryInfo = CreateDateTimeCategoryInfo(catgry, variable.Response.MissingValues);
                statisticsInfo.CategoryInfos.Add(categoryInfo);
            }
            statisticsInfo.CalcPercents();
            return statisticsInfo;
        }

        private static VariableItem CreateVariableItem(XElement varElem, ReaderContext context)
        {
            string id = (string)varElem.Attribute(ATTR_ID);
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            string title = (string)varElem.Attribute(ATTR_NAME);
            if (string.IsNullOrEmpty(title))
            {
                return null;
            }
            string representationType = (string)varElem.Attribute(ATTR_REPRESENTATION_TYPE);
            string responseTypeCode = GetTypeFromRepresentationType(representationType);
            if (responseTypeCode == null)
            {
                return null;
            }

            string files = (string)varElem.Attribute(ATTR_FILES);
            if (!string.IsNullOrEmpty(files))
            {
                //Memorize Dataset ID(when reading Dataset)
                context.DataSetIds[id] = SplitIds(files);
            }
                
            Variable variable = new Variable();
            variable.Id = id;
            variable.Title = title;
            variable.Label = (string)varElem.Element(cb + TAG_LABL);
            variable.Response.TypeCode = responseTypeCode;


            variable.ConceptId = context.FindConceptIdByVarId(variable.Id);
            Question question =  null;
            if (variable.ConceptId != null)            
            {
                //An exception error caught when creating ViewModel if corresponding Concept does not exist
                question = CreateQuestion(varElem);
                if (question != null)
                {
                    question.ConceptId = variable.ConceptId;
                    variable.QuestionId = question.Id;
                }
            }

            CategorySchemeItem categorySchemeItem = null;
            StatisticsInfo statisticsInfo = null;
            if (variable.Response.IsTypeChoices)
            {
                categorySchemeItem = CreateCategorySchemeItem(varElem, variable);
                List<Category> categories = new List<Category>();
                List<Code> codes = new List<Code>();
                if (categorySchemeItem != null)
                {
                    categories = categorySchemeItem.CategoryScheme.Categories;
                    codes = categorySchemeItem.CodeScheme.Codes;
                }
                statisticsInfo = CreateSingleAnswerStatisticsInfo(varElem, variable, categories, codes);
            }
            else if (variable.Response.IsTypeNumber)
            {
                SetupRange(varElem, variable.Response);
                statisticsInfo = CreateNumberStatisticsInfo(varElem, variable);
            }
            else if (variable.Response.IsTypeFree)
            {
                SetupRange(varElem, variable.Response);
            }
            else if (variable.Response.IsTypeDateTime)
            {
                statisticsInfo = CreateDateTimeStatisticsInfo(varElem, variable);
            }
            VariableItem variableItem = new VariableItem(variable, question);
            variableItem.CategorySchemeItem = categorySchemeItem;
            variableItem.StatisticsInfo = statisticsInfo;
            return variableItem;
        }

        private static void CreateVariables(XElement codebookElem, StudyUnit studyUnit, ReaderContext context)
        {
            XElement dataDscrElem = codebookElem.Element(cb + TAG_DATA_DSCR);
            if (dataDscrElem == null)
            {
                return;
            }
            List<Variable> variables = new List<Variable>();
            List<Question> questions = new List<Question>();
            List<CategoryScheme> categorySchemes = new List<CategoryScheme>();
            List<CodeScheme> codeSchemes = new List<CodeScheme>();
            List<StatisticsInfo> statisticsInfos = new List<StatisticsInfo>();
            IEnumerable<XElement> varElems = dataDscrElem.Elements(cb + TAG_VAR);
            foreach (XElement varElem in varElems)
            {
                VariableItem variableItem = CreateVariableItem(varElem, context);
                if (variableItem != null)
                {
                    Variable variable = variableItem.Variable;
                    variables.Add(variable);

                    Question question = variableItem.Question;
                    if (question != null)
                    {
                        questions.Add(question);
                    }
                    if (question != null && question.ConceptId == null)
                    {
                        Debug.WriteLine("Question's conceptId is null");
                    }

                    CategorySchemeItem categorySchemeItem = variableItem.CategorySchemeItem;
                    if (categorySchemeItem != null)
                    {
                        categorySchemes.Add(categorySchemeItem.CategoryScheme);
                        codeSchemes.Add(categorySchemeItem.CodeScheme);
                    }

                    StatisticsInfo statisticsInfo = variableItem.StatisticsInfo;
                    if (statisticsInfo != null)
                    {
                        statisticsInfos.Add(statisticsInfo);
                    }
                }
            }
            if (variables.Count > 0)
            {
                studyUnit.Variables = variables;
            }
            if (questions.Count > 0)
            {
                studyUnit.Questions = questions;
            }
            if (categorySchemes.Count > 0)
            {
                studyUnit.CategorySchemes = categorySchemes;
            }
            if (codeSchemes.Count > 0)
            {
                studyUnit.CodeSchemes = codeSchemes;
            }
            if (statisticsInfos.Count > 0)
            {
                studyUnit.StatisticsInfos = statisticsInfos;
            }
        }

        private static DataSetItem CreateDataSetItem(XElement fileDscrElem)
        {
            string id = (string)fileDscrElem.Attribute(ATTR_ID);
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }
            string uri = (string)fileDscrElem.Attribute(ATTR_URI);


            XElement fileTxtElem = fileDscrElem.Element(cb + TAG_FILE_TXT);
            if (fileTxtElem == null)
            {
                return null;
            }

            string title = (string)fileTxtElem.Element(cb + TAG_FILE_NAME);
            string memo = (string)fileTxtElem.Element(cb + TAG_FILE_CONT);
            string format = (string)fileTxtElem.Element(cb + TAG_FORMAT);
            DataSet dataSet = new DataSet()
            {
                Id = id,
                Title = title,
                Memo = memo
            };
            DataFile dataFile = new DataFile()
            {
                Uri = uri,
                Format = format
            };
            dataFile.DataSetId = dataSet.Id;

            return new DataSetItem(dataSet, dataFile);
        }

        private static void CreateDataSets(XElement codebookElem, StudyUnit studyUnit, ReaderContext context)
        {
            IEnumerable<XElement> fileDscrElems = codebookElem.Elements(cb + TAG_FILE_DSCR);
            List<DataSet> dataSets = new List<DataSet>();
            List<DataFile> dataFiles = new List<DataFile>();
            foreach (XElement fileDscrElem in fileDscrElems)
            {
                DataSetItem dataSetItem = CreateDataSetItem(fileDscrElem);
                if (dataSetItem != null)
                {
                    DataSet dataSet = dataSetItem.DataSet;

                    List<string> variableIds = new List<string>();
                    foreach (KeyValuePair<string, List<string>> pair in context.DataSetIds)
                    {
                        string variableId = pair.Key;
                        List<string> dataSetIds = pair.Value;
                        if (dataSetIds.Contains(dataSet.Id))
                        {
                            variableIds.Add(variableId);
                        }
                    }

                    dataSet.VariableGuids = variableIds;
                    dataSets.Add(dataSet);
                    dataFiles.Add(dataSetItem.DataFile);
                }
            }

            if (dataSets.Count > 0)
            {
                studyUnit.DataSets = dataSets;
                studyUnit.DataFiles = dataFiles;
            }
        }


        private static Book CreateBook(XElement citationElem)
        {
            XElement titlStmtElem = citationElem.Element(cb + TAG_TITL_STMT);
            if (titlStmtElem == null)
            {
                return null;
            }
            string title = (string)titlStmtElem.Element(cb + TAG_TITL);
            if (string.IsNullOrEmpty(title))
            {
                return null;
            }
            Book book = new Book();
            book.BookTypeCode = Options.BOOK_TYPE_TREATISE_WITH_PEER_REVIEW;
            book.Title = title;


            XElement rspStmtElem = citationElem.Element(cb + TAG_RSP_STMT);
            if (rspStmtElem != null)
            {
                //Author
                book.Author = (string)rspStmtElem.Element(cb + TAG_AUTH_ENTY);
                //Editor
                book.Editor = (string)rspStmtElem.Element(cb + TAG_OTH_ID);
            }

            //XElement prodStmtElem = citationElem.Element(cb + TAG_PROD_STMT);
            //if (prodStmtElem != null)
            //{
            //    //publisher
            //    book.Publisher = (string)prodStmtElem.Element(cb + TAG_PRODUCER);
            //}

            XElement distStmtElem = citationElem.Element(cb + TAG_DIST_STMT);
            if (distStmtElem != null)
            {
                //Date of publication
                book.AnnouncementDate = (string)distStmtElem.Element(cb + TAG_DIST_DATE);
            }

            ParseIdentifier((string)citationElem.Element(cb + TAG_BIBL_CIT), book);

            XElement holdingsElem = citationElem.Element(cb + TAG_HOLDINGS);
            if (holdingsElem != null)
            {
                book.Url = (string)holdingsElem.Attribute(ATTR_URI);
            }

            book.Summary = (string)citationElem.Element(terms + TAG_ABSTRACT);
            book.Language = (string)citationElem.Element(dc + TAG_LANGUAGE);
            ParsePublisher((string)citationElem.Element(dc + TAG_PUBLISHER), book);

            return book;
        }

        private static void CreateBooks(XElement codebookElem, StudyUnit studyUnit)
        {
            XElement stdyDscrElem = codebookElem.Element(cb + TAG_STDY_DSCR);
            if (stdyDscrElem == null)
            {
                return;
            }
            XElement othrStdyMatElem = stdyDscrElem.Element(cb + TAG_OTHR_STDY_MAT);
            if (othrStdyMatElem == null)
            {
                return;
            }
            XElement relMatElem = othrStdyMatElem.Element(cb + TAG_REL_MAT);
            if (relMatElem == null)
            {
                return;
            }

            List<Book> books = new List<Book>();
            IEnumerable<XElement> citationElems = relMatElem.Elements(cb + TAG_CITATION);
            foreach (XElement citationElem in citationElems)
            {
                Book book = CreateBook(citationElem);
                if (book != null)
                {
                    books.Add(book);   
                }                
            }
            if (books.Count > 0)
            {
                studyUnit.Books = books;
            }
        }

        private static StudyUnit CreateStudyUnit(XElement codebookElem)
        {
            //Read StudyUnit
            StudyUnit studyUnit = StudyUnit.CreateDefault();

            //Not applicable in Events

            //Study Member
            CreateMembers(codebookElem, studyUnit);

            //Abstract
            CreateAbstract(codebookElem, studyUnit);

            //Coverage
            CreateCoverage(codebookElem, studyUnit);

            //Funding
            CreateFundingInfos(codebookElem, studyUnit);

            //Sampling(should read before Universe)
            ReaderContext context = new ReaderContext();
            CreateSamplings(codebookElem, studyUnit, context);

            //Universe
            CreateUniverses(codebookElem, studyUnit, context);

            //Concept
            CreateConceptSchemes(codebookElem, studyUnit, context);

//            //question
//            CreateQuestions(codebookElem, studyUnit);

            //Category(Since it exists below var tag, it may not be read independently)
//            CreateCategorySchemes(codebookElem, studyUnit);

            //Variable
            CreateVariables(codebookElem, studyUnit, context);

            //Dataset
            CreateDataSets(codebookElem, studyUnit, context);

            //Related materials
            CreateBooks(codebookElem, studyUnit);

            return studyUnit;
        }

        private static EDOModel CreateSingleModel(XElement codebookElem)
        {
            EDOModel model = new EDOModel();
            StudyUnit studyUnit = CreateStudyUnit(codebookElem);
            if (studyUnit != null)
            {
                model.StudyUnits.Add(studyUnit);
            }
            return model;
        }

        private void MergeDataSet(StudyUnit newStudyUnit, StudyUnit curStudyUnit)
        {
            if (newStudyUnit.DataSets.Count > 0 && newStudyUnit.DataSets[0].Title == EDOConstants.LABEL_ALL)
            {
                newStudyUnit.DataSets.RemoveAt(0);
            }
            curStudyUnit.DataSets.AddRange(newStudyUnit.DataSets);
            curStudyUnit.DataFiles.AddRange(newStudyUnit.DataFiles);
        }

        public void MergeStudyUnit(StudyUnit newStudyUnit, StudyUnit curStudyUnit, DDIImportOption importOption)
        {
            if (importOption.ImportMember)
            {
                curStudyUnit.MergeMember(newStudyUnit);
            }
            if (importOption.ImportAbstract)
            {
                curStudyUnit.MergeAbstract(newStudyUnit);
            }
            if (importOption.ImportCoverage)
            {
                curStudyUnit.MergeCoverage(newStudyUnit);
            }
            if (importOption.ImportFundingInfo)
            {
                curStudyUnit.MergeFundingInfo(newStudyUnit);
            }
            if (importOption.ImportSampling)
            {
                curStudyUnit.MergeSampling(newStudyUnit);
            }
            if (importOption.ImportConcept)
            {
                curStudyUnit.MergeConcept(newStudyUnit);
            }
            if (importOption.ImportQuestion)
            {
                curStudyUnit.MergeQuestion(newStudyUnit);
            }
            if (importOption.ImportVariable)
            {
                curStudyUnit.MergeVariable(newStudyUnit);
                curStudyUnit.MergeCategory(newStudyUnit);
                curStudyUnit.MergeCode(newStudyUnit);
            }
            if (importOption.ImportDataSet)
            {
                curStudyUnit.MergeDataSet(newStudyUnit);
                curStudyUnit.MergeDataFile(newStudyUnit);
            }
            if (importOption.ImportBook)
            {
                curStudyUnit.MergeBook(newStudyUnit);
            }
            if (importOption.ImportStatistics)
            {
                curStudyUnit.MergeStatisticsInfos(newStudyUnit);
            }
            DDIUtils.FillCollectorFields(newStudyUnit);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curStudyUnit"></param>
        /// <param name="curEdoModel"></param>
        /// <param name="path"></param>
        /// <param name="merge"></param>
        /// <returns></returns>
        /// <exception cref="DDIIO.ValidationException" />
        /// <exception cref="System.Xml.XmlException" />
        public EDOModel Read(string path)
        {
            CanImport(path);

            XmlReaderSettings settings = new XmlReaderSettings();
            using (XmlReader reader = XmlReader.Create(path, settings))
            {
                XDocument doc = XDocument.Load(reader);
                XElement codebookElem = doc.Element(cb + TAG_CODEBOOK);
                if (codebookElem != null)
                {
                    return CreateSingleModel(codebookElem);
                }
            }

            throw new ValidationException(new List<ValidationError>() { new ValidationError("", Properties.Resources.DDI2CodeBookDontExist, 0, 0) });
        }

        public DDIImportOption GenerateImportOption()
        {
            return new DDI2ImportOption();
        }
    }
}
