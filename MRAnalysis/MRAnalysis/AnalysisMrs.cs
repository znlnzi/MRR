using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MRAnalysis.Common;
using MRAnalysis.Model;

namespace MRAnalysis
{
    public partial class AnalysisMrs : BaseControl
    {

        private Dictionary<string, string> _dicMrNameCN = new Dictionary<string, string>()
        {
            {"MR.RSRP","广东省_LTE-P01_参考信号接收功率" },
            {"MR.RSRQ","广东省_LTE-P02_参考信号接收质量" },
            {"MR.Tadv","广东省_LTE-P03_时间提前量" },
            {"MR.PowerHeadRoom","广东省_LTE-P04_UE发射功率余量" },
            {"MR.ReceivedIPower","广东省_LTE-P05_ENB接收干扰功率" },
            {"MR.AOA","广东省_LTE-P06_ENB天线到达角" },
            {"MR.PacketLossRateULQci","广东省_LTE-P07_上行丢包率" },
            {"MR.PacketLossRateDLQci","广东省_LTE-P08_下行丢包率" },
            {"MR.SinrUL","广东省_LTE-P09_上行信噪比" },
            {"MR.eNBRxTxTimeDiff","广东省_LTE-P13_eNB收发时间差" },
        };
        private DataTable _statisticeTable;
        private List<DataTable> _lstTable; 

        public AnalysisMrs()
        {
            InitializeComponent();
            _fileType = EnumHelper.FileType.MRS;
        }

