using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace EDO.QuestionCategory.SequenceForm
{
    /// <summary>
    /// ChangeSingleNumberWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class ChangeSingleQuestionNumberWindow : Window
    {
        private ChangeSingleQuestionNumberWindowVM viewModel;
        public ChangeSingleQuestionNumberWindow(ChangeSingleQuestionNumberWindowVM viewModel)
        {
            InitializeComponent();
            this.viewModel = viewModel;
            DataContext = viewModel;
        }

        private bool ValidateAll()
        {
            bool result = false;
            try
            {
                viewModel.Validate();
                result = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return result;
        }

        private void okButton_Clicked(object sender, RoutedEventArgs e)
        {
            if (!ValidateAll())
            {
                return;
            }
            this.DialogResult = true;
        }

        private void cancelButton_Clicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}
