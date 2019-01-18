using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Main;
using System.Diagnostics;
using System.Xml;
using System.Xml.Linq;
using EDO.Core.Model;
using System.Windows;
using EDO.Core.Util;
using EDO.Core.ViewModel;
using EDO.Properties;
using EDO.QuestionCategory.QuestionGroupForm;
using EDO.QuestionCategory.QuestionForm;
using EDO.Core.Model.Statistics;
using EDO.VariableCategory.VariableForm;
using EDO.QuestionCategory.CodeForm;
using EDO.Core.Util.Statistics;
using System.Collections.ObjectModel;

namespace EDO.Core.IO
{
    public class DDI3Writer :DDI3IO, IDDIWriter
    {
        private static XDeclaration DECLARATION = new XDeclaration("1.0", "UTF-8", "no");

        private static XElement CreateNullableDescription(string desc)
        {
            return CreateNullable(r + TAG_DESCRIPTION, desc);
        }

        private XElement CreateNullableLabel(object obj)
        {
            return CreateNullable(r + TAG_LABEL, obj);
        }

        private EDOConfig config;
        public EDOConfig Config
        {
            get => config;
            set => config = value;
        }

        public DDI3Writer() {}

        private StudyUnitVM studyUnit;

        [Obsolete("Error check at export was canceled.")]
        private void AddError(string message, FormVM form)
        {
            WriteError newErrorInfo = new WriteError(message, studyUnit, form);
            if (!ContainsError(newErrorInfo))
            {
                AddError(newErrorInfo);
            }
        }

        [Obsolete("Error check at export was canceled.", false)]
        private int RemoveError(StudyUnitVM studyUnitVM)
        {
            return RemoveError(param =>
            {
                WriteError writeError = (WriteError)param;
                return writeError.EDOUnit == studyUnitVM;
            });
        }

        private XAttribute CreateIDAttribute(object id)
        {
            return new XAttribute(ATTR_ID, id);
        }

        private XAttribute CreateAgencyAttribute()
        {
            return new XAttribute(ATTR_AGENCY, AGENCY);
        }

        private XAttribute CreateVersionAttribute()
        {
            return new XAttribute(ATTR_VERSION, VERSION);
        }

        private XElement CreateUserID()
        {
            string userId = config.UserId;
            if (string.IsNullOrEmpty(userId))
            {
                userId = IDUtils.NewGuid();
                config.UserId = userId;
            }
            return new XElement(r + TAG_USER_ID, new XAttribute(ATTR_TYPE, "EDO"), userId);
        }

        private XElement CreateID(string id)
        {
            return new XElement(r + TAG_ID, id);
        }

        private XElement CreateContent(object obj)
        {
            return new XElement(r + TAG_CONTENT, obj);
        }

        private XElement CreateReferenceID(XName name, string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return null;
            }    
            return new XElement(name, CreateID(id),
                    new XElement(r + TAG_IDENTIFYING_AGENCY, AGENCY),
                    new XElement(r + TAG_VERSION, VERSION));
        }

