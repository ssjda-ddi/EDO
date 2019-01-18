using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using EDO.Core.Util;
using EDO.Properties;

namespace EDO.Core.Model
{
    public class StudyUnit : IFile, IIDPropertiesProvider
    {
        public static StudyUnit FindByConceptSchemeId(List<StudyUnit> studyUnits, string conceptSchemeId)
        {
            foreach (StudyUnit studyUnit in studyUnits)
            {
                if (studyUnit.ExistConceptScheme(conceptSchemeId))
                {
                    return studyUnit;
                }
            }
            return null;
        }

        public static StudyUnit FindByConceptId(List<StudyUnit> studyUnits, string conceptId)
        {
            foreach (StudyUnit studyUnit in studyUnits)
            {
                if (studyUnit.ExistConcept(conceptId))
                {
                    return studyUnit;
                }
            }
            return null;
        }

        public static StudyUnit FindByVariableId(List<StudyUnit> studyUnits, string variableId)
        {
            foreach (StudyUnit studyUnit in studyUnits)
            {
                if (studyUnit.ExistVariable(variableId))
                {
                    return studyUnit;
                }
            }
            return null;
        }

        public static List<string> GetStudyUnitGuids(List<StudyUnit> studyUnits)
        {
            List<string> guids = new List<string>();
            foreach (StudyUnit studyUnit in studyUnits)
            {
                guids.Add(studyUnit.Id);
            }
            return guids;
        }

        /// <summary>
        /// デフォルト設定を適用したStudyUnitを作成します。minimunをtrueにすると、最低限の設定のみを行ったStudyUnitを返します。
        /// </summary>
        /// <param name="minimum">設定を最低限にするかどうか</param>
        /// <returns>デフォルト設定を適用したStudyUnit</returns>
        public static StudyUnit CreateDefault(bool minimum = false)
        {
            StudyUnit studyUnit = new StudyUnit();
            int i = 1;
            if (!minimum)
            {
                foreach (Option option in Options.EventTypes)
                {
                    studyUnit.Events.Add(new Event() { Title = option.Label, No = i++ });
                }
                studyUnit.FundingInfos.Add(new FundingInfo());
                studyUnit.Samplings.Add(new Sampling());
                studyUnit.ConceptSchemes.Add(new ConceptScheme());
                ControlConstructScheme scheme = new ControlConstructScheme();
                studyUnit.ControlConstructSchemes.Add(scheme);
                studyUnit.DefaultControlConstructSchemeId = scheme.Id;
                DataSet dataSet = new DataSet();
                dataSet.Title = EDOConstants.LABEL_ALL;
                dataSet.Memo = Resources.AllDataSet; //All variables
                studyUnit.DefaultDataSetId = dataSet.Id;
                studyUnit.DataSets.Add(dataSet);

                dataSet.IsCreatedDataFile = true;
                DataFile dataFile = DataFile.createDataFile();
                dataFile.DataSetId = dataSet.Id;
                studyUnit.DataFiles.Add(dataFile);

                studyUnit.CreateBinaryCodeScheme();
            }

            studyUnit.Coverage = Coverage.CreateDefault();

            return studyUnit;
        }

        public string[] IdProperties
        {
            get
            {
                return new string[] 
                {
                    "Id", //1
                    "InstanceId", //2
                    "UniverseSchemeId", //3
                    "ConceptualComponentId", //4
                    "GeographicStructureSchemeId", //5
                    "DataCollectionId", //6
                    "QuestionSchemeId", //7
                    "LogicalProductId", //8
                    "DataRelationshipId", //9
                    "PhysicalDataProductId", //10
                    "PhysicalStructureSchemeId", //11
                    "RecordLayoutSchemeId", //12
                    "ArchiveId", //13
                    "OrganizationSchemeId", //14
                    "MethodologyId", //15
                    "ProcessingEventId", //16
                };
            }
        }

