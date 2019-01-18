using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.ViewModel;
using System.Collections.ObjectModel;
using System.Diagnostics;
using EDO.Core.Model;
using System.ComponentModel;

namespace EDO.GroupCategory.CompareForm
{
    public class CompareRowVM :BaseVM, IEditableObject
    {
        public static CompareRowVM FindByStringId(ObservableCollection<CompareRowVM> rows, string rowId)
        {
            //Search rows using stringified ID
            foreach (CompareRowVM row in rows)
            {
                if (row.Id.ToString() == rowId)
                {
                    return row;
                }
            }
            return null;
        }

        private static void UpdateCell(CompareCell cell, DiffOption diffOption, ObservableCollection<CompareRowVM> rows)
        {
            if (diffOption == null)
            {
                return;
            }
            if (diffOption.IsPartialMatch)
            {
                //Get and save the list of GroupId when matches partially Code contains ROWID
                cell.CompareValue = Options.COMPARE_VALUE_PARTIALMATCH_CODE;
                CompareRowVM targetRow = FindByStringId(rows, diffOption.Code);
                cell.TargetTitle = targetRow.Title;
            }
            else
            {
                //In the case of ○ or ×, save as is.
                cell.CompareValue = diffOption.Code;
                cell.TargetTitle = null;
            }
        }

        public CompareRowVM(List<string> studyUnitGuids)
        {
            rowModel = new CompareRow();
            this.studyUnitGuids = studyUnitGuids;
            diffOptions = new ObservableCollection<DiffOption>();
            selectedDiffOptions = new ObservableCollection<DiffOption>();
            foreach (string guid in studyUnitGuids)
            {
                selectedDiffOptions.Add(null);
            }
            backSelectedDiffOptions = new List<DiffOption>();
        }

        #region Property

        private CompareRow rowModel;
        public CompareRow RowModel { get { return rowModel; } }
        public string Id { get { return rowModel.Id; } }
        public void AddGroupId(GroupId groupId)
        {
            rowModel.RowGroupIds.Add(groupId);
        }
        public List<GroupId> RowGroupIds { get { return rowModel.RowGroupIds; } }

        private List<string> studyUnitGuids;

        private ObservableCollection<DiffOption> diffOptions;
        public ObservableCollection<DiffOption> DiffOptions { get { return diffOptions; } }

        private ObservableCollection<DiffOption> selectedDiffOptions;
        public ObservableCollection<DiffOption> SelectedDiffOptions { get { return selectedDiffOptions; } }

        private string backTitle;
        private string backMemo;
        private List<DiffOption> backSelectedDiffOptions;

        public string Title
        {
            get
            {
                return rowModel.Title;
            }
            set
            {
                if (rowModel.Title != value)
                {
                    rowModel.Title = value;
                    NotifyPropertyChanged("Title");
                }
            }
        }

        public string Memo
        {
            get
            {
                return rowModel.Memo;
            }
            set
            {
                if (rowModel.Memo != value)
                {
                    rowModel.Memo = value;
                    NotifyPropertyChanged("Memo");
                }
            }
        }

        public bool ContainsAll(List<GroupId> rowGroupIds)
        {
            return rowModel.ContainsAll(rowGroupIds);
        }

        #endregion

        #region initialize from the model; reflect on the model

        private DiffOption FindDiffOptionByCode(string code)
        {
            foreach (DiffOption diffOption in diffOptions)
            {
                if (diffOption.Code == code)
                {
                    return diffOption;
                }
            }
            return null;
        }

        private DiffOption FindDiffOptionByTitle(ObservableCollection<CompareRowVM> rows, string title)
        {
            //loop through all DiffOption
            foreach (DiffOption diffOption in diffOptions)
            {
                if (diffOption.IsMatchOrNotMatch)
                {
                    continue;
                }
                //get rows 
                CompareRowVM row = FindByStringId(rows, diffOption.Code);
                if (row.Title == title)
                {
                    //Select the current line if it contains the previous contents completely
                    return diffOption;
                }
            }
            return null;
        }

