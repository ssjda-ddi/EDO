using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.VariableCategory.VariableForm;
using EDO.QuestionCategory.QuestionForm;
using System.Diagnostics;

namespace EDO.Core.Util.Statistics
{
    // Variable information for statistics
    public class NumberHelper
    {
        public static NumberHelper Create(VariableVM variable)
        {
            NumberHelper result = new NumberHelper();
            result.Scale = variable.Response.Scale;
            result.Min = variable.Response.Min;
            result.Max = variable.Response.Max;
            foreach (MissingValueVM missingValue in variable.Response.MissingValues)
            {
                result.AddMissingValue(missingValue.Content);
            }
            return result;
        }

        public static decimal? ToDecimal(string str, int scale)
        {
            // str="23.452", scale=2 => 23.45m
            // str="23.45", scale=2 - 23.45m
            decimal num = 0;
            if (!decimal.TryParse(str, out num))
            {
                return null;
            }
            return Math.Round(num, scale);
        }

        public static decimal? ToDecimal(double value, int scale)
        {
            decimal? result = null;
            try
            {
                decimal decimalValue = Convert.ToDecimal(value);
                result = Math.Round(decimalValue, scale);
            }
            catch (OverflowException ex)
            {
                Debug.WriteLine(ex.Message);
            }
            return result;
        }

        public NumberHelper()
        {
            Scale = 0;
            MissingValues = new List<decimal>();
        }

        public int Scale { get; set; }

        public List<decimal> MissingValues { get; set; }
        public void AddMissingValue(string value)
        {
            decimal? mv = ToDecimal(value, Scale);
            if (mv.HasValue)
            {
                MissingValues.Add(mv.Value);
            }
        }

        public decimal? Max { get; set; }
        public decimal? Min { get; set; }

        public bool IsMissingValue(double doubleValue)
        {
            decimal? decimalValue = ToDecimal(doubleValue, Scale);
            if (!decimalValue.HasValue)
            {
                return true;
            }
            bool isMissingValue =  MissingValues.Any(x => x == decimalValue.Value);
            if (isMissingValue)
            {
                return true;
            }
            if (Min.HasValue && decimalValue.Value < Min.Value)
            {
                return true;
            }
            if (Max.HasValue && decimalValue.Value > Max.Value)
            {
                return true;
            }
            return false;
        }
    }
}
