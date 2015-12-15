using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRAnalysis.Common
{
    public class EnumHelper
    {
        public enum State
        {
            Debug = 0,
            Info = 1,
            Warn = 2,
            Error = 4
        }

        public enum FileType
        {
            MRS,
            MRO,
            MRE
        }
    }
}
