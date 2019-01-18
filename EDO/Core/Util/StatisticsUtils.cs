using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.Model.Statistics;
using EDO.Core.Model;
using EDO.QuestionCategory.CodeForm;
using EDO.VariableCategory.VariableForm;
using EDO.Core.IO;
using EDO.Main;
using EDO.QuestionCategory.QuestionForm;
using System.Collections.ObjectModel;
using System.Diagnostics;
using EDO.Core.Util.Statistics;

namespace EDO.Core.Util
{
    public class StatisticsUtils
    {
        #region Common Utility Methods

        public static bool IsEmptyValue(object value)
        {
            if (value == null || DBNull.Value.Equals(value))
            {
                return true;
            }
            if (value is string || value is double)
            {
                return false;
            }
            return true; // unknown type
        }

        public static string ToCategoryValue(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            string result = null;
            if (value is double) // for spss
            {
                int intValue = Convert.ToInt32((double)value);
                result = intValue.ToString();
            }
            else 
            {
                int num = 0;
                result = value.ToString();
                if (!int.TryParse(result, out num))
                { 
                    // for xls ("1.00000" => "1")
                    decimal decimalValue = 0;
                    if (decimal.TryParse(result, out decimalValue))
                    {
                        result = string.Format("{0:G29}", decimalValue);
                    }
                } 
            }
            return result;
        }

        public static double ToDouble(object value)
        {
            double result = 0.0;
            if (value is double)
            {
                result = (double)value;
            }
            else
            {
                double.TryParse(value.ToString(), out result);
            }
            return result;
        }

        public static int ToInt(string value)
        {
            int result = 0;
            int.TryParse(value, out result);
            return result;
        }

        public static decimal ToDecimal(string value)
        {
            decimal result = 0m;
            decimal.TryParse(value, out result);
            return result;
        }

        public static decimal Round(decimal value, int scale = 1)
        {
            //digits=1
            //1.23->1.2
            //digits=2
            // 1.402->1.41
            return Math.Round(value, scale);
        }

        public static string RoundAsString(decimal value, int scale = 1)
        {
            return Round(value, scale).ToString();
        }

        public static string BuildFormat(int scale)
        {
            StringBuilder fmt = new StringBuilder("{0:0");
            if (scale > 0)
            {
                fmt.Append(".");
            }
            for (int i = 0; i < scale; i++)
            {
                fmt.Append("0");
            }
            fmt.Append("}");
            return fmt.ToString();
        }

        public static string ToString(double value, int scale = 2)
        {
            // Build format string (like {0:0.00})
            string fmt = BuildFormat(scale);
            return string.Format(fmt, value);
        }

        public static string ToString(decimal value, int scale = 2)
        {
            // Build format string (like {0:0.00})
            string fmt = BuildFormat(scale);
            return string.Format(fmt, value);
        }

        public static decimal ToPercent(decimal num, decimal total)
        {
            if (total == decimal.Zero)
            {
                return decimal.Zero;
            }
            decimal value = decimal.Divide(num, total);
            return decimal.Multiply(value, 100);
        }

        public static string TotalFrequencyString(List<CategoryInfo> categoryInfos)
        {
            return CategoryInfo.TotalFrequency(categoryInfos).ToString();
        }

        public static string TotalPercentString(List<CategoryInfo> categoryInfos, int scale)
        {
            return ToString(CategoryInfo.TotalPercent(categoryInfos), scale);
        }

        public static string TotalEffectivePercentString(List<CategoryInfo> categoryInfos, int scale)
        {
            return ToString(CategoryInfo.TotalEffectivePercent(categoryInfos), scale);
        }

        #endregion

        #region SummaryInfo Methods

        public static SummaryInfo CreateSummaryInfo(ICollection<object> values, NumberHelper numericHelper)
        {
            SummaryInfo summaryInfo = new SummaryInfo();
            int emptyCases = 0;
            int validCases = 0;
            int invalidCases = 0;
            List<double> validValues = new List<double>();
            foreach (object value in values)
            {
                if (IsEmptyValue(value))
                {
                    emptyCases++;
                }
                else
                {
                    double num = ToDouble(value);
                    if (numericHelper.IsMissingValue(num)) 
                    {
                        invalidCases++;
                    }
                    else
                    {
                        validCases++;
                        validValues.Add(num);
                    }
                }
            }
            summaryInfo.ValidCases = validCases;
            summaryInfo.InvalidCases = invalidCases;
            summaryInfo.EmptyCases = emptyCases;
            if (validCases > 0)
            {
                summaryInfo.Mean = validValues.Mean();
                summaryInfo.Median = validValues.Median();
                summaryInfo.StandardDeviation = validValues.StandardDeviation();
                summaryInfo.Minimum = validValues.Min();
                summaryInfo.Maximum = validValues.Max();
            }
            return summaryInfo;
        }

