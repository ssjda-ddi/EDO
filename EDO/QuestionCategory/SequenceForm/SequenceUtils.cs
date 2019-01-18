using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.Model;
using EDO.Core.Util;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Diagnostics;
using EDO.Core.ViewModel;
using EDO.Properties;

namespace EDO.QuestionCategory.SequenceForm
{
    public static class SequenceUtils
    {

        private static IfCondition CreateIfCondition(List<CondGroupVM> condGroups)
        {
            StringBuilder buf = new StringBuilder();
            foreach (CondGroupVM condGroup in condGroups)
            {
                buf.Append(condGroup.Expression(BaseVM.EMPTY_VALUE));
            }
            IfCondition ifCondition = new IfCondition();
            ifCondition.Code = buf.ToString();
            ifCondition.QuestionId = condGroups[0].FirstQuestionConstruct.Id;    
            return ifCondition;
        }

        private static IfThenElse CreateIfThenElse(BranchVM branch)
        {
            IfThenElse ifThenElse = new IfThenElse();
            ifThenElse.No = "-";
            ifThenElse.IfCondition = CreateIfCondition(branch.ValidCondGroups);
            ifThenElse.ThenConstructId = branch.ThenConstruct.Id;
            return ifThenElse;
        }

        private static ElseIf CreateElseIf(BranchVM branch)
        {
            ElseIf elseIf = new ElseIf();
            elseIf.IfCondition = CreateIfCondition(branch.ValidCondGroups);
            elseIf.ThenConstructId = branch.ThenConstruct.Id;
            return elseIf;
        }

        public static IfThenElse CreateIfThenElse(List<BranchVM> branches)
        {
            if (branches.Count == 0)
            {
                return null;
            }
            IfThenElse ifThenElse = CreateIfThenElse(branches[0]);
            for (int i = 1; i < branches.Count; i++)
            {
                BranchVM branch = branches[i];
                if (branch.IsTypeElseIf)
                {
                    ElseIf elseIf = CreateElseIf(branch);
                    if (elseIf != null)
                    {
                        ifThenElse.ElseIfs.Add(elseIf);
                    }
                }
                else if (branch.IsTypeElse)
                {
                    ifThenElse.ElseConstructId = branch.ThenConstruct.Id;
                }                
            }
            return ifThenElse;
        }


        private static void CreateCondGroups(string code, BranchVM branch, ObservableCollection<QuestionConstructVM> questionConstructs)
        {
            CondParser parser = new CondParser(code);
            List<CondGroup> condGroupElems  = parser.Parse();

            foreach (CondGroup condGroupElem in condGroupElems)
            {
                CondGroupVM condGroup = new CondGroupVM()
                {
                    Parent = branch
                };
                condGroup.SelectedConnectionCode = Option.FindCodeByLabel(Options.Connections, condGroupElem.Connector);
                branch.CondGroups.Add(condGroup);

                foreach (Cond condElem in condGroupElem.Conds)
                {
                    CondVM cond = new CondVM()
                    {
                        Parent = condGroup
                    };
                    condGroup.Conds.Add(cond);
                    cond.SelectedQuestionConstruct = QuestionConstructVM.FindQuestionConstructByNo(questionConstructs, condElem.LeftValue);
                    cond.SelectedOperatorCode = Option.FindCodeByLabel(Options.Operators, condElem.Operator);
                    cond.CondValue = condElem.RightValue;
                }
            }
        }

        private static BranchVM CreateIfBranch(IfThenElse ifThenElse, CreateBranchWindowVM window)
        {
            BranchVM branch = new BranchVM(BranchVM.TYPE_IF_CODE)
            {
                Parent = window
            };
            branch.Init();
            branch.CondGroups.Clear();
            IfCondition ifCondition = ifThenElse.IfCondition;
            CreateCondGroups(ifCondition.Code, branch, window.QuestionConstructs);
            branch.ThenConstruct = EDOUtils.Find(window.ThenConstructs, ifThenElse.ThenConstructId);
            return branch;
        }

        private static List<BranchVM> CreateElseIfBranches(IfThenElse ifThenElse, CreateBranchWindowVM window)
        {
            List<BranchVM> branches = new List<BranchVM>();
            List<ElseIf> elseIfs = ifThenElse.ElseIfs;
            foreach (ElseIf elseIf in elseIfs)
            {
                BranchVM branch = new BranchVM(BranchVM.TYPE_ELSE_IF_CODE)
                {
                    Parent = window
                };
                branch.Init();
                branch.CondGroups.Clear();
                CreateCondGroups(elseIf.IfCondition.Code, branch, window.QuestionConstructs);
                branch.ThenConstruct = EDOUtils.Find(window.ThenConstructs, elseIf.ThenConstructId);
                branches.Add(branch);
            }
            return branches;
        }

