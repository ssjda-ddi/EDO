using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Main;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Collections.ObjectModel;
using System.Windows;
using EDO.StudyCategory.MemberForm;
using EDO.StudyCategory.FundingInfoForm;
using EDO.SamplingCategory.SamplingForm;
using EDO.Core.Model;
using EDO.VariableCategory.VariableForm;
using EDO.QuestionCategory.QuestionForm;
using EDO.Core.Util;
using EDO.QuestionCategory.CodeForm;
using EDO.Properties;
using EDO.Core.Model.Statistics;
using System.Diagnostics;
using EDO.Core.Util.Statistics;

namespace EDO.Core.IO
{
    public class ValueConverter
    {
        public bool HasData { get; set; }
        public string ToString(string value) {
            if (!HasData)
            {
                return string.Empty;
            }
            return value;
        }
    }

    public class CodebookWriter :DocxWriter
    {

        private StudyUnitVM studyUnit;
        public CodebookWriter(StudyUnitVM studyUnit)
        {
            this.studyUnit = studyUnit;
        }

        private DocxParam CreateTitleParam()
        {
            DocxParam param = new DocxParam()
            {
                Justification = new Justification() { Val = JustificationValues.Center },
                FontSize = "32",
                RunFonts = new RunFonts() { Hint = FontTypeHintValues.EastAsia, AsciiTheme = ThemeFontValues.MajorEastAsia, HighAnsiTheme = ThemeFontValues.MajorEastAsia, EastAsiaTheme = ThemeFontValues.MajorEastAsia }
            };
            return param;
        }

        private Paragraph CreateEmptyTitleParagraph()
        {
            DocxParam param = CreateTitleParam();
            param.Justification = new Justification()
            {
                Val = JustificationValues.Left
            };
            return CreateParagraph("", param);
        }

        private Paragraph CreateTitleParagraph(string title)
        {            
            return CreateParagraph(title, CreateTitleParam());
        }

        private void WriteCover(Body body)
        {
            for (int i = 0; i < 6; i++)
            {
                Paragraph paragraph = CreateEmptyTitleParagraph();
                body.Append(paragraph);

            }
            DocxParam param = CreateTitleParam();
            Paragraph title1 = CreateParagraph(studyUnit.AbstractForm.Title, param);
            body.Append(title1);
            Paragraph title2 = CreateParagraph(Resources.Codebook, param);//Codebook
            body.Append(title2);
            Paragraph breakPage = CreateBreakPageParagraph();
            body.Append(breakPage);
        }

        private void WriteTableOfContents(Body body)
        {
            Paragraph p = CreateParagraph(Resources.Toc); //Index
            body.Append(p);

            p = CreateParagraph("1. " + Resources.StudyAbstract); //Abstract
            body.Append(p);

            DocxParam param = new DocxParam()
            {
                Indentation = new Indentation() { Left = "420", LeftChars = 200 }
            };
            p = CreateParagraph("1.1. " + Resources.StudyMember, param); //Study Member
            body.Append(p);
            p = CreateParagraph("1.2. " + Resources.StudyPurpose, param); //Purpose
            body.Append(p);
            p = CreateParagraph("1.3. " + Resources.StudyAbstract, param); //Abstract
            body.Append(p);
            p = CreateParagraph("1.4. " + Resources.StudyFund, param); //Funding
            body.Append(p);
            p = CreateParagraph("");
            body.Append(p);
            p = CreateParagraph("2. " + Resources.DataCollectionMethod); //Data Collection
            body.Append(p);
            p = CreateParagraph("2.1. " + Resources.Universe, param); //Universe
            body.Append(p);
            p = CreateParagraph("2.2. " + Resources.SamplingMethod, param); //Sampling Procedure
            body.Append(p);
            p = CreateParagraph("2.3. " + Resources.CollectionPeriod, param); //Data Collection Date/Period
            body.Append(p);
            p = CreateParagraph("2.4. " + Resources.CollectionMethod, param); //Mode of Data Collection
            body.Append(p);
            p = CreateParagraph("");
            body.Append(p);
            p = CreateParagraph("3. " + Resources.VariableSummary); //Variable Summary
            body.Append(p);

            DocxParam param2 = new DocxParam()
            {
                Break = new Break() { Type = BreakValues.Page }
            };
            p = CreateBreakPageParagraph();
            body.Append(p);
        }

