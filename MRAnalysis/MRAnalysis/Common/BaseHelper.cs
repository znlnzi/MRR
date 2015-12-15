using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using MRAnalysis.Model;

namespace MRAnalysis.Common
{
    public class BaseHelper
    {
        #region 成员变量

        private static List<float> _rsrpValueLst;
        private static List<float> _rsrqValueLst;
        private static List<float> _tadvValueLst;
        private static List<float> _powerHeadRoomValueLst;
        private static List<float> _receivedIPowerValueLst;
        private static List<float> _aoaValueLst;
        private static List<float> _packetLossRateUdlQciValueLst;
        private static List<float> _sinrUlValueLst;
        private static List<float> _ripprbValueLst;
        private static List<float> _enbRxTxTimeDiffValueLst;
        private static Dictionary<string,Ftp> _ftpDic = new Dictionary<string, Ftp>();
        private static int _ftpCount = 1;

        #endregion

        #region 属性

        #region Mrs

        /// <summary>
        /// RSRP每个区间的平均值
        /// </summary>
        public static List<float> RsrpValueList => _rsrpValueLst ?? (_rsrpValueLst = GetRsrpAValueLst());

        /// <summary>
        /// RSRQ每个区间的平均值
        /// </summary>
        public static List<float> RsrqValueList => _rsrqValueLst ?? (_rsrqValueLst = GetRsrqValueLst());

        /// <summary>
        /// TADV每个区间的平均值
        /// </summary>
        public static List<float> TadvValueList => _tadvValueLst ?? (_tadvValueLst = GetTadvValueLst());

        /// <summary>
        /// PowerHeadRoom每个区间的平均值
        /// </summary>
        public static List<float> PowerHeadRoomValueList
            => _powerHeadRoomValueLst ?? (_powerHeadRoomValueLst = GetPowerHeadRoomValueLst());

        /// <summary>
        /// ReceivedIPower每个区间的平均值
        /// </summary>
        public static List<float> ReceivedIPowerValueList
            => _receivedIPowerValueLst ?? (_receivedIPowerValueLst = GetReceivedIPowerValueLst());

        /// <summary>
        /// AOA每个区间的平均值
        /// </summary>
        public static List<float> AoaValueList => _aoaValueLst ?? (_aoaValueLst = GetAoaValueLst());

        /// <summary>
        /// PacketLossRateULQciX 或 PacketLossRateDLQciX每个区间的平均值
        /// </summary>
        public static List<float> PacketLossRateUdlQciValueList
            => _packetLossRateUdlQciValueLst ?? (_packetLossRateUdlQciValueLst = GetPacketLossRateUdlQciValueLst());

        /// <summary>
        /// SinrUL每个区间的平均值
        /// </summary>
        public static List<float> SinrUlValueList => _sinrUlValueLst ?? (_sinrUlValueLst = GetSinrUlValueLst());

        /// <summary>
        /// RIPPRB每个区间的平均值
        /// </summary>
        public static List<float> RipprbValueList => _ripprbValueLst ?? (_ripprbValueLst = GetRipprbValueLst());

        /// <summary>
        /// EnbRxTxTimeDiff每个区间的平均值
        /// </summary>
        public static List<float> EnbRxTxTimeDiffValueList
            => _enbRxTxTimeDiffValueLst ?? (_enbRxTxTimeDiffValueLst = GetEnbRxTxTimeDiffValueLst());

        #endregion

        public static Dictionary<string,Ftp> FtpDic
        {
            get
            {
                if (_ftpDic.Count == 0)
                {
                    _ftpDic = new Dictionary<string, Ftp>();
                    for (var i = 1; i <= _ftpCount; i++)
                    {
                        var ftp = new Ftp()
                        {
                            Host = ConfigurationManager.AppSettings["Host" + i],
                            UsrName = ConfigurationManager.AppSettings["User" + i],
                            Password = ConfigurationManager.AppSettings["Pwd" + i],
                            Port = Convert.ToInt32(ConfigurationManager.AppSettings["Port" + i]),
                            RootPath = ConfigurationManager.AppSettings["RootPath" + i]
                        };
                        _ftpDic.Add(ConfigurationManager.AppSettings["Host" + i], ftp);
                    }
                }
                return _ftpDic;
            }
        } 
        #endregion