        private DiffOption FindDiffOption(ObservableCollection<CompareRowVM> rows, CompareCell cell)
        {
            //Search in DiffOption from the contents of existent cells
            if (cell == null)
            {
                return null;
            }
            DiffOption diffOption = null;
            if (Options.IsPartialMatch(cell.CompareValue))
            {
                diffOption = FindDiffOptionByTitle(rows, cell.TargetTitle);
            }
            else
            {
                diffOption = FindDiffOptionByCode(cell.CompareValue);
            }
            return diffOption;
        }

        private void CreateDiffOptions(ObservableCollection<CompareRowVM> rows)
        {
            diffOptions.Clear();
            diffOptions.Add(new DiffOption(Options.CompareValueMatch));
            diffOptions.Add(new DiffOption(Options.CompareValueNotMatch));
            foreach (CompareRowVM row in rows)
            {
                if (row != this)
                {
                    diffOptions.Add(new DiffOption(row.Id.ToString(), Options.CompareValuePartialMatch.Label, row.Title));
                }
            }
        }

        private void CreateCells()
        {
            rowModel.Cells.Clear();
            foreach (string studyUnitId in studyUnitGuids)
            {
                CompareCell cell = new CompareCell();
                rowModel.Cells.Add(cell);
                cell.ColumnStudyUnitId = studyUnitId;
            }
        }

        private void UpdateViewModel(ObservableCollection<CompareRowVM> rows, CompareRow existRowModel)
        {
            if (existRowModel == null)
            {
                return;
            }
            //always use an up-to-date title 
            //Use an existing memo
            Memo = existRowModel.Memo;
            //Update the value of the cell. there may be a variable of the same variable name in the same StudyUnit
            selectedDiffOptions.Clear();
            foreach (CompareCell cell in rowModel.Cells)
            {
                //Get an existing cell that corresponds to the cell
                CompareCell existCell = existRowModel.FindCell(cell.ColumnStudyUnitId);
                //if the cell is existent, convert to DiffOption corresponding to it
                DiffOption diffOption = FindDiffOption(rows, existCell);
                //Add to DiffOption(Onthe screen selectedDiffOptions will be updated)
                selectedDiffOptions.Add(diffOption);
            }
        }

        public void Init(ObservableCollection<CompareRowVM> rows, CompareRow existRowModel)
        {
            //Create Category
            CreateDiffOptions(rows);
            //Create cell
            CreateCells();
            //Update ViewModel using the value of existsing model
            UpdateViewModel(rows, existRowModel);
        }

        public void UpdateModel(ObservableCollection<CompareRowVM> rows)
        {
            //Update the information of model
            int i = 0;
            foreach (DiffOption diffOption in selectedDiffOptions)
            {
                CompareCell cell = rowModel.Cells[i++];
                UpdateCell(cell, diffOption, rows);
            }
        }
        #endregion

        #region IEditableObject

        public bool InEdit { get { return inEdit; } }

        private bool inEdit;

        public void BeginEdit()
        {
            if (inEdit)
            {
                return;
            }
            inEdit = true;
            backTitle = Title;
            backMemo = Memo;
            backSelectedDiffOptions.Clear();
            foreach (DiffOption diffOption in selectedDiffOptions)
            {
                backSelectedDiffOptions.Add(diffOption);
            }
        }

        public void CancelEdit()
        {
            if (!inEdit)
            {
                return;
            }
            inEdit = false;
            Title = backTitle;
            Memo = backMemo;
            selectedDiffOptions.Clear();
            foreach (DiffOption diffOption in backSelectedDiffOptions)
            {
                selectedDiffOptions.Add(diffOption);
            }
        }

        public Action<IEditableObject> ItemEndEditAction { get; set; }

        public void EndEdit()
        {
            if (!inEdit)
            {
                return;
            }
            inEdit = false;
            backTitle = null;
            backMemo = null;
            backSelectedDiffOptions.Clear();

            CompareFormVM parent = (CompareFormVM)Parent;
            parent.UpdateModel();
            Memorize();
        }

        #endregion
    }
}
