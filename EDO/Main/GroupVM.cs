using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.ComponentModel;
using EDO.GroupCategory.GroupForm;
using EDO.GroupCategory.CompareForm;
using EDO.Core.ViewModel;
using EDO.Core.View;
using EDO.Core.Model;
using EDO.Core.Util;

namespace EDO.Main
{
    public class GroupVM :EDOUnitVM
    {
        public class SharedItem
        {
            public ConceptScheme ConceptScheme { get; set; }
            public Concept Concept { get; set; }
            public Variable Variable { get; set; }
        }

        private GroupFormVM groupForm;

        private CompareFormVM compareConceptSchemeForm;

        private CompareFormVM compareConceptForm;

        private CompareFormVM compareVariableForm;

        public GroupVM(MainWindowVM mainWindowVM, Group group) :base(mainWindowVM, group)
        {
            this.groupModel = group;
            groupForm = new GroupFormVM(this);
            MenuItemVM categoryGroup = new MenuItemVM(MenuElem.C_GROUP, groupForm);
            MenuItemVM menuDetail = new MenuItemVM(MenuElem.M_DETAIL, groupForm);
            categoryGroup.Add(menuDetail);
            this.MenuItems.Add(categoryGroup);

            compareConceptSchemeForm = new CompareFormVM(this, groupModel.ConceptSchemeCompareTable);
            MenuItemVM menuCompareDai = new MenuItemVM(MenuElem.M_COMPARE_DAI, compareConceptSchemeForm);
            categoryGroup.Add(menuCompareDai);

            compareConceptForm = new CompareFormVM(this, groupModel.ConceptCompareTable);
            MenuItemVM menuCompareSho = new MenuItemVM(MenuElem.M_COMPARE_SHO, compareConceptForm);
            categoryGroup.Add(menuCompareSho);

            compareVariableForm = new CompareFormVM(this, groupModel.VariableCompareTable);
            MenuItemVM menuCompareVariable = new MenuItemVM(MenuElem.M_COMPARE_VARIABLE, compareVariableForm);
            categoryGroup.Add(menuCompareVariable);
        }

        #region Delegate Method for Model

        private Group groupModel;
        public Group GroupModel
        {
            get
            {
                return groupModel;
            }
        }

        public CompareTable ConceptSchemeCompareTableModel {get {return groupModel.ConceptSchemeCompareTable; }}
        public CompareTable ConceptCompareTableModel { get { return groupModel.ConceptCompareTable; } }
        public CompareTable VariableCompareTableModel { get { return groupModel.VariableCompareTable; } }

        #endregion

