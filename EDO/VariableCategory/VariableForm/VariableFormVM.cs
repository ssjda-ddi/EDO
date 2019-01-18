using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using EDO.Core.ViewModel;
using System.ComponentModel;
using EDO.Main;
using EDO.Core.Model;
using EDO.QuestionCategory.ConceptForm;
using EDO.SamplingCategory.SamplingForm;
using System.Diagnostics;
using System.Collections.Specialized;
using EDO.DataCategory.DataSetForm;
using System.Windows.Input;
using EDO.Core.Util;
using System.Windows;
using EDO.QuestionCategory.QuestionForm;
using EDO.QuestionCategory.CodeForm;
using EDO.Core.View;

namespace EDO.VariableCategory.VariableForm
{
    public class VariableFormVM :FormVM
    {
        #region Initialize
        public VariableFormVM(StudyUnitVM studyUnit) :base(studyUnit)
        {
            variables = new ObservableCollection<VariableVM>();
            foreach (Variable variableModel in studyUnit.VariableModels)
            {
                //Create VariableVM
                VariableVM variable = new VariableVM(variableModel);
                InitVariable(variable);
                //Create ResponseVM
                variable.Response = CreateResponse(variableModel.Response);
                //Add to array
                variables.Add(variable);
            }
            modelSyncher = new ModelSyncher<VariableVM, Variable>(this, variables, studyUnit.VariableModels);
        }

        protected override void Reload(VMState state)
        {
            if (state != null)
            {
                SelectedVariableItem = EDOUtils.Find(variables, state.State1);
            }
            if (SelectedVariableItem == null || !Variables.Contains(SelectedVariable))
            {
                SelectedVariableItem = EDOUtils.GetFirst<VariableVM>(variables);
            }
        }

        public override VMState SaveState()
        {
            if (SelectedVariable == null)
            {
                //Check needed?
                return null;
            }
            return new VMState(SelectedVariable.Id);
        }

        private ModelSyncher<VariableVM, Variable> modelSyncher;

        private ResponseVM CreateResponse(Response responseModel)
        {
            ResponseVM response = StudyUnit.CreateResponse(responseModel);
            response.IsQuestionDesignMode = false;
            return response;
        }

        public override void InitRow(object newItem)
        {
            if (newItem is VariableVM)
            {
                InitVariable((VariableVM)newItem);
            }
        }


        public void InitVariable(VariableVM variable)
        {
            variable.Parent = this;
            variable.ResponseTypes = Options.ResponseTypes;
        }

        #endregion

        #region Property

        private DataSetFormVM DataSetForm { get { return this.StudyUnit.DataSetForm; } }

        public ObservableCollection<VariableVM> variables;
        public ObservableCollection<VariableVM> Variables { get { return variables; } }

        public ObservableCollection<QuestionVM> Questions
        {
            get
            {
                return StudyUnit.AllQuestions;
            }
        }

        public ObservableCollection<ConceptVM> Concepts 
        {
            get
            {
                ObservableCollection<ConceptVM> concepts = new ObservableCollection<ConceptVM>(StudyUnit.AllConcepts);
                return concepts;
            }
        }

        public ObservableCollection<UniverseVM> Universes
        {
            get
            {
                ObservableCollection<UniverseVM> universes = new ObservableCollection<UniverseVM>(StudyUnit.Universes);
                return universes;
            }
        }

        private VariableFormView Window
        {
            get
            {
                return (VariableFormView)View;
            }
        }

        private Object selectedVariableItem;
        public Object SelectedVariableItem
        {
            get
            {
                return selectedVariableItem;
            }
            set
            {
                if (selectedVariableItem != value)
                {
                    selectedVariableItem = value;
                    Window.UpdateTemplate();
                    NotifyPropertyChanged("SelectedVariableItem");
                }
            }
        }

        public VariableVM SelectedVariable
        {
            get
            {
                return selectedVariableItem as VariableVM;
            }
        }


        #endregion

        #region method

        private void CreateVariable(Action<Variable> action, QuestionVM question, int removeIndex)
        {
            Variable variableModel = new Variable();
            variableModel.Label = question.Title;
            variableModel.ConceptId = question.Question.ConceptId;
            variableModel.QuestionId = question.Id;
            variableModel.UniverseId = StudyUnit.DefaultUniverseGuid;
            variableModel.Response = question.DupResponseModel();
            action(variableModel);

            variableModel.Response.Title = null; //Stay this null because the title in Response Style of variables cannot be set.
            variableModel.GeneratorQuestionId = question.Id;
           
            VariableVM newVariable = new VariableVM(variableModel);
            InitVariable(newVariable);
            newVariable.Response = CreateResponse(variableModel.Response);
            Variables.Insert(removeIndex, newVariable);
        }

