using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.QuestionCategory.CodeForm;
using EDO.Core.Model.Statistics;
using EDO.Core.Model;

namespace EDO.Core.Util.Statistics
{
    public class SingleAnswerHelper :CategoryHelper
    {
        public static SingleAnswerHelper Create(ICollection<CodeVM> codes)
        {
            return new SingleAnswerHelper(new List<ICode>(codes));
        }

        public SingleAnswerHelper(ICollection<ICode> codes)
        {
            this.codes = codes;
        }

        private ICollection<ICode> codes;

        public override CategoryInfo CreateCategoryInfo(string key, int cases)
        {
            CategoryTypes type = CategoryTypes.NormalValue;
            string title = EDOConstants.LABEL_UNDEFINED;
            ICode code = codes.FirstOrDefault(x => x.Value == key);
            if (code != null)
            {
                type = ToCategoryType(code.IsMissingValue);
                title = code.Label;
            }
            CategoryInfo categoryInfo = new CategoryInfo();
            categoryInfo.Frequency = cases;
            categoryInfo.CategoryType = type;
            categoryInfo.CodeValue = key;
            categoryInfo.CategoryTitle = title;
            return categoryInfo;
        }
         
    }
}
