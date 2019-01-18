using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using EDO.Core.Model;
using System.Diagnostics;
using System.Windows;
using EDO.Core.ViewModel;
using EDO.Core.Util;
using System.Windows.Input;
using System.Windows.Data;
using System.Reflection;

namespace EDO.Core.View
{
    public class FormView :UserControl, IValidatable
    {
        public static InputBinding IB(ICommand command, KeyGesture key)
        {
            return new InputBinding(command, key);
        }

        public static InputBindingCollection IBC(params InputBinding[] inputBindings)
        {
            InputBindingCollection collection = new InputBindingCollection(inputBindings);
            return collection;
        }

        public FormView()
        {
            Loaded += new RoutedEventHandler(form_Loaded);
            DataContextChanged += new DependencyPropertyChangedEventHandler(form_DataContextChanged);
        }

        protected FormVM FormVM { get {return EDOUtils.GetVM<FormVM>(this); }}

        private void form_Loaded(object sender, RoutedEventArgs e)
        {
            List<DataGrid> dataGrids = DataGrids;
            foreach (DataGrid dataGrid in DataGrids)
            {
                if (dataGrid != null)
                {
                    dataGrid.InitializingNewItem += dataGrid_InitializingNewItem;
                }
            }
            SetupDataGridInputBindings();
            OnFormLoaded();
        }

        protected virtual void SetupDataGridInputBindings()
        {
            DataGridHelper.SetInputBindings(DataGrids, DataGridInputBindingCollections);
        }

        protected virtual void OnFormLoaded() { }

        private void dataGrid_InitializingNewItem(object sender, InitializingNewItemEventArgs e)
        {
            FormVM.InitRow(e.NewItem);
        }

        protected virtual List<DataGrid> DataGrids
        {
            get
            {
                return new List<DataGrid>();
            }
        }

        protected virtual List<InputBindingCollection> DataGridInputBindingCollections
        {
            get
            {
                return new List<InputBindingCollection>();
            }
        }

        private void form_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            EDOUtils.UpdateViewOfVM(this);
            if (FormVM != null)
            {
//                Debug.WriteLine("IsLoaded=" + IsLoaded);
                if (IsLoaded)
                {
                    // *In most cases form_Loaded will be called so then InputBinding should be set.
                    // *But when the tab is changed opening multiple Study Unit files using EventFormView
                    // Confirmed that form_Loaded was not called and InputBinding was not set as a result.
                    // *In above case, DataContext seems to be modified keeping IsLoaded=true, so here I set InputBindings.
                    // *Because it is called in the order of form_DataContextChanged (IsLoaded = false) -> form_Loaded, so never InputBindings is set twice
                    // *In the case of IsLoaded=false, DataGridHelper.FindDataGrid(this, "universeDataGrid") might return null.
                    SetupDataGridInputBindings();
                }
            }
            OnFormDataContextChanged();
        }

        protected virtual void OnFormDataContextChanged() { }

        public void FinalizeDataGrid()
        {
            DataGridHelper.Finalize(DataGrids);
        }

        public virtual bool Validate()
        {
            List<DataGrid> dataGrids = DataGrids;
            DataGridHelper.CommitEdit(dataGrids);
            if (!Validator.Validate(this))
            {
                return false;
            }
            DataGridHelper.Finalize(dataGrids);
            FormVM.Complete(null);
            return true;
        }
    }
}
