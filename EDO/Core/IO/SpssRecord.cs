using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO.Core.IO
{
    public class SpssRecord
    {
        private object[] elems;
        private SpssData spssData;


        internal SpssRecord(object[] elems, SpssData spssData)
        {
            this.elems = elems;
            this.spssData = spssData;
        }

        public object this[int index]
        {
            get
            {
                return elems[index];
            }
        }

        //public object this[Variable variable]
        //{
        //    get
        //    {
        //        return this[variable.Index];
        //    }
        //}

        public override string ToString()
        {
            return string.Join(",", elems);
//            return string.Join(",", elems.Select(x => x.ToString()).ToArray());
        }
    }
}
