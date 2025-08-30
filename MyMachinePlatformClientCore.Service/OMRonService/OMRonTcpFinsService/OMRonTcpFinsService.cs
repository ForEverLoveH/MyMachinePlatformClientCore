using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using MyMachinePlatformClientCore.Log.MyLogs;
using MyMachinePlatformClientCore.Service.LogService;

namespace MyMachinePlatformClientCore.Service.OMRonService ;

public class OMRonTcpFinsService
{
     [DllImport("kernel32", CharSet = CharSet.Auto)]
        private static extern uint GetTickCount();

        private Action<LogMessage> LogDataCallBack;

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

        internal Socket OmronTcpSocket = null;
        private Byte DA1 = 0;
        private Byte SA1 = 0;

        // event handles to synchronize threads
        private AutoResetEvent m_hImmeTaskEvent = null;

        private ManualResetEvent m_hThreadExitEvent = null;
        private object m_critSecPeriodList = new object();
        private object m_critSecImmeList = new object();
        private object m_critUpdateReadList = new object();
        private List<TaskStructTCPFINS> m_periodTaskList = new List<TaskStructTCPFINS>();
        private Queue<TaskStructTCPFINS> m_immeTaskList = new Queue<TaskStructTCPFINS>();

        private Dictionary<uint, short>[] m_mapReadPLCMem = new Dictionary<uint, short>[6]
        {
            new Dictionary<uint, short>(),
            new Dictionary<uint, short>(),
            new Dictionary<uint, short>(),
            new Dictionary<uint, short>(),
            new Dictionary<uint, short>(),
            new Dictionary<uint, short>(),
        };

