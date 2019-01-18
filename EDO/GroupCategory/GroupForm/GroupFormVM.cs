using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.ViewModel;
using EDO.Main;
using System.Collections.ObjectModel;
using EDO.Core.Model;

namespace EDO.GroupCategory.GroupForm
{
    public class GroupFormVM :FormVM
    {

        private Group group;

        public GroupFormVM(GroupVM group)
            : base(group)
        {
            this.group = group.GroupModel;
            this.Times = Options.Times;
            this.Instruments = Options.Instruments;
            this.Panels = Options.Panels;
            this.Geographies = Options.Geographies;
            this.DataSets = Options.DataSets;
            this.Languages = Options.Languages;
        }

        public ObservableCollection<Option> Times { get; set; }
        public ObservableCollection<Option> Instruments { get; set; }
        public ObservableCollection<Option> Panels { get; set; }
        public ObservableCollection<Option> Geographies { get; set; }
        public ObservableCollection<Option> DataSets { get; set; }
        public ObservableCollection<Option> Languages { get; set; }

        // Time Method
        public string TimeCode
        {
            get
            {
                return group.TimeCode;
            }
            set
            {
                if (group.TimeCode != value)
                {
                    group.TimeCode = value;
                    NotifyPropertyChanged("TimeCode");
                    Memorize();
                }
            }
        }

        // Questionnaire
        public string InstrumentCode
        {
            get
            {
                return group.InstrumentCode;
            }
            set
            {
                if (group.InstrumentCode != value)
                {
                    group.InstrumentCode = value;
                    NotifyPropertyChanged("InstrumentCode");
                    Memorize();
                }
            }
        }

        // Panel
        public string PanelCode
        {
            get
            {
                return group.PanelCode;
            }
            set
            {
                if (group.PanelCode != value)
                {
                    group.PanelCode = value;
                    NotifyPropertyChanged("PanelCode");
                    Memorize();
                }
            }
        }

        // Geograpy
        public string GeographyCode
        {
            get
            {
                return group.GeographyCode;
            }
            set
            {
                if (group.GeographyCode != value)
                {
                    group.GeographyCode = value;
                    NotifyPropertyChanged("GeographyCode");
                    Memorize();
                }
            }
        }

        // Dataset
        public string DataSetCode
        {
            get
            {
                return group.DataSetCode;
            }
            set
            {
                if (group.DataSetCode != value)
                {
                    group.DataSetCode = value;
                    NotifyPropertyChanged("DataSetCode");
                    Memorize();
                }
            }
        }

        // Language
        public string LanguageCode
        {
            get
            {
                return group.LanguageCode;
            }
            set
            {
                if (group.LanguageCode != value)
                {
                    group.LanguageCode = value;
                    NotifyPropertyChanged("LanguageCode");
                    Memorize();
                }
            }
        }

        // Purpose
        public string Purpose
        {
            get
            {
                return group.Purpose;
            }
            set
            {
                if (group.Purpose != value)
                {
                    group.Purpose = value;
                    NotifyPropertyChanged("Purpose");
                    Memorize();
                }
            }
        }

        // Studies in this Group
        public string IncludeStudyUnitNames
        {
            get
            {
                MainWindowVM main = this.Main;
                return main.StudyUnitNames;
            }
        }
    }
}
