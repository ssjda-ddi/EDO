using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.Model;
using EDO.Main;
using EDO.Core.View;
using EDO.QuestionCategory.SequenceForm;
using EDO.DataCategory.DataFileForm;
using System.Windows;
using EDO.Core.Util;
using EDO.Properties;
using System.Windows.Input;
using System.IO;

namespace EDO.Core.IO
{
    public static class MiscSerializer
    {

        private static readonly string QUESTIONNAIRE_FILTER = Resources.QuestionnaireFileFilter;
        public static void ExportQuestionnaire(EDOConfig config, StudyUnitVM studyUnit)
        {
            ControlConstructSchemeVM controlConstructScheme = null;
            if (studyUnit.ControlConstructSchemes.Count > 1)
            {
                SelectObjectWindowVM<ControlConstructSchemeVM> vm = new SelectObjectWindowVM<ControlConstructSchemeVM>(studyUnit.ControlConstructSchemes);
                SelectObjectWindow window = new SelectObjectWindow(vm);
                controlConstructScheme = SelectObjectWindow.Select(Resources.SelectOrder, vm) as ControlConstructSchemeVM; //Select Sequence
            }
            else if (studyUnit.ControlConstructSchemes.Count == 1)
            {
                controlConstructScheme = studyUnit.ControlConstructSchemes[0];
            }
            if (controlConstructScheme == null)
            {
                return;
            }
            string path = IOUtils.QuerySavePathName(Resources.ExportQuestionnair + ": " + controlConstructScheme.Title , null, QUESTIONNAIRE_FILTER, true);
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            try
            {
                QuestionnaireWriter writer = new QuestionnaireWriter(config, controlConstructScheme);
                writer.Write(path);
            }
            catch (Exception ex)
            {
                EDOUtils.ShowUnexpectedError(ex);
            }
        }

        private static readonly string CODEBOOK_FILTER = Resources.CodeBookFileFilter;
        public static void ExportCodebook(StudyUnitVM studyUnit)
        {
            string path = IOUtils.QuerySavePathName(Resources.ExportCodebook, null, CODEBOOK_FILTER, true); //Export Codebook
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            try
            {
                CodebookWriter writer = new CodebookWriter(studyUnit);
                writer.Write(path);
            }
            catch (Exception ex)
            {
                EDOUtils.ShowUnexpectedError(ex);
            }
        }

        private static readonly string SPSS_FILTER = Resources.SPSSFileFilter;
        public static bool ImportSpssVariables(StudyUnitVM studyUnit)
        {            
            string path = IOUtils.QueryOpenPathName(SPSS_FILTER);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            try
            {
                SpssReader reader = new SpssReader();
                return reader.ImportVariables(path, studyUnit);
            }
            catch (Exception ex)
            {
                EDOUtils.ShowUnexpectedError(ex);
            }
            return false;
        }

        public static bool ImportSpssAll(StudyUnitVM studyUnit, MainWindowVM mainWindow)
        {
            string path = IOUtils.QueryOpenPathName(SPSS_FILTER);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            try
            {
                SpssReader reader = new SpssReader();
                if (!reader.ImportVariables(path, studyUnit))
                {
                    return false;
                }
                StudyUnit studyUnitModel = studyUnit.StudyUnitModel;
                mainWindow.RecreateViewModels();
                RawData rawData = reader.LoadRawData(path);
                if (rawData == null)
                {
                    return false;
                }

                StudyUnitVM newStudyUnit = mainWindow.GetStudyUnit(studyUnitModel);
                List<StatisticsInfo> statisticsInfos = StatisticsUtils.CreateStatisticsInfos(rawData, newStudyUnit);
                studyUnitModel.StatisticsInfos = statisticsInfos;
                mainWindow.RecreateViewModels();
                return true;
            }
            catch (Exception ex)
            {
                EDOUtils.ShowUnexpectedError(ex);
            }
            return false;
        }

        private static readonly string SETUPSYNTAX_FILTER = Resources.SetupSyntaxFileFilter;
        public static void ExportSetupSyntax(StudyUnitVM studyUnit)
        {
            DataFileVM dataFile = null;
            if (studyUnit.DataFiles.Count > 1)
            {
                SelectObjectWindowVM<DataFileVM> vm = new SelectObjectWindowVM<DataFileVM>(studyUnit.DataFiles);
                SelectObjectWindow window = new SelectObjectWindow(vm);
                dataFile = SelectObjectWindow.Select(Resources.SelectDataFile, vm) as DataFileVM; //Select Data File
            }
            else if (studyUnit.DataFiles.Count == 1)
            {
                dataFile = studyUnit.DataFiles[0];
            }
            if (dataFile == null)
            {
                return;
            }
            string path = IOUtils.QuerySavePathName(Resources.ExportSetupSyntax, null, SETUPSYNTAX_FILTER, true); //Export Setup Syntax
            if (string.IsNullOrEmpty(path))
            {
                return;
            }
            try
            {
                SetupSyntaxWriter writer = new SetupSyntaxWriter(studyUnit, dataFile);
                writer.Write(path);
            }
            catch (Exception ex)
            {
                EDOUtils.ShowUnexpectedError(ex);
            }
        }

        public static bool ImportData(StudyUnitVM studyUnit)
        {
            string path = IOUtils.QueryOpenPathName(Resources.DataFilter);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            bool result = false;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                string extension = Path.GetExtension(path);
                RawData rawData = null;
                if (extension == ".sav")
                {
                    SpssReader reader = new SpssReader();
                    rawData = reader.LoadRawData(path);
                }
                else if (extension == ".xlsx")
                {
                    ExcelReader reader = new ExcelReader();
                    rawData = reader.LoadRawData(path);
                }
                else if (extension == ".csv")
                {
                    CsvReader reader = new CsvReader();
                    rawData = reader.LoadRawData(path);
                }


                if (rawData != null)
                {
                    List<StatisticsInfo> statisticsInfos = StatisticsUtils.CreateStatisticsInfos(rawData, studyUnit);
                    studyUnit.StudyUnitModel.StatisticsInfos = statisticsInfos;
                    result = true;
                }
            }
            catch (Exception ex)
            {
                EDOUtils.ShowUnexpectedError(ex);
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
            return result;
        }
    }
}
