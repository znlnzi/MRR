using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MRAnalysis.Common;

namespace MRAnalysis.Model
{
    [Serializable]
    public class Log
    {
        public string Message { get; set; }

        public EnumHelper.State Level { get; set; }

        public override string ToString()
        {
            return Message;
        }
    }
}
