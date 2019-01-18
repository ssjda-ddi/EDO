using EDO.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EDO.Core.IO
{
    class DDIGroupFileDialogResult: FileDialogResult
    {
        public static readonly string FILTER = Resources.DDIGroupFilter;

        public DDIGroupFileDialogResult() { }

        public override int FilterIndex
        {
            set
            {
                base.FilterIndex = value;
                switch (value)
                {
                    case 1:
                        Version = DDIVersion.DDI3_1;
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }
        public DDIVersion Version { get; set; }
    }
}
