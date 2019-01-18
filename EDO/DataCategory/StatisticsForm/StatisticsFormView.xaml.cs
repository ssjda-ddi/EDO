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
using EDO.Core.View;
using EDO.Core.Util;

namespace EDO.DataCategory.StatisticsForm
{
    /// <summary>
    /// AggregateValueFormView.xaml の相互作用ロジック
    /// </summary>
    public partial class StatisticsFormView : FormView
    {
        public StatisticsFormView()
        {
            InitializeComponent();
        }

        private StatisticsFormVM VM
        {
            get
            {
                return EDOUtils.GetVM<StatisticsFormVM>(this);
            }
        }
    }
}
