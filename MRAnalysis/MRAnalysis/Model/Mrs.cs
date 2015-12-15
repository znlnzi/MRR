using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRAnalysis.Model
{
    public class Mrs:BaseEntity
    {
        /// <summary>
        /// Mr名称
        /// </summary>
        public string MrName { get; set; }

        /// <summary>
        /// Smr列表
        /// </summary>
        public List<string> SmrNameLst { get; set; } 

        /// <summary>
        /// Smr的采样点
        /// </summary>
        public List<int> CountList { get; set; }

        /// <summary>
        /// 总采样点
        /// </summary>
        public int Count
        {
            get
            {
                return CountList.Sum();
            }
        }

        /// <summary>
        /// 总值
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// 上下行的Qci
        /// </summary>
        public string Qci { get; set; }

        /// <summary>
        /// RSRP大于负105dbm
        /// </summary>
        public int RSRP105
        {
            get
            {
                var value = 0;
                for (var i = 12; i < CountList.Count; i++)
                {
                    value += CountList[i];
                }

                return value;
            }
        }

        /// <summary>
        /// RSRP大于负110dbm
        /// </summary>
        public int RSRP110
        {
            get
            {
                var value = 0;
                for (var i = 7; i < CountList.Count; i++)
                {
                    value += CountList[i];
                }

                return value;
            }
        }

        /// <summary>
        /// Enbrip大于负105dbm
        /// </summary>
        public int Enbrip105
        {
            get
            {
                var value = 0;
                for (var i = 22; i < CountList.Count; i++)
                {
                    value += CountList[i];
                }

                return value;
            }
        }
    }
}