        private void CreateVariableFor(QuestionVM question, int questionIndex)
        {
            VariableGenerationInfo oldInfo = question.VariableGenerationInfo;
            VariableGenerationInfo newInfo = question.CreateVariableGenerationInfo();
            if (oldInfo != null && oldInfo.VariableGenerationType == newInfo.VariableGenerationType)
            {
                return;
            }
            question.VariableGenerationInfo = newInfo;

            // remove old variables
            List<VariableVM> generatedVariables = VariableVM.FindByQuestionId(Variables, question.Id);
            Debug.WriteLine(generatedVariables.Count);
            int removeIndex = Variables.Count;
            foreach (VariableVM variable in generatedVariables)
            {
                if (removeIndex == Variables.Count)
                {
                    removeIndex = Variables.IndexOf(variable);
                }
                StudyUnit.OnRemoveVariable(variable);
                Variables.Remove(variable);
            }

            // create single or multiple variables
            if (newInfo.VariableGenerationType == VariableGenerationType.SingleVariable)
            {
                CreateVariable((variableMovel) => {
                    variableMovel.Title = "V" + (questionIndex + 1);
                }, question, removeIndex);
            }
            else
            {
                CodeSchemeVM codeScheme = question.Response.CodeScheme;
                ICollection<CodeVM> codes = codeScheme.Codes;
                string variablePrefix = "V" + (questionIndex + 1) + "_";
                foreach (CodeVM code in codes)
                {
                    CreateVariable((variableModel) => {
                        variableModel.Title = variablePrefix + code.Value;
                        variableModel.Label = code.Label;
                        variableModel.Response = question.DupResponseModel();
                        variableModel.Response.TypeCode = Options.RESPONSE_TYPE_CHOICES_CODE;
                        variableModel.Response.CodeSchemeId = StudyUnit.BinaryCodeSchemeId;
                    }, question, removeIndex++);
                }
            }
        }

        public void CreateVariables(ICollection<QuestionVM> questions)
        {
            //It will be called at the end of Question window. Variables will be automatically generated.
            //(Recreating the deleted variables allowed?)
            int questionIndex = 0;
            foreach (QuestionVM question in questions)
            {
                CreateVariableFor(question, questionIndex);
                questionIndex++;
            }
        }

        public VariableVM FindVariable(string variableId)
        {
            return VariableVM.Find(Variables, variableId);
        }

        public List<VariableVM> FindVariablesByUniverseId(string universeId)
        {
            return VariableVM.FindByUniverseId(Variables, universeId);
        }

        public List<VariableVM> FindVariablesByQuestionId(string questionId)
        {
            return VariableVM.FindByQuestionId(Variables, questionId);
        }

        #endregion


        #region Commands

        private ICommand selectResponseCommand;
        public ICommand SelectResponseCommand
        {
            get
            {
                if (selectResponseCommand == null)
                {
                    selectResponseCommand = new RelayCommand(param => SelectResponse(), param => CanSelectResponse);
                }
                return selectResponseCommand;
            }
        }

        public bool CanSelectResponse
        {
            get
            {
                return SelectedVariable != null;
            }
        }

        public void SelectResponse()
        {
            ResponseVM newResponse = StudyUnit.SelectAndCreateResponse(SelectedVariable.Response);
            if (newResponse != null)
            {
                newResponse.IsQuestionDesignMode = false;
                SelectedVariable.Response = newResponse;
                Window.UpdateTemplate();
            }
        }

        #endregion

        public List<ResponseVM> FindResponses(CodeSchemeVM codeScheme)
        {
            List<ResponseVM> responses = new List<ResponseVM>();
            foreach (VariableVM variable in Variables)
            {
                ResponseVM response = variable.Response;
                if (response.IsTypeChoices && response.CodeScheme == codeScheme)
                {
                    responses.Add(response);
                }
            }
            return responses;
        }

        private ICommand removeVariableCommand;
        public ICommand RemoveVariableCommand
        {
            get
            {
                if (removeVariableCommand == null)
                {
                    removeVariableCommand = new RelayCommand(param => RemoveVariable(), param => CanRemoveVariable);
                }
                return removeVariableCommand;
            }
        }

        public bool CanRemoveVariable
        {
            get
            {
                if (SelectedVariable == null)
                {
                    return false;
                }
                return true;
            }
        }

        public void RemoveVariable()
        {
            StudyUnit.OnRemoveVariable(SelectedVariable);
            Variables.Remove(SelectedVariable);
            SelectedVariableItem = null;
        }

        public void RemoveUniverse(UniverseVM universe)
        {
            foreach (VariableVM variable in variables)
            {
                if (variable.UniverseId == universe.Id)
                {
                    variable.UniverseId = null;
                }
            }
        }

        public void OnRemoveConcepts(List<ConceptVM> concepts)
        {
            foreach (ConceptVM concept in concepts)
            {
                OnRemoveConcept(concept);
            }
        }

        public void OnRemoveConcept(ConceptVM concept)
        {
            foreach (VariableVM variable in variables)
            {
                if (variable.ConceptId == concept.Id)
                {
                    variable.ConceptId = null;
                    variable.Concept = null;
                }
            }
        }

        public void OnRemoveQuestion(QuestionVM question)
        {
            foreach (VariableVM variable in variables)
            {
                if (variable.QuestionId == question.Id)
                {
                    variable.QuestionId = null;
                }
            }
        }

        protected override Action GetCompleteAction(VMState state)
        {
            return () => { StudyUnit.CompleteVariables(); };
        }
    }
}
