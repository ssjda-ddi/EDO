using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.Model;
using EDO.Core.Util;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace EDO.Core.IO
{
    public class RenameResult
    {
        public RenameResult(string propertyName)
        {
            PropertyName = propertyName;
        }

        public string PropertyName { get; set; }
        public string OldId { get; set;  }
        public string NewId { get; set; }
        public bool IsRenamed
        {
            get
            {
                return OldId != null && NewId != null;
            }
        }
        public bool IsIdPropertyRenamed
        {
            get
            {
                return PropertyName == "Id" && IsRenamed;            
            }
        }
    }

    public static class DDIUtils
    {
        //holding in a form such as List <EDOID>, not as List<string>, makes it possible to identify whether the ID of any part. It would be easier to debug.
        private static void CollectIds(IIDPropertiesProvider obj, List<string> ids)
        {
            foreach (string propertyName in obj.IdProperties)
            {
                string id = (string)PropertyPathHelper.GetValue(obj, propertyName);
                ids.Add(id);
            }                
        }

        private static void CollectIds<T>(List<T> objects, List<string> ids) where T:IIDPropertiesProvider
        {
            foreach (T obj in objects)
            {
                CollectIds(obj, ids);
            }
        }

        public static List<string> CollectIds(StudyUnit studyUnit)
        {
            List<string> ids = new List<string>();
            //0. StudyUnit itself
            CollectIds(studyUnit, ids);
            //1. Event
            CollectIds(studyUnit.Events, ids);
            //2. Member
            CollectIds(studyUnit.Members, ids);
            //3. Organization
            CollectIds(studyUnit.Organizations, ids);
            //4. Abstract
            CollectIds(studyUnit.Abstract, ids);
            //5. Coverage
            CollectIds(studyUnit.Coverage, ids);
            //6. Funding Agency
            CollectIds(studyUnit.FundingInfos, ids);
            //6-1. Organization including 
            CollectIds(FundingInfo.GetOrganizations(studyUnit.FundingInfos), ids);
            //7.Universe
            CollectIds(Sampling.GetUniverses(studyUnit.Samplings), ids);
            //8.Sampling
            CollectIds(studyUnit.Samplings, ids);
            //9. Concept Scheme
            CollectIds(studyUnit.ConceptSchemes, ids);
            //9-1. Concept
            CollectIds(ConceptScheme.GetConcepts(studyUnit.ConceptSchemes), ids);
            //10. Question
            CollectIds(studyUnit.Questions, ids);
            //10-1. Answer
            CollectIds(Question.GetResponses(studyUnit.Questions), ids);
            //11. Category Scheme
            CollectIds(studyUnit.CategorySchemes, ids);
            //11-1. Category
            CollectIds(CategoryScheme.GetCategories(studyUnit.CategorySchemes), ids);
            //12. Code Scheme
            CollectIds(studyUnit.CodeSchemes, ids);
            //12-1. Code
            CollectIds(CodeScheme.GetCodes(studyUnit.CodeSchemes), ids);
            //13. Variable Scheme
            CollectIds(studyUnit.VariableScheme, ids);
            //14. Variable
            CollectIds(studyUnit.Variables, ids);
            //14-1. Answer
            CollectIds(Variable.GetResponses(studyUnit.Variables), ids);
            //15. Dataset
            CollectIds(studyUnit.DataSets, ids);
            //16. Data File
            CollectIds(studyUnit.DataFiles, ids);
            //17. Order of Question
            CollectIds(studyUnit.ControlConstructSchemes, ids);
            //17-1.Sequence
            CollectIds(ControlConstructScheme.GetSequences(studyUnit.ControlConstructSchemes), ids);
            //17-2.Constructs
            CollectIds(ControlConstructScheme.GetConstructs(studyUnit.ControlConstructSchemes), ids);

            return ids;
        }

        private static RenameResult RenameId(IIDPropertiesProvider obj, string propertyName, List<string> ids)
        {
            RenameResult result = new RenameResult(propertyName);
            string id = (string)PropertyPathHelper.GetValue(obj, propertyName);
            if (ids.Contains(id))
            {
                string newId =  IDUtils.NewGuid();
                PropertyPathHelper.SetValue(obj, propertyName, newId);
                result.OldId = id;
                result.NewId = newId;
                return result;
            }
            return result;
        }

        private static void RenameIds(IIDPropertiesProvider obj, List<string>ids)
        {
            RenameIds(obj, ids, null);
        }

        private static void RenameIds(IIDPropertiesProvider obj, List<string>ids, Action<RenameResult> action)
        {
            foreach (string propertyName in obj.IdProperties)
            {
                RenameResult result = RenameId(obj, propertyName, ids);
                if (result.IsIdPropertyRenamed && action != null)
                {
                    action(result);
                }
            }            
        }

        private static void RenameIds<T>(List<T> objects, List<string> ids) where T : IIDPropertiesProvider
        {
            RenameIds(objects, ids, null);
        }

        private static void RenameIds<T>(List<T> objects, List<string> ids, Action<RenameResult> action) where T: IIDPropertiesProvider
        {
            foreach (T obj in objects)
            {
                RenameIds(obj, ids, action);
            }
        }


        private static bool IsIdChanged(string propertyName, string old)
        {
            return (propertyName == "Id" && old != null);
        }

        public static void RenameIds(StudyUnit orgStudyUnit, StudyUnit newStudyUnit)
        {
            // renumber the ID of newStudyUnit to avoid duplication between ID of newStudyUnit ID and orgStudyUnit

            List<string> ids = CollectIds(orgStudyUnit);

            //0. StudyUnit itself
            RenameIds(newStudyUnit, ids);
            //1. Event
            RenameIds(newStudyUnit.Events, ids);
            //2. Member
            RenameIds(newStudyUnit.Members, ids, (result) =>
            {
                Sampling.ChangeMemberId(newStudyUnit.Samplings, result.OldId, result.NewId);
            });

            //3. Organization
            RenameIds(newStudyUnit.Organizations, ids, (result) =>
            {
                Member.ChangeOrganizationId(newStudyUnit.Members, result.OldId, result.NewId);
                Sampling.ChangeMemberId(newStudyUnit.Samplings, result.OldId, result.NewId);
            });
            //4. Abstract
            RenameIds(newStudyUnit.Abstract, ids);
            //5. Coverage
            RenameIds(newStudyUnit.Coverage, ids);
            //6. Funding Agency
            RenameIds(newStudyUnit.FundingInfos, ids);
            RenameIds(newStudyUnit.FundingInfoOrganizations, ids); //Organization of Funding Agency
            ////7.Universe
            RenameIds(newStudyUnit.AllUniverses, ids, (result) =>
            {
                Variable.ChangeUniverseId(newStudyUnit.Variables, result.OldId, result.NewId);
            });
            //8.Sampling
            RenameIds(newStudyUnit.Samplings, ids);
            //9. Concept Scheme
            RenameIds(newStudyUnit.ConceptSchemes, ids);
            //9-1. Concept
            RenameIds(ConceptScheme.GetConcepts(newStudyUnit.ConceptSchemes), ids, (result) =>
            {
                Question.ChangeConceptId(newStudyUnit.Questions, result.OldId, result.NewId);
                Variable.ChangeConceptId(newStudyUnit.Variables, result.OldId, result.NewId);
                Book.ChangeMetaDataId(newStudyUnit.Books, result.OldId, result.NewId);
            });
            //10. Question
            RenameIds(newStudyUnit.Questions, ids, (result) =>
            {
                ControlConstructScheme.ChangeQuestionId(newStudyUnit.ControlConstructSchemes, result.OldId, result.NewId);
                Variable.ChangeQuestionId(newStudyUnit.Variables, result.OldId, result.NewId);
                Book.ChangeMetaDataId(newStudyUnit.Books, result.OldId, result.NewId);
            });
            //11. Category Scheme
            RenameIds(newStudyUnit.CategorySchemes, ids, (result) =>
            {
                CategoryScheme.ChangeCategorySchemeId(newStudyUnit.CategorySchemes, result.OldId, result.NewId);
            });
            //11-1. Category
            RenameIds(CategoryScheme.GetCategories(newStudyUnit.CategorySchemes), ids, (result) =>
            {
                CodeScheme.ChangeCategoryId(newStudyUnit.CodeSchemes, result.OldId, result.NewId);
            });
            //12. Code Scheme
            RenameIds(newStudyUnit.CodeSchemes, ids, (result) =>
            {
                CodeScheme.ChangeCodeSchemeId(newStudyUnit.CodeSchemes, result.OldId, result.NewId);                
            });
            //12-1. Code
            RenameIds(CodeScheme.GetCodes(newStudyUnit.CodeSchemes), ids);
            //13. Variable Scheme
            RenameIds(newStudyUnit.VariableScheme, ids);
            //14. Variable
            RenameIds(newStudyUnit.Variables, ids, (result) =>
            {
                DataSet.ChangeVariableId(newStudyUnit.DataSets, result.OldId, result.NewId);
                Book.ChangeMetaDataId(newStudyUnit.Books, result.OldId, result.NewId);
                StatisticsInfo.ChangeVariableId(newStudyUnit.StatisticsInfos, result.OldId, result.NewId);
            });
            //15. Dataset
            RenameIds(newStudyUnit.DataSets, ids, (result) =>
            {
                DataFile.ChangeDataSetId(newStudyUnit.DataFiles, result.OldId, result.NewId);
            });
            //16. Data File
            RenameIds(newStudyUnit.DataFiles, ids);
            //17. Order of Question
            RenameIds(newStudyUnit.ControlConstructSchemes, ids);
            //17-1.Sequence
            RenameIds(ControlConstructScheme.GetSequences(newStudyUnit.ControlConstructSchemes), ids);
            //17-2.Constructs
            RenameIds(ControlConstructScheme.GetConstructs(newStudyUnit.ControlConstructSchemes), ids, (result) =>
            {
                ControlConstructScheme.ChangeControlConstructId(newStudyUnit.ControlConstructSchemes, result.OldId, result.NewId);
            });

            //Related materials
            RenameIds(newStudyUnit.Books, ids);
        }

        public static void CheckDuplicate(StudyUnit orgStudyUnit, StudyUnit newStudyUnit)
        {
            List<string> orgIds = CollectIds(orgStudyUnit);
            List<string> newIds = CollectIds(newStudyUnit);
            foreach (string newId in newIds)
            {
                if (orgIds.Contains(newId))
                {
                    Debug.WriteLine(string.Format("duplicate id {0}", newId));
                }
            }
        }

        public static void FillCollectorFields(StudyUnit newStudyUnit)
        {
            foreach (Sampling samplingModel in newStudyUnit.Samplings)
            {
                Member member = newStudyUnit.FindMember(samplingModel.MemberId);
                if (member != null)
                {
                    samplingModel.LastName = member.LastName;
                    samplingModel.FirstName = member.FirstName;
                    samplingModel.Position = member.Position;
                    Organization membersOrganization = newStudyUnit.FindOrganization(member.OrganizationId);
                    if (membersOrganization != null)
                    {
                        samplingModel.OrganizationName = membersOrganization.OrganizationName;
                    }
                    samplingModel.CollectorTypeCode = Options.COLLECTOR_TYPE_INDIVIDUAL;
                }
                else
                {
                    Organization organization = newStudyUnit.FindOrganization(samplingModel.MemberId);
                    if (organization != null)
                    {
                        samplingModel.OrganizationName = organization.OrganizationName;
                    }
                    samplingModel.CollectorTypeCode = Options.COLLECTOR_TYPE_ORGANIZATION;
                }

            }
        }

        public static Category FindCategoryByCodeValue(List<Category> categories, List<Code> codes, string codeValue)
        {
            Code code = Code.FindByCodeValue(codes, codeValue);
            if (code == null)
            {
                return null;
            }
            Category category = Category.Find(categories, code.CategoryId);
            return category;
        }
    }
}
