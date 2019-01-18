using System;
using System.Collections.Generic;
using System.Linq;
using EDO.Core.Model;
using System.Xml;
using System.Xml.Linq;
using EDO.Core.Util;
using EDO.Core.Model.Statistics;
using EDO.Core.Util.Statistics;

namespace EDO.Core.IO
{
    public class DDI3Reader :DDI3IO, IDDIReader
    {
        #region Utility for reading

        public static string ReadReferenceID(XElement elem, XName name)
        {
            XElement refElem = elem.Descendants(name).FirstOrDefault();
            string refId = null;
            if (refElem != null)
            {
                refId = (string)refElem.Element(r + TAG_ID);
            }
            return refId;
        }

        public static DateRange ReadDateRange(XElement elem, XName name)
        {
            DateRange dateRange = null;
            XElement dateElem = elem.Descendants(name).FirstOrDefault();
            if (dateElem != null)
            {
                //Date
                DateTime? fromDate = (DateTime?)dateElem.Element(r + TAG_SIMPLE_DATE);
                DateTime? toDate = null;
                if (fromDate == null)
                {
                    fromDate = (DateTime?)dateElem.Element(r + TAG_START_DATE);
                    toDate = (DateTime?)dateElem.Element(r + TAG_END_DATE);
                }
                if (fromDate != null || toDate != null)
                {
                    dateRange = new DateRange(fromDate, toDate);
                }
            }
            return dateRange;
        }

        public static DateUnit ReadDateUnit(XElement rootElem, XName dateElemName)
        {
            DateUnit dateUnit = null;
            XElement dateElem = rootElem.Descendants(dateElemName).FirstOrDefault() ;
            if (dateElem != null)
            {
                string dateStr = (string)dateElem.Element(r + TAG_SIMPLE_DATE);
                dateUnit = ParseDateUnit(dateStr);
            }
            return dateUnit;
        }


        public static string ReadDateUnitAsString(XElement rootElem, XName dateElemName)
        {
            DateUnit dateUnit = ReadDateUnit(rootElem, dateElemName);
            if (dateUnit == null)
            {
                return null;
            }
            return dateUnit.ToString();
        }
        #endregion

        #region Read StudyUnit

        public static Event CreateEvent(XElement lifeEventElem)
        {
            Event eventModel = new Event();
            string id = (string)lifeEventElem.Attribute(ATTR_ID);
            if (id == null)
            {
                return null;
            }
            eventModel.Id = id;
            //Title
            XElement descElem = lifeEventElem.Element(r + TAG_DESCRIPTION);
            if (descElem != null)
            {
                eventModel.Title = descElem.Value;
            }

            //Description
            XElement labelElem = lifeEventElem.Element(r + TAG_LABEL);
            if (labelElem != null)
            {
                eventModel.Memo = labelElem.Value;
            }
            eventModel.DateRange = ReadDateRange(lifeEventElem, r + TAG_DATE);
            return eventModel;
        }

        public static void CreateEvents(XElement studyUnitElem, StudyUnit studyUnit)
        {
            //Load Events
            XElement lifeInfoElem = studyUnitElem.Descendants(r + TAG_LIFECYCLE_INFORMATION).FirstOrDefault();
            if (lifeInfoElem == null)
            {
                return;
            }
            List<Event> eventModels = new List<Event>();
            IEnumerable<XElement> elements = lifeInfoElem.Elements(r + TAG_LIFECYCLE_EVENT);
            foreach (XElement lifeEventElem in elements)
            {
                Event eventModel = CreateEvent(lifeEventElem);
                if (eventModel != null)
                {
                    ////It should be removed the same thing by searching by title(assume what are made with DefaultStudyUnit)
                    //Event.RemoveByTitle(studyUnit.Events, eventModel.Title);
                    eventModels.Add(eventModel);
                }
            }
            if (eventModels.Count > 0)
            {
                studyUnit.Events = eventModels;
            }
        }

        public static Organization CreateOrganization(XElement organizationElem)
        {
            string id = (string)organizationElem.Attribute(ATTR_ID);
            if (id == null)
            {
                return null;
            }
            Organization organizationModel = new Organization();
            organizationModel.Id = id;
            organizationModel.OrganizationName = (string)organizationElem.Element(a + TAG_ORGANIZATION_NAME);
            return organizationModel;
        }

        public static void CreateOrganizations(XElement studyUnitElem, StudyUnit studyUnit)
        {
            XElement archiveElem = studyUnitElem.Element(a + TAG_ARCHIVE);
            if (archiveElem == null)
            {
                return;
            }
            string archiveId = (string)archiveElem.Attribute(ATTR_ID);
            if (archiveId == null)
            {
                return;
            }
            XElement organizationSchemeElem = archiveElem.Element(a + TAG_ORGANIZATION_SCHEME);
            if (organizationSchemeElem == null)
            {
                return;
            }
            string organizationSchemeId = (string)organizationSchemeElem.Attribute(ATTR_ID);
            if (organizationSchemeId == null)
            {
                return;
            }

            List<Organization> organizationModels = new List<Organization>();
            IEnumerable<XElement> elements = organizationSchemeElem.Elements(a + TAG_ORGANIZATION);
            foreach (XElement organizationElem in elements)
            {
                Organization organizationModel = CreateOrganization(organizationElem);
                if (organizationModel != null)
                {
                    organizationModels.Add(organizationModel);
                }
            }
            studyUnit.ArchiveId = archiveId;
            studyUnit.OrganizationSchemeId = organizationSchemeId;
            if (organizationModels.Count > 0)
            {
                studyUnit.Organizations = organizationModels;
            }
        }

        public static Member CreateMember(XElement individualElem)
        {
            Member memberModel = new Member();
            string id = (string)individualElem.Attribute(ATTR_ID);
            if (id == null)
            {
                return null;
            }
            memberModel.Id = id;
            XElement nameElem = individualElem.Element(a + TAG_INDIVIDUAL_NAME);
            if (nameElem != null)
            {
                memberModel.FirstName = (string)nameElem.Element(a + TAG_FIRST);
                memberModel.LastName = (string)nameElem.Element(a + TAG_LAST);
            }
            XElement posElem = individualElem.Element(a + TAG_POSITION);
            if (posElem != null)
            {
                memberModel.Position = (string)posElem.Element(a + TAG_TITLE);
            }
            string role = (string)individualElem.Element(r + TAG_DESCRIPTION);
            if (role != null)
            {
                memberModel.RoleName = role;
            }

            XElement refElem = individualElem.Descendants(a + TAG_ORGANIZATION_REFERENCE).FirstOrDefault();
            if (refElem != null)
            {
                string refId = (string)refElem.Element(r + TAG_ID);
                memberModel.OrganizationId = refId;
            }
            return memberModel;
        }

        public static void CreateMembers(XElement studyUnitElem, StudyUnit studyUnit)
        {
            List<Member> members = new List<Member>();
            IEnumerable<XElement> elements = studyUnitElem.Descendants(a + TAG_INDIVIDUAL);
            foreach (XElement individualElem in elements)
            {
                Member memberModel = CreateMember(individualElem);
                if (memberModel != null && studyUnit.FindOrganization(memberModel.OrganizationId) != null)
                {
                    members.Add(memberModel);
                }
            }
            if (members.Count > 0)
            {
                studyUnit.Members = members;
            }
        }

        public static void CreateAbstract(XElement studyUnitElem, StudyUnit studyUnit)
        {
            ///// Title
            Abstract abstractModel = new Abstract();
            XElement citationElem = studyUnitElem.Element(r + TAG_CITATION);
            if (citationElem == null)
            {
                return;
            }
            XElement titleElement = citationElem.Element(r + TAG_TITLE);
            if (titleElement == null)
            {
                return;
            }
            abstractModel.Title = titleElement.Value;

            ///// Abstract
            XElement abstractElem = studyUnitElem.Element(s + TAG_ABSTRACT);
            if (abstractElem == null)
            {
                return;
            }
            XAttribute summaryIdAttr = abstractElem.Attribute(ATTR_ID);
            if (summaryIdAttr == null)
            {
                return;
            }
            XElement contentElem = abstractElem.Element(r + TAG_CONTENT);
            if (contentElem == null)
            {
                return;
            }
            abstractModel.SummaryId = summaryIdAttr.Value;
            abstractModel.Summary = contentElem.Value;

            ///// Purpose
            XElement purposeElem = studyUnitElem.Element(s + TAG_PURPOSE);
            if (purposeElem == null)
            {
                return;
            }
            XAttribute purposeIdAttr = purposeElem.Attribute(ATTR_ID);
            if (purposeIdAttr == null)
            {
                return;
            }
            contentElem = purposeElem.Element(r + TAG_CONTENT);
            if (contentElem == null)
            {
                return;
            }
            abstractModel.PurposeId = purposeIdAttr.Value;
            abstractModel.Purpose = contentElem.Value;
            studyUnit.Abstract = abstractModel;
        }

        private static Dictionary<string, string> ReadGeographicStructureElems(XElement studyUnitElem)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            XElement conceptualComponentElem = studyUnitElem.Element(c + TAG_CONCEPTUAL_COMPONENT);
            if (conceptualComponentElem == null)
            {
                return dict;
            }
            XElement geographicStructureSchemeElem = conceptualComponentElem.Element(c + TAG_GEOGRAPHIC_STRUCTURE_SCHEME);
            if (geographicStructureSchemeElem == null)
            {
                return dict;
            }
            IEnumerable<XElement> elems = geographicStructureSchemeElem.Elements(r + TAG_GEOGRAPHIC_STRUCTURE);
            foreach (XElement elem in elems)
            {
                string id = (string)elem.Attribute(ATTR_ID);
                if (id == null)
                {
                    continue;
                }
                XElement geographyElem = elem.Element(r + TAG_GEOGRAPHY);
                if (geographyElem == null)
                {
                    continue;
                }
                XElement levelElem = geographyElem.Element(r + TAG_LEVEL);
                if (levelElem == null)
                {
                    continue;
                }
                string name = (string)levelElem.Element(r + TAG_NAME);
                if (name == null)
                {
                    continue;
                }
                dict.Add(id, name);
            }
            return dict;
        }

