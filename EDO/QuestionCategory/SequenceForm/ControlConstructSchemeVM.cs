using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.ViewModel;
using EDO.Core.Model;
using EDO.Core.Util;
using System.Collections.ObjectModel;
using EDO.QuestionCategory.QuestionForm;
using System.Windows.Input;
using System.Windows;
using EDO.Core.View;
using EDO.DataCategory.DataSetForm;
using System.Diagnostics;
using EDO.Properties;
using EDO.QuestionCategory.SequenceForm.Chart;
using EDO.QuestionCategory.QuestionGroupForm;

namespace EDO.QuestionCategory.SequenceForm
{
    public class ControlConstructSchemeVM :BaseVM, IOrderedObject, IStringIDProvider
    {
        public ControlConstructSchemeVM()
            : this(new ControlConstructScheme())
        {
        }

        public ControlConstructSchemeVM(ControlConstructScheme controlConstructScheme)
        {
            this.controlConstructScheme = controlConstructScheme;
            constructModels = new List<IConstruct>();
            constructs = new ObservableCollection<ConstructVM>();
            //InModel, it holds as an each array of Question, Description and Branch.
            //In VM it holds all as a collection of abstract classes
            //ModelSyncher cannot be used here, so call UpdateModel in every operation
        }

        public void Init()
        {
            List<string> ids = controlConstructScheme.Sequence.ControlConstructIds;
            foreach (string id in ids)
            {
                QuestionConstruct questionConstructModel = controlConstructScheme.FindQuestionConstruct(id);
                if (questionConstructModel != null)
                {
                    QuestionVM question = StudyUnit.FindQuestion(questionConstructModel.QuestionId);
                      Debug.Assert(question != null, "Question not found id=" + questionConstructModel.QuestionId);
                    QuestionConstructVM questionConstruct = new QuestionConstructVM(questionConstructModel, question);
                    InitConstruct(questionConstruct);
                    constructModels.Add(questionConstructModel);
                    constructs.Add(questionConstruct);
                    continue;
                }
                QuestionGroupConstruct questionGroupConstructModel = controlConstructScheme.FindQuestionGroupConstruct(id);
                if (questionGroupConstructModel != null) 
                {
                    QuestionGroupVM questionGroup = StudyUnit.FindQuestionGroup(questionGroupConstructModel.QuestionGroupId);
                    QuestionGroupConstructVM questionGroupConstruct = new QuestionGroupConstructVM(questionGroupConstructModel, questionGroup);
                    InitConstruct(questionGroupConstruct);
                    constructModels.Add(questionGroupConstructModel);
                    constructs.Add(questionGroupConstruct);
                    continue;
                }
                Statement statementModel = controlConstructScheme.FindStatement(id);
                if (statementModel != null)
                {
                    StatementVM statement = new StatementVM(statementModel);
                    InitConstruct(statement);
                    constructModels.Add(statementModel);
                    constructs.Add(statement);
                    continue;
                }
                IfThenElse ifThenElseModel = controlConstructScheme.FindIfThenElse(id);
                if (ifThenElseModel != null)
                {
                    IfThenElseVM ifThenElse = new IfThenElseVM(ifThenElseModel);
                    InitConstruct(ifThenElse);
                    constructModels.Add(ifThenElseModel);
                    constructs.Add(ifThenElse);
                }
            }

            List<QuestionConstructVM> questionConstructs = QuestionConstructs;
            foreach (ConstructVM construct in constructs)
            {
                if (construct is IfThenElseVM)
                {
                    IfThenElseVM ifThenElse = (IfThenElseVM)construct;
                    ifThenElse.ThenConstructs = ThenConstructs;
                }
            }
            modelSyncher = new ModelSyncher<ConstructVM, IConstruct>(this, constructs, constructModels);
            InitTitle();
        }

        #region Model related

        private ControlConstructScheme controlConstructScheme;
        public ControlConstructScheme ControlConstructScheme { get { return controlConstructScheme; } }
        public override object Model
        {
            get
            {
                return controlConstructScheme;
            }
        }
        public string Id
        {
            get { return controlConstructScheme.Id; }
        }