        #endregion

        #region CategoryInfo Methods

        public static void CalcPercent(List<CategoryInfo> categoryInfos)
        {
            int validCases = 0;
            int invalidCases = 0;
            foreach (CategoryInfo info in categoryInfos)
            {
                if (info.IsTypeNormalValue)
                {
                    validCases += info.Frequency;
                } else if (info.IsTypeMissingValue)
                {
                    invalidCases += info.Frequency;
                }
            }
            int totalCases = validCases + invalidCases;
            foreach (CategoryInfo info in categoryInfos)
            {
                decimal percent = 0;
                decimal effectivePercent = 0;
                if (info.IsTypeNormalValue)
                {
                    percent = ToPercent(info.Frequency, totalCases);
                    effectivePercent = ToPercent(info.Frequency, validCases);
                } 
                else if (info.IsTypeMissingValue)
                {
                    percent = ToPercent(info.Frequency, totalCases);
                }
                info.Percent = percent;
                info.EffectivePercent = effectivePercent;
            }
        }

        public static void CalcMultiAnswerPercent(List<CategoryInfo> categoryInfos, int validRowCount, int rowCount)
        {
            if (rowCount == 0)
            {
                return;
            }

            int totalResponse = 0;
            int invalidResponse = 0;
            foreach (CategoryInfo info in categoryInfos)
            {
                totalResponse += info.Frequency;
                if (info.IsTypeMissingValue)
                {
                    invalidResponse += info.Frequency;
                }
            }
            int validReponse = totalResponse - invalidResponse;

            foreach (CategoryInfo info in categoryInfos)
            {
                info.Percent = ToPercent(info.Frequency, totalResponse);
                info.CasePercent = ToPercent(info.Frequency, rowCount);
                if (info.IsTypeNormalValue)
                {
                    info.EffectivePercent = ToPercent(info.Frequency, validReponse);
                    info.EffectiveCasePercent = ToPercent(info.Frequency, validRowCount);
                }
            }
        }

        public static Dictionary<string, int> CreateCasesDict(ICollection<object> values)
        {
            Dictionary<string, int> casesDict = new Dictionary<string, int>();
            foreach (object value in values)
            {
                string key = ToCategoryValue(value);
                if (casesDict.ContainsKey(key))
                {
                    casesDict[key]++;
                }
                else
                {
                    casesDict[key] = 1;
                }
            }
            return casesDict;
        }

        public static CategoryInfo CreateEmptyCategoryInfo(int emptyCases)
        {
            CategoryInfo emptyCategoryInfo = new CategoryInfo();
            emptyCategoryInfo.Frequency = emptyCases;
            emptyCategoryInfo.CodeValue = null;
            emptyCategoryInfo.CategoryType = CategoryTypes.MissingValue;
            emptyCategoryInfo.CategoryTitle = EDOConstants.LABEL_EMPTY;
            return emptyCategoryInfo;
        }

        public static List<CategoryInfo> CreateSingleAnswerOrDateTimeCategoryInfos(ICollection<object> values, CategoryHelper categoryHelper)
        {
            Dictionary<string, int> casesDict = CreateCasesDict(values);

            Dictionary<string, string> sortKeyDict = new Dictionary<string, string>();
            List<string> keyList = casesDict.Keys.ToList();
            foreach (string key in keyList)
            {
                // 1 => 0000000001
                // 21=> 0000000021
                sortKeyDict[key] = EDOUtils.PadZero(key, 10);
            }

            keyList.Sort((a, b) => string.Compare(sortKeyDict[a], sortKeyDict[b]));
            List<CategoryInfo> categoryInfos = new List<CategoryInfo>();
            int emptyCases = 0;
            foreach (string key in keyList)
            {
                int cases = casesDict[key];
                if (string.IsNullOrEmpty(key)) {
                    emptyCases += cases;
                    continue;
                }
                CategoryInfo categoryInfo = categoryHelper.CreateCategoryInfo(key, cases);
                categoryInfos.Add(categoryInfo);
            }
            CategoryInfo emptyCategoryInfo = CreateEmptyCategoryInfo(emptyCases);
            categoryInfos.Add(emptyCategoryInfo);
            CalcPercent(categoryInfos);
            return categoryInfos;
        }