        public override void Analysis(string file)
        {
            var fileInfo = new FileInfo(file);
            var fileName = fileInfo.Name;
            if (fileInfo.Length == 0)
            {
                return;
            }
            try
            {
                XmlHelper xmlReader = new XmlHelper();
                xmlReader.Paeser(file);               
                string enbId = xmlReader.GetEnbId();
                for (var i = 0; i < 27; i++)
                {
                    try
                    {
                        var smrList = xmlReader.GetSmrList(i);
                        _dicMrs = xmlReader.GetMrsDic(_dicMrs, enbId, i, smrList);
                    }
                    catch (Exception e)
                    {
                        LogHelper.Log(this, new Log() { Level = EnumHelper.State.Info, Message = "解析文件【" + fileName + "】失败：" + e.Message });
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Log(this, new Log() { Level = EnumHelper.State.Info, Message = "解析文件失败【" + fileName + "】:" + e.Message });
            }
        }

        public override void Export()
        {
            CreateTale();
            try
            {
                var dir = Path.Combine(_outputPath, DateTime.Now.ToString("yyyyMMddHHmmss"));
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                foreach (var table in _lstTable)
                {
                    var fileName = dir + @"\" + table.TableName + ".csv";
                    try
                    {
                        LogHelper.Log(this, new Log() { Level = EnumHelper.State.Info, Message = "导出文件【" + fileName + "】成功" });
                        ExcelHelper.DataTableToCsv(table, fileName);
                    }
                    catch (Exception e)
                    {
                        LogHelper.Log(this, new Log() { Level = EnumHelper.State.Error, Message = "导出文件【" + fileName + "】失败：" + e.Message });
                    }
                }
                var statisticeFileName = dir + @"\" + _statisticeTable.TableName + ".csv";
                try
                {                   
                    LogHelper.Log(this, new Log() { Level = EnumHelper.State.Info, Message = "导出文件【" + statisticeFileName + "】成功" });
                    ExcelHelper.DataTableToCsv(_statisticeTable, statisticeFileName);
                }
                catch(Exception e)
                {
                    LogHelper.Log(this, new Log() { Level = EnumHelper.State.Error, Message = "导出文件【" + statisticeFileName + "】失败：" + e.Message });
                }
                LogHelper.Log(this, new Log() { Level = EnumHelper.State.Info, Message = "导出文件成功" });
            }
            catch (Exception e)
            {
                LogHelper.Log(this, new Log() { Level = EnumHelper.State.Error, Message = "导出文件失败" });
            }
        }

        private int SumValue(int start,int end,List<int> countLst)
        {
            int result = 0;
            for(var i =start;i<end;i++)
            {
                result += countLst[i];
            }
            return result;
        }

        private void CreateTale()
        {
            _statisticeTable = new DataTable("汇总");
            _statisticeTable.Columns.Add("地市");
            _statisticeTable.Columns.Add("CGI");
            _statisticeTable.Columns.Add("OBJECTID");
            _statisticeTable.Columns.Add("MRS总采样点数");
            _statisticeTable.Columns.Add("MRS大于负105DBM的采样点数");
            _statisticeTable.Columns.Add("MRS大于负110DBM的采样点数");
            _statisticeTable.Columns.Add("MRS覆盖率大于负105DBM");
            _statisticeTable.Columns.Add("MRS覆盖率大于负110DBM");
            _statisticeTable.Columns.Add("服务小区电平(MRS)");
            _statisticeTable.Columns.Add("上行干扰百分比");
            _statisticeTable.Columns.Add("SINR平均值");
            _statisticeTable.Columns.Add("UE发射功率余量平均值");
            _statisticeTable.Columns.Add("弱覆盖小区(MRS)");
            _statisticeTable.Columns.Add("高干扰小区");
            _statisticeTable.Columns.Add("MRS总值");
            _statisticeTable.Columns.Add("SINR总采样点");
            _statisticeTable.Columns.Add("SINR总值");
            _statisticeTable.Columns.Add("UE发射功率余量总采样点");
            _statisticeTable.Columns.Add("UE发射功率余量总值");
            _statisticeTable.Columns.Add("eNB接收干扰功率总采样点");
            _statisticeTable.Columns.Add("eNB接收干扰功率大于-105采样点");
            var dicStatistice = new Dictionary<int, DataRow>();
            _lstTable = new List<DataTable>();
            foreach (var mrName in _dicMrs.Keys)
            {
                if (!_dicMrNameCN.ContainsKey(mrName))
                {
                    continue;
                }
                string mrNameCn = _dicMrNameCN[mrName];
                var table = new DataTable(mrNameCn);
                table.Columns.Add("日期");
                table.Columns.Add("省份");
                table.Columns.Add("地市");
                table.Columns.Add("区县");
                table.Columns.Add("ENODEB_ID");
                table.Columns.Add("EUTRANCELL_ID");
                bool isCreateTable = true;
                foreach (var mrs in _dicMrs[mrName].Values)
                {
                    var smrLst = mrs.SmrNameLst;
                    var qci = mrs.Qci;
                    var enbId = mrs.EnbId;
                    var cellId = mrs.CellId;
                    var countLst = mrs.CountList;
                    var count = mrs.Count;
                    var value = mrs.Value;
                    if (isCreateTable)
                    {
                        if (qci != "")
                        {
                            table.Columns.Add("QCI");
                        }
                        foreach (var smr in smrLst)
                        {
                            table.Columns.Add(smr);
                        }
                        table.Columns.Add("总采样点");
                        table.Columns.Add("平均值");
                        isCreateTable = false;
                    }

                    if(!dicStatistice.ContainsKey(cellId))
                    {
                        var tempRow = _statisticeTable.NewRow();
                        tempRow["地市"] = "江门";
                        tempRow["CGI"] = "460-00-" + enbId;
                        tempRow["OBJECTID"] = cellId.ToString();
                        tempRow["MRS总采样点数"] = 0;
                        tempRow["服务小区电平(MRS)"] = 0;
                        tempRow["MRS大于负105DBM的采样点数"] = 0;
                        tempRow["MRS大于负110DBM的采样点数"] = 0;
                        tempRow["MRS覆盖率大于负105DBM"] = 0;
                        tempRow["MRS覆盖率大于负110DBM"] = 0;
                        tempRow["弱覆盖小区(MRS)"] = 0;
                        tempRow["SINR平均值"] = 0;
                        tempRow["UE发射功率余量平均值"] = 0;
                        tempRow["上行干扰百分比"] = 0;
                        tempRow["高干扰小区"] = 0;
                        tempRow["MRS总值"] = 0;
                        tempRow["SINR总采样点"] = 0;
                        tempRow["SINR总值"] = 0;
                        tempRow["UE发射功率余量总采样点"] = 0;
                        tempRow["UE发射功率余量总值"] = 0;
                        tempRow["eNB接收干扰功率总采样点"] = 0;
                        tempRow["eNB接收干扰功率大于-105采样点"] = 0;
                        _statisticeTable.Rows.Add(tempRow);
                        dicStatistice.Add(cellId, tempRow);
                    }

                    var statisticeRow = dicStatistice[cellId];
                    switch(mrName)
                    {
                        case "MR.RSRP":
                            statisticeRow["MRS总采样点数"] = Convert.ToInt32(statisticeRow["MRS总采样点数"]) + count;
                            statisticeRow["MRS总值"] = Convert.ToDouble(statisticeRow["MRS总值"]) + value;
                            statisticeRow["服务小区电平(MRS)"] = Convert.ToDouble(statisticeRow["MRS总值"]) / Convert.ToInt32(statisticeRow["MRS总采样点数"]);
                            var rsrp105 = mrs.RSRP105;
                            var rsrp110 = mrs.RSRP110;
                            statisticeRow["MRS大于负105DBM的采样点数"] = Convert.ToInt32(statisticeRow["MRS大于负105DBM的采样点数"]) + rsrp105;
                            statisticeRow["MRS大于负110DBM的采样点数"] = Convert.ToInt32(statisticeRow["MRS大于负110DBM的采样点数"]) + rsrp110;
                            statisticeRow["MRS覆盖率大于负105DBM"] = (double)Convert.ToInt32(statisticeRow["MRS大于负105DBM的采样点数"]) / Convert.ToInt32(statisticeRow["MRS总采样点数"]);
                            statisticeRow["MRS覆盖率大于负110DBM"] = (double)Convert.ToInt32(statisticeRow["MRS大于负105DBM的采样点数"]) / Convert.ToInt32(statisticeRow["MRS总采样点数"]);
                            if((1- (double)rsrp110 / count)*100>10)
                            {
                                statisticeRow["弱覆盖小区(MRS)"] = Convert.ToInt32(statisticeRow["弱覆盖小区(MRS)"]) + 1;
                            }
                            break;
                        case "MR.SinrUL":
                            statisticeRow["SINR总采样点"] = Convert.ToInt32(statisticeRow["SINR总采样点"]) + count;
                            statisticeRow["SINR总值"] = Convert.ToDouble(statisticeRow["SINR总值"]) + value;
                            statisticeRow["SINR平均值"] = Convert.ToDouble(statisticeRow["SINR总值"]) / Convert.ToInt32(statisticeRow["SINR总采样点"]);
                            break;
                        case "MR.PowerHeadRoom":
                            statisticeRow["UE发射功率余量总采样点"] = Convert.ToInt32(statisticeRow["UE发射功率余量总采样点"]) + count;
                            statisticeRow["UE发射功率余量总值"] = Convert.ToDouble(statisticeRow["UE发射功率余量总值"]) + value;
                            statisticeRow["UE发射功率余量平均值"] = Convert.ToDouble(statisticeRow["UE发射功率余量总值"]) / Convert.ToInt32(statisticeRow["UE发射功率余量总采样点"]);
                            break;
                        case "MR.ReceivedIPower":
                            statisticeRow["eNB接收干扰功率总采样点"] = Convert.ToInt32(statisticeRow["eNB接收干扰功率总采样点"]) + count;
                            statisticeRow["eNB接收干扰功率大于-105采样点"] = Convert.ToInt32(statisticeRow["eNB接收干扰功率大于-105采样点"]) + mrs.Enbrip105;
                            statisticeRow["上行干扰百分比"] = (double)Convert.ToInt32(statisticeRow["eNB接收干扰功率大于-105采样点"]) / Convert.ToInt32(statisticeRow["eNB接收干扰功率总采样点"]) * 100;
                            if(((double)mrs.Enbrip105 / count)*100>5)
                            {
                                statisticeRow["高干扰小区"] = Convert.ToInt32(statisticeRow["高干扰小区"]) + 1;
                            }
                            break;
                    }

                    var row = table.NewRow();
                    row["日期"] = "";
                    row["省份"] = "广东";
                    row["地市"] = "江门";
                    row["区县"] = "江门";
                    row["ENODEB_ID"] = "460-00-" + enbId;
                    row["EUTRANCELL_ID"] = cellId%256;
                    if (qci != "")
                    {
                        row["QCI"] = qci;
                    }
                    for (var i = 0; i < smrLst.Count; i++)
                    {
                        var smr = smrLst[i];
                        row[smr] = countLst[i];
                    }
                    row["总采样点"] = count;
                    if (count != 0)
                    {
                        row["平均值"] = value / count;
                    }
                    else
                    {
                        row["平均值"] = 0;
                    }
                    table.Rows.Add(row);
                }

                _lstTable.Add(table);
            }

            _statisticeTable.Columns.Remove("MRS总值");
            _statisticeTable.Columns.Remove("SINR总采样点");
            _statisticeTable.Columns.Remove("SINR总值");
            _statisticeTable.Columns.Remove("UE发射功率余量总采样点");
            _statisticeTable.Columns.Remove("UE发射功率余量总值");
            _statisticeTable.Columns.Remove("eNB接收干扰功率总采样点");
            _statisticeTable.Columns.Remove("eNB接收干扰功率大于-105采样点");
        }
    }
}