        private static BranchVM CreateElseBranch(IfThenElse ifThenElse, CreateBranchWindowVM window)
        {
            if (ifThenElse.ElseConstructId == null)
            {
                return null;
            }
            BranchVM branch = new BranchVM(BranchVM.TYPE_ELSE_CODE)
            {
                Parent = window
            };
            branch.Init();
            branch.CondGroups.Clear();
            branch.ThenConstruct = EDOUtils.Find(window.ThenConstructs, ifThenElse.ElseConstructId);
            return branch;
        }

        public static ObservableCollection<BranchVM> CreateBranches(IfThenElse ifThenElse, CreateBranchWindowVM window)
        {
            ObservableCollection<BranchVM> branches = new ObservableCollection<BranchVM>();
            BranchVM ifBranch = CreateIfBranch(ifThenElse, window);
            branches.Add(ifBranch);
            List<BranchVM> elseIfBranches = CreateElseIfBranches(ifThenElse, window);
            branches.AddRange(elseIfBranches);
            BranchVM elseBranch = CreateElseBranch(ifThenElse, window);
            if (elseBranch != null)
            {
                branches.Add(elseBranch);
            }
            return branches;
        }

        private static Dictionary<string, List<StatementVM>> CreateStatementDict(ControlConstructSchemeVM controlConstructScheme)
        {
            var result = new Dictionary<string, List<StatementVM>>();
            foreach (StatementVM statement in controlConstructScheme.Statements)
            {
                string no = statement.No;
                List<StatementVM> statements = null;
                if (result.ContainsKey(no))
                {
                    statements = result[no];
                }
                else
                {
                    statements = new List<StatementVM>();
                    result[no] = statements;
                }
                statements.Add(statement);
            }
            return result;
        }

        private static List<QuestionNumberVM> CreateAllQuestionNumber(ControlConstructSchemeVM controlConstructScheme, QuestionNumberVM questionNumber)
        {
            List<QuestionNumberVM> questionNumbers = new List<QuestionNumberVM>();
            foreach (QuestionConstructVM questionConstruct in controlConstructScheme.QuestionConstructs)
            {
                QuestionNumberVM newQuestionNumber = null;
                if (questionConstruct == questionNumber.QuestionConstruct)
                {
                    newQuestionNumber = questionNumber;
                }
                else
                {
                    newQuestionNumber = new QuestionNumberVM(questionConstruct);
                }
                questionNumbers.Add(newQuestionNumber);
            }
            return questionNumbers;
        }

        public static void ValidateQuestionNumber(ControlConstructSchemeVM controlConstructScheme, QuestionNumberVM questionNumber)
        {
            List<QuestionNumberVM> questionNumbers = CreateAllQuestionNumber(controlConstructScheme, questionNumber); // for duplicate check
            ValidateQuestionNumbers(controlConstructScheme, questionNumbers);
        }

        public static void ValidateQuestionNumbers(ControlConstructSchemeVM controlConstructScheme, ICollection<QuestionNumberVM> questionNumbers)
        {
            QuestionNumberVM.ClearValidationFlags(questionNumbers);
            Dictionary<string, List<QuestionNumberVM>> questionNumberDict = QuestionNumberVM.CreateQuestionNumberDict(questionNumbers);
            Dictionary<string, List<StatementVM>> statementDict = CreateStatementDict(controlConstructScheme);
            foreach (KeyValuePair<string, List<QuestionNumberVM>> pair in questionNumberDict)
            {
                List<StatementVM> statements = new List<StatementVM>();
                if (statementDict.ContainsKey(pair.Key))
                {
                    statements = statementDict[pair.Key];
                }
                QuestionNumberVM.CheckDuplicate(pair.Key, pair.Value, statements);
            }
            int errorCount = QuestionNumberVM.CountError(questionNumbers);
            if (errorCount > 0)
            {
                throw new ApplicationException(Resources.DuplicateQuestionNumber);
            }
        }

        private static bool UpdateQuestionNumberOfCond(CondVM cond, ICollection<QuestionNumberVM> questionNumbers)
        {
            foreach (QuestionNumberVM questionNumber in questionNumbers)
            {
                if (cond.SelectedQuestionConstructNo == questionNumber.BeforeValue)
                {
                    cond.ForceUpdateSelectedQuestionConstructNo(questionNumber.AfterValue);
                    return true;
                }
            }
            return false;
        }

        private static bool UpdateQuestionNumbers(ICollection<CondVM> conds, ICollection<QuestionNumberVM> questionNumbers)
        {
            bool updated = false;
            foreach (CondVM cond in conds)
            {
                if (UpdateQuestionNumberOfCond(cond, questionNumbers))
                {
                    updated = true;
                }
            }
            return updated;
        }

