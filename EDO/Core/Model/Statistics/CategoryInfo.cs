using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using EDO.Core.Util;

namespace EDO.Core.Model.Statistics
{
    public enum CategoryTypes
    {
        NormalValue,
        MissingValue
    }

    public class CategoryInfo
    {
        public static void SplitByType(List<CategoryInfo> categoryInfos, List<CategoryInfo> normalInfos, List<CategoryInfo> missingInfos)
        {
            foreach (CategoryInfo categoryInfo in categoryInfos)
            {
                if (categoryInfo.IsTypeNormalValue)
                {
                    normalInfos.Add(categoryInfo);
                }
                else if (categoryInfo.IsTypeMissingValue)
                {
                    missingInfos.Add(categoryInfo);
                }
            }
        }

        public static decimal TotalFrequency(List<CategoryInfo> categoryInfos)
        {
            return categoryInfos.Sum(x => x.Frequency);
        }

        public static decimal TotalPercent(List<CategoryInfo> categoryInfos)
        {
            return categoryInfos.Sum(x => x.Percent);
        }

        public static decimal TotalEffectivePercent(List<CategoryInfo> categoryInfos)
        {
            return categoryInfos.Sum(x => x.EffectivePercent);
        }

        public static CategoryInfo FindByCodeValue(List<CategoryInfo> categoryInfos, string codeValue)
        {
            return categoryInfos.FirstOrDefault(x => x.CodeValue == codeValue);
        }

        public CategoryInfo()
        {
            CategoryType = CategoryTypes.NormalValue;
        }
        public CategoryTypes CategoryType { get; set; } // for multiple answer
        public string VariableTitle { get; set; }
        public string CodeValue { get; set; }
        public string CategoryTitle { get; set; }
        public int Frequency { get; set; }
        public decimal Percent { get; set; }
        public decimal EffectivePercent { get; set; }
        public decimal CasePercent { get; set; } // for multiple answer
        public decimal EffectiveCasePercent { get; set; } // for multiple answe
        public bool IsTypeNormalValue {get {return CategoryTypes.NormalValue == CategoryType; }}
        public bool IsTypeMissingValue { get { return CategoryTypes.MissingValue == CategoryType; } }
        public bool IsEmptyValue { get { return string.IsNullOrEmpty(CodeValue); } }

        [XmlIgnore]
        public int Scale { get; set; }
        public string FrequencyString { get { return Frequency.ToString(); } }
        public string PercentString { get { return StatisticsUtils.ToString(Percent, Scale); } }
        public string EffectivePercentString 
        { 
            get 
            {
                return IsTypeNormalValue ? StatisticsUtils.ToString(EffectivePercent, Scale) : EDOConstants.EMPTY_CURSOR;
            }
        }
        public string CasePercentString { get { return StatisticsUtils.ToString(CasePercent, Scale); } }
        public string EffectiveCasePercentString
        {
            get
            {
                return IsTypeNormalValue ? StatisticsUtils.ToString(EffectiveCasePercent, Scale) : EDOConstants.EMPTY_CURSOR;
            }
        }
    }
}