        public void InitTitle()
        {
            if (string.IsNullOrEmpty(controlConstructScheme.Title))
            {
                controlConstructScheme.Title = EDOUtils.OrderTitle(this);
            }
        }

        public string Title
        {
            get
            {
                return controlConstructScheme.Title;
            }
            set
            {
                if (controlConstructScheme.Title != value)
                {
                    controlConstructScheme.Title = value;
                    InitTitle();
                    NotifyPropertyChanged("Title");
                    Memorize();
                }
            }
        }

        public bool RenumberQuestionNo
        {
            get
            {
                return controlConstructScheme.RenumberQuestionNo;
            }
            set
            {
                if (controlConstructScheme.RenumberQuestionNo != value)
                {
                    controlConstructScheme.RenumberQuestionNo = value;
                    NotifyPropertyChanged("RenumberQuestionNo");
                    Memorize();
                }
            }
        }

        private void UpdateModel(bool memorize)
        {
            controlConstructScheme.QuestionConstructs.Clear();
            controlConstructScheme.QuestionGroupConstructs.Clear();
            controlConstructScheme.Statements.Clear();
            controlConstructScheme.IfThenElses.Clear();
            controlConstructScheme.Sequence.ControlConstructIds.Clear();
            foreach (IConstruct construct in constructModels)
            {
                if (construct is QuestionConstruct)
                {
                    controlConstructScheme.QuestionConstructs.Add((QuestionConstruct)construct);
                }
                else if (construct is QuestionGroupConstruct)
                {
                    controlConstructScheme.QuestionGroupConstructs.Add((QuestionGroupConstruct)construct);
                }
                else if (construct is Statement)
                {
                    controlConstructScheme.Statements.Add((Statement)construct);
                }
                else if (construct is IfThenElse)
                {
                    controlConstructScheme.IfThenElses.Add((IfThenElse)construct);
                }
                controlConstructScheme.Sequence.ControlConstructIds.Add(construct.Id);
            }
            if (memorize)
            {
                Memorize();
            }
        }

        #endregion

        #region Child VM related

        private List<IConstruct> constructModels;
        private ObservableCollection<ConstructVM> constructs;
        public ObservableCollection<ConstructVM> Constructs { get { return constructs; } }
        private ModelSyncher<ConstructVM, IConstruct> modelSyncher;
        private void InitConstruct(ConstructVM construct)
        {
            construct.Parent = this;
        }

        public List<QuestionConstructVM> QuestionConstructs
        {
            get
            {
                List<QuestionConstructVM> questionConstructs = new List<QuestionConstructVM>();
                foreach (ConstructVM construct in constructs)
                {
                    if (construct is QuestionConstructVM)
                    {
                        questionConstructs.Add((QuestionConstructVM)construct);
                    }
                }
                return questionConstructs;
            }
        }

        public List<ConstructVM> ThenConstructs
        {
            get
            {
                List<ConstructVM> thenConstructs = new List<ConstructVM>();
                foreach (ConstructVM construct in constructs)
                {
                    if (construct is QuestionConstructVM || construct is  QuestionGroupConstructVM || construct is StatementVM)
                    {
                        thenConstructs.Add(construct);
                    }
                }
                return thenConstructs;
            }
        }

        public List<IfThenElseVM> IfThenElses
        {
            get
            {
                List<IfThenElseVM> ifThenElses = new List<IfThenElseVM>();
                foreach (ConstructVM construct in constructs)
                {
                    if (construct is IfThenElseVM)
                    {
                        ifThenElses.Add((IfThenElseVM)construct);
                    }
                }
                return ifThenElses;
            }
        }

        public List<StatementVM> Statements
        {
            get
            {
                List<StatementVM> statements = new List<StatementVM>();
                foreach  (ConstructVM construct in constructs)
                {
                    if (construct is StatementVM)
                    {
                        statements.Add((StatementVM)construct);
                    }
                }
                return statements;
            }
        }

