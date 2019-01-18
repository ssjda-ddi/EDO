using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.Model.Statistics;
using EDO.Core.Model;

namespace EDO.Core.Util.Statistics
{
    public abstract class CategoryHelper
    {
        public static CategoryTypes GeSingleAnswerCategoryType(string codeValue, Category category)
        {
            if (string.IsNullOrEmpty(codeValue))
            {
                return CategoryTypes.MissingValue;
            }
            if (category != null)
            {
                return CategoryHelper.ToCategoryType(category.IsMissingValue);
            }
            return CategoryTypes.NormalValue;
        }

        public static string GeSingleAnswerCategoryTitle(string codeValue, Category category)
        {
            if (string.IsNullOrEmpty(codeValue))
            {
                return EDOConstants.LABEL_EMPTY;
            }
            if (category != null)
            {
                return category.Title;
            }
            return EDOConstants.LABEL_UNDEFINED;
        }

        public static CategoryTypes GetDateTimeCategoryType(string codeValue, List<MissingValue> missingValues)
        {
            if (string.IsNullOrEmpty(codeValue))
            {
                return CategoryTypes.MissingValue;
            }
            return CategoryHelper.ToCategoryType(missingValues.Any(x => x.Content == codeValue));
        }

        public static string GetDateTimeCategoryTitle(string codeValue)
        {
            if (string.IsNullOrEmpty(codeValue))
            {
                return EDOConstants.LABEL_EMPTY;
            }
            return codeValue;
        }

        public static CategoryTypes ToCategoryType(bool isMissingValue) { return isMissingValue ? CategoryTypes.MissingValue : CategoryTypes.NormalValue; }
        public abstract CategoryInfo CreateCategoryInfo(string key, int cases);
    }
}