        public static void CreateCoverage(XElement studyUnitElem, StudyUnit studyUnit)
        {
            XElement coverageElem = studyUnitElem.Element(r + TAG_COVERAGE);
            if (coverageElem == null)
            {
                return;
            }
            Coverage coverageModel = Coverage.CreateDefault();
            XElement topicElem = coverageElem.Element(r + TAG_TOPICAL_COVERAGE);
            if (topicElem != null)
            {
                //Topical Coverage
                IEnumerable<XElement> elements = topicElem.Elements(r + TAG_SUBJECT);
                List<string> labels = new List<string>();
                foreach (XElement subjectElem in elements)
                {
                    labels.Add(subjectElem.Value);
                }
                coverageModel.CheckTopics(labels);

                //Keyword
                elements = topicElem.Elements(r + TAG_KEYWORD);
                foreach (XElement keywordElem in elements)
                {
                    Keyword keyword = new Keyword()
                    {
                        Content = keywordElem.Value
                    };
                    coverageModel.Keywords.Add(keyword);
                }
            }

            //Study date
            XElement temporalElem = coverageElem.Element(r + TAG_TEMPORAL_COVERAGE);
            if (temporalElem != null)
            {
                coverageModel.DateRange = ReadDateRange(temporalElem, r + TAG_REFERENCE_DATE);
            }

            //Description
            XElement spatialElem = coverageElem.Element(r + TAG_SPATIAL_COVERAGE);
            if (spatialElem != null)
            {
                //Geographic Levels Covered
                Dictionary<string, string> labelDict = ReadGeographicStructureElems(studyUnitElem);

                List<string> checkLabels = new List<string>();
                IEnumerable<XElement> elements = spatialElem.Elements(r + TAG_GEOGRAPHIC_STRUCTURE_REFERENCE);
                foreach (XElement refElem in elements)
                {
                    string refId = (string)refElem.Element(r + TAG_ID);
                    if (refId == null) 
                    {
                        continue;
                    }
                    string code = IDUtils.ToCode(refId);
                    if (code == null)
                    {
                        continue;
                    }
                    if (!labelDict.ContainsKey(refId))
                    {
                        continue;
                    }
                    string label = labelDict[refId];
                    checkLabels.Add(label);
                }
                coverageModel.CheckAreas(checkLabels);

                //Description
                string memo = (string)spatialElem.Element(r + TAG_DESCRIPTION);
                coverageModel.Memo = memo;
            }



            studyUnit.Coverage = coverageModel;
        }

        public static FundingInfo CreateFundingInfo(XElement fundingInfoElem, List<Organization> organizations)
        {
            FundingInfo fundingInfo = new FundingInfo();
            string id = ReadReferenceID(fundingInfoElem, r + TAG_AGENCY_ORGANIZATION_REFERENCE);
            if (id == null)
            {
                return null;
            }
            Organization organizationModel = Organization.GetOrganization(organizations, id);
            if (organizationModel == null)
            {
                return null;
            }
            organizations.Remove(organizationModel);
            fundingInfo.Organization = organizationModel;
            fundingInfo.Number = (string)fundingInfoElem.Element(r + TAG_GRANT_NUMBER);
            IEnumerable<XElement> elements = fundingInfoElem.Elements(r + TAG_DESCRIPTION);
            foreach (XElement elem in elements)
            {
                string value = null;
                if (IsDDIFundingInfoTitle(elem.Value, ref value))
                {
                    fundingInfo.Title = value;
                }
                else if (IsDDIFundingInfoMoney(elem.Value, ref value))
                {
                    decimal money;
                    if (Decimal.TryParse(value, out money))
                    {
                        fundingInfo.Money = money;
                    }
                }
                else if (IsDDIFundingInfoDateRange(elem.Value, ref value))
                {
                    fundingInfo.DateRange = ParseDateRange(value);
                }
            }
            return fundingInfo;
        }

        public static void CreateFundingInfos(XElement studyUnitElem, StudyUnit studyUnit)
        {
            List<FundingInfo> fundingInfoModels = new List<FundingInfo>();
            IEnumerable<XElement> elements = studyUnitElem.Elements(r + TAG_FUNDING_INFORMATION);
            foreach (XElement fundingInfoElem in elements)
            {
                FundingInfo fundingInfoModel = CreateFundingInfo(fundingInfoElem, studyUnit.Organizations);
                if (fundingInfoModel != null)
                {
                    fundingInfoModels.Add(fundingInfoModel);
                }
            }
            if (fundingInfoModels.Count > 0)
            {
                studyUnit.FundingInfos = fundingInfoModels;
            }
        }

        public static Universe CreateUniverse(XElement universeElem)
        {
            string id = (string)universeElem.Attribute(ATTR_ID);
            if (id == null)
            {
                return null;
            }

            Universe universe = new Universe();
            universe.Id = id;

            universe.Title = (string)universeElem.Element(r + TAG_LABEL);
            universe.Memo = (string)universeElem.Element(c + TAG_HUMAN_READABLE);

            return universe;
        }

        public static void CreateUniverses(XElement studyUnitElem, StudyUnit studyUnit)
        {
            XElement universeSchemeElem = studyUnitElem.Descendants(c + TAG_UNIVERSE_SCHEME).FirstOrDefault();
            if (universeSchemeElem == null)
            {
                return;
            }
            string universeSchemeId = (string)universeSchemeElem.Attribute(ATTR_ID);
            if (universeSchemeId == null)
            {
                return;
            }

//            List<Universe> universeModels = new List<Universe>();

            ///// Reading of the main Universe
            IEnumerable<XElement> elements = studyUnitElem.Descendants(c + TAG_UNIVERSE);
            int index = 0;
            foreach (XElement universeElem in elements) //This loop corresponds to the tab of the sampling method
            {
                Sampling sampling = studyUnit.GetSamplingAt(index++);
                if (sampling == null)
                {
                    break;
                }
                Universe universeModel = CreateUniverse(universeElem);
                if (universeModel == null)
                {
                    continue;
                }
                universeModel.IsMain = true;
                sampling.Universes.Add(universeModel);
                ///// Reading of the sub-Universe
                IEnumerable<XElement> subElements = universeElem.Elements(c + TAG_SUB_UNIVERSE);
                foreach (XElement subUniverseElem in subElements)
                {
                    Universe subUniverseModel = CreateUniverse(subUniverseElem);
                    if (subUniverseModel != null)
                    {
                        sampling.Universes.Add(subUniverseModel);
                    }
                }
            }

            List<Universe> universeModels = studyUnit.AllUniverses;
            ///// Read Sampling method in order and associate
            XElement dataCollectionElem = studyUnitElem.Element(d + TAG_DATA_COLLECTION);
            if (dataCollectionElem != null)
            {
                XElement methodologyElem = dataCollectionElem.Element(d + TAG_METHODOLOGY);
                if (methodologyElem != null)
                {
                    index = 0;
                    IEnumerable<XElement> samplingProcedureElems = methodologyElem.Elements(d + TAG_SAMPLING_PROCEDURE);
                    foreach (XElement samplingProcedureElem in samplingProcedureElems)
                    {
                        string id = (string)samplingProcedureElem.Attribute(ATTR_ID);
                        string content = (string)samplingProcedureElem.Element(r + TAG_CONTENT);
                        if (index < universeModels.Count && id != null && content != null)
                        {
                            Universe universeModel = universeModels[index++];
                            universeModel.SamplingProcedureId = id;
                            universeModel.Method = content;
                        }
                    }
                }
            }

            studyUnit.UniverseSchemeId = universeSchemeId;
        }

        public static void CreateSampling(XElement studyUnitElem, StudyUnit studyUnit)
        {
            XElement dataCollectionElem = studyUnitElem.Element(d + TAG_DATA_COLLECTION);
            if (dataCollectionElem == null)
            {
                return;
            }
            string dataCollectionId = (string)dataCollectionElem.Attribute(ATTR_ID);
            if (dataCollectionId == null)
            {
                return;
            }
            XElement methodologyElem = dataCollectionElem.Element(d + TAG_METHODOLOGY);
            if (methodologyElem == null)
            {
                return;
            }
            string methodologyId = (string)methodologyElem.Attribute(ATTR_ID);
            if (methodologyElem == null)
            {
                return;
            }

            List<Sampling> samplingModels = new List<Sampling>();

            IEnumerable<XElement> collectionEventElems = dataCollectionElem.Elements(d + TAG_COLLECTION_EVENT);
            foreach (XElement collectionEventElem in collectionEventElems)
            {
                Sampling samplingModel = new Sampling();
                samplingModel.DateRange = ReadDateRange(collectionEventElem, d + TAG_DATA_COLLECTION_DATE);
                samplingModel.MemberId = ReadReferenceID(collectionEventElem, d + TAG_DATA_COLLECTOR_ORGANIZATION_REFERENCE);
                XElement modeOfCollection = collectionEventElem.Element(d + TAG_MODE_OF_COLLECTION);
                if (modeOfCollection != null)
                {
                    string content = (string)modeOfCollection.Element(r + TAG_CONTENT);
                    samplingModel.MethodName = content;
                }

                XElement situationElem = collectionEventElem.Element(d + TAG_COLLECTION_SITUATION);
                if (situationElem != null)
                {
                    samplingModel.Situation = (string)situationElem.Element(r + TAG_CONTENT);
                }
                samplingModels.Add(samplingModel);
            }
            studyUnit.MethodologyId = methodologyId;
            studyUnit.DataCollectionId = dataCollectionId;

            XElement processingEventElem = dataCollectionElem.Element(d + TAG_PROCESSING_EVENT);
            if (processingEventElem != null) {
                int index = 0;
                IEnumerable<XElement> dataAppraisalInformationElems = processingEventElem.Elements(d + TAG_DATA_APPRAISAL_INFORMATION);
                foreach (XElement dataAppraisalInformationElem in dataAppraisalInformationElems)
                {
                    if (index >=  samplingModels.Count)
                    {
                        break;
                    }
                    Sampling samplingModel = samplingModels[index++];

                    IEnumerable<XElement> responseRateElems = dataAppraisalInformationElem.Elements(d + TAG_RESPONSE_RATE);
                    foreach (XElement responseRateElem in responseRateElems)
                    {
                        string content = (string)responseRateElem;
                        SamplingNumber samplingNumberModel = new SamplingNumber();
                        ParseResponseRate(content, samplingNumberModel);
                        samplingModel.SamplingNumbers.Add(samplingNumberModel);
                    }
                }
            }

            if (samplingModels.Count > 0)
            {
                studyUnit.Samplings = samplingModels;
            }
        }

        public static ConceptScheme CreateConceptScheme(XElement conceptSchemeElem)
        {
            string id = (string)conceptSchemeElem.Attribute(ATTR_ID);
            if (id == null)
            {
                return null;
            }
            ConceptScheme conceptSchemeModel = new ConceptScheme();
            conceptSchemeModel.Id = id;
            conceptSchemeModel.Title = (string)conceptSchemeElem.Element(r + TAG_LABEL);
            conceptSchemeModel.Memo = (string)conceptSchemeElem.Element(r + TAG_DESCRIPTION);

            IEnumerable<XElement> elements = conceptSchemeElem.Descendants(c + TAG_CONCEPT);
            foreach (XElement conceptElem in elements)
            {
                string conceptId = (string)conceptElem.Attribute(ATTR_ID);
                if (conceptId == null)
                {
                    continue;
                }
                Concept conceptModel = new Concept();
                conceptModel.Id = conceptId;
                conceptModel.Title = (string)conceptElem.Element(r + TAG_LABEL);
                conceptModel.Content = (string)conceptElem.Element(r + TAG_DESCRIPTION);
                conceptSchemeModel.Concepts.Add(conceptModel);
            }
            return conceptSchemeModel;
        }