        #region 方法

        /*
        从-∞到-120dBm一个区间,对应MR.RSRP.00;
        从-120 dBm到-115 dBm为一个区间,对应MR.RSRP.01;从-115dBm到-80dBm每1dB一个区间,对 应MR.RSRP.02到MR.RSRP.36;
        从-80dBm到-60dBm每2dB一个区间,对应MR.RSRP.37 到MR.RSRP.46;大于-60dBm一个区间,对应MR.RSRP.47,依此类推。
        */

        /// <summary>
        /// 获取RSRP每个区间的平均值
        /// </summary>
        /// <returns></returns>
        private static List<float> GetRsrpAValueLst()
        {
            var rsrpVerages = new List<float>();

            for (var i = 0; i < 48; i++)
            {
                float verage;
                switch (i)
                {
                    case 0:
                        verage = -(140 + 120)/2;
                        break;
                    case 1:
                        verage = -(120 + 115)/2;
                        break;
                    case 2:
                        verage = -(115 + 114)/2;
                        break;
                    default:
                        if (i > 2 && i < 37)
                        {
                            verage = rsrpVerages[i - 1] + 1;
                        }
                        else if (i == 37)
                        {
                            verage = -(80 + 78)/2;
                        }
                        else if (i > 37 && i < 47)
                        {
                            verage = rsrpVerages[i - 1] + 2;
                        }
                        else
                        {
                            verage = -(60 + 44)/2;
                        }
                        break;
                }

                rsrpVerages.Add(verage);
            }

            return rsrpVerages;
        }

        /*
        从-∞到-19.5dB为一个区间,对应MR.RSRQ.00;
        从-19.5到-3.5dB每1个dB一个区间,对应MR.RSRQ.01到MR.RSRQ.16;
        大于-3.5dB一个区间, 对应MR.RSRQ.17。
        */

        /// <summary>
        /// 获取RSRQ每个区间的平均值
        /// </summary>
        /// <returns></returns>
        private static List<float> GetRsrqValueLst()
        {
            var rsrqVerages = new List<float>();
            for (var i = 0; i < 18; i++)
            {
                float verage;
                switch (i)
                {
                    case 0:
                    case 17:
                        verage = 0;
                        break;
                    default:
                        if (i == 1)
                        {
                            verage = (float) (-(19.5 + 18.5)/2);
                        }
                        else
                        {
                            verage = rsrqVerages[i - 1] + 1;
                        }
                        break;
                }

                rsrqVerages.Add(verage);
            }

            return rsrqVerages;
        }

        /*
        从0到192Ts每16Ts为一个区间,对应MR.Tadv.00到MR.Tadv.11;
        从192Ts到1024Ts每32Ts为一个区间,对应MR.Tadv.12到MR.Tadv.37; 
        从1024Ts到2048Ts每256Ts为一个区间,对应MR.Tadv.38到MR.Tadv.41;
        从2048Ts 到4096Ts每1048Ts为一个区间,对应MR. Tadv.42和MR.Tadv.43;
        大于4096Ts为一个 区间,对应MR.Tadv.44。
        */

        /// <summary>
        /// 获取TADV每个区间的平均值
        /// </summary>
        /// <returns></returns>
        private static List<float> GetTadvValueLst()
        {
            var tadvVerages = new List<float>();
            for (var i = 0; i < 45; i++)
            {
                float verage;
                switch (i)
                {
                    case 0:
                        verage = (0 + 16)/2;
                        break;
                    case 12:
                        verage = (192 + 224)/2;
                        break;
                    case 38:
                        verage = (1024 + 1280)/2;
                        break;
                    case 42:
                        verage = (2048 + 3072)/2;
                        break;
                    case 44:
                        verage = 0;
                        break;
                    default:
                        if (i > 0 && i < 12)
                        {
                            verage = tadvVerages[i - 1] + 16;
                        }
                        else if (i > 12 && 1 < 38)
                        {
                            verage = tadvVerages[i - 1] + 32;
                        }
                        else if (i > 38 && 1 < 42)
                        {
                            verage = tadvVerages[i - 1] + 256;
                        }
                        else
                        {
                            verage = tadvVerages[i - 1] + 1048;
                        }
                        break;
                }
                tadvVerages.Add(verage);
            }

            return tadvVerages;
        }

