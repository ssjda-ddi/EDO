using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.ViewModel;

namespace EDO.DataCategory.StatisticsForm
{
    public class StatisticsInfoRowVM :BaseVM
    {
        public StatisticsInfoRowVM()
        {
        }

        private string column0;
        public string Column0
        {
            get
            {
                return column0;
            }
            set
            {
                if (column0 != value)
                {
                    column0 = value;
                    NotifyPropertyChanged("Column0");
                }
            }
        }

        private string column1;
        public string Column1
        {
            get
            {
                return column1;
            }
            set
            {
                if (column1 != value)
                {
                    column1 = value;
                    NotifyPropertyChanged("Column1");
                }
            }
        }

        private string column2;
        public string Column2
        {
            get
            {
                return column2;
            }
            set
            {
                if (column2 != value)
                {
                    column2 = value;
                    NotifyPropertyChanged("Column2");
                }
            }
        }

        private string column3;
        public string Column3
        {
            get
            {
                return column3;
            }
            set
            {
                if (column3 != value)
                {
                    column3 = value;
                    NotifyPropertyChanged("Column3");
                }
            }
        }

        private string column4;
        public string Column4
        {
            get
            {
                return column4;
            }
            set
            {
                if (column4 != value)
                {
                    column4 = value;
                    NotifyPropertyChanged("Column4");
                }
            }
        }

        private string column5;
        public string Column5
        {
            get
            {
                return column5;
            }
            set
            {
                if (column5 != value)
                {
                    column5 = value;
                    NotifyPropertyChanged("Column5");
                }
            }
        }

        private string column6;
        public string Column6
        {
            get
            {
                return column6;
            }
            set
            {
                if (column6 != value)
                {
                    column6 = value;
                    NotifyPropertyChanged("Column6");
                }
            }
        }

        private string column7;
        public string Column7
        {
            get
            {
                return column7;
            }
            set
            {
                if (column7 != value)
                {
                    column7 = value;
                    NotifyPropertyChanged("Column7");
                }
            }
        }

    }
}
