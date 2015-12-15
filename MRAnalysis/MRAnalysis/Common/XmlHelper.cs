using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using MRAnalysis.Model;
using MRAnalysis.XML;

namespace MRAnalysis.Common
{
    public class XmlHelper
    {
        private string _rootNode = "bulkPmMrDataFile";
        private string _secondNode = "eNB";
        private string _thirdNode = "measurement";
        private string _smrNode = "smr";
        private string _valueNode = "object";
        private XmlNode _xmlNode;

        /// <summary>
        /// 解析xml文件
        /// </summary>
        /// <param name="path"></param>
        public void Paeser(string path)
        {
            string str = File.ReadAllText(path, Encoding.UTF8);   //读取XML文件
            var xmlParser = new XmlParser();
            var xn = xmlParser.Parse(str);
            _xmlNode = xn;
        }

        /// <summary>
        /// 获取enbId
        /// </summary>
        /// <returns></returns>
        public string GetEnbId()
        {
            var nodePath = string.Format("{0}>0>{1}>0", _rootNode, _secondNode);
            var temp = _xmlNode.GetNode(nodePath);
            var enbId = temp.GetValue("@id");

            return enbId;
        }

        /// <summary>
        /// 获取mrName
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetMrName(int index)
        {
            var nodePath = $"{_rootNode}>0>{_secondNode}>0>{_thirdNode}>{index}";
            var temp = _xmlNode.GetNode(nodePath);
            var mrName = temp.GetValue("@mrName");

            return mrName;
        }

        /// <summary>
        /// 获取smr列表
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public List<string> GetSmrList(int index)
        {
            var nodePath = $"{_rootNode}>0>{_secondNode}>0>{_thirdNode}>{index}>{_smrNode}>0";
            var smr = _xmlNode.GetValue(nodePath + ">_text");
            var smrList = new List<string>(smr.Split(' '));

            return smrList;
        }

