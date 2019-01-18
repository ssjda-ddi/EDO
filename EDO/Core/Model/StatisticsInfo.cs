using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.Model.Statistics;
using System.Xml.Serialization;
using EDO.Core.Util;

namespace EDO.Core.Model
{
    public enum StatisticsTypes
    {
        ChoicesSingleAnswer,
        ChoicesMultipleAnswer,
        Number,
        Free,
        DateTime,
        Unknown
    };

    public class StatisticsInfo
    {
        public static bool IsTypeChoicesSingleAnswerOf(StatisticsTypes type)
        {
            return StatisticsTypes.ChoicesSingleAnswer == type;
        }

        public static bool IsTypeChoicesMultipleAnswerOf(StatisticsTypes type)
        {
            return StatisticsTypes.ChoicesMultipleAnswer == type;
        }

        public static bool IsTypeNumberOf(StatisticsTypes type)
        {
            return StatisticsTypes.Number == type;
        }

        public static bool IsTypeFreeOf(StatisticsTypes type)
        {
            return StatisticsTypes.Free == type;
        }

        public static bool IsTypeDateTimeOf(StatisticsTypes type)
        {
            return StatisticsTypes.DateTime == type;
        }

        public static bool IsTypeUnknownOf(StatisticsTypes type)
        {
            return StatisticsTypes.Unknown == type;
        }

        public static StatisticsInfo FindByQuestionId(List<StatisticsInfo> infos, string questionId)
        {
            return infos.Where(x => x.QuestionId == questionId).FirstOrDefault();
        }

        public static StatisticsInfo FindByVariableId(List<StatisticsInfo> infos, string variableId)
        {
            return infos.Where(x => x.VariableId == variableId).FirstOrDefault();
        }

        public static StatisticsInfo FindByQuestionIdOrVariableId(List<StatisticsInfo> infos, string questionId, string variableId)
        {
            StatisticsInfo statisticsInfo = FindByQuestionId(infos, questionId);
            if (statisticsInfo != null)
            {
                return statisticsInfo;
            }
            return StatisticsInfo.FindByVariableId(infos, variableId);
        }

        public static void ChangeVariableId(List<StatisticsInfo> statisticsInfos, string oldVariableId, string newVariableId)
        {
            foreach (StatisticsInfo statisticsInfo in statisticsInfos)
            {
                if (statisticsInfo.VariableId == oldVariableId)
                {
                    statisticsInfo.VariableId = newVariableId;
                }
            }
        }

        public StatisticsInfo()
        {
            VariableId = null;
            StatisticsType = StatisticsTypes.Unknown;
            SummaryInfo = new SummaryInfo();
            CategoryInfos = new List<CategoryInfo>();
            Scale = EDOConstants.DEF_SCALE;
        }

        public string VariableId { get; set; }
        public string QuestionId { get; set; } // for Multiple Answer
        public StatisticsTypes StatisticsType { get; set; }
        public SummaryInfo SummaryInfo { get; set; }
        public List<CategoryInfo> CategoryInfos { get; set; }
        public int Scale { get; set; }

        public bool IsTypeChoicesSingleAnswer { get { return IsTypeChoicesSingleAnswerOf(StatisticsType); } }
        public bool IsTypeChoicesMultipleAnswer { get { return IsTypeChoicesMultipleAnswerOf(StatisticsType); } }
        public bool IsTypeNumber { get { return IsTypeNumberOf(StatisticsType); } }
        public bool IsTypeFree { get { return IsTypeFreeOf(StatisticsType); } }
        public bool IsTypeDateTime { get { return IsTypeDateTimeOf(StatisticsType); } }
        public bool IsTypeUnknown { get { return IsTypeUnknownOf(StatisticsType); } }
        public string FullPercent { get { return StatisticsUtils.ToString(100m, Scale); } }

        public void ApplyScale()
        {
            SummaryInfo.Scale = Scale;
            CategoryInfos.ForEach(x => x.Scale = Scale);
        }

        public void CalcPercents()
        {
            StatisticsUtils.CalcPercent(CategoryInfos);
        }
    }
}
