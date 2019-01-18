using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.ViewModel;
using System.ComponentModel;
using EDO.Core.Model;

namespace EDO.QuestionCategory.QuestionForm
{
    public class MissingValueVM :BaseVM, IEditableObject
    {
        public static bool IsMissingValue(ICollection<MissingValueVM> missingValues, string value)
        {
            return missingValues.Any(x => x.Content == value);
        }

        public static List<string> ToMissingValueStrings(ICollection<MissingValueVM> missingValues)
        {
            return missingValues.Select(x => x.Content).ToList();
        }

        private MissingValue missingValue;
        private MissingValue bakMissingValue;
    
        public MissingValueVM() :this(new MissingValue())
        {
        }

        public MissingValueVM(MissingValue missingValue)
        {
            this.missingValue = missingValue;
        }

        public MissingValue MissingValue { get { return missingValue; } }

        public override object Model
        {
            get
            {
                return missingValue;
            }
        }

        public string Content
        {
            get
            {
                return missingValue.Content;
            }
            set
            {
                if (missingValue.Content != value)
                {
                    missingValue.Content = value;
                    NotifyPropertyChanged("Content");
                }
            }
        }


        protected override void PrepareValidation()
        {
            if (string.IsNullOrEmpty(Content))
            {
                Content = EMPTY_VALUE;
            }
        }


        #region IEditableObject Member

        private bool inEdit = false;
        public bool InEdit { get { return inEdit; } }
        public void BeginEdit()
        {
            if (inEdit)
            {
                return;
            }
            inEdit = true;
            bakMissingValue = missingValue.Clone() as MissingValue;
        }

        public void CancelEdit()
        {
            if (!inEdit)
            {
                return;
            }
            inEdit = false;
            this.Content = bakMissingValue.Content;

        }

        public void EndEdit()
        {
            if (!inEdit)
            {
                return;
            }
            inEdit = false;
            bakMissingValue = null;
            Memorize();   
        }

        #endregion
    }
}
