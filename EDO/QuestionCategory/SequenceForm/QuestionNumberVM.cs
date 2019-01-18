using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.ViewModel;

namespace EDO.QuestionCategory.SequenceForm
{
    public class QuestionNumberVM :BaseVM
    {
        public static Dictionary<string, List<QuestionNumberVM>> CreateQuestionNumberDict(ICollection<QuestionNumberVM> questionNumbers)
        {
            var result = new Dictionary<string, List<QuestionNumberVM>>();
            foreach (QuestionNumberVM questionNumber in questionNumbers)
            {
                string nextNumber = questionNumber.NextValue;

                List<QuestionNumberVM> sameQuestionNumbers = null;
                if (result.ContainsKey(nextNumber))
                {
                    sameQuestionNumbers = result[nextNumber];
                }
                else
                {
                    sameQuestionNumbers = new List<QuestionNumberVM>();
                    result[nextNumber] = sameQuestionNumbers;
                }
                sameQuestionNumbers.Add(questionNumber);
            }
            return result;
        }

        public static void ClearValidationFlags(ICollection<QuestionNumberVM> questionNumbers)
        {
            foreach (QuestionNumberVM questionNumber in questionNumbers)
            {
                questionNumber.IsValid = true;
            }
        }

        public static int CountError(ICollection<QuestionNumberVM> questionNumbers)
        {
            int errorCount = 0;
            foreach (QuestionNumberVM questionNumber in questionNumbers)
            {
                if (!questionNumber.IsValid)
                {
                    errorCount++;
                }
            }
            return errorCount;
        }

        public static void CheckDuplicate(string nextNumber, List<QuestionNumberVM> questionNumbers, List<StatementVM> statements)
        {
            //if (questionNumbers.Count < 2)
            //{
            //    return;
            //}
            foreach (QuestionNumberVM questionNumber in questionNumbers)
            {
                if (questionNumber.HasAfterValue && (questionNumbers.Count > 1 || statements.Count > 0))
                {
                    questionNumber.IsValid = false;
                }
            }
        }

        public static List<QuestionNumberVM> FindUpdatingQuestionNumbers(ICollection<QuestionNumberVM> questionNumbers)
        {
            List<QuestionNumberVM> updatingQuestionNumbers = new List<QuestionNumberVM>();
            foreach (QuestionNumberVM questionNumber in questionNumbers)
            {
                if (questionNumber.HasAfterValue)
                {
                    updatingQuestionNumbers.Add(questionNumber);
                }
            }
            return updatingQuestionNumbers;
        }

        public static void UpdateQuestionNumbers(List<QuestionNumberVM> questionNumbers)
        {
            foreach (QuestionNumberVM questionNumber in questionNumbers)
            {
                questionNumber.UpdateQuestionNumber();
            }
        }

        public QuestionNumberVM(QuestionConstructVM questionConstruct)
        {
            this.questionConstruct = questionConstruct;
            IsValid = true;
        }

        private QuestionConstructVM questionConstruct;

        public QuestionConstructVM QuestionConstruct { get { return questionConstruct; } }

        public string BeforeValue
        {
            get
            {
                return questionConstruct.No;
            }
        }

        private string afterValue;
        public string AfterValue
        {
            get
            {
                return afterValue;
            }
            set
            {
                if (afterValue != value && (string.IsNullOrEmpty(value) || SequenceUtils.ValidateQuestionNumber(value)))
                {
                    afterValue = value;
                    NotifyPropertyChanged("AfterValue");
                }
                NotifyPropertyChanged("AfterValue");
            }
        }

        public void UpdateQuestionNumber()
        {
            questionConstruct.No = afterValue;
        }

        private bool isValid;
        public bool IsValid 
        {
            get
            {
                return isValid;
            }
            set
            {
                if (isValid != value)
                {
                    isValid = value;
                    NotifyPropertyChanged("IsValid");
                }
            }
        }

        public bool HasAfterValue
        {
            get
            {
                return !string.IsNullOrEmpty(AfterValue);
            }
        }

        public string NextValue
        {
            get
            {
                if (HasAfterValue)
                {
                    return AfterValue;
                }
                return BeforeValue;
            }
        }
    }
}
