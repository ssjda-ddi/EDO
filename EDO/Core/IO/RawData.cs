using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO.Core.IO
{
    public class RawData
    {
        public RawData()
        {
            Variables = new List<RawVariable>();
            Records = new List<RawRecord>();
        }

        public List<RawVariable> Variables { get; private set; }
        public List<RawRecord> Records { get; private set; }

        public void AddVariables(IEnumerable<string> variableNames)
        {
            int index = 0;
            foreach (string variableName in variableNames)
            {
                Variables.Add(new RawVariable(variableName, index++));
            }
        }

        public void AddRecord(IEnumerable<string> values)
        {
            Records.Add(new RawRecord(values.ToArray<object>()));
        }


        public RawVariable GetVariable(string variableName)
        {
            foreach (RawVariable variable in Variables)
            {
                if (string.Compare(variable.Name, variableName, true) == 0)
                {
                    return variable;
                }
            }
            return null;
        }

        public List<object> GetColumnValues(int index)
        {
            List<object> values = new List<object>();
            foreach (RawRecord record in Records)
            {
                object value = record.GetValue(index);
                values.Add(value);
            }
            return values;
        }

        public List<int> GetVariableIndexes(List<string> variableNames)
        {
            List<int> indexes = new List<int>();
            foreach (string variableName in variableNames)
            {
                RawVariable rawVariable = GetVariable(variableName);
                if (rawVariable != null)
                {
                    indexes.Add(rawVariable.Index);
                }
            }
            return indexes;
        }
    }
}
