using System;
using System.Collections.Generic;
using System.Text;

namespace MRAnalysis.Model
{
    public class BaseEntity
    {
        /// <summary>
        /// 站点ID
        /// </summary>
        public string EnbId { get; set; }

        /// <summary>
        /// 小区ID
        /// </summary>
        public int CellId { get; set; }

        /// <summary>
        /// 日期
        /// </summary>
        public string Date { get; set; }
    }
}
