using EDO.Properties;
using System;

namespace EDO.Core.IO
{
    class DDIFileDialogResult : FileDialogResult
    {
        public static readonly string FILTER = Resources.DDIFileFilter;

        public DDIFileDialogResult() { }

        public override int FilterIndex {
            set
            {
                base.FilterIndex = value;
                switch (value)
                {
                    case 1:
                        Version = DDIVersion.DDI3_1;
                        break;
                    case 2:
                        Version = DDIVersion.DDI2_5;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        public DDIVersion Version { get; set; }
    }
}
