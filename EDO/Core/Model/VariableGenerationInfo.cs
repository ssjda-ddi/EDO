using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO.Core.Model
{
    public class VariableGenerationInfo :ICloneable
    {
        public VariableGenerationInfo()
        {
            ResponseTypeCode = Options.RESPONSE_TYPE_UNKNOWN_CODE;
            ChoicesLayoutMesurementMethod = ChoicesLayoutMesurementMethod.Single;
        }

        // source question info
        public string ResponseTypeCode { get; set; }
        public ChoicesLayoutMesurementMethod ChoicesLayoutMesurementMethod { get; set; }

        // calculated data
        public VariableGenerationType VariableGenerationType
        {
            get
            {
                if (ChoicesLayoutMesurementMethod == ChoicesLayoutMesurementMethod.Multiple)
                {
                    return VariableGenerationType.MultipleVariable;
                }
                return VariableGenerationType.SingleVariable;
            }
        }

        #region ICloneable member

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion
    }
}
