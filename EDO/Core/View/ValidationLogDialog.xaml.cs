using System.Collections.Generic;
using System.Windows;
using EDO.Core.IO;

namespace EDO.Core.View
{
    /// <summary>
    /// ValidationLogDialog.xaml の相互作用ロジック
    /// </summary>
    public partial class ValidationLogDialog : Window
    {
        private List<DDIIO.ValidationError> errors;


        public ValidationLogDialog(List<DDIIO.ValidationError> errors)
        {
            InitializeComponent();
            this.errors = errors;
            this.DataContext = errors;
            Owner = Application.Current.MainWindow;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