        private void InsertConstruct(ConstructVM construct, bool manualOperation)
        {
            InitConstruct(construct);
            if (manualOperation)
            {
                //add below the selected row if added by screen
                int index = SelectedConstructIndex + 1;
                constructs.Insert(index, construct);
            }
            else
            {
                //Added to the end when new question is added automatically
                constructs.Add(construct);
            }
            //memory to be able to Undo if added by screen
            UpdateModel(manualOperation);
        }

        public void InsertQuestionConstruct(QuestionVM question, bool manualOperation)
        {
            ConstructVM construct = ConstructVM.FindByQuestionId(constructs, question.Id);
            if (construct != null)
            {
                if (manualOperation)
                {
                    //show error message if added by screen
                    MessageBox.Show(Resources.AlreadySelectedQuestion); //This question is already selected
                }
                return;
            }
            QuestionConstruct questionConstructModel = new QuestionConstruct();
            questionConstructModel.QuestionId = question.Id;
            questionConstructModel.No = ControlConstructScheme.QUESTION_NO_PREFIX + (ConstructUtils.QuestionConstructCount(constructs) + 1);
            QuestionConstructVM questionConstruct = new QuestionConstructVM(questionConstructModel, question);
            InsertConstruct(questionConstruct, manualOperation);
        }


        public void InsertQuestionGroupConstruct(QuestionGroupVM questionGroup, bool manualOperation)
        {
            ConstructVM construct = ConstructVM.FindByQuestionGroupId(constructs, questionGroup.Id);
            if (construct != null)
            {
                if (manualOperation)
                {
                    MessageBox.Show(Resources.AlreadySelectedQuestionGroup); //This question group is already selected
                }
                return;
            }
            QuestionGroupConstruct questionGroupConstructModel = new QuestionGroupConstruct();
            questionGroupConstructModel.QuestionGroupId = questionGroup.Id;
            questionGroupConstructModel.No = ControlConstructScheme.QUESTION_GROUP_NO_PREFIX + (ConstructUtils.QuestionGroupConstructCount(constructs) + 1);
            QuestionGroupConstructVM questionGroupConstruct = new QuestionGroupConstructVM(questionGroupConstructModel, questionGroup);
            InsertConstruct(questionGroupConstruct, manualOperation);
        }

        private void InsertStatementConstruct(Statement statementModel)
        {
            statementModel.No = ControlConstructScheme.STATEMENT_NO_PREFIX + (ConstructUtils.StatementCount(constructs) + 1);
            StatementVM statement = new StatementVM(statementModel);
            InsertConstruct(statement, true);
        }

        public void InsertIfThenElseConstruct(IfThenElse ifThenElseModel)
        {
            if (ifThenElseModel == null)
            {
                return;
            }
            ifThenElseModel.No = ControlConstructScheme.IFTHENELSE_NO;
            IfThenElseVM ifThenElse = new IfThenElseVM(ifThenElseModel);
            ifThenElse.ThenConstructs = ThenConstructs;
            InsertConstruct(ifThenElse, true);
        }

        #endregion

        #region IOrderedObject Member

        public int OrderNo { get; set; }
        public string OrderPrefix { get; set; }

        #endregion

        #region Commands Related
        private object selectedConstructItem;
        public object SelectedConstructItem
        {
            get
            {
                return selectedConstructItem;
            }
            set
            {
                if (selectedConstructItem != value)
                {
                    selectedConstructItem = value;
                    NotifyPropertyChanged("SelectedConstructItem");
                }
            }
        }

        public ConstructVM SelectedConstruct
        {
            get
            {
                return SelectedConstructItem as ConstructVM;
            }
        }

        private ICommand addQuestionCommand;
        public ICommand AddQuestionCommand
        {
            get
            {
                if (addQuestionCommand == null)
                {
                    addQuestionCommand = new RelayCommand(param => AddQuestion(), param => CanAddQuestion);
                }
                return addQuestionCommand;
            }
        }

        private bool CanAddQuestion
        {
            get
            {
                return true;
            }
        }


        private void AddQuestion()
        {
            ObservableCollection<QuestionVM> questions = StudyUnit.AllQuestions;
            SelectObjectWindowVM<QuestionVM> vm = new SelectObjectWindowVM<QuestionVM>(questions, "Content");
            QuestionVM question = SelectObjectWindow.Select(Resources.SelectQuestion, vm) as QuestionVM;//Select Question
            if (question != null)
            {
                InsertQuestionConstruct(question, true);
            }
        }

