using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.ViewModel;
using EDO.Core.Model;

namespace EDO.DataCategory.StatisticsForm
{
    public class VariableInfoVM :BaseVM, IStringIDProvider
    {
        public static VariableInfoVM Find(ICollection<VariableInfoVM> variables, string variableId)
        {
            return variables.Where(x => x.Id == variableId).FirstOrDefault();
        }

        public VariableInfoVM(Variable variable)
        {
            this.variable = variable;
        }

        private Variable variable;
        private Variable Variable { get { return variable; } }

        public override object Model { get { return variable; } }

        public string Id { get { return variable.Id; } }

        public string QuestionId { get { return variable.QuestionId; } }

        private string conceptTitle;

        public string ConceptTitle
        {
            get
            {
                return conceptTitle;
            }
            set
            {
                if (conceptTitle != value)
                {
                    conceptTitle = value;
                    NotifyPropertyChanged("ConceptTitle");
                }
            }
        }

        public string VariableTitle
        {
            get
            {
                return variable.Title;
            }
        }

        public string VariableLabel
        {
            get
            {
                return variable.Label;
            }
        }

        private string questionTitle;
        public string QuestionTitle
        {
            get
            {
                return questionTitle;
            }
            set
            {
                if (questionTitle != value)
                {
                    questionTitle = value;
                    NotifyPropertyChanged("QuestionTitle");
                }
            }
        }

        private string universeTitle;
        public string UniverseTitle
        {
            get
            {
                return universeTitle;
            }
            set
            {
                if (universeTitle != value)
                {
                    universeTitle = value;
                    NotifyPropertyChanged("UniverseTitle");
                }
            }
        }

        private string variableType;
        public string VariableType
        {
            get
            {
                return variableType;
            }
            set
            {
                if (variableType != value)
                {
                    variableType = value;
                    NotifyPropertyChanged("VariableType");
                }
            }
        }
        
    }
}
