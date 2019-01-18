using System;
using EDO.Main;
using EDO.Core.Model;
using static EDO.Core.IO.DDIIO;

namespace EDO.Core.IO
{
    public class DDISerializer
    {
        public DDISerializer(DDIVersion ddiVersion)
        {
            DDIVersion = ddiVersion;
            switch (DDIVersion)
            {
                case DDIVersion.DDI2_5:
                    writer = new DDI2Writer();
                    reader = new DDI2Reader();
                    break;
                case DDIVersion.DDI3_1:
                    writer = new DDI3Writer();
                    reader = new DDI3Reader();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        public DDIVersion DDIVersion { get; }
        private IDDIWriter writer;
        private IDDIReader reader;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="config"></param>
        /// <param name="vm"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException" />
        public ValidationResult Export(string fileName, EDOConfig config, object vm)
        {
            writer.Config = config;
            switch (vm)
            {
                case StudyUnitVM studyUnit:
                    writer.WriteStudyUnit(fileName, studyUnit);
                    break;
                case GroupVM group:
                    writer.WriteGroup(fileName, group);
                    break;
                default:
                    throw new ArgumentException();
            }
            return (writer as DDIIO).Validate(fileName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        /// <exception cref="DDIIO.ValidationException" />
        /// <exception cref="System.Xml.XmlException" />
        public EDOModel Import(string fileName)
        {
            return reader.Read(fileName);
        }

        public DDIImportOption GenerateImportOption()
        {
            return reader.GenerateImportOption();
        }

        public void MergeStudyUnit(StudyUnit fromStudyUnit, StudyUnit toStudyUnit, DDIImportOption importOption)
        {
            reader.MergeStudyUnit(fromStudyUnit, toStudyUnit, importOption);
        }

    }
}
