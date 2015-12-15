using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace MRAnalysis.Common
{
    public class FtpHelper
    {
        public class FtpLogEventArgs : EventArgs
        {
            private string _mStrLog = string.Empty;
            public string Log => _mStrLog;

            public FtpLogEventArgs(string strLog)
            {
                _mStrLog = strLog;
            }
        }
        public delegate void FtpLogEventHandler(object sender, FtpLogEventArgs e);

        public class FtpTranProgressEventArgs : EventArgs
        {
            #region 成员变量

            private uint _mPercent = 0u;
            private bool _mCancel = false;

            #endregion

            #region 属性

            public uint Percent => _mPercent;
            public bool Cancel => _mCancel;

            #endregion

            #region 初始化

            public FtpTranProgressEventArgs(uint percent)
            {
                _mPercent = percent;
                _mCancel = false;
            }

            #endregion

        }
        public delegate void FtpTranProgressEventHandler(object sender, FtpTranProgressEventArgs e);


        #region 成员变量

        public enum FtpTransferType
        {
            Binary,
            ASCII
        }
        public enum FtpMode
        {
            Active,
            Passive
        }
        public enum FtpSystemType
        {
            UNIX,
            WINDOWS
        }

        private Socket _mSocketConnect = null;
        private string mStrServer = string.Empty;
        private int _mIntPort = 21;
        private string _mStrUser = string.Empty;
        private string _mStrPassword = string.Empty;
        private string _mStrPath = string.Empty;
        private bool _mIsConnected = false;
        private FtpMode _mMode = FtpMode.Passive;
        private FtpSystemType _mSystemType = FtpSystemType.UNIX;
        private string _mStrReply = string.Empty;
        private int _mIntReplyCode = 0;
        private static int BLOCK_SIZE = 2048;
        private byte[] _mBuffer = new byte[BLOCK_SIZE];

        #endregion

        #region 事件

        private event FtpLogEventHandler MFtpLogEvent;

        public event FtpLogEventHandler FtpLogEvent
        {
            add
            {
                FtpLogEventHandler handlerTemp;
                FtpLogEventHandler fieldsChanged = MFtpLogEvent;
                do
                {
                    handlerTemp = fieldsChanged;
                    FtpLogEventHandler handlerRes = (FtpLogEventHandler) Delegate.Combine(handlerTemp, value);
                    fieldsChanged = Interlocked.CompareExchange<FtpLogEventHandler>(ref MFtpLogEvent, handlerRes,
                        handlerTemp);
                } while (fieldsChanged != handlerTemp);
            }

            remove
            {
                FtpLogEventHandler handlerTemp;
                FtpLogEventHandler fieldsChanged = MFtpLogEvent;
                do
                {
                    handlerTemp = fieldsChanged;
                    FtpLogEventHandler handlerRes = (FtpLogEventHandler) Delegate.Remove(handlerTemp, value);
                    fieldsChanged = Interlocked.CompareExchange<FtpLogEventHandler>(ref MFtpLogEvent, handlerRes,
                        handlerTemp);
                } while (fieldsChanged != handlerTemp);
            }
        }

        private event FtpTranProgressEventHandler MFtpTranProgressEvent;

        public event FtpTranProgressEventHandler FtpTranProgressEvent
        {
            add
            {
                FtpTranProgressEventHandler handlerTemp;
                FtpTranProgressEventHandler fieldsChanged = MFtpTranProgressEvent;
                do
                {
                    handlerTemp = fieldsChanged;
                    FtpTranProgressEventHandler handlerRes =
                        (FtpTranProgressEventHandler) Delegate.Combine(handlerTemp, value);
                    fieldsChanged =
                        Interlocked.CompareExchange<FtpTranProgressEventHandler>(ref MFtpTranProgressEvent,
                            handlerRes, handlerTemp);
                } while (fieldsChanged != handlerTemp);
            }
            remove
            {
                FtpTranProgressEventHandler handlerTemp;
                FtpTranProgressEventHandler fieldsChanged = MFtpTranProgressEvent;
                do
                {
                    handlerTemp = fieldsChanged;
                    FtpTranProgressEventHandler handlerRes =
                        (FtpTranProgressEventHandler) Delegate.Remove(handlerTemp, value);
                    fieldsChanged =
                        Interlocked.CompareExchange<FtpTranProgressEventHandler>(ref MFtpTranProgressEvent,
                            handlerRes, handlerTemp);
                } while (fieldsChanged != handlerTemp);
            }
        }

        #endregion

        #region 属性

        public bool Connected => _mIsConnected;

        public FtpTransferType TransferType
        {
            set
            {
                SendCommand(value == FtpTransferType.Binary ? "TYPE I" : "TYPE A");
                if (_mIntReplyCode != 200)
                {
                    throw new IOException(_mStrReply.Substring(4));
                }
            }
        }

        public FtpMode Mode
        {
            get { return _mMode; }
            set { _mMode = value; }
        }

        public FtpSystemType SystemType
        {
            get { return _mSystemType; }
            set { _mSystemType = value; }
        }

        #endregion

        protected virtual void OnFtpLogEvent(FtpLogEventArgs e)
        {
            if (MFtpLogEvent != null)
            {
                MFtpLogEvent(this, e);
            }
        }
        protected virtual void OnFtpTranProgressEvent(FtpTranProgressEventArgs e)
        {
            if (MFtpTranProgressEvent != null)
            {
                MFtpTranProgressEvent(this, e);
            }
        }
        public FtpHelper(string server, string path, string user, string password, int port, FtpMode mode)
        {
            mStrServer = server;
            _mStrPath = path;
            _mStrUser = user;
            _mStrPassword = password;
            _mIntPort = port;
            _mMode = mode;
        }

        public void Connect()
        {
            _mSocketConnect = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            IPHostEntry ipHost = Dns.GetHostEntry(mStrServer);
            IPEndPoint iPEndPoint = new IPEndPoint(ipHost.AddressList[0], _mIntPort);
            try
            {
                _mSocketConnect.Connect(iPEndPoint);
            }
            catch (Exception ex)
            {
                throw new IOException("Couldn't connect to remote server");
            }
            ReadReply();
            if (_mIntReplyCode != 220)
            {
                DisConnect();
                throw new IOException(_mStrReply.Substring(4));
            }
            SendCommand("USER " + _mStrUser);
            if (_mIntReplyCode != 331 && _mIntReplyCode != 230)
            {
                CloseSocketConnect();
                throw new IOException(_mStrReply.Substring(4));
            }
            if (_mIntReplyCode == 331)
            {
                SendCommand("PASS " + _mStrPassword);
                if (_mIntReplyCode != 230 && _mIntReplyCode != 202)
                {
                    CloseSocketConnect();
                    throw new IOException(_mStrReply.Substring(4));
                }
            }
            SendCommand("SYST");
            if (_mIntReplyCode != 215)
            {
                CloseSocketConnect();
                throw new IOException(_mStrReply.Substring(4));
            }

            if (_mStrReply[4].ToString() == "W" || _mStrReply[4].ToString() == "w")
            {
                _mSystemType = FtpSystemType.WINDOWS;
            }
            _mIsConnected = true;
            ChDir(_mStrPath);
        }

        public void DisConnect()
        {
            CloseSocketConnect();
        }

        public void ChDir(string strDirName)
        {
            if (strDirName.Equals(""))
            {
                return;
            }
            if (!_mIsConnected)
            {
                Connect();
            }
            SendCommand("CWD " + strDirName);
            if (_mIntReplyCode != 250)
            {
                throw new IOException(_mStrReply.Substring(4));
            }
            _mStrPath = strDirName;
        }

        public void Reset(long size)
        {
            if (!_mIsConnected)
            {
                Connect();
            }
            SendCommand("REST " + size.ToString());
            if (_mIntReplyCode != 350)
            {
                throw new IOException(_mStrReply.Substring(4));
            }
        }

        public string[] Dir(string strMark)
        {
            if (!_mIsConnected)
            {
                Connect();
            }
            char[] array = new char[]
            {
                '\n',
                '\r'
            };
            string text;
            string[] result;
            if (_mMode == FtpMode.Active)
            {
                TcpListener tcpListener = null;
                CreateDataListener(ref tcpListener);
                TransferType = FtpTransferType.ASCII;
                SendCommand("NLST " + strMark);
                Socket socket = tcpListener.AcceptSocket();
                text = "";
                int num;
                do
                {
                    num = socket.Receive(_mBuffer, _mBuffer.Length, 0);
                    text += Encoding.Default.GetString(_mBuffer, 0, num);
                }
                while (num >= _mBuffer.Length);
                result = text.Split(array, StringSplitOptions.RemoveEmptyEntries);
                socket.Close();
                tcpListener.Stop();
                return result;
            }
            Socket socket2 = CreateDataSocket();
            SendCommand("NLST " + strMark);
            if (_mIntReplyCode != 150 && _mIntReplyCode != 125 && _mIntReplyCode != 226)
            {
                throw new IOException(_mStrReply.Substring(4));
            }
            text = "";
            int num2;
            do
            {
                num2 = socket2.Receive(_mBuffer, _mBuffer.Length, 0);
                text += Encoding.Default.GetString(_mBuffer, 0, num2);
            }
            while (num2 >= _mBuffer.Length);
            result = text.Split(array, StringSplitOptions.RemoveEmptyEntries);
            socket2.Close();
            if (_mIntReplyCode != 226)
            {
                ReadReply();
                if (_mIntReplyCode != 226)
                {
                    throw new IOException(_mStrReply.Substring(4));
                }
            }
            return result;
        }

        public void UploadFile(string strFile)
        {
            if (!_mIsConnected)
            {
                Connect();
            }
            if (_mMode == FtpMode.Active)
            {
                TcpListener tcpListener = null;
                CreateDataListener(ref tcpListener);
                TransferType = FtpTransferType.Binary;
                SendCommand("STOR " + Path.GetFileName(strFile));
                if (_mIntReplyCode != 125 && _mIntReplyCode != 150)
                {
                    throw new IOException(_mStrReply.Substring(4));
                }
                Socket socket = tcpListener.AcceptSocket();
                FileStream fileStream = new FileStream(strFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                long length = fileStream.Length;
                long num = 0L;
                int num2;
                while ((num2 = fileStream.Read(_mBuffer, 0, _mBuffer.Length)) > 0)
                {
                    num += (long)num2;
                    uint percent = (uint)(num * 100L / length);
                    FtpTranProgressEventArgs e = new FtpTranProgressEventArgs(percent);
                    OnFtpTranProgressEvent(e);
                    socket.Send(_mBuffer, num2, 0);
                }
                fileStream.Close();
                if (socket.Connected)
                {
                    socket.Close();
                }
                tcpListener.Stop();
                return;
            }
            else
            {
                Socket socket2 = CreateDataSocket();
                SendCommand("STOR " + Path.GetFileName(strFile));
                if (_mIntReplyCode != 125 && _mIntReplyCode != 150)
                {
                    throw new IOException(_mStrReply.Substring(4));
                }
                FileStream fileStream2 = new FileStream(strFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                long length = fileStream2.Length;
                long num = 0L;
                int num2;
                while ((num2 = fileStream2.Read(_mBuffer, 0, _mBuffer.Length)) > 0)
                {
                    num += (long)num2;
                    uint percent = (uint)(num * 100L / length);
                    FtpTranProgressEventArgs e2 = new FtpTranProgressEventArgs(percent);
                    OnFtpTranProgressEvent(e2);
                    socket2.Send(_mBuffer, num2, 0);
                }
                fileStream2.Close();
                if (socket2.Connected)
                {
                    socket2.Close();
                }
                if (_mIntReplyCode != 226 && _mIntReplyCode != 250)
                {
                    ReadReply();
                    if (_mIntReplyCode != 226 && _mIntReplyCode != 250)
                    {
                        throw new IOException(_mStrReply.Substring(4));
                    }
                }
                return;
            }
        }

        public void DownloadFile(string strRemoteFileName, string strLocalFolder, string strLocalFileName)
        {
            if (!_mIsConnected)
            {
                Connect();
            }
            if (_mMode == FtpMode.Active)
            {
                TcpListener tcpListener = null;
                CreateDataListener(ref tcpListener);
                string extension = Path.GetExtension(strRemoteFileName);
                if (extension != ".txt" && extension != ".TXT")
                {
                    TransferType = FtpTransferType.Binary;
                }
                if (strLocalFileName == "")
                {
                    strLocalFileName = strRemoteFileName;
                }
                FileStream fileStream = new FileStream(strLocalFolder + "\\" + strLocalFileName, FileMode.Create);
                SendCommand("RETR " + strRemoteFileName);
                if (_mIntReplyCode != 150 && _mIntReplyCode != 125 && _mIntReplyCode != 226 && _mIntReplyCode != 250 && _mIntReplyCode != 200)
                {
                    fileStream.Close();
                    throw new IOException(_mStrReply.Substring(4));
                }
                Socket socket = tcpListener.AcceptSocket();
                while (true)
                {
                    int num = socket.Receive(_mBuffer, _mBuffer.Length, 0);
                    if (num <= 0)
                    {
                        break;
                    }
                    fileStream.Write(_mBuffer, 0, num);
                }
                fileStream.Close();
                if (socket.Connected)
                {
                    socket.Close();
                }
                if (_mIntReplyCode != 226 && _mIntReplyCode != 250)
                {
                    ReadReply();
                    if (_mIntReplyCode != 226 && _mIntReplyCode != 250)
                    {
                        throw new IOException(_mStrReply.Substring(4));
                    }
                }
                tcpListener.Stop();
                return;
            }
            else
            {
                string extension2 = Path.GetExtension(strRemoteFileName);
                if (extension2 != ".txt" && extension2 != ".TXT")
                {
                    TransferType = FtpTransferType.Binary;
                }
                if (strLocalFileName == "")
                {
                    strLocalFileName = strRemoteFileName;
                }
                FileStream fileStream2 = new FileStream(strLocalFolder + "\\" + strLocalFileName, FileMode.Create);
                Socket socket2 = CreateDataSocket();
                SendCommand("RETR " + strRemoteFileName);
                if (_mIntReplyCode != 150 && _mIntReplyCode != 125 && _mIntReplyCode != 226 && _mIntReplyCode != 250)
                {
                    fileStream2.Close();
                    throw new IOException(_mStrReply.Substring(4));
                }
                while (true)
                {
                    int num2 = socket2.Receive(_mBuffer, _mBuffer.Length, 0);
                    if (num2 <= 0)
                    {
                        break;
                    }
                    fileStream2.Write(_mBuffer, 0, num2);
                }
                fileStream2.Close();
                if (socket2.Connected)
                {
                    socket2.Close();
                }
                if (_mIntReplyCode != 226 && _mIntReplyCode != 250)
                {
                    ReadReply();
                    if (_mIntReplyCode != 226 && _mIntReplyCode != 250)
                    {
                        throw new IOException(_mStrReply.Substring(4));
                    }
                }
                return;
            }
        }

        public void CreateDir(string strDirName)
        {
            if (!_mIsConnected)
            {
                Connect();
            }
            SendCommand("MKD " + strDirName);
            if (_mIntReplyCode != 257)
            {
                throw new IOException(_mStrReply.Substring(4));
            }
        }

        public void DeleteDir(string strDirName)
        {
            if (!_mIsConnected)
            {
                Connect();
            }
            SendCommand("RMD " + strDirName);
            if (_mIntReplyCode != 250)
            {
                throw new IOException(_mStrReply.Substring(4));
            }
        }

        public void DeleteFile(string strFile)
        {
            if (!_mIsConnected)
            {
                Connect();
            }
            SendCommand("DELE " + strFile);
            if (_mIntReplyCode != 250)
            {
                throw new IOException(_mStrReply.Substring(4));
            }
        }

        private long GetFileSize(string strFileName)
        {
            if (!_mIsConnected)
            {
                Connect();
            }
            SendCommand("SIZE " + Path.GetFileName(strFileName));
            if (_mIntReplyCode == 213 || _mIntReplyCode == 200)
            {
                return long.Parse(_mStrReply.Substring(4));
            }
            throw new IOException(_mStrReply.Substring(4));
        }

        private string ReadLine()
        {
            string text = string.Empty;
            Thread.Sleep(200);
            int num;
            do
            {
                text = "";
                num = _mSocketConnect.Receive(_mBuffer, _mBuffer.Length, 0);
                text += Encoding.Default.GetString(_mBuffer, 0, num);
                FtpLogEventArgs e = new FtpLogEventArgs("应答:  " + text);
                OnFtpLogEvent(e);
            }
            while (num >= _mBuffer.Length);
            char[] array = new char[]
            {
                '\n'
            };
            string[] array2 = text.Split(array);
            if (text.Length > 2)
            {
                text = array2[array2.Length - 2];
            }
            else
            {
                text = array2[0];
            }
            if (!text.Substring(3, 1).Equals(" "))
            {
                return ReadLine();
            }
            return text;
        }

        private void ReadReply()
        {
            _mStrReply = ReadLine();
            _mIntReplyCode = int.Parse(_mStrReply.Substring(0, 3));
        }

        private void SendCommand(string strCommand)
        {
            FtpLogEventArgs e = new FtpLogEventArgs("命令:  " + strCommand);
            OnFtpLogEvent(e);
            byte[] bytes = Encoding.Default.GetBytes((strCommand + "\r\n").ToCharArray());
            _mSocketConnect.Send(bytes, bytes.Length, 0);
            ReadReply();
        }

        private void CloseSocketConnect()
        {
            if (_mSocketConnect != null)
            {
                _mSocketConnect.Close();
                _mSocketConnect = null;
            }
            _mIsConnected = false;
        }

        private Socket CreateDataSocket()
        {
            Socket result;
            try
            {
                SendCommand("PASV");
                if (_mIntReplyCode != 227)
                {
                    throw new IOException(_mStrReply.Substring(4));
                }
                int num = _mStrReply.IndexOf('(');
                int num2 = _mStrReply.IndexOf(')');
                string text = _mStrReply.Substring(num + 1, num2 - num - 1);
                string[] array = new string[6];
                array = text.Split(new char[]
                {
                    ','
                });
                if (array.Length != 6)
                {
                    throw new IOException("Malformed PASV strReply: " + _mStrReply);
                }
                string text2 = string.Concat(new string[]
                {
                    array[0],
                    ".",
                    array[1],
                    ".",
                    array[2],
                    ".",
                    array[3]
                });
                try
                {
                    num = int.Parse(array[4]);
                    num2 = int.Parse(array[5]);
                }
                catch
                {
                    throw new IOException("Malformed PASV strReply: " + _mStrReply);
                }
                int num3 = (num << 8) + num2;
                Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPEndPoint iPEndPoint = new IPEndPoint(IPAddress.Parse(text2), num3);
                try
                {
                    socket.Connect(iPEndPoint);
                }
                catch (Exception)
                {
                    throw new IOException("Can't connect to remote server");
                }
                result = socket;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.ToString());
            }
            return result;
        }

        private void CreateDataListener(ref TcpListener listener)
        {
            string hostName = Dns.GetHostName();
            IPAddress iPAddress = Dns.GetHostEntry(hostName).AddressList[0];
            listener = new TcpListener(iPAddress, 0);
            listener.Start();
            IPEndPoint iPEndPoint = (IPEndPoint)listener.LocalEndpoint;
            int num = iPEndPoint.Port >> 8;
            int num2 = iPEndPoint.Port & 255;
            SendCommand(string.Concat(new string[]
            {
                "PORT ",
                iPEndPoint.Address.ToString().Replace(".", ","),
                ",",
                num.ToString(),
                ",",
                num2.ToString()
            }));
            if (_mIntReplyCode != 200 && _mIntReplyCode != 226)
            {
                throw new IOException(_mStrReply.Substring(4));
            }
        }
    }
}
