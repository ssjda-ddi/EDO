using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.ViewModel;
using EDO.Core.Model;
using System.Collections.ObjectModel;
using EDO.Core.Model.Statistics;
using EDO.Core.Util;
using System.Diagnostics;
using EDO.Core.Util.Statistics;
using EDO.Properties;

namespace EDO.DataCategory.StatisticsForm
{
    public class StatisticsInfoVM :BaseVM
    {
        public static StatisticsInfoVM FindByVariableId(ICollection<StatisticsInfoVM> infos, string variableId)
        {
            return infos.FirstOrDefault(x => x.VariableId == variableId);
        }

        public static StatisticsInfoVM FindByQuestionId(ICollection<StatisticsInfoVM> infos, string questionId)
        {
            return infos.FirstOrDefault(x => x.QuestionId == questionId);
        }

        public static StatisticsInfoVM FindByQuestionIdOrVariableId(ICollection<StatisticsInfoVM> infos, string questionId, string variableId)
        {
            StatisticsInfoVM statisticsInfo = FindByQuestionId(infos, questionId);
            if (statisticsInfo != null)
            {
                return statisticsInfo;
            }
            return FindByVariableId(infos, variableId);
        }

        public static StatisticsInfoRowVM CreateCategoryInfoRow(CategoryInfo categoryInfo)
        {
            StatisticsInfoRowVM row = new StatisticsInfoRowVM();
            row.Column0 = categoryInfo.CodeValue;
            row.Column1 = categoryInfo.CategoryTitle;
            row.Column2 = categoryInfo.FrequencyString;
            row.Column3 = categoryInfo.PercentString;
            row.Column4 = categoryInfo.EffectivePercentString;
            return row;
        }

        public static StatisticsInfoRowVM CreateTotalRow(CategoryInfoCollection collection, string title, string effectivePercent = EDOConstants.EMPTY_CURSOR)
        {
            StatisticsInfoRowVM row = new StatisticsInfoRowVM();
            row.Column1 = title;
            row.Column2 = collection.TotalFrequencyString;
            row.Column3 = collection.TotalPercentString;
            row.Column4 = effectivePercent;
            return row;
        }

        public static StatisticsInfoRowVM CreateMultipleAnswerCategoryInfoRow(CategoryInfo categoryInfo)
        {
            StatisticsInfoRowVM row = new StatisticsInfoRowVM();
            row.Column0 = categoryInfo.VariableTitle;
            row.Column1 = categoryInfo.CodeValue;
            row.Column2 = categoryInfo.CategoryTitle;
            row.Column3 = categoryInfo.FrequencyString;
            row.Column4 = categoryInfo.PercentString;
            row.Column5 = categoryInfo.CasePercentString;
            row.Column6 = categoryInfo.EffectivePercentString;
            row.Column7 = categoryInfo.EffectiveCasePercentString;
            return row;
        }

        public static StatisticsInfoRowVM CreateMultipleAnswerNormalValueTotalRow(CategoryInfoCollection collection)
        {
            StatisticsInfoRowVM row = new StatisticsInfoRowVM();
            row.Column2 = Resources.TotalValidValues;
            row.Column3 = collection.TotalFrequencyString;
            row.Column4 = collection.TotalPercentString;
            row.Column5 = EDOConstants.EMPTY_CURSOR;
            row.Column6 = collection.FullPercent;
            row.Column7 = EDOConstants.EMPTY_CURSOR;
            return row;
        }

        public static StatisticsInfoRowVM CreateMultipleAnswerMissingValueTotalRow(CategoryInfoCollection collection)
        {
            StatisticsInfoRowVM row = new StatisticsInfoRowVM();
            row.Column2 = Resources.TotalInvalidValues;
            row.Column3 = collection.TotalFrequencyString;
            row.Column4 = collection.TotalPercentString;
            row.Column5 = EDOConstants.EMPTY_CURSOR;
            row.Column6 = EDOConstants.EMPTY_CURSOR;
            row.Column7 = EDOConstants.EMPTY_CURSOR;
            return row;
        }

        public static StatisticsInfoRowVM CreateMultipleAnswerTotalResponsesRow(CategoryInfoCollection collection)
        {
            StatisticsInfoRowVM row = new StatisticsInfoRowVM();
            row.Column2 = Resources.TotalResponses;
            row.Column3 = collection.TotalFrequencyString;
            row.Column4 = collection.FullPercent;
            row.Column5 = EDOConstants.EMPTY_CURSOR;
            row.Column6 = EDOConstants.EMPTY_CURSOR;
            row.Column7 = EDOConstants.EMPTY_CURSOR;
            return row;
        }

        public static StatisticsInfoRowVM CreateMultipleAnswerTotalSamplesRow(string totalCase)
        {
            StatisticsInfoRowVM totalRow = new StatisticsInfoRowVM();
            totalRow.Column2 = Resources.TotalSamples;
            totalRow.Column3 = totalCase;
            totalRow.Column4 = EDOConstants.EMPTY_CURSOR;
            totalRow.Column5 = EDOConstants.EMPTY_CURSOR;
            totalRow.Column6 = EDOConstants.EMPTY_CURSOR;
            totalRow.Column7 = EDOConstants.EMPTY_CURSOR;
            return totalRow;
        }

        public static StatisticsInfoRowVM CreateNumberRow(string label, string value)
        {
            StatisticsInfoRowVM row = new StatisticsInfoRowVM();
            row.Column0 = label;
            row.Column1 = value;
            return row;
        }

