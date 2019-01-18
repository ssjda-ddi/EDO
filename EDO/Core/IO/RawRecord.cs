using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO.Core.IO
{
    public class RawRecord
    {
        public RawRecord() :this(new object[] {})
        {
        }

        public RawRecord(object[] elems)
        {
            this.elems = elems;
        }

        private object[] elems;

        public object this[int index]
        {
            get
            {
                return elems[index];
            }
        }

        public object GetValue(int index)
        {
            if (elems == null || (index < 0 || index >= elems.Count()))
            {
                return null;
            }
            return elems[index];
        }

        public override string ToString()
        {
            return string.Join(",", elems);
        }        
    }
}
