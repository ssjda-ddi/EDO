using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO.Core.Model
{
    public class SamplingNumber :ICloneable
    {
        public SamplingNumber()
        {
        }

        public string SampleSize { get; set; }
        public string NumberOfResponses { get; set; }
        public string ResponseRate { get; set; }
        public string Description { get; set; }

        #region ICloneable Member

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
