using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EDO.Core.Util.Statistics
{
    public interface ICode
    {
        string Value { get; }
        string Label { get; }
        bool IsMissingValue { get; }
    }
}