        private void WriteAbstract(Body body)
        {
            Paragraph p = CreateMidashi1Paragraph("1. " + Resources.StudyAbstract); //Abstract
            body.Append(p);

            p = CreateMidashi2Paragraph("1.1. " + Resources.StudyMember); //Study Member
            body.Append(p);
           
            foreach (MemberVM member in studyUnit.Members)
            {
                string text = member.LastName + member.FirstName + " " + member.OrganizationName + " " + member.Position;
                p = CreateParagraph(text);
                body.Append(p);
            }
            p = CreateEmptyParagraph();
            body.Append(p);

            p = CreateMidashi2Paragraph("1.2. " + Resources.StudyPurpose); //Purpose
            body.Append(p);
            p = CreateParagraph(studyUnit.AbstractForm.Purpose);
            body.Append(p);
            p = CreateEmptyParagraph();
            body.Append(p);


            p = CreateMidashi2Paragraph("1.3. " + Resources.StudyAbstract); //Abstract
            body.Append(p);
            p = CreateParagraph(studyUnit.AbstractForm.Summary);
            body.Append(p);
            p = CreateEmptyParagraph();
            body.Append(p);

            p = CreateMidashi2Paragraph("1.4. " + Resources.StudyFund); //Funding
            body.Append(p);
            foreach (FundingInfoVM fundingInfo in studyUnit.FundingInfos)
            {
                if (!string.IsNullOrEmpty(fundingInfo.Number))
                {
                    string text = fundingInfo.Title + " (" + fundingInfo.OrganizationName + ", " + Resources.SubjectNumber + ":" + fundingInfo.Number + ")"; //Grant Number
                    p = CreateParagraph(text);
                    body.Append(p);
                }
            }
            p = CreateEmptyParagraph();
            body.Append(p);

            p = CreateBreakPageParagraph();
            body.Append(p);
        }

        private void WriteSamplingMethod(Body body)
        {
            Paragraph p = CreateMidashi1Paragraph("2. " + Resources.CollectionMethodOfData); //Data Collection
            body.Append(p);

            int samplingIndex = 1;
            foreach (Sampling sampling in studyUnit.SamplingModels)
            {
                String prefix = "2." + samplingIndex + ".";
                p = CreateMidashi2Paragraph(prefix + " " + Resources.Universe + samplingIndex); //Universe
                samplingIndex++;
                body.Append(p);
                foreach (UniverseVM universe in studyUnit.Universes)
                {
                    p = CreateParagraph(universe.Memo);
                    body.Append(p);
                }
                p = CreateEmptyParagraph();
                body.Append(p);

                p = CreateMidashi2Paragraph(prefix + "1. " + Resources.SamplingMethod); //Sampling Procedure
                body.Append(p);
                foreach (UniverseVM universe in studyUnit.Universes)
                {
                    p = CreateParagraph(universe.Method);
                    body.Append(p);
                }
                p = CreateEmptyParagraph();
                body.Append(p);

                p = CreateMidashi2Paragraph(prefix + "2. " + Resources.CollectionPeriod); //Data Collection Date/Period
                body.Append(p);
                DateRange range = sampling.DateRange;
                if (range != null)
                {
                    p = CreateParagraph(range.ToStringJa());
                    body.Append(p);
                }
                p = CreateEmptyParagraph();
                body.Append(p);

                p = CreateMidashi2Paragraph(prefix + "3. " + Resources.CollectionMethod); //Mode of Data Collection
                body.Append(p);
                p = CreateParagraph(sampling.MethodName);
                body.Append(p);
                p = CreateBreakPageParagraph();
                body.Append(p);
            }


            samplingIndex++;

        }