        public static void CreateConceptSchemes(XElement studyUnitElem, StudyUnit studyUnit)
        {
            XElement conceptualComponentElem = studyUnitElem.Element(c + TAG_CONCEPTUAL_COMPONENT);
            if (conceptualComponentElem == null)
            {
                return;
            }
            string conceptualComponentId = (string)conceptualComponentElem.Attribute(ATTR_ID);
            if (conceptualComponentId == null)
            {
                return;
            }
            List<ConceptScheme> conceptSchemeModels = new List<ConceptScheme>();
            IEnumerable<XElement> elements = conceptualComponentElem.Elements(c + TAG_CONCEPT_SCHEME);
            foreach (XElement conceptSchemeElem in elements)
            {
                ConceptScheme conceptSchemeModel = CreateConceptScheme(conceptSchemeElem);
                if (conceptSchemeModel != null)
                {
                    conceptSchemeModels.Add(conceptSchemeModel);
                }
            }
            studyUnit.ConceptualComponentId = conceptualComponentId;
            if (conceptSchemeModels.Count > 0)
            {
                studyUnit.ConceptSchemes = conceptSchemeModels;
            }

            XElement geographicStructureSchemeElem = conceptualComponentElem.Element(c + TAG_GEOGRAPHIC_STRUCTURE_SCHEME);
            if (geographicStructureSchemeElem != null)
            {
                studyUnit.GeographicStructureSchemeId = (string)geographicStructureSchemeElem.Attribute(ATTR_ID);
            }
        }

        private static List<MissingValue> CreateMissingValues(XElement responseElem)
        {
            List<MissingValue> missingValues = new List<MissingValue>();
            string missingValueStr = (string)responseElem.Attribute(ATTR_MISSING_VALUE);
            if (string.IsNullOrEmpty(missingValueStr))
            {
                return missingValues;
            }
            string[] missingValueStrs = missingValueStr.Split(' ');
            foreach (string str in missingValueStrs)
            {
                MissingValue missingValue = new MissingValue() { Content = str };
                missingValues.Add(missingValue);
            }
            return missingValues;
        }

        public static Question CreateQuestion(XElement questionElem)
        {
            string id = (string)questionElem.Attribute(ATTR_ID);
            if (id == null)
            {
                return null;
            }
            Question question = new Question();
            question.Id = id;

            //Title
            question.Title = (string)questionElem.Element(d + TAG_QUESTION_ITEM_NAME);
            //text
            XElement questionTextElem = questionElem.Element(d + TAG_QUESTION_TEXT);
            if (questionTextElem != null)
            {
                XElement literalTextElem = questionTextElem.Descendants(d + TAG_TEXT).FirstOrDefault();
                if (literalTextElem != null)
                {
                    question.Text = literalTextElem.Value;
                }
            }
            //Concept
            string conceptId = ReadReferenceID(questionElem, d + TAG_CONCEPT_REFERENCE);
            question.ConceptId = conceptId;

            //Item
            XElement codeDomainElem = questionElem.Element(d + TAG_CODE_DOMAIN);
            XElement numericDomainElem = questionElem.Element(d + TAG_NUMERIC_DOMAIN);
            XElement textDomainElem = questionElem.Element(d + TAG_TEXT_DOMAIN);
            XElement dateTimeDomain = questionElem.Element(d + TAG_DATE_TIME_DOMAIN);
            if (codeDomainElem != null)
            {
                question.Response.TypeCode = Options.RESPONSE_TYPE_CHOICES_CODE;
                question.Response.Title = (string)codeDomainElem.Element(d + TAG_LABEL);
                question.Response.CodeSchemeId = ReadReferenceID(codeDomainElem, r + TAG_CODE_SCHEME_REFERENCE);

            } else if (numericDomainElem != null)
            {
                //Numerical answers
                question.Response.TypeCode = Options.RESPONSE_TYPE_NUMBER_CODE;
                question.Response.Title = (string)numericDomainElem.Element(d + TAG_LABEL);
                string numericTypeLabel = (string)numericDomainElem.Attribute(ATTR_TYPE);
                question.Response.DetailTypeCode = Options.NumberTypeCode(numericTypeLabel);
                XElement numerRangeElem = numericDomainElem.Element(r + TAG_NUMBER_RANGE);
                if (numerRangeElem != null)
                {
                    question.Response.Min = (decimal?)numerRangeElem.Element(r + TAG_LOW);
                    question.Response.Max = (decimal?)numerRangeElem.Element(r + TAG_HIGH);
                }
                question.Response.MissingValues = CreateMissingValues(numericDomainElem);
            } else if (textDomainElem != null)
            {
                //Free-text answers
                question.Response.TypeCode = Options.RESPONSE_TYPE_FREE_CODE;
                question.Response.Title = (string)textDomainElem.Element(r + TAG_LABEL);
                question.Response.Min = (decimal?)textDomainElem.Attribute(ATTR_MIN_LENGTH);
                question.Response.Max = (decimal?)textDomainElem.Attribute(ATTR_MAX_LENGTH);
                question.Response.MissingValues = CreateMissingValues(textDomainElem);
            }
            else if (dateTimeDomain != null)
            {
                //Date answers
                question.Response.TypeCode = Options.RESPONSE_TYPE_DATETIME_CODE;
                question.Response.Title = (string)dateTimeDomain.Element(d + TAG_LABEL);
                string typeLabel = (string)dateTimeDomain.Attribute(ATTR_TYPE);
                question.Response.DetailTypeCode = Options.DateTimeTypeCode(typeLabel);
                question.Response.MissingValues = CreateMissingValues(dateTimeDomain);
            }
            else
            {
                return null;
            }
            return question;
        }

        public static QuestionGroup CreateQuestionGroup(XElement questionGroupElem, List<Question> questionModels) 
        {
            string id = (string)questionGroupElem.Attribute(ATTR_ID);
            if (id == null)
            {
                return null;
            }
            QuestionGroup questionGroup = new QuestionGroup();
            questionGroup.Id = id;

            //Title
            questionGroup.Title = (string)questionGroupElem.Element(d + TAG_MULTIPLE_QUESTION_ITEM_NAME);
            //Description
            XElement questionTextElem = questionGroupElem.Element(d + TAG_QUESTION_TEXT);
            if (questionTextElem != null)
            {
                XElement literalTextElem = questionTextElem.Descendants(d + TAG_TEXT).FirstOrDefault();
                if (literalTextElem != null)
                {
                    questionGroup.Memo = literalTextElem.Value;
                }
            }

            IEnumerable<XElement> elements = questionGroupElem.Descendants(d + TAG_QUESTION_ITEM);
            foreach (XElement questionElem in elements)
            {
                Question questionModel = CreateQuestion(questionElem);
                if (questionModel != null)
                {
                    questionModels.Add(questionModel);
                }
            }

            foreach (Question questionModel in questionModels)
            {
                questionGroup.QuestionIds.Add(questionModel.Id);
            }
            return questionGroup;
        }

        public static void CreateQuestions(XElement studyUnitElem, StudyUnit studyUnit)
        {
            XElement questionSchemeElem = studyUnitElem.Descendants(d + TAG_QUESTION_SCHEME).FirstOrDefault();
            if (questionSchemeElem == null)
            {
                return;
            }
            string questionSchemeId = (string)questionSchemeElem.Attribute(ATTR_ID);
            List<Question> questionModels = new List<Question>();
            IEnumerable<XElement> elements = questionSchemeElem.Elements(d + TAG_QUESTION_ITEM);
            foreach (XElement questionItem in elements)
            {
                Question questionModel = CreateQuestion(questionItem);
                if (questionModel != null)
                {
                    questionModels.Add(questionModel);
                }
            }
            List<QuestionGroup> questionGroupModels = new List<QuestionGroup>();
            elements = questionSchemeElem.Elements(d + TAG_MULTIPLE_QUESTION_ITEM);
            foreach (XElement questionGroupElem in elements)
            {
                List<Question> questionModelsInGroup = new List<Question>();
                QuestionGroup questionGroupModel = CreateQuestionGroup(questionGroupElem, questionModelsInGroup);
                if (questionModelsInGroup != null)
                {
                    questionGroupModels.Add(questionGroupModel);
                    questionModels.AddRange(questionModelsInGroup);
                }
            }
            studyUnit.QuestionSchemeId = questionSchemeId;
            if (questionModels.Count > 0)
            {
                studyUnit.Questions = questionModels;
            }
            if (questionGroupModels.Count > 0)
            {
                studyUnit.QuestionGroups = questionGroupModels;
            }
        }

        private static bool IsAttrMissingValid(string missing)
        {
            if (string.IsNullOrEmpty(missing))
            {
                return false;
            }
            return missing == "1" || missing.ToLower() == "true";
        }

        public static CategoryScheme CreateCategoryScheme(XElement categorySchemeElem)
        {
            string id = (string)categorySchemeElem.Attribute(ATTR_ID);
            if (id == null)
            {
                return null;
            }
            CategoryScheme categorySchemeModel = new CategoryScheme();
            categorySchemeModel.Id = id;
            categorySchemeModel.Title = (string)categorySchemeElem.Element(r + TAG_LABEL);
            categorySchemeModel.Memo = (string)categorySchemeElem.Element(r + TAG_DESCRIPTION);

            IEnumerable<XElement> elements = categorySchemeElem.Elements(l + TAG_CATEGORY);
            foreach (XElement categoryElem in elements)
            {
                string categoryId = (string)categoryElem.Attribute(ATTR_ID);
                if (categoryId == null)
                {
                    continue;
                }
                Category categoryModel = new Category();
                if (IsAttrMissingValid((string)categoryElem.Attribute(ATTR_MISSING)))
                {
                    categoryModel.IsMissingValue = true;
                }
                categoryModel.Id = categoryId;
                categoryModel.Title = (string)categoryElem.Element(r + TAG_LABEL);
                categoryModel.Memo = (string)categoryElem.Element(r + TAG_DESCRIPTION);
                categorySchemeModel.Categories.Add(categoryModel);
            }
            return categorySchemeModel;
        }