        public StudyUnit()
        {
            this.Id = IDUtils.NewGuid(); //1

            this.Events = new List<Event>(); //*1
            this.Members = new List<Member>(); //*2
            this.Organizations = new List<Organization>(); //*3
            this.Abstract = new Abstract(); //*4
            this.Coverage = new Coverage(); //*5
            this.FundingInfos = new List<FundingInfo>(); //*6
            this.Universes = new List<Universe>(); //*7
            this.Samplings = new List<Sampling>(); //*8
            this.ConceptSchemes = new List<ConceptScheme>(); //*9
            this.Questions = new List<Question>(); //*10
            this.CategorySchemes = new List<CategoryScheme>(); //*11
            this.CodeSchemes = new List<CodeScheme>(); //*12
            this.VariableScheme = new VariableScheme(); //*13
            this.Variables = new List<Variable>(); //*14
            this.DataSets = new List<DataSet>(); //*15
            this.DataFiles = new List<DataFile>(); //*16
            this.ControlConstructSchemes = new List<ControlConstructScheme>(); //*17
            this.QuestionGroups = new List<QuestionGroup>();
            this.Books = new List<Book>();
            this.StatisticsInfos = new List<StatisticsInfo>();

            this.InstanceId = IDUtils.NewGuid(); //2
            this.UniverseSchemeId = IDUtils.NewGuid(); //3
            this.ConceptualComponentId = IDUtils.NewGuid(); //4
            this.GeographicStructureSchemeId = IDUtils.NewGuid(); //5
            this.DataCollectionId = IDUtils.NewGuid(); //6
            this.QuestionSchemeId = IDUtils.NewGuid(); //7
            this.LogicalProductId = IDUtils.NewGuid(); //8
            this.DataRelationshipId = IDUtils.NewGuid(); //9
            this.PhysicalDataProductId = IDUtils.NewGuid(); //10
            this.PhysicalStructureSchemeId = IDUtils.NewGuid(); //11
            this.RecordLayoutSchemeId = IDUtils.NewGuid(); //12
            this.ArchiveId = IDUtils.NewGuid(); //13
            this.OrganizationSchemeId = IDUtils.NewGuid(); //14
            this.MethodologyId = IDUtils.NewGuid(); //15
            this.ProcessingEventId = IDUtils.NewGuid(); //16
        }
        public string InstanceId { get; set; } // For DDI output
        public string Id { get; set; }
        public List<Event> Events { get; set; }
        public Abstract Abstract { get; set; }
        public string Title { get { return Abstract.Title; } }
        public List<Member> Members { get; set; }
        public void AddMember(Member member)
        {
            this.Members.Add(member);
        }
        public List<Organization> Organizations { get; set; }
        public void AddOrganization(Organization organization)
        {
            this.Organizations.Add(organization);
        }

        public Coverage Coverage { get; set; }

        public List<FundingInfo> FundingInfos { get; set; }
        public List<Organization> FundingInfoOrganizations
        {
            get
            {
                List<Organization> organizations = new List<Organization>();
                foreach (FundingInfo fundingInfo in FundingInfos)
                {
                    if (fundingInfo.Organization != null)
                    {
                        organizations.Add(fundingInfo.Organization);
                    }
                }
                return organizations;
            }
        }

        public List<Universe> Universes { get; set; } // Sampling is left for the purpose to read only one file
        public Sampling Sampling { get; set; } // Sampling is left for the purpose to read only one file

        public List<Universe> AllUniverses
        {
            get
            {
                return Sampling.GetUniverses(Samplings);
            }
        }

        public List<Sampling> Samplings { get; set; }
        public Sampling GetSamplingAt(int index)
        {
            if (index < 0 || index >= Samplings.Count)
                return null;
            return Samplings[index];
        }

        public List<ConceptScheme> ConceptSchemes { get; set; }

        public List<Question> Questions { get; set; }

        public List<CategoryScheme> CategorySchemes { get; set; }