        private Paragraph CreateCodeParagraph(CodeVM code)
        {
            Paragraph paragraph1 = new Paragraph();

            ParagraphProperties paragraphProperties1 = new ParagraphProperties();
            Indentation indentation1 = new Indentation(){ FirstLine = "210", FirstLineChars = 100 };

            paragraphProperties1.Append(indentation1);

            Run run1 = new Run();

            RunProperties runProperties1 = new RunProperties();
            RunFonts runFonts1 = new RunFonts(){ Hint = FontTypeHintValues.EastAsia };

            runProperties1.Append(runFonts1);
            Text text1 = new Text();
            text1.Text = code.Value;
            run1.Append(runProperties1);
            run1.Append(text1);

            Run run2 = new Run();

            RunProperties runProperties2 = new RunProperties();
            RunFonts runFonts2 = new RunFonts(){ Hint = FontTypeHintValues.EastAsia };

            runProperties2.Append(runFonts2);
            TabChar tabChar1 = new TabChar();

            run2.Append(runProperties2);
            run2.Append(tabChar1);

            Run run3 = new Run();

            RunProperties runProperties3 = new RunProperties();
            RunFonts runFonts3 = new RunFonts(){ Hint = FontTypeHintValues.EastAsia };

            runProperties3.Append(runFonts3);
            Text text2 = new Text();
            text2.Text = code.Label;

            run3.Append(runProperties3);
            run3.Append(text2);

            paragraph1.Append(paragraphProperties1);
            paragraph1.Append(run1);
            paragraph1.Append(run2);
            paragraph1.Append(run3);
            return paragraph1;
        }

        private void WriteQuestionInfo(Body body, QuestionVM question)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(Resources.CorrespondingQuestionSentence); //対応する質問文
            if (question != null)
            {
                buf.Append(" " + question.Text);
            }

            //質問文の段落
            Paragraph p = CreateParagraph(buf.ToString());
            body.Append(p);
        }

        private void WriteVariableInfo(Body body, VariableVM variable)
        {
            StringBuilder buf = new StringBuilder();
            buf.Append(variable.Title);
            buf.Append(" ");
            buf.Append(variable.Label);
            buf.Append(" (");
            buf.Append(variable.Response.TypeName);
            buf.Append(" )");
            Paragraph p = CreateParagraph(buf.ToString());
            body.Append(p);
        }

        private Table CreateTable()
        {
            Table table = new Table();

            TableProperties tableProperties = new TableProperties();
            DocumentFormat.OpenXml.Wordprocessing.TableStyle tableStyle = new DocumentFormat.OpenXml.Wordprocessing.TableStyle() { Val = "TableGrid" };
            TableWidth tableWidth = new TableWidth() { Width = "0", Type = TableWidthUnitValues.Auto };
            TableLook tableLook = new TableLook()
            {
                Val = "04A0",
                FirstRow = true,
                LastRow = false,
                FirstColumn = true,
                LastColumn = false,
                NoHorizontalBand = false,
                NoVerticalBand = true
            };
            tableProperties.Append(tableStyle);
            tableProperties.Append(tableWidth);
            tableProperties.Append(tableLook);
            table.Append(tableProperties);
            return table;
        }

        private DocxTableRowParam CreateMultiAnswerCategoryInfoRow(CategoryInfo categoryInfo, ValueConverter converter, bool writeEffectivePercent)
        {
            DocxTableRowParam rowParam = new DocxTableRowParam();
            rowParam.Add(categoryInfo.VariableTitle);
            rowParam.Add(categoryInfo.CodeValue);
            rowParam.Add(categoryInfo.CategoryTitle);
            rowParam.AddRight(converter.ToString(categoryInfo.FrequencyString));
            rowParam.AddRight(converter.ToString(categoryInfo.PercentString));
            rowParam.AddRight(converter.ToString(categoryInfo.CasePercentString));
            if (writeEffectivePercent)
            {
                rowParam.AddRight(converter.ToString(categoryInfo.EffectivePercentString));
                rowParam.AddRight(converter.ToString(categoryInfo.EffectiveCasePercentString));
            }
            else
            {
                rowParam.AddRight(converter.ToString(EDOConstants.EMPTY_CURSOR));
                rowParam.AddRight(converter.ToString(EDOConstants.EMPTY_CURSOR));
            }
            return rowParam;
        }

