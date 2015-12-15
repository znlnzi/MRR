using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MRAnalysis.Model
{
    public class Ftp
    {
        /// <summary>
        /// 域名
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UsrName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 端口号
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// 根节点
        /// </summary>
        public string RootPath { get; set; }
    }
}