        public List<CodeScheme> CodeSchemes { get; set; }

        public VariableScheme VariableScheme { get; set; }

        public List<Variable> Variables { get; set; }

        public List<DataSet> DataSets { get; set; }

        public List<ControlConstructScheme> ControlConstructSchemes { get; set; }

        public List<QuestionGroup> QuestionGroups { get; set; }

        public string DefaultDataSetId { get; set; }

        public string DefaultDataFileId { get; set; }

        public string DefaultControlConstructSchemeId { get; set; }

        public string BinaryCodeSchemeId { get; set; }

        public string BinaryCategorySchemeId { get; set; }

        public List<DataFile> DataFiles { get; set; }

        public List<Book> Books { get; set; }

        public List<StatisticsInfo> StatisticsInfos { get; set; }

        [XmlIgnoreAttribute]
        public string PathName { get; set; }

        public Variable FindVariable(string variableId)
        {
            return Variable.Find(Variables, variableId);
        }

        public List<Variable> FindVariablesByConceptId(string conceptId)
        {
            return Variable.FindByConceptId(Variables, conceptId);
        }

        public bool ExistVariable(string variableId)
        {
            return FindVariable(variableId) != null;
        }        

        public DataSet FindDataSet(string dataSetId)
        {
            return DataSet.Find(DataSets, dataSetId);
        }

        public Universe FindMainUniverse()
        {
            return Sampling.FindMainUniverse(Samplings);
        }

        public Universe FindUniverse(string universeId)
        {
            return Sampling.FindUniverse(Samplings, universeId);
        }

        public ConceptScheme FindConceptScheme(string conceptSchemeId)
        {
            return ConceptScheme.FindConceptScheme(ConceptSchemes, conceptSchemeId);
        }

        public bool ExistConceptScheme(string conceptSchemeId)
        {
            return FindConceptScheme(conceptSchemeId) != null;
        }

        public Concept FindConcept(string conceptId)
        {
            return ConceptScheme.FindConcept(ConceptSchemes, conceptId);
        }

        public bool ExistConcept(string conceptId)
        {
            return FindConcept(conceptId) != null;
        }

        public ConceptScheme FindConceptSchemeByConceptId(string conceptId)
        {
            return ConceptScheme.FindConceptSchemeByConceptId(ConceptSchemes, conceptId);
        }

        public ConceptScheme FindConceptSchemeByVariableId(string variableId)
        {
            Variable variable = FindVariable(variableId);
            return FindConceptSchemeByConceptId(variable.ConceptId);
        }

        public Question FindQuestion(string questionId)
        {
            return Question.Find(Questions, questionId);
        }

        public QuestionGroup FindQuestionGroup(string questionGroupId)
        {
            return QuestionGroup.Find(QuestionGroups, questionGroupId);
        }

        public CodeScheme FindCodeScheme(string codeSchemeId)
        {
            return CodeScheme.Find(CodeSchemes, codeSchemeId);
        }


        public CategoryScheme FindCategoryScheme(string categorySchemeId)
        {
            return CategoryScheme.Find(CategorySchemes, categorySchemeId);
        }

        public CategoryScheme FindCategorySchemeByCategoryId(string categoryId)
        {
            return CategoryScheme.FindByCategoryId(CategorySchemes, categoryId);
        }

        public List<Code> FindCodes(string codeSchemeId)
        {
            CodeScheme codeScheme = FindCodeScheme(codeSchemeId);
            if (codeScheme == null)
            {
                return new List<Code>();
            }
            return codeScheme.Codes;
        }

        public List<Category> AllCategories
        {
            get
            {
                List<Category> categories = new List<Category>();
                foreach (CategoryScheme categoryScheme in CategorySchemes)
                {
                    categories.AddRange(categoryScheme.Categories);
                }
                return categories;
            }
        }