        private ICommand addQuestionGroupCommand;
        public ICommand AddQuestionGroupCommand
        {
            get
            {
                if (addQuestionGroupCommand == null)
                {
                    addQuestionGroupCommand = new RelayCommand(param => AddQuestionGroup(), param => CanAddQuestionGroup);
                }
                return addQuestionGroupCommand;
            }
        }

        private bool CanAddQuestionGroup
        {
            get
            {
                return true;
            }
        }

        private void AddQuestionGroup()
        {
            ObservableCollection<QuestionGroupVM> questionGroups = StudyUnit.QuestionGroups;
            SelectObjectWindowVM<QuestionGroupVM> vm = new SelectObjectWindowVM<QuestionGroupVM>(questionGroups);
            QuestionGroupVM questionGroup = SelectObjectWindow.Select(Resources.SelectQuestionGroup, vm) as QuestionGroupVM;//Select Question Group
            if (questionGroup != null)
            {
                InsertQuestionGroupConstruct(questionGroup, true);
            }
        }

        private ICommand addSentenceCommand;
        public ICommand AddSentenceCommand
        {
            get
            {
                if (addSentenceCommand == null)
                {
                    addSentenceCommand = new RelayCommand(param => AddSentence(), param => CanAddSentence);
                }
                return addSentenceCommand;
            }
        }

        public bool CanAddSentence
        {
            get
            {
                return true;
            }
        }

        public void AddSentence()
        {
            CreateSentenceWindowVM vm = new CreateSentenceWindowVM(this, new Statement());
            CreateSentenceWindow window = new CreateSentenceWindow(vm);
            window.Owner = Application.Current.MainWindow;
            if (window.ShowDialog() == true)
            {
                InsertStatementConstruct(vm.Statement);
            }
        }

        private ICommand addBranchCommand;
        public ICommand AddBranchCommand
        {
            get
            {
                if (addBranchCommand == null)
                {
                    addBranchCommand = new RelayCommand(param => AddBranch(), param => CanAddBranch);
                }
                return addBranchCommand;
            }
        }

        public bool CanAddBranch
        {
            get
            {
                return (SelectedConstruct is QuestionConstructVM);
            }
        }

        public void AddBranch()
        {
            CreateBranchWindowVM vm = new CreateBranchWindowVM(this);
            CreateBranchWindow window = new CreateBranchWindow(vm);
            window.Owner = Application.Current.MainWindow;
            if (window.ShowDialog() == true)
            {
                InsertIfThenElseConstruct(vm.IfThenElse);
            }
        }

        private ICommand editConstructCommand;
        public ICommand EditConstructCommand
        {
            get
            {
                if (editConstructCommand == null)
                {
                    editConstructCommand = new RelayCommand(param => EditConstruct(), param => CanEditConstruct);
                }
                return editConstructCommand;
            }
        }

        public bool CanEditConstruct
        {
            get
            {
                return (SelectedConstruct is StatementVM || SelectedConstruct is IfThenElseVM || SelectedConstruct is QuestionConstructVM);
            }
        }

        public void EditConstruct()
        {
            ConstructVM construct = SelectedConstruct;
            if (construct is StatementVM)
            {
                StatementVM statement = (StatementVM)construct;
                CreateSentenceWindowVM vm = new CreateSentenceWindowVM(this,  (Statement)statement.Model);
                CreateSentenceWindow window = new CreateSentenceWindow(vm);
                window.Owner = Application.Current.MainWindow;
                if (window.ShowDialog() == true && vm.Statement != null)
                {
                    StatementVM newStatement = new StatementVM(vm.Statement);
                    InitConstruct(newStatement);
                    int index = constructs.IndexOf(construct);
                    constructs.RemoveAt(index);
                    constructs.Insert(index, newStatement);
                    UpdateModel(true);
                    SelectedConstructItem = newStatement;
                }
            }
            else if (construct is IfThenElseVM)
            {
                EditBranchExternal((IfThenElseVM)construct, Application.Current.MainWindow);
            }
            else if (construct is QuestionConstructVM)
            {
                ChangeSingleQuestionNumberWindowVM vm = new ChangeSingleQuestionNumberWindowVM((QuestionConstructVM)construct);
                ChangeSingleQuestionNumberWindow window = new ChangeSingleQuestionNumberWindow(vm);
                window.Owner = Application.Current.MainWindow;
                if (window.ShowDialog() == true)
                {
                    using (UndoTransaction tx = new UndoTransaction(UndoManager))
                    {
                        if (SequenceUtils.RenumberQuestionNumber(this, vm.QuestionNumber))
                        {
                            UpdateModel(false);
                            tx.Commit();
                        }
                    }
                }
            }
        }