        private Dictionary<uint, uint>[] m_mapReadPLCMemTimeStamp = new Dictionary<uint, uint>[6]
        {
            new Dictionary<uint, uint>(),
            new Dictionary<uint, uint>(),
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

        private WaitHandle GetImmeTaskEvtHandle()
        {
            return m_hImmeTaskEvent;
        }

        private WaitHandle GetThreadExitHandle()
        {
            return m_hThreadExitEvent;
        }

        /// <summary>
        /// SocketThreadProc任务线程句柄
        /// </summary>
        // private static System.Threading.Thread m_pSocketThreadProc;
        private System.Threading.Thread m_pSocketThreadProc;

        /// <summary>
        /// 欧姆龙通讯线程计数
        /// </summary>
        private static int omronTreadNums = 0;
        /// <summary>
        /// 
        /// </summary>
        private void SocketThreadProc()
        {
            bool bRun = true;
            int dwRes = 0;
            WaitHandle[] hArray = { null, null };
            hArray[0] = GetImmeTaskEvtHandle();
            hArray[1] = GetThreadExitHandle();
            while (bRun)
            {
                dwRes = WaitHandle.WaitAny(hArray, 20);
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
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="task"></param>
        private void AddImmeTask(TaskStructTCPFINS task)
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
        /// <summary>
        /// 
        /// </summary>
        private void ProcessImmeTask()
        {
            bool rst = false;
            // The task describe in the list
            if (m_immeTaskList == null)
            {
                return;
            }
            if (m_immeTaskList.Count == 0)
                return;
            TaskStructTCPFINS task;
            while (m_immeTaskList.Count != 0)
            {
                try
                {
                    lock (m_critSecImmeList)
                    {
                        // task = m_immeTaskList[0];
                        task = m_immeTaskList.Dequeue();//移除并返回
                    }
                    rst = SendAndGetRes(task);

                    if (task.m_hEventFinish != null && rst == true)
                    {
                        // Tell the call thread (main thread of the program) that
                        // immediate task has finished.
                        task.m_hEventFinish.Set();//将
                    }
                    Thread.Sleep(20);
                }
                catch (System.Exception ex)
                {
                   LogDataCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "Omron TCPFINS 异常" + ex.Message));
                }
            }
        }

       /// <summary>
       ///  Use this in serve thread
       /// </summary>
        private void ProcessPeriodTask()
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
            foreach (TaskStructTCPFINS task in m_periodTaskList)
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
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="unWordsCount"></param>
        /// <returns></returns>
        public bool AddReadArea(MemAreaTCPFINS area, int unBeginWord, uint unWordsCount)
        {
            TaskStructTCPFINS task = new TaskStructTCPFINS();
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <returns></returns>
        private  bool SendAndGetRes(TaskStructTCPFINS pTask)
        {
            try
            {
                pTask.m_strCmd[20] = DA1;
                pTask.m_strCmd[23] = SA1;
                pTask.m_strCmd[25] = 0X01;
                Send(pTask.m_strCmd, pTask.DataLen);
                // wait for a plc response
                Byte[] respNADS = new Byte[pTask.RecvDateLen];
                Receive(ref respNADS, respNADS.Length);
                //返回头文件比较
                if (!(respNADS[0] == pTask.m_strCmd[0] && respNADS[1] == pTask.m_strCmd[1] && respNADS[2] == pTask.m_strCmd[2] && respNADS[3] == pTask.m_strCmd[3]))
                {
                    _lastError = "respNads return error 0-3";
                    return false;
                }
                //返回字节长度         
                byte[] _adr = BitConverter.GetBytes((short)pTask.RecvDateLen - 8);
                if (!(respNADS[4] == 0X00 && respNADS[5] == 0X00 && respNADS[6] == _adr[1] && respNADS[7] == _adr[0]))
                {
                    _lastError = "respNads return error 4-7";
                    return false;
                }
                //发送命令与接收命令
                if (!(respNADS[8] == pTask.m_strCmd[8] && respNADS[9] == pTask.m_strCmd[9] && respNADS[10] == pTask.m_strCmd[10] && respNADS[11] == pTask.m_strCmd[11]))
                {
                    _lastError = "respNads return error 8-11";
                    return false;
                }

                //错误代码Error
                if (!(respNADS[12] == pTask.m_strCmd[12] && respNADS[13] == pTask.m_strCmd[13] && respNADS[14] == pTask.m_strCmd[14] && respNADS[15] == pTask.m_strCmd[15]))
                {
                    _lastError = "respNads return error 12-15";
                    return false;
                }

                //ICF *** RSV***GTC***DNA
                if (!(respNADS[16] == 0XC0 && respNADS[17] == pTask.m_strCmd[17] && respNADS[18] == pTask.m_strCmd[18] && respNADS[19] == pTask.m_strCmd[19]))
                {
                    _lastError = "respNads return error 16-19";
                    return false;
                }

                //dst_rout ***DA2 *** SNA *** src_rout// [20]PLC节点 (现在设定8号机为08节点)// [23]PC端IP最后一位172.168.250.199
                if (!(respNADS[20] == pTask.m_strCmd[23] && respNADS[21] == pTask.m_strCmd[21] && respNADS[22] == pTask.m_strCmd[22] && respNADS[23] == pTask.m_strCmd[20]))
                {
                    _lastError = "respNads return error 20-33";
                    return false;
                }

                //SA2 ***SID
                if (!(respNADS[24] == pTask.m_strCmd[24] && respNADS[25] == pTask.m_strCmd[25]))
                {
                    _lastError = "<Read>respNads return error 24-25";
                    return false;
                }
                if (pTask.m_nReadOrWrite == 0 /*Read*/)
                {
                    if (respNADS[26 + 0] == 0X01 && respNADS[26 + 1] == 0X01 && respNADS[26 + 2] == 0X00 && respNADS[26 + 3] == 0X00)//读内存
                    {
                        for (UInt32 i = 0; i < (int)(pTask.m_unWordsCount); i++)
                        {
                            Int16 value = respNADS[26 + i * 2 + 4];
                            value <<= 8;
                            value += Convert.ToInt16(respNADS[26 + i * 2 + 5]);
                            UpdateReadWord(pTask.m_PLCArea, (uint)pTask.m_unBeginWord + i, value);
                        }
                        return true;
                    }
                    else
                    {
                        _lastError = "<Read>respNads return error 26-29";
                        Close();
                        return false;
                    }
                }
                else/*write*/
                {
                    if (respNADS[26 + 0] == 0X01 && respNADS[26 + 1] == 0X02 && respNADS[26 + 2] == 0X00 && respNADS[26 + 3] == 0X00)//读内存
                    {
                        return true;
                    }
                    else
                    {
                        //错误
                        _lastError = "<write>respNads return error 26-29";
                        Close();
                        return false;
                    }
                }
            }
            catch (System.Exception ex)
            {
                _lastError = ex.Message;
                Close();
                LogDataCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,"Omron TCPFINS SendAndGetRes 异常" + ex.Message));
                return false;
            }
            return true;
        }

        #region **** properties