        public static void CreateCategorySchemes(XElement studyUnitElem, StudyUnit studyUnit)
        {
            List<CategoryScheme> categorySchemeModels = new List<CategoryScheme>();
            IEnumerable<XElement> elements = studyUnitElem.Descendants(l + TAG_CATEGORY_SCHEME);
            foreach (XElement categorySchemeElem in elements)
            {
                CategoryScheme categorySchemeModel = CreateCategoryScheme(categorySchemeElem);
                if (categorySchemeModel != null)
                {
                    categorySchemeModels.Add(categorySchemeModel);
                }
            }
            if (categorySchemeModels.Count > 0)
            {
                studyUnit.CategorySchemes = categorySchemeModels;
            }
        }

        public static CodeScheme CreateCodeScheme(XElement codeSchemeElem)
        {
            string id = (string)codeSchemeElem.Attribute(ATTR_ID);
            if (id == null)
            {
                return null;
            }
            CodeScheme codeSchemeModel = new CodeScheme();
            codeSchemeModel.Id = id;
            codeSchemeModel.Title = (string)codeSchemeElem.Element(r + TAG_LABEL);
            codeSchemeModel.Memo = (string)codeSchemeElem.Element(r + TAG_DESCRIPTION);

            IEnumerable<XElement> elements = codeSchemeElem.Elements(l + TAG_CODE);
            foreach (XElement codeElem in elements)
            {
                //string codeId = (string)codeElem.Attribute(ATTR_ID);
                //if (codeId == null)
                //{
                //    continue;
                //}
                Code codeModel = new Code();
                //codeModel.Id = codeId;
                codeModel.CategoryId = ReadReferenceID(codeElem, l + TAG_CATEGORY_REFERENCE);
                codeModel.Value = (string)codeElem.Element(l + TAG_VALUE);
                codeSchemeModel.Codes.Add(codeModel);
            }

            return codeSchemeModel;
        }

        public static void CreateCodeSchemes(XElement studyUnitElem, StudyUnit studyUnit)
        {
            List<CodeScheme> codeSchemeModels = new List<CodeScheme>();
            IEnumerable<XElement> elements = studyUnitElem.Descendants(l + TAG_CODE_SCHEME);
            foreach (XElement codeSchemeElem in elements)
            {
                CodeScheme codeSchemeModel = CreateCodeScheme(codeSchemeElem);
                if (codeSchemeModel != null)
                {
                    codeSchemeModels.Add(codeSchemeModel);
                }
            }
            if (codeSchemeModels.Count > 0)
            {
                studyUnit.CodeSchemes = codeSchemeModels;
            }
        }

        public static Variable CreateVariable(XElement variableElem)
        {
            string id = (string)variableElem.Attribute(ATTR_ID);
            if (id == null)
            {
                return null;
            }
            Variable variable = new Variable();
            variable.Id = id;
            variable.Title = (string)variableElem.Element(l + TAG_VARIABLE_NAME);
            variable.Label = (string)variableElem.Element(r + TAG_LABEL);
            variable.ConceptId = ReadReferenceID(variableElem, l + TAG_CONCEPT_REFERENCE);
            variable.QuestionId = ReadReferenceID(variableElem, l + TAG_QUESTION_REFERENCE);
            variable.UniverseId = ReadReferenceID(variableElem, r + TAG_UNIVERSE_REFERENCE);
            XElement representationElem = variableElem.Element(l + TAG_REPRESENTATION);
            if (representationElem != null)
            {
                //Item
                XElement codeRepresentationElem = representationElem.Element(l + TAG_CODE_REPRESENTATION);
                XElement numericRepresentationElem = representationElem.Element(l + TAG_NUMERIC_REPRESENTATION);
                XElement textRepresentationElem = representationElem.Element(l + TAG_TEXT_REPRESENTATION);
                XElement dateTimeRepresentationElem = representationElem.Element(l + TAG_DATE_TIME_REPRESENTATION);
                if (codeRepresentationElem != null)
                {
                    variable.Response.TypeCode = Options.RESPONSE_TYPE_CHOICES_CODE;
                    variable.Response.CodeSchemeId = ReadReferenceID(codeRepresentationElem, r + TAG_CODE_SCHEME_REFERENCE);

                }
                else if (numericRepresentationElem != null)
                {
                    //Numerical answers
                    variable.Response.TypeCode = Options.RESPONSE_TYPE_NUMBER_CODE;
                    variable.Response.Title = (string)numericRepresentationElem.Element(d + TAG_LABEL);
                    string numericTypeLabel = (string)numericRepresentationElem.Attribute(ATTR_TYPE);
                    variable.Response.DetailTypeCode = Options.NumberTypeCode(numericTypeLabel);
                    XElement numerRangeElem = numericRepresentationElem.Element(r + TAG_NUMBER_RANGE);
                    if (numerRangeElem != null)
                    {
                        variable.Response.Min = (decimal?)numerRangeElem.Element(r + TAG_LOW);
                        variable.Response.Max = (decimal?)numerRangeElem.Element(r + TAG_HIGH);
                    }
                    variable.Response.MissingValues = CreateMissingValues(numericRepresentationElem);
                }
                else if (textRepresentationElem != null)
                {
                    //Free-text answers
                    variable.Response.TypeCode = Options.RESPONSE_TYPE_FREE_CODE;
                    variable.Response.Title = (string)textRepresentationElem.Element(r + TAG_LABEL);
                    variable.Response.Min = (decimal?)textRepresentationElem.Attribute(ATTR_MIN_LENGTH);
                    variable.Response.Max = (decimal?)textRepresentationElem.Attribute(ATTR_MAX_LENGTH);
                    variable.Response.MissingValues = CreateMissingValues(textRepresentationElem);
                }
                else if (dateTimeRepresentationElem != null)
                {
                    //Date answers
                    variable.Response.TypeCode = Options.RESPONSE_TYPE_DATETIME_CODE;
                    variable.Response.Title = (string)dateTimeRepresentationElem.Element(d + TAG_LABEL);
                    string typeLabel = (string)dateTimeRepresentationElem.Attribute(ATTR_TYPE);
                    variable.Response.DetailTypeCode = Options.DateTimeTypeCode(typeLabel);
                    variable.Response.MissingValues = CreateMissingValues(dateTimeRepresentationElem);
                }
                else
                {
                    return null;
                }
            }
            return variable;
        }

        public static void CreateVariables(XElement studyUnitElem, StudyUnit studyUnit)
        {
            List<Variable> variableModels = new List<Variable>();
            IEnumerable<XElement> elements = studyUnitElem.Descendants(l + TAG_VARIABLE);
            foreach (XElement variableElem in elements)
            {
                Variable variableModel = CreateVariable(variableElem);
                if (variableModel != null)
                {
                    Question question = studyUnit.FindQuestion(variableModel.QuestionId);
                    if (question != null)
                    {
                        question.VariableGenerationInfo = question.CreateVariableGenerationInfo();
                    }
                    variableModels.Add(variableModel);
                }
            }
            if (variableModels.Count > 0)
            {
                studyUnit.Variables = variableModels;
            }
        }


        public static DataSet CreateDataSet(XElement logicalRecordElem)
        {
            string id = (string)logicalRecordElem.Attribute(ATTR_ID);
            if (id == null)
            {
                return null;
            }
            DataSet dataSetModel = new DataSet();
            dataSetModel.Id = id;
            dataSetModel.Title = (string)logicalRecordElem.Element(l + TAG_LOGICAL_RECORD_NAME);
            dataSetModel.Memo = (string)logicalRecordElem.Element(r + TAG_DESCRIPTION);

            IEnumerable<XElement> elements = logicalRecordElem.Descendants(l + TAG_VARIABLE_USED_REFERENCE);
            foreach (XElement variableInRecordElem in elements)
            {
                string variableId = (string)variableInRecordElem.Element(r + TAG_ID);
                if (variableId != null)
                {
                    dataSetModel.VariableGuids.Add(variableId);
                }
            }
            return dataSetModel;
        }

        public static void CreateDataSets(XElement studyUnitElem, StudyUnit studyUnit)
        {
            XElement logicalProductElem = studyUnitElem.Element(l + TAG_LOGICAL_PRODUCT);
            if (logicalProductElem == null)
            {
                return;
            }
            string logicalProductId = (string)logicalProductElem.Attribute(ATTR_ID);
            if (logicalProductId == null)
            {
                return;
            }
            XElement dataRelationshipElem = logicalProductElem.Element(l + TAG_DATA_RELATIONSHIP);
            if (dataRelationshipElem == null)
            {
                return;
            }
            string dataRelationShipId = (string)dataRelationshipElem.Attribute(ATTR_ID);
            if (dataRelationShipId == null)
            {
                return;
            }
            List<DataSet> dataSetModels = new List<DataSet>();
            IEnumerable<XElement> elements = dataRelationshipElem.Elements(l + TAG_LOGICAL_RECORD);
            foreach (XElement logicalRecordElem in elements)
            {
                DataSet dataSetModel = CreateDataSet(logicalRecordElem);
                if (dataSetModel != null)
                {
                    dataSetModels.Add(dataSetModel);
                }
            }
            studyUnit.LogicalProductId = logicalProductId;
            studyUnit.DataCollectionId = dataRelationShipId;
            if (dataSetModels.Count > 0)
            {
                studyUnit.DataSets = dataSetModels;
            }
        }

        public static XElement FindRecordLayoutElem(IEnumerable<XElement> recordLayoutElems, string id)
        {
            foreach (XElement recordLayoutElem in recordLayoutElems)
            {
                string physicalStructureReferenceId = ReadReferenceID(recordLayoutElem, p + TAG_PHYSICAL_STRUCTURE_REFERENCE);
                if (physicalStructureReferenceId == id)
                {
                    return recordLayoutElem;
                }
            }
            return null;
        }

        public static XElement FindPhysicalInstanceElem(IEnumerable<XElement> physicalInstanceElems, string id)
        {
            foreach (XElement physicalInstanceElem in physicalInstanceElems)
            {
                string recordLayoutId = ReadReferenceID(physicalInstanceElem, pi + TAG_RECORD_LAYOUT_REFERENCE);
                if (recordLayoutId == id)
                {
                    return physicalInstanceElem;
                }
            }
            return null;
        }

