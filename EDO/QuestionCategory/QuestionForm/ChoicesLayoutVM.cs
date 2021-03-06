﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.ViewModel;
using EDO.Core.Model.Layout;
using EDO.Core.Model;

namespace EDO.QuestionCategory.QuestionForm
{
    public class ChoicesLayoutVM : ResponseLayoutVM
    {
        public static bool IsValidMultipleAnswerSelectedValue(string value)
        {
            int num = 0;
            if (!Int32.TryParse(value, out num))
            {
                return false;
            }
            return num >= 1 && num <= 10;
        }

        public ChoicesLayoutVM(ChoicesLayout choicesLayout)
            : base(choicesLayout)
        {
        }

        public ChoicesLayout ChoicesLayout { get { return (ChoicesLayout)Layout; } }

        public ChoicesLayoutMesurementMethod MeasurementMethod {
            get
            {
                return ChoicesLayout.MeasurementMethod;
            }
            set
            {
                if (ChoicesLayout.MeasurementMethod != value)
                {
                    ChoicesLayout.MeasurementMethod = value;
                    NotifyPropertyChanged("MeasurementMethod");
                    Memorize();
                }
            }
        }

        public int? MaxSelectionCount
        {
            get
            {
                return ChoicesLayout.MaxSelectionCount;
            }
            set
            {
                if (ChoicesLayout.MaxSelectionCount != value)
                {
                    ChoicesLayout.MaxSelectionCount = value;
                    NotifyPropertyChanged("MaxSelectionCount");
                    Memorize();
                }
            }
        }

        public string MultipleAnswerSelectedValue
        {
            get
            {
                return ChoicesLayout.MultipleAnswerSelectedValue;
            }
            set
            {
                if (ChoicesLayout.MultipleAnswerSelectedValue != value && IsValidMultipleAnswerSelectedValue(value))
                {
                    ChoicesLayout.MultipleAnswerSelectedValue = value;
                    NotifyPropertyChanged("MultipleAnswerSelectedValue");
                    Memorize();
                }
            }
        }

        public bool Surround
        {
            get
            {
                return ChoicesLayout.Surround;
            }
            set
            {
                if (ChoicesLayout.Surround != value)
                {
                    ChoicesLayout.Surround = value;
                    NotifyPropertyChanged("Surround");
                    Memorize();
                }
            }
        }

        public ChoicesLayoutDirection Direction
        {
            get
            {
                return ChoicesLayout.Direction;
            }
            set
            {
                if (ChoicesLayout.Direction != value)
                {
                    ChoicesLayout.Direction = value;
                    NotifyPropertyChanged("Direction");
                    Memorize();
                }
            }
        }

        public int? ColumnCount
        {
            get
            {
                return ChoicesLayout.ColumnCount;
            }
            set
            {
                if (ChoicesLayout.ColumnCount != value)
                {
                    ChoicesLayout.ColumnCount = value;
                    NotifyPropertyChanged("ColumnCount");
                    Memorize();
                }
            }
        }

        public bool MeasurementMethodMultiple
        {
            get
            {
                return ChoicesLayout.MeasurementMethodMultiple;
            }
        }
    }
}
