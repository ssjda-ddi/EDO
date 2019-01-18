using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.VisualBasic.FileIO;

namespace EDO.Core.IO
{
    public class CsvReader
    {
        public RawData LoadRawData(string path)
        {
            Debug.Assert(!string.IsNullOrEmpty(path));

            RawData rawData = null;
            using (TextFieldParser parser = new TextFieldParser(path, Encoding.UTF8))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (parser.EndOfData == false)
                {
                    string[] values = parser.ReadFields();
                    if (rawData == null)
                    {
                        rawData = new RawData();
                        rawData.AddVariables(values);
                    }
                    else
                    {
                        rawData.AddRecord(values);
                    }
                }
            }
            return rawData;
        }
    }
}