        public static DataFile CreateDataFile(XElement physicalStructureElem, IEnumerable<XElement> recordLayoutElems, IEnumerable<XElement> physicalInstanceElems)
        {
            string id = (string)physicalStructureElem.Attribute(ATTR_ID);
            if (id == null)
            {
                return null;
            }
            DataFile dataFileModel = new DataFile();
            dataFileModel.Id = id;
            dataFileModel.Format = (string)physicalStructureElem.Element(p + TAG_FORMAT);
            string delimiterLabel = (string)physicalStructureElem.Element(p + TAG_DEFAULT_DELIMITER);
            dataFileModel.DelimiterCode = Options.FindCodeByLabel(Options.Delimiters, delimiterLabel);
            dataFileModel.DataSetId = ReadReferenceID(physicalStructureElem, p + TAG_LOGICAL_RECORD_REFERENCE);

            XElement recordLayoutElem = FindRecordLayoutElem(recordLayoutElems, dataFileModel.Id);
            if (recordLayoutElem != null)
            {
                string recordLayoutId = (string)recordLayoutElem.Attribute(ATTR_ID);
                XElement physicalInstanceElem = FindPhysicalInstanceElem(physicalInstanceElems, recordLayoutId);
                if (physicalInstanceElem != null)
                {
                    XElement dataFileIdentificationElem = physicalInstanceElem.Element(pi + TAG_DATA_FILE_IDENTIFICATION);
                    if (dataFileIdentificationElem != null)
                    {
                        dataFileModel.Uri = (string)dataFileIdentificationElem.Element(pi + TAG_URI);
                    }
                }
            }
            return dataFileModel;
        }

        public static void CreateDataFiles(XElement studyUnitElem, StudyUnit studyUnit)
        {
            XElement physicalDataProductElem = studyUnitElem.Element(p + TAG_PHYSICAL_DATA_PRODUCT);
            if (physicalDataProductElem == null)
            {
                return;
            }
            string physicalDataProductId = (string)physicalDataProductElem.Attribute(ATTR_ID);
            if (physicalDataProductId == null)
            {
                return;
            }
            XElement physicalStructureSchemeElem = physicalDataProductElem.Element(p + TAG_PHYSICAL_STRUCTURE_SCHEME);
            if (physicalStructureSchemeElem == null)
            {
                return;
            }
            string physicalStructureSchemeId = (string)physicalStructureSchemeElem.Attribute(ATTR_ID);
            if (physicalStructureSchemeId == null)
            {
                return;
            }
            XElement recordLayoutSchemeElem = physicalDataProductElem.Element(p + TAG_RECORD_LAYOUT_SCHEME);
            if (recordLayoutSchemeElem == null)
            {
                return;
            }
            string recordLayoutSchemeId = (string)recordLayoutSchemeElem.Attribute(ATTR_ID);
            if (recordLayoutSchemeId == null)
            {
                return;
            }

            List<DataFile> dataFileModels = new List<DataFile>();
            IEnumerable<XElement> physicalStructureElems = physicalStructureSchemeElem.Elements(p + TAG_PHYSICAL_STRUCTURE);
            IEnumerable<XElement> recordLayoutElems = recordLayoutSchemeElem.Descendants(p + TAG_RECORD_LAYOUT);
            IEnumerable<XElement> physicalInstanceElems = studyUnitElem.Descendants(pi + TAG_PHYSICAL_INSTANCE);
            foreach (XElement physicalStructureElem in physicalStructureElems)
            {
                DataFile dataFileModel = CreateDataFile(physicalStructureElem, recordLayoutElems, physicalInstanceElems);
                if (dataFileModel != null)
                {
                    dataFileModels.Add(dataFileModel);
                }
            }

            studyUnit.PhysicalDataProductId = physicalDataProductId;
            studyUnit.PhysicalStructureSchemeId = physicalStructureSchemeId;
            studyUnit.RecordLayoutSchemeId = recordLayoutSchemeId;
            if (dataFileModels.Count > 0)
            {
                studyUnit.DataFiles = dataFileModels;
            }
        }

        private static StatisticsInfo CreateNumerStatisticsInfo(XElement variableStatisticsElem, Variable variable)
        {
            StatisticsInfo statisticsInfo = new StatisticsInfo();
            statisticsInfo.Scale = variable.Response.Scale;
            statisticsInfo.VariableId = variable.Id;
            statisticsInfo.StatisticsType = StatisticsTypes.Number;

            IEnumerable<XElement> summaryStatisticElems = variableStatisticsElem.Elements(pi + TAG_SUMMARY_STATISTIC);
            foreach (XElement summaryStatisticElem in summaryStatisticElems)
            {
                string typeName = (string)summaryStatisticElem.Element(pi + TAG_SUMMARY_STATISTIC_TYPE);
                string value = (string)summaryStatisticElem.Element(pi + TAG_VALUE);
                if (typeName == DDI_MEAN)
                {
                    statisticsInfo.SummaryInfo.Mean = StatisticsUtils.ToDouble(value);
                }
                else if (typeName == DDI_MEDIAN)
                {
                    statisticsInfo.SummaryInfo.Median = StatisticsUtils.ToDouble(value);
                }
                else if (typeName == DDI_VALID_CASES)
                {
                    statisticsInfo.SummaryInfo.ValidCases = StatisticsUtils.ToInt(value);
                }
                else if (typeName == DDI_INVALID_CASES)
                {
                    statisticsInfo.SummaryInfo.InvalidCases = StatisticsUtils.ToInt(value);
                }
                else if (typeName == DDI_MINIMUM)
                {
                    statisticsInfo.SummaryInfo.Minimum = StatisticsUtils.ToDouble(value);
                }
                else if (typeName == DDI_MAXIMUM)
                {
                    statisticsInfo.SummaryInfo.Maximum = StatisticsUtils.ToDouble(value);
                }
                else if (typeName == DDI_STANDARD_DEVIATION)
                {
                    statisticsInfo.SummaryInfo.StandardDeviation = StatisticsUtils.ToDouble(value);
                }
            }
            return statisticsInfo;
        }


        private static CategoryInfo CreateSingleAnswerCategoryInfo(XElement categoryStatisticsElem, List<Category> categories, List<Code> codes)
        {
            IEnumerable<XElement> categoryStatisticElems = categoryStatisticsElem.Elements(pi + TAG_CATEGORY_STATISTIC);
            int frequency = 0;
            decimal percent = 0m;
            foreach (XElement categoryStatisticElem in categoryStatisticElems)
            {
                string typeName = (string)categoryStatisticElem.Element(pi + TAG_CATEGORY_STATISTIC_TYPE);
                string value = (string)categoryStatisticElem.Element(pi + TAG_VALUE);
                if (typeName == DDI_FREQUENCY)
                {
                    frequency = StatisticsUtils.ToInt(value);
                }
                else if (typeName == DDI_PERCENT)
                {
                    percent = StatisticsUtils.ToDecimal(value);
                }
            }
            string codeValue = (string)categoryStatisticsElem.Element(pi + TAG_CATEGORY_VALUE);
            Category category = DDIUtils.FindCategoryByCodeValue(categories, codes, codeValue);
            CategoryInfo categoryInfo = new CategoryInfo();
            categoryInfo.CategoryType = CategoryHelper.GeSingleAnswerCategoryType(codeValue, category);
            categoryInfo.Frequency = frequency;
            categoryInfo.Percent = percent;
            categoryInfo.CodeValue = codeValue;
            categoryInfo.CategoryTitle = CategoryHelper.GeSingleAnswerCategoryTitle(codeValue, category);
            return categoryInfo;
        }

        private static StatisticsInfo CreateSingleAnswerStatisticsInfo(XElement variableStatisticsElem, Variable variable, List<Category> categories, List<Code> codes)
        {
            StatisticsInfo statisticsInfo = new StatisticsInfo();
            statisticsInfo.Scale = variable.Response.Scale;
            statisticsInfo.VariableId = variable.Id;
            statisticsInfo.StatisticsType = StatisticsTypes.ChoicesSingleAnswer;

            IEnumerable<XElement> categoryStatisticsElems = variableStatisticsElem.Elements(pi + TAG_CATEGORY_STATISTICS);
            foreach (XElement categoryStatisticsElem in categoryStatisticsElems)
            {
                CategoryInfo categoryInfo = CreateSingleAnswerCategoryInfo(categoryStatisticsElem, categories, codes);
                statisticsInfo.CategoryInfos.Add(categoryInfo);
            }
            statisticsInfo.CalcPercents();
            return statisticsInfo;
        }

        private static CategoryInfo CreateDateTimeCategoryInfo(XElement categoryStatisticsElem, List<MissingValue> missingValues)
        {
            IEnumerable<XElement> categoryStatisticElems = categoryStatisticsElem.Elements(pi + TAG_CATEGORY_STATISTIC);
            int frequency = 0;
            decimal percent = 0m;
            foreach (XElement categoryStatisticElem in categoryStatisticElems)
            {
                string typeName = (string)categoryStatisticElem.Element(pi + TAG_CATEGORY_STATISTIC_TYPE);
                string value = (string)categoryStatisticElem.Element(pi + TAG_VALUE);
                if (typeName == DDI_FREQUENCY)
                {
                    frequency = StatisticsUtils.ToInt(value);
                }
                else if (typeName == DDI_PERCENT)
                {
                    percent = StatisticsUtils.ToDecimal(value);
                }
            }
            string codeValue = (string)categoryStatisticsElem.Element(pi + TAG_CATEGORY_VALUE);
            CategoryInfo categoryInfo = new CategoryInfo();
            categoryInfo.CategoryType = CategoryHelper.GetDateTimeCategoryType(codeValue, missingValues);
            categoryInfo.Frequency = frequency;
            categoryInfo.Percent = percent;
            categoryInfo.CodeValue = codeValue;
            categoryInfo.CategoryTitle = CategoryHelper.GetDateTimeCategoryTitle(codeValue);
            return categoryInfo;
        }
        private static StatisticsInfo CreateDateTimeStatisticsInfo(XElement variableStatisticsElem, Variable variable)
        {
            StatisticsInfo statisticsInfo = new StatisticsInfo();
            statisticsInfo.Scale = variable.Response.Scale;
            statisticsInfo.VariableId = variable.Id;
            statisticsInfo.StatisticsType = StatisticsTypes.DateTime;

            IEnumerable<XElement> categoryStatisticsElems = variableStatisticsElem.Elements(pi + TAG_CATEGORY_STATISTICS);
            foreach (XElement categoryStatisticsElem in categoryStatisticsElems)
            {
                CategoryInfo categoryInfo = CreateDateTimeCategoryInfo(categoryStatisticsElem, variable.Response.MissingValues);
                statisticsInfo.CategoryInfos.Add(categoryInfo);
            }
            statisticsInfo.CalcPercents();
            return statisticsInfo;
        }

