using EDO.Core.Model;

namespace EDO.Core.IO
{
    public interface IDDIReader
    {
        DDIImportOption GenerateImportOption();

        EDOModel Read(string path);

        void MergeStudyUnit(StudyUnit newStudyUnit, StudyUnit curStudyUnit, DDIImportOption importOption);
    }
}