        private DocxTableRowParam CreateMultiAnswerTotalRow(CategoryInfoCollection collection, ValueConverter converter, string title, string percent, string effectivePercent)
        {
            DocxTableRowParam rowParam = new DocxTableRowParam();
            rowParam.Add();
            rowParam.Add();
            rowParam.Add(title);
            rowParam.AddRight(converter.ToString(collection.TotalFrequencyString));
            rowParam.AddRight(converter.ToString(percent));
            rowParam.AddRight(converter.ToString(EDOConstants.EMPTY_CURSOR));
            rowParam.AddRight(converter.ToString(effectivePercent));
            rowParam.AddRight(converter.ToString(EDOConstants.EMPTY_CURSOR));
            return rowParam;
        }

        private void WriteChoicesMultiAnswerVariable(Body body, CodebookVariable codebookVariable)
        {
            WriteQuestionInfo(body, codebookVariable.Question);


            Paragraph p = CreateEmptyParagraph();
            body.Append(p);

            Table table = CreateTable();
            body.Append(table);

            DocxTableRowParam rowParam = new DocxTableRowParam();
            rowParam.Add(Resources.VariableName);
            rowParam.Add(Resources.CodeNumber);
            rowParam.Add(Resources.BranchSentence);
            rowParam.Add(Resources.Frequency);
            rowParam.Add(Resources.Percent);
            rowParam.Add(Resources.CasePercent);
            rowParam.Add(Resources.EffectivePercent);
            rowParam.Add(Resources.EffectiveCasePercent);
            table.Append(rowParam.CreateTableRow());


            StatisticsInfo statisticsInfo = codebookVariable.StatisticsInfo;
            if (statisticsInfo == null) 
            {
                statisticsInfo = StatisticsUtils.CreateDummyChoicesMultiAnswerStatisticsInfo(codebookVariable);
            }

            CategoryInfoCollection[] collections = CategoryInfoCollection.Create(statisticsInfo);
            ValueConverter converter = new ValueConverter() {HasData = codebookVariable.HasData };
            // normal values
            CategoryInfoCollection normalCollection = collections[1];
            foreach (CategoryInfo normalInfo in normalCollection)
            {
                rowParam = CreateMultiAnswerCategoryInfoRow(normalInfo, converter, true);
                table.Append(rowParam.CreateTableRow());
            }
            // normal values subtotal
            rowParam = CreateMultiAnswerTotalRow(normalCollection, converter, Resources.TotalValidValues, normalCollection.TotalPercentString, normalCollection.FullPercent);
            table.Append(rowParam.CreateTableRow());

            // missing values
            CategoryInfoCollection missingCollection = collections[2];
            foreach (CategoryInfo missingInfo in missingCollection)
            {
                rowParam = CreateMultiAnswerCategoryInfoRow(missingInfo, converter, false);
                table.Append(rowParam.CreateTableRow());
            }
            // missing values subtotal
            rowParam = CreateMultiAnswerTotalRow(missingCollection, converter, Resources.TotalInvalidValues, normalCollection.TotalPercentString, EDOConstants.EMPTY_CURSOR);
            table.Append(rowParam.CreateTableRow());

            CategoryInfoCollection totalCollection = collections[0];
            // total responses
            rowParam = CreateMultiAnswerTotalRow(totalCollection, converter, Resources.TotalResponses, totalCollection.FullPercent, EDOConstants.EMPTY_CURSOR);
            table.Append(rowParam.CreateTableRow());

            // total samples
            rowParam = CreateMultiAnswerTotalRow(totalCollection, converter, Resources.TotalSamples, EDOConstants.EMPTY_CURSOR, EDOConstants.EMPTY_CURSOR);
            table.Append(rowParam.CreateTableRow());

        }