        /// <summary>
        /// returns the connection status
        /// </summary>
        public bool Connected
        {
            get
            {
                try
                {
                    return (OmronTcpSocket == null) ? false : OmronTcpSocket.Connected;
                }
                catch (Exception ex)
                {
                    LogDataCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, ex.Message));
                    return false;
                }
            }
        }

        #endregion **** properties

        /// <summary>
        /// 构造函数
        /// </summary>
        public OMRonTcpFinsService(Action<LogMessage>logDataCallBack=null)
        {
            this._ping = new Ping();
            this._endPoint = new IPEndPoint(0, 0);
            this.LogDataCallBack = logDataCallBack;
        }
         /// <summary>
        /// set ip and port
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        public void SetTCPParams(IPAddress ip, int port)
        {
            this._endPoint.Address = ip;
            this._endPoint.Port = port;
        }

        /// <summary>
        /// 连接欧姆龙以太网
        /// </summary>
        /// <returns></returns>
        private bool TCPConnect()
        {
            try
            {
                if (OmronTcpSocket != null)
                {
                    OmronTcpSocket.Dispose();
                }
                OmronTcpSocket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                OmronTcpSocket.SendTimeout = _timeout;
                OmronTcpSocket.ReceiveTimeout = _timeout;
                OmronTcpSocket.SendBufferSize = 1024000000;
                OmronTcpSocket.ReceiveBufferSize = 1024000000;

                OmronTcpSocket.Connect(this._endPoint);
                return this.Connected;
            }
            catch (System.Exception ex)//SocketException error
            {
                /////////////////////////////////错误处理////////////////////////////////////////
                /////////////////////////////////
                //Sleep(10);
                System.Threading.Thread.Sleep(10);
                return false;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Connect()
        {
            if (this.Connected == true && this.initConnect == true)
            {
                return true;
            }

            try
            {
                if (this.TCPConnect())
                {
                    return NodeAddressDataSend();
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
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
                if (OmronTcpSocket == null) return;
                if (Connected)
                {
                    OmronTcpSocket.Disconnect(false);
                    OmronTcpSocket.Close();
                }
                OmronTcpSocket.Dispose();
                OmronTcpSocket = null;
            }
            Thread.Sleep(300);
        }

        /// <summary>
        /// 欧姆龙握手信号初始化
        /// </summary>
        /// <returns></returns>
        private bool initConnect = false;

        private bool NodeAddressDataSend()
        {
            // NODE ADDRESS DATA SEND buffer
            //
            Byte[] cmdNADS = new Byte[]
			{
			0x46, 0x49, 0x4E, 0x53, // 'F' 'I' 'N' 'S'
			0x00, 0x00, 0x00, 0x0C,	// 12 Bytes expected  cmdNADS[6] = 0x00;//Slave address cmdNADS[7] = 0x0C;//m_cmdword;//命令字功能码
			0x00, 0x00, 0x00, 0x00,	// NADS Command (0 Client to server, 1 server to client)
			0x00, 0x00, 0x00, 0x00,	// Error code (Not used)
			0x00, 0x00, 0x00, 0x00	// Client node address, 0 = auto assigned cmdNADS[19] = src_rout;（本机IP地址最后1位）
			};
            // send NADS command
            //
            Send(cmdNADS, cmdNADS.Length);
            // wait for a plc response
            //
            Byte[] respNADS = new Byte[24];
            Receive(ref respNADS, respNADS.Length);

            // checks response error
            //核对接收数据
            if (respNADS[15] != 0)
            {
                // no more actions
                //
                _lastError = "NASD command error respNADS[15] : " + respNADS[15];
                initConnect = false;
                Close();
                return false;
            }
            // checking header error
            if (respNADS[8] != 0 || respNADS[9] != 0 || respNADS[10] != 0 || respNADS[11] != 1)
            {
                this._lastError = "Error sending NADS command respNADS[8]...respNADS[11]. "
                                    + respNADS[8].ToString() + " "
                                    + respNADS[9].ToString() + " "
                                    + respNADS[10].ToString() + " "
                                    + respNADS[11].ToString();
                // no more actions
                //
                initConnect = false;
                Close();
                return false;
            }
            // save the client & server node in the FINS command for all next conversations
            //
            DA1 = respNADS[23];////节点数
            SA1 = respNADS[19];//本机IP地址

            //初始化句柄

            if (m_hThreadExitEvent == null)
            {
                m_hThreadExitEvent = new ManualResetEvent(false);
            }

            if (m_hImmeTaskEvent == null)
            {
                m_hImmeTaskEvent = new AutoResetEvent(false);
            }

            //接下来是开通线程
            if (m_pSocketThreadProc == null)
            {
                m_pSocketThreadProc = new System.Threading.Thread(SocketThreadProc);
                m_pSocketThreadProc.Start();
                omronTreadNums++;
                
            }

            initConnect = true;
            return true;
        }

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
            int bytesSent = OmronTcpSocket.Send(command, cmdLen, SocketFlags.None);

            // it checks the number of bytes sent
            //
            if (bytesSent != cmdLen)
            {
                string msg = string.Format("Sending error. (Expected bytes: {0}  Sent: {1})"
                                            , cmdLen, bytesSent);
                LogDataCallBack?.Invoke( new LogMessage()
                {
                    _LogType = LogType.WARN,
                    message = msg,
                });
                
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
            int bytesRecv = OmronTcpSocket.Receive(response, respLen, SocketFlags.None);

            // check the number of bytes received
            //
            if (bytesRecv != respLen)
            {
                string msg = string.Format("Receiving error. (Expected: {0}  Received: {1})"
                                            , respLen, bytesRecv);
               LogDataCallBack?.Invoke(new LogMessage()
               {
                   _LogType = LogType.WARN,
                   message = msg
               });
            }
            return bytesRecv;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="bit"></param>
        /// <returns></returns>
        public bool ReadPLCBit(MemAreaTCPFINS area, uint unBeginWord, int bit)
        {
            short result = 0;
            result = GetReadWord(area, unBeginWord);
            if (Convert.ToBoolean((result >> bit) & 0x1))//把其它bit设为0,再把当前位与0X1与计算，判断是否为0,或不为0
                return true;
            else
                return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="bit"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public bool ReadPLCBit(MemAreaTCPFINS area, uint unBeginWord, int bit, out uint time)
        {
            short result = 0;
            result = GetReadWord(area, unBeginWord);
            time = GetReadWordTimeStamp(area, unBeginWord);
            if (Convert.ToBoolean((result >> bit) & 0x1))//把其它bit设为0,再把当前位与0X1与计算，判断是否为0,或不为0
                return true;
            else
                return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <returns></returns>
        public short ReadPLCShort(MemAreaTCPFINS area, uint unBeginWord)
        {
            return GetReadWord(area, unBeginWord);
        }

        public short ReadPLCShort(MemAreaTCPFINS area, uint unBeginWord, out uint time)
        {
            time = GetReadWordTimeStamp(area, unBeginWord);
            return GetReadWord(area, unBeginWord);
        }

        public int ReadPLCDDInt(MemAreaTCPFINS area, uint unBeginWord)
        {
            return ((ushort)GetReadWord(area, unBeginWord + 1)) * 256 * 256 + (ushort)GetReadWord(area, unBeginWord);
        }

        public int ReadPLCDDInt(MemAreaTCPFINS area, uint unBeginWord, out uint time)
        {
            time = GetReadWordTimeStamp(area, unBeginWord);
            return ((ushort)GetReadWord(area, unBeginWord + 1)) * 256 * 256 + (ushort)GetReadWord(area, unBeginWord);
        }

        public float ReadPLCFloat(MemAreaTCPFINS area, uint unBeginWord)
        {
            float getDWord;
            Byte[] resp = new Byte[4];
            resp[2] = (Byte)(GetReadWord(area, unBeginWord + 1) & 0xFFFF);
            resp[3] = (Byte)((GetReadWord(area, unBeginWord + 1) >> 8) & 0xFF);
            resp[0] = (Byte)(GetReadWord(area, unBeginWord) & 0xFFFF);
            resp[1] = (Byte)((GetReadWord(area, unBeginWord) >> 8) & 0xFF);
            getDWord = BitConverter.ToSingle(resp, 0);
            return getDWord;
        }

        public float ReadPLCFloat(MemAreaTCPFINS area, uint unBeginWord, out uint time)
        {
            time = GetReadWordTimeStamp(area, unBeginWord);
            float getDWord;
            Byte[] resp = new Byte[4];
            resp[2] = (Byte)(GetReadWord(area, unBeginWord + 1) & 0xFFFF);
            resp[3] = (Byte)((GetReadWord(area, unBeginWord + 1) >> 8) & 0xFF);
            resp[0] = (Byte)(GetReadWord(area, unBeginWord) & 0xFFFF);
            resp[1] = (Byte)((GetReadWord(area, unBeginWord) >> 8) & 0xFF);
            getDWord = BitConverter.ToSingle(resp, 0);
            return getDWord;
        }

        public string ReadPLCString(MemAreaTCPFINS area, uint unBeginWord, short unStringCount)
        {
            short inttemp1 = 0;
            byte[] temp = new byte[unStringCount];
            Array.Clear(temp, 0, unStringCount);
            inttemp1 = (short)(unStringCount % 2);
            for (int i = 0; i < unStringCount; i++, i++)
            {
                temp[i] = (byte)(GetReadWord(area, (uint)(unBeginWord + i / 2)) / 256);
                if ((i + 1) < unStringCount)
                {
                    temp[i + 1] = (byte)(GetReadWord(area, (uint)(unBeginWord + i / 2)) % 256);
                }
            }
            string readString = null;
            for (int i = 0; i < unStringCount; i++)
            {
                if (temp[i] != 0X00)
                {
                    readString += Encoding.ASCII.GetString(temp, i, 1);
                }
            }
            return readString;
        }

        public short[] ReadArrayShortInt(MemAreaTCPFINS area, uint unBeginWord, short unStringCount)
        {
            short[] readshortArray = new short[unStringCount];

            for (int i = 0; i < unStringCount; i++)
            {
                readshortArray[i] = GetReadWord(area, (uint)(unBeginWord + i));
            }

            return readshortArray;
        }

        public short[] ReadPLCArrayShortIntWait(MemAreaTCPFINS area, uint unBeginWord, uint unWordcounts, int unMilliseconds = 1000)
        {
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_unWordsCount = unWordcounts;
            task.m_nWordOrDWord = 0;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            // Now we add an immediate task, when the task is processed the read value
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            short[] shortArray = new short[unWordcounts];
            for (int i = 0; i < unWordcounts; i++)
            {
                shortArray[i] = GetReadWord(area, (uint)(unBeginWord + i));
            }
            return shortArray;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="unStringCount"></param>
        /// <returns></returns>
        public string ReadPLCString(MemAreaTCPFINS area, uint unBeginWord, short unStringCount, out uint time)
        {
            time = GetReadWordTimeStamp(area, unBeginWord);
            short inttemp1 = 0;
            byte[] temp = new byte[unStringCount];
            Array.Clear(temp, 0, unStringCount);
            inttemp1 = (short)(unStringCount % 2);
            for (int i = 0; i < unStringCount; i++, i++)
            {
                temp[i] = (byte)(GetReadWord(area, (uint)(unBeginWord + i / 2)) / 256);
                if ((i + 1) < unStringCount)
                {
                    temp[i + 1] = (byte)(GetReadWord(area, (uint)(unBeginWord + i / 2)) % 256);
                }
            }
            string readString = null;
            for (int i = 0; i < unStringCount; i++)
            {
                if (temp[i] != 0X00)
                {
                    readString += Encoding.ASCII.GetString(temp, i, 1);
                }
            }
            return readString;
        }

        public string ReadPLCStringChange(MemAreaTCPFINS area, uint unBeginWord, short unStringCount)
        {
            short inttemp1 = 0;
            byte[] temp = new byte[unStringCount];
            Array.Clear(temp, 0, unStringCount);
            inttemp1 = (short)(unStringCount % 2);
            for (int i = 0; i < unStringCount; i++, i++)
            {
                temp[i] = (byte)(GetReadWord(area, (uint)(unBeginWord + i / 2)) % 256);
                if ((i + 1) < unStringCount)
                {
                    temp[i + 1] = (byte)(GetReadWord(area, (uint)(unBeginWord + i / 2)) / 256);
                }
            }
            string readString = "";
            for (int i = 0; i < unStringCount; i++)
            {
                if (temp[i] != 0X00)
                {
                    readString += Encoding.ASCII.GetString(temp, i, 1);
                }
            }
            return readString;
        }

        public bool ReadPLCBitWait(MemAreaTCPFINS area, uint unBeginWord, int bit, int unMilliseconds = 1000)
        {
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_unWordsCount = 1;
            task.m_nWordOrDWord = 0;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);//超时
            task.m_hEventFinish.Close();
            // Now we add an immediate task, when the task is processed the read value
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            short result = 0;
            result = GetReadWord(area, unBeginWord);
            if (Convert.ToBoolean((result >> bit) & 0x1))//把其它bit设为0,再把当前位与0X1与计算，判断是否为0,或不为0
                return true;
            else
                return false;
        }

        public bool ReadPLCBitWait(MemAreaTCPFINS area, uint unBeginWord, int bit, int unMilliseconds, out bool Quality)
        {
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_unWordsCount = 1;
            task.m_nWordOrDWord = 0;
            task.GenStrCmd();
            AddImmeTask(task);
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                Quality = true;
            }
            else
            {
                Quality = false;
            }
            task.m_hEventFinish.Close();
            // Now we add an immediate task, when the task is processed the read value
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            short result = 0;
            result = GetReadWord(area, unBeginWord);
            if (Convert.ToBoolean((result >> bit) & 0x1))//把其它bit设为0,再把当前位与0X1与计算，判断是否为0,或不为0
                return true;
            else
                return false;
        }

        public short ReadPLCShortWait(MemAreaTCPFINS area, uint unBeginWord, int unMilliseconds = 1000)
        {
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_unWordsCount = 1;
            task.m_nWordOrDWord = 0;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            // Now we add an immediate task, when the task is processed the read value
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            return GetReadWord(area, unBeginWord);
        }

        public short ReadPLCShortWait(MemAreaTCPFINS area, uint unBeginWord, int unMilliseconds, out bool Quality)
        {
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_unWordsCount = 1;
            task.m_nWordOrDWord = 0;
            task.GenStrCmd();
            AddImmeTask(task);
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                Quality = true;
            }
            else
            {
                Quality = false;
            }
            task.m_hEventFinish.Close();
            // Now we add an immediate task, when the task is processed the read value
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            return GetReadWord(area, unBeginWord);
        }

        public int ReadPLCDDIntWait(MemAreaTCPFINS area, uint unBeginWord, int unMilliseconds = 1000)
        {
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_unWordsCount = 2;
            task.m_nWordOrDWord = 0;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            // Now we add an immediate task, when the task is processed the read value
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            short a = GetReadWord(area, unBeginWord);
            short b = GetReadWord(area, unBeginWord + 1);
            short[] c = new short[2];
            c[0] = a;
            c[1] = b;
            int[] ret = new int[1];
            Buffer.BlockCopy(c, 0, ret, 0, 4);
            //  return ((ushort)GetReadWord(area, unBeginWord + 1)) * 256 * 256 + GetReadWord(area, unBeginWord);
            return ret[0];
        }

        public int ReadPLCDDIntWait(MemAreaTCPFINS area, uint unBeginWord, int unMilliseconds, out bool Quality)
        {
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_unWordsCount = 2;
            task.m_nWordOrDWord = 0;
            task.GenStrCmd();
            AddImmeTask(task);
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                Quality = true;
            }
            else
            {
                Quality = false;
            }
            task.m_hEventFinish.Close();
            // Now we add an immediate task, when the task is processed the read value
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            return GetReadWord(area, unBeginWord + 1) * 256 * 256 + GetReadWord(area, unBeginWord);
        }

        public float ReadPLCFloatWait(MemAreaTCPFINS area, uint unBeginWord, int unMilliseconds = 1000)
        {
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_unWordsCount = 2;
            task.m_nWordOrDWord = 0;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            float getDWord;
            Byte[] resp = new Byte[4];
            resp[2] = (Byte)(GetReadWord(area, unBeginWord + 1) & 0xFFFF);
            resp[3] = (Byte)((GetReadWord(area, unBeginWord + 1) >> 8) & 0xFF);
            resp[0] = (Byte)(GetReadWord(area, unBeginWord) & 0xFFFF);
            resp[1] = (Byte)((GetReadWord(area, unBeginWord) >> 8) & 0xFF);
            getDWord = BitConverter.ToSingle(resp, 0);
            return getDWord;
        }

        public float ReadPLCFloatWait(MemAreaTCPFINS area, uint unBeginWord, int unMilliseconds, out bool Quality)
        {
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_unWordsCount = 2;
            task.m_nWordOrDWord = 0;
            task.GenStrCmd();
            AddImmeTask(task);
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                Quality = true;
            }
            else
            {
                Quality = false;
            }
            task.m_hEventFinish.Close();
            float getDWord;
            Byte[] resp = new Byte[4];
            resp[2] = (Byte)(GetReadWord(area, unBeginWord + 1) & 0xFFFF);
            resp[3] = (Byte)((GetReadWord(area, unBeginWord + 1) >> 8) & 0xFF);
            resp[0] = (Byte)(GetReadWord(area, unBeginWord) & 0xFFFF);
            resp[1] = (Byte)((GetReadWord(area, unBeginWord) >> 8) & 0xFF);
            getDWord = BitConverter.ToSingle(resp, 0);
            return getDWord;
        }

        // Write PLC bit in overlapped way
        public bool WritePLCBit(MemAreaTCPFINS area, uint unBeginWord, int unBit, short wValue)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 1; // Write
            task.m_nWordOrBit = 1; // Bit
            task.m_nWordOrDWord = 0;//word;
            task.m_PLCArea = area;
            task.m_unBit = (uint)unBit;
            task.m_unBeginWord = (int)unBeginWord;
            if (Convert.ToBoolean(wValue))
            {
                task.m_wWriteValue = 1;
            }
            else
            {
                task.m_wWriteValue = 0;
            }
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        public bool WritePLCShort(MemAreaTCPFINS area, uint unBeginWord, short shortintValue)//写PLC16位数据(有符号)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 1; // Write
            task.m_nWordOrBit = 0; // Word
            task.m_nWordOrDWord = 0;//word;
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_wWriteValue = shortintValue;
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        public bool WritePLCDInt(MemAreaTCPFINS area, uint unBeginWord, int nValue)//写PLC16位数据(有符号)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 1; // Write
            task.m_nWordOrBit = 0; // Word
            task.m_nWordOrDWord = 1;//Dword;
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_dwWriteValue = nValue;
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        public bool WritePLCFloat(MemAreaTCPFINS area, uint unBeginWord, float fValue)//写PLC32位数据(浮点数)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 1; // Write
            task.m_nWordOrBit = 0; // Word
            task.m_nWordOrDWord = 1;//Dword;
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            Byte[] temp = new Byte[4];
            temp = BitConverter.GetBytes(fValue);
            task.m_dwWriteValue = BitConverter.ToInt32(temp, 0);
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        public bool WritePLCMutilShortInt(MemAreaTCPFINS area, uint unBeginWord, short[] strValue)//写PLC16位数组
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 1; // Write
            task.m_nWordOrBit = 0; // Word
            task.m_nWordOrDWord = 3;//MutilShort;
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_short = strValue;
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        public bool WritePLCMutilDDInt(MemAreaTCPFINS area, uint unBeginWord, uint[] strValue)//写PLC32位数组
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 1; // Write
            task.m_nWordOrBit = 0; // Word
            task.m_nWordOrDWord = 4;//MutilDword;
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_uint = strValue;
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        public bool WritePLCString(MemAreaTCPFINS area, uint unBeginWord, string strValue)//写PLC字符串数据
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 1; // Write
            task.m_nWordOrBit = 0; // Word
            task.m_nWordOrDWord = 2;//string;
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_strData = strValue;
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        // Write PLC bit in overlapped way
        public bool WritePLCBitWait(MemAreaTCPFINS area, uint unBeginWord, int unBit, short wValue, int unMilliseconds = 1000)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 1; // Write
            task.m_hEventFinish.Reset();
            task.m_nWordOrBit = 1; // Bit
            task.m_nWordOrDWord = 0;//word;
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_unBit = (uint)unBit;
            if (Convert.ToBoolean(wValue))
            {
                task.m_wWriteValue = 1;
            }
            else
            {
                task.m_wWriteValue = 0;
            }
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            // task.m_hEventFinish.Close();
            return true;
        }

        // Write PLC bit in overlapped way
        public bool WritePLCBitWait(MemAreaTCPFINS area, uint unBeginWord, int unBit, short wValue, int unMilliseconds, out bool Quality)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 1; // Write
            task.m_hEventFinish.Reset();//将线程设置为非终止状态
            task.m_nWordOrBit = 1; // Bit
            task.m_nWordOrDWord = 0;//word;
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_unBit = (uint)unBit;
            if (Convert.ToBoolean(wValue))
            {
                task.m_wWriteValue = 1;
            }
            else
            {
                task.m_wWriteValue = 0;
            }
            task.GenStrCmd();
            AddImmeTask(task);
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                Quality = true;
            }
            else
            {
                Quality = false;
            }
            //task.m_hEventFinish.Close();
            return true;
        }

        public bool WritePLCShortWait(MemAreaTCPFINS area, uint unBeginWord, short shortintValue, int unMilliseconds = 2000)//写PLC16位数据(有符号)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            bool Quality = false;
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 1; // Write
            task.m_hEventFinish.Reset();
            task.m_nWordOrBit = 0; // Word
            task.m_nWordOrDWord = 0;//word;
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_wWriteValue = shortintValue;
            task.GenStrCmd();
            AddImmeTask(task);
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                Quality = true;
            }
            else
            {
                Quality = false;
            }
            task.m_hEventFinish.Close();
            return Quality;
        }

        public bool WritePLCShortWait(MemAreaTCPFINS area, uint unBeginWord, short shortintValue, int unMilliseconds, out bool Quality)//写PLC16位数据(有符号)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 1; // Write
            task.m_hEventFinish.Reset();
            task.m_nWordOrBit = 0; // Word
            task.m_nWordOrDWord = 0;//word;
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_wWriteValue = shortintValue;
            task.GenStrCmd();
            AddImmeTask(task);
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                Quality = true;
            }
            else
            {
                Quality = false;
            }
            //     task.m_hEventFinish.Close();
            return true;
        }

        public bool WritePLCDIntWait(MemAreaTCPFINS area, uint unBeginWord, int nValue, int unMilliseconds = 1000)//写PLC16位数据(有符号)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 1; // Write
            task.m_hEventFinish.Reset();
            task.m_nWordOrBit = 0; // Word
            task.m_nWordOrDWord = 1;//Dword;
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_dwWriteValue = nValue;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return true;
        }

        public bool WritePLCDIntWait(MemAreaTCPFINS area, uint unBeginWord, int nValue, int unMilliseconds, out bool Quality)//写PLC16位数据(有符号)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 1; // Write
            task.m_hEventFinish.Reset();
            task.m_nWordOrBit = 0; // Word
            task.m_nWordOrDWord = 1;//Dword;
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_dwWriteValue = nValue;
            task.GenStrCmd();
            AddImmeTask(task);
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                Quality = true;
            }
            else
            {
                Quality = false;
            }
            task.m_hEventFinish.Close();
            return true;
        }
        
          public bool WritePLCFloatWait(MemAreaTCPFINS area, uint unBeginWord, float fValue, int unMilliseconds = 1000)//写PLC32位数据(浮点数)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 1; // Write
            task.m_hEventFinish.Reset();
            task.m_nWordOrBit = 0; // Word
            task.m_nWordOrDWord = 1;//Dword;
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            Byte[] temp = new Byte[4];
            temp = BitConverter.GetBytes(fValue);
            task.m_dwWriteValue = BitConverter.ToInt32(temp, 0);
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return true;
        }

        public bool WritePLCFloatWait(MemAreaTCPFINS area, uint unBeginWord, float fValue, int unMilliseconds, out bool Quality)//写PLC32位数据(浮点数)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_nReadOrWrite = 1; // Write
            task.m_hEventFinish.Reset();
            task.m_nWordOrBit = 0; // Word
            task.m_nWordOrDWord = 1;//Dword;
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            Byte[] temp = new Byte[4];
            temp = BitConverter.GetBytes(fValue);
            task.m_dwWriteValue = BitConverter.ToInt32(temp, 0);
            task.GenStrCmd();
            AddImmeTask(task);
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                Quality = true;
            }
            else
            {
                Quality = false;
            }
            task.m_hEventFinish.Close();
            return true;
        }

        public bool WritePLCStringWait(MemAreaTCPFINS area, uint unBeginWord, string strValue, int unMilliseconds = 1000)//写PLC32位数据(浮点数)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_hEventFinish.Reset();
            task.m_nReadOrWrite = 1; // Write
            task.m_nWordOrBit = 0; // Word
            task.m_nWordOrDWord = 2;//string;
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_strData = strValue;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return true;
        }

        public bool WritePLCStringWait(MemAreaTCPFINS area, uint unBeginWord, string strValue, int unMilliseconds, out bool Quality)//写PLC32位数据(浮点数)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructTCPFINS task = new TaskStructTCPFINS();
            task.m_hEventFinish.Reset();
            task.m_nReadOrWrite = 1; // Write
            task.m_nWordOrBit = 0; // Word
            task.m_nWordOrDWord = 2;//string;
            task.m_PLCArea = area;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_strData = strValue;
            task.GenStrCmd();
            AddImmeTask(task);
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                Quality = true;
            }
            else
            {
                Quality = false;
            }
            task.m_hEventFinish.Close();
            return true;
        }

        // PLC memory map operate
        private bool UpdateReadWord(MemAreaTCPFINS area, UInt32 unWordAddress, short wUpdate)
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

        private short GetReadWord(MemAreaTCPFINS area, uint unWordAddress)
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

        private uint GetReadWordTimeStamp(MemAreaTCPFINS area, uint unWordAddress)
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
    }
    