        public static int CountValidCases(string multipleAnswerSelectedValue, ICollection<object> values)
        {
            int emptyCases = 0;
            int validCases = 0;
            foreach (object value in values)
            {
                string key = ToCategoryValue(value);
                if (multipleAnswerSelectedValue == key)
                {
                    validCases++;
                }
                else
                {
                    emptyCases++;
                }
            }
            return validCases;
        }

        public static CategoryInfo CreateMultipleAnswerCategoryInfo(string multipleAnswerSelectedValue, ICollection<object> values, string variableName, CodeVM code)
        {
            CategoryInfo categoryInfo = new CategoryInfo();
            categoryInfo.Frequency = CountValidCases(multipleAnswerSelectedValue, values);
            categoryInfo.CategoryType = CategoryHelper.ToCategoryType(code.IsMissingValue);
            categoryInfo.VariableTitle = variableName;
            categoryInfo.CodeValue = code.Value;
            categoryInfo.CategoryTitle = code.Label;
            return categoryInfo;
        }

        public static List<CategoryInfo> CreateMultipleAnswerCategoryInfos(RawData data, QuestionVM question, List<VariableVM> variables)
        {
            List<CategoryInfo> categoryInfos = new List<CategoryInfo>();
            ObservableCollection<CodeVM> codes = question.Response.Codes;
            string multipleAnswerSelectedValue = question.MultipleAnswerSelectedValue;
            for (int i = 0; i < variables.Count; i++)
            {
                if (i >= codes.Count)
                {
                    break;
                }
                VariableVM variable = variables[i];
                CodeVM code = codes[i];
                RawVariable rawVariable = data.GetVariable(variable.Title);
                List<object> values = rawVariable != null ? data.GetColumnValues(rawVariable.Index) : new List<object>();
                CategoryInfo categoryInfo = CreateMultipleAnswerCategoryInfo(multipleAnswerSelectedValue, values, variable.Title, code);
                categoryInfos.Add(categoryInfo);
            }
            return categoryInfos;
        }

        #endregion

        #region StatisticsInfo Methods

        private static StatisticsInfo CreateSimpleStatisticsInfo(RawData data, VariableVM variable, Action<List<object>, StatisticsInfo> initializer)
        {
            RawVariable rawVariable = data.GetVariable(variable.Title);
            if (rawVariable == null)
            {
                return null;
            }
            List<object> values = data.GetColumnValues(rawVariable.Index);
            StatisticsInfo statisticsInfo = new StatisticsInfo();
            statisticsInfo.Scale = variable.Response.Scale;
            statisticsInfo.VariableId = variable.Id;
            initializer(values, statisticsInfo);
            return statisticsInfo;
        }

        private static StatisticsInfo CreateNumberStatisticsInfo(RawData data, VariableVM variable)
        {
            return CreateSimpleStatisticsInfo(data, variable, 
                (values, info) => {
                    info.StatisticsType = StatisticsTypes.Number;
                    NumberHelper numericHelper = NumberHelper.Create(variable);
                    SummaryInfo summaryInfo = CreateSummaryInfo(values, numericHelper);
                    info.SummaryInfo = summaryInfo;
                });
        }

        private static StatisticsInfo CreateDateTimeStatisticsInfo(RawData data, VariableVM variable)
        {
            return CreateSimpleStatisticsInfo(data, variable,
                (values, info) =>
                {
                    info.StatisticsType = StatisticsTypes.DateTime;
                    DateTimeHelper helper = DateTimeHelper.Create(variable.Response.MissingValues);
                    List<CategoryInfo> categoryInfos = CreateSingleAnswerOrDateTimeCategoryInfos(values, helper);
                    info.CategoryInfos = categoryInfos;
                });
        }