        private DocxTableRowParam CreateNormalCategoryInfoRow(CategoryInfo categoryInfo, ValueConverter converter, bool writeCode = true)
        {
            DocxTableRowParam rowParam = new DocxTableRowParam();
            if (writeCode)
            {
                rowParam.AddRight(categoryInfo.CodeValue);
            }
            rowParam.Add(categoryInfo.CategoryTitle);
            rowParam.AddRight(converter.ToString(categoryInfo.FrequencyString));
            rowParam.AddRight(converter.ToString(categoryInfo.PercentString));
            rowParam.AddRight(converter.ToString(categoryInfo.EffectivePercentString));
            return rowParam;
        }

        private DocxTableRowParam CreateTotalRow(CategoryInfoCollection collection, ValueConverter converter, string title, string effectivePercent = EDOConstants.EMPTY_CURSOR, bool writeCode = true)
        {
            DocxTableRowParam rowParam = new DocxTableRowParam();
            if (writeCode)
            {
                rowParam.Add();
            }
            rowParam.Add(title);
            rowParam.AddRight(converter.ToString(collection.TotalFrequencyString));
            rowParam.AddRight(converter.ToString(collection.TotalPercentString));
            rowParam.AddRight(converter.ToString(effectivePercent));
            return rowParam;
        }

        private DocxTableRowParam CreateMissingOrEmptyCategoryInfoRow(CategoryInfo categoryInfo, ValueConverter converter, bool writeCode = true)
        {
            DocxTableRowParam rowParam = new DocxTableRowParam();
            if (writeCode)
            {
                rowParam.AddRight(categoryInfo.CodeValue);
            }
            rowParam.Add(categoryInfo.CategoryTitle);
            rowParam.AddRight(converter.ToString(categoryInfo.FrequencyString));
            rowParam.AddRight(converter.ToString(categoryInfo.PercentString));
            rowParam.AddRight(EDOConstants.EMPTY_CURSOR);
            return rowParam;
        }

        private void WriteChoicesSingleAnswerVariable(Body body, CodebookVariable codebookVariable, bool writeCode = true)
        {
            WriteVariableInfo(body, codebookVariable.FirstVariable);
            WriteQuestionInfo(body, codebookVariable.Question);

            Paragraph p = CreateEmptyParagraph();
            body.Append(p);

            Table table = CreateTable();
            body.Append(table);

            DocxTableRowParam rowParam = new DocxTableRowParam();
            if (writeCode)
            {
                rowParam.Add(Resources.CodeNumber);
            }
            rowParam.Add(Resources.BranchSentence);
            rowParam.Add(Resources.Frequency);
            rowParam.Add(Resources.TotalPercent);
            rowParam.Add(Resources.TotalEffectivePercent);
            table.Append(rowParam.CreateTableRow());

            StatisticsInfo statisticsInfo = codebookVariable.StatisticsInfo;
            if (statisticsInfo == null)
            {
                statisticsInfo = StatisticsUtils.CreateDummyChoicesSingleAnswerStatisticsInfo(codebookVariable);
            }

            ValueConverter converter = new ValueConverter() {HasData = codebookVariable.HasData };
            CategoryInfoCollection[] collections = CategoryInfoCollection.Create(statisticsInfo);

            // normal values
            CategoryInfoCollection normalCollection = collections[1];
            foreach (CategoryInfo normalInfo in normalCollection)
            {
                rowParam = CreateNormalCategoryInfoRow(normalInfo, converter, writeCode);
                table.Append(rowParam.CreateTableRow());
            }
            // normal values subtotal
            rowParam = CreateTotalRow(normalCollection, converter, Resources.TotalValidValues, normalCollection.FullPercent, writeCode);
            table.Append(rowParam.CreateTableRow());

            // missing values
            CategoryInfoCollection missingCollection = collections[2];
            foreach (CategoryInfo missingInfo in missingCollection)
            {
                rowParam = CreateMissingOrEmptyCategoryInfoRow(missingInfo, converter, writeCode);
                table.Append(rowParam.CreateTableRow());
            }

            // missing values subtotal
            rowParam = CreateTotalRow(missingCollection, converter, Resources.TotalInvalidValues, EDOConstants.EMPTY_CURSOR, writeCode);
            table.Append(rowParam.CreateTableRow());

            // total
            CategoryInfoCollection totalCollection = collections[0];
            rowParam = CreateTotalRow(totalCollection, converter, Resources.TotalAll, EDOConstants.EMPTY_CURSOR, writeCode);
            table.Append(rowParam.CreateTableRow());
        }

