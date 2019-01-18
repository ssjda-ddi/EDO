using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.Model.Statistics;
using EDO.Core.Model;

namespace EDO.Core.Util.Statistics
{
    public class CategoryInfoCollection :IEnumerable<CategoryInfo>
    {
        public static CategoryInfoCollection[] Create(StatisticsInfo statisticsInfo)
        {
            CategoryInfoCollection totalCollection = new CategoryInfoCollection() {Scale = statisticsInfo.Scale };
            CategoryInfoCollection normalCollection = new CategoryInfoCollection() {Scale = statisticsInfo.Scale };
            CategoryInfoCollection missingCollection = new CategoryInfoCollection() {Scale = statisticsInfo.Scale };
            foreach (CategoryInfo categoryInfo in statisticsInfo.CategoryInfos)
            {
                if (categoryInfo.IsTypeNormalValue)
                {
                    normalCollection.Add(categoryInfo);
                }
                else if (categoryInfo.IsTypeMissingValue)
                {
                    missingCollection.Add(categoryInfo);
                }
                totalCollection.Add(categoryInfo);
            }
            return new CategoryInfoCollection[] { totalCollection, normalCollection, missingCollection };
        }

        private List<CategoryInfo> categoryInfos;

        public CategoryInfoCollection()
        {
            categoryInfos = new List<CategoryInfo>();
        }

        public int Scale { get; set; }

        public void Add(CategoryInfo categoryInfo)
        {
            categoryInfos.Add(categoryInfo);
        }

        public IEnumerator<CategoryInfo> GetEnumerator()
        {
            return categoryInfos.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string FullPercent { get { return StatisticsUtils.ToString(100m, Scale); } }
        public string TotalFrequencyString { get { return StatisticsUtils.TotalFrequencyString(categoryInfos); } }
        public string TotalPercentString { get { return StatisticsUtils.TotalPercentString(categoryInfos, Scale); } }
        public decimal TotalResponse { get { return CategoryInfo.TotalFrequency(categoryInfos); } }
    }
}
