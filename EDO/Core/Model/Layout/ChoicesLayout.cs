using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO.Core.Model.Layout
{
    public class ChoicesLayout :ResponseLayout
    {
        public static readonly string DEF_MULTI_ANSWER_SELECTED_VALUE = "1";

        public ChoicesLayout()
        {
            MultipleAnswerSelectedValue = DEF_MULTI_ANSWER_SELECTED_VALUE; // Default MA selected value.
        }

        public ChoicesLayoutMesurementMethod MeasurementMethod { get; set; }

        public bool MeasurementMethodMultiple { get { return ChoicesLayoutMesurementMethod.Multiple == MeasurementMethod; } }

        public int? MaxSelectionCount { get; set; }

        public string MultipleAnswerSelectedValue { get; set; }

        public bool Surround { get; set; }

        public ChoicesLayoutDirection Direction { get; set; }

        public int? ColumnCount { get; set; }
    }
}
