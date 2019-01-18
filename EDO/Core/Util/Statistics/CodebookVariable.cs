using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.QuestionCategory.QuestionForm;
using EDO.VariableCategory.VariableForm;
using EDO.Core.Model;
using EDO.Core.Model.Statistics;

namespace EDO.Core.Util.Statistics
{
    // temporary object for codebook writer
    public class CodebookVariable
    {
        public static CodebookVariable FindByQuestionId(List<CodebookVariable> variables, string questionId)
        {
            return variables.Where(x => (x.IsTypeChoicesMultipleAnswer && x.Question.Id == questionId)).FirstOrDefault();
        }

        public CodebookVariable()
        {
            StatisticsType = StatisticsTypes.Unknown;
            Variables = new List<VariableVM>();
        }

        public StatisticsTypes StatisticsType { get; set; }
        public QuestionVM Question { get; set; }
        public List<VariableVM> Variables { get; set; }
        public VariableVM FirstVariable { get { return Variables.FirstOrDefault(); } }
        public StatisticsInfo StatisticsInfo { get; set; }
        public bool IsTypeChoicesSingleAnswer { get { return StatisticsInfo.IsTypeChoicesSingleAnswerOf(StatisticsType); } }
        public bool IsTypeChoicesMultipleAnswer { get { return StatisticsInfo.IsTypeChoicesMultipleAnswerOf(StatisticsType); } }
        public bool IsTypeNumber { get { return StatisticsInfo.IsTypeNumberOf(StatisticsType); } }
        public bool IsTypeFree { get { return StatisticsInfo.IsTypeFreeOf(StatisticsType); } }
        public bool IsTypeDateTime { get { return StatisticsInfo.IsTypeDateTimeOf(StatisticsType); } }
        public bool IsTypeUnknown { get { return StatisticsInfo.IsTypeUnknownOf(StatisticsType); } }

        public bool HasData { get { return StatisticsInfo != null; } }

        private SummaryInfo SummaryInfo {get {return StatisticsInfo != null ? StatisticsInfo.SummaryInfo : null; }}

        public string ValidCasesString { get { return SummaryInfo != null ? SummaryInfo.ValidCasesString : string.Empty; } }
        public string InvalidCasesString { get { return SummaryInfo != null ? SummaryInfo.InvalidCasesString : string.Empty; } }
        public string TotalCasesString { get { return SummaryInfo != null ? SummaryInfo.TotalCasesString : string.Empty; } }
        public string MeanString { get { return SummaryInfo != null ? SummaryInfo.MeanString : string.Empty; } }
        public string MedianString { get { return SummaryInfo != null ? SummaryInfo.MedianString : string.Empty; } }
        public string StandardDeviationString { get { return SummaryInfo != null ? SummaryInfo.StandardDeviationString : string.Empty; } }
        public string MinimumString { get { return SummaryInfo != null ? SummaryInfo.MinimumString : string.Empty; } }
        public string MaximumString { get { return SummaryInfo != null ? SummaryInfo.MaximumString : string.Empty; } }
    }
}