        public void ReplaceIfThenElse(IfThenElseVM ifThenElse, IfThenElseVM newIfThenElse)
        {
            InitConstruct(newIfThenElse);
            newIfThenElse.ThenConstructs = ThenConstructs;
            int index = constructs.IndexOf(ifThenElse);
            constructs.RemoveAt(index);
            constructs.Insert(index, newIfThenElse);
        }

        public bool EditBranchExternal(IfThenElseVM ifThenElse, Window ownerWindow)
        {
            CreateBranchWindowVM vm = new CreateBranchWindowVM(this, (IfThenElse)ifThenElse.Model);
            CreateBranchWindow window = new CreateBranchWindow(vm);
            window.Owner = Application.Current.MainWindow;
            if (window.ShowDialog() == true && vm.IfThenElse != null)
            {
                IfThenElseVM newIfThenElse = new IfThenElseVM(vm.IfThenElse);
                ReplaceIfThenElse(ifThenElse, newIfThenElse);
                UpdateModel(true);
                SelectedConstructItem = newIfThenElse;
                return true;
            }

            return false;
        }

        private ICommand removeConstructCommand;
        public ICommand RemoveConstructCommand
        {
            get
            {
                if (removeConstructCommand == null)
                {
                    removeConstructCommand = new RelayCommand(param => RemoveConstruct(), param => CanRemoveConstruct);
                }
                return removeConstructCommand;
            }
        }

        public bool CanRemoveConstruct
        {
            get
            {
                if (SelectedConstruct == null)
                {
                    return false;
                }
                return true;
            }
        }

        public void RemoveConstruct()
        {
            Constructs.Remove(SelectedConstruct);
            UpdateModel(true);
        }

        private int SelectedConstructIndex
        {
            get
            {
                if (SelectedConstruct == null)
                {
                    return -1;
                }
                return Constructs.IndexOf(SelectedConstruct);
            }
        }

        private bool CanReorderVariable()
        {
            //when you query the view, it is determined the order is not possible to change if it is sorted
            SequenceFormVM parent = (SequenceFormVM)Parent;
            return parent.CanReorderVariable();
        }

        private void FocusCell()
        {
            SequenceFormVM parent = (SequenceFormVM)Parent;
            parent.FocusCell();
        }

        private ICommand upConstructCommand;
        public ICommand UpConstructCommand
        {
            get
            {
                if (upConstructCommand == null)
                {
                    upConstructCommand = new RelayCommand(param => UpConstruct(), param => CanUpConstruct);
                }
                return upConstructCommand;
            }
        }

        public bool CanUpConstruct
        {
            get
            {
                if (SelectedConstruct == null)
                {
                    return false;
                }
                if (!CanReorderVariable())
                {
                    return false;
                }
                if (SelectedConstructIndex == 0)
                {
                    return false;
                }
                return true;
            }
        }

