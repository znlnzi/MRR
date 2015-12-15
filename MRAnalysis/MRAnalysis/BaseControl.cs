using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using MRAnalysis.Common;
using MRAnalysis.Model;

namespace MRAnalysis
{
    public partial class BaseControl : LogControl
    {
        public struct DownLoad
        {
            public List<string> localDirs;
            public string host;
        };

        public bool _isAnalysis = false;
        public EnumHelper.FileType _fileType;
        public string _rootPath = Path.Combine(@"D:\", "DownLoad");
        public string _unZipPath = Path.Combine(@"D:\", "UnZip");
        public string _outputPath = Path.Combine(@"D:\", "OutPut");
        public List<string> _lstSequence = new List<string>();
        public List<string> _dateList = new List<string>();
        public Dictionary<string, List<string>> _dicLocalDir = new Dictionary<string, List<string>>();
        public Dictionary<string,List<string>> _dicSite = new Dictionary<string, List<string>>(); 
        public Dictionary<string,Dictionary<string,Mrs>> _dicMrs = new Dictionary<string, Dictionary<string, Mrs>>(); 
        public Dictionary<string,Mro> _dicMro = new Dictionary<string, Mro>();
        private Thread _analysisThread;
        private List<int> _lstThreadId = new List<int>();
        private int _downloadCount;
        private int _hasDownloadCount;
        private int _analysisCount;
        private int _hasAnalysis;
        private int _siteFileCount;
        private bool _isLocal;


        public BaseControl()
        {
            InitializeComponent();
            dgvLog.ColumnHeadersVisible = false;
            dgvLog.RowHeadersVisible = false;
            dgvLog.DataSource = LogEntityBindingSource;
            dgvLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLog.Columns[1].Visible = false;
        }

        private void Init()
        {
            _dateList = new List<string>();
            _lstSequence = new List<string>();
            _dicLocalDir = new Dictionary<string, List<string>>();
            _dicSite = new Dictionary<string, List<string>>();
            _dicMro = new Dictionary<string, Mro>();
            _lstThreadId = new List<int>();
            _downloadCount = 0;
            _hasDownloadCount = 0;
            _siteFileCount = 0;
            _analysisCount = 0;
        }

        delegate void ProgressDelegate(int progress,string type);

        private void SetProgress(int progress,string type)
        {
            if (this.InvokeRequired)
            {
                var progressDelegate = new ProgressDelegate(SetProgress);
                this.Invoke(progressDelegate, new object[] { progress,type });
            }
            else
            {
                if (type == "Download")
                {
                    progressBarEx1.Value = progress;
                }
                else
                {
                    progressBarEx2.Value = progress;
                }
            }
        }

        private void SetProgressMax(int max,string type)
        {
            if (this.InvokeRequired)
            {
                var progressDelegate = new ProgressDelegate(SetProgressMax);
                this.Invoke(progressDelegate, new object[] { max, type });
            }
            else
            {

                if (type == "Download")
                {
                    progressBarEx1.Maximum = max;
                }
                else
                {
                    progressBarEx2.Maximum = max;
                }
            }
        }

        public void DownLoadFiles(string strMask,List<string> localDirs,string host)
        {
            var ftpDic = BaseHelper.FtpDic;
            if (!ftpDic.ContainsKey(host))
            {
                return;
            }

            var ftp = ftpDic[host];
            var user = ftp.UsrName;
            var password = ftp.Password;
            var port = ftp.Port;
            var path = ftp.RootPath;
            var ftpClient = new FtpHelper(host, path, user, password, port, FtpHelper.FtpMode.Passive);
            try
            {
                LogHelper.Log(this, new Log() { Level = EnumHelper.State.Info, Message = $"FTP[{host}]开始连接" });
                ftpClient.Connect();
                LogHelper.Log(this, new Log() { Level = EnumHelper.State.Info, Message = $"FTP[{host}]连接成功" });
            }
            catch (Exception e)
            {
                LogHelper.Log(this, new Log() {Level = EnumHelper.State.Error, Message = $"FTP[{host}]连接失败:{e.Message}"});
                return;
            }
            if (ftpClient.Connected)
            {
                foreach (var localDir in localDirs)
                {
                    var directoryInfo  = new DirectoryInfo(localDir);
                    var site = directoryInfo.Name;
                    var remoteDir = localDir.Replace(_rootPath, path); 
                    try
                    {
                        ftpClient.ChDir(remoteDir);
                    }
                    catch (Exception e)
                    {
                        LogHelper.Log(this, new Log() { Level = EnumHelper.State.Error, Message = $"FTP[{host}]的文件夹[{remoteDir}]不存在:{e.Message}" });
                        continue;
                    }
                    try
                    {
                        LogHelper.Log(this, new Log() { Level = EnumHelper.State.Info, Message = $"FTP[{host}]开始获取文件列表" });
                        var lstFile = ftpClient.Dir(strMask);
                        if (_analysisCount == 0)
                        {
                            _siteFileCount = lstFile.Length;
                            _analysisCount = _downloadCount*lstFile.Length;
                            SetProgressMax(_analysisCount, "Analysis");
                        }
                        LogHelper.Log(this, new Log() { Level = EnumHelper.State.Info, Message = $"FTP[{host}]开始下载文件" });
                        foreach (var file in lstFile)
                        {
                            var localFile = Path.Combine(localDir, file);
                            var remoteFile = Path.Combine(remoteDir, file);
                            if (!File.Exists(localFile))
                            {
                                try
                                {
                                    ftpClient.DownloadFile(remoteFile, localDir, file);
                                }
                                catch (Exception e)
                                {
                                    LogHelper.Log(this, new Log() { Level = EnumHelper.State.Error, Message = $"FTP[{host}]的文件[{remoteFile}]下载失败:{e.Message}" });
                                }
                            }
                            _lstSequence.Add(localFile);
                        }
                        
                        SetProgress(++_hasDownloadCount,"Download");
                        LogHelper.Log(this, new Log() { Level = EnumHelper.State.Info, Message = $"站点[{site}]下载文件完成" });
                    }
                    catch (Exception e)
                    {
                        LogHelper.Log(this, new Log() { Level = EnumHelper.State.Error, Message = $"FTP[{site}]的文件列表获取失败:{e.Message}" });
                    }
                }
                ftpClient.DisConnect();
                try
                {
                    _lstThreadId.Remove(Thread.CurrentThread.ManagedThreadId);
                }
                catch (Exception e)
                {
                    LogHelper.Log(this, new Log() { Level = EnumHelper.State.Error, Message = $"{e.Message}" });
                }

            }
        }

        public void AnalysisThread()
        {
            while (_isAnalysis)
            {
                try
                {
                    if (_lstSequence.Count > 0)
                    {
                        var file = _lstSequence[0];
                        _lstSequence.Remove(file);
                        var unzipFile = UnZip(file);
                        Analysis(unzipFile);
                        File.Delete(unzipFile);
                        if ((++_hasAnalysis)%_siteFileCount == 0)
                        {
                            SetProgress(_hasAnalysis, "Analysis");
                        }

                    }
                    Thread.Sleep(100);
                    if (_lstThreadId.Count == 0 && _lstSequence.Count == 0)
                    {
                        _isAnalysis = false;
                        Export();
                    }
                }
                catch (Exception e)
                {
                    LogHelper.Log(this, new Log() { Level = EnumHelper.State.Error, Message = $"解析文件失败:{e.Message}" });
                }
            }
        }

        public void DownLoadThread(object ob)
        {
            try
            {
                var strMask = "";
                var downLoad = (DownLoad) ob;
                switch (_fileType)
                {
                    case EnumHelper.FileType.MRS:
                        strMask = "TD-LTE_MRS_ZTE_*.zip";
                        break;
                    case EnumHelper.FileType.MRO:
                        strMask = "TD-LTE_MRO_ZTE_*.zip";
                        break;
                }
                DownLoadFiles(strMask,downLoad.localDirs,downLoad.host);
            }
            catch (Exception e)
            {
                LogHelper.Log(this, new Log() { Level = EnumHelper.State.Error, Message = $"{e.Message}" });
            }
        }

        private string UnZip(string file)
        {
            var unzipFile = string.Empty;
            try
            {
                try
                {
                    var zipHelper = new ZipHelper();
                    unzipFile = zipHelper.UnZipFile(file, _unZipPath);
                }
                catch (Exception e)
                {
                    LogHelper.Log(this, new Log() { Level = EnumHelper.State.Info, Message = $"解压文件[{file}]失败:{e.Message}" });
                }
            }
            catch (Exception e)
            {
                LogHelper.Log(this, new Log() { Level = EnumHelper.State.Error, Message = "解压文件失败：" + e.Message });
            }

            return unzipFile;
        }

        public virtual void Analysis(string file)
        {

        }

        public virtual void Export()
        {

        }

        private void AnalysisMrs(string file)
        {
            var fileInfo = new FileInfo(file);
            var fileName = fileInfo.Name;
            try
            {
                XmlHelper xmlReader = new XmlHelper();
                xmlReader.Paeser(file);
                string enbId = xmlReader.GetEnbId();
                LogHelper.Log(this, new Log() { Level = EnumHelper.State.Info, Message = "解析文件【" + fileName + "】的参数" });
                for (var i = 0; i < 27; i++)
                {
                    try
                    {
                        var smrList = xmlReader.GetSmrList(i);
                        _dicMrs = xmlReader.GetMrsDic(_dicMrs, enbId, i, smrList);
                    }
                    catch (Exception e)
                    {
                        LogHelper.Log(this,  new Log(){ Level = EnumHelper.State.Info, Message = "解析文件【" + fileName + "】失败：" + e.Message});
                    }
                }
            }
            catch (Exception e)
            {
                LogHelper.Log(this, new Log() { Level = EnumHelper.State.Info, Message = "解析文件失败【" + fileName + "】:" + e.Message });
            }
        }

        private void AnalysisMro(string file)
        {
            var fileInfo = new FileInfo(file);
            var fileName = fileInfo.Name;
            try
            {
                XmlHelper xmlReader = new XmlHelper();
                xmlReader.Paeser(file);
                string enbId = xmlReader.GetEnbId();
                LogHelper.Log(this, new Log() { Level = EnumHelper.State.Info, Message = "解析文件【" + fileName + "】的参数" });
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

        public int ReadSite(string file)
        {
            var siteCount = 0;
            var table = ExcelHelper.CsvToDataTable(file);
            for (var i = 0; i < table.Rows.Count; i++)
            {
                var row = table.Rows[i];
                var host = string.Empty;
                var site = string.Empty;
                if (table.Columns.Count == 1)
                {
                    host = "local";
                    site = row[0].ToString();
                    _isLocal = true;
                }
                else
                {
                    host = row[0].ToString();
                    site = row[1].ToString();
                    _isLocal = false;
                }
                siteCount++;
                if (!_dicSite.ContainsKey(host))
                {
                    _dicSite.Add(host,new List<string>());
                }
                _dicSite[host].Add(site);
            }

            return siteCount;
        }

        public void CreateLoacalDir()
        {
            foreach (var host in _dicSite.Keys)
            {
                var sites = _dicSite[host];
                foreach (var site in sites)
                {
                    foreach (var date in _dateList)
                    {
                        var dir = Path.Combine(Path.Combine(_rootPath, date), site);
                        if (!_dicLocalDir.ContainsKey(host))
                        {
                            _dicLocalDir.Add(host,new List<string>());
                        }
                        if (!Directory.Exists(dir))
                        {
                            Directory.CreateDirectory(dir);
                        }
                        _dicLocalDir[host].Add(dir);
                    }
                }
            }
        }

        private void dgvLog_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            var dgr = dgvLog.Rows[e.RowIndex];
            string value = dgr.Cells[1].FormattedValue.ToString();
            switch (value)
            {
                case "Warn":
                    dgr.DefaultCellStyle.BackColor = Color.Yellow;
                    break;
                case "Error":
                    dgr.DefaultCellStyle.BackColor = Color.Red;
                    break;
            }
        }

        private void btnAnalysis_Click(object sender, EventArgs e)
        {
            var ftpDic = BaseHelper.FtpDic;
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    LogHelper.Log(this, new Log() { Level = EnumHelper.State.Info, Message = $"开始解析..." });
                    Init();
                    var file = fileDialog.FileName;
                    _dateList = txtDate.Text.Split(new[] { ",", "，", " ", "\t" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    _downloadCount = ReadSite(file)*_dateList.Count;
                    progressBarEx1.Minimum = 0;
                    progressBarEx1.Maximum = _downloadCount;
                    progressBarEx2.Minimum = 0;
                    CreateLoacalDir();

                    foreach (var host in _dicLocalDir.Keys)
                    {
                        var localDirLst = _dicLocalDir[host];
                        if (_isLocal)
                        {
                            foreach (string localDir in localDirLst)
                            {
                                var fileLst = Directory.GetFiles(localDir);
                                foreach (var fileName in fileLst)
                                {
                                    Regex regex = null;
                                    switch (_fileType)
                                    {
                                        case EnumHelper.FileType.MRS:
                                            regex = new Regex(@"^(?<fpath>([a-zA-Z]:\\)([\s\.\-\w]+\\)*)(?<fname>TD-LTE_MRS_ZTE_*)(?<namext>(\.zip)*)");
                                            break;
                                        case EnumHelper.FileType.MRO:
                                            regex = new Regex(@"^(?<fpath>([a-zA-Z]:\\)([\s\.\-\w]+\\)*)(?<fname>TD-LTE_MRO_ZTE_*)(?<namext>(\.zip)*)");
                                            break;
                                    }
                                    if (regex != null)
                                    {
                                        Match result = regex.Match(fileName);
                                        if (result.Success)
                                        {
                                            _lstSequence.Add(fileName);
                                        }
                                    }

                                }
                                if (_analysisCount == 0)
                                {
                                    _siteFileCount = _lstSequence.Count;
                                    _analysisCount = _siteFileCount*_downloadCount;
                                    progressBarEx2.Maximum = _analysisCount;
                                }
                            }
                        }
                        else
                        {
                            int j = 100;
                            for (int i = 0; i < localDirLst.Count; i += 100)
                            {
                                List<string> cList = new List<string>();
                                cList = localDirLst.Take(j).Skip(i).ToList();
                                j += 100;
                                var downLoad = new DownLoad()
                                {
                                    localDirs = cList,
                                    host = host
                                };
                                var downloadThread = new Thread(new ParameterizedThreadStart(DownLoadThread));
                                if (_fileType == EnumHelper.FileType.MRS)
                                {
                                    downloadThread.Name = "MRS";
                                }
                                else if (_fileType == EnumHelper.FileType.MRO)
                                {
                                    downloadThread.Name = "MRO";
                                }
                                downloadThread.Start(downLoad);
                                _lstThreadId.Add(downloadThread.ManagedThreadId);
                            }
                        }
                    }
                    _isAnalysis = true;
                    _analysisThread = new Thread(AnalysisThread);
                    _analysisThread.Start();
                }
                catch (Exception exception)
                {
                    LogHelper.Log(this, new Log() { Level = EnumHelper.State.Error, Message = $"解析失败:{exception.Message}" });
                }
            }
        }
    }
}
