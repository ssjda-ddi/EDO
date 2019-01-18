using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using EDO.Core.ViewModel;

namespace EDO.QuestionCategory.SequenceForm
{
    public class ChangeMultipleQuestionNumberWindowVM :BaseVM
    {
        public ChangeMultipleQuestionNumberWindowVM(ControlConstructSchemeVM controlConstructScheme)
        {
            this.controlConstructScheme = controlConstructScheme;

            questionNumbers = new ObservableCollection<QuestionNumberVM>();
            foreach (QuestionConstructVM questionConstruct in controlConstructScheme.QuestionConstructs)
            {
                QuestionNumberVM questionNumber = new QuestionNumberVM(questionConstruct);
                questionNumbers.Add(questionNumber);
            }
        }

        private ControlConstructSchemeVM controlConstructScheme;
        private ObservableCollection<QuestionNumberVM> questionNumbers;
        public ObservableCollection<QuestionNumberVM> QuestionNumbers { get { return questionNumbers; } }


        public void Validate()
        {
            SequenceUtils.ValidateQuestionNumbers(controlConstructScheme, questionNumbers);
        }
    }
}