        public Category FindCategory(string categoryId)
        {
            foreach (CategoryScheme categoryScheme in CategorySchemes)
            {
                Category category = categoryScheme.FindCategory(categoryId);
                if (category != null)
                {
                    return category;
                }
            }
            return null;
        }

        public Member FindMember(string memberId)
        {
            return Member.Find(Members, memberId);
        }

        public Organization FindOrganization(string organizationId)
        {
            return Organization.GetOrganization(Organizations, organizationId);
        }

        public string UniverseSchemeId { get; set; } // for DDI
        public string ConceptualComponentId { get; set; } // for DDI
        public string GeographicStructureSchemeId { get; set; } // for DDI
        public string DataCollectionId { get; set; } // for DDI
        public string QuestionSchemeId { get; set; } // for DDI
        public string LogicalProductId { get; set; } // for DDI
        public string DataRelationshipId { get; set; } // for DDI
        public string PhysicalDataProductId { get; set; } // for DDI
        public string PhysicalStructureSchemeId { get; set; } // for DDI
        public string RecordLayoutSchemeId { get; set; }// for DDI
        public string ArchiveId { get; set; }// for DDI
        public string OrganizationSchemeId { get; set; }// for DDI
        public string MethodologyId { get; set; }//for DDI
        public string ProcessingEventId { get; set; } // for DDI

        //public void DisableDaihyosha()
        //{
        //    //If member is already exists, Role in the importing data is set to Other
        //    //(Pricipal Investigator is limited to one)
        //    foreach (Member member in Members)
        //    {
        //        if (member.RoleCode == Options.ROLE_DAIHYOSHA_CODE)
        //        {
        //            member.RoleCode = Options.ROLE_OTHER_CODE;
        //        }
        //    }
        //}

        public DataFile DefaultDataFile
        {
            get
            {
                return DataFile.Find(DataFiles, DefaultDataFileId);
            }
        }

        public DataSet DefaultDataSet
        {
            get
            {
                return DataSet.Find(DataSets, DefaultDataSetId);
            }
        }

        public bool ContainsInQuestionGroup(Question question)
        {
            return QuestionGroup.Contains(QuestionGroups, question);
        }


        public CodeScheme FindBinaryCodeScheme
        {
            get
            {
                return CodeScheme.Find(CodeSchemes, BinaryCodeSchemeId);
            }
        }

        public void CreateBinaryCodeScheme()
        {
            if (FindBinaryCodeScheme != null)
            {
                return;
            }

            CodeScheme codeScheme = new CodeScheme();
            codeScheme.Title = Resources.SelectUnselect;

            CategoryScheme categoryScheme = new CategoryScheme();
            categoryScheme.Title = codeScheme.Title;

            Category selectedCategory = new Category();
            selectedCategory.Title = Resources.Select;
            selectedCategory.CategorySchemeId = categoryScheme.Id;

            Category unselectedCategory = new Category();
            unselectedCategory.Title = Resources.Unselect;
            unselectedCategory.CategorySchemeId = categoryScheme.Id;

            categoryScheme.Categories.Add(selectedCategory);
            categoryScheme.Categories.Add(unselectedCategory);

            Code selectedCode = new Code();
            selectedCode.Value = EDOConstants.SELECTED_CODE_VALUE;
            selectedCode.CategoryId = selectedCategory.Id;
            selectedCode.CodeSchemeId = codeScheme.Id;
            codeScheme.Codes.Add(selectedCode);

            Code unselectedCode = new Code();
            unselectedCode.Value = EDOConstants.UNSELECTED_CODE_VALUE;
            unselectedCode.CategoryId = unselectedCategory.Id;
            unselectedCode.CodeSchemeId = codeScheme.Id;
            codeScheme.Codes.Add(unselectedCode);


            CategorySchemes.Insert(0, categoryScheme);
            CodeSchemes.Insert(0, codeScheme);

            BinaryCodeSchemeId = codeScheme.Id;
            BinaryCategorySchemeId = categoryScheme.Id;
        }
    }
}
