using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using EDO.Core.Util;

namespace EDO.Main
{
    public class AboutWindowVM
    {
        public AboutWindowVM()
        {
        }

        public string Version
        {
            get
            {
                return $"Version {ApplicationDetails.ProductVersion}";
            }
        }

        public string BuildVersion
        {
            get
            {
                return $"(Build {ApplicationDetails.BuildVersion})";
            }
        }

        public string Name
        {
            get
            {
                return ApplicationDetails.ProductTitle;
            }
        }

        public string Copyright
        {
            get
            {
                return ApplicationDetails.CopyRight;
            }
        }
    }
}
