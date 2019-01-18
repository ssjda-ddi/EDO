using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.Model;
using System.Diagnostics;
using System.IO;
using SpssLib.FileParser;
using EDO.Main;
using EDO.Core.Util;
using EDO.Properties;
using SpssLib.SpssDataset;

namespace EDO.Core.IO
{
    using Variable = EDO.Core.Model.Variable;
    using SpssVariable = SpssLib.SpssDataset.Variable;
    using EDO.VariableCategory.VariableForm;
    using EDO.Core.Model.Statistics;
    using EDO.QuestionCategory.CodeForm;
    using EDO.QuestionCategory.QuestionForm;

    public class SpssReader
    {
        static bool ExistValue(CodeScheme codeScheme, string missingValue)
        {
            //Currently compare as double text context(must have compared by ε)
            string missingValueStr = missingValue.ToString();
            foreach (Code existCode in codeScheme.Codes)
            {
                if (existCode.Value == missingValueStr)
                {
                    return true;
                }
            }
            return false;
        }

        public bool ImportVariables(string path, StudyUnitVM studyUnit)
        {
            Debug.Assert(!string.IsNullOrEmpty(path));
            StudyUnit studyUnitModel = studyUnit.StudyUnitModel;
            FileStream stream = File.OpenRead(path);
            SavFileParser parser = new SavFileParser(stream);
            List<Variable> variables = new List<Variable>();
            foreach (SpssLib.SpssDataset.Variable v in parser.Variables)
            {
                Variable variable = new Variable();
                variable.Title = v.Name;
                variable.Label = v.Label;
                variables.Add(variable);
                if (v.Type == SpssLib.SpssDataset.DataType.Text)
                {
                    variable.Response.TypeCode = Options.RESPONSE_TYPE_FREE_CODE;
                }
                else
                {
                    if (v.ValueLabels.Count > 0)
                    {
                        CategoryScheme categoryScheme = new CategoryScheme();
                        categoryScheme.Title = v.Label;
                        studyUnitModel.CategorySchemes.Add(categoryScheme);
                        CodeScheme codeScheme = new CodeScheme();
                        codeScheme.Title = v.Label; ;
                        studyUnitModel.CodeSchemes.Add(codeScheme);

                        variable.Response.TypeCode = Options.RESPONSE_TYPE_CHOICES_CODE;
                        variable.Response.CodeSchemeId = codeScheme.Id;
                        // Create Category
                        foreach (KeyValuePair<double, string> pair in v.ValueLabels)
                        {
                            Category category = new Category();
                            categoryScheme.Categories.Add(category);
                            category.Title = pair.Value;
                            category.CategorySchemeId = categoryScheme.Id;

                            Code code = new Code();
                            codeScheme.Codes.Add(code);
                            code.CategoryId = category.Id;
                            code.CodeSchemeId = codeScheme.Id;
                            code.Value = pair.Key.ToString();
                        }
                        // Add Missing value
                        if (v.MissingValues.Count > 0)
                        {
                            foreach (double missingValue in v.MissingValues)
                            {
                                string missingValueStr = missingValue.ToString();
                                if (ExistValue(codeScheme, missingValueStr))
                                {
                                    continue;
                                }
                                Category category = new Category();
                                categoryScheme.Categories.Add(category);
                                category.Title = Resources.MissingValue; //Missing value
                                category.IsMissingValue = true;
                                category.CategorySchemeId = categoryScheme.Id;

                                Code code = new Code();
                                codeScheme.Codes.Add(code);
                                code.CategoryId = category.Id;
                                code.CodeSchemeId = codeScheme.Id;
                                code.Value = missingValueStr;
                            }
                        }
                    }
                    else
                    {
                        variable.Response.TypeCode = Options.RESPONSE_TYPE_NUMBER_CODE;
                    }
                }
            }

            if (variables.Count > 0)
            {
                ConceptScheme conceptScheme = new ConceptScheme();
                conceptScheme.Title = EDOUtils.UniqueLabel(ConceptScheme.GetTitles(studyUnitModel.ConceptSchemes), Resources.ImportVariable); //Imported Variable
                string name = Path.GetFileName(path);
                conceptScheme.Memo = EDOUtils.UniqueLabel(ConceptScheme.GetMemos(studyUnitModel.ConceptSchemes), string.Format(Resources.ImportVariableFrom, name)); //Imported variable from {0}

                Concept concept = new Concept();
                concept.Title = EDOUtils.UniqueLabel(ConceptScheme.GetConceptTitles(studyUnitModel.ConceptSchemes), Resources.ImportVariable);//Imported Variable
                concept.Content = EDOUtils.UniqueLabel(ConceptScheme.GetConceptContents(studyUnitModel.ConceptSchemes), string.Format(Resources.ImportVariableFrom, name));//Imported variable from {0}
                conceptScheme.Concepts.Add(concept);
                studyUnitModel.ConceptSchemes.Add(conceptScheme);

                foreach (Variable variable in variables)
                {
                    Question question = new Question();
                    question.Title = variable.Label;
                    question.ConceptId = concept.Id;
                    question.Response.TypeCode = variable.Response.TypeCode;
                    question.Response.CodeSchemeId = variable.Response.CodeSchemeId;
                    question.VariableGenerationInfo = question.CreateVariableGenerationInfo();
                    studyUnitModel.Questions.Add(question);
                    variable.ConceptId = concept.Id;
                    variable.QuestionId = question.Id;
                    studyUnitModel.Variables.Add(variable);

                }
            }
            return true;
        }

