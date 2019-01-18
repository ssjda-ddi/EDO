using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using System.IO;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace EDO.Core.IO
{
    public class ExcelParser :IDisposable
    {
        // A2, B2 => A, B
        public static string GetColumnName(string cellReference)
        {
            Regex regex = new Regex("[A-Za-z]+");
            Match match = regex.Match(cellReference);
            return match.Value;
        }

        public class ExcelRow
        {
            private List<string> cellNames;
            private List<string> values;
            public ExcelRow()
            {
                cellNames = new List<string>();
                values = new List<string>();
            }
            public List<string> CellNames { get { return cellNames; } }
            public List<string> Values { get { return values; } }
            public void Add(string cellName, string value)
            {
                cellNames.Add(cellName);
                values.Add(value);
            }
            public int Count { get { return cellNames.Count; } }
            public string ToCSV()
            {
                return string.Join(",", values);
            }
            public object[] ToObjectArray()
            {
                object[] result = new object[Values.Count];
                for (int i = 0; i < values.Count; i++)
                {
                    result[i] = values[i];
                }
                return result;
            }
        }

        private SpreadsheetDocument document;
        private WorkbookPart workbookPart;
        private WorksheetPart worksheetPart;

        public ExcelParser(string fileName)
        {
            document = SpreadsheetDocument.Open(fileName, false);
            if (document == null)
            {
                throw new FileLoadException(string.Format("file load error: {0}, fileName"));
            }
            workbookPart = document.WorkbookPart;
            ChangeSheet();
        }

        public void Dispose()
        {
            if (document != null)
            {
                document.Close();
            }
        }

        public void ChangeSheet(string sheetName = null)
        {
            Sheet sheet = null;
            if (string.IsNullOrEmpty(sheetName))
            {
                sheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault();
            }
            else
            {
                sheet = workbookPart.Workbook.Descendants<Sheet>().Where(s => s.Name == sheetName).FirstOrDefault();
            }
            if (sheet == null)
            {
                throw new ArgumentException("sheetName");
            }
            worksheetPart = (WorksheetPart)(workbookPart.GetPartById(sheet.Id));
        }


        public ExcelRow GetRow(int rowIndex)
        {
            IEnumerable<Row> rows = worksheetPart.Worksheet.Descendants<Row>();
            if (rowIndex >= rows.Count())
            {
                return new ExcelRow();
            }
            Row row = rows.ElementAt(rowIndex);
            return GetRow(row);
        }

        private ExcelRow GetRow(Row row)
        {
            ExcelRow excelRow = new ExcelRow();
            foreach (Cell cell in row.Descendants<Cell>())
            {
                string value = GetCellValue(cell);
                excelRow.Add(cell.CellReference, value);
            }
            return excelRow;

        }

        private ExcelRow GetRow(Row row, List<string> headrCellNames, Dictionary<string, string> headerColumnNames)
        {
            uint rowIndex = row.RowIndex;
            IEnumerable<Cell> cells = row.Descendants<Cell>();
            Dictionary<string, Cell> cellDict = new Dictionary<string, Cell>();
            foreach (Cell cell in cells)
            {
                cellDict[cell.CellReference] = cell;
            }

            ExcelRow excelRow = new ExcelRow();
            foreach (string headerCellName in headrCellNames)
            {
                string columnName = headerColumnNames[headerCellName];
                string cellName = columnName + rowIndex;
                string value = null;
                if (cellDict.ContainsKey(cellName))
                {
                    Cell cell = cellDict[cellName];
                    value = GetCellValue(cell);
                }
                excelRow.Add(cellName, value);
            }

            return excelRow;
        }

        public List<ExcelRow> GetAll() 
        {
            Stopwatch sw = new Stopwatch();

            List<Row> rows = new List<Row>(worksheetPart.Worksheet.Descendants<Row>());
            if (rows.Count() == 0) {
                return new List<ExcelRow>();
            }

            //Debug.WriteLine("Elapsed1={0}", sw.Elapsed);
            //sw.Restart();

            List<ExcelRow> excelRows = new List<ExcelRow>();
            ExcelRow headerRow = GetRow(rows[0]);
            excelRows.Add(headerRow);

            Dictionary<string, string> headerColumnNames = new Dictionary<string, string>();
            foreach (string headerCellName in headerRow.CellNames)
            {
                headerColumnNames[headerCellName] = GetColumnName(headerCellName);
            }

            for (int i = 1; i < rows.Count(); i++)
            {
                Row row = rows[i];
                ExcelRow excelRow = GetRow(row, headerRow.CellNames, headerColumnNames);
                excelRows.Add(excelRow);
            }

            //Debug.WriteLine("Elapsed2={0}", sw.Elapsed);
            //sw.Stop();

            return excelRows;
        }

        public string GetCellValue(string addressName)
        {
            if (worksheetPart == null)
            {
                return null;
            }
            Cell cell = worksheetPart.Worksheet.Descendants<Cell>().
              Where(c => c.CellReference == addressName).FirstOrDefault();
            if (cell == null)
            {
                return null;
            }
            return GetCellValue(cell);
        }

        public string GetCellValue(Cell cell)
        {
            string value = cell.InnerText;
            if (cell.DataType != null)
            {
                switch (cell.DataType.Value)
                {
                    case CellValues.SharedString:
                        var stringTable =
                            workbookPart.GetPartsOfType<SharedStringTablePart>()
                            .FirstOrDefault();
                        if (stringTable != null)
                        {
                            value =
                                stringTable.SharedStringTable
                                .ElementAt(int.Parse(value)).InnerText;
                        }
                        break;

                    case CellValues.Boolean:
                        switch (value)
                        {
                            case "0":
                                value = "FALSE";
                                break;
                            default:
                                value = "TRUE";
                                break;
                        }
                        break;
                }
            }

            return value;

        }


    }
}