        private static void CreateStatisticsInfos(XElement studyUnitElem, StudyUnit studyUnit)
        {
            XElement physicalInstanceElem = studyUnitElem.Element(pi + TAG_PHYSICAL_INSTANCE);
            if (physicalInstanceElem == null)
            {
                return;
            }
            XElement statisticsElem = physicalInstanceElem.Element(pi + TAG_STATISTICS);
            if (statisticsElem == null)
            {
                return;
            }
            List<StatisticsInfo> statisticsInfos = new List<StatisticsInfo>();
            IEnumerable<XElement> variableStatisticsElems = statisticsElem.Elements(pi + TAG_VARIABLE_STATISTICS);
            List<Category> allCategories = studyUnit.AllCategories;
            foreach (XElement variableStatisticsElem in variableStatisticsElems)
            {
                string variableId = ReadReferenceID(variableStatisticsElem, pi + TAG_VARIABLE_REFERENCE);
                Variable variable = studyUnit.FindVariable(variableId);
                if (variable == null)
                {
                    continue;
                }
                StatisticsInfo statisticsInfo = null;
                if (variable.Response.IsTypeNumber)
                {
                    statisticsInfo = CreateNumerStatisticsInfo(variableStatisticsElem, variable);
                } else if (variable.Response.IsTypeChoices)
                {
                    List<Code> codes = studyUnit.FindCodes(variable.Response.CodeSchemeId);
                    statisticsInfo = CreateSingleAnswerStatisticsInfo(variableStatisticsElem, variable, allCategories, codes);
                } else if (variable.Response.IsTypeDateTime)
                {
                    statisticsInfo = CreateDateTimeStatisticsInfo(variableStatisticsElem, variable);
                }
                if (statisticsInfo != null)
                {
                    statisticsInfos.Add(statisticsInfo);
                }
            }
            studyUnit.StatisticsInfos.AddRange(statisticsInfos);
        }

        private static BookRelationType GetBookRelationType(StudyUnit studyUnit, string id)
        {
            Concept concept = studyUnit.FindConcept(id);
            if (concept != null)
            {
                return BookRelationType.Concept;
            }
            Question question = studyUnit.FindQuestion(id);
            if (question != null)
            {
                return BookRelationType.Question;
            }
            Variable variable = studyUnit.FindVariable(id);
            if (variable != null)
            {
                return BookRelationType.Variable;
            }
            return BookRelationType.Abstract;
        }

        private static Book CreateBook(XElement otherMaterialElem, StudyUnit studyUnit)
        {
            string bookId = (string)otherMaterialElem.Attribute(ATTR_ID);
            if (bookId == null)
            {
                return null;
            }
            string type = (string)otherMaterialElem.Attribute(ATTR_TYPE);
            if (type == null)
            {
                return null;
            }

            XElement citationElem = otherMaterialElem.Element(r + TAG_CITATION);
            if (citationElem == null)
            {
                return null;
            }

            string title = (string)citationElem.Element(r + TAG_TITLE);
            if (title == null)
            {
                return null;
            }
            Book bookModel = new Book();
            bookModel.BookTypeCode = GetBookTypeCode(type);
            bookModel.Id = bookId;
            bookModel.Title = title;

            bookModel.Author = (string)citationElem.Element(r + TAG_CREATOR);
            // Set Publisher and City
            ParsePublisher((string)citationElem.Element(r + TAG_PUBLISHER), bookModel);
            bookModel.Editor = (string)citationElem.Element(r + TAG_CONTRIBUTOR);
            bookModel.AnnouncementDate = ReadDateUnitAsString(citationElem, r + TAG_PUBLICATION_DATE);
            bookModel.Language = (string)citationElem.Element(r + TAG_LANGUAGE);


            XElement dcelements = citationElem.Element(dce + TAG_DCELEMENTS);
            if (dcelements != null)
            {
                bookModel.Summary = (string)dcelements.Element(dc + TAG_DC_DESCRIPTION);
                ParseIdentifier((string)dcelements.Element(dc + TAG_DC_IDENTIFIER), bookModel);
            }

            bookModel.Url = (string)otherMaterialElem.Element(r + TAG_EXTERNAL_URL_REFERENCE);
            IEnumerable<XElement> relationshipElems = otherMaterialElem.Elements(r + TAG_RELATIONSHIP);
            foreach (XElement relationshipElem in relationshipElems)
            {
                string id = ReadReferenceID(relationshipElem, r + TAG_RELATED_TO_REFERENCE);
                BookRelation relation = new BookRelation();
                relation.BookRelationType = GetBookRelationType(studyUnit, id);
                if (relation.IsBookRelationTypeAbstract)
                {
                    id = null;
                }
                relation.MetadataId = id;
                bookModel.BookRelations.Add(relation);
            }
            return bookModel;
        }

        private static List<Book> CreateBooksFrom(XElement parentElement, StudyUnit studyUnit)
        {
            List<Book> bookModels = new List<Book>();
            if (parentElement == null)
            {
                return bookModels;  
            }
            IEnumerable<XElement> otherMaterialElems = parentElement.Elements(r + TAG_OTHER_MATERIAL);
            foreach (XElement otherMaterialElem in otherMaterialElems)
            {
                Book bookModel = CreateBook(otherMaterialElem, studyUnit);
                if (bookModel != null)
                {
                    bookModels.Add(bookModel);
                }
            }
            return bookModels;
        }

        public static void CreateBooks(XElement studyUnitElement, StudyUnit studyUnit)
        {
            List<Book> allBooks = new List<Book>();

            //Abstract related(just under StudyUnit)
            List<Book> abstractBooks = CreateBooksFrom(studyUnitElement, studyUnit);
            allBooks.AddRange(abstractBooks);

            //Which is associated with variable concept(below ConceptualComponent)
            XElement conceptualComponentElem = studyUnitElement.Element(c + TAG_CONCEPTUAL_COMPONENT);
            List<Book> conceptBooks = CreateBooksFrom(conceptualComponentElem, studyUnit);
            allBooks.AddRange(conceptBooks);

            //Question related
            XElement dataCollectionElem = studyUnitElement.Element(d + TAG_DATA_COLLECTION);
            List<Book> questionBooks = CreateBooksFrom(dataCollectionElem, studyUnit);
            allBooks.AddRange(questionBooks);

            //Which is associated with variable
            XElement logicalProductElem = studyUnitElement.Element(l + TAG_LOGICAL_PRODUCT);
            List<Book> variableBooks = CreateBooksFrom(logicalProductElem, studyUnit);
            allBooks.AddRange(variableBooks);


            List<Book> uniqBooks = new List<Book>();
            foreach (Book book in allBooks)
            {
                Book existBook = Book.FindByTitle(uniqBooks, book.Title);
                if (existBook == null)
                {
                    uniqBooks.Add(book);
                }
            }

            if (uniqBooks.Count > 0)
            {
                studyUnit.Books = uniqBooks;
            }
        }

        public static ControlConstructScheme CreateControlConstructScheme(XElement controlConstructSchemeElem, StudyUnit studyUnit)
        {
            string id = (string)controlConstructSchemeElem.Attribute(ATTR_ID);
            if (id == null)
            {
                return null;
            }
            ControlConstructScheme controlConstructSchemeModel = new ControlConstructScheme();
            controlConstructSchemeModel.Title = (string)controlConstructSchemeElem.Element(d + TAG_CONTROL_CONSTRUCT_SCHEME_NAME);
            IEnumerable<XElement> questionConstructElems = controlConstructSchemeElem.Elements(d + TAG_QUESTION_CONSTRUCT);
            foreach (XElement questionConstructElem in questionConstructElems)
            {
                string questionConstructId = (string)questionConstructElem.Attribute(ATTR_ID);
                if (questionConstructId == null)
                {
                    continue;
                }
                string questionId = ReadReferenceID(questionConstructElem, d + TAG_QUESTION_REFERENCE);
                if (questionId == null)
                {
                    continue;
                }
                string no = (string)questionConstructElem.Element(r + TAG_LABEL);
                if (no == null)
                {
                    continue;
                }
                if (studyUnit.FindQuestion(questionId) != null)
                {
                    QuestionConstruct questionConstruct = new QuestionConstruct();
                    questionConstruct.Id = questionConstructId;
                    questionConstruct.No = no;
                    questionConstruct.QuestionId = questionId;
                    controlConstructSchemeModel.QuestionConstructs.Add(questionConstruct);

                }
                else if (studyUnit.FindQuestionGroup(questionId) != null)
                {
                    QuestionGroupConstruct questionGroupConstruct = new QuestionGroupConstruct();
                    questionGroupConstruct.Id = questionConstructId;
                    questionGroupConstruct.No = no;
                    questionGroupConstruct.QuestionGroupId = questionId;
                    controlConstructSchemeModel.QuestionGroupConstructs.Add(questionGroupConstruct);
                }

            }

            IEnumerable<XElement> statementItemElems = controlConstructSchemeElem.Elements(d + TAG_STATEMENT_ITEM);
            foreach (XElement statementItemElem in statementItemElems)
            {
                string statementId = (string)statementItemElem.Attribute(ATTR_ID);
                if (statementId == null)
                {
                    continue;
                }
                string no = (string)statementItemElem.Attribute(r + TAG_LABEL);
                if (no == null)
                {
                    continue;
                }
                Statement statement = new Statement();
                statement.Id = statementId;
                statement.No = no;
                XElement textElem = statementItemElem.Descendants(d + TAG_TEXT).FirstOrDefault();
                if (textElem != null)
                {
                    statement.Text = textElem.Value;
                }
                controlConstructSchemeModel.Statements.Add(statement);
            }

            IEnumerable<XElement> ifThenElseElems = controlConstructSchemeElem.Elements(d + TAG_IF_THEN_ELSE);
            foreach (XElement ifThenElseElem in ifThenElseElems)
            {
                string ifThenElseId = (string)ifThenElseElem.Attribute(ATTR_ID);
                if (ifThenElseId == null)
                {
                    continue;
                }
                XElement ifConditionElem = ifThenElseElem.Element(d + TAG_IF_CONDITION);
                if (ifConditionElem == null)
                {
                    continue;
                }
                string thenConstructId = ReadReferenceID(ifThenElseElem, d + TAG_THEN_CONSTRUCT_REFERENCE);
                if (thenConstructId == null)
                {
                    continue;
                }

                IfThenElse ifThenElse = new IfThenElse();
                ifThenElse.Id = ifThenElseId;
                ifThenElse.No = ControlConstructScheme.IFTHENELSE_NO;
                ifThenElse.IfCondition.Code = (string)ifConditionElem.Element(r + TAG_CODE);
                ifThenElse.IfCondition.QuestionId = ReadReferenceID(ifConditionElem, r + TAG_SOURCE_QUESTION_REFERENCE);
                ifThenElse.ThenConstructId = thenConstructId;
                controlConstructSchemeModel.IfThenElses.Add(ifThenElse);

                IEnumerable<XElement> elseIfElems = ifThenElseElem.Elements(d + TAG_ELSE_IF);
                foreach (XElement elseIfElem in elseIfElems)
                {
                    XElement ifConditionElem2 = elseIfElem.Element(d + TAG_IF_CONDITION);
                    if (ifConditionElem2 == null)
                    {
                        continue;
                    }
                    string thenConstructId2 = ReadReferenceID(elseIfElem, d + TAG_THEN_CONSTRUCT_REFERENCE);
                    if (thenConstructId2 == null)
                    {
                        continue;
                    }
                    ElseIf elseIf = new ElseIf();
                    elseIf.IfCondition.Code = (string)ifConditionElem2.Element(r + TAG_CODE);
                    elseIf.IfCondition.QuestionId = ReadReferenceID(ifConditionElem2, r + TAG_SOURCE_QUESTION_REFERENCE);
                    elseIf.ThenConstructId = thenConstructId2;
                    ifThenElse.ElseIfs.Add(elseIf);
                }
            }

            XElement sequenceElem = controlConstructSchemeElem.Element(d + TAG_SEQUENCE);
            if (sequenceElem != null)
            {
                controlConstructSchemeModel.Sequence.Id = (string)sequenceElem.Attribute(ATTR_ID);
                IEnumerable<XElement> controlConstructReferenceElems = sequenceElem.Elements(d + TAG_CONTROL_CONSTRUCT_REFERENCE);
                foreach (XElement controlConstructReferenceElem in controlConstructReferenceElems)
                {
                    string controlConstructId = (string)controlConstructReferenceElem.Element(r + TAG_ID);
                    if (controlConstructId != null)
                    {
                        controlConstructSchemeModel.Sequence.ControlConstructIds.Add(controlConstructId);
                    }
                }
            }
            return controlConstructSchemeModel;
        }

