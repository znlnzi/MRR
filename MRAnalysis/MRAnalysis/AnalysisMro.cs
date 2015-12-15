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
    public partial class AnalysisMro : BaseControl
    {
        public AnalysisMro()
        {
            InitializeComponent();
            _fileType=EnumHelper.FileType.MRO;
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
                try
                {
                    var smrList = xmlReader.GetSmrList(0);
                    _dicMro = xmlReader.GetMroDic(_dicMro, enbId, smrList);
                }
                catch (Exception e)
                {
                    LogHelper.Log(this, new Log() { Level = EnumHelper.State.Info, Message = "解析文件【" + fileName + "】失败：" + e.Message });
                }
            }
            catch (Exception e)
            {
                LogHelper.Log(this, new Log() { Level = EnumHelper.State.Info, Message = "解析文件失败【" + fileName + "】:" + e.Message });
            }
        }

        public override void Export()
        {
            var table = CreateTale();
            try
            {
                var dir = Path.Combine(_outputPath, DateTime.Now.ToString("yyyyMMddHHmmss"));
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

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
                LogHelper.Log(this, new Log() { Level = EnumHelper.State.Info, Message = "导出文件成功" });
            }
            catch (Exception e)
            {
                LogHelper.Log(this, new Log() { Level = EnumHelper.State.Error, Message = "导出文件失败:"+e.Message });
            }
        }

        private DataTable CreateTale()
        {
            var table = new DataTable("三大运营商网络覆盖率统计");
            table.Columns.Add("移动小区");
            table.Columns.Add("小区名称");
            table.Columns.Add("电平(dBm)");
            table.Columns.Add("采样点");
            table.Columns.Add(">=-100采样点");
            table.Columns.Add(">=-110采样点");
            table.Columns.Add(">=-100采样点占比");
            table.Columns.Add(">=-110采样点占比");
            table.Columns.Add("联通电平(dBm)");
            table.Columns.Add("联通采样点");
            table.Columns.Add("联通>=-100采样点");
            table.Columns.Add("联通>=-110采样点");
            table.Columns.Add("联通>=-100采样点占比");
            table.Columns.Add("联通>=-110采样点占比");
            table.Columns.Add("电信电平(dBm)");
            table.Columns.Add("电信采样点");
            table.Columns.Add("电信>=-100采样点");
            table.Columns.Add("电信>=-110采样点");
            table.Columns.Add("电信>=-100采样点占比");
            table.Columns.Add("电信>=-110采样点占比");
            foreach (var mro in _dicMro.Values)
            {
                var row = table.NewRow();
                row["移动小区"] = mro.Earfcn + "-" + mro.ScPci;
                row["采样点"] = mro.Count;
                row[">=-100采样点"] = mro.Rsrp100;
                row[">=-110采样点"] = mro.Rsrp110;
                if (mro.Count != 0)
                {
                    row["电平(dBm)"] = (double)mro.Level / mro.Count;
                    row[">=-100采样点占比"] = ((double) mro.Rsrp100/mro.Count).ToString("P");
                    row[">=-110采样点占比"] = ((double) mro.Rsrp110/mro.Count).ToString("P");
                }

                row["联通采样点"] = mro.UnicomCount;
                row["联通>=-100采样点"] = mro.UnicomRsrp100;
                row["联通>=-110采样点"] = mro.UnicomRsrp110;
                if (mro.UnicomCount != 0)
                {
                    row["联通电平(dBm)"] = (double)mro.UnicomLevel/mro.UnicomCount;
                    row["联通>=-100采样点占比"] = ((double)mro.UnicomRsrp100 / mro.UnicomCount).ToString("P");
                    row["联通>=-110采样点占比"] = ((double)mro.UnicomRsrp110 / mro.UnicomCount).ToString("P");
                }

                row["电信采样点"] = mro.TeleComCount;
                row["电信>=-100采样点"] = mro.TelecomRsrp100;
                row["电信>=-110采样点"] = mro.TelecomRsrp110;
                if (mro.TeleComCount != 0)
                {
                    row["电信电平(dBm)"] = (double)mro.TelecomLevel/mro.TeleComCount;
                    row["电信>=-100采样点占比"] = ((double)mro.TelecomRsrp100 / mro.TeleComCount).ToString("P");
                    row["电信>=-110采样点占比"] = ((double)mro.TelecomRsrp110 / mro.TeleComCount).ToString("P");
                }

                table.Rows.Add(row);
            }

            return table;
        }
    }
}