        /*
        从-23dB到40dB,1dB对应一个统计区间;
        大于40dB为一个 区间,对应MR.PowerHeadRoom.63。
        */

        /// <summary>
        /// 获取PowerHeadRoom每个区间的平均值
        /// </summary>
        /// <returns></returns>
        private static List<float> GetPowerHeadRoomValueLst()
        {
            var powerHeadRoomVerages = new List<float>();
            for (var i = 0; i < 64; i++)
            {
                float veage;
                switch (i)
                {
                    case 0:
                        veage = -(23 + 22)/2;
                        break;
                    case 63:
                        veage = 0;
                        break;
                    default:
                        veage = powerHeadRoomVerages[i - 1] + 1;
                        break;
                }
                powerHeadRoomVerages.Add(veage);
            }

            return powerHeadRoomVerages;
        }

        /*
        小于-126dBm为一个区间,对应MR.ReceivedIPower.00;
        从-126.0dBm到-75dBm每1dBm为一个区间,大于-75.0dBm为一个区间,对应 MR.ReceivedIPower.52,依此类推。
        */

        /// <summary>
        /// 获取ReceivedIPower每个区间的平均值
        /// </summary>
        /// <returns></returns>
        private static List<float> GetReceivedIPowerValueLst()
        {
            var receivedIPowerVerages = new List<float>();
            for (var i = 0; i < 53; i++)
            {
                float verage;
                switch (i)
                {
                    case 0:
                    case 52:
                        verage = 0;
                        break;
                    case 1:
                        verage = -(126 + 125)/2;
                        break;
                    default:
                        verage = receivedIPowerVerages[i - 1] + 1;
                        break;
                }
                receivedIPowerVerages.Add(verage);
            }

            return receivedIPowerVerages;
        }

        /*
        0度到小于5度为一个区间,对应MR.AOA.00;
        355度到小于360度为一个区间,对应MR.AOA.71,依此类推。
        */

        /// <summary>
        /// 获取AOA每个区间的平均值
        /// </summary>
        /// <returns></returns>
        private static List<float> GetAoaValueLst()
        {
            var aoaVerages = new List<float>();
            for (var i = 0; i < 72; i++)
            {
                float verage;
                switch (i)
                {
                    case 0:
                        verage = (0 + 5)/2;
                        break;
                    default:
                        verage = aoaVerages[i - 1] + 5;
                        break;
                }
                aoaVerages.Add(verage);
            }

            return aoaVerages;
        }

        /*
        0到2‰为一个区间,对应MR.PacketLossRateULQciX.00;
        2‰ 到 5‰ 为 一 个 区 间 , 对 应 MR.PacketLossRateULQciX.01;
        5‰ 到 10‰ 为 一 个 区 间 , 对 应 MR.PacketLossRateULQciX.02; 
        从 10‰ 到 100‰ 每 10‰ 为 一 个 区 间 , 对 应 MR.PacketLossRateULQciX.03到MR.PacketLossRateULQciX.11;
        从100‰到200‰每 20‰ 为 一 个 区 间 , 对 应 MR.PacketLossRateULQciX.12 到 MR.PacketLossRateULQciX.16;
        从 200‰ 到 500‰ 每 50‰ 为 一 个 区 间 , 对 应 MR.PacketLossRateULQciX.17到MR.PacketLossRateULQciX.22;
        从500‰到1000‰每 100‰ 为 一 个 区 间 , 对 应 MR.PacketLossRateULQciX.23 到 MR.PacketLossRateULQciX.27。
        */

