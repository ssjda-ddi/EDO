using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using EDO.Core.Util;

namespace EDO.Core.Model.Statistics
{
    public class SummaryInfo
    {
        public SummaryInfo()
        {
        }

        public int ValidCases { get; set; }
        public int InvalidCases { get; set; }
        public int EmptyCases { get; set; }
        public int TotalCases { get { return ValidCases + InvalidCases + EmptyCases; } }
        public double Mean { get; set; }
        public double Median { get; set; }
        public double StandardDeviation { get; set; }
        public double Minimum { get; set; }
        public double Maximum { get; set; }


        [XmlIgnore]
        public int Scale { get; set; }
        public string ValidCasesString { get { return ValidCases.ToString(); } }
        public string InvalidCasesString { get { return InvalidCases.ToString(); } }
        public string TotalCasesString { get { return TotalCases.ToString(); } }
        public string MeanString { get { return StatisticsUtils.ToString(Mean, Scale); } }
        public string MedianString { get { return StatisticsUtils.ToString(Median, Scale); } }
        public string StandardDeviationString {get {return StatisticsUtils.ToString(StandardDeviation, Scale); }}
        public string MinimumString {get {return StatisticsUtils.ToString(Minimum, Scale);}}
        public string MaximumString { get { return StatisticsUtils.ToString(Maximum, Scale); } }
    }
}