        private XElement CreateReferenceID(XName name, string id, string schemeId)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(schemeId))
            {
                return null;
            }
            XElement scheme = new XElement(r + TAG_SCHEME,
                CreateID(schemeId), 
                new XElement(r + TAG_IDENTIFYING_AGENCY, AGENCY),
                new XElement(r + TAG_VERSION, VERSION));

            return new XElement(name, 
                scheme,
                CreateID(id),
                new XElement(r + TAG_IDENTIFYING_AGENCY, AGENCY),
                new XElement(r + TAG_VERSION, VERSION));
        }

        private XElement CreateReferenceModuleID(XName name, string id, string moduleId)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(moduleId))
            {
                return null;
            }
            XElement module = new XElement(r + TAG_MODULE,
                CreateID(moduleId),
                new XElement(r + TAG_IDENTIFYING_AGENCY, AGENCY),
                new XElement(r + TAG_VERSION, VERSION));

            return new XElement(name,
                module,
                CreateID(id),
                new XElement(r + TAG_IDENTIFYING_AGENCY, AGENCY),
                new XElement(r + TAG_VERSION, VERSION));
        }

        private XElement CreateLabel(object obj)
        {
            return new XElement(r + TAG_LABEL, obj);
        }

        private XElement CreateDescription(object obj)
        {
            return new XElement(r + TAG_DESCRIPTION, obj);
        }

        private XElement CreateDate(XName name, DateRange dateRange)
        {
            if (dateRange.IsEmpty)
            {
                return null;
            }
            if (dateRange.IsFromDateOnly)
            {
                return new XElement(name,
                    new XElement(r + TAG_SIMPLE_DATE, ToString(dateRange.FromDateTime)));
            } 
            return new XElement(name,
                new XElement(r + TAG_START_DATE, ToString(dateRange.FromDateTime)),
                new XElement(r + TAG_END_DATE, ToString(dateRange.ToDateTime)));
        }

        private XElement CreateAgencyOrganizationReference(Organization organization)
        {
            if (organization == null)
            {
                AddError(Resources.InputOrganization, studyUnit.MemberForm); //Enter Affiliation
                return null;
            }
            return CreateReferenceID(r + TAG_AGENCY_ORGANIZATION_REFERENCE, organization.Id, OrganizationSchemeId());
        }

        private XElement CreateLifecycleEvent(Event eventModel)
        {
            ///// Create LifeCycleEvent
            // Required except Description. Return null when required field is not entered.
            XElement date = CreateDate(r + TAG_DATE, eventModel.DateRange);
            if (date == null)
            {
                return null;
            }
            XElement description = new XElement(r + TAG_DESCRIPTION, eventModel.Title);
            if (description == null)
            {
                return null;
            }
            Organization organizationModel = studyUnit.OrganizationModels.Count > 0 ? studyUnit.OrganizationModels[0] : null;
            XElement agencyOrganizationReference = CreateAgencyOrganizationReference(organizationModel);
            if (agencyOrganizationReference == null)
            {
                return null;
            }
            XElement memo = new XElement(r + TAG_LABEL, eventModel.Memo);
            XElement lifecycleEvent = new XElement(r + TAG_LIFECYCLE_EVENT, CreateIDAttribute(eventModel.Id));
            lifecycleEvent.Add(memo);
            lifecycleEvent.Add(date);
            lifecycleEvent.Add(agencyOrganizationReference);
            lifecycleEvent.Add(description);
            return lifecycleEvent;
        }

        private XElement CreateIndividual(Member member)
        {
            ///// Create a:Individual Member
            XElement individual = new XElement(a + TAG_INDIVIDUAL,
                CreateIDAttribute(member.Id));
            //IndividualName?
            XElement individualName = new XElement(a + TAG_INDIVIDUAL_NAME);
            //First?
            individualName.Add(CreateNullable(a + TAG_FIRST, member.FirstName));
            //Last?
            individualName.Add(CreateNullable(a + TAG_LAST, member.LastName));
            individual.Add(EmptyToNull(individualName));
            //Position*
            XElement position = new XElement(a + TAG_POSITION);
            //Title
            position.Add(CreateNullable(a + TAG_TITLE, member.Position));
            individual.Add(EmptyToNull(position));
            //r:Description*
            individual.Add(CreateNullable(r + TAG_DESCRIPTION, member.RoleName));
            //Relation*
            individual.Add(
                new XElement(a + TAG_RELATION,
                    CreateReferenceID(a + TAG_ORGANIZATION_REFERENCE, member.OrganizationId),
                    new XElement(r + TAG_DESCRIPTION) //r:Description+
                    ));
            return individual;
        }

        private XElement CreateOrganization(Organization organization)
        {
            ///// Organization
            return new XElement(a + TAG_ORGANIZATION,
                CreateIDAttribute(organization.Id),
                new XElement(a + TAG_ORGANIZATION_NAME, organization.OrganizationName));
        }

        private XElement CreateArchive()
        {
            ///// a:Archive
            XElement archive = new XElement(a + TAG_ARCHIVE,
                CreateIDAttribute(studyUnit.StudyUnitModel.ArchiveId),
                CreateVersionAttribute(),
                CreateAgencyAttribute()
                );
            if (studyUnit.OrganizationModels.Count == 0)
            {
                AddError(Resources.InputOrganization, studyUnit.MemberForm); //Enter Affiliation
            } else {
                //a:ArchiveSpecific
                Organization organization = studyUnit.OrganizationModels[0];
                XElement organizationReference = new XElement(a + TAG_ARCHIVE_SPECIFIC,
                    CreateReferenceID(a + TAG_ARCHIVE_ORGANIZATION_REFERENCE, organization.Id, OrganizationSchemeId()));
                archive.Add(organizationReference);
            }
            //a:OrganizationScheme
            XElement organizationScheme = new XElement(a + TAG_ORGANIZATION_SCHEME, 
                CreateIDAttribute(studyUnit.StudyUnitModel.OrganizationSchemeId),
                CreateVersionAttribute(), 
                CreateAgencyAttribute()
                );
            archive.Add(organizationScheme);
            foreach (Organization organizationModel in studyUnit.OrganizationModels)
            {
                //Organization*
                XElement organization = CreateOrganization(organizationModel);
                organizationScheme.Add(organization);
            }
            foreach (FundingInfo fundingInfo in studyUnit.FundingInfoModels)
            {
                //Save the name of Funding Agency as organization
                //Organization*
                XElement organization = CreateOrganization(fundingInfo.Organization);
                organizationScheme.Add(organization);
            }
            foreach (Member member in studyUnit.MemberModels)
            {
                //Individual*
                XElement individual = CreateIndividual(member);
                organizationScheme.Add(individual);
            }
            //r:LifecycleInformation? study event
            XElement lifecycleInformation = new XElement(r + TAG_LIFECYCLE_INFORMATION);
            foreach (Event eventModel in studyUnit.EventModels)
            {
                XElement lifecycleEvent = CreateLifecycleEvent(eventModel);
                lifecycleInformation.Add(lifecycleEvent);
            }
            archive.Add(EmptyToNull(lifecycleInformation));
            return archive;
        }

        private XElement CreateTopicalCoverage()
        {
            ///// r:TopicalCoverage
            XElement topicalCoverage = new XElement(r + TAG_TOPICAL_COVERAGE, CreateIDAttribute(studyUnit.CoverageModel.TopicalCoverageId));
            //r:Subject* checkbox of Subject
            foreach (CheckOption topicOption in studyUnit.CoverageModel.Topics)
            {
                if (!topicOption.IsChecked)
                {
                    continue;
                }
                string label = topicOption.HasMemo ? topicOption.Memo : Options.CoverageTopicLabel(topicOption.Code);
                XElement topic = new XElement(r + TAG_SUBJECT,
                    new XAttribute(ATTR_CODE_LIST_AGENCY, AGENCY),
                    label);
                topicalCoverage.Add(topic);
            }
            //r:Keyword* keyword
            foreach (Keyword keywordModel in studyUnit.CoverageModel.Keywords)
            {
                XElement keyword = new XElement(r + TAG_KEYWORD,
                    new XAttribute(ATTR_CODE_LIST_AGENCY, AGENCY),
                    keywordModel.Content);
                topicalCoverage.Add(keyword);
            }
            //Return null if no element included
            return EmptyToNull(topicalCoverage);
        }

        private XElement CreateTemporalCoverage()
        {
            ///// r:TemporalCoverage
            //r: ReferenceDate* Date of study
            XElement temporalCoverage = new XElement(r + TAG_TEMPORAL_COVERAGE,
                CreateIDAttribute(studyUnit.CoverageModel.TemporalCoverageId),
                CreateDate(r + TAG_REFERENCE_DATE, studyUnit.CoverageModel.DateRange));
            //Return null if no element included
            return EmptyToNull(temporalCoverage);
        }

        private XElement CreateSpatialCoverage()
        {
            ///// r:SpatialCoverage
            List<CheckOption> checkedAreas = studyUnit.CoverageModel.CheckedAreas;
            int areaCount = checkedAreas.Count;
            if (areaCount == 0)
            {
                return null;
            }
            //Area of study
            XElement spatialCoverage = new XElement(r + TAG_SPATIAL_COVERAGE,
                CreateIDAttribute(studyUnit.CoverageModel.SpatialCoverageId));
            //Description*
            spatialCoverage.Add(CreateNullableDescription(studyUnit.CoverageModel.Memo));
            //GeographicStructureReference*
            foreach (CheckOption option in checkedAreas)
            {
                XElement reference = CreateReferenceID(r + TAG_GEOGRAPHIC_STRUCTURE_REFERENCE, studyUnit.CoverageModel.GetGeographicStructureId(option));
                spatialCoverage.Add(reference);
            }
            //TopLevelReference
            CheckOption topArea = checkedAreas[0];
            XElement topLevelReference = new XElement(r + TAG_TOP_LEVEL_REFERENCE,
                CreateReferenceID(r + TAG_LEVEL_REFERENCE, studyUnit.CoverageModel.GetGeographicStructureId(topArea), GeographicStructureSchemeId()),
                new XElement(r + TAG_LEVEL_NAME, topArea.Label));
            spatialCoverage.Add(topLevelReference);
            //LowestLevelReference
            CheckOption lowestArea = checkedAreas[areaCount - 1];
            XElement lowestLevelReference = new XElement(r + TAG_LOWEST_LEVEL_REFERENCE,
                CreateReferenceID(r + TAG_LEVEL_REFERENCE, studyUnit.CoverageModel.GetGeographicStructureId(lowestArea), GeographicStructureSchemeId()),
                new XElement(r + TAG_LEVEL_NAME, lowestArea.Label));
            spatialCoverage.Add(lowestLevelReference);
            //Null-convert is not needed because element is always included
            return spatialCoverage;
        }

        private XElement CreateCoverage()
        {
            ///// r:Coverage
            XElement coverage =  new XElement(r + TAG_COVERAGE);
            //r:TopicalCoverage?
            coverage.Add(CreateTopicalCoverage());
            //r:SpatialCoverage?
            coverage.Add(CreateSpatialCoverage());
            //r:TemporalCoverage?
            coverage.Add(CreateTemporalCoverage());
            //Return null if no element included
            return EmptyToNull(coverage);
        }

        private XElement CreateFundingInformation(FundingInfo fundingInfo)
        {
            ///// Export r:FundingInformation
            XElement fundingInfoElem =  new XElement(r + TAG_FUNDING_INFORMATION);
            //AgencyOrganizationReference+
            fundingInfoElem.Add(CreateAgencyOrganizationReference(fundingInfo.Organization));
            //GrantNumber*
            fundingInfoElem.Add(CreateNullable(r + TAG_GRANT_NUMBER, fundingInfo.Number));
            //Description*
            fundingInfoElem.Add(CreateNullable(r + TAG_DESCRIPTION, ToDDIFundingInfoMoney(fundingInfo.Money)));
            fundingInfoElem.Add(CreateNullable(r + TAG_DESCRIPTION, ToDDIFundingInfoTitle(fundingInfo.Title)));
            fundingInfoElem.Add(CreateNullable(r + TAG_DESCRIPTION, ToDDIFundingInfoDateRange(fundingInfo.DateRange)));
            return fundingInfoElem;
        }

        private string SamplingError(string message, Sampling samplingModel)
        {
            return message + "(" + samplingModel.Title + ")";
        }

        private XElement CreateConceptualComponent(bool isGroup)
        {
            //c:ConceptualComponent
            XElement conceptComponent = new XElement(c + TAG_CONCEPTUAL_COMPONENT, 
                CreateIDAttribute(studyUnit.StudyUnitModel.ConceptualComponentId),
                CreateVersionAttribute(), 
                CreateAgencyAttribute()
                );

            if (!isGroup)
            {
                //Related materials
                List<XElement> otherMaterials = CreateOtherMaterials(BookRelationType.Concept);
                conceptComponent.Add(otherMaterials);
            }

            //c:ConceptScheme*
            foreach (ConceptScheme conceptSchemeModel in studyUnit.ConceptSchemeModels)
            {
                XElement conceptScheme = new XElement(c + TAG_CONCEPT_SCHEME,
                    CreateIDAttribute(conceptSchemeModel.Id),
                    CreateVersionAttribute(), 
                    CreateAgencyAttribute(),
                    CreateNullable(r + TAG_LABEL, conceptSchemeModel.Title), // r:Label*
                    CreateNullable(r + TAG_DESCRIPTION, conceptSchemeModel.Memo)); // r:Description*

                //c:Concept*
                foreach (Concept conceptModel in conceptSchemeModel.Concepts)
                {
                    XElement concept = new XElement(c + TAG_CONCEPT,
                        CreateIDAttribute(conceptModel.Id),
                        CreateNullable(r + TAG_LABEL, conceptModel.Title),// r:Label*
                        CreateNullable(r + TAG_DESCRIPTION, conceptModel.Content));// r:Description*
                    conceptScheme.Add(EmptyToNull(concept));
                }
                conceptComponent.Add(EmptyToNull(conceptScheme));
            }

            // c:UniverseScheme*
            XElement universeScheme = new XElement(c + TAG_UNIVERSE_SCHEME,
                CreateIDAttribute(UniverseSchemeId()),
                CreateVersionAttribute(),
                CreateAgencyAttribute()
                );
            conceptComponent.Add(universeScheme);
            foreach (Sampling samplingModel in studyUnit.SamplingModels)
            {
                // main Universe
                Universe mainUniverseModel = samplingModel.MainUniverse;
                if (mainUniverseModel == null)
                {
                    if (isGroup)
                    {
                        continue;
                    }
                    AddError(SamplingError(Resources.InputUniverse, samplingModel), studyUnit.SamplingForm); //Enter Universe
                }
                else
                {

                    // c:Universe*(It'll never be null because the title is automatically converted as "Not Entered")
                    XElement universe = new XElement(c + TAG_UNIVERSE,
                        CreateIDAttribute(mainUniverseModel.Id),
                        CreateNullable(r + TAG_LABEL, mainUniverseModel.Title), //r:Label*
                        CreateNullable(c + TAG_HUMAN_READABLE, mainUniverseModel.Memo)); //c:HumanReadable
                    universeScheme.Add(universe);
                    foreach (Universe subUniverseModel in samplingModel.Universes)
                    {
                        if (subUniverseModel == mainUniverseModel)
                        {
                            //assumes that main exists only one
                            continue;
                        }
                        //Store as sub Universe except the main one
                        //c:Subuniverse*
                        XElement subUniverse = new XElement(c + TAG_SUB_UNIVERSE,
                            CreateIDAttribute(subUniverseModel.Id),
                            CreateNullable(r + TAG_LABEL, subUniverseModel.Title),//r:Label*
                            CreateNullable(c + TAG_HUMAN_READABLE, subUniverseModel.Memo));//c:HumanReadable
                        universe.Add(subUniverse);
                    }
                }
            }

            if (!isGroup)
            {
                // level such as Country or Prefecture
                //GeographicStructureScheme*
                XElement geographicsStructureScheme = new XElement(c + TAG_GEOGRAPHIC_STRUCTURE_SCHEME, 
                    CreateIDAttribute(GeographicStructureSchemeId()),
                    CreateVersionAttribute(),
                    CreateAgencyAttribute()
                    );
                conceptComponent.Add(geographicsStructureScheme);
                //r:GeographicStructure(null check is not needed because Label is always saved)
                foreach (CheckOption option in studyUnit.CoverageModel.CheckedAreas)
                {
                    XElement geographicStructure = new XElement(r + TAG_GEOGRAPHIC_STRUCTURE,
                        CreateIDAttribute(studyUnit.CoverageModel.GetGeographicStructureId(option)),
                        new XElement(r + TAG_GEOGRAPHY,
                            CreateIDAttribute(studyUnit.CoverageModel.GetGeographicId(option)),
                            new XElement(r + TAG_LEVEL,
                                new XElement(r + TAG_NAME, option.Label))));
                    geographicsStructureScheme.Add(geographicStructure);
                }

            }
            return conceptComponent;
        }

        private XElement CreateCodeResponse(XName name, Response response)
        {
            XElement codeResponse = new XElement(name);
            codeResponse.Add(CreateReferenceID(r + TAG_CODE_SCHEME_REFERENCE, response.CodeSchemeId, response.CodeSchemeId));
            return codeResponse;
        }

        private XAttribute CreateMissingValueAttribute(Response response)
        {
            if (response.MissingValues.Count == 0)
            {
                return null;
            }
            return new XAttribute(ATTR_MISSING_VALUE, response.JoinMissingValuesContent());
        }

        private XElement CreateNumberResponse(XName name, Response response)
        {
            return new XElement(name,
                CreateMissingValueAttribute(response),
                new XAttribute(ATTR_TYPE, Options.NumberTypeDDILabel(response.DetailTypeCode)),
                new XElement(r + TAG_NUMBER_RANGE,
                    new XElement(r + TAG_LOW, response.Min),
                    new XElement(r + TAG_HIGH, response.Max)
                    ));
        }

        private XElement CreateTextResponse(XName name, Response response)
        {
            XElement textDomain = new XElement(name,
                CreateMissingValueAttribute(response),
                new XAttribute(ATTR_MAX_LENGTH, response.Max),
                new XAttribute(ATTR_MIN_LENGTH, response.Min));
            return textDomain;
        }

        private XElement CreateDateTimeResponse(XName name, Response response)
        {
            XElement dateTimeDomain = new XElement(name,
                CreateMissingValueAttribute(response),
                new XAttribute(ATTR_TYPE, Options.DateTimeTypeDDILabel(response.DetailTypeCode))
                );
            return dateTimeDomain;
        }

        private string UniverseSchemeId()
        {
            return studyUnit.StudyUnitModel.UniverseSchemeId;
        }

        private string GeographicStructureSchemeId()
        {
            return studyUnit.StudyUnitModel.GeographicStructureSchemeId;        
        }

        private string OrganizationSchemeId()
        {
            return studyUnit.StudyUnitModel.OrganizationSchemeId;
        }

        private string QuestionSchemeId()
        {
            return studyUnit.StudyUnitModel.QuestionSchemeId;
        }

        private string VariableSchemeId() 
        {
            return studyUnit.VariableSchemeModel.Id;
        }
        
        private string LogicalProductId()
        {
            return studyUnit.StudyUnitModel.LogicalProductId;
        }

        private string PhysicalStructureSchemeId()
        {
            return studyUnit.StudyUnitModel.PhysicalStructureSchemeId;
        }

        private string RecordLayoutSchemeId()
        {
            return studyUnit.StudyUnitModel.RecordLayoutSchemeId;
        }

        private string ConceptSchemeId(string conceptId)
        {
            ConceptScheme conceptScheme = studyUnit.StudyUnitModel.FindConceptSchemeByConceptId(conceptId);
            return conceptScheme != null ? conceptScheme.Id : null;
        }

        private string CategorySchemeId(string categoryId)
        {
            CategoryScheme categoryScheme = studyUnit.StudyUnitModel.FindCategorySchemeByCategoryId(categoryId);
            return categoryScheme != null ? categoryScheme.Id : null;
        }


        private XElement CreateQuestionItem(Question questionModel)
        {
            //d:QuestionItem+(Null check is not needed)
            XElement questionItem = new XElement(d + TAG_QUESTION_ITEM,
                CreateIDAttribute(questionModel.Id),
                new XElement(d + TAG_QUESTION_ITEM_NAME, questionModel.Title), //QuestionItemName*
                new XElement(d + TAG_QUESTION_TEXT, //QuestionText*
                    new XElement(d + TAG_LITERAL_TEXT, //LiteralTextText+
                        new XElement(d + TAG_TEXT, questionModel.Text))));//Text

            //d:ResponseDomain
            XElement response = null;
            Response responseModel = questionModel.Response;
            if (responseModel.IsTypeChoices)
            {
                response = CreateCodeResponse(d + TAG_CODE_DOMAIN, responseModel);
                response.Add(new XElement(d + TAG_LABEL, responseModel.Title));
            }
            else if (responseModel.IsTypeNumber)
            {
                if (responseModel.DetailTypeCode == null)
                {
                    MenuItemVM menuItem = studyUnit.FindMenuItem(studyUnit.QuestionForm);
                    AddError(Resources.SelectNumberType, studyUnit.QuestionForm); //Enter Type of Numeric Variable
                }
                else if (responseModel.Min == null || responseModel.Max == null)
                {
                    AddError(Resources.InputMinAndMax, studyUnit.QuestionForm); //Enter Minimum and Maximum Value of Numeric Variable
                }
                else
                {
                    response = CreateNumberResponse(d + TAG_NUMERIC_DOMAIN, responseModel);
                    response.Add(new XElement(d + TAG_LABEL, responseModel.Title));
                }
            }
            else if (responseModel.IsTypeFree)
            {
                if (responseModel.Min == null || responseModel.Max == null)
                {
                    AddError(Resources.InputMinAndMax, studyUnit.QuestionForm); //Enter Minimum and Maximum Value of Numeric Variable
                }
                else
                {
                    response = CreateTextResponse(d + TAG_TEXT_DOMAIN, responseModel);
                    response.Add(new XElement(r + TAG_LABEL, responseModel.Title));
                }
            }
            else if (responseModel.IsTypeDateTime)
            {
                if (responseModel.DetailTypeCode == null)
                {
                    AddError(Resources.SelectDateType, studyUnit.QuestionForm); //Enter Type of Date
                }
                else
                {
                    response = CreateDateTimeResponse(d + TAG_DATE_TIME_DOMAIN, responseModel);
                    response.Add(new XElement(d + TAG_LABEL, responseModel.Title));
                }
            }
            questionItem.Add(response);
            //ConceptReference*
            XElement conceptReference = CreateReferenceID(d + TAG_CONCEPT_REFERENCE, questionModel.ConceptId, ConceptSchemeId(questionModel.ConceptId));
            questionItem.Add(conceptReference);

            return questionItem;
        }

        private XElement CreateQuestionScheme()
        {
            if (studyUnit.QuestionModels.Count == 0)
            {
                return null;
            }
            //d:QuestionScheme
            XElement questionScheme = new XElement(d + TAG_QUESTION_SCHEME, 
                CreateIDAttribute(studyUnit.StudyUnitModel.QuestionSchemeId),
                CreateVersionAttribute(), 
                CreateAgencyAttribute()
                );
            //Export questions that are not included in the QuestionGroup in advance 
            foreach (Question questionModel in studyUnit.QuestionModels)
            {
                if (!studyUnit.StudyUnitModel.ContainsInQuestionGroup(questionModel))
                {
                    XElement questionItem = CreateQuestionItem(questionModel);
                    questionScheme.Add(questionItem);
                }
            }
            foreach (QuestionGroupVM questionGroup in studyUnit.QuestionGroups)
            {
                if (questionGroup.Questions.Count == 0)
                {
                    AddError(Resources.PleaseSelectQuestion, studyUnit.QuestionGroupForm); //Select a question
                }
                else
                {
                    XElement multipleQuestionItem = new XElement(d + TAG_MULTIPLE_QUESTION_ITEM,
                        CreateIDAttribute(questionGroup.Id),
                        new XElement(d + TAG_MULTIPLE_QUESTION_ITEM_NAME, questionGroup.Title),
                        new XElement(d + TAG_QUESTION_TEXT,
                            new XElement(d + TAG_LITERAL_TEXT,
                                new XElement(d + TAG_TEXT, questionGroup.Memo))));
                    questionScheme.Add(multipleQuestionItem);

                    XElement subQuestions = new XElement(d + TAG_SUB_QUESTIONS);
                    multipleQuestionItem.Add(subQuestions);
                    foreach (QuestionVM question in questionGroup.Questions)
                    {
                        XElement questionItem = CreateQuestionItem((Question)question.Model);
                        subQuestions.Add(questionItem);
                    }
                }
            }
            return questionScheme;
        }

        private XElement CreateControlConstructSchemes(ControlConstructScheme schemeModel)
        {
            //d:ControlConstructScheme
            if (!schemeModel.HasConstruct)
            {
                return null;
            }
            XElement scheme = new XElement(d + TAG_CONTROL_CONSTRUCT_SCHEME, 
                CreateIDAttribute(schemeModel.Id),
                CreateVersionAttribute(), 
                CreateAgencyAttribute(),
                CreateNullable(d + TAG_CONTROL_CONSTRUCT_SCHEME_NAME, schemeModel.Title) //ControlConstructSchemeName*
                );

            //d:ControlConstruct+
            //d:QuestionConstruct
            foreach (QuestionConstruct constructModel in schemeModel.QuestionConstructs)
            {
                XElement questionConstruct = new XElement(d + TAG_QUESTION_CONSTRUCT, CreateIDAttribute(constructModel.Id),
                    CreateNullable(r + TAG_LABEL, constructModel.No), //r:Label*
                    CreateReferenceID(d + TAG_QUESTION_REFERENCE, constructModel.QuestionId, QuestionSchemeId()) //QuestionReference
                    );
                scheme.Add(questionConstruct);
            }
            //Question Group
            foreach (QuestionGroupConstruct constructModel in schemeModel.QuestionGroupConstructs)
            {
                XElement questionConstruct = new XElement(d + TAG_QUESTION_CONSTRUCT, CreateIDAttribute(constructModel.Id),
                    CreateNullable(r + TAG_LABEL, constructModel.No), //r:Label*
                    CreateReferenceID(d + TAG_QUESTION_REFERENCE, constructModel.QuestionGroupId, QuestionSchemeId()) //QuestionReference
                    );
                scheme.Add(questionConstruct);
            }


            //d:StatementItem
            foreach (Statement statementModel in schemeModel.Statements)
            {
                XElement statement = new XElement(d + TAG_STATEMENT_ITEM, CreateIDAttribute(statementModel.Id),
                    new XElement(r + TAG_LABEL, statementModel.No),//r:Label*
                    new XElement(d + TAG_DISPLAY_TEXT,// DisplayText+
                        new XElement(d + TAG_LITERAL_TEXT, //LiteralText+
                            new XElement(d + TAG_TEXT, statementModel.Text))) //Text+
                    );
                scheme.Add(statement);
            }
            //d:IfThenElse
            foreach (IfThenElse ifThenElseModel in schemeModel.IfThenElses)
            {
                XElement ifThenElse = new XElement(d + TAG_IF_THEN_ELSE, CreateIDAttribute(ifThenElseModel.Id));
                scheme.Add(ifThenElse);

                //IfCondition
                XElement ifCondition = new XElement(d + TAG_IF_CONDITION,
                    new XElement(r + TAG_CODE, ifThenElseModel.IfCondition.Code), //Code+
                    CreateReferenceID(r + TAG_SOURCE_QUESTION_REFERENCE, ifThenElseModel.IfCondition.QuestionId, QuestionSchemeId()) //SourceQuestionReference*
                    );
                ifThenElse.Add(ifCondition);

                //ThenConstructReference
                ifThenElse.Add(CreateReferenceID(d + TAG_THEN_CONSTRUCT_REFERENCE, ifThenElseModel.ThenConstructId, schemeModel.Id));

                //ElseIf*
                foreach (ElseIf elseIfModel in ifThenElseModel.ElseIfs)
                {
                    XElement elseIf = new XElement(d + TAG_ELSE_IF);
                    ifThenElse.Add(elseIf);

                    //IfCondition
                    XElement elseIfCondition = new XElement(d + TAG_IF_CONDITION,
                        new XElement(r + TAG_CODE, elseIfModel.IfCondition.Code),
                        CreateReferenceID(r + TAG_SOURCE_QUESTION_REFERENCE, elseIfModel.IfCondition.QuestionId, QuestionSchemeId())
                        );
                    elseIf.Add(elseIfCondition);
                    //ThenConstructReference
                    elseIf.Add(CreateReferenceID(d + TAG_THEN_CONSTRUCT_REFERENCE, elseIfModel.ThenConstructId, schemeModel.Id));
                }
                //ElseConstructReference?
                ifThenElse.Add(CreateReferenceID(d + TAG_ELSE_CONSTRUCT_REFERENCE, ifThenElseModel.ElseConstructId, schemeModel.Id));
            }

            // Sequence
            XElement sequence = new XElement(d + TAG_SEQUENCE, CreateIDAttribute(schemeModel.Sequence.Id));
            scheme.Add(sequence);
            foreach (string id in schemeModel.Sequence.ControlConstructIds)
            {
                //ControlConstructReference*
                XElement controlConstruct = CreateReferenceID(d + TAG_CONTROL_CONSTRUCT_REFERENCE, id, schemeModel.Id);
                sequence.Add(controlConstruct);
            }
            return scheme;
        }

        private List<XElement> CreateControlConstructSchemes()
        {
            //r:ControlConstructScheme
            List<XElement> controlConstructSchemes = new List<XElement>();
            foreach (ControlConstructScheme scheme in studyUnit.ControlConstructSchemeModels)
            {
                controlConstructSchemes.Add(CreateControlConstructSchemes(scheme));
            }
            return controlConstructSchemes;
        }

        private XElement CreateProcessingEvent()
        {
            XElement processingEvent = new XElement(d + TAG_PROCESSING_EVENT,
                CreateIDAttribute(studyUnit.StudyUnitModel.ProcessingEventId)
            );
            foreach (Sampling samplingModel in studyUnit.SamplingModels)
            {
                XElement dataAppraisalInformation = new XElement(d + TAG_DATA_APPRAISAL_INFORMATION);
                processingEvent.Add(dataAppraisalInformation);
                foreach (SamplingNumber samplingNumberModel in samplingModel.SamplingNumbers)
                {
                    // content format = ResponseRate(SampleSize,NumberOfResponses,Description)
                    string content = ToResponseRateContent(samplingNumberModel);
                    XElement responseRate = new XElement(d + TAG_RESPONSE_RATE, content);
                    dataAppraisalInformation.Add(responseRate);
                }
            }
            return processingEvent;
        }

        private XElement CreateDataCollection()
        {
            ///// d:DataCollection
            XElement dataCollection = new XElement(d + TAG_DATA_COLLECTION, 
                CreateIDAttribute(studyUnit.StudyUnitModel.DataCollectionId),
                CreateVersionAttribute(), 
                CreateAgencyAttribute()
                );

            dataCollection.Add(CreateOtherMaterials(BookRelationType.Question));

            //d:Methodology? (This tag will never be empty because Universe is required)
            XElement methodology = new XElement(d + TAG_METHODOLOGY, CreateIDAttribute(studyUnit.StudyUnitModel.MethodologyId));
            dataCollection.Add(methodology);
            foreach (Universe universeModel in studyUnit.AllUniverseModels)
            {
                //Content must be saved in order to associate with Universe
                XElement samplingProcesure =
                    new XElement(d + TAG_SAMPLING_PROCEDURE, CreateIDAttribute(universeModel.SamplingProcedureId),
                        new XElement(r + TAG_CONTENT, universeModel.Method));
                methodology.Add(samplingProcesure);
            }


            foreach (Sampling samplingModel in studyUnit.SamplingModels)
            {
                //CollectionEvent*
                XElement collectionEvent = new XElement(d + TAG_COLLECTION_EVENT, CreateIDAttribute(samplingModel.CollectionEventId));
                dataCollection.Add(collectionEvent);
                if (samplingModel.MemberId == null)
                {
                    AddError(SamplingError(Resources.InputCollectionLeader, samplingModel), studyUnit.SamplingForm); //Enter Data Collection Responsibility
                }
                else
                {
                    //DataCollectorOrganizationReference*
                    XElement organizationReference = CreateReferenceID(d + TAG_DATA_COLLECTOR_ORGANIZATION_REFERENCE, samplingModel.MemberId, OrganizationSchemeId());
                    collectionEvent.Add(organizationReference);
                }
                if (samplingModel.DateRange.IsEmpty)
                {
                    AddError(SamplingError(Resources.InputCollectionDate, samplingModel), studyUnit.SamplingForm); //Enter Data Collection Date/Period
                }
                else
                {
                    //DataCollectionDate
                    XElement date = CreateDate(d + TAG_DATA_COLLECTION_DATE, samplingModel.DateRange);
                    collectionEvent.Add(date);
                }
                //ModeOfCollection*(may be null)
                XElement method = new XElement(d + TAG_MODE_OF_COLLECTION,
                    CreateIDAttribute(samplingModel.ModeOfCollectionId),
                    CreateNullable(r + TAG_CONTENT, samplingModel.MethodName));
                collectionEvent.Add(EmptyToNull(method));
                //CollectionSituation*(may be null)
                XElement situation = new XElement(d + TAG_COLLECTION_SITUATION,
                    CreateIDAttribute(samplingModel.ModeOfCollectionId),
                    CreateNullable(r + TAG_CONTENT, samplingModel.Situation));
                collectionEvent.Add(EmptyToNull(situation));
            }

            // QuestionScheme*
            dataCollection.Add(CreateQuestionScheme());



            //ControlConstructScheme*
            List<XElement> controlConstructSchemes = CreateControlConstructSchemes();
            foreach (XElement controlConstructScheme in controlConstructSchemes)
            {
                dataCollection.Add(controlConstructScheme);
            }

            //ProcessingEvent
            dataCollection.Add(CreateProcessingEvent());

            return dataCollection;
        }

        private XElement CreateCategoryScheme(CategoryScheme categorySchemeModel)
        {
            //l:CategoryScheme
            XElement categoryScheme = new XElement(l + TAG_CATEGORY_SCHEME,
                CreateIDAttribute(categorySchemeModel.Id),
                CreateVersionAttribute(), 
                CreateAgencyAttribute(),
                CreateLabel(categorySchemeModel.Title), //r:Label*
                CreateNullableDescription(categorySchemeModel.Memo));// r:Description*
            //l:Category*
            foreach (Category categoryModel in categorySchemeModel.Categories)
            {
                XAttribute missing = null;
                if (categoryModel.IsMissingValue)
                {
                    missing = new XAttribute(ATTR_MISSING, "1");
                }
                XElement category = new XElement(l + TAG_CATEGORY,
                    CreateIDAttribute(categoryModel.Id),
                    missing,
                    CreateLabel(categoryModel.Title),//r:Label*
                    CreateNullableDescription(categoryModel.Memo));// r:Description*
                categoryScheme.Add(category);
            }
            return categoryScheme;
        }

        private XElement CreateCodeScheme(CodeScheme codeSchemeModel)
        {
            //l:CodeScheme
            XElement codeScheme = new XElement(l + TAG_CODE_SCHEME,
                CreateIDAttribute(codeSchemeModel.Id),
                CreateVersionAttribute(), 
                CreateAgencyAttribute(),
                CreateLabel(codeSchemeModel.Title),//r:Label*
                CreateNullableDescription(codeSchemeModel.Memo)); //r:Description*,
            //Code*
            foreach (Code codeModel in codeSchemeModel.Codes)
            {
                XElement code = new XElement(l + TAG_CODE,
                    CreateReferenceID(l + TAG_CATEGORY_REFERENCE, codeModel.CategoryId, CategorySchemeId(codeModel.CategoryId)), //CategoryReference
                    new XElement(l + TAG_VALUE, codeModel.Value)); //Value
                codeScheme.Add(code);
            }
            return codeScheme;
        }

        private XElement CreateVariableScheme()
        {
            //l:VariableScheme
            if (studyUnit.VariableModels.Count == 0)
            {
                return null;
            }
            XElement variableScheme = new XElement(l + TAG_VARIABLE_SCHEME, 
                CreateIDAttribute(VariableSchemeId()),
                CreateVersionAttribute(), 
                CreateAgencyAttribute()
                );
            foreach (Variable variableModel in studyUnit.VariableModels)
            {
                //Variable*
                XElement variable = new XElement(l + TAG_VARIABLE,
                    CreateIDAttribute(variableModel.Id),
                    new XElement(l + TAG_VARIABLE_NAME, variableModel.Title), // VariableName*
                    CreateNullableLabel(variableModel.Label), //r:Label*
                    CreateReferenceID(r + TAG_UNIVERSE_REFERENCE, variableModel.UniverseId, UniverseSchemeId()), //r:UniverseReference*
                    CreateReferenceID(l + TAG_CONCEPT_REFERENCE, variableModel.ConceptId, ConceptSchemeId(variableModel.ConceptId)), //r:ConceptReference
                    CreateReferenceID(l + TAG_QUESTION_REFERENCE, variableModel.QuestionId, QuestionSchemeId())); //r:QuestionReference
                variableScheme.Add(variable);


                Response responseModel = variableModel.Response;
                XElement response = null;
                if (responseModel.IsTypeChoices)
                {
                    response = CreateCodeResponse(l + TAG_CODE_REPRESENTATION, responseModel);
                }
                else if (responseModel.IsTypeNumber)
                {
                    if (responseModel.DetailTypeCode == null)
                    {
                        AddError(Resources.SelectNumberType, studyUnit.VariableForm); //Enter Type of Numeric Variable
                    }
                    else if (responseModel.Min == null || responseModel.Max == null)
                    {
                        AddError(Resources.InputMinAndMax, studyUnit.VariableForm); //Enter Minimum and Maximum Value of Numeric Variable
                    }
                    else
                    {
                        response = CreateNumberResponse(l + TAG_NUMERIC_REPRESENTATION, responseModel);
                    }
                } else if (responseModel.IsTypeFree)
                {
                    if (responseModel.Min == null || responseModel.Max == null)
                    {
                        AddError(Resources.InputMinAndMax, studyUnit.VariableForm); //Enter Minimum and Maximum Value of Numeric Variable
                    }
                    else
                    {
                        response = CreateTextResponse(l + TAG_TEXT_REPRESENTATION, responseModel);
                    }
                } else if (responseModel.IsTypeDateTime)
                {
                    if (responseModel.DetailTypeCode == null)
                    {
                        AddError(Resources.SelectDateType, studyUnit.VariableForm); //Enter Type of Date
                    }
                    else
                    {
                        response = CreateDateTimeResponse(l + TAG_DATE_TIME_REPRESENTATION, responseModel);
                    }
                }
                if (response != null)
                {
                    //Representation?
                    XElement representation = new XElement(l + TAG_REPRESENTATION);
                    representation.Add(response);
                    variable.Add(representation);
                }
            }
            return variableScheme;
        }

        private XElement CreateDataRelationship()
        {
            ///// l:DataRelationship
            if (studyUnit.DataSetModels.Count == 0)
            {
                return null;
            }
            XElement dataRelationship = new XElement(l + TAG_DATA_RELATIONSHIP, CreateIDAttribute(studyUnit.StudyUnitModel.DataCollectionId));
            //LogicalRecord+
            //LogicalRecord=DataSet repeat
            foreach (DataSet dataSetModel in studyUnit.DataSetModels)
            {
                XElement logicalRecord = new XElement(l + TAG_LOGICAL_RECORD,
                    CreateIDAttribute(dataSetModel.Id),
                    new XAttribute(ATTR_HAS_LOCATOR, "false"),
                    CreateNullable(l + TAG_LOGICAL_RECORD_NAME, dataSetModel.Title),//LogicalRecordName*
                    CreateNullableDescription(dataSetModel.Memo));// r:Description*
                dataRelationship.Add(logicalRecord);
                //l:VariablesInRecord 
                XElement variablesInRecord = new XElement(l + TAG_VARIABLES_IN_RECORD,
                    new XAttribute(ATTR_ALL_VARIABLES_IN_LOGICAL_PRODUCT, "false"));
                logicalRecord.Add(variablesInRecord);
                //VariableSchemeReference*
                variablesInRecord.Add(CreateReferenceID(l + TAG_VARIABLE_SCHEME_REFERENCE, VariableSchemeId(), VariableSchemeId()));
                foreach (string variableId in dataSetModel.VariableGuids)
                {
                    //VariableUsedReference*
                    variablesInRecord.Add(CreateReferenceID(l + TAG_VARIABLE_USED_REFERENCE, variableId, VariableSchemeId()));
                }
            }
            return dataRelationship;
        }

        private XElement CreateLogicalProduct()
        {
            //l:LogicalProduct
            XElement logicalProduct = new XElement(l + TAG_LOGICAL_PRODUCT,
                CreateIDAttribute(LogicalProductId()),
                CreateVersionAttribute(),
                CreateAgencyAttribute()
                );
            //l:DataRelationship*
            logicalProduct.Add(CreateDataRelationship());
            logicalProduct.Add(CreateOtherMaterials(BookRelationType.Variable));
            //l:CategoryScheme*
            foreach (CategoryScheme categorySchemeModel in studyUnit.CategorySchemeModels)
            {
                logicalProduct.Add(CreateCategoryScheme(categorySchemeModel));
            }
            //l:CodeScheme*
            foreach (CodeScheme codeSchemeModel in studyUnit.CodeSchemeModels)
            {
                logicalProduct.Add(CreateCodeScheme(codeSchemeModel));
            }
            //l:VariableScheme*
            logicalProduct.Add(CreateVariableScheme());
            return logicalProduct;
        }

        private XElement CreatePhysicalDataProduct()
        {
            //p:PhysicalDataProduct*
            XElement physicalDataProduct = new XElement(p + TAG_PHYSICAL_DATA_PRODUCT,
                CreateIDAttribute(studyUnit.StudyUnitModel.PhysicalDataProductId),
                CreateVersionAttribute(),
                CreateAgencyAttribute()
                );            
            //Check if at least one instance exists
            if (studyUnit.DataFileModels.Count > 0)
            {
                //PhysicalStructureScheme*
                XElement physicalStructureScheme = new XElement(p + TAG_PHYSICAL_STRUCTURE_SCHEME,
                    CreateIDAttribute(PhysicalStructureSchemeId()),
                    CreateVersionAttribute(),
                    CreateAgencyAttribute()
                    );
                physicalDataProduct.Add(physicalStructureScheme);
                
                //PhysicalStructure+
                //Created in each DataFile(ex. format)
                foreach (DataFile dataFileModel in studyUnit.DataFileModels)
                {
                    DataSet dataSetModel = studyUnit.FindDataSetModel(dataFileModel.DataSetId);
                    XElement physicalStructure = new XElement(p + TAG_PHYSICAL_STRUCTURE,
                        CreateIDAttribute(dataFileModel.Id),
                        CreateReferenceID(p + TAG_LOGICAL_PRODUCT_REFERENCE, LogicalProductId(), LogicalProductId()), //LogicalProductReference+
                        CreateNullable(p + TAG_FORMAT, dataFileModel.Format), //Format?
                        CreateNullable(p + TAG_DEFAULT_DELIMITER, Options.FindLabel(Options.Delimiters, dataFileModel.DelimiterCode)), //DefaultDelimiter?
                        new XElement(p + TAG_GROSS_RECORD_STRUCTURE,//GrossRecordStructure+
                            CreateIDAttribute(dataFileModel.GrossRecordStructureId),
                            CreateReferenceID(p + TAG_LOGICAL_RECORD_REFERENCE, dataSetModel.Id, LogicalProductId()), //ID of the data set LogicalRecordReference
                            new XElement(p + TAG_PHYSICAL_RECORD_SEGMENT, CreateIDAttribute(dataFileModel.PhysicalRecordSegment))));  //PhysicalRecordSegment+
                    physicalStructureScheme.Add(physicalStructure);
                }

                //RecordLayoutScheme*
                //DataFile each
                XElement recordLayoutScheme = new XElement(p + TAG_RECORD_LAYOUT_SCHEME, 
                    CreateIDAttribute(RecordLayoutSchemeId()),
                    CreateVersionAttribute(), 
                    CreateAgencyAttribute()
                    );
                foreach (DataFile dataFileModel in studyUnit.DataFileModels)
                {
                    DataSet dataSetModel = studyUnit.FindDataSetModel(dataFileModel.DataSetId);
                    if (dataSetModel.VariableGuids.Count > 0) {

                        XElement physicalStructureReference = CreateReferenceID(p + TAG_PHYSICAL_STRUCTURE_REFERENCE, dataFileModel.Id, PhysicalStructureSchemeId());
                        physicalStructureReference.Add(new XElement(p + TAG_PHYSICAL_RECORD_SEGMENT_USED, dataFileModel.PhysicalRecordSegment));

                        //RecordLayout+
                        XElement baseRecordLayout = new XElement(p + TAG_RECORD_LAYOUT,
                            CreateIDAttribute(dataFileModel.RecordLayoutId),
                            CreateUserID(),
                            physicalStructureReference, //PhysicalStructureReference
                            new XElement(p + TAG_CHARACTER_SET, "UTF-8"),
                            new XElement(p + TAG_ARRAY_BASE, "0"));
                        recordLayoutScheme.Add(baseRecordLayout);

                        //DataItem+
                        int i = 0;
                        foreach (string id in dataSetModel.VariableGuids)
                        {
                            XElement dataItem = new XElement(p + TAG_DATA_ITEM,
                                CreateReferenceID(p + TAG_VARIABLE_REFERENCE, id, VariableSchemeId()), //VariableReference
                                new XElement(p + TAG_PHYSICAL_LOCATION, //PhysicalLocation
                                    new XElement(p + TAG_ARRAY_POSITION, i++))); //ArrayPosition?
                            baseRecordLayout.Add(dataItem);
                        }
                    }
                }
                physicalDataProduct.Add(EmptyToNull(recordLayoutScheme));
            }
            return physicalDataProduct;
        }

        private XElement CreateSummaryStatistic(string name, object value)
        {
            XElement summaryStatictics = new XElement(pi + TAG_SUMMARY_STATISTIC);
            XElement meanType = new XElement(pi + TAG_SUMMARY_STATISTIC_TYPE, name);
            summaryStatictics.Add(meanType);
            XElement weighted = new XElement(pi + TAG_WEIGHTED, false);
            summaryStatictics.Add(weighted);
            XElement meanValue = new XElement(pi + TAG_VALUE, value);
            summaryStatictics.Add(meanValue);
            return summaryStatictics;
        }

        private XElement CreateNumerVariableStatisticsElement(StatisticsInfo statisticsInfoModel)
        {
            VariableVM variable = studyUnit.FindVariable(statisticsInfoModel.VariableId);
            if (variable == null)
            {
                return null;
            }

            XElement variableStatistics = new XElement(pi + TAG_VARIABLE_STATISTICS);

            XElement variableReference = CreateReferenceID(pi + TAG_VARIABLE_REFERENCE, statisticsInfoModel.VariableId, VariableSchemeId());
            variableStatistics.Add(variableReference);

            XElement totalResponse = new XElement(pi + TAG_TOTAL_RESPONSES, statisticsInfoModel.SummaryInfo.TotalCases);
            variableStatistics.Add(totalResponse);

            SummaryInfo summaryInfo = statisticsInfoModel.SummaryInfo;
            variableStatistics.Add(CreateSummaryStatistic(DDI_MEAN, summaryInfo.Mean));
            variableStatistics.Add(CreateSummaryStatistic(DDI_MEDIAN, summaryInfo.Median));
            variableStatistics.Add(CreateSummaryStatistic(DDI_VALID_CASES, summaryInfo.ValidCases));
            variableStatistics.Add(CreateSummaryStatistic(DDI_INVALID_CASES, summaryInfo.InvalidCases));
            variableStatistics.Add(CreateSummaryStatistic(DDI_MINIMUM, summaryInfo.Minimum));
            variableStatistics.Add(CreateSummaryStatistic(DDI_MAXIMUM, summaryInfo.Maximum));
            variableStatistics.Add(CreateSummaryStatistic(DDI_STANDARD_DEVIATION, summaryInfo.StandardDeviation));

            return variableStatistics;
        }

        private XElement CreateCategoryElement(string type, object value)
        {
            XElement categoryStatistic = new XElement(pi + TAG_CATEGORY_STATISTIC);
            XElement categoryStatisticType = new XElement(pi + TAG_CATEGORY_STATISTIC_TYPE, type);
            categoryStatistic.Add(categoryStatisticType);
            XElement weighted = new XElement(pi + TAG_WEIGHTED, false);
            categoryStatistic.Add(weighted);
            XElement valueElem = new XElement(pi + TAG_VALUE, value);
            categoryStatistic.Add(valueElem);
            return categoryStatistic;
        }

        private XElement CreateCategoryStatisticsElement(CategoryInfo categoryInfo)
        {
            XElement categoryStatistics = new XElement(pi + TAG_CATEGORY_STATISTICS);

            XElement categoryValue = new XElement(pi + TAG_CATEGORY_VALUE, categoryInfo.CodeValue);
            categoryStatistics.Add(categoryValue);

            XElement frequency = CreateCategoryElement(DDI_FREQUENCY, categoryInfo.Frequency);
            categoryStatistics.Add(frequency);
            XElement percent = CreateCategoryElement(DDI_PERCENT, categoryInfo.Percent);
            categoryStatistics.Add(percent);

            return categoryStatistics;
        } 

        private XElement CreateSingleAnswerOrDateTimeVariableStatisticsElement(StatisticsInfo statisticsInfoModel)
        {
            VariableVM variable = studyUnit.FindVariable(statisticsInfoModel.VariableId);
            if (variable == null)
            {
                return null;
            }

            // single answer type statistics
            XElement variableStatistics = new XElement(pi + TAG_VARIABLE_STATISTICS);

            XElement variableReference = CreateReferenceID(pi + TAG_VARIABLE_REFERENCE, statisticsInfoModel.VariableId, VariableSchemeId());
            variableStatistics.Add(variableReference);

            CategoryInfoCollection[] collections = CategoryInfoCollection.Create(statisticsInfoModel);
            CategoryInfoCollection totalCollection = collections[0];
            XElement totalResponse = new XElement(pi + TAG_TOTAL_RESPONSES, totalCollection.TotalResponse);
            variableStatistics.Add(totalResponse);

            // missing values
            CategoryInfoCollection missingCollection = collections[2];
            foreach (CategoryInfo missingInfo in missingCollection)
            {
                CodeVM code = variable.FindCodeByValue(missingInfo.CodeValue);
                if (code != null)
                {
                    XElement missingCategoryReference = CreateReferenceID(pi + TAG_EXCLUDED_MISSING_CATEGORY_REFERENCE, code.CodeId, variable.Response.CodeScheme.Id);
                    variableStatistics.Add(missingCategoryReference);
                }
            }

            // normal values
            foreach (CategoryInfo categoryInfo in totalCollection)
            {
                XElement categoryStatistics =  CreateCategoryStatisticsElement(categoryInfo);
                variableStatistics.Add(categoryStatistics);
            }
            return variableStatistics;
        }

        private List<CategoryInfo> ConvertMultiAnswerCategoryInfo(CategoryInfo categoryInfo, int rowCount)
        {
            List<CategoryInfo> categoryInfos = new List<CategoryInfo>();

            CategoryInfo selectedCategoryInfo = new CategoryInfo();
            selectedCategoryInfo.CodeValue = EDOConstants.SELECTED_CODE_VALUE;
            selectedCategoryInfo.Frequency = categoryInfo.Frequency;
            selectedCategoryInfo.Percent = StatisticsUtils.ToPercent(selectedCategoryInfo.Frequency, rowCount);
            categoryInfos.Add(selectedCategoryInfo);

            CategoryInfo unselectedCategoryInfo = new CategoryInfo();
            unselectedCategoryInfo.CodeValue = EDOConstants.UNSELECTED_CODE_VALUE;
            unselectedCategoryInfo.Frequency = rowCount - selectedCategoryInfo.Frequency;
            unselectedCategoryInfo.Percent = 100.0m - selectedCategoryInfo.Percent;
            categoryInfos.Add(unselectedCategoryInfo);

            return categoryInfos;
        }

        private XElement CreateMultiAnswerVariableStatisticsElement(VariableVM variable, StatisticsInfo statisticsInfoModel, CategoryInfo orgCategoryInfo)
        {
            XElement variableStatistics = new XElement(pi + TAG_VARIABLE_STATISTICS);

            XElement variableReference = CreateReferenceID(pi + TAG_VARIABLE_REFERENCE, variable.Id, VariableSchemeId());
            variableStatistics.Add(variableReference);



//            XElement categoryStatistics = CreateCategoryStatisticsElement(categoryInfo);
//            variableStatistics.Add(categoryStatistics);
            List<CategoryInfo> categoryInfos = ConvertMultiAnswerCategoryInfo(orgCategoryInfo, statisticsInfoModel.SummaryInfo.TotalCases);
            foreach (CategoryInfo categoryInfo in categoryInfos)
            {
                XElement categoryStatistics = CreateCategoryStatisticsElement(categoryInfo);
                variableStatistics.Add(categoryStatistics);
            }
            return variableStatistics;
        }

        private List<XElement> CreateMultiAnswerVariableStatisticsElements(StatisticsInfo statisticsInfoModel)
        {
            QuestionVM question = studyUnit.FindQuestion(statisticsInfoModel.QuestionId);
            if (question == null)
            {
                return null;
            }
            List<XElement> statisticsElements = new List<XElement>();
            List<VariableVM> variables = studyUnit.FindVariablesByQuestionId(statisticsInfoModel.QuestionId);
            ObservableCollection<CodeVM> codes = question.Response.Codes;
            int totalRowCount = statisticsInfoModel.SummaryInfo.TotalCases;
            for (int i = 0; i < variables.Count; i++)
            {
                if (i >= codes.Count || i >= statisticsInfoModel.CategoryInfos.Count)
                {
                    break;
                }
                VariableVM variable = variables[i];
                CodeVM code = codes[i];
                CategoryInfo categoryInfo = statisticsInfoModel.CategoryInfos[i];
                XElement statistics = CreateMultiAnswerVariableStatisticsElement(variable, statisticsInfoModel, categoryInfo);
                statisticsElements.Add(statistics);

            }
            return statisticsElements;
        }

        private XElement CreateStatistics()
        {
            XElement statistics = new XElement(pi + TAG_STATISTICS);
            foreach (StatisticsInfo statisticsInfoModel in studyUnit.StatisticsInfoModels)
            {
                if (statisticsInfoModel.IsTypeNumber)
                {
                    // number type statistics
                    statistics.Add(CreateNumerVariableStatisticsElement(statisticsInfoModel));
                }
                else if (statisticsInfoModel.IsTypeChoicesSingleAnswer)
                {
                    // single answer
                    statistics.Add(CreateSingleAnswerOrDateTimeVariableStatisticsElement(statisticsInfoModel));
                } else if (statisticsInfoModel.IsTypeDateTime)
                {
                    // datetime
                    statistics.Add(CreateSingleAnswerOrDateTimeVariableStatisticsElement(statisticsInfoModel));
                }
                else if (statisticsInfoModel.IsTypeChoicesMultipleAnswer)
                {
                    // multi answer
                    statistics.Add(CreateMultiAnswerVariableStatisticsElements(statisticsInfoModel));
                }
            }
            return statistics;
        }

        private XElement CreatePhysicalInstance(DataFile dataFileModel)
        {
            //d:PhysicalInstance
            DataSet dataSetModel = studyUnit.FindDataSetModel(dataFileModel.DataSetId);
            XElement physicalInstance = new XElement(pi + TAG_PHYSICAL_INSTANCE,
                CreateIDAttribute(dataFileModel.PhysicalInstanceId),
                CreateVersionAttribute(), 
                CreateAgencyAttribute(),
                CreateReferenceID(pi + TAG_RECORD_LAYOUT_REFERENCE, dataFileModel.RecordLayoutId, RecordLayoutSchemeId()), //RecordLayoutReference+
                new XElement(pi + TAG_DATA_FILE_IDENTIFICATION,  //DataFileIdentification+
                    CreateIDAttribute(dataFileModel.DataFileIdentificationId),
                    new XElement(pi + TAG_URI, dataFileModel.Uri)),//URI+
                new XElement(pi + TAG_GROSS_FILE_STRUCTURE, //GrossFileStructure?
                    CreateIDAttribute(dataFileModel.GrossFileStructureId),
                    new XElement(pi + TAG_CASE_QUANTITY, "0"), //CaseQuantity?
                    new XElement(pi + TAG_OVERALL_RECORD_COUNT, dataSetModel.VariableGuids.Count))); //OverallRecordCount?

            if (studyUnit.DefaultDataSetId == dataFileModel.DataSetId)
            {
                // create statistics info
                physicalInstance.Add(CreateStatistics());
            }
            return physicalInstance;
        }


        private List<XElement> CreatePhysicalInstances()
        {
            List<XElement> physicalInstances = new List<XElement>();
            foreach (DataFile dataFileModel in studyUnit.DataFileModels)
            {
                physicalInstances.Add(CreatePhysicalInstance(dataFileModel));
            }
            return physicalInstances;
        }

        private bool IsMatchBookType(Book book, BookRelationType type)
        {
            if (book.BookRelations.Count == 0)
            {
                return BookRelationType.Abstract == type;
            }
            foreach (BookRelation relation in book.BookRelations)
            {
                if (relation.BookRelationType == type)
                {
                    return true;
                }
            }
            return false;
        }

        private List<Book> GetBooks(BookRelationType type)
        {
            List<Book> books = new List<Book>();
            foreach (Book bookModel in studyUnit.BookModels)
            {
                if (IsMatchBookType(bookModel, type))
                {
                    //export all in accordance with the relevant contained
                    books.Add(bookModel);
                }
            }
            return books;
        }

        private XElement CreateOtherMaterial(Book book, BookRelationType type)
        {
            XElement otherMaterial = new XElement(r + TAG_OTHER_MATERIAL,
                CreateIDAttribute(book.GetBookId(type)),
                new XAttribute(ATTR_TYPE,GetDDIBookType(book.BookTypeCode)));

            XElement citation = new XElement(r + TAG_CITATION,
                new XElement(r + TAG_TITLE, book.Title)
                );
            otherMaterial.Add(citation);
            if (!string.IsNullOrEmpty(book.Author))
            {
                citation.Add(new XElement(r + TAG_CREATOR, book.Author));                    
            }
            string publisher = BuildPublisher(book);
            if (!string.IsNullOrEmpty(publisher))
            {
                citation.Add(new XElement(r + TAG_PUBLISHER, publisher));
            }
            if (!string.IsNullOrEmpty(book.Editor))
            {
                citation.Add(new XElement(r + TAG_CONTRIBUTOR, book.Editor));                   
            }
            string simpleDate =  ToSimpleDate(book.AnnouncementDate);
            if (!string.IsNullOrEmpty(simpleDate))
            {
                citation.Add(
                    new XElement(r + TAG_PUBLICATION_DATE,
                        new XElement(r + TAG_SIMPLE_DATE, simpleDate)));
            }
            if (!string.IsNullOrEmpty(book.Language))
            {
                citation.Add(new XElement(r + TAG_LANGUAGE, book.Language));
            }
            string identifier = BuildIdentifier(book);
            XElement identifierElement = null;
            if (!string.IsNullOrEmpty(identifier)) {
                identifierElement = new XElement(dc + TAG_DC_IDENTIFIER, identifier);
            }
            XElement descriptionElement = null;
            if (!string.IsNullOrEmpty(book.Summary))
            {
                descriptionElement = new XElement(dc + TAG_DC_DESCRIPTION, book.Summary);
            }
            if (identifierElement != null || descriptionElement != null)
            {
                XElement dcelements = new XElement(dce + TAG_DCELEMENTS);
                citation.Add(dcelements);
                dcelements.Add(identifierElement);
                dcelements.Add(descriptionElement);
            }

            if (!string.IsNullOrEmpty(book.Url))
            {
                otherMaterial.Add(new XElement(r + TAG_EXTERNAL_URL_REFERENCE, book.Url));
            }

            foreach (BookRelation relation in book.BookRelations)
            {
                string schemeId = null;
                string metaDataId = relation.MetadataId;
                if (relation.IsBookRelationTypeAbstract)
                {
                    schemeId = studyUnit.Id;
                    metaDataId = studyUnit.Id;
                    otherMaterial.Add(new XElement(r + TAG_RELATIONSHIP,
                        CreateReferenceModuleID(r + TAG_RELATED_TO_REFERENCE, metaDataId, schemeId)));
                }
                else
                {
                    if (relation.IsBookRelationTypeConcept)
                    {
                        ConceptScheme conceptScheme = studyUnit.StudyUnitModel.FindConceptSchemeByConceptId(metaDataId);
                        schemeId = conceptScheme.Id;
                    }
                    else if (relation.IsBookRelationTypeQuestion)
                    {
                        schemeId = studyUnit.StudyUnitModel.QuestionSchemeId;
                    }
                    else if (relation.IsBookRelationTypeVariable)
                    {
                        schemeId = studyUnit.StudyUnitModel.VariableScheme.Id;
                    }
                    otherMaterial.Add(new XElement(r + TAG_RELATIONSHIP,
                        CreateReferenceID(r + TAG_RELATED_TO_REFERENCE, relation.MetadataId, schemeId)));
                }
            }

            return otherMaterial;
        }

        private List<XElement> CreateOtherMaterials(BookRelationType type)
        {
            List<XElement> otherMaterials = new List<XElement>();
            List<Book> books = GetBooks(type);
            foreach (Book book in books)
            {
                otherMaterials.Add(CreateOtherMaterial(book, type));
            }
            return otherMaterials;
        }

        private XElement CreateStudyUnit()
        {
            ///// Output StudyUnit tag
            XElement su = new XElement(s + TAG_STUDY_UNIT, 
                CreateIDAttribute(studyUnit.Id),
                CreateVersionAttribute(),
                CreateAgencyAttribute());
            //r:Citation
            su.Add(new XElement(r + TAG_CITATION, new XElement(r + TAG_TITLE, studyUnit.AbstractModel.Title)));
            //Abstract+
            su.Add(new XElement(s + TAG_ABSTRACT, CreateIDAttribute(studyUnit.AbstractModel.SummaryId), CreateContent(studyUnit.AbstractModel.Summary)));
            //r:UniverseReference+
            Universe universe = studyUnit.FindMainUniverseModel();
            if (universe == null)
            {
                AddError(Resources.InputUniverse, studyUnit.SamplingForm); //Enter Universe
            }
            else
            {
                XElement universeReference = CreateReferenceID(r + TAG_UNIVERSE_REFERENCE, universe.Id, UniverseSchemeId());
                su.Add(universeReference);
            }
            //r:FundingInformation*
            foreach (FundingInfo fundingInfo in studyUnit.FundingInfoModels)
            {
                su.Add(CreateFundingInformation(fundingInfo));
            }
            //r:Purpose
            su.Add(new XElement(s + TAG_PURPOSE, CreateIDAttribute(studyUnit.AbstractModel.PurposeId), CreateContent(studyUnit.AbstractModel.Purpose))); 
            //r:Coverage?
            su.Add(CreateCoverage());
            //r:OtherMaterial
            su.Add(CreateOtherMaterials(BookRelationType.Abstract));
            //c:ComceptualComponent*
            su.Add(CreateConceptualComponent(false));
            //d:DataCollection*
            su.Add(CreateDataCollection());
            //l:BaseLogicalProduct*
            su.Add(CreateLogicalProduct());
            //p:PhysicalDataProduct*
            su.Add(CreatePhysicalDataProduct());
            //pi:PhysicalInstance*
            su.Add(CreatePhysicalInstances());
            //a:Archive?
            su.Add(CreateArchive());
            return su;
        }

        public void WriteStudyUnit(string path, StudyUnitVM studyUnit)
        {
            Debug.Assert(!string.IsNullOrEmpty(path));
            this.studyUnit = studyUnit;

            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = true;
            xws.Encoding = Encoding.UTF8;
            using (XmlWriter xw = XmlWriter.Create(path, xws))
            {
                XDocument doc = new XDocument(
                    DECLARATION,
                    new XElement(ddi + TAG_DDI_INSTANCE,
                        CreateIDAttribute(studyUnit.StudyUnitModel.InstanceId),
                        CreateVersionAttribute(),
                        CreateAgencyAttribute(),
                        new XAttribute(XNamespace.Xmlns + "ddi", ddi),
                        new XAttribute(XNamespace.Xmlns + "s", s),
                        new XAttribute(XNamespace.Xmlns + "r", r),
                        new XAttribute(XNamespace.Xmlns + "a", a),
                        new XAttribute(XNamespace.Xmlns + "c", c),
                        new XAttribute(XNamespace.Xmlns + "d", d),
                        new XAttribute(XNamespace.Xmlns + "l", l),
                        new XAttribute(XNamespace.Xmlns + "p", p),
                        new XAttribute(XNamespace.Xmlns + "pi", pi),
                        new XAttribute(XNamespace.Xmlns + "dce", dce),
                        new XAttribute(XNamespace.Xmlns + "dc", dc),
                        CreateStudyUnit())
                );
                doc.WriteTo(xw);
            }
        }

        private List<CompareItem> DoCreateCompareItem(GroupVM group, List<StudyUnitVM> studyUnits, CompareTable compareTable)
        {
            //**Return the array of CompareLitem looking up the group comparison table
            //Use CompareRow on the screen. it has an array of CompareCell inside
            //has a function to convert it to an array of pair format(CompareItem) to be used in DDI format
            //
            //**ID stored in CompareItem
            //CompareTable will be one onf the following: ConceptScheme-ID、Concept-ID、Variable-ID.
            List<CompareItem> items = new List<CompareItem>();
            foreach (CompareRow row in compareTable.Rows)
            {
                //Get and save all the valid ○ cells
                //Valid ○ cell is "the cell where ○ is set and which includes line variable in StudyUnit"
                //
                // In the case when V1 is included in S1 and V2 is included in S2, the value placed in * should be ignored.
                //
                //    S1     S2
                // V1        *
                // V2 *

                //Save ○
                List<CompareCell> matchCells = row.MatchValidCells();
                if (matchCells.Count > 1)
                {
                    //Don't save if ○ of comparison does not exist. in th e case of ○, it must be present in pairs on the same line as follows
                    //     S1    S2
                    // V1  ○    ○

                    //get the variable corresponding to the cell
                    //Needs to be processed in List.
                    List<GroupId> sourceIds = row.RelatedGroupIds(matchCells[0]);
                    for (int i = 1; i < matchCells.Count; i++)
                    {
                        List<GroupId> targetIds = row.RelatedGroupIds(matchCells[i]);
                        foreach (GroupId sourceId in sourceIds)
                        {
                            foreach (GroupId targetId in targetIds)
                            {
                                CompareItem item = CompareItem.CreateMatch(sourceId, targetId, row.Memo);
                                items.Add(item);
                            }
                        }
                    }
                }
                //Save △
                List<CompareCell> partialMatchCells = row.PartialMatchValidCells();
                foreach (CompareCell targetCell in partialMatchCells)                {
                    List<GroupId> targetIds = row.RelatedGroupIds(targetCell);

                    //In the case of △, the target line have to be found.
                    //search the title of the selected target variable
                    //(Only one line should be found because the titles are unique)
                    CompareRow sourceRow = compareTable.FindRowByTitle(targetCell.TargetTitle);
                    //Valid all cells in the target row is processed.
                    //    S1      S2      S3
                    //V1  △(V2)
                    //V2        △(V1) △(V1)
                    List<CompareCell> validCells = sourceRow.ValidCells();
                    foreach (CompareCell sourceCell in validCells)
                    {
                        List<GroupId> sourceIds = sourceRow.RelatedGroupIds(sourceCell);
                        foreach (GroupId sourceId in sourceIds)
                        {
                            foreach (GroupId targetId in targetIds)
                            {
                                CompareItem item = CompareItem.CreatePartialMatch(sourceId, targetId, row.Memo);
                                items.Add(item);
                            }
                        }
                    }
                }
                //**Implicit relationships in the documentation is not saved for now
            }
            return items;
        }

        private GroupId FindParentConceptSchemeIdByConceptId(List<StudyUnitVM> studyUnits, GroupId conceptId)
        {
            StudyUnitVM studyUnit = StudyUnitVM.Find(studyUnits, conceptId.StudyUnitId);
            ConceptScheme conceptScheme = studyUnit.StudyUnitModel.FindConceptSchemeByConceptId(conceptId.Id);
            return new GroupId(conceptId.StudyUnitId, conceptScheme.Id);
        }

        private GroupId FindParentConceptSchemeIdByVariableId(List<StudyUnitVM> studyUnits, GroupId variableId)
        {
            StudyUnitVM studyUnit = StudyUnitVM.Find(studyUnits, variableId.StudyUnitId);
            ConceptScheme conceptScheme = studyUnit.StudyUnitModel.FindConceptSchemeByVariableId(variableId.Id);
            return new GroupId(variableId.StudyUnitId, conceptScheme.Id);
        }

        private XElement CreateConceptSchemeElement(CompareItem item, string prefix)
        {
            //cm:ConceptMap
            XElement element = new XElement(cm + TAG_CONCEPT_MAP,
                CreateIDAttribute(item.IdWithPrefix(prefix)),
                CreateReferenceID(cm + TAG_SOURCE_SCHEME_REFERENCE, item.SourceId.Id),
                CreateReferenceID(cm + TAG_TARGET_SCHEME_REFERENCE, item.TargetId.Id),
                new XElement(cm + TAG_CORRESPONDENCE, //Correspondence
                    new XElement(cm + TAG_COMMONALITY, item.Memo), //Commonality+
                    new XElement(cm + TAG_DIFFERENCE, item.Memo), //Difference+
                    new XElement(cm + TAG_COMMONALITY_WEIGHT, item.Weight) //CommonalityWeight?
                    ));
            return element;
        }

        private XElement CreateItemElement(CompareItem item)
        {
            XElement childElement = new XElement(cm + TAG_ITEM_MAP,
                new XElement(cm + TAG_SOURCE_ITEM, item.SourceItemId),
                new XElement(cm + TAG_TARGET_ITEM, item.TargetItemId),
                new XElement(cm + TAG_CORRESPONDENCE,
                    new XElement(cm + TAG_COMMONALITY, item.Memo),
                    new XElement(cm + TAG_DIFFERENCE, item.Memo),
                    new XElement(cm + TAG_COMMONALITY_WEIGHT, item.Weight)
                    ));
            return childElement;
        }

        private XElement CreateVariableElement(CompareItem item)
        {
            XElement element = new XElement(cm + TAG_VARIABLE_MAP,
                CreateIDAttribute(item.Id),
                CreateReferenceID(cm + TAG_SOURCE_SCHEME_REFERENCE, item.SourceId.Id),
                CreateReferenceID(cm + TAG_TARGET_SCHEME_REFERENCE, item.TargetId.Id),
                new XElement(cm + TAG_CORRESPONDENCE,
                    new XElement(cm + TAG_COMMONALITY, item.Memo),
                    new XElement(cm + TAG_DIFFERENCE, item.Memo)
                    ));
            return element;
        }

        private List<XElement> CreateConceptSchemeMap(GroupVM group, List<StudyUnitVM> studyUnits)
        {
            //It is a complex process to save tags by comparing ConceptScheme and Concept, because parent-child relationship have to be considered.
            //Process flow:
            //*Get the comparison result of Concept.
            //*Put together the comparison result for each of ConceptScheme with the same correspondence, the comparison result of Concept
            //*Get the comparison result of ConceptScheme.
            //*Output the comparison result of ConceptScheme.
            //*Write a comparison result of Concept remaining (in ConceptScheme level comparison result does not exist but in Concept level it may exist).

            //Get a correspondence of Concept
            List<CompareItem> conceptItems = DoCreateCompareItem(group, studyUnits, group.GroupModel.ConceptCompareTable);
            Dictionary<string, List<CompareItem>> conceptMap = new Dictionary<string,List<CompareItem>>();
            //Those having a (parent) ConceptScheme same comparison source and comparison target must belong to the same tag, so it is sorted by by using conceptMap
            foreach (CompareItem item in conceptItems)
            {
                //ParentId id formed as Concept_XXXXX_YYYYY
                item.ParentIdPrefix = "Concept";
                item.ParentSourceId = FindParentConceptSchemeIdByConceptId(studyUnits, item.SourceId);
                item.ParentTargetId = FindParentConceptSchemeIdByConceptId(studyUnits, item.TargetId);
                List<CompareItem> list = null;
                if (conceptMap.ContainsKey(item.ParentId))
                {
                    list = conceptMap[item.ParentId];
                } else {
                    list = new List<CompareItem>();
                    conceptMap[item.ParentId] = list;
                }
                list.Add(item);
            }
            //Get a correspondence of ConceptScheme
            List<CompareItem> conceptSchemeItems = DoCreateCompareItem(group, studyUnits, group.GroupModel.ConceptSchemeCompareTable);
            List<XElement> elements = new List<XElement>();
            foreach (CompareItem item in conceptSchemeItems)
            {
                XElement element = CreateConceptSchemeElement(item, "Concept");
                elements.Add(element);
                if (conceptMap.ContainsKey(item.Id))
                {
                    List<CompareItem> concepts = conceptMap[item.Id];
                    foreach (CompareItem childItem in concepts)
                    {
                        XElement childElement = CreateItemElement(childItem);
                        element.Add(childElement);
                    }
                    conceptMap.Remove(item.Id);
                }
            }
            //Save the rest of ConceptMap
            foreach (KeyValuePair<string, List<CompareItem>> pair in conceptMap)
            {
                List<CompareItem> concepts = pair.Value;
                CompareItem first = concepts.First();
                CompareItem parentItem = new CompareItem(
                    first.ParentSourceId,
                    first.ParentTargetId,
                    Resources.GenerateForCmpareConcept, "0"); //Generated to compare concepts
                XElement element = CreateConceptSchemeElement(parentItem, "Variable");
                elements.Add(element);
                foreach (CompareItem childItem in concepts)
                {
                    XElement childElement = CreateItemElement(childItem);
                    element.Add(childElement);
                }
            }

            return elements;
        }

        private List<XElement> CreateVariableMap(GroupVM group, List<StudyUnitVM> studyUnits)
        {
            List<CompareItem> variableItems = DoCreateCompareItem(group, studyUnits, group.GroupModel.VariableCompareTable);
            Dictionary<string, List<CompareItem>> variableMap = new Dictionary<string, List<CompareItem>>();
            foreach (CompareItem item in variableItems)
            {
                item.ParentSourceId = FindParentConceptSchemeIdByVariableId(studyUnits, item.SourceId);
                item.ParentTargetId = FindParentConceptSchemeIdByVariableId(studyUnits, item.TargetId);
                List<CompareItem> list = null;
                if (variableMap.ContainsKey(item.ParentId))
                {
                    list = variableMap[item.ParentId];
                }
                if (list == null)
                {
                    list = new List<CompareItem>();
                    variableMap[item.ParentId] = list;
                }
                list.Add(item);
            }

            List<XElement> elements = new List<XElement>();

            foreach (KeyValuePair<string, List<CompareItem>> pair in variableMap)
            {
                List<CompareItem> variables = pair.Value;
                CompareItem first = variables.First();
                CompareItem parentItem = new CompareItem(
                    first.ParentSourceId,
                    first.ParentTargetId,
                    Resources.GenerateForCmpareVariable, "0"); //Generated to compare variables
                XElement element = CreateVariableElement(parentItem);
                elements.Add(element);
                foreach (CompareItem childItem in variables)
                {
                    XElement childElement = CreateItemElement(childItem);
                    element.Add(childElement);
                }
            }
            return elements;
        }

        private XElement CreateGroup(GroupVM group, List<StudyUnitVM> studyUnits)
        {
            Group groupModel = group.GroupModel;
            XElement gr = new XElement(g + TAG_GROUP,
                CreateIDAttribute(groupModel.Id),
                CreateVersionAttribute(),
                CreateAgencyAttribute(),
                new XAttribute(ATTR_DATA_SET, groupModel.DataSetCode),
                new XAttribute(ATTR_GEOGRAPHY, groupModel.GeographyCode),
                new XAttribute(ATTR_INSTRUMENT,groupModel.InstrumentCode),
                new XAttribute(ATTR_LANGUAGE, groupModel.LanguageCode),
                new XAttribute(ATTR_PANEL, groupModel.PanelCode),
                new XAttribute(ATTR_TIME, groupModel.TimeCode),
                new XElement(r + TAG_CITATION, new XElement(r + TAG_TITLE, group.Title)),
                new XElement(g + TAG_PURPOSE,
                    CreateIDAttribute(groupModel.PurposeId),
                    CreateContent(groupModel.Purpose))
                );

            //Output common Universe and Variable Concept
            //StudyUnit sharedStudyUnitModel = group.GroupModel.SharedStudyUnit;
            //XElement concepts = new XElement(g + TAG_CONCEPT);
            //XElement conceptualComponent = new XElement(c + TAG_CONCEPTUAL_COMPONENT);

            StudyUnitVM sharedStudyUnit = new StudyUnitVM(group.Main, group.GroupModel.SharedStudyUnit);
            this.studyUnit = sharedStudyUnit;

            XElement conceptualComponent = CreateConceptualComponent(true);
            if (conceptualComponent != null) 
            {            
                XElement concepts = new XElement(g + TAG_CONCEPTS, conceptualComponent);
                gr.Add(concepts);
            }
            XElement logicalProduct = CreateLogicalProduct();
            if (logicalProduct != null)
            {
                XElement logical = new XElement(g + TAG_LOGICAL_PRODUCT, logicalProduct);
                gr.Add(logical);
            }
            return gr;
        }

        private XElement CreateComparison(GroupVM group, List<StudyUnitVM> studyUnits)
        {
            //Save comparison result of Group
            Group groupModel = group.GroupModel;
            //cm::Comparison
            XElement comparison = new XElement(cm + TAG_COMPARISON, 
                CreateIDAttribute(groupModel.ComparisonId),
                CreateVersionAttribute(),
                CreateAgencyAttribute()
                );
            //Comparison list of ConceptScheme and Concept
            List<XElement> conceptSchemeMaps = CreateConceptSchemeMap(group, studyUnits);
            foreach (XElement conceptSchemeMap in conceptSchemeMaps)
            {
                comparison.Add(conceptSchemeMap);
            }
            //List of comparison result of Variable
            List<XElement> variableMaps = CreateVariableMap(group, studyUnits);
            foreach (XElement variableMap in variableMaps)
            {
                comparison.Add(variableMap);
            }
            return comparison;
        }

        private XElement CreateSharedStudyUnit(GroupVM group)
        {
            XElement su = new XElement(g + TAG_STUDY_UNIT);
            StudyUnitVM sharedStudyUnit = new StudyUnitVM(group.Main, group.GroupModel.SharedStudyUnit);
            this.studyUnit = sharedStudyUnit;
            su.Add(CreateStudyUnit());
            if (HasError)
            {
                DumpError();
            }
            int errorCount = RemoveError(sharedStudyUnit);
            if (errorCount > 0)
            {
                //Do not export if there is an error(Can not be distinguished from other StudyUnit?)
                return null;
            }
            return su;
        }

        public void WriteGroup(string path, GroupVM group)
        {
            Debug.Assert(!string.IsNullOrEmpty(path));
            var studyUnits = group.StudyUnits;

            XmlWriterSettings xws = new XmlWriterSettings();
            xws.Indent = true;
            xws.Encoding = Encoding.UTF8;
            using (XmlWriter xw = XmlWriter.Create(path, xws))
            {
                XElement gr = CreateGroup(group, studyUnits);
                foreach (StudyUnitVM studyUnit in studyUnits)
                {
                    XElement su = new XElement(g + TAG_STUDY_UNIT);
                    gr.Add(su);
                    this.studyUnit = studyUnit;
                    su.Add(CreateStudyUnit());
                }
                gr.Add(CreateComparison(group, studyUnits));

                XDocument doc = new XDocument(
                    DECLARATION,
                    new XElement(ddi + TAG_DDI_INSTANCE,
                        CreateIDAttribute(group.GroupModel.InstanceId),
                        CreateVersionAttribute(),
                        CreateAgencyAttribute(),
                        new XAttribute(XNamespace.Xmlns + "ddi", ddi),
                        new XAttribute(XNamespace.Xmlns + "s", s),
                        new XAttribute(XNamespace.Xmlns + "r", r),
                        new XAttribute(XNamespace.Xmlns + "a", a),
                        new XAttribute(XNamespace.Xmlns + "c", c),
                        new XAttribute(XNamespace.Xmlns + "d", d),
                        new XAttribute(XNamespace.Xmlns + "l", l),
                        new XAttribute(XNamespace.Xmlns + "p", p),
                        new XAttribute(XNamespace.Xmlns + "pi", pi),
                        new XAttribute(XNamespace.Xmlns + "g", g),
                        new XAttribute(XNamespace.Xmlns + "cm", cm),
                        new XAttribute(XNamespace.Xmlns + "dce", dce),
                        new XAttribute(XNamespace.Xmlns + "dc", dc),
                        gr
                        ));
                doc.WriteTo(xw);
            }
        }
    }
}
