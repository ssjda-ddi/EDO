using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using EDO.Core.ViewModel;
using EDO.Core.Model;
using System.Diagnostics;

namespace EDO.SamplingCategory.SamplingForm
{
    public class SamplingNumberVM :BaseVM, IEditableObject
    {
        private SamplingNumber samplingNumber;
        private SamplingNumber bakSamplingNumber;

        public SamplingNumberVM()
            : this(new SamplingNumber())
        {
        }

        public SamplingNumberVM(SamplingNumber samplingNumber)
        {
            this.samplingNumber = samplingNumber;
            this.bakSamplingNumber = null;
        }

        #region Property

        public SamplingNumber SamplingNumber { get { return samplingNumber; } }
        public override object Model { get { return samplingNumber; } }
        public string SampleSize
        {
            get
            {
                return samplingNumber.SampleSize;
            }
            set
            {
                if (samplingNumber.SampleSize != value)
                {
                    samplingNumber.SampleSize = value;
                    NotifyPropertyChanged("SampleSize");
                }
            }
        }

        public string NumberOfResponses
        {
            get
            {
                return samplingNumber.NumberOfResponses;
            }
            set
            {
                if (samplingNumber.NumberOfResponses != value)
                {
                    samplingNumber.NumberOfResponses = value;
                    NotifyPropertyChanged("NumberOfResponses");
                }
            }
        }

        public string ResponseRate
        {
            get
            {
                return samplingNumber.ResponseRate;
            }
            set
            {
                if (samplingNumber.ResponseRate != value)
                {
                    samplingNumber.ResponseRate = value;
                    NotifyPropertyChanged("ResponseRate");
                }
            }
        }

        public string Description
        {
            get
            {
                return samplingNumber.Description;
            }
            set
            {
                if (samplingNumber.Description != value)
                {
                    samplingNumber.Description = value;
                    NotifyPropertyChanged("Description");
                }
            }
        }

        #endregion

        #region IEditableObject Member


        public bool InEdit { get { return inEdit; } }

        private bool inEdit;


        public void BeginEdit()
        {
            if (inEdit)
            {
                return;
            }
            inEdit = true;
            bakSamplingNumber = samplingNumber.Clone() as SamplingNumber;
        }

        public void CancelEdit()
        {
            if (!inEdit)
            {
                return;
            }
            inEdit = false;
            this.SampleSize = bakSamplingNumber.SampleSize;
            this.NumberOfResponses = bakSamplingNumber.NumberOfResponses;
            this.ResponseRate = bakSamplingNumber.ResponseRate;
            this.Description = bakSamplingNumber.Description;
        }

        public void EndEdit()
        {
            if (!inEdit)
            {
                return;
            }
            inEdit = false;
            bakSamplingNumber = null;
            Memorize();
        }


        protected override void PrepareValidation()
        {
            if (string.IsNullOrEmpty(SampleSize))
            {
                this.SampleSize = "0";
            }
        }

        #endregion
    }
}