        private static bool UpdateQuestionNumbers(ICollection<CondGroupVM> condGroups, ICollection<QuestionNumberVM> questionNumbers)
        {
            bool updated = false;
            foreach (CondGroupVM condGroup in condGroups)
            {
                if (UpdateQuestionNumbers(condGroup.Conds, questionNumbers))
                {
                    updated = true;
                }
            }
            return updated;
        }


        private static bool UpdateQuestionNumberOfThenConstructNo(BranchVM branch, ICollection<QuestionNumberVM> questionNumbers)
        {
            foreach (QuestionNumberVM questionNumber in questionNumbers)
            {
                if (questionNumber.QuestionConstruct == branch.ThenConstruct)
                {
                    branch.ForceUpdateThenConstructNo(questionNumber.AfterValue);
                    return true;
                }
            }
            return false;
        }

        private static bool UpdateQuestionNumbers(ICollection<BranchVM> branches, ICollection<QuestionNumberVM> questionNumbers)
        {
            bool updated = false;
            foreach (BranchVM branch in branches)
            {
                if (UpdateQuestionNumbers(branch.CondGroups, questionNumbers))
                {
                    updated = true;
                }
                if (UpdateQuestionNumberOfThenConstructNo(branch, questionNumbers))
                {
                    updated = true;
                }

            }
            return updated;
        }

        private static Dictionary<IfThenElseVM, CreateBranchWindowVM> UpdateQuestionNumberOfBranches(ControlConstructSchemeVM controlConstructScheme, ICollection<QuestionNumberVM> updatingQuestionNumbers)
        {
            Dictionary<IfThenElseVM, CreateBranchWindowVM> updatingIfThenElseDict = new Dictionary<IfThenElseVM, CreateBranchWindowVM>();

            List<IfThenElseVM> ifThenElses = controlConstructScheme.IfThenElses;
            foreach (IfThenElseVM ifThenElse in ifThenElses)
            {
                CreateBranchWindowVM vm = new CreateBranchWindowVM(controlConstructScheme, ifThenElse.IfThenElse);
                if (UpdateQuestionNumbers(vm.Branches, updatingQuestionNumbers))
                {
                    updatingIfThenElseDict[ifThenElse] = vm;
                }
            }
            return updatingIfThenElseDict;
        }

        private static void ReplaceIfThenElses(ControlConstructSchemeVM controlConstructScheme, Dictionary<IfThenElseVM, CreateBranchWindowVM> updatingIfThenElseDict)
        {
            Dictionary<IfThenElseVM, IfThenElseVM> ifThenElses = new Dictionary<IfThenElseVM, IfThenElseVM>();
            foreach (KeyValuePair<IfThenElseVM, CreateBranchWindowVM> pair in updatingIfThenElseDict)
            {
                IfThenElseVM ifThenElse = pair.Key;
                CreateBranchWindowVM vm = pair.Value;
                vm.Save();
                IfThenElseVM newIfThenElse = new IfThenElseVM(vm.IfThenElse);
                controlConstructScheme.ReplaceIfThenElse(ifThenElse, newIfThenElse);
            }
        }

        public static bool RenumberQuestionNumber(ControlConstructSchemeVM controlConstructScheme, QuestionNumberVM questionNumber)
        {
            List<QuestionNumberVM> questionNumbers = new List<QuestionNumberVM>();
            questionNumbers.Add(questionNumber);
            return RenumberQuestionNumbers(controlConstructScheme, questionNumbers);
        }

        public static bool RenumberQuestionNumbers(ControlConstructSchemeVM controlConstructScheme, ICollection<QuestionNumberVM> questionNumbers)
        {
            List<QuestionNumberVM> updatingQuestionNumbers = QuestionNumberVM.FindUpdatingQuestionNumbers(questionNumbers);
            if (updatingQuestionNumbers.Count == 0)
            {
                return false;
            }
            // Change question numbers in the "if condition"s (need before and after question values) on memory
            Dictionary<IfThenElseVM, CreateBranchWindowVM> updatingIfThenElseDict = UpdateQuestionNumberOfBranches(controlConstructScheme, updatingQuestionNumbers);
            // Change QuestionConstruct's no
            QuestionNumberVM.UpdateQuestionNumbers(updatingQuestionNumbers);
            // replace if then else.
            ReplaceIfThenElses(controlConstructScheme, updatingIfThenElseDict);
            return true;
        }

        public static bool ValidateQuestionNumber(string number)
        {
            Regex reg = new Regex(@"^\w+$");
            return reg.IsMatch(number);
        }
    }
}
