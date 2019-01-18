using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.QuestionCategory.QuestionForm;
using EDO.Core.Model.Statistics;

namespace EDO.Core.Util.Statistics
{
    public class DateTimeHelper :CategoryHelper
    {
        public static DateTimeHelper Create(ICollection<MissingValueVM> missingValues)
        {
            return new DateTimeHelper(MissingValueVM.ToMissingValueStrings(missingValues));
        }

        public DateTimeHelper(ICollection<string> missingValues)
        {
            this.missingValues = missingValues;
        }

        private ICollection<string> missingValues;

        public override CategoryInfo CreateCategoryInfo(string key, int cases)
        {
            CategoryInfo categoryInfo = new CategoryInfo();
            categoryInfo.Frequency = cases;
            bool isMissngValue = missingValues.Any(x => x == key);
            categoryInfo.CategoryType = ToCategoryType(isMissngValue);
            categoryInfo.CodeValue = key;
            categoryInfo.CategoryTitle = key;
            return categoryInfo;
        }
    }
}
