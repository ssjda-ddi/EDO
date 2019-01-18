using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.Model;

namespace EDO.GroupCategory.CompareForm
{
    public class DiffOption : Option
    {
        public DiffOption(Option option)
            : this(option.Code, option.Label, null)
        {
        }

        public DiffOption(string code, string label) :this(code, label, null)
        {
        }

        public DiffOption(string code, string label, string targetTitle) :base(code, label)
        {
            this.TargetTitle = targetTitle;
        }

        public string TargetTitle { get; set; } //Title of the item in the comparison to be displayed in the combo

        public string DetailLabel
        {
            get
            {
                if (string.IsNullOrEmpty(TargetTitle)) 
                {
                    return Label;
                }
                return Label + " " + TargetTitle;
            }
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", Code, Label, TargetTitle);
        }

        public bool IsMatchOrNotMatch
        {
            get
            {
                return Options.IsMatch(Code) || Options.IsNotMatch(Code);
            }
        }

        public bool IsPartialMatch
        {
            get
            {
                //When it partially matches as the code of Diff options, RowId is stored in the code.
                //use the decision of not being partial match or exact match here
                return !IsMatchOrNotMatch;
            }
        }
    }
}
