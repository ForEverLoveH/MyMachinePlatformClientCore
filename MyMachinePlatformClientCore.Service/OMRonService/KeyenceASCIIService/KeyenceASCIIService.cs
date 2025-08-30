using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using MyMachinePlatformClientCore.Log.MyLogs;

namespace MyMachinePlatformClientCore.Service.OMRonService ;

 public class KeyenceASCIIService
    {
        [DllImport("kernel32", CharSet = CharSet.Auto)]
        private static extern uint GetTickCount();
        public uint NewTime
        {
            get { return GetTickCount(); }
        }
        public int timeout
        {
            get { return _timeout; }
            set { _timeout = value; }
        }
        private int _timeout = 1000;
        private Ping _ping = null;
        private IPEndPoint _endPoint = null;

        internal Socket KVTCPSocket = null;
        private Byte DA1 = 0;
        private Byte SA1 = 0;
        private string _localIP = null;
        // event handles to synchronize threads 
        AutoResetEvent m_hImmeTaskEvent = null;
        ManualResetEvent m_hThreadExitEvent = null;
        object m_critSecPeriodList = new object();
        object m_critSecImmeList = new object();
        object m_critUpdateReadList = new object();
        object m_UcritUpdateReadList = new object();
        List<TaskStructTCPKV> m_periodTaskList = new List<TaskStructTCPKV>();
        Queue<TaskStructTCPKV> m_immeTaskList = new Queue<TaskStructTCPKV>();
        Dictionary<uint, short>[] m_mapReadPLCMem = new Dictionary<uint, short>[34]
        {
            new Dictionary<uint, short>(),new Dictionary<uint, short>(),new Dictionary<uint, short>(),new Dictionary<uint, short>(),new Dictionary<uint, short>(),
            new Dictionary<uint, short>(),new Dictionary<uint, short>(),new Dictionary<uint, short>(),new Dictionary<uint, short>(),new Dictionary<uint, short>(),
            new Dictionary<uint, short>(),new Dictionary<uint, short>(),new Dictionary<uint, short>(),new Dictionary<uint, short>(),new Dictionary<uint, short>(),
            new Dictionary<uint, short>(),new Dictionary<uint, short>(),new Dictionary<uint, short>(),new Dictionary<uint, short>(),new Dictionary<uint, short>(),
            new Dictionary<uint, short>(),new Dictionary<uint, short>(),new Dictionary<uint, short>(),new Dictionary<uint, short>(),new Dictionary<uint, short>(),
            new Dictionary<uint, short>(),new Dictionary<uint, short>(),new Dictionary<uint, short>(),new Dictionary<uint, short>(),new Dictionary<uint, short>(),
            new Dictionary<uint, short>(),
            new Dictionary<uint, short>(),
            new Dictionary<uint, short>(),
            new Dictionary<uint, short>(),
        };
        Dictionary<uint, ushort>[] m_mapReadPLCUMem = new Dictionary<uint, ushort>[34]
        {
            new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),
            new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),
            new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),
            new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),
            new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),
            new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),new Dictionary<uint, ushort>(),
            new Dictionary<uint, ushort>(),
            new Dictionary<uint, ushort>(),
            new Dictionary<uint, ushort>(),
            new Dictionary<uint, ushort>(),
        };

        Dictionary<uint, uint>[] m_mapReadPLCMemTimeStamp = new Dictionary<uint, uint>[34]
        {
            new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(),
            new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(),
            new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(),
            new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(),
            new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(),
            new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(),
            new Dictionary<uint, uint>(),
            new Dictionary<uint, uint>(),
            new Dictionary<uint, uint>(),
            new Dictionary<uint, uint>(),
        };
        Dictionary<uint, uint>[] m_mapReadPLCMemUTimeStamp = new Dictionary<uint, uint>[34]
        {
            new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(),
            new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(),
            new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(),
            new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(),
            new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(),
            new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(), new Dictionary<uint, uint>(),
            new Dictionary<uint, uint>(),
            new Dictionary<uint, uint>(),
            new Dictionary<uint, uint>(),
            new Dictionary<uint, uint>(),
        };

        private string _lastError = "";
        /// <summary>
        /// return last error detected
        /// </summary>
        public string LastError
        {
            get { return _lastError; }
        }

        private WaitHandle GetImmeTaskEvtHandle() { return m_hImmeTaskEvent; }
        private WaitHandle GetThreadExitHandle() { return m_hThreadExitEvent; }
        private static System.Threading.Thread m_pSocketThreadProc;


        public bool Connected
        {
            get { return (KVTCPSocket == null) ? false : KVTCPSocket.Connected; }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public KeyenceASCIIService()
        {
            // used to ping the PLC
            //
            this._ping = new Ping();

            // EndPoint parametres
            //
            this._endPoint = new IPEndPoint(0, 0);
        }

        /// <summary>
        /// set ip and port
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void SetTCPParams(IPAddress ip, int port, string localIP)
        {
            this._endPoint.Address = ip;
            this._endPoint.Port = port;
            this._localIP = localIP;
            if (m_hThreadExitEvent == null)
            {
                m_hThreadExitEvent = new ManualResetEvent(false);
            }

            if (m_hImmeTaskEvent == null)
            {
                m_hImmeTaskEvent = new AutoResetEvent(false);
            }

            //接下来是开通线程
            m_pSocketThreadProc = new System.Threading.Thread(SocketThreadProc);
            m_pSocketThreadProc.Start();
        }

        /// <summary>
        /// 连接欧姆龙以太网
        /// </summary>
        /// <returns></returns>
        private bool TCPConnect()
        {
            initConnect = true;
            try
            {
                if (KVTCPSocket != null)
                {
                    KVTCPSocket.Dispose();
                }
                KVTCPSocket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                KVTCPSocket.SendTimeout = _timeout;
                KVTCPSocket.ReceiveTimeout = _timeout;
                KVTCPSocket.SendBufferSize = 10240000;
                KVTCPSocket.ReceiveBufferSize = 10240000;
                KVTCPSocket.Bind(new IPEndPoint(IPAddress.Parse(this._localIP), 0));
                KVTCPSocket.Connect(this._endPoint);
                return this.Connected;
            }
            catch (System.Exception ex)//SocketException error
            {
                //PrintfLog.LogError("17" + ex.Message);
                System.Threading.Thread.Sleep(10);
                return false;
            }
        }

        public bool Connect()
        {
            if (this.Connected == true && this.initConnect == true)
            {
                return true;
            }

            try
            {
                return this.TCPConnect();
            }
            catch (Exception ex)
            {
                //PrintfLog.LogError("18" + ex.Message);
                Close();
                this._lastError = ex.Message;
                return false;
            }
        }

        /// <summary>
        /// close the socket
        /// </summary>
        /// <returns></returns>
        public void Close()
        {
            initConnect = false;
            lock (this)
            {
                if (KVTCPSocket == null) return;
                if (Connected)
                {
                    KVTCPSocket.Disconnect(false);
                    KVTCPSocket.Close();
                }
                KVTCPSocket.Dispose();
                KVTCPSocket = null;
            }
        }

        /// <summary>
        /// 欧姆龙握手信号初始化
        /// </summary>
        /// <returns></returns>
        private bool initConnect = false;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="cmdLen"></param>
        /// <returns></returns>
        private int Send(Byte[] command, int cmdLen)
        {
            if (!Connected)
            {
                throw new Exception("Socket is not connected.");
            }
            // sends the command
            //
            int bytesSent = KVTCPSocket.Send(command, cmdLen, SocketFlags.None);

            // it checks the number of bytes sent
            //
            if (bytesSent != cmdLen)
            {
                string msg = string.Format("Sending error. (Expected bytes: {0}  Sent: {1})"
                                            , cmdLen, bytesSent);
                throw new Exception(msg);
            }
            return bytesSent;
        }

        /// <summary>
        /// receives a response from the plc
        /// </summary>
        /// <param name="response"></param>
        /// <param name="respLen"></param>
        /// <returns></returns>
        private int Receive(ref Byte[] response, int respLen)
        {
            if (!this.Connected)
            {
                throw new Exception("Socket is not connected.");
            }

            // receives the response, this is a synchronous method and can hang the process        
            int bytesRecv = KVTCPSocket.Receive(response, respLen, SocketFlags.None);

            // check the number of bytes received
            //
            if (bytesRecv != respLen)
            {
                string msg = string.Format("Receiving error. (Expected: {0}  Received: {1})"
                                            , respLen, bytesRecv);
                throw new Exception(msg);
            }
            return bytesRecv;
        }

        private void SocketThreadProc()
        {
            bool bRun = true;
            int dwRes = 0;
            WaitHandle[] hArray = { null, null };
            hArray[0] = GetImmeTaskEvtHandle();
            hArray[1] = GetThreadExitHandle();

            while (bRun)
            {
                dwRes = WaitHandle.WaitAny(hArray, 15);
                switch (dwRes)
                {
                    case WaitHandle.WaitTimeout:
                        if (Connected && initConnect)
                        {
                            ProcessPeriodTask();
                        }
                        else
                        {
                            // ClearReadArea();
                        }
                        break;
                    case 0:
                        // Process the immediately task
                        ////m_hImmeTaskEvent
                        if (Connected && initConnect)
                        {
                            ProcessImmeTask();
                        }
                        break;
                    case 1:
                        // End this thread
                        //m_hThreadExitEvent
                        bRun = false;
                        break;
                    default:
                        // Error had happened, just break                      
                        break;
                }
            }
        }

        private void AddImmeTask(TaskStructTCPKV task)
        {
            lock (m_critSecImmeList)
            {
                m_immeTaskList.Enqueue(task);
            }
            // signal the event so that
            // the serve thread can process the task.
            if (m_hImmeTaskEvent != null)
            {
                m_hImmeTaskEvent.Set();
            }
        }

        private void ProcessImmeTask()
        {
            bool res = false;
            // The task describe in the list
            if (m_immeTaskList == null)
            {
                return;
            }
            if (m_immeTaskList.Count == 0)
                return;
            TaskStructTCPKV task;
            while (m_immeTaskList.Count != 0)
            {
                try
                {
                    lock (m_critSecImmeList)
                    {
                        // task = m_immeTaskList[0];
                        task = m_immeTaskList.Dequeue();//移除并返回
                    }
                    res = SendAndGetRes(task);
                    Thread.Sleep(5);

                    if (task.m_hEventFinish != null && res == true)
                    {
                        // Tell the call thread (main thread of the program) that
                        // immediate task has finished.
                        task.m_hEventFinish.Set();
                    }
                }
                catch (System.Exception ex)
                {
                  
                }
            }
        }

        // Use this in serve thread
        void ProcessPeriodTask()
        {
            if (m_periodTaskList == null)
            {
                _lastError = "m_periodTaskList is Null";
                return;
            }
            if (m_periodTaskList.Count == 0)
            {
                _lastError = "m_periodTaskList.Count is 0";
                return;
            }

            bool bRes;
            foreach (TaskStructTCPKV task in m_periodTaskList)
            {
                lock (m_critSecPeriodList)
                {
                    bRes = SendAndGetRes(task);
                }
                if (!bRes)
                {
                    // Something wrong must be in serial communication. such as
                    // comm timeout, not connected, and so on. In this situation
                    // bRes is false and we break here, and give up the task in
                    // the list in this period. give a chance to process something
                    // in the wait function.

                    break;
                }
                Thread.Sleep(5);
            }
        }

        public bool AddReadArea(MemAreaTCPKV area, uint unBeginWord, uint unWordsCount)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_nReadOrWrite = 0;//read
            task.m_PLCArea = area;//
            task.m_unBeginWord = unBeginWord;
            task.m_unWordsCount = unWordsCount;
            //发送命令
            task.GenStrCmd();
            lock (m_critSecPeriodList)
            {
                m_periodTaskList.Add(task);
            }
            return true;
        }

        public bool ClearReadArea()
        {
            if (m_periodTaskList.Count != -1)
            {
                m_periodTaskList.Clear();
            }
            return true;
        }
        string strRcvData111;
        bool SendAndGetRes(TaskStructTCPKV pTask)
        {
            try
            {
                Send(pTask.m_strCmd, pTask.m_strCmd.Length);
                strRcvData111 = ASCIIEncoding.ASCII.GetString(pTask.m_strCmd);

                Thread.Sleep(30);

                Byte[] respNADS = new Byte[pTask.RcvDataLength];

                Receive(ref respNADS, respNADS.Length);
                string strRcvData = ASCIIEncoding.ASCII.GetString(respNADS);

                if (pTask.m_nReadOrWrite == 0 /*Read*/)
                {
                    if (!strRcvData.Contains("E"))
                    {
                        string[] strRcvDataArr = strRcvData.Split(' ');
                        for (uint i = 0; i < pTask.m_unWordsCount; i++)
                        {
                            short value = 0;
                            short.TryParse(strRcvDataArr[i], out value);
                            //UpdateReadWord(pTask.m_PLCArea, (uint)(pTask.m_unBeginWord + i), value);
                            ushort uvalue = 0;
                            ushort.TryParse(strRcvDataArr[i], out uvalue);
                            UpdateReadWord(pTask.m_PLCArea, (uint)(pTask.m_unBeginWord + i), value);
                        }
                    }
                    else
                    {
                        for (uint i = 0; i < pTask.m_unWordsCount; i++)
                        {
                            short value = 0;
                            ushort uvalue = 0;
                            // UpdateReadWord(pTask.m_PLCArea, (uint)(pTask.m_unBeginWord + i), value);
                            UpdateReadWord(pTask.m_PLCArea, (uint)(pTask.m_unBeginWord + i), value);
                        }
                        return false;
                    }

                }
                else if (pTask.m_nReadOrWrite == 26)
                {
                    if (!strRcvData.Contains("E"))
                    {
                        string[] strRcvDataArr = strRcvData.Split(' ');
                        for (uint i = 0; i < pTask.m_unWordsCount; i++)
                        {
                            //UpdateReadWord(pTask.m_PLCArea, (uint)(pTask.m_unBeginWord + i), value);
                            ushort uvalue = 0;
                            ushort.TryParse(strRcvDataArr[i], out uvalue);
                            UpdateReaduWord(pTask.m_PLCArea, (uint)(pTask.m_unBeginWord + i), uvalue);
                        }
                    }
                    else
                    {
                        for (uint i = 0; i < pTask.m_unWordsCount; i++)
                        {
                            ushort uvalue = 0;
                            // UpdateReadWord(pTask.m_PLCArea, (uint)(pTask.m_unBeginWord + i), value);
                            UpdateReaduWord(pTask.m_PLCArea, (uint)(pTask.m_unBeginWord + i), uvalue);
                        }
                        return false;
                    }
                }
                else/*write*/
                {
                    if (!strRcvData.Contains("OK"))
                    {
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                

                _lastError = ex.Message;
                Close();
                return false;
            }
            return true;
        }

        bool UpdateReadWord(MemAreaTCPKV area, UInt32 unWordAddress, short wUpdate)
        {
            lock (m_critUpdateReadList)              // CriticalSect
            {
                if (m_mapReadPLCMem[(int)area].ContainsKey(unWordAddress) == false)
                {
                    m_mapReadPLCMem[(int)area].Add(unWordAddress, wUpdate);
                    m_mapReadPLCMemTimeStamp[(int)area].Add(unWordAddress, GetTickCount());
                }
                else
                {
                    m_mapReadPLCMem[(int)area][unWordAddress] = wUpdate;
                    m_mapReadPLCMemTimeStamp[(int)area][unWordAddress] = GetTickCount();
                }
            }
            return true;
        }
        bool UpdateReaduWord(MemAreaTCPKV area, UInt32 unWordAddress, ushort uUpdate)
        {

            lock (m_UcritUpdateReadList)              // CriticalSect
            {
                if (m_mapReadPLCUMem[(int)area].ContainsKey(unWordAddress) == false)
                {
                    m_mapReadPLCUMem[(int)area].Add(unWordAddress, uUpdate);
                    m_mapReadPLCMemUTimeStamp[(int)area].Add(unWordAddress, GetTickCount());
                }
                else
                {
                    m_mapReadPLCUMem[(int)area][unWordAddress] = uUpdate;
                    m_mapReadPLCMemUTimeStamp[(int)area][unWordAddress] = GetTickCount();
                }
            }
            return true;

        }
        private short GetReadWord(MemAreaTCPKV area, uint unWordAddress)
        {
            short wUpdate1 = 0;
            lock (m_critUpdateReadList)// CriticalSect
            {
                if (m_mapReadPLCMem[(int)area].ContainsKey(unWordAddress) == true)
                {
                    wUpdate1 = m_mapReadPLCMem[(int)area][unWordAddress];
                }
            }
            return wUpdate1;
        }
        private ushort GetReadUWord(MemAreaTCPKV area, uint unWordAddress)
        {
            ushort wUpdate1 = 0;
            lock (m_critUpdateReadList)// CriticalSect
            {
                if (m_mapReadPLCUMem[(int)area].ContainsKey(unWordAddress) == true)
                {
                    wUpdate1 = m_mapReadPLCUMem[(int)area][unWordAddress];
                }
            }
            return wUpdate1;
        }
        private uint GetReadWordTimeStamp(MemAreaTCPKV area, uint unWordAddress)
        {
            uint TimeStamp1 = 0;
            lock (m_critUpdateReadList)// CriticalSect
            {

                if (m_mapReadPLCMemTimeStamp[(int)area].ContainsKey(unWordAddress) == true)
                {
                    TimeStamp1 = m_mapReadPLCMemTimeStamp[(int)area][unWordAddress];
                }
            }
            return TimeStamp1;
        }




        /// <summary>批量写位数据
        /// /
        /// </summary>
        /// <param name="area"></param>
        /// <param name="iAddress"></param>
        /// <param name="iSize"></param>
        /// <param name="onOffBits"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool WritePLCMultiBitWait(MemAreaTCPKV area, uint iAddress, uint iSize, byte[] onOffBits, int unMilliseconds = 1000)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.MainCommand = (uint)MainCmd.MutilWRITE;
            task.SubCommand = (uint)SubCmd.BITADDR;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = iSize;
            //task.iData = onOffBits;
            task.onOffBits = onOffBits;
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality;
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                quality = true;
            }
            else
            {
                quality = false;
            }
            //bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return quality;
        }

        /// <summary>批量写位
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="iAddress"></param>
        /// <param name="iSize"></param>
        /// <param name="onOffBits"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool WritePLCMultiBit(MemAreaTCPKV area, uint iAddress, uint iSize, byte[] onOffBits)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_PLCArea = area;
            task.MainCommand = (uint)MainCmd.MutilWRITE;
            task.SubCommand = (uint)SubCmd.BITADDR;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = iSize;
            task.onOffBits = onOffBits;
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        /// <summary>写位等待完成
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="iAddress"></param>
        /// <param name="value"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool WritePLCBitWait(MemAreaTCPKV area, uint iAddress, byte value, int unMilliseconds = 1000)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.MainCommand = (uint)MainCmd.MutilWRITE;
            task.SubCommand = (uint)SubCmd.BITADDR;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = 1;
            byte[] onOffBits = new byte[1];
            if (Convert.ToBoolean(value))
            {
                value = 1;
            }
            else
            {
                value = 0;
            }
            onOffBits[0] = value;
            task.onOffBits = onOffBits;
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality;
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                quality = true;
            }
            else
            {
                quality = false;
            }
            task.m_hEventFinish.Close();
            return quality;
        }

        /// <summary>写位
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="iAddress"></param>
        /// <param name="value"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool WritePLCBit(MemAreaTCPKV area, uint iAddress, byte value)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_PLCArea = area;
            task.MainCommand = (uint)MainCmd.MutilWRITE;
            task.SubCommand = (uint)SubCmd.BITADDR;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = 1;
            byte[] onOffBits = new byte[1];
            if (Convert.ToBoolean(value))
            {
                value = 1;
            }
            else
            {
                value = 0;
            }
            onOffBits[0] = value;
            task.onOffBits = onOffBits;
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        /// <summary>写字等待完成
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="iAddress"></param>
        /// <param name="value"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool WritePLCShortWait(MemAreaTCPKV area, uint iAddress, short value, int unMilliseconds = 1000)
        {

            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = 1;
            task.m_nReadOrWrite = 1;
            task.m_nWriteDataType = 0;
            task.m_dData = new object[1];
            task.m_dData[0] = value;
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality;
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                quality = true;
            }
            else
            {
                quality = false;
            }
            task.m_hEventFinish.Close();
            return quality;
        }

        /// <summary>写字
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="iAddress"></param>
        /// <param name="value"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool WritePLCShort(MemAreaTCPKV area, uint iAddress, short value)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_PLCArea = area;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = 1;
            task.m_nReadOrWrite = 1;
            task.m_nWriteDataType = 0;
            task.m_dData = new object[1];
            task.m_dData[0] = value;
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        /// <summary>写多个short
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="strValue"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool WritePLCMutilShortIntWait(MemAreaTCPKV area, uint unBeginWord, short[] strValue, int unMilliseconds = 1000)//写PLC16位数组
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = unBeginWord;
            task.m_unWordsCount = (uint)strValue.Length;
            task.m_nReadOrWrite = 1;
            task.m_nWriteDataType = 1;
            task.m_dData = new object[strValue.Length];
            for (int i = 0; i < strValue.Length; i++)
            {
                task.m_dData[i] = strValue[i];
            }
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality;
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                quality = true;
            }
            else
            {
                quality = false;
            }
            task.m_hEventFinish.Close();
            return quality;
        }
        /// <summary>写多个int
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="strValue"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool WritePLCMutilDDIntWait(MemAreaTCPKV area, uint unBeginWord, int[] strValue, int unMilliseconds = 1000)//写PLC32位数组
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = unBeginWord;
            task.m_unWordsCount = (uint)strValue.Length;
            task.m_nReadOrWrite = 1;
            task.m_nWriteDataType = 1;
            task.m_dData = new object[strValue.Length];
            for (int i = 0; i < strValue.Length; i++)
            {
                task.m_dData[i] = strValue[i];
            }
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality;
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                quality = true;
            }
            else
            {
                quality = false;
            }
            task.m_hEventFinish.Close();
            return quality;
        }

        /// <summary>写多个Float
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="strValue"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool WritePLCMutilFloatWait(MemAreaTCPKV area, uint iAddress, float[] value, int unMilliseconds = 1000)//写PLCfloat数组
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = 2;
            task.m_nReadOrWrite = 1;
            task.m_nWriteDataType = 3;
            task.m_dData = new object[2 * value.Length];
            for (int i = 0; i < value.Length; i++)
            {
                Byte[] temp = BitConverter.GetBytes((float)value[i]);
                Byte[] val = BitConverter.GetBytes((float)value[i]);
                val[3] = temp[2];
                val[2] = temp[3];
                val[1] = temp[0];
                val[0] = temp[1];
                int countA = i * 2;
                int countB = i * 2 + 1;
                task.m_dData[countA] = string.Format("{0:X02}{1:X02}", val[0], val[1]);
                task.m_dData[countB] = string.Format("{0:X02}{1:X02}", val[2], val[3]);
            }


            task.GenStrCmd();
            AddImmeTask(task);
            bool quality;
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                quality = true;
            }
            else
            {
                quality = false;
            }
            //bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return quality;
        }

        /// <summary>写双字等待完成
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="iAddress"></param>
        /// <param name="value"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool WritePLCDDIntWait(MemAreaTCPKV area, uint iAddress, int value, int unMilliseconds = 1000)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = 1;
            task.m_nReadOrWrite = 1;
            task.m_nWriteDataType = 1;
            task.m_dData = new object[1];
            task.m_dData[0] = value;
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality;
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                quality = true;
            }
            else
            {
                quality = false;
            }
            task.m_hEventFinish.Close();
            return quality;
        }

        /// <summary>写双字
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="iAddress"></param>
        /// <param name="value"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool WritePLCDDInt(MemAreaTCPKV area, uint iAddress, int value)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_PLCArea = area;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = 1;
            task.m_nReadOrWrite = 1;
            task.m_nWriteDataType = 1;
            task.m_dData = new object[1];
            task.m_dData[0] = value;
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        /// <summary>写浮点数等待完成
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="iAddress"></param>
        /// <param name="value"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool WritePLCFloatWait(MemAreaTCPKV area, uint iAddress, float value, int unMilliseconds = 1000)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = 2;
            task.m_nReadOrWrite = 1;
            task.m_nWriteDataType = 3;
            task.m_dData = new object[2];
            Byte[] temp = BitConverter.GetBytes((float)value);
            Byte[] val = BitConverter.GetBytes((float)value);
            //float index = BitConverter.ToSingle(val, 0);
            val[3] = temp[2];
            val[2] = temp[3];
            val[1] = temp[0];
            val[0] = temp[1];

            float index1 = BitConverter.ToSingle(val, 0);

            task.m_dData[0] = string.Format("{0:X02}{1:X02}", val[0], val[1]);
            task.m_dData[1] = string.Format("{0:X02}{1:X02}", val[2], val[3]);
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality;
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                quality = true;
            }
            else
            {
                quality = false;
            }
            //bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return quality;
        }

        /// <summary>写浮点数
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="iAddress"></param>
        /// <param name="value"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool WritePLCFloat(MemAreaTCPKV area, uint iAddress, float value)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_PLCArea = area;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = 2;
            task.m_nReadOrWrite = 1;
            task.m_nWriteDataType = 3;
            task.m_dData = new object[2];
            Byte[] temp = BitConverter.GetBytes((float)value);
            Byte[] val = BitConverter.GetBytes((float)value);
            val[3] = temp[2];
            val[2] = temp[3];
            val[1] = temp[0];
            val[0] = temp[1];
            task.m_dData[0] = string.Format("{0:X02}{1:X02}", val[0], val[1]);
            task.m_dData[1] = string.Format("{0:X02}{1:X02}", val[2], val[3]);
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        /// <summary>写浮点数等待完成
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="iAddress"></param>
        /// <param name="value"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool WritePLCDoubleWait(MemAreaTCPKV area, uint iAddress, double value, int unMilliseconds = 1000)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = 4;
            task.m_nReadOrWrite = 1;
            task.m_nWriteDataType = 4;
            task.m_dData = new object[4];
            Byte[] temp = BitConverter.GetBytes((double)value);
            Byte[] val = BitConverter.GetBytes((double)value);
            val[3] = temp[2];
            val[2] = temp[3];
            val[1] = temp[0];
            val[0] = temp[1];
            val[7] = temp[6];
            val[6] = temp[7];
            val[5] = temp[4];
            val[4] = temp[5];
            task.m_dData[0] = string.Format("{0:X02}{1:X02}", val[0], val[1]);
            task.m_dData[1] = string.Format("{0:X02}{1:X02}", val[2], val[3]);
            task.m_dData[2] = string.Format("{0:X02}{1:X02}", val[4], val[5]);
            task.m_dData[3] = string.Format("{0:X02}{1:X02}", val[6], val[7]);
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality;
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                quality = true;
            }
            else
            {
                quality = false;
            }
            //bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return quality;
        }

        /// <summary>写浮点数
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="iAddress"></param>
        /// <param name="value"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool WritePLCDouble(MemAreaTCPKV area, uint iAddress, double value)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = 4;
            task.m_nReadOrWrite = 1;
            task.m_nWriteDataType = 4;
            task.m_dData = new object[4];
            Byte[] temp = BitConverter.GetBytes((double)value);
            Byte[] val = BitConverter.GetBytes((double)value);
            val[3] = temp[2];
            val[2] = temp[3];
            val[1] = temp[0];
            val[0] = temp[1];
            val[7] = temp[6];
            val[6] = temp[7];
            val[5] = temp[4];
            val[4] = temp[5];
            task.m_dData[0] = string.Format("{0:X02}{1:X02}", val[0], val[1]);
            task.m_dData[1] = string.Format("{0:X02}{1:X02}", val[2], val[3]);
            task.m_dData[2] = string.Format("{0:X02}{1:X02}", val[4], val[5]);
            task.m_dData[3] = string.Format("{0:X02}{1:X02}", val[6], val[7]);
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        /// <summary>写字符串等待完成
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="iAddress"></param>
        /// <param name="value"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool WritePLCStringWait(MemAreaTCPKV area, uint iAddress, string value, int unMilliseconds = 1000)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_nReadOrWrite = 1;
            task.m_nWriteDataType = 2;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = (uint)(value.Length % 2 + value.Length / 2);
            byte[] temp = new byte[value.Length];
            byte[] change = new byte[value.Length];
            temp = Encoding.ASCII.GetBytes(value);
            #region 数据转换
            for (int i = 0; i < temp.Length; )
            {
                if (change.Length / 2 != 0)
                {

                    if (i == temp.Length - 1)
                    {
                        change[i] = temp[i];
                    }
                    else
                    {
                        change[i] = temp[i + 1];
                        change[i + 1] = temp[i];
                    }
                }
                else
                {
                    change[i] = temp[i + 1];
                    change[i + 1] = temp[i];
                }
                i += 2;
            }

            byte[] data;
            if (value.Length % 2 != 0)
            {
                data = new byte[value.Length + 1];
                Buffer.BlockCopy(change, 0, data, 0, change.Length);
                data[temp.Length] = 0X00;
            }
            else
            {
                data = new byte[value.Length];
                Buffer.BlockCopy(change, 0, data, 0, change.Length);
            }

            uint len = task.m_unWordsCount;
            task.m_dData = new object[len];
            for (int i = 0; i < data.Length; )
            {
                if (i != data.Length - 1)
                {
                    task.m_dData[i / 2] = string.Format("{0:X2}{1:X2}", data[i], data[i + 1]);
                }
                else
                {
                    task.m_dData[i / 2] = string.Format("{0:X2}", data[i]);
                }
                i = i + 2;
            }
            #endregion
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality;
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                quality = true;
            }
            else
            {
                quality = false;
            }
            task.m_hEventFinish.Close();
            return quality;
        }

        public bool WritePLCStringChangeWait(MemAreaTCPKV area, uint iAddress, string value, int unMilliseconds = 1000)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_nReadOrWrite = 1;
            task.m_nWriteDataType = 2;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = (uint)(value.Length % 2 + value.Length / 2);
            byte[] temp = new byte[value.Length];
            byte[] change = new byte[value.Length];
            temp = Encoding.ASCII.GetBytes(value);
            #region 数据转换
            byte[] data;
            if (value.Length % 2 != 0)
            {
                data = new byte[value.Length + 1];
                Buffer.BlockCopy(temp, 0, data, 0, temp.Length);
                data[temp.Length] = 0X00;
            }
            else
            {
                data = new byte[value.Length];
                Buffer.BlockCopy(temp, 0, data, 0, temp.Length);
            }

            uint len = task.m_unWordsCount;
            task.m_dData = new object[len];
            for (int i = 0; i < data.Length; )
            {
                if (i != data.Length - 1)
                {
                    task.m_dData[i / 2] = string.Format("{0:X2}{1:X2}", data[i], data[i + 1]);
                }
                else
                {
                    task.m_dData[i / 2] = string.Format("{0:X2}", data[i]);
                }
                i = i + 2;
            }
            #endregion
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality;
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                quality = true;
            }
            else
            {
                quality = false;
            }
            task.m_hEventFinish.Close();

            return quality;
        }

        /// <summary>写字符串
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="iAddress"></param>
        /// <param name="value"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool WritePLCString(MemAreaTCPKV area, uint iAddress, string value)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_nReadOrWrite = 1;
            task.m_nWriteDataType = 2;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = (uint)(value.Length % 2 + value.Length / 2);
            byte[] temp = new byte[value.Length];
            byte[] change = new byte[value.Length];
            temp = Encoding.ASCII.GetBytes(value);
            #region 数据转换
            for (int i = 0; i < temp.Length; )
            {
                if (change.Length / 2 != 0)
                {

                    if (i == temp.Length - 1)
                    {
                        change[i] = temp[i];
                    }
                    else
                    {
                        change[i] = temp[i + 1];
                        change[i + 1] = temp[i];
                    }
                }
                else
                {
                    change[i] = temp[i + 1];
                    change[i + 1] = temp[i];
                }
                i += 2;
            }

            byte[] data;
            if (value.Length % 2 != 0)
            {
                data = new byte[value.Length + 1];
                Buffer.BlockCopy(change, 0, data, 0, change.Length);
                data[temp.Length] = 0X00;
            }
            else
            {
                data = new byte[value.Length];
                Buffer.BlockCopy(change, 0, data, 0, change.Length);
            }

            uint len = task.m_unWordsCount;
            task.m_dData = new object[len];
            for (int i = 0; i < data.Length; )
            {
                if (i != data.Length - 1)
                {
                    task.m_dData[i / 2] = string.Format("{0:X2}{1:X2}", data[i], data[i + 1]);
                }
                else
                {
                    task.m_dData[i / 2] = string.Format("{0:X2}", data[i]);
                }
                i = i + 2;
            }
            #endregion
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        /// <summary>写字符串-高低字节交叉
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="iAddress"></param>
        /// <param name="value"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool WritePLCStringChange(MemAreaTCPKV area, uint iAddress, string value)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_nReadOrWrite = 1;
            task.m_nWriteDataType = 2;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = (uint)(value.Length % 2 + value.Length / 2);
            byte[] temp = new byte[value.Length];
            byte[] change = new byte[value.Length];
            temp = Encoding.ASCII.GetBytes(value);
            #region 数据转换
            byte[] data;
            if (value.Length % 2 != 0)
            {
                data = new byte[value.Length + 1];
                Buffer.BlockCopy(temp, 0, data, 0, temp.Length);
                data[temp.Length] = 0X00;
            }
            else
            {
                data = new byte[value.Length];
                Buffer.BlockCopy(temp, 0, data, 0, temp.Length);
            }

            uint len = task.m_unWordsCount;
            task.m_dData = new object[len];
            for (int i = 0; i < data.Length; )
            {
                if (i != data.Length - 1)
                {
                    task.m_dData[i / 2] = string.Format("{0:X2}{1:X2}", data[i], data[i + 1]);
                }
                else
                {
                    task.m_dData[i / 2] = string.Format("{0:X2}", data[i]);
                }
                i = i + 2;
            }
            #endregion
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        public bool ReadPLCBitWait(MemAreaTCPKV area, uint iAddress, int bit, out bool value, int unMilliseconds = 1000)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = iAddress;
            task.m_nReadOrWrite = 0;
            task.m_unWordsCount = 1;
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                short result = 0;
                quality = true;
                result = GetReadWord(area, iAddress);
                if (Convert.ToBoolean((result >> bit) & 0x1))//把其它bit设为0,再把当前位与0X1与计算，判断是否为0,或不为0
                    value = true;
                else
                    value = false;
            }
            else
            {
                value = false;
                quality = false;
            }
            task.m_hEventFinish.Close();

            return quality;
        }

        public bool ReadPLCBit(MemAreaTCPKV area, uint iAddress, int bit)
        {
            short result = 0;
            result = GetReadWord(area, iAddress);
            if (Convert.ToBoolean((result >> bit) & 0x1))//把其它bit设为0,再把当前位与0X1与计算，判断是否为0,或不为0
                return true;
            else
                return false;
        }

        public bool ReadPLCBit(MemAreaTCPKV area, uint iAddress, int bit, out uint time)
        {
            short result = 0;
            result = GetReadWord(area, iAddress);
            time = GetReadWordTimeStamp(area, iAddress);
            if (Convert.ToBoolean((result >> bit) & 0x1))//把其它bit设为0,再把当前位与0X1与计算，判断是否为0,或不为0
                return true;
            else
                return false;
        }

        public bool ReadPLCShortWait(MemAreaTCPKV area, uint iAddress, out short value, int unMilliseconds = 1000)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = iAddress;
            task.m_nReadOrWrite = 0;
            task.m_unWordsCount = 1;
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality;
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                quality = true;
                value = GetReadWord(area, iAddress);
            }
            else
            {
                value = 0;
                quality = false;
            }
            task.m_hEventFinish.Close();
            return quality;
        }

        public short ReadPLCShort(MemAreaTCPKV area, uint iAddress)
        {
            return GetReadWord(area, iAddress);
        }

        public short ReadPLCShort(MemAreaTCPKV area, uint iAddress, out uint time)
        {
            time = GetReadWordTimeStamp(area, iAddress);
            return GetReadWord(area, iAddress);
        }

        public short[] ReadArrayShortInt(MemAreaTCPKV area, uint iAddress, short unStringCount)
        {
            short[] readshortArray = new short[unStringCount];


            for (int i = 0; i < unStringCount; i++)
            {
                readshortArray[i] = GetReadWord(area, (uint)(iAddress + i));
            }

            return readshortArray;

        }

        /// <summary>读取16位数组
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="iAddress"></param>
        /// <param name="acount"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public short[] ReadPLCArrayShortIntWait(MemAreaTCPKV area, uint iAddress, int acount, int unMilliseconds = 1000)
        {
            short[] readshortArray = new short[acount];

            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = (uint)acount;
            task.m_nReadOrWrite = 1;
            task.m_nWriteDataType = 0;
            task.m_dData = new object[acount];
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            for (int i = 0; i < acount; i++)
            {
                readshortArray[i] = GetReadWord(area, (uint)(iAddress + i));
            }

            return readshortArray;
        }
        /// <summary>读取十六位无符号数组
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="iAddress"></param>
        /// <param name="acount"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public ushort[] ReadPLCArrayUShortIntWait(MemAreaTCPKV area, uint iAddress, int acount, out bool quality, int unMilliseconds = 1000)
        {
            ushort[] readshortArray = new ushort[acount];

            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = (uint)acount;
            task.m_nReadOrWrite = 26;
            task.m_nWriteDataType = 0;
            task.m_dData = new object[acount];
            task.GenStrCmd();
            AddImmeTask(task);
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                quality = true;
                //value = GetReadUWord(area, iAddress);
            }
            else
            {
                quality = false;
            }
            task.m_hEventFinish.Close();
            for (int i = 0; i < acount; i++)
            {
                readshortArray[i] = (ushort)GetReadUWord(area, (uint)(iAddress + i));
            }

            return readshortArray;
        }

        /// <summary>读int
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="iAddress"></param>
        /// <param name="value"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool ReadPLCDDIntWait(MemAreaTCPKV area, uint iAddress, out int value, int unMilliseconds = 1000)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = iAddress;
            task.m_nReadOrWrite = 0;
            task.m_unWordsCount = 2;
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                quality = true;
                value = ((ushort)GetReadWord(area, iAddress + 1)) * 256 * 256 + (ushort)GetReadWord(area, iAddress);
            }
            else
            {
                value = 0;
                quality = false;
            }
            task.m_hEventFinish.Close();
            return quality;
        }

        /// <summary>读int
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="iAddress"></param>
        /// <returns></returns>
        public int ReadPLCDDInt(MemAreaTCPKV area, uint iAddress)
        {
            return ((ushort)GetReadWord(area, iAddress + 1)) * 256 * 256 + (ushort)GetReadWord(area, iAddress);
        }

        public int ReadPLCDDInt(MemAreaTCPKV area, uint iAddress, out uint time)
        {
            time = GetReadWordTimeStamp(area, iAddress);
            return ((ushort)GetReadWord(area, iAddress + 1)) * 256 * 256 + (ushort)GetReadWord(area, iAddress);
        }

        public bool ReadPLCFloatWait(MemAreaTCPKV area, uint iAddress, out float value, int unMilliseconds = 1000)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = iAddress;
            task.m_nReadOrWrite = 0;
            task.m_unWordsCount = 2;
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality;
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                quality = true;
                Byte[] resp = new Byte[4];
                resp[2] = (Byte)(GetReadWord(area, iAddress + 1) & 0xFFFF);
                resp[3] = (Byte)((GetReadWord(area, iAddress + 1) >> 8) & 0xFF);
                resp[0] = (Byte)(GetReadWord(area, iAddress) & 0xFFFF);
                resp[1] = (Byte)((GetReadWord(area, iAddress) >> 8) & 0xFF);
                value = BitConverter.ToSingle(resp, 0);
            }
            else
            {
                quality = false;
                value = 0.0F;
            }
            task.m_hEventFinish.Close();
            return quality;
        }

        public float ReadPLCFloat(MemAreaTCPKV area, uint iAddress)
        {
            float value;
            Byte[] resp = new Byte[4];
            resp[2] = (Byte)(GetReadWord(area, iAddress + 1) & 0xFFFF);

            resp[3] = (Byte)((GetReadWord(area, iAddress + 1) >> 8) & 0xFF);
            resp[0] = (Byte)(GetReadWord(area, iAddress) & 0xFFFF);
            resp[1] = (Byte)((GetReadWord(area, iAddress) >> 8) & 0xFF);

            value = BitConverter.ToSingle(resp, 0);
            return value;
        }

        public float ReadPLCFloat(MemAreaTCPKV area, uint iAddress, out uint time)
        {
            time = GetReadWordTimeStamp(area, iAddress);
            float getDWord;
            Byte[] resp = new Byte[4];
            resp[2] = (Byte)(GetReadWord(area, iAddress + 1) & 0xFFFF);
            resp[3] = (Byte)((GetReadWord(area, iAddress + 1) >> 8) & 0xFF);
            resp[0] = (Byte)(GetReadWord(area, iAddress) & 0xFFFF);
            resp[1] = (Byte)((GetReadWord(area, iAddress) >> 8) & 0xFF);
            getDWord = BitConverter.ToSingle(resp, 0);
            return getDWord;
        }

        public bool ReadPLCDoubleWait(MemAreaTCPKV area, uint iAddress, out double value, int unMilliseconds = 1000)
        {
            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = iAddress;
            task.m_nReadOrWrite = 0;
            task.m_unWordsCount = 4;
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality;
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                quality = true;
                Byte[] resp = new Byte[8];
                resp[2] = (Byte)(GetReadWord(area, iAddress + 1) & 0xFFFF);
                resp[3] = (Byte)((GetReadWord(area, iAddress + 1) >> 8) & 0xFF);
                resp[0] = (Byte)(GetReadWord(area, iAddress) & 0xFFFF);
                resp[1] = (Byte)((GetReadWord(area, iAddress) >> 8) & 0xFF);
                resp[6] = (Byte)(GetReadWord(area, iAddress + 3) & 0xFFFF);
                resp[7] = (Byte)((GetReadWord(area, iAddress + 3) >> 8) & 0xFF);
                resp[4] = (Byte)(GetReadWord(area, iAddress + 2) & 0xFFFF);
                resp[5] = (Byte)((GetReadWord(area, iAddress + 2) >> 8) & 0xFF);
                value = BitConverter.ToDouble(resp, 0);
            }
            else
            {
                quality = false;
                value = 0.0F;
            }
            task.m_hEventFinish.Close();
            return quality;
        }

        public double ReadPLCDouble(MemAreaTCPKV area, uint iAddress)
        {
            double value;
            Byte[] resp = new Byte[8];
            resp[2] = (Byte)(GetReadWord(area, iAddress + 1) & 0xFFFF);
            resp[3] = (Byte)((GetReadWord(area, iAddress + 1) >> 8) & 0xFF);
            resp[0] = (Byte)(GetReadWord(area, iAddress) & 0xFFFF);
            resp[1] = (Byte)((GetReadWord(area, iAddress) >> 8) & 0xFF);
            resp[6] = (Byte)(GetReadWord(area, iAddress + 3) & 0xFFFF);
            resp[7] = (Byte)((GetReadWord(area, iAddress + 3) >> 8) & 0xFF);
            resp[4] = (Byte)(GetReadWord(area, iAddress + 2) & 0xFFFF);
            resp[5] = (Byte)((GetReadWord(area, iAddress + 2) >> 8) & 0xFF);
            value = BitConverter.ToDouble(resp, 0);
            return value;
        }

        public double ReadPLCDouble(MemAreaTCPKV area, uint iAddress, out uint time)
        {
            time = GetReadWordTimeStamp(area, iAddress);
            double getDWord;
            Byte[] resp = new Byte[8];
            resp[2] = (Byte)(GetReadWord(area, iAddress + 1) & 0xFFFF);
            resp[3] = (Byte)((GetReadWord(area, iAddress + 1) >> 8) & 0xFF);
            resp[0] = (Byte)(GetReadWord(area, iAddress) & 0xFFFF);
            resp[1] = (Byte)((GetReadWord(area, iAddress) >> 8) & 0xFF);
            getDWord = BitConverter.ToDouble(resp, 0);
            return getDWord;
        }

        public bool ReadPLCStringWait(MemAreaTCPKV area, uint iAddress, int num, out string value, int unMilliseconds = 1000)
        {

            TaskStructTCPKV task = new TaskStructTCPKV();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.MainCommand = (uint)MainCmd.MutilREAD;
            task.SubCommand = (uint)SubCmd.WordADDR;
            task.m_unBeginWord = iAddress;
            task.m_unWordsCount = (uint)num;
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality;
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                quality = true;
            }
            else
            {
                quality = false;
            }
            //bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            byte[] temp = new byte[num * 2];
            Array.Clear(temp, 0, num * 2);
            for (int j = 0, i = 0; j < num; j++)
            {
                temp[i] = (byte)(GetReadWord(area, (uint)(iAddress + j)) % 256);
                if ((i + 1) < num * 2)
                {
                    temp[i + 1] = (byte)(GetReadWord(area, (uint)(iAddress + j)) / 256);
                }
                i += 2;
            }

            string readString = null;
            for (int i = 0; i < num * 2; i++)
            {
                if (temp[i] != 0X00)
                {
                    readString += Encoding.ASCII.GetString(temp, i, 1);
                }
            }
            value = readString;
            return quality;
        }

        public string ReadPLCString(MemAreaTCPKV area, uint iAddress, int num)
        {

            byte[] temp = new byte[num * 2];
            Array.Clear(temp, 0, num * 2);
            for (int j = 0, i = 0; j < num; j++)
            {
                temp[i] = (byte)(GetReadWord(area, (uint)(iAddress + j)) % 256);
                if ((i + 1) < num * 2)
                {
                    temp[i + 1] = (byte)(GetReadWord(area, (uint)(iAddress + j)) / 256);
                }
                i += 2;
            }

            string readString = null;
            for (int i = 0; i < num * 2; i++)
            {
                if (temp[i] != 0X00)
                {
                    readString += Encoding.ASCII.GetString(temp, i, 1);
                }
            }
            return readString;
        }

        public string ReadPLCString(MemAreaTCPKV area, uint iAddress, int num, out uint time)
        {
            time = GetReadWordTimeStamp(area, iAddress);
            byte[] temp = new byte[num * 2];
            Array.Clear(temp, 0, num * 2);
            for (int j = 0, i = 0; j < num; j++)
            {
                temp[i] = (byte)(GetReadWord(area, (uint)(iAddress + j)) % 256);
                if ((i + 1) < num * 2)
                {
                    temp[i + 1] = (byte)(GetReadWord(area, (uint)(iAddress + j)) / 256);
                }
                i += 2;
            }

            string readString = null;
            for (int i = 0; i < num * 2; i++)
            {
                if (temp[i] != 0X00)
                {
                    readString += Encoding.ASCII.GetString(temp, i, 1);
                }
            }
            return readString;
        }

    }