        private static StatisticsInfo CreateSingleAnswerStatisticsInfo(RawData data, VariableVM variable)
        {
            return CreateSimpleStatisticsInfo(data, variable,
                (values, info) =>
                {
                    info.StatisticsType = StatisticsTypes.ChoicesSingleAnswer;
                    SingleAnswerHelper helper = SingleAnswerHelper.Create(variable.Response.Codes);
                    List<CategoryInfo> categoryInfos = CreateSingleAnswerOrDateTimeCategoryInfos(values, helper);
                    info.CategoryInfos = categoryInfos;
                });
        }

        private static bool IsEmptyRecord(RawRecord record, List<int> indexes)
        {
            int validValueCount = 0;
            foreach (int index in indexes)
            {
                object value = record.GetValue(index);
                if (!IsEmptyValue(value))
                {
                    validValueCount++;
                }
            }
            return validValueCount == 0;
        }

        private static int CalcEmptyRowCount(RawData data, List<VariableVM> variables)
        {
            List<string> variableTitles = VariableVM.GetTitles(variables);
            List<int> indexes = data.GetVariableIndexes(variableTitles);
            int emptyRowCount = 0;
            foreach (RawRecord record in data.Records)
            {
                if (IsEmptyRecord(record, indexes))
                {
                    emptyRowCount++;
                }
            }
            return emptyRowCount;
        }

        private static StatisticsInfo CreateMultipleAnswerStatisticsInfo(RawData data, QuestionVM question, List<VariableVM> variables)
        {
            //for debug
            //data.Records.Add(new RawRecord());
            //data.Records.Add(new RawRecord());
            List<CategoryInfo> categoryInfos = CreateMultipleAnswerCategoryInfos(data, question, variables);
            if (categoryInfos.Count == 0)
            {
                return null;
            }
            int rowCount = data.Records.Count;
            int emptyRowCount = CalcEmptyRowCount(data, variables);
            int validRowCount = rowCount - emptyRowCount;
            CalcMultiAnswerPercent(categoryInfos, validRowCount, rowCount);

            StatisticsInfo statisticsInfo = new StatisticsInfo();
            statisticsInfo.Scale = question.Response.Scale;
            statisticsInfo.QuestionId = question.Id;
            statisticsInfo.StatisticsType = StatisticsTypes.ChoicesMultipleAnswer;
            statisticsInfo.CategoryInfos = categoryInfos;
            statisticsInfo.SummaryInfo.ValidCases = validRowCount;
            statisticsInfo.SummaryInfo.InvalidCases = emptyRowCount;
            return statisticsInfo;
        }

        public static List<StatisticsInfo> CreateStatisticsInfos(RawData data, StudyUnitVM studyUnit)
        {
            Dictionary<string, List<VariableVM>> maVariables = new Dictionary<string, List<VariableVM>>();
            List<StatisticsInfo> statisticsInfos = new List<StatisticsInfo>();
            foreach (VariableVM variable in studyUnit.Variables)
            {
                StatisticsTypes type = studyUnit.GetStatisticsType(variable);
                StatisticsInfo statisticsInfo = null;

                if (StatisticsInfo.IsTypeNumberOf(type))
                {
                    statisticsInfo = CreateNumberStatisticsInfo(data, variable);
                }
                else if (StatisticsInfo.IsTypeDateTimeOf(type))
                {
                    statisticsInfo = CreateDateTimeStatisticsInfo(data, variable);
                }
                else if (StatisticsInfo.IsTypeChoicesSingleAnswerOf(type))
                {
                    statisticsInfo = CreateSingleAnswerStatisticsInfo(data, variable);
                }
                else if (StatisticsInfo.IsTypeChoicesMultipleAnswerOf(type))
                {
                    List<VariableVM> variables = null;
                    string key = variable.QuestionId;
                    if (maVariables.ContainsKey(key))
                    {
                        variables = maVariables[key];
                    }
                    else
                    {
                        variables = new List<VariableVM>();
                        maVariables[key] = variables;
                    }
                    variables.Add(variable);
                }
                if (statisticsInfo != null)
                {
                    statisticsInfos.Add(statisticsInfo);
                }

            }
            // Multiple Answer
            foreach (KeyValuePair<string, List<VariableVM>> pair in maVariables)
            {
                string questionId = pair.Key;
                List<VariableVM> variables = pair.Value;
                QuestionVM question = studyUnit.FindQuestion(questionId);
                if (question == null) 
                {
                    continue;
                }
                StatisticsInfo statisticsInfo = CreateMultipleAnswerStatisticsInfo(data, question, variables);
                if (statisticsInfo == null)
                {
                    continue;
                }
                statisticsInfos.Add(statisticsInfo);
            }
            return statisticsInfos;
        }

