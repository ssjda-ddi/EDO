using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO.QuestionCategory.SequenceForm
{
    public class ChangeSingleQuestionNumberWindowVM
    {
        public ChangeSingleQuestionNumberWindowVM(QuestionConstructVM questionConstruct)
        {
            this.questionConstruct = questionConstruct;
            this.controlConstructScheme = (ControlConstructSchemeVM)questionConstruct.Parent;
            this.questionNumber = new QuestionNumberVM(questionConstruct);
        }

        private QuestionConstructVM questionConstruct;
        private ControlConstructSchemeVM controlConstructScheme;
        private QuestionNumberVM questionNumber;
        public QuestionNumberVM QuestionNumber { get { return questionNumber; } }

        public void Validate()
        {
            SequenceUtils.ValidateQuestionNumber(controlConstructScheme, questionNumber);
        }
    }
}
