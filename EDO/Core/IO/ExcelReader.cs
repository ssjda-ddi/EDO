using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace EDO.Core.IO
{
    public class ExcelReader
    {
        public RawData LoadRawData(string path)
        {
            Debug.Assert(!string.IsNullOrEmpty(path));

            RawData rawData = null;
            using (ExcelParser parser = new ExcelParser(path))
            {
                List<ExcelParser.ExcelRow> excelRows = parser.GetAll();
                if (excelRows.Count > 0)
                {
                    rawData = new RawData();
                    ExcelParser.ExcelRow headerRow = excelRows[0];
                    rawData.AddVariables(headerRow.Values);

                    for (int i = 1; i < excelRows.Count; i++)
                    {
                        ExcelParser.ExcelRow excelRow = excelRows[i];
                        rawData.AddRecord(excelRow.Values);
                    }
                }
            }
            return rawData;
        }
    }
}