        #endregion

        #region CodebookVariable Methods

        public static List<CodebookVariable> CreateCodebookVariables(StudyUnitVM studyUnit)
        {
            List<CodebookVariable> codebookVariables = new List<CodebookVariable>();
            ObservableCollection<QuestionVM> questions = studyUnit.AllQuestions; 
            foreach (VariableVM variable in studyUnit.Variables)
            {
                QuestionVM question = EDOUtils.Find<QuestionVM>(questions, variable.QuestionId);
                StatisticsInfo statisticsInfo = studyUnit.FindStatisticsInfoModel(variable);
                StatisticsTypes statisticsType = StatisticsTypes.Unknown;
                if (statisticsInfo == null)
                {
                    statisticsType = studyUnit.GetStatisticsType(variable);
                }
                else
                {
                    statisticsType = statisticsInfo.StatisticsType;
                    statisticsInfo.ApplyScale();
                }
                CodebookVariable codebookVariable = null;
                if (question != null && StatisticsInfo.IsTypeChoicesMultipleAnswerOf(statisticsType))
                {
                    codebookVariable = CodebookVariable.FindByQuestionId(codebookVariables, question.Id);
                }
                if (codebookVariable == null)
                {
                    codebookVariable = new CodebookVariable();
                    codebookVariables.Add(codebookVariable);
                }
                codebookVariable.Variables.Add(variable);
                codebookVariable.Question = question;
                codebookVariable.StatisticsInfo = statisticsInfo;
                codebookVariable.StatisticsType = statisticsType;
            }
            return codebookVariables;
        }

        public static StatisticsInfo CreateDummyChoicesMultiAnswerStatisticsInfo(CodebookVariable codebookVariable)
        {
            Debug.Assert(codebookVariable.IsTypeChoicesMultipleAnswer);
            Debug.Assert(codebookVariable.Question != null);

            QuestionVM question = codebookVariable.Question;
            ObservableCollection<CodeVM> codes = question.Response.Codes;
            List<VariableVM> variables = codebookVariable.Variables;

            List<CategoryInfo> categoryInfos = new List<CategoryInfo>();
            for (int i = 0; i < variables.Count; i++)
            {
                if (i >= codes.Count)
                {
                    break;
                }
                VariableVM variable = variables[i];
                CodeVM code = codes[i];

                CategoryInfo categoryInfo = new CategoryInfo();
                categoryInfo.VariableTitle = variable.Title;
                categoryInfo.CodeValue = code.Value;
                categoryInfo.CategoryTitle = code.Label;
                categoryInfos.Add(categoryInfo);
            }

            StatisticsInfo statisticsInfo = new StatisticsInfo();
            statisticsInfo.QuestionId = question.Id;
            statisticsInfo.StatisticsType = StatisticsTypes.ChoicesMultipleAnswer;
            statisticsInfo.CategoryInfos = categoryInfos;
            return statisticsInfo;

        }

        public static StatisticsInfo CreateDummyChoicesSingleAnswerStatisticsInfo(CodebookVariable codebookVariable)
        {
            VariableVM variable = codebookVariable.FirstVariable;
            ObservableCollection<CodeVM> codes = variable.Response.Codes;
            List<CategoryInfo> categoryInfos = new List<CategoryInfo>();
            foreach (CodeVM code in codes)
            {
                CategoryInfo categoryInfo = new CategoryInfo();
                categoryInfo.CategoryType = CategoryHelper.ToCategoryType(code.IsMissingValue);
                categoryInfo.CodeValue = code.Value;
                categoryInfo.CategoryTitle = code.Label;
                categoryInfos.Add(categoryInfo);
            }
            StatisticsInfo statisticsInfo = new StatisticsInfo();
            statisticsInfo.VariableId = variable.Id;
            statisticsInfo.StatisticsType = StatisticsTypes.ChoicesSingleAnswer;
            statisticsInfo.CategoryInfos = categoryInfos;
            return statisticsInfo;
        }

        #endregion
    }
}
