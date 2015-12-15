using System;
using System.Collections.Generic;
using System.Text;

namespace MRAnalysis.Model
{
    public class Mro:BaseEntity
    {
        #region 移动

        /// <summary>
        /// 移动fcn
        /// </summary>
        public string Earfcn { get; set; }

        /// <summary>
        /// 移动频点
        /// </summary>
        public string ScPci { get; set; }

        /// <summary>
        /// 移动总采样点
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 移动电平>100dbm的采样点
        /// </summary>
        public int Rsrp100 { get; set; }

        /// <summary>
        /// 移动电平>110dbm的采样点
        /// </summary>
        public int Rsrp110 { get; set; }

        /// <summary>
        /// 移动平均电平
        /// </summary>
        public int Level { get; set; }

        #endregion

        #region 联通

        /// <summary>
        /// 联通总采样点
        /// </summary>
        public int UnicomCount { get; set; }

        /// <summary>
        /// 联通电平>100dbm的采样点
        /// </summary>
        public int UnicomRsrp100 { get; set; }

        /// <summary>
        /// 联通电平>110dbm的采样点
        /// </summary>
        public int UnicomRsrp110 { get; set; }

        /// <summary>
        /// 联通平均电平
        /// </summary>
        public int UnicomLevel { get; set; }

        #endregion

        #region 电信

        /// <summary>
        /// 电信总采样点
        /// </summary>
        public int TeleComCount { get; set; }

        /// <summary>
        /// 电信电平>100dbm的采样点数
        /// </summary>
        public int TelecomRsrp100 { get; set; }


        /// <summary>
        ///  电信电平>110dbm的采样点数
        /// </summary>
        public int TelecomRsrp110 { get; set; }

        /// <summary>
        /// 电信平均电平
        /// </summary>
        public int TelecomLevel { get; set; }

        #endregion

    }
}