        public static ObservableCollection<StatisticsInfoRowVM> CreateNumberRows(StatisticsInfo statisticsInfo)
        {
            ObservableCollection<StatisticsInfoRowVM> rows = new ObservableCollection<StatisticsInfoRowVM>();
            SummaryInfo summaryInfo = statisticsInfo.SummaryInfo;
            rows.Add(CreateNumberRow(Resources.ValidFrequency, summaryInfo.ValidCasesString));
            rows.Add(CreateNumberRow(Resources.InvalidFrequency, summaryInfo.InvalidCasesString));
            rows.Add(CreateNumberRow(Resources.SampleFrequency, summaryInfo.TotalCasesString));
            rows.Add(CreateNumberRow(Resources.Mean, summaryInfo.MeanString));
            rows.Add(CreateNumberRow(Resources.Median, summaryInfo.MedianString));
            rows.Add(CreateNumberRow(Resources.StandardDeviation, summaryInfo.StandardDeviationString));
            rows.Add(CreateNumberRow(Resources.Minimum, summaryInfo.MinimumString));
            rows.Add(CreateNumberRow(Resources.Maximum, summaryInfo.MaximumString));
            return rows;
        }

        public ObservableCollection<StatisticsInfoRowVM> CreateSingleAnswerOrDateTimeRows(StatisticsInfo statisticsInfo)
        {
            ObservableCollection<StatisticsInfoRowVM> rows = new ObservableCollection<StatisticsInfoRowVM>();

            CategoryInfoCollection[] collections = CategoryInfoCollection.Create(statisticsInfo);

            // normal values
            CategoryInfoCollection normalCollection = collections[1];
            foreach (CategoryInfo categoryInfo in normalCollection)
            {
                StatisticsInfoRowVM row = CreateCategoryInfoRow(categoryInfo);
                rows.Add(row);
            }
            // normal values subtotal
            StatisticsInfoRowVM subTotalRow = CreateTotalRow(normalCollection, Resources.TotalValidValues, normalCollection.FullPercent);
            rows.Add(subTotalRow);

            // missing values
            CategoryInfoCollection missingCollection = collections[2];
            foreach (CategoryInfo categoryInfo in missingCollection)
            {
                StatisticsInfoRowVM row = CreateCategoryInfoRow(categoryInfo);
                rows.Add(row);
            }
            // missing values subtotal
            subTotalRow = CreateTotalRow(missingCollection, Resources.TotalInvalidValues);
            rows.Add(subTotalRow);

            // total
            CategoryInfoCollection totalCollection = collections[0];
            StatisticsInfoRowVM totalRow = CreateTotalRow(totalCollection, Resources.TotalAll);
            rows.Add(totalRow);
            return rows;
        }

        public static ObservableCollection<StatisticsInfoRowVM> CreateMultipleAnswerRows(StatisticsInfo statisticsInfo)
        {
            ObservableCollection<StatisticsInfoRowVM> rows = new ObservableCollection<StatisticsInfoRowVM>();

            CategoryInfoCollection[] collections = CategoryInfoCollection.Create(statisticsInfo);

            // normal values
            CategoryInfoCollection normalCollection = collections[1];
            foreach (CategoryInfo normalInfo in normalCollection)
            {
                StatisticsInfoRowVM row = CreateMultipleAnswerCategoryInfoRow(normalInfo);
                rows.Add(row);
            }
            // normal values subtotal
            StatisticsInfoRowVM subTotalRow = CreateMultipleAnswerNormalValueTotalRow(normalCollection);
            rows.Add(subTotalRow);

            // missing values
            CategoryInfoCollection missingCollection = collections[2];
            foreach (CategoryInfo missingInfo in missingCollection)
            {
                StatisticsInfoRowVM row = CreateMultipleAnswerCategoryInfoRow(missingInfo);
                rows.Add(row);
            }
            // missing values subtotal
            subTotalRow = CreateMultipleAnswerMissingValueTotalRow(missingCollection);
            rows.Add(subTotalRow);

            // total responses row
            CategoryInfoCollection totalCollection  = collections[0];
            StatisticsInfoRowVM totalResponseRow = CreateMultipleAnswerTotalResponsesRow(totalCollection);
            rows.Add(totalResponseRow);

            // total samples row
            StatisticsInfoRowVM totalSamplesRow = CreateMultipleAnswerTotalSamplesRow(statisticsInfo.SummaryInfo.TotalCasesString);
            rows.Add(totalSamplesRow);
            return rows;
        }

        public StatisticsInfoVM(StatisticsInfo statisticsInfo)
        {
            this.statisticsInfo = statisticsInfo;
            statisticsInfo.ApplyScale();
            this.rows = new ObservableCollection<StatisticsInfoRowVM>();
            if (statisticsInfo.IsTypeNumber)
            {
                rows = CreateNumberRows(statisticsInfo);
            }
            else if (statisticsInfo.IsTypeChoicesSingleAnswer || statisticsInfo.IsTypeDateTime)
            {
                rows = CreateSingleAnswerOrDateTimeRows(statisticsInfo);
            }
            else if (statisticsInfo.IsTypeChoicesMultipleAnswer)
            {
                rows = CreateMultipleAnswerRows(statisticsInfo);
            }
        }

        private StatisticsInfo statisticsInfo;

        public StatisticsInfo StatisticsInfo { get { return statisticsInfo; } }

        public override object Model { get { return statisticsInfo; } }

        public string VariableId { get { return statisticsInfo.VariableId; } }

        public string QuestionId { get { return statisticsInfo.QuestionId; } }

        public StatisticsTypes StatisticsType { get { return statisticsInfo.StatisticsType; } }

        private ObservableCollection<StatisticsInfoRowVM> rows;
        public ObservableCollection<StatisticsInfoRowVM> Rows { get { return rows; } }          
    }
}
