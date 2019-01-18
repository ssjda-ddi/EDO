using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpssLib.SpssDataset;
using SpssLib.FileParser;

namespace EDO.Core.IO
{
    using SpssVariable = SpssLib.SpssDataset.Variable;

    public class SpssData
    {
        public ICollection<SpssVariable> Variables { get; private set; }
        public ICollection<SpssRecord> Records { get; private set; }
        public SpssData()
        {
            Variables = new List<SpssVariable>();
            Records = new List<SpssRecord>();
        }

        public SpssData(SavFileParser fileReader)
            : this()
        {
            foreach (var variable in fileReader.Variables)
            {
                Variables.Add(variable);
            }
            foreach (var dataRecord in fileReader.ParsedDataRecords)
            {
                Records.Add(new SpssRecord(dataRecord.ToArray(), this));
            }
        }

        //public SpssDataset(Stream fileStream)
        //    : this(new SavFileParser(fileStream))
        //{
        //}
        public SpssVariable GetVariable(string variableName)
        {
            foreach (SpssVariable variable in Variables)
            {
                if (variable.Name == variableName)
                {
                    return variable;
                }
            }
            return null;
        }

        public int GetVariableIndex(string variableName)
        {
            int index = 0;
            foreach (SpssVariable variable in Variables)
            {
                if (variable.Name == variableName)
                {
                    return index;
                }
                index++;
            }
            return -1;
        }

        public List<object> GetColumnValues(int index)
        {
            List<object> values = new List<object>();
            foreach (SpssRecord record in Records)
            {
                object value = record[index];
                values.Add(value);
            }
            return values;
        }
    }
}