        public override string Title
        {
            get
            {
                return groupModel.Title;
            }
            set
            {
                if (groupModel.Title != value)
                {
                    groupModel.Title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        public List<StudyUnitVM> StudyUnits
        {
            get => Main.StudyUnits;
        }

        private List<ConceptScheme> FindConceptSchemeModels(List<GroupId> conceptSchemeIds)
        {
            //Get those matched by the concept scheme level
            List<ConceptScheme> conceptSchemes = new List<ConceptScheme>();
            foreach (GroupId conceptSchemeId in conceptSchemeIds)
            {
                StudyUnitVM studyUnit = StudyUnitVM.Find(Main.StudyUnits, conceptSchemeId.StudyUnitId);
                if (studyUnit == null)
                {
                    continue;
                }
                StudyUnit studyUnitModel = studyUnit.StudyUnitModel;

                ConceptScheme conceptScheme = studyUnitModel.FindConceptScheme(conceptSchemeId.Id);
                if (conceptScheme == null)
                {
                    continue;
                }
                conceptSchemes.Add(conceptScheme);
            }
            return conceptSchemes;
        }

        private List<Concept> FindConceptModels(List<GroupId> conceptIds)
        {
            //Get those matched by the concept scheme level
            List<Concept> concepts = new List<Concept>();
            foreach (GroupId conceptId in conceptIds)
            {
                StudyUnitVM studyUnit = StudyUnitVM.Find(Main.StudyUnits, conceptId.StudyUnitId);
                if (studyUnit == null)
                {
                    continue;
                }
                StudyUnit studyUnitModel = studyUnit.StudyUnitModel;
                Concept concept = studyUnitModel.FindConcept(conceptId.Id);
                if (concept == null)
                {
                    continue;
                }
                concepts.Add(concept);
            }
            return concepts;
        }

        private List<Variable> FindVariableModels(List<GroupId> variableIds)
        {
            //Get those matched by the concept scheme level
            List<Variable> variables = new List<Variable>();
            foreach (GroupId variableId in variableIds)
            {
                StudyUnitVM studyUnit = StudyUnitVM.Find(Main.StudyUnits, variableId.StudyUnitId);
                if (studyUnit == null)
                {
                    continue;
                }
                StudyUnit studyUnitModel = studyUnit.StudyUnitModel;
                Variable variable = studyUnitModel.FindVariable(variableId.Id);
                if (variable == null)
                {
                    continue;
                }
                variables.Add(variable);
            }
            return variables;
        }

        private StudyUnit FindStudyUnitModel(Variable variable)
        {
            foreach (StudyUnitVM studyUnit in Main.StudyUnits)
            {
                if (studyUnit.StudyUnitModel.Variables.Contains(variable))
                {
                    return studyUnit.StudyUnitModel;
                }
            }
            return null;
        }

        private CodeScheme FindCodeScheme(string codeSchemeId)
        {
            foreach (StudyUnitVM studyUnit in Main.StudyUnits)
            {
                CodeScheme codeScheme = studyUnit.StudyUnitModel.FindCodeScheme(codeSchemeId);
                if (codeScheme != null)
                {
                    return codeScheme;
                }
            }
            return null;            
        }

        private ConceptScheme CloneConceptScheme(ConceptScheme orgConceptScheme)
        {
            ConceptScheme newConceptScheme = (ConceptScheme)orgConceptScheme.Clone();
            newConceptScheme.Id = IDUtils.NewGuid();
            newConceptScheme.Concepts = new List<Concept>();
            return newConceptScheme;
        }

        private Concept CloneConcept(Concept orgConcept)
        {
            Concept newConcept = (Concept)orgConcept.Clone();
            newConcept.Id = IDUtils.NewGuid();
            return newConcept;
        }

        private Variable CloneVariable(Variable orgVariable)
        {
            Variable newVariable = (Variable)orgVariable.Clone();
            newVariable.Id = IDUtils.NewGuid();

            Response newResponse = (Response)orgVariable.Response.Clone();
            newResponse.Id = IDUtils.NewGuid();

            newVariable.Response = newResponse;

            return newVariable;
        }

        private Universe CloneUniverse(Universe orgUniverse)
        {
            Universe newUniverse = (Universe)orgUniverse.Clone();
            newUniverse.Id = IDUtils.NewGuid();
            return newUniverse;
        }

        private CodeScheme CloneCodeScheme(CodeScheme codeScheme)
        {
            CodeScheme newCodeScheme = (CodeScheme)codeScheme.Clone();
            newCodeScheme.Id = IDUtils.NewGuid();
            newCodeScheme.Codes = new List<Code>();
            foreach (Code code in codeScheme.Codes)
            {
                Code newCode = (Code)code.Clone();
                newCode.CodeSchemeId = newCodeScheme.Id;
                newCodeScheme.Codes.Add(newCode);
            }
            return newCodeScheme;
        }

        private CategoryScheme CloneCategoryScheme(CategoryScheme categoryScheme)
        {
            CategoryScheme newCategoryScheme = (CategoryScheme)categoryScheme.Clone();
            newCategoryScheme.Id = IDUtils.NewGuid();
            newCategoryScheme.Categories = new List<Category>();
            return newCategoryScheme;
        }

        public void SyncModel()
        {
            compareConceptSchemeForm.SyncModel();
            compareConceptForm.SyncModel();
            compareVariableForm.SyncModel();
            UpdateSharedStudyUnit();
        }

        public void UpdateSharedStudyUnit()
        {
            //Save the variables shared by Group
            StudyUnit sharedStudyUnit = groupModel.GetClearedSharedStudyUnit();

            //Remember the association of new models and the old ID that has been treated
            Dictionary<string, ConceptScheme> conceptSchemeMap = new Dictionary<string, ConceptScheme>();
            Dictionary<string, Concept> conceptMap = new Dictionary<string, Concept>();
            Dictionary<string, Variable> variableMap = new Dictionary<string, Variable>();
            Dictionary<string, Universe> universeMap = new Dictionary<string, Universe>();
            Dictionary<string, CodeScheme> codeSchemeMap = new Dictionary<string, CodeScheme>();
            Dictionary<string, Category> categoryMap = new Dictionary<string, Category>();
            Dictionary<string, CategoryScheme> categorySchemeMap = new Dictionary<string, CategoryScheme>();


            //Get a model that exactly match
            List<ConceptScheme> conceptSchemes = FindConceptSchemeModels(groupModel.ConceptSchemeCompareTable.GetMatchIds());
            List<Concept> concepts = FindConceptModels(groupModel.ConceptCompareTable.GetMatchIds());
            List<Variable> variables = FindVariableModels(groupModel.VariableCompareTable.GetMatchIds());

            //Save which is fully matched with variable level(Concept Scheme and Concept Level are needed?)
            foreach (Variable variable in variables)
            {                
                //ConceptScheme, Concept and Variable will be saved when they are fully matched with all levels
                Concept concept = Concept.Find(concepts, variable.ConceptId);
                if (concept == null)
                {
                    continue;
                }
                ConceptScheme conceptScheme = ConceptScheme.FindConceptSchemeByConceptId(conceptSchemes, concept.Id);
                if (conceptScheme == null)
                {
                    continue;
                }
                //Get StudyUnit
                StudyUnit curStudyUnit = FindStudyUnitModel(variable);
                if (curStudyUnit == null)
                {
                    continue;
                }
                //Universe
                Universe universe = curStudyUnit.FindUniverse(variable.UniverseId);
                if (universe == null)
                {
                    continue;
                }
                //Add ConceptScheme
                ConceptScheme newConceptScheme = null;
                if (!conceptSchemeMap.ContainsKey(conceptScheme.Id))
                {
                    newConceptScheme = CloneConceptScheme(conceptScheme);
                    conceptSchemeMap[conceptScheme.Id] = newConceptScheme;
                    sharedStudyUnit.ConceptSchemes.Add(newConceptScheme);
                }
                //Add Concept
                Concept newConcept = null;
                if (!conceptMap.ContainsKey(concept.Id))
                {
                    newConcept = CloneConcept(concept);
                    conceptMap[concept.Id] = newConcept;
                    newConceptScheme.Concepts.Add(newConcept);
                }
                //Add Universe
                Universe newUniverse = null;
                if (!universeMap.ContainsKey(variable.UniverseId))
                {
                    newUniverse = CloneUniverse(universe);
                    universeMap[universe.Id] = newUniverse;
                    sharedStudyUnit.Samplings[0].Universes.Add(newUniverse);
                }

                //Add Variable
                Variable newVariable = null;
                if (!variableMap.ContainsKey(variable.Id))
                {
                    newVariable = CloneVariable(variable);
                    newVariable.ConceptId = newConcept.Id;
                    newVariable.QuestionId = null;
                    newVariable.UniverseId = newUniverse.Id;
                    variableMap[variable.Id] = newVariable;
                    sharedStudyUnit.Variables.Add(newVariable);
                }

                if (newVariable.Response.IsTypeChoices)
                {
                    //Process in the case of Category
                    CodeScheme codeScheme = curStudyUnit.FindCodeScheme(variable.Response.CodeSchemeId);
                    if (codeScheme == null)
                    {
                        continue;
                    }
                    CodeScheme newCodeScheme = null;
                    if (!codeSchemeMap.ContainsKey(codeScheme.Id))
                    {
                        //Save CodeScheme
                        newCodeScheme = CloneCodeScheme(codeScheme);
                        codeSchemeMap[codeScheme.Id] = newCodeScheme;
                        newVariable.Response.CodeSchemeId = newCodeScheme.Id;
                        sharedStudyUnit.CodeSchemes.Add(newCodeScheme);

                        foreach (Code newCode in newCodeScheme.Codes)
                        {
                            Category category = curStudyUnit.FindCategory(newCode.CategoryId);
                            if (category == null)
                            {
                                continue;
                            }
                            CategoryScheme categoryScheme = curStudyUnit.FindCategoryScheme(category.CategorySchemeId);
                            if (categoryScheme == null)
                            {
                                continue;
                            }
                            CategoryScheme newCategoryScheme = null;
                            if (!categorySchemeMap.ContainsKey(categoryScheme.Id))
                            {
                                newCategoryScheme = CloneCategoryScheme(categoryScheme);
                                categorySchemeMap[categoryScheme.Id] = newCategoryScheme;
                                sharedStudyUnit.CategorySchemes.Add(newCategoryScheme);
                            }
                            Category newCategory = null;
                            if (!categoryMap.ContainsKey(category.Id))
                            {
                                newCategory = (Category)category.Clone();
                                newCategory.Id = IDUtils.NewGuid();
                                newCategory.CategorySchemeId = newCategoryScheme.Id;
                                categoryMap[category.Id] = newCategory;
                                newCategoryScheme.Categories.Add(newCategory);
                                newCode.CategoryId = newCategory.Id;
                            }
                        }
                    }
                }
            }
        }
    }

}