        public static void CreateControlConstructSchemes(XElement studyUnitElem, StudyUnit studyUnit)
        {
            List<ControlConstructScheme> controlConstructSchemeModels = new List<ControlConstructScheme>();
            IEnumerable<XElement> elements = studyUnitElem.Descendants(d + TAG_CONTROL_CONSTRUCT_SCHEME);
            foreach (XElement controlConstructSchemeElem in elements)
            {
                ControlConstructScheme controlConstructSchemeModel = CreateControlConstructScheme(controlConstructSchemeElem, studyUnit);
                if (controlConstructSchemeModel != null)
                {
                    controlConstructSchemeModels.Add(controlConstructSchemeModel);
                }
            }
            if (controlConstructSchemeModels.Count > 0)
            {
                studyUnit.ControlConstructSchemes = controlConstructSchemeModels;
            }
        }

        /// <summary>
        /// StudyUnitノードのXElementを受け取り、StudyUnitインスタンスに変換して返す。
        /// </summary>
        /// <param name="studyUnitElement">StudyUnitノードのXElement</param>
        /// <returns>StudyUnitインスタンス</returns>
        private static StudyUnit CreateStudyUnit(XElement studyUnitElement)
        {
            //Read StudyUnit
            StudyUnit studyUnit = StudyUnit.CreateDefault(true);

            //Events
            DDI3Reader.CreateEvents(studyUnitElement, studyUnit);

            //Organization
            DDI3Reader.CreateOrganizations(studyUnitElement, studyUnit);

            //Study Member
            DDI3Reader.CreateMembers(studyUnitElement, studyUnit);

            //Abstract
            DDI3Reader.CreateAbstract(studyUnitElement, studyUnit);
 
            //Coverage
            DDI3Reader.CreateCoverage(studyUnitElement, studyUnit);

            //Funding Agency
            DDI3Reader.CreateFundingInfos(studyUnitElement, studyUnit);

            //Sampling(should be loaded before Universe)
            DDI3Reader.CreateSampling(studyUnitElement, studyUnit);

            //Universe
            DDI3Reader.CreateUniverses(studyUnitElement, studyUnit);

            //Concept
            DDI3Reader.CreateConceptSchemes(studyUnitElement, studyUnit);

            //Question
            DDI3Reader.CreateQuestions(studyUnitElement, studyUnit);

            //Category
            DDI3Reader.CreateCategorySchemes(studyUnitElement, studyUnit);

            //Code
            DDI3Reader.CreateCodeSchemes(studyUnitElement, studyUnit);

            //Sequence
            DDI3Reader.CreateControlConstructSchemes(studyUnitElement, studyUnit);

            //Variable
            DDI3Reader.CreateVariables(studyUnitElement, studyUnit);

            //Dataset
            DDI3Reader.CreateDataSets(studyUnitElement, studyUnit);

            //Data File
            DDI3Reader.CreateDataFiles(studyUnitElement, studyUnit);

            //Statistics Info
            DDI3Reader.CreateStatisticsInfos(studyUnitElement, studyUnit);

            //Related materials
            DDI3Reader.CreateBooks(studyUnitElement, studyUnit);

            return studyUnit;
        }

        #endregion

        #region Read Group

        private static EDOModel CreateSingleModel(XElement studyUnitElem)
        {
            EDOModel model = new EDOModel();
            StudyUnit studyUnit = CreateStudyUnit(studyUnitElem);
            if (studyUnit != null)
            {
                model.StudyUnits.Add(studyUnit);
            }
            return model;
        }

        private static CompareItem CreateConceptSchemeCompareItem(XElement conceptMapElem, List<StudyUnit> studyUnits)
        {
            string sourceSchemeId = ReadReferenceID(conceptMapElem, cm + TAG_SOURCE_SCHEME_REFERENCE);
            if (sourceSchemeId == null) {
                return null;
            }
            StudyUnit sourceStudyUnit = StudyUnit.FindByConceptSchemeId(studyUnits, sourceSchemeId);
            if (sourceStudyUnit == null)
            {
                return null;
            }
            ConceptScheme sourceConceptScheme = sourceStudyUnit.FindConceptScheme(sourceSchemeId);

            string targetSchemeId = ReadReferenceID(conceptMapElem, cm + TAG_TARGET_SCHEME_REFERENCE);
            if (targetSchemeId == null) 
            {
                return null;
            }
            StudyUnit targetStudyUnit = StudyUnit.FindByConceptSchemeId(studyUnits, targetSchemeId);
            if (targetStudyUnit == null)
            {
                return null;
            }
            ConceptScheme targetConceptScheme = targetStudyUnit.FindConceptScheme(targetSchemeId);
            if (targetConceptScheme == null)
            {
                return null;
            }

            XElement correspondenceElem = conceptMapElem.Element(cm + TAG_CORRESPONDENCE);
            if (correspondenceElem == null) 
            {
                return null;
            }

            string weightStr = (string)correspondenceElem.Element(cm + TAG_COMMONALITY_WEIGHT);
            if (string.IsNullOrEmpty(weightStr))
            {
                return null;
            }
            double weight = 0;
            if (!double.TryParse(weightStr, out weight))
            {
                return null;
            }
            bool validWeight = weight > 0;
            if (!validWeight) {
                return null;
            }                        
            string memo = (string)correspondenceElem.Element(cm + TAG_COMMONALITY);
            GroupId sourceId = new GroupId(sourceStudyUnit.Id, sourceSchemeId);
            GroupId targetId = new GroupId(targetStudyUnit.Id, targetSchemeId);
            CompareItem compareItem = new CompareItem(sourceId, targetId, memo, weightStr);
            compareItem.SourceTitle = sourceConceptScheme.Title;
            compareItem.TargetTitle = targetConceptScheme.Title;
            return compareItem;
        }

        private static CompareItem CreateConceptCompareItem(XElement conceptMapElem, List<StudyUnit> studyUnits)
        {
            string rawSourceConceptId = (string)conceptMapElem.Element(cm + TAG_SOURCE_ITEM);
            if (rawSourceConceptId == null)
            {
                return null;
            }
            string sourceConceptId = CompareItem.ToOrigId(rawSourceConceptId);
            StudyUnit sourceStudyUnit = StudyUnit.FindByConceptId(studyUnits, sourceConceptId);
            if (sourceStudyUnit == null)
            {
                return null;
            }
            Concept sourceConcept = sourceStudyUnit.FindConcept(sourceConceptId);

            string rawTargetConceptId = (string)conceptMapElem.Element(cm + TAG_TARGET_ITEM);
            if (rawTargetConceptId == null)
            {
                return null;
            }
            string targetConceptId = CompareItem.ToOrigId(rawTargetConceptId);
            StudyUnit targetStudyUnit = StudyUnit.FindByConceptId(studyUnits, targetConceptId);
            if (targetStudyUnit == null)
            {
                return null;
            }
            Concept targetConcept = targetStudyUnit.FindConcept(targetConceptId);
            if (targetConcept == null)
            {
                return null;
            }

            XElement correspondenceElem = conceptMapElem.Element(cm + TAG_CORRESPONDENCE);
            if (correspondenceElem == null)
            {
                return null;
            }

            string weightStr = (string)correspondenceElem.Element(cm + TAG_COMMONALITY_WEIGHT);
            if (string.IsNullOrEmpty(weightStr))
            {
                return null;
            }
            double weight = 0;
            if (!double.TryParse(weightStr, out weight))
            {
                return null;
            }
            bool validWeight = weight > 0;
            if (!validWeight)
            {
                return null;
            }
            string memo = (string)correspondenceElem.Element(cm + TAG_COMMONALITY);
            GroupId sourceId = new GroupId(sourceStudyUnit.Id, sourceConceptId);
            GroupId targetId = new GroupId(targetStudyUnit.Id, targetConceptId);
            CompareItem compareItem = new CompareItem(sourceId, targetId, memo, weightStr);
            compareItem.SourceTitle = sourceConcept.Title;
            compareItem.TargetTitle = targetConcept.Title;
            return compareItem;
        }