        private void WriteNumberVariable(Body body, CodebookVariable codebookVariable)
        {
            WriteVariableInfo(body, codebookVariable.FirstVariable);
            WriteQuestionInfo(body, codebookVariable.Question);

            Paragraph p = CreateEmptyParagraph();
            body.Append(p);

            Table table = CreateTable();
            body.Append(table);

            DocxTableRowParam rowParam = new DocxTableRowParam();
            rowParam.Add(Resources.Item);
            rowParam.Add(Resources.Value);
            table.Append(rowParam.CreateTableRow());

            rowParam = new DocxTableRowParam();
            rowParam.Add(Resources.ValidFrequency);
            rowParam.AddRight(codebookVariable.ValidCasesString);
            table.Append(rowParam.CreateTableRow());

            rowParam = new DocxTableRowParam();
            rowParam.Add(Resources.InvalidFrequency);
            rowParam.AddRight(codebookVariable.InvalidCasesString);
            table.Append(rowParam.CreateTableRow());

            rowParam = new DocxTableRowParam();
            rowParam.Add(Resources.SampleFrequency);
            rowParam.AddRight(codebookVariable.TotalCasesString);
            table.Append(rowParam.CreateTableRow());

            rowParam = new DocxTableRowParam();
            rowParam.Add(Resources.Mean);
            rowParam.AddRight(codebookVariable.MeanString);
            table.Append(rowParam.CreateTableRow());

            rowParam = new DocxTableRowParam();
            rowParam.Add(Resources.Median);
            rowParam.AddRight(codebookVariable.MedianString);
            table.Append(rowParam.CreateTableRow());

            rowParam = new DocxTableRowParam();
            rowParam.Add(Resources.StandardDeviation);
            rowParam.AddRight(codebookVariable.StandardDeviationString);
            table.Append(rowParam.CreateTableRow());

            rowParam = new DocxTableRowParam();
            rowParam.Add(Resources.Minimum);
            rowParam.AddRight(codebookVariable.MinimumString);
            table.Append(rowParam.CreateTableRow());

            rowParam = new DocxTableRowParam();
            rowParam.Add(Resources.Maximum);
            rowParam.AddRight(codebookVariable.MaximumString);
            table.Append(rowParam.CreateTableRow());

        }

        private void WriteVariables(Body body)
        {
            Paragraph p = CreateMidashi1Paragraph("3. " + Resources.VariableSummary); //変数の概要
            body.Append(p);

            List<CodebookVariable> codebookVariables = StatisticsUtils.CreateCodebookVariables(studyUnit);
            foreach (CodebookVariable codebookVariable in codebookVariables)
            {
                if (codebookVariable.IsTypeChoicesMultipleAnswer)
                {
                    WriteChoicesMultiAnswerVariable(body, codebookVariable);
                }
                else if (codebookVariable.IsTypeChoicesSingleAnswer)
                {
                    WriteChoicesSingleAnswerVariable(body, codebookVariable);
                } else if (codebookVariable.IsTypeDateTime)
                {
                    WriteChoicesSingleAnswerVariable(body, codebookVariable, false);
                }
                else if (codebookVariable.IsTypeNumber)
                {
                    WriteNumberVariable(body, codebookVariable);
                }
                else
                {
                    WriteVariableInfo(body, codebookVariable.FirstVariable);
                    WriteQuestionInfo(body, codebookVariable.Question);
                }

                //Empty Paragraph
                p = CreateEmptyParagraph();
                body.Append(p);

            }

            p = CreateBreakPageParagraph();
            body.Append(p);
        }

        protected override void WriteBody(Body body)
        {
            //Output cover page
            WriteCover(body);

            //Output Index
            WriteTableOfContents(body);

            //1. Abstract
            WriteAbstract(body);

            //2. Data Collection
            WriteSamplingMethod(body);

            //3. Variable
            WriteVariables(body);
        }


        public string Resoures { get; set; }

        public string Resouces { get; set; }
    }
}