        /// <summary>
        /// 获取PacketLossRateULQciX 或 PacketLossRateDLQciX每个区间的平均值
        /// </summary>
        /// <returns></returns>
        private static List<float> GetPacketLossRateUdlQciValueLst()
        {
            var packetLossRateUdlQciVerages = new List<float>();
            for (var i = 0; i < 28; i++)
            {
                float verage;
                switch (i)
                {
                    case 0:
                        verage = (0 + 2)/2;
                        break;
                    case 1:
                        verage = (2 + 5)/2;
                        break;
                    case 2:
                        verage = (5 + 10)/2;
                        break;
                    case 3:
                        verage = (10 + 20)/2;
                        break;
                    case 12:
                        verage = (100 + 120)/2;
                        break;
                    case 17:
                        verage = (200 + 250)/2;
                        break;
                    case 23:
                        verage = (500 + 600)/2;
                        break;
                    default:
                        if (i > 3 && i < 12)
                        {
                            verage = packetLossRateUdlQciVerages[i - 1] + 10;
                        }
                        else if (i > 12 && i < 17)
                        {
                            verage = packetLossRateUdlQciVerages[i - 1] + 20;
                        }
                        else if (i > 17 && i < 23)
                        {
                            verage = packetLossRateUdlQciVerages[i - 1] + 50;
                        }
                        else
                        {
                            verage = packetLossRateUdlQciVerages[i - 1] + 100;
                        }
                        break;
                }
                packetLossRateUdlQciVerages.Add(verage);
            }

            return packetLossRateUdlQciVerages;
        }

        /*
        SINR小于-10dB,对应MR.SinrUL.00;
        从-10dB到25dB,每 1dB 为 一 个 区 间 , 对 应 MR.SinrUL.01 到 MR.SinrUL.35 ;
        大 于 25dB , 对 应 MR.SinrUL.36。
        */

        /// <summary>
        /// 获取SinrUL每个区间的平均值
        /// </summary>
        /// <returns></returns>
        private static List<float> GetSinrUlValueLst()
        {
            var sinrUlVerage = new List<float>();
            for (var i = 0; i < 37; i++)
            {
                float verage;
                switch (i)
                {
                    case 0:
                    case 36:
                        verage = 0;
                        break;
                    case 1:
                        verage = -(10 + 9)/2;
                        break;
                    default:
                        verage = sinrUlVerage[i - 1] + 1;
                        break;
                }
                sinrUlVerage.Add(verage);
            }

            return sinrUlVerage;
        }

        /*
        小于-126dBm为一个区间,对应MR.RIPPRB.00;
        从-126.0dBm到-75dBm每1dBm为一个区间,大于-75.0dBm为一个区间,对应 MR.RIPPRB.52,依此类推。
        */

        /// <summary>
        /// 获取RIPPRB每个区间的平均值
        /// </summary>
        /// <returns></returns>
        private static List<float> GetRipprbValueLst()
        {
            var ripprbVerages = new List<float>();
            for (var i = 0; i < 53; i++)
            {
                float verage;
                switch (i)
                {
                    case 0:
                    case 52:
                        verage = 0;
                        break;
                    case 1:
                        verage = -(126 + 125)/2;
                        break;
                    default:
                        verage = ripprbVerages[i - 1] + 1;
                        break;
                }
                ripprbVerages.Add(verage);
            }

            return ripprbVerages;
        }

        /*
        496Ts到-480Ts 为一个区间,对应MR.eNBRxTxTimeDiff.00;
        496Ts到512Ts为一个区间,对应MR eNBRxTxTimeDiff.63, 每16Ts为一个区间,依此类推。
        */

        /// <summary>
        /// 获取EnbRxTxTimeDiff每个区间的平均值
        /// </summary>
        /// <returns></returns>
        private static List<float> GetEnbRxTxTimeDiffValueLst()
        {
            var enbRxTxTimeDiffVerages = new List<float>();
            for (var i = 0; i < 64; i++)
            {
                float verage;
                switch (i)
                {
                    case 0:
                        verage = -(496 + 480)/2;
                        break;
                    default:
                        verage = enbRxTxTimeDiffVerages[i - 1] + 16;
                        break;
                }

                enbRxTxTimeDiffVerages.Add(verage);
            }

            return enbRxTxTimeDiffVerages;
        }

        #endregion

    }
}