        private void MoveConstruct(int fromIndex, int toIndex)
        {
            using (UndoTransaction tx = new UndoTransaction(UndoManager))
            {
                ConstructVM fromConstruct = Constructs[fromIndex];
                ConstructVM toConstruct = Constructs[toIndex];
                if (RenumberQuestionNo && fromConstruct is QuestionConstructVM && toConstruct is QuestionConstructVM)
                {
                    QuestionNumberVM fromQuestionNumber = new QuestionNumberVM((QuestionConstructVM)fromConstruct);
                    fromQuestionNumber.AfterValue = toConstruct.No;
                    QuestionNumberVM toQuestionNumber = new QuestionNumberVM((QuestionConstructVM)toConstruct);
                    toQuestionNumber.AfterValue = fromConstruct.No;
                    List<QuestionNumberVM> questionNumbers = new List<QuestionNumberVM>();
                    questionNumbers.Add(fromQuestionNumber);
                    questionNumbers.Add(toQuestionNumber);
                    SequenceUtils.RenumberQuestionNumbers(this, questionNumbers);
                }
                Constructs.Move(fromIndex, toIndex);                
                UpdateModel(false);
                tx.Commit();
            }
            FocusCell();
        }

        public void UpConstruct()
        {
            ConstructVM construct = SelectedConstruct;
            int fromIndex = SelectedConstructIndex;
            int toIndex = fromIndex - 1;
            MoveConstruct(fromIndex, toIndex);
        }

        private ICommand downConstructCommand;
        public ICommand DownConstructCommand
        {
            get
            {
                if (downConstructCommand == null)
                {
                    downConstructCommand = new RelayCommand(param => DownConstruct(), param => CanDownConstruct);
                }
                return downConstructCommand;
            }
        }

        public bool CanDownConstruct
        {
            get
            {
                if (SelectedConstruct == null)
                {
                    return false;
                }
                if (!CanReorderVariable())
                {
                    return false;
                }
                if (SelectedConstructIndex == Constructs.Count - 1)
                {
                    return false;
                }
                return true;
            }
        }



        public void DownConstruct()
        {
            ConstructVM construct = SelectedConstruct;
            int fromIndex = SelectedConstructIndex;
            int toIndex = fromIndex + 1;
            MoveConstruct(fromIndex, toIndex);
        }


        #endregion


        public void RemoveQuestion(QuestionVM question)
        {
            bool removed = false;
            for (int i = constructs.Count - 1; i >= 0; i--)
            {
                ConstructVM construct = constructs[i];
                if (construct is QuestionConstructVM )
                {
                    QuestionConstructVM questionConstruct = (QuestionConstructVM)construct;
                    if (questionConstruct.Question == question)
                    {
                        constructs.RemoveAt(i);
                        removed = true;
                    }
                }
            }
            if (removed)
            {
                UpdateModel(false);
            }
        }


        private ICommand previewCommand;
        public ICommand PreviewCommand
        {
            get
            {
                if (previewCommand == null)
                {
                    previewCommand = new RelayCommand(param => Preview(), param => CanPreview);
                }
                return previewCommand;
            }
        }

        public bool CanPreview
        {
            get
            {
                return true;
            }
        }


        public void Preview()
        {
            ChartWindowVM vm = new ChartWindowVM(this);
            ChartWindow window = new ChartWindow(vm);
            window.Owner = Application.Current.MainWindow; 
            window.ShowDialog();
        }

        private ICommand changeQuestionNumbersCommand;
        public ICommand ChangeQuestionNumbersCommand
        {
            get
            {
                if (changeQuestionNumbersCommand == null)
                {
                    changeQuestionNumbersCommand = new RelayCommand(param => ChangeQuestionNumbers(), param => CanChangeQuestionNumbers);
                }
                return changeQuestionNumbersCommand;
            }
        }

        public bool CanChangeQuestionNumbers
        {
            get
            {
                return true;
            }
        }

        public void ChangeQuestionNumbers()
        {
            ChangeMultipleQuestionNumberWindowVM vm = new ChangeMultipleQuestionNumberWindowVM(this);
            ChangeMultipleQuestionNumberWindow window = new ChangeMultipleQuestionNumberWindow(vm);
            window.Owner = Application.Current.MainWindow;
            if (window.ShowDialog() == true)
            {
                using (UndoTransaction tx = new UndoTransaction(UndoManager))
                {
                    if (SequenceUtils.RenumberQuestionNumbers(this, vm.QuestionNumbers))
                    {
                        UpdateModel(false);
                        tx.Commit();
                    }
                }
            }
        }
    }
}
