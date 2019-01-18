using EDO.Core.Model;
using EDO.Main;
using System.Collections.Generic;

namespace EDO.Core.IO
{
    interface IDDIWriter
    {
        void WriteStudyUnit(string fileName, StudyUnitVM studyUnit);
        void WriteGroup(string fileName, GroupVM group);
        EDOConfig Config
        {
            set;
            get;
        }
    }
}