        //public bool ImportRecords(string path, StudyUnitVM studyUnit)
        //{
        //    Debug.Assert(!string.IsNullOrEmpty(path));
        //    FileStream stream = File.OpenRead(path);
        //    SavFileParser parser = new SavFileParser(stream);

        //    SpssData spssData = new SpssData(parser);

        //    ICollection<SpssVariable> variables = spssData.Variables;
        //    foreach (SpssVariable variable in variables)
        //    {
        //        Debug.WriteLine(variable.Index + " " + variable.Name);
        //    }

        //    ICollection<SpssRecord> records = spssData.Records;
        //    foreach (SpssRecord record in records)
        //    {
        //        Debug.WriteLine(record.ToString());
        //    }
        //    try
        //    {
        //        List<StatisticsInfo> statisticsInfos = CreateStatisticsInfos(spssData, studyUnit);
        //        StudyUnit studyUnitModel = studyUnit.StudyUnitModel;
        //        studyUnitModel.StatisticsInfos = statisticsInfos;
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.Message);
        //    }

        //    return true;
        //}

        //private double? ToDouble(object elem)
        //{
        //    if (elem == null || DBNull.Value.Equals(elem))
        //    {
        //        return null;
        //    }
        //    try
        //    {
        //        return (double)elem;
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.Message);
        //    }
        //    return null;
        //}

        //private bool IsMissingValue(SpssVariable spssVariable, double num)
        //{
        //    foreach (double missingValue in spssVariable.MissingValues)
        //    {
        //        if (missingValue == num)
        //        {
        //            return true;
        //        }
        //    }
        //    return false;
        //}


        public RawData LoadRawData(string path)
        {
            Debug.Assert(!string.IsNullOrEmpty(path));

            RawData rawData = null;
            using (SavFileParser parser = new SavFileParser(File.OpenRead(path)))
            {
                rawData = new RawData();
                int index = 0;
                foreach (SpssVariable variable in parser.Variables)
                {
                    rawData.Variables.Add(new RawVariable(variable.Name, index++, variable.MissingValues));
                }
                foreach (IEnumerable<object> elems in parser.ParsedDataRecords)
                {
                    rawData.Records.Add(new RawRecord(elems.ToArray()));
                }
            }
            return rawData;
        }
    }
}
