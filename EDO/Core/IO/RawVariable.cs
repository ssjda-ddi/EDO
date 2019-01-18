using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO.Core.IO
{
    public class RawVariable
    {
        public RawVariable(string name, int index) :this(name, index, new List<double>())
        {
        }

        public RawVariable(string name, int index, ICollection<double> missingValues)
        {
            Name = name;
            Index = index;
            MissingValues = new List<double>(missingValues);
        }

        public string Name { get; set; }
        public int Index { get; set; }
        public List<double> MissingValues { get; private set; }
    }
}