        /// <summary>
        /// 获取Mrs数据
        /// </summary>
        /// <param name="dicMrs"></param>
        /// <param name="enbId"></param>
        /// <param name="index"></param>
        /// <param name="smrLst"></param>
        /// <returns></returns>
        public Dictionary<string, Dictionary<string,Mrs>> GetMrsDic(Dictionary<string, Dictionary<string,Mrs>> dicMrs, string enbId, int index, List<string> smrLst)
        {
            var mrName = GetMrName(index);
            var qci = string.Empty;
            if (mrName.Contains("Qci"))
            {
                qci = mrName.Substring(mrName.Length - 1, 1);
                for (var i = 0; i < smrLst.Count; i++)
                {
                    smrLst[i] = smrLst[i].Replace(mrName, mrName.Substring(0, mrName.Length - 1));
                }
                mrName = mrName.Substring(0, mrName.Length - 1);
            }
            var nodePath = $"{_rootNode}>0>{_secondNode}>0>{_thirdNode}>{index}>{_valueNode}";
            var nodeList = _xmlNode.GetNodeList(nodePath);
            if (!dicMrs.ContainsKey(mrName))
            {
                dicMrs.Add(mrName,new Dictionary<string, Mrs>());
            }
            for (var i = 0; i < nodeList.Count; i++)
            {
                try
                {
                    var objectNode = (XmlNode) nodeList[i];
                    var cellId = objectNode.GetValue("@id");
                    cellId = cellId.Split(':')[0];
                    var valueNodes = objectNode.GetNodeList("v");
                    foreach (XmlNode valueNode in valueNodes)
                    {
                        // 同个Object节点下可能存在多个v节点
                        if (!dicMrs[mrName].ContainsKey(cellId + qci))
                        {
                            var list = new List<int>();
                            for (var j = 0; j < smrLst.Count; j++)
                            {
                                list.Add(0);
                            }
                            dicMrs[mrName].Add(cellId + qci, new Mrs()
                            {
                                EnbId = enbId,
                                CellId = Convert.ToInt32(cellId),
                                MrName = mrName,
                                SmrNameLst = smrLst,
                                CountList = list,
                                Qci = qci
                            });
                        }
                        var mrs = dicMrs[mrName][cellId+qci];
                        var counts = valueNode.GetValue("_text"); //采样点数
                        var countLst = Str2Int(counts);
                        var value = CalulateValues(countLst, mrName);

                        var oldCountLst = mrs.CountList;
                        mrs.CountList = SumCountLst(oldCountLst, countLst);
                        mrs.Value += value;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            return dicMrs;
        }

        /// <summary>
        /// 采样点相加
        /// </summary>
        /// <param name="oldLst">已有的采样点列表</param>
        /// <param name="addLst">新增的采样点列表</param>
        /// <returns>新的采样点列表</returns>
        private List<int> SumCountLst(List<int> oldLst, List<int> addLst)
        {
            var newLst = new List<int>();
            for (var i = 0; i < oldLst.Count; i++)
            {
                newLst.Add(oldLst[i] + addLst[i]);
            }

            return newLst;
        }

        /// <summary>
        /// 字符串列表转为数值列表
        /// </summary>
        /// <param name="str">字符串列表(空格分割)</param>
        /// <returns>数值列表</returns>

        private List<int> Str2Int(string str)
        {
            var intLst = new List<int>();
            var strs = str.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < strs.Length; i++)
            {
                intLst.Add(Convert.ToInt32(strs[i]));
            }

            return intLst;
        } 

        /// <summary>
        /// 计算Mrs总值
        /// </summary>
        /// <param name="countLst">smr采样点</param>
        /// <param name="mrName">Mr名称</param>
        /// <returns></returns>
        private float CalulateValues(List<int> countLst, string mrName)
        {
            var valueLst = new List<float>();
            float value = 0;
            switch (mrName)
            {
                case "MR.RSRP":
                    valueLst = BaseHelper.RsrpValueList;
                    break;
                case "MR.RSRQ":
                    valueLst = BaseHelper.RsrqValueList;
                    break;
                case "MR.Tadv":
                    valueLst = BaseHelper.TadvValueList;
                    break;
                case "MR.PowerHeadRoom":
                    valueLst = BaseHelper.PowerHeadRoomValueList;
                    break;
                case "MR.ReceivedIPower":
                    valueLst = BaseHelper.ReceivedIPowerValueList;
                    break;
                case "MR.AOA":
                    valueLst = BaseHelper.AoaValueList;
                    break;
                case "MR.PacketLossRateULQci":
                    valueLst = BaseHelper.PacketLossRateUdlQciValueList;
                    break;
                case "MR.PacketLossRateDLQci":
                    valueLst = BaseHelper.PacketLossRateUdlQciValueList;
                    break;
                case "MR.SinrUL":
                    valueLst = BaseHelper.SinrUlValueList;
                    break;
                case "MR.eNBRxTxTimeDiff":
                    valueLst = BaseHelper.EnbRxTxTimeDiffValueList;
                    break;
                case "MR.RIPPRB":
                    valueLst = BaseHelper.RipprbValueList;
                    break;
            }

            for (var i = 0; i < countLst.Count; i++)
            {
                value += countLst[i]*valueLst[i];
            }

            return value;
        } 

        /// <summary>
        /// 获取Mro数据
        /// </summary>
        /// <param name="dicMro"></param>
        /// <param name="enbId"></param>
        /// <param name="smrList"></param>
        /// <returns></returns>
        public Dictionary<string, Mro> GetMroDic(Dictionary<string, Mro> dicMro, string enbId, List<string> smrList)
        {
            string nodePath = $"{_rootNode}>0>{_secondNode}>0>{_thirdNode}>0>{_valueNode}";
            var nodeList = _xmlNode.GetNodeList(nodePath);
            for (var i = 0; i < nodeList.Count; i++)
            {
                var objectNode = (XmlNode)nodeList[i];
                var cellId = objectNode.GetValue("@id");
                var valueNodes = objectNode.GetNodeList("v");
                // 同个Object节点下可能存在多个v节点
                foreach (XmlNode valueNode in valueNodes)
                {
                    var value = valueNode.GetValue("_text");
                    var dic = ConvertToDic(value, smrList);
                    var fcn = dic["MR.LteScEarfcn"];
                    var pci = dic["MR.LteScPci"];
                    var rsrp = dic["MR.LteScRSRP"];
                    var ncfcn = dic["MR.LteNcEarfcn"];
                    var ncpci = dic["MR.LteNcPci"];
                    var ncrsrp = dic["MR.LteNcRSRP"];
                    if (!dicMro.ContainsKey(cellId))
                    {
                        dicMro.Add(cellId, new Mro() { CellId = Convert.ToInt32(cellId), Earfcn = fcn, ScPci = pci });
                    }
                    var mroEntity = dicMro[cellId];
                    mroEntity.Count++;
                    if (rsrp != "NIL")
                    {
                        var rsrpValue = Convert.ToInt32(rsrp);
                        mroEntity = CalculateLevel(mroEntity, rsrpValue, "");
                    }

                    var type = GetType(ncfcn, ncpci);
                    if (type != "")
                    {
                        var ncrsrpValue = Convert.ToInt32(ncrsrp);
                        mroEntity = CalculateLevel(mroEntity, ncrsrpValue, type);
                        if (type == "Unicom")
                        {
                            mroEntity.UnicomCount++;
                        }
                        else
                        {
                            mroEntity.TeleComCount++;
                        }
                    }
                }
            }

            return dicMro;
        }
        
        /// <summary>
        /// 计算电平
        /// </summary>
        private Mro CalculateLevel(Mro mroEntity, int rsrpValue, string type)
        {
            if (IsMoreThan100(rsrpValue))
            {
                switch (type)
                {
                    case "Unicom":
                        mroEntity.UnicomRsrp100++;
                        break;
                    case "TeleCom":
                        mroEntity.TelecomRsrp100++;
                        break;
                    case "":
                        mroEntity.Rsrp100++;
                        break;
                }
            }
            if (IsMoreThan110(rsrpValue))
            {
                switch (type)
                {
                    case "Unicom":
                        mroEntity.UnicomRsrp110++;
                        break;
                    case "TeleCom":
                        mroEntity.TelecomRsrp110++;
                        break;
                    case "":
                        mroEntity.Rsrp110++;
                        break;
                }
            }
            if (rsrpValue != 0)
            {
                switch (type)
                {
                    case "Unicom":
                        mroEntity.UnicomLevel += (-140) + (rsrpValue - 1);
                        break;
                    case "TeleCom":
                        mroEntity.TelecomLevel += (-140) + (rsrpValue - 1);
                        break;
                    case "":
                        mroEntity.Level += (-140) + (rsrpValue - 1);
                        break;
                }
            }

            return mroEntity;
        }

        /// <summary>
        /// 判断频点属于哪个运营商
        /// </summary>
        /// <param name="fcn"></param>
        /// <param name="pci"></param>
        /// <returns></returns>
        private string GetType(string fcn, string pci)
        {
            if (fcn == "1650")
            {
                return "Uncom";
            }
            else if (fcn == "1825")
            {
                return "Telecom";
            }

            return "";
        }

        /// <summary>
        /// 判断采样点是否电平>100dbm
        /// </summary>
        private bool IsMoreThan100(int value)
        {
            if (value >= 39)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 判断采样点是否电平>110dbm
        /// </summary>
        private bool IsMoreThan110(int value)
        {
            if (value >= 29)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// 将采样点和Smr列表转成字典
        /// </summary>
        private Dictionary<string, string> ConvertToDic(string values, List<string> smrList)
        {
            var dic = new Dictionary<string, string>();
            var valueStrs = values.TrimStart().TrimEnd().Split(' ');
            for (var i = 0; i < smrList.Count; i++)
            {
                var smr = smrList[i];
                var value = valueStrs[i];
                dic.Add(smr, value);
            }

            return dic;
        }
    }
}