        private static CompareItem CreateVariableCompareItem(XElement variableMapElem, List<StudyUnit> studyUnits)
        {
            string rawVariableId = (string)variableMapElem.Element(cm + TAG_SOURCE_ITEM);
            if (rawVariableId == null)
            {
                return null;
            }
            string sourceVariableId = CompareItem.ToOrigId(rawVariableId);
            StudyUnit sourceStudyUnit = StudyUnit.FindByVariableId(studyUnits, sourceVariableId);
            if (sourceStudyUnit == null)
            {
                return null;
            }
            Variable sourceVariable = sourceStudyUnit.FindVariable(sourceVariableId);

            string rawTargetVariableId = (string)variableMapElem.Element(cm + TAG_TARGET_ITEM);
            if (rawTargetVariableId == null)
            {
                return null;
            }
            string targetVariableId = CompareItem.ToOrigId(rawTargetVariableId);
            StudyUnit targetStudyUnit = StudyUnit.FindByVariableId(studyUnits, targetVariableId);
            if (targetStudyUnit == null)
            {
                return null;
            }
            Variable targetVariable = targetStudyUnit.FindVariable(targetVariableId);
            if (targetVariable == null)
            {
                return null;
            }

            XElement correspondenceElem = variableMapElem.Element(cm + TAG_CORRESPONDENCE);
            if (correspondenceElem == null)
            {
                return null;
            }

            string weightStr = (string)correspondenceElem.Element(cm + TAG_COMMONALITY_WEIGHT);
            if (string.IsNullOrEmpty(weightStr))
            {
                return null;
            }
            double weight = 0;
            if (!double.TryParse(weightStr, out weight))
            {
                return null;
            }
            bool validWeight = weight > 0;
            if (!validWeight)
            {
                return null;
            }
            string memo = (string)correspondenceElem.Element(cm + TAG_COMMONALITY);
            GroupId sourceId = new GroupId(sourceStudyUnit.Id, sourceVariableId);
            GroupId targetId = new GroupId(targetStudyUnit.Id, targetVariableId);
            CompareItem compareItem = new CompareItem(sourceId, targetId, memo, weightStr);
            compareItem.SourceTitle = sourceVariable.Title;
            compareItem.TargetTitle = targetVariable.Title;
            return compareItem;
        }

        private static ICollection<CompareRow> CreateCompareRows(List<CompareItem> compareItems)
        {
            Dictionary<string, CompareRow> rowMap = new Dictionary<string, CompareRow>();
            foreach (CompareItem compareItem in compareItems)
            {
                CompareRow row = null;
                if (rowMap.ContainsKey(compareItem.TargetTitle))
                {
                    row = rowMap[compareItem.TargetTitle];
                }
                else
                {
                    row = new CompareRow();
                    row.Memo = compareItem.Memo;
                    row.Title = compareItem.TargetTitle;
                    rowMap[row.Title] = row;
                }
                row.RowGroupIds.Add(compareItem.TargetId); //Target basically

                if (compareItem.IsMatch)
                {
                    CompareCell sourceCell = row.FindCell(compareItem.SourceId.StudyUnitId);
                    if (sourceCell == null)
                    {
                        sourceCell = new CompareCell();
                        sourceCell.CompareValue = Options.COMPARE_VALUE_MATCH_CODE;
                        sourceCell.ColumnStudyUnitId = compareItem.SourceId.StudyUnitId;
                        row.Cells.Add(sourceCell);
                    }
                }
                CompareCell targetCell = row.FindCell(compareItem.TargetId.StudyUnitId);
                if (targetCell == null)
                {
                    targetCell = new CompareCell();
                    targetCell.CompareValue = compareItem.IsMatch ? Options.COMPARE_VALUE_MATCH_CODE : Options.COMPARE_VALUE_PARTIALMATCH_CODE; //Those that do not match would not have been exported
                    if (compareItem.IsPartialPatch)
                    {
                        targetCell.TargetTitle = compareItem.SourceTitle;
                    }
                    targetCell.ColumnStudyUnitId = compareItem.TargetId.StudyUnitId;
                    row.Cells.Add(targetCell);
                }
            }
            return rowMap.Values;
        }

        private static void InitCompareTable(XElement comparisonElem, EDOModel model)
        {
            IEnumerable<XElement> conceptMapElems = comparisonElem.Elements(cm + TAG_CONCEPT_MAP);
            List<CompareItem> conceptSchemeItems = new List<CompareItem>();
            List<CompareItem> conceptItems = new List<CompareItem>();
            List<CompareItem> variableItems = new List<CompareItem>();
            foreach (XElement conceptMapElem in conceptMapElems)
            {
                CompareItem compareItem = CreateConceptSchemeCompareItem(conceptMapElem, model.StudyUnits);
                if (compareItem != null)
                {
                    conceptSchemeItems.Add(compareItem);
                }
                IEnumerable<XElement> itemMapElems = conceptMapElem.Elements(cm + TAG_ITEM_MAP);
                foreach (XElement itemMapElem in itemMapElems)
                {
                    CompareItem conceptItem = CreateConceptCompareItem(itemMapElem, model.StudyUnits);
                    if (conceptItem != null)
                    {
                        conceptItems.Add(conceptItem);
                    }
                }
            }
            IEnumerable<XElement> variableMapElems = comparisonElem.Elements(cm + TAG_VARIABLE_MAP);
            foreach (XElement variableMapElem in variableMapElems)
            {
                IEnumerable<XElement> itemMapElems = variableMapElem.Elements(cm + TAG_ITEM_MAP);
                foreach (XElement itemMapElem in itemMapElems)
                {
                    CompareItem variableItem = CreateVariableCompareItem(itemMapElem, model.StudyUnits);
                    if (variableItem != null)
                    {
                        variableItems.Add(variableItem);
                    }
                }
            }

            ICollection<CompareRow> conceptSchemeCompareRows = CreateCompareRows(conceptSchemeItems);
            model.Group.ConceptSchemeCompareTable.Rows.AddRange(conceptSchemeCompareRows);
            ICollection<CompareRow> conceptCompareRows = CreateCompareRows(conceptItems);
            model.Group.ConceptCompareTable.Rows.AddRange(conceptCompareRows);
            ICollection<CompareRow> variableCompareRows = CreateCompareRows(variableItems);
            model.Group.VariableCompareTable.Rows.AddRange(variableCompareRows);
        }

        private static void InitGroup(XElement groupElem, EDOModel model)
        {
            string id = (string)groupElem.Attribute(ATTR_ID);
            if (id == null)
            {
                return;
            }
            Group group = Group.CreateDefault();
            model.Group = group;
            group.Id = id;
            group.DataSetCode = (string)groupElem.Attribute(ATTR_DATA_SET);
            group.GeographyCode = (string)groupElem.Attribute(ATTR_GEOGRAPHY);
            group.InstrumentCode = (string)groupElem.Attribute(ATTR_INSTRUMENT);
            group.LanguageCode = (string)groupElem.Attribute(ATTR_LANGUAGE);
            group.PanelCode = (string)groupElem.Attribute(ATTR_PANEL);
            group.TimeCode = (string)groupElem.Attribute(ATTR_TIME);
            XElement purposeElem = groupElem.Element(g + TAG_PURPOSE);
            if (purposeElem != null)
            {
                group.PurposeId = (string)purposeElem.Attribute(ATTR_ID);
                group.Purpose = (string)purposeElem.Element(r + TAG_CONTENT);
            }
            XElement comparisonElem = groupElem.Element(cm + TAG_COMPARISON);
            if (comparisonElem != null)
            {
                InitCompareTable(comparisonElem, model);
            }
        }

        private static EDOModel CreateGroupModel(XElement groupElem)
        {
            EDOModel model = new EDOModel();
            IEnumerable<XElement> elements = groupElem.Descendants(s + TAG_STUDY_UNIT);
            foreach (XElement studyUnitElem in elements)
            {
                StudyUnit studyUnit = CreateStudyUnit(studyUnitElem);
                if (studyUnit != null)
                {
                    if (Group.IsSharedStudyUnit(studyUnit))
                    {
                        model.Group.SharedStudyUnit = studyUnit;
                    }
                    else
                    {
                        model.StudyUnits.Add(studyUnit);
                    }
                }
            }
            InitGroup(groupElem, model);
            return model;
        }

        #endregion

        public void MergeStudyUnit(StudyUnit newStudyUnit, StudyUnit curStudyUnit, DDIImportOption importOption)
        {
            DDIUtils.RenameIds(curStudyUnit, newStudyUnit);
            //Events
            if (importOption.ImportEvent)
            {
                curStudyUnit.MergeEvent(newStudyUnit);
            }

            //Study Member
            if (importOption.ImportMember)
            {
                curStudyUnit.MergeMember(newStudyUnit);
            }

            //Abstract
            if (importOption.ImportAbstract)
            {
                curStudyUnit.MergeAbstract(newStudyUnit);
            }

            //Coverage
            if (importOption.ImportCoverage)
            {
                curStudyUnit.MergeCoverage(newStudyUnit);
            }

            //Funding
            if (importOption.ImportFundingInfo)
            {
                curStudyUnit.MergeFundingInfo(newStudyUnit);
            }

            //Data Collection
            if (importOption.ImportSampling)
            {
                curStudyUnit.MergeSampling(newStudyUnit);
            }

            //Variable concept
            if (importOption.ImportConcept)
            {
                curStudyUnit.MergeConcept(newStudyUnit);
            }

            //Question
            if (importOption.ImportQuestion)
            {
                curStudyUnit.MergeQuestion(newStudyUnit);
            }

            //Category
            if (importOption.ImportCategory)
            {
                curStudyUnit.MergeCategory(newStudyUnit);
            }

            //Code
            if (importOption.ImportCode)
            {
                curStudyUnit.MergeCode(newStudyUnit);
            }

            //Sequence
            if (importOption.ImportQuestion)
            {
                curStudyUnit.MergeSequence(newStudyUnit);
            }

            if (importOption.ImportQuestionGroup)
            {
                curStudyUnit.MergeQuestionGroup(newStudyUnit);
            }

            //Variable
            if (importOption.ImportVariable)
            {
                curStudyUnit.MergeVariable(newStudyUnit);
            }

            //Dataset
            if (importOption.ImportDataSet)
            {
                curStudyUnit.MergeDataSet(newStudyUnit);
            }

            //Data File
            if (importOption.ImportDataFile)
            {
                curStudyUnit.MergeDataFile(newStudyUnit);
            }

            //Related materials
            if (importOption.ImportBook)
            {
                curStudyUnit.MergeBook(newStudyUnit);
            }

            //statistics
            if (importOption.ImportStatistics)
            {
                curStudyUnit.MergeStatisticsInfos(newStudyUnit);
            }

            DDIUtils.FillCollectorFields(newStudyUnit);
        }

        /// <summary>
        /// DDI3データを読み込み、現在のStudyUnitおよびEdoModelに上書きします。
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
                XElement groupElem = doc.Descendants(g + TAG_GROUP).FirstOrDefault();
                if (groupElem != null)
                {
                    return CreateGroupModel(groupElem);
                }
                else
                {
                    XElement studyUnitElem = doc.Descendants(s + TAG_STUDY_UNIT).FirstOrDefault();
                    if (studyUnitElem != null)
                    {
                        return CreateSingleModel(studyUnitElem);
                    }
                }
            }

            throw new ValidationException(new List<ValidationError>() { new ValidationError("", Properties.Resources.DDI3StudyUnitOrGroupDoesntExist, 0, 0) }); // TODO
        }

        public DDIImportOption GenerateImportOption()
        {
            return new DDI3ImportOption();
        }

    }
}
