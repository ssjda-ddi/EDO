using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Main;
using EDO.Core.ViewModel;
using System.Collections.ObjectModel;
using EDO.VariableCategory.VariableForm;
using EDO.QuestionCategory.QuestionForm;
using EDO.QuestionCategory.ConceptForm;
using EDO.SamplingCategory.SamplingForm;
using EDO.Core.Model;
using EDO.Core.View;
using EDO.Core.Util;

namespace EDO.DataCategory.StatisticsForm
{
    public class StatisticsFormVM :FormVM
    {
        public StatisticsFormVM(StudyUnitVM studyUnit)
            : base(studyUnit)
        {
            variableInfos = new ObservableCollection<VariableInfoVM>();
            statisticsInfos = new ObservableCollection<StatisticsInfoVM>();
        }

        private void CreateVariableInfos()
        {
            variableInfos.Clear();
            ObservableCollection<VariableVM> variables = StudyUnit.Variables;
            foreach (VariableVM variable in variables)
            {
                VariableInfoVM variableInfo = new VariableInfoVM(variable.Variable);
                variableInfos.Add(variableInfo);
                ConceptVM concept = StudyUnit.FindConcept(variable.ConceptId);
                if (concept != null)
                {
                    variableInfo.ConceptTitle = concept.Title;
                }
                QuestionVM question = StudyUnit.FindQuestion(variable.QuestionId);
                if (question != null)
                {
                    variableInfo.QuestionTitle = question.Content;
                }
                UniverseVM universe = StudyUnit.FindUniverse(variable.UniverseId);
                if (universe != null)
                {
                    variableInfo.UniverseTitle = universe.Title;
                }
                variableInfo.VariableType = Options.FindResponseTypeLabel(variable.ResponseTypeCode);
            }
            selectedVariableInfoItem = null;
        }

        private void CreateStatisticsInfos()
        {
            statisticsInfos.Clear();
            List<StatisticsInfo> statisticsInfoModels = StudyUnit.StatisticsInfoModels;
            foreach (StatisticsInfo statisticsInfoModel in statisticsInfoModels)
            {
                StatisticsInfoVM statisticsInfo = new StatisticsInfoVM(statisticsInfoModel);
                statisticsInfos.Add(statisticsInfo);
            }
        }

        private ObservableCollection<VariableInfoVM> variableInfos;
        public ObservableCollection<VariableInfoVM> VariableInfos { get { return variableInfos; } }

        private ObservableCollection<StatisticsInfoVM> statisticsInfos;
        public ObservableCollection<StatisticsInfoVM> StatisticsInfos { get { return statisticsInfos; } }

        private string SelectedVariableId
        {
            get
            {
                if (selectedVariableInfoItem == null)
                {
                    return null;
                }
                return ((VariableInfoVM)selectedVariableInfoItem).Id;
            }
        }

        public VariableInfoVM SelectedVariableInfo
        {
            get
            {
                return selectedVariableInfoItem as VariableInfoVM;
            }
        }

        private void SelectStatisticsInfo()
        {
            StatisticsInfoVM statisticsInfo = null;
            VariableInfoVM variableInfo = SelectedVariableInfo;
            if (variableInfo != null)
            {
                statisticsInfo = StatisticsInfoVM.FindByQuestionIdOrVariableId(StatisticsInfos, variableInfo.QuestionId, variableInfo.Id);
            }
            SelectedStatisticsInfo = statisticsInfo;
        }

        private string selectedVariableId;
        private object selectedVariableInfoItem;
        public object SelectedVariableInfoItem
        {
            get
            {
                return selectedVariableInfoItem;
            }
            set
            {
                if (selectedVariableInfoItem != value)
                {
                    selectedVariableInfoItem = value;
                    selectedVariableId = SelectedVariableId;
                    NotifyPropertyChanged("SelectedVariableInfoItem");
                    SelectStatisticsInfo();
                }
            }
        }

        private StatisticsInfoVM selectedStatisticsInfo;
        public StatisticsInfoVM SelectedStatisticsInfo
        {
            get
            {
                return selectedStatisticsInfo;
            }
            set
            {
                if (selectedStatisticsInfo != value)
                {
                    selectedStatisticsInfo = value;
                    NotifyPropertyChanged("SelectedStatisticsInfo");

                    NotifyPropertyChanged("SelectedStatisticsType");
                }
            }
        }

        public StatisticsTypes SelectedStatisticsType
        {
            get
            {
                if (selectedStatisticsInfo == null)
                {
                    return StatisticsTypes.Unknown;
                }
                StatisticsTypes[] validTypes = new StatisticsTypes[] {
                    StatisticsTypes.Number,
                    StatisticsTypes.ChoicesSingleAnswer,
                    StatisticsTypes.ChoicesMultipleAnswer,
                    StatisticsTypes.DateTime
                };
                if (validTypes.Contains(selectedStatisticsInfo.StatisticsType))
                {
                    return selectedStatisticsInfo.StatisticsType;
                }
                return StatisticsTypes.Unknown;
            }
        }

        protected override void Reload(VMState state)
        {
            CreateVariableInfos();
            CreateStatisticsInfos();
            if (state != null)
            {
                if (state.State1 != null)
                {
                    SelectedVariableInfoItem = EDOUtils.Find(variableInfos, (string)state.State1);
                }
            }
            if (SelectedVariableInfoItem == null)
            {
                SelectedVariableInfoItem = EDOUtils.FindOrFirst(variableInfos, selectedVariableId); // keep selectedIndex when page changes
            }
        }

        public override VMState SaveState()
        {
            return new VMState(selectedVariableId);
        }
    }
}
