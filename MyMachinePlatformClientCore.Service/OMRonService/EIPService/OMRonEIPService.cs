using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using HslCommunication.Profinet.Omron;
using MyMachinePlatformClientCore.Log.MyLogs;

namespace MyMachinePlatformClientCore.Service.OMRonService;

public class OMRonEIPService
{
    public OMRonEIPService()
    {
        this._ping = new Ping();

        // EndPoint parametres
        //
        this._endPoint = new IPEndPoint(0, 0);
    }
    #region  变量注册

    private  OMRonEIPFunction function = new OMRonEIPFunction();
    private byte[] m_RegisterMsg = new byte[4];
    public bool m_IsRegistered = false;
    #endregion
    
    [DllImport("kernel32", CharSet = CharSet.Auto)]
    private static extern uint GetTickCount();
    public int BindPort { get; set; } //绑定本地端口
    /// <summary>是否读取NG
    /// 
    /// </summary>
    bool isReadNG = false;
    /// <summary>是否写入NG
    /// 
    /// </summary>
    bool isWriteNG = false;
    int m_ThreadConNum = 1;
    /// <summary>即时长度超长
    /// 
    /// </summary>
    bool isImmeTaskCountOut = false;
    /// <summary>周期长度超长
    /// 
    /// </summary>
    bool isPeriodTaskCountOut = false;

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
    private int TaskTime = 0;
    internal Socket OmronTcpSocket = null;
    private Byte DA1 = 0;
    private Byte SA1 = 0;
    // event handles to synchronize threads 
    AutoResetEvent m_hImmeTaskEvent = null;
    ManualResetEvent m_hThreadExitEvent = null;
    object m_critSecPeriodList = new object();
    object m_critSecImmeList = new object();
    object m_critUpdateReadList = new object();
    List<TaskStructEIP> m_periodTaskList = new List<TaskStructEIP>();
    Queue<TaskStructEIP> m_immeTaskList = new Queue<TaskStructEIP>();
    /// <summary>
    /// 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool IsBitDevice(MemAreaEIP type)
    {
        return !((type == MemAreaEIP.EIP)
                 || (type == MemAreaEIP.HR)
                 || (type == MemAreaEIP.AR)
                 || (type == MemAreaEIP.DM)
                 || (type == MemAreaEIP.EM)
                 || (type == MemAreaEIP.WR));
    }
    /// <summary>
    /// 欧姆龙握手信号初始化
    /// </summary>
    /// <returns></returns>
    private bool initConnect = false;
    
    #region 不同数据类型的地址字典
    Dictionary<string, object>[] m_mapReadPLCMem = new Dictionary<string, object>[1]
    {
        new Dictionary<string, object>(),
    };
    #region 非数组数据字典
    /// <summary>
    /// string地址字典
    /// </summary>
    Dictionary<string, string>[] m_mapReadPLCMemString = new Dictionary<string, string>[1]
    {
        new Dictionary<string, string>(),
    };
    /// <summary>
    /// short地址字典
    /// </summary>
    Dictionary<string, short>[] m_mapReadPLCMemShort = new Dictionary<string, short>[1]
    {
        new Dictionary<string, short>(),
    };
    /// <summary>
    /// ushort地址字典
    /// </summary>
    Dictionary<string, ushort>[] m_mapReadPLCMemUshort = new Dictionary<string, ushort>[1]
    {
        new Dictionary<string, ushort>(),
    };
    /// <summary>
    /// int地址字典
    /// </summary>
    Dictionary<string, int>[] m_mapReadPLCMemInt = new Dictionary<string, int>[1]
    {
        new Dictionary<string, int>(),
    };
    /// <summary>
    /// uint地址字典
    /// </summary>
    Dictionary<string, uint>[] m_mapReadPLCMemUint = new Dictionary<string, uint>[1]
    {
        new Dictionary<string, uint>(),
    };
    /// <summary>
    /// float地址字典
    /// </summary>
    Dictionary<string, float>[] m_mapReadPLCMemFlaot = new Dictionary<string, float>[1]
    {
        new Dictionary<string, float>(),
    };
    /// <summary>
    /// double地址字典
    /// </summary>
    Dictionary<string, double>[] m_mapReadPLCMemDouble = new Dictionary<string, double>[1]
    {
        new Dictionary<string, double>(),
    };
    /// <summary>
    /// bool地址字典
    /// </summary>
    Dictionary<string, bool>[] m_mapReadPLCMemBool = new Dictionary<string, bool>[1]
    {
        new Dictionary<string, bool>(),
    };
    /// <summary>
    /// byte地址字典
    /// </summary>
    Dictionary<string, byte>[] m_mapReadPLCMemByte = new Dictionary<string, byte>[1]
    {
        new Dictionary<string, byte>(),
    };
    #endregion
    
    #region 数组对象字典
       
       /// <summary>
        /// short数组地址字典
        /// </summary>
        Dictionary<string, short[]>[] m_mapReadPLCMemShortArr = new Dictionary<string, short[]>[1]
        {
            new Dictionary<string, short[]>(),
        };
        /// <summary>
        /// ushort数组地址字典
        /// </summary>
        Dictionary<string, ushort[]>[] m_mapReadPLCMemUShortArr = new Dictionary<string, ushort[]>[1]
        {
            new Dictionary<string, ushort[]>(),
        };

        /// <summary>
        /// int数组地址字典
        /// </summary>
        Dictionary<string, int[]>[] m_mapReadPLCMemIntArr = new Dictionary<string, int[]>[1]
        {
            new Dictionary<string, int[]>(),
        };
        /// <summary>
        /// uint数组地址字典
        /// </summary>
        Dictionary<string, uint[]>[] m_mapReadPLCMemUintArr = new Dictionary<string, uint[]>[1]
        {
            new Dictionary<string, uint[]>(),
        };
        /// <summary>
        /// float数组地址字典
        /// </summary>
        Dictionary<string, float[]>[] m_mapReadPLCMemFlaotArr = new Dictionary<string, float[]>[1]
        {
            new Dictionary<string, float[]>(),
        };
        /// <summary>
        /// double数组地址字典
        /// </summary>
        Dictionary<string, double[]>[] m_mapReadPLCMemDoubleArr = new Dictionary<string, double[]>[1]
        {
            new Dictionary<string, double[]>(),
        };
        /// <summary>
        /// bool数组地址字典
        /// </summary>
        Dictionary<string, bool[]>[] m_mapReadPLCMemBoolArr = new Dictionary<string, bool[]>[1]
        {
            new Dictionary<string, bool[]>(),
        };
        /// <summary>
        /// byte数组地址字典
        /// </summary>
        Dictionary<string, byte[]>[] m_mapReadPLCMemByteArr = new Dictionary<string, byte[]>[1]
        {
            new Dictionary<string, byte[]>(),
        };
        #endregion
        /// <summary>
        /// 变量更新时间戳字典
        /// </summary>
    Dictionary<string, uint>[] m_mapReadPLCMemTimeStamp = new Dictionary<string, uint>[1]
    {
        new Dictionary<string, uint>(),
    };
    #endregion
    
    private string _lastError = string.Empty;

    public string LastError
    {
        get=>_lastError;
    }
    public bool Connected { get; set; }
    private WaitHandle GetImmeTaskEvtHandle() { return m_hImmeTaskEvent; }
    private WaitHandle GetThreadExitHandle() { return m_hThreadExitEvent; }
    private System.Threading.Thread m_pSocketThreadProc;
    /// <summary>
    /// 
    /// </summary>
    private void SocketThreadProc()
    {
        int dwRes = 0;
        WaitHandle[] hArray = { null, null };
        hArray[0] = GetImmeTaskEvtHandle();
        hArray[1] = GetThreadExitHandle();

        while (true)
        {
            dwRes = WaitHandle.WaitAny(hArray, TaskTime);

            if (Connected)
            {
                switch (dwRes)
                {
                    case WaitHandle.WaitTimeout:
                        ProcessPeriodTask();
                        break;
                    case 0:
                        ProcessImmeTask();
                        break;
                }
            }
        }
    }
    /// <summary>执行即时任务
    /// 
    /// </summary>
   private void ProcessImmeTask()
    {
        bool rst = false;
        if (m_immeTaskList == null)
        {
            return;
        }
        if (m_immeTaskList.Count == 0)
            return;
        TaskStructEIP task;

        #region  任务条数异常校验-新增
        if (m_immeTaskList.Count > 80)
        {
            if (!isImmeTaskCountOut)
            {
                MyLogTool.Warn(string.Format("Omron EIP 异常err,m_immeTaskList.Count大于80：{0}", m_immeTaskList.Count));
                isImmeTaskCountOut = true;
            }

        }
        else
        {
            isImmeTaskCountOut = false;
        }
        #endregion

        while (m_immeTaskList.Count != 0)
        {
            try
            {
                lock (m_critSecImmeList)
                {
                    task = m_immeTaskList.Dequeue();
                }

                rst = SendAndGetRes(task);

                if (task.m_hEventFinish != null && rst == true)
                {
                    task.m_hEventFinish.Set();
                }

            }
            catch (System.Exception ex)
            {
                // PrintfLog.LogError("Omron EIP 异常" + ex.Message);
            }

        }
    }

    /// <summary>
    /// 添加即时任务
    /// </summary>
    /// <param name="task"></param>h
    private void AddImmeTask(TaskStructEIP task)
    {
        lock (m_critSecImmeList)
        {
            m_immeTaskList.Enqueue(task);
        }

        if (m_hImmeTaskEvent != null)
        {
            m_hImmeTaskEvent.Set();
        }
    }
    
    
    /// <summary>
    /// 添加周期任务
    /// </summary>
     private    void ProcessPeriodTask()
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
            Monitor.Enter(m_critSecPeriodList);

            #region 任务条数异常校验-新增
            if (m_periodTaskList.Count > 80)
            {
                if (!isPeriodTaskCountOut)
                {
                    MyLogTool.Warn(string.Format("Omron EIP 异常err,m_periodTaskList.Count大于80：{0}", m_periodTaskList.Count));
                    isPeriodTaskCountOut = true;
                }

            }
            else
            {
                isPeriodTaskCountOut = false;
            }
            #endregion

            Stopwatch stime = new Stopwatch();
            Stopwatch stimeTemp = new Stopwatch();

            long TimeNum = 0, TimeMax = 0;
            string MaxTag = string.Empty;

            stime.Start();//周期计时
            foreach (TaskStructEIP task in m_periodTaskList)
            {
                lock (m_critSecPeriodList)
                {
                    stimeTemp.Restart();//开始计时
                    bRes = SendAndGetRes(task);
                    stimeTemp.Stop();//结束计时

                    //记录最大耗时任务
                    if (TimeMax < stimeTemp.ElapsedMilliseconds)
                    {
                        TimeMax = stimeTemp.ElapsedMilliseconds;
                        MaxTag = task.m_strTag + "~" + stimeTemp.ElapsedMilliseconds;
                    }
                }
            }
            stime.Stop();//周期计时结束
            TimeNum = stime.ElapsedMilliseconds;
            if (TimeNum > 1000)//一个周期超过1s时，打印日志
            {
               MyLogTool.Log(string.Format("Omron EIP 异常err,m_periodTaskList周期任务执行周期超过1s：{0}，任务条数：{1}，最大超时变量：{2}，端口号：{3}", TimeNum, m_periodTaskList.Count, MaxTag, this.BindPort));
            }
            Monitor.Exit(m_critSecPeriodList);
        }
    /// <summary>长度检查-限制
    /// 
    /// </summary>
    /// <param name="dataType">类型</param>
    /// <param name="ReadLength">长度</param>
    /// <returns>检查修改后的长度</returns>
    private short LengthCheck(OMRonEIPDataType dataType)
    {
        short n_length = 1;
        switch (dataType)
        {
            case OMRonEIPDataType.IntArr:
            case OMRonEIPDataType.WordArr:
                n_length = 200;
                break;
            case OMRonEIPDataType.DwordArr:
            case OMRonEIPDataType.DIntArr:
            case OMRonEIPDataType.RealArr:
                n_length = 100;
                break;
            default:
                n_length = 1;
                break;
        }

        return n_length;
    }
    /// <summary>长度检查-限制
    /// 
    /// </summary>
    /// <param name="dataType">类型</param>
    /// <param name="ReadLength">长度</param>
    /// <returns>检查修改后的长度</returns>
    private short LengthCheck(OMRonEIPDataType dataType, short ReadLength)
    {
        short n_length = 1;
        switch (dataType)
        {
            case OMRonEIPDataType.IntArr:
            case OMRonEIPDataType.WordArr:
                if (ReadLength > 200 || ReadLength < 1)
                    n_length = 200;
                else
                    n_length = ReadLength;

                break;
            case OMRonEIPDataType.DwordArr:
            case OMRonEIPDataType.DIntArr:
            case OMRonEIPDataType.RealArr:
                if (ReadLength > 100 || ReadLength < 1)
                    n_length = 100;
                else
                    n_length = ReadLength;

                break;
            default:
                n_length = 1;
                break;
        }

        return n_length;
    }
    
    /// <summary>添加周期任务1--
    /// 可以读取长度较短的数组(int16[200-]; int32[100-])，不用给出长度，直接给出变量名即可。
    /// </summary>
    /// <param name="tag">标签名，变量名</param>
    /// <param name="dataType">类型</param>
    /// <param name="ReadLength">读取长度,通常不赋值。读取int32时不可超过100，读取int32时不可超过200，</param>
    /// <param name="area"></param>
    /// <returns></returns>
    public bool AddReadArea(string tag, OMRonEIPDataType dataType, short ReadLength = 1, MemAreaEIP area = MemAreaEIP.EIP)
    {
        short m_ReadLength = 1;
        m_ReadLength = LengthCheck(dataType, ReadLength);//读取长度-检查限制


        TaskStructEIP task = new TaskStructEIP();
        task.m_nReadOrWrite = 0;//read
        task.m_PLCArea = area;//
        task.m_strTag = tag;
        task.m_nDataType = dataType;
        task.m_ReadCount = m_ReadLength > 1 ? m_ReadLength : 1;
        //发送命令
        task.GenStrCmd(m_ReadLength);
        lock (m_critSecPeriodList)
        {
            m_periodTaskList.Add(task);
        }
        return true;
    }
    /// <summary>添加周期任务2--
    /// 读取超长数组 int16[200+];int32[100+];
    /// </summary>
    /// <param name="tag">标签名，变量名</param>
    /// <param name="dataType">类型,目前只支持</param>
    /// <param name="ArrayCount">PLC创建的数组 总长度为多少</param>
    /// <param name="isComArr">是否启用自动合包操作</param>
    /// <param name="ReadLength">单次读取长度，int16不可超过200，int32不可超过100</param>
    /// <param name="area"></param>
    /// <returns></returns>
    public bool AddReadLongArrArea(string tag,  OMRonEIPDataType dataType, int ArrayCount, bool isComArr = true, MemAreaEIP area = MemAreaEIP.EIP)
        {
            int n_ArrayIndex = 0;
            short n_ReadCount = 0;
            string n_tag = string.Empty;
            short m_ReadLength = 1;

            m_ReadLength = LengthCheck(dataType);//读取长度-检查限制

            if (isComArr)//一次读不完时，进行组合数组并合包
            {
                for (int i = 0; i < ArrayCount; i += m_ReadLength)
                {
                    n_ArrayIndex = i;

                    if (ArrayCount - n_ArrayIndex < m_ReadLength && ArrayCount - n_ArrayIndex > 0)//300-200
                    {
                        //n_ArrayIndex = n_ArrayIndex + (ArrayCount - n_ArrayIndex);//200=200+（300-200）

                        n_ReadCount = Convert.ToInt16(ArrayCount - n_ArrayIndex);//读取长度
                    }
                    else
                    {
                        n_ReadCount = m_ReadLength;//读取长度
                    }

                    TaskStructEIP task = new TaskStructEIP();
                    task.m_ArrayCount = ArrayCount;
                    task.isComArr = true;
                    task.m_ArrayIndex = n_ArrayIndex;
                    task.m_ReadCount = n_ReadCount;
                    n_tag = string.Format("{0}[{1}]", tag, n_ArrayIndex);

                    task.m_nReadOrWrite = 0;//read
                    task.m_PLCArea = area;//
                    task.m_strTag = n_tag;
                    task.m_nDataType = dataType;
                    //发送命令
                    task.GenStrCmd(n_ReadCount);
                    lock (m_critSecPeriodList)
                    {
                        m_periodTaskList.Add(task);
                    }

                }
            }


            return true;
        }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
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
    private void IsConnected()
    {
        try
        {
            if (OmronTcpSocket != null)
            {
                IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();

                TcpConnectionInformation[] tcpConnections = ipProperties.GetActiveTcpConnections();

                foreach (TcpConnectionInformation c in tcpConnections)
                {
                    TcpState stateOfConnection = c.State;

                    if (c.LocalEndPoint.Equals(OmronTcpSocket.LocalEndPoint) && c.RemoteEndPoint.Equals(OmronTcpSocket.RemoteEndPoint))
                    {
                        if (stateOfConnection == TcpState.Established)
                        {
                            Connected = true;
                            return;
                        }

                        break;
                    }
                }
            }

            initConnect = false;
            m_IsRegistered = false;
            Connected = false;
            Connect();
        }
        catch
        {
            initConnect = false;
            m_IsRegistered = false;
            Connected = false;
            Connect();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ip"></param>
    /// <param name="port"></param>
    /// <param name="bindPort"></param>
    /// <param name="TaskTime"></param>
    public void SetTCPParams(IPAddress ip, int port, int bindPort, int TaskTime = 10)
    {
        this.BindPort = bindPort;

        this._endPoint.Address = ip;
        this._endPoint.Port = port;
        this.TaskTime = TaskTime;

        if (m_hThreadExitEvent == null)
        {
            m_hThreadExitEvent = new ManualResetEvent(false);
        }

        if (m_hImmeTaskEvent == null)
        {
            m_hImmeTaskEvent = new AutoResetEvent(false);
        }

        //开启连接检测，并进行掉线重连
        Task.Run(() =>
        {
            while (true)
            {
                IsConnected();
                Thread.Sleep(100);
            }
        });

        //开启任务线程
        m_pSocketThreadProc = new Thread(SocketThreadProc);
        m_pSocketThreadProc.Start();
    }
    
     /// <summary>
     ///连接以太网
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

                OmronTcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                OmronTcpSocket.SendTimeout = _timeout;
                OmronTcpSocket.ReceiveTimeout = _timeout;


                // 设置发送缓冲区大小为64KB
                OmronTcpSocket.SendBufferSize = 64 * 1024;
                // 设置接收缓冲区大小为64KB
                OmronTcpSocket.ReceiveBufferSize = 64 * 1024;

                //// 设置发送队列大小为100
                //OmronTcpSocket.SendQueueSize = 100;
                //// 设置接收队列大小为100
                //OmronTcpSocket.ReceiveQueueSize = 100;

                if (BindPort > 1023) OmronTcpSocket.Bind(new IPEndPoint(IPAddress.Parse("0.0.0.0"), BindPort)); //指定本地端口

                OmronTcpSocket.Connect(_endPoint);

                #region KeepAlive机制
                OmronTcpSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true); //开启KeepAlive
                uint dummy = 0;
                byte[] inOptionValues = new byte[Marshal.SizeOf(dummy) * 3];
                BitConverter.GetBytes((uint)1).CopyTo(inOptionValues, 0);
                BitConverter.GetBytes((uint)300).CopyTo(inOptionValues, Marshal.SizeOf(dummy)); //keep-alive间隔
                BitConverter.GetBytes((uint)50).CopyTo(inOptionValues, Marshal.SizeOf(dummy) * 2); // 尝试间隔
                OmronTcpSocket.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
                #endregion

                IsConnected();
                Register();//eip驱动新增-注册

                try
                {
                    BindPort = 0;
                    int clientPort = ((IPEndPoint)OmronTcpSocket.LocalEndPoint).Port;
                    BindPort = clientPort;
                }
                catch (Exception)
                { }

                MyLogTool.Log(string.Format("PLC连接端口p{0}-第{1}次连接", BindPort, m_ThreadConNum++));
                return Connected;
            }
            catch (Exception ex)
            {
                MyLogTool.Log(string.Format("PLC连连接失败，err{0}", ex.ToString()));
                return false;
            }
        }
     /// <summary>
     /// 
     /// </summary>
     /// <returns></returns>
     public bool Connect()=>TCPConnect();
     /// <summary>
     /// 
     /// </summary>
     private void Register()
     {
         byte[] Registercmd = function.OmRonEipProtocol.RegisterCmd;//注册信息
         byte[] raed_resp = new byte[512];

         //1、发送注册信息
         Send(Registercmd, Registercmd.Length);
         //2、接受返回的注册ID
         Receive(ref raed_resp, raed_resp.Length);

         //3、替换命令中的注册信息
         // pTask.m_strCmd[4] = raed_resp[4]; pTask.m_strCmd[5] = raed_resp[5]; pTask.m_strCmd[6] = raed_resp[6]; pTask.m_strCmd[7] = raed_resp[7];
         //中转保存当前变量的通讯注册信息
         m_RegisterMsg[0] = raed_resp[4]; m_RegisterMsg[1] = raed_resp[5]; m_RegisterMsg[2] = raed_resp[6]; m_RegisterMsg[3] = raed_resp[7];
         //注册完成，将当前变量的注册状态变为已注册
         m_IsRegistered = true;

         initConnect = true;
     }
     /// <summary>
     /// 
     /// </summary>
     /// <param name="command"></param>
     /// <param name="cmdLen"></param>
     /// <returns></returns>
     /// <exception cref="Exception"></exception>
     private int Send(Byte[] command, int cmdLen)//发送报文
     {
         if (!Connected)
         {
             throw new Exception("Socket is not connected.");
         }
         int bytesSent = OmronTcpSocket.Send(command, cmdLen, SocketFlags.None);
         if (bytesSent != cmdLen)
         {
             string msg = string.Format("Sending error. (Expected bytes: {0}  Sent: {1})"
                 , cmdLen, bytesSent);
             throw new Exception(msg);
         }
         return bytesSent;
     }
     /// <summary>
     /// 
     /// </summary>
     /// <param name="response"></param>
     /// <param name="respLen"></param>
     /// <returns></returns>
     /// <exception cref="Exception"></exception>
     private int Receive(ref Byte[] response, int respLen)//接收报文
     {
         if (!Connected)
         {
             throw new Exception("Socket is not connected.");
         }
         int bytesRecv = OmronTcpSocket.Receive(response, respLen, SocketFlags.None);

         // check the number of bytes received
         //
         //if (bytesRecv != respLen)
         //{
         //    string msg = string.Format("Receiving error. (Expected: {0}  Received: {1})"
         //                                , respLen, bytesRecv);
         //    throw new Exception(msg);
         //}
         return bytesRecv;
     }
     /// <summary>
     /// 
     /// </summary>
     /// <param name="pTask"></param>
     /// <returns></returns>
     private   bool SendAndGetRes(TaskStructEIP pTask)//数据收发
        {
            #region 1.计时开始-测试效率时使用
            string readOrWrite = string.Empty;//读还是写
            //string timeStart = string.Empty;//开始时间
            //string timeEnd = string.Empty;//结束时间

            //Stopwatch stime = new Stopwatch();
            //long TimeNum = 0;
            //stime.Start();

            //timeStart = DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss.fff");
            #endregion

            #region 2.数据收发
            readOrWrite = pTask.m_nReadOrWrite == 0 ? "Read" : "Write";
            string strSend = "";
            int CNT = 0;
            byte[] rcv1;
            try
            {
                byte[] raed_resp = new byte[600];
                if (!m_IsRegistered)
                {
                    //1、发送注册信息
                    Send(pTask.Registercmd, pTask.Registercmd.Length);
                    //2、接受返回的注册ID
                    CNT = Receive(ref raed_resp, raed_resp.Length);

                    //3、替换命令中的注册信息
                    pTask.m_strCmd[4] = raed_resp[4]; pTask.m_strCmd[5] = raed_resp[5]; pTask.m_strCmd[6] = raed_resp[6]; pTask.m_strCmd[7] = raed_resp[7];
                    //中转保存当前变量的通讯注册信息
                    pTask.RegisterMsg[0] = raed_resp[4]; pTask.RegisterMsg[1] = raed_resp[5]; pTask.RegisterMsg[2] = raed_resp[6]; pTask.RegisterMsg[3] = raed_resp[7];
                    //注册完成，将当前变量的注册状态变为已注册
                    pTask.IsRegistered = true;
                }
                else
                {
                    pTask.m_strCmd[4] = m_RegisterMsg[0]; pTask.m_strCmd[5] = m_RegisterMsg[1]; pTask.m_strCmd[6] = m_RegisterMsg[2]; pTask.m_strCmd[7] = m_RegisterMsg[3];
                }

                strSend = BitConverter.ToString(pTask.m_strCmd);//-----------------------------------

                //4、发送读/写命令
                Send(pTask.m_strCmd, pTask.m_strCmd.Length);
                // Thread.Sleep(1);
                //5、接受返回的数据信息
                CNT = Receive(ref raed_resp, raed_resp.Length);
                // Thread.Sleep(10);
                rcv1 = new byte[CNT];
                Array.Copy(raed_resp, 0, rcv1, 0, CNT);

                string strReceive = BitConverter.ToString(rcv1);//-----------------------------------
                //6、解析返回的数据信息
                if (pTask.m_nReadOrWrite == 0 /*Read*/)
                {
                    // readOrWrite = "Read";
                    bool isReadOK = GetReadVelue(pTask, rcv1);
                    if (!isReadOK)
                    {
                       MyLogTool.Warn(string.Format("PLC驱动err,{0}异常:{1}", readOrWrite, pTask.m_strTag));
                        return false;
                    }
                }
                else/*write*/
                {
                    // readOrWrite = "Write";
                    byte[] byteLen = new byte[2];
                    byteLen[0] = rcv1[38];
                    byteLen[1] = rcv1[39];
                    int stateCodeLen = BitConverter.ToInt16(byteLen, 0);
                    stateCodeLen = stateCodeLen - 2;
                    for (int i = 42; i < 42 + stateCodeLen; i++)
                    {
                        try
                        {
                            if (rcv1[i] != 0x00)//状态
                            {
                                if (!isWriteNG)
                                {
                                    isWriteNG = true;
                                    MyLogTool.Warn(string.Format("PLC驱动err,{0}异常:{1}", readOrWrite, pTask.m_strTag));
                                }
                                return false;
                            }
                            else
                            {
                                isWriteNG = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            MyLogTool.Warn(string.Format("PLC驱动err,{0}异常:{1},ex:{2}", readOrWrite, pTask.m_strTag, ex.ToString()));
                        }
                    }
                }


            }
            catch (Exception ex)
            {
                MyLogTool.Log(string.Format("PLC驱动err,{0}异常:{1},ex:{2}", readOrWrite, pTask.m_strTag, ex.ToString()));
                return false;
            }
            #endregion

            #region 3.计时结束-测试效率时使用

            //timeEnd = DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss.fff");

            //stime.Stop();
            //TimeNum = stime.ElapsedMilliseconds;
            //CMachineCtrl.MsgListAdd(string.Format("{0},{1},耗时：{2},开始：{3},结束：{4}", readOrWrite, pTask.m_strTag, TimeNum, timeStart, timeEnd));

            #endregion

            return true;
        }
       /// <summary> 
       /// 数据解析
       /// </summary>
       /// <param name="pTask"></param>
       /// <param name="data"></param>
       /// <returns></returns>
      private  bool GetReadVelue(TaskStructEIP pTask, byte[] data)
        {
            string tag = pTask.m_strTag;
            object dataValue = new object();
            if (data[42] == 0x00 && data[43] == 0x00)
            {
                isReadNG = false;
                if (data[44] == 0xA0 && data[45] == 0x02)//读取的标签数据类型为结构体
                {
                    //暂不使用结构体读写
                }
                else//读取的标签数据类型为数组或其他基本数据类型
                {

                    switch (data[44])
                    {
                        case 0xc1:

                            #region 新的处理方式
                            int boolLen = data.Length - 46;//长度判断
                            bool[] rcv = new bool[boolLen * 8];
                            int k = 0;
                            for (int i = 0; i < boolLen; i++)
                            {
                                bool isTrue = false;
                                for (int j = 0; j < 8; j++)
                                {
                                    int res = ((data[46 + i] >> j) & 0x1);
                                    if (res == 1)
                                    {
                                        isTrue = true;
                                    }
                                    else
                                    {
                                        isTrue = false;
                                    }
                                    rcv[k] = isTrue;
                                    k++;
                                }
                            }
                            UpdateReadWord(pTask, rcv);
                            #endregion

                            #region BOOL旧-PLC的bool类型有8位 但是永远只有第一位可以置为1，所以下面的方法不适用

                            //int boolLen = data.Length - 46;

                            //if (tag.Contains("["))//
                            //{

                            //    bool rcv = false;
                            //    if (data[46] == 0x01)//BOOL
                            //    {
                            //        rcv = true;
                            //    }
                            //    else
                            //    {
                            //        rcv = false;
                            //    }
                            //    UpdateReadWord(tag, rcv);
                            //}
                            //else
                            //{
                            //    bool[] rcv = new bool[boolLen * 8];
                            //    int k = 0;
                            //    for (int i = 0; i < boolLen; i++)
                            //    {
                            //        bool isTrue = false;
                            //        for (int j = 0; j < 8; j++)
                            //        {
                            //            int res = ((data[46 + i] >> j) & 0x1);
                            //            if (res == 1)
                            //            {
                            //                isTrue = true;
                            //            }
                            //            else
                            //            {
                            //                isTrue = false;
                            //            }
                            //            rcv[k] = isTrue;
                            //            k++;
                            //        }
                            //    }
                            //    UpdateReadWord(tag, rcv);
                            //}

                            #endregion

                            break;
                        case 0xd0://读string字符串
                            #region 读string字符串

                            string strData = "";
                            byte[] byteLen = new byte[2];
                            byteLen[0] = data[46];
                            byteLen[1] = data[47];
                            int strLen = BitConverter.ToInt16(byteLen, 0);
                            for (int i = 48; i < 48 + strLen; i++)
                            {
                                strData = strData + (char)data[i];
                            }
                            UpdateReadWord(pTask, strData);

                            #endregion

                            break;
                        case 0xd1://BYTE

                            #region BYTE
                            byte resByte = data[46];
                            UpdateReadWord(pTask, resByte);
                            #endregion

                            break;
                        case 0xC2://读有符号单字INT等同于C#的short
                            #region //读有符号单字INT等同于C#的short

                            if (data.Length <= 48 && !pTask.isComArr)
                            {
                                byte[] dataI = new byte[2];
                                dataI[0] = data[46];
                                dataI[1] = 0;

                                short Idata = BitConverter.ToInt16(dataI, 0);
                                UpdateReadWord(pTask, Idata);
                            }
                            else
                            {
                                int len = pTask.isComArr ? pTask.m_ArrayCount : (data.Length - 46) / 2;
                                //int len = (data.Length - 46) / 2;
                                short[] sRcv = new short[len];
                                for (int i = 46, j = 0; i < data.Length; j++)
                                {
                                    byte[] dataI = new byte[2];
                                    dataI[0] = data[i];
                                    dataI[1] = data[i + 1];
                                    short Idata = BitConverter.ToInt16(dataI, 0);
                                    sRcv[j] = Idata;
                                    i = i + 2;
                                }
                                UpdateReadWord(pTask, sRcv);
                            }

                            #endregion
                            break;
                        case 0xC3://读有符号单字INT等同于C#的short
                            #region 读有符号单字INT等同于C#的short

                            if (data.Length <= 48 && !pTask.isComArr)
                            {
                                byte[] dataI = new byte[2];
                                dataI[0] = data[46];
                                dataI[1] = data[47];

                                short Idata = BitConverter.ToInt16(dataI, 0);
                                UpdateReadWord(pTask, Idata);
                            }
                            else
                            {
                                int len = pTask.isComArr ? pTask.m_ArrayCount : (data.Length - 46) / 2;
                                short[] sRcv = new short[len];
                                for (int i = 46, j = 0; i < data.Length; j++)
                                {
                                    byte[] dataI = new byte[2];
                                    dataI[0] = data[i];
                                    dataI[1] = data[i + 1];
                                    short Idata = BitConverter.ToInt16(dataI, 0);
                                    sRcv[j] = Idata;
                                    i = i + 2;
                                }
                                UpdateReadWord(pTask, sRcv);
                            }

                            #endregion

                            break;
                        case 0xc7://读PLC无符号UINT等同于C#的Ushort
                        case 0xd2://读PLC无符号单字Word等同于C#的Ushort
                            #region C#的Ushort

                            if (data.Length <= 48 && !pTask.isComArr)
                            {
                                byte[] dataI = new byte[2];
                                dataI[0] = data[46];
                                dataI[1] = data[47];

                                ushort Idata = BitConverter.ToUInt16(dataI, 0);
                                UpdateReadWord(pTask, Idata);
                                //UpdateReadWord(tag, Idata);
                            }
                            else
                            {
                                int len = pTask.isComArr ? pTask.m_ArrayCount : (data.Length - 46) / 2;
                                ushort[] sRcv = new ushort[len];
                                for (int i = 46, j = 0; i < data.Length; j++)
                                {
                                    byte[] dataI = new byte[2];
                                    dataI[0] = data[i];
                                    dataI[1] = data[i + 1];
                                    ushort Idata = BitConverter.ToUInt16(dataI, 0);
                                    sRcv[j] = Idata;
                                    i = i + 2;
                                }

                                UpdateReadWord(pTask, sRcv);
                                // UpdateReadWord(tag, sRcv);
                            }

                            #endregion
                            break;
                        case 0xC4://读有符号双字DINT等同于C#的int32
                            #region 读有符号双字DINT等同于C#的int32

                            if (data.Length <= 50 && !pTask.isComArr)
                            {
                                byte[] dataDi = new byte[4];
                                dataDi[0] = data[46];
                                dataDi[1] = data[47];
                                dataDi[2] = data[48];
                                dataDi[3] = data[49];
                                int Didata = BitConverter.ToInt32(dataDi, 0);
                                UpdateReadWord(pTask, Didata);
                            }
                            else
                            {
                                int len = pTask.isComArr ? pTask.m_ArrayCount : (data.Length - 46) / 4;
                                //  int len = (data.Length - 46) / 4;
                                int[] iRcv = new int[len];
                                for (int i = 46, j = 0; i < data.Length; j++)
                                {
                                    byte[] dataI = new byte[4];
                                    dataI[0] = data[i];
                                    dataI[1] = data[i + 1];
                                    dataI[2] = data[i + 2];
                                    dataI[3] = data[i + 3];
                                    int Idata = BitConverter.ToInt32(dataI, 0);
                                    iRcv[j] = Idata;
                                    i = i + 4;
                                }
                                UpdateReadWord(pTask, iRcv);
                            }

                            #endregion
                            break;
                        case 0xc8://读PLC无符号UDINT等同于C#的Uint
                        case 0xd3://读无符号双字DWord等同于C#的Uint
                            #region C#的Uint

                            if (data.Length <= 50 && !pTask.isComArr)
                            {
                                byte[] dataDi = new byte[4];
                                dataDi[0] = data[46];
                                dataDi[1] = data[47];
                                dataDi[2] = data[48];
                                dataDi[3] = data[49];
                                uint Didata = BitConverter.ToUInt32(dataDi, 0);
                                UpdateReadWord(pTask, Didata);
                            }
                            else
                            {
                                int len = pTask.isComArr ? pTask.m_ArrayCount : (data.Length - 46) / 4;
                                // int len = (data.Length - 46) / 4;
                                uint[] iRcv = new uint[len];
                                for (int i = 46, j = 0; i < data.Length; j++)
                                {
                                    byte[] dataI = new byte[4];
                                    dataI[0] = data[i];
                                    dataI[1] = data[i + 1];
                                    dataI[2] = data[i + 2];
                                    dataI[3] = data[i + 3];
                                    uint Idata = BitConverter.ToUInt32(dataI, 0);
                                    iRcv[j] = Idata;
                                    i = i + 4;
                                }
                                UpdateReadWord(pTask, iRcv);
                            }

                            #endregion
                            break;
                        case 0xca://读real实数
                            #region 读real实数

                            if (data.Length <= 50 && !pTask.isComArr)
                            {
                                byte[] dataF = new byte[4];
                                dataF[0] = data[46];
                                dataF[1] = data[47];
                                dataF[2] = data[48];
                                dataF[3] = data[49];
                                float fdata = BitConverter.ToSingle(dataF, 0);
                                UpdateReadWord(pTask, fdata);

                            }
                            else
                            {

                                int len = pTask.isComArr ? pTask.m_ArrayCount : (data.Length - 46) / 4;
                                //int len = (data.Length - 46) / 4;
                                float[] iRcv = new float[len];
                                for (int i = 46, j = 0; i < data.Length; j++)
                                {
                                    byte[] dataI = new byte[4];
                                    dataI[0] = data[i];
                                    dataI[1] = data[i + 1];
                                    dataI[2] = data[i + 2];
                                    dataI[3] = data[i + 3];
                                    float Idata = BitConverter.ToSingle(dataI, 0);
                                    iRcv[j] = Idata;
                                    i = i + 4;
                                }
                                UpdateReadWord(pTask, iRcv);
                            }

                            #endregion
                            break;
                    }
                }

            }
            else
            {
                if (!isReadNG)
                {
                    //CMachineCtrl.MsgListAdd(string.Format("PLC驱动err,Read异常:{0}", tag));
                    string str = tag;
                    isReadNG = true;
                }
                UpdateReadWord(pTask, 0);
                return false;
            }
            return true;
        }

        #region  读取
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public bool ReadPLCBit(string tag, MemAreaEIP area = MemAreaEIP.EIP)//2023 0325 无法读取bit
        {
            bool result = false;
            object rcv = GetReadWord(tag, OMRonEIPDataType.Bool, area);
            result = Convert.ToBoolean(rcv);

            return result;
        }
        public bool ReadPLCBit(string tag, out uint time, MemAreaEIP area = MemAreaEIP.EIP)//2023 0325 无法读取bit
        {
            bool result = false;
            object rcv = GetReadWord(tag, OMRonEIPDataType.Bool, area);
            result = Convert.ToBoolean(rcv);
            time = GetReadWordTimeStamp(tag);
            return result;

        }
        public bool ReadPLCBool(string tag, MemAreaEIP area = MemAreaEIP.EIP)//2023 0325 新增
        {
            bool result = false;
            object rcv = GetReadWord(tag, OMRonEIPDataType.Bool, area);

            try
            {
                result = ((bool)rcv);
            }
            catch (Exception)
            {
                result = ((bool[])rcv)[0];//PLC的bool类型有16位所以读上来的是16长度的bool[],但是只有第一个能改变的，也就是我们需要读的bool 
            }


            return result;
        }
        public byte ReadPLCByte(string tag, MemAreaEIP area = MemAreaEIP.EIP)//2023 0325 新增  （byte范围0~255）
        {
            byte result;
            object rcv = GetReadWord(tag, OMRonEIPDataType.Byte, area);
            result = (byte)rcv;//PLC的bool类型有16位所以读上来的是16长度的bool[],但是只有第一个能改变的，也就是我们需要读的bool

            return result;
        }
        public short ReadPLCShortInt(string tag, MemAreaEIP area = MemAreaEIP.EIP)//2023 0327 已测，可用
        {
            short result = 0;
            object rcv = GetReadWord(tag, OMRonEIPDataType.Int, area);
            short.TryParse(rcv.ToString(), out result);
            return result;
        }
        public short ReadPLCShortInt(string tag, out uint time, MemAreaEIP area = MemAreaEIP.EIP)
        {
            short result = 0;
            object rcv = GetReadWord(tag, OMRonEIPDataType.Int, area);
            short.TryParse(rcv.ToString(), out result);
            time = GetReadWordTimeStamp(tag);

            return result;

        }
        public ushort ReadPLCUShortInt(string tag, MemAreaEIP area = MemAreaEIP.EIP)// 2023 0327 int16的正整数，无法使用（0327已改，可以使用）
        {
            ushort result = 0;
            object rcv = GetReadWord(tag, OMRonEIPDataType.UInt, area);
            ushort.TryParse(rcv.ToString(), out result);
            return result;
        }
        public ushort ReadPLCUShortInt(string tag, out uint time, MemAreaEIP area = MemAreaEIP.EIP)// 2023 0327 int16的正整数，无法使用 （0327已改，可以使用）
        {
            ushort result = 0;
            object rcv = GetReadWord(tag, OMRonEIPDataType.UInt, area);
            ushort.TryParse(rcv.ToString(), out result);
            time = GetReadWordTimeStamp(tag);

            return result;

        }
        public ushort ReadPLCWord(string tag, out uint time, MemAreaEIP area = MemAreaEIP.EIP)//原来的ReadPLCUShortInt方法，用于读Word
        {
            ushort result = 0;
            object rcv = GetReadWord(tag, OMRonEIPDataType.Word, area);
            ushort.TryParse(rcv.ToString(), out result);
            time = GetReadWordTimeStamp(tag);

            return result;
        }
        public int ReadPLCDInt(string tag, MemAreaEIP area = MemAreaEIP.EIP)//2023 0327 已测，可用
        {
            int result = 0;
            object rcv = GetReadWord(tag, OMRonEIPDataType.DInt, area);
            int.TryParse(rcv.ToString(), out result);
            return result;
        }
        public int ReadPLCDInt(string tag, out uint time, MemAreaEIP area = MemAreaEIP.EIP)
        {
            int result = 0;
            object rcv = GetReadWord(tag,OMRonEIPDataType.DInt, area);
            int.TryParse(rcv.ToString(), out result);
            time = GetReadWordTimeStamp(tag);

            return result;
        }
        public uint ReadPLCUDInt(string tag, MemAreaEIP area = MemAreaEIP.EIP)//2023 0327 已测，可用
        {
            uint result = 0;
            object rcv = GetReadWord(tag, OMRonEIPDataType.Dword, area);
            uint.TryParse(rcv.ToString(), out result);
            return result;
        }
        public uint ReadPLCUDInt(string tag, out uint time, MemAreaEIP area = MemAreaEIP.EIP)
        {
            uint result = 0;
            object rcv = GetReadWord(tag, OMRonEIPDataType.Dword, area);
            uint.TryParse(rcv.ToString(), out result);
            time = GetReadWordTimeStamp(tag);

            return result;
        }
        public float ReadPLCFloat(string tag, MemAreaEIP area = MemAreaEIP.EIP)//2023 0327 已测，可用
        {
            float result = 0;
            object rcv = GetReadWord(tag, OMRonEIPDataType.Real, area);
            float.TryParse(rcv.ToString(), out result);
            return result;
        }
        public float ReadPLCFloat(string tag, out uint time, MemAreaEIP area = MemAreaEIP.EIP)
        {
            float result = 0;
            object rcv = GetReadWord(tag, OMRonEIPDataType.Real, area);
            float.TryParse(rcv.ToString(), out result);
            time = GetReadWordTimeStamp(tag);

            return result;
        }
        public string ReadPLCString(string tag, MemAreaEIP area = MemAreaEIP.EIP)//2023 0327 可以读取英文，但无法读取中文字符
        {
            string result = "";
            object rcv = GetReadWord(tag, OMRonEIPDataType.String, area);
            result = rcv.ToString();
            return result;
        }
        public string ReadPLCString(string tag, out uint time, MemAreaEIP area = MemAreaEIP.EIP)
        {
            string result = "";
            object rcv = GetReadWord(tag, OMRonEIPDataType.String, area);
            result = rcv.ToString();
            time = GetReadWordTimeStamp(tag);
            return result;
        }
        
        public string ReadPLCStringChange(string tag, MemAreaEIP area = MemAreaEIP.EIP)//2023 0327 其他PLC驱动中为了读取高低字节交换的字符串，当前eip驱动无用
        {
            string result = "";
            object rcv = GetReadWord(tag, OMRonEIPDataType.String, area);
            result = rcv.ToString();
            return result;
        }
        
         public bool ReadPLCBitWait(string tag, MemAreaEIP area = MemAreaEIP.EIP, int unMilliseconds = 1000)
        {
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);//超时 
            task.m_hEventFinish.Close();
            // Now we add an immediate task, when the task is processed the read value 
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            bool result = false;
            object rcv = GetReadWord(tag,  OMRonEIPDataType.Bool, area);
            result = Convert.ToBoolean(rcv);

            return result;
        }
        public bool ReadPLCBitWait(string tag, out bool Quality, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)
        {
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
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
            bool result = false;
            object rcv = GetReadWord(tag,  OMRonEIPDataType.Bool, area);
            result = Convert.ToBoolean(rcv);

            return result;
        }
        public bool ReadPLCBoolWait(string tag, out bool Quality, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)//2023 0419新增
        {
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
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
            bool result = false;
            object rcv = GetReadWord(tag,  OMRonEIPDataType.Bool, area);
            // result = Convert.ToBoolean(rcv);
            try
            {
                result = ((bool)rcv);
            }
            catch (Exception)
            {
                result = ((bool[])rcv)[0];//PLC的bool类型有16位所以读上来的是16长度的bool[],但是只有第一个能改变的，也就是我们需要读的bool 
            }


            return result;
        }
        public short ReadPLCShortIntWait(string tag, MemAreaEIP area = MemAreaEIP.EIP, int unMilliseconds = 1000)
        {
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            // Now we add an immediate task, when the task is processed the read value 
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            short result = 0;
            object rcv = GetReadWord(tag,  OMRonEIPDataType.Int, area);
            short.TryParse(rcv.ToString(), out result);
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="value"></param>
        /// <param name="area"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool ReadPLCShortIntWait(string tag, out short value, MemAreaEIP area = MemAreaEIP.EIP, int unMilliseconds = 1000)
        {
            if (IsBitDevice(area))
            {
                value = 0;
                return false;
            }
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality;
            if (task.m_hEventFinish.WaitOne(unMilliseconds))
            {
                quality = true;
                object rcv = GetReadWord(tag,  OMRonEIPDataType.Int, area);
                short.TryParse(rcv.ToString(), out value);
            }
            else
            {
                value = 0;
                quality = false;
            }
            task.m_hEventFinish.Close();
            return quality;

            // Now we add an immediate task, when the task is processed the read value 
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            //short result = 0;
            //object rcv = GetReadWord(tag, DataType.Int, area);
            //short.TryParse(rcv.ToString(), out result);
        }
        public ushort ReadPLCUShortIntWait(string tag, MemAreaEIP area = MemAreaEIP.EIP, int unMilliseconds = 1000)
        {
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            // Now we add an immediate task, when the task is processed the read value 
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            ushort result = 0;
            object rcv = GetReadWord(tag,  OMRonEIPDataType.Word, area);
            ushort.TryParse(rcv.ToString(), out result);
            return result;
        }
        public short ReadPLCShortIntWait(string tag, out bool Quality, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)
        {
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
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
            object rcv = GetReadWord(tag,  OMRonEIPDataType.Int, area);
            short.TryParse(rcv.ToString(), out result);
            return result;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="Quality"></param>
        /// <param name="unMilliseconds"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public ushort ReadPLCUShortIntWait(string tag, out bool Quality, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)
        {
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
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
            ushort result = 0;
            object rcv = GetReadWord(tag,  OMRonEIPDataType.Word, area);
            ushort.TryParse(rcv.ToString(), out result);
            return result;
        }
        public int ReadPLCDIntWait(string tag, MemAreaEIP area = MemAreaEIP.EIP, int unMilliseconds = 1000)
        {
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            // Now we add an immediate task, when the task is processed the read value 
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            int result = 0;
            object rcv = GetReadWord(tag, OMRonEIPDataType.DInt, area);
            int.TryParse(rcv.ToString(), out result);
            return result;
        }
        public uint ReadPLCUDIntWait(string tag, MemAreaEIP area = MemAreaEIP.EIP, int unMilliseconds = 1000)
        {
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            // Now we add an immediate task, when the task is processed the read value 
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            uint result = 0;
            object rcv = GetReadWord(tag,  OMRonEIPDataType.Dword, area);
            uint.TryParse(rcv.ToString(), out result);
            return result;
        }
        public int ReadPLCDIntWait(string tag, out bool Quality, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)
        {
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
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
            int result = 0;
            object rcv = GetReadWord(tag,  OMRonEIPDataType.DInt, area);
            int.TryParse(rcv.ToString(), out result);
            return result;
        }
        public uint ReadPLCUDIntWait(string tag, out bool Quality, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)
        {
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
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
            uint result = 0;
            object rcv = GetReadWord(tag,  OMRonEIPDataType.Dword, area);
            uint.TryParse(rcv.ToString(), out result);
            return result;
        }
        public float ReadPLCFloatWait(string tag, MemAreaEIP area = MemAreaEIP.EIP, int unMilliseconds = 1000)
        {
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            float result = 0;
            object rcv = GetReadWord(tag,  OMRonEIPDataType.Real, area);
            float.TryParse(rcv.ToString(), out result);
            return result;
        }
        public float ReadPLCFloatWait(string tag, out bool Quality, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)
        {
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
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
            float result = 0;
            object rcv = GetReadWord(tag,  OMRonEIPDataType.Real, area);
            float.TryParse(rcv.ToString(), out result);
            return result;
        }

        //Read PLC ArrayData----------

        /// <summary>读取Bool数组 标签
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public bool[] ReadArrayBool(string tag, out uint time, MemAreaEIP area = MemAreaEIP.EIP)
        {
            object rcv = GetReadWord(tag,  OMRonEIPDataType.BoolArr, area);
            bool[] readBoolArray = (bool[])rcv;
            time = GetReadWordTimeStamp(tag);
            return readBoolArray;
        }
        /// <summary>读取Bool数组 标签
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public bool[] ReadArrayBool(string tag, MemAreaEIP area = MemAreaEIP.EIP)//2023 0327 已测，可用
        {
            object rcv = GetReadWord(tag,  OMRonEIPDataType.BoolArr, area);
            bool[] readBoolArray = (bool[])rcv;
            return readBoolArray;
        }
        /// <summary>读取short即int16(有符号)数组 标签
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public short[] ReadArrayShortInt(string tag, MemAreaEIP area = MemAreaEIP.EIP)//2023 0327 已测，可用
        {
            try
            {
                // return new short[500];
                object rcv = GetReadWord(tag,  OMRonEIPDataType.IntArr, area);
                short[] readshortArray = (short[])rcv;


                return readshortArray;
            }
            catch(Exception e)
            {
                return null;
            }
        }
        /// <summary>读取short即int16(有符号)数组 标签
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public short[] ReadArrayShortInt(string tag, out uint time, MemAreaEIP area = MemAreaEIP.EIP)//2023 0427 已测，可用
        {
            // return new short[500];
            object rcv = GetReadWord(tag,  OMRonEIPDataType.IntArr, area);
            short[] readshortArray = (short[])rcv;
            time = GetReadWordTimeStamp(tag);

            return readshortArray;
        }
        /// <summary>读取ushort即uint16(无符号)数组 标签
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public ushort[] ReadArrayUshortInt(string tag, MemAreaEIP area = MemAreaEIP.EIP)//2023 0327 已测，可用
        {
            object rcv = GetReadWord(tag,  OMRonEIPDataType.WordArr, area);
            ushort[] readshortArray = (ushort[])rcv;
            return readshortArray;
        }
        /// <summary>读取ushort即uint16(无符号)数组 标签
        /// 
        /// </summary>
        /// <param name="tag">数组</param>
        /// <param name="time">最后一次刷新时间</param>
        /// <param name="area"></param>
        /// <returns></returns>
        public ushort[] ReadArrayUshortInt(string tag, out uint time, MemAreaEIP area = MemAreaEIP.EIP)//2023 0915 已测，可用
        {
            object rcv = GetReadWord(tag,  OMRonEIPDataType.WordArr, area);
            ushort[] readshortArray = (ushort[])rcv;
            time = GetReadWordTimeStamp(tag);
            return readshortArray;
        }
        /// <summary>读取int32(有符号)数组 标签
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public int[] ReadArrayDInt(string tag, MemAreaEIP area = MemAreaEIP.EIP)//2023 0327 已测，可用
        {
            object rcv = GetReadWord(tag,  OMRonEIPDataType.DIntArr, area);
            int[] readArray = (int[])rcv;
            return readArray;
        }
        /// <summary>读取int32(有符号)数组 标签
        /// 
        /// </summary>
        /// <param name="tag">数组</param>
        /// <param name="time">最后一次刷新时间</param>
        /// <param name="area"></param>
        /// <returns></returns>
        public int[] ReadArrayDInt(string tag, out uint time, MemAreaEIP area = MemAreaEIP.EIP)//2023 0915 已测，可用
        {
            object rcv = GetReadWord(tag,  OMRonEIPDataType.DIntArr, area);
            int[] readArray = (int[])rcv;
            time = GetReadWordTimeStamp(tag);
            return readArray;
        }
        /// <summary>读取word数组 标签
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public ushort[] ReadArrayWord(string tag, MemAreaEIP area = MemAreaEIP.EIP)//未测
        {
            object rcv = GetReadWord(tag,  OMRonEIPDataType.WordArr, area);
            ushort[] readArray = (ushort[])rcv;
            return readArray;
        }
        public string ReadArrayWordString(string tag, MemAreaEIP area = MemAreaEIP.EIP)//无法使用
        {
            string result = string.Empty;

            object rcv = GetReadWord(tag,  OMRonEIPDataType.WordArr, area);
            ushort[] readArray = (ushort[])rcv;


            foreach (var item in readArray)
            {
                string temp = item.ToString("X4");

                if (item != 0X00)
                {
                    result += temp;

                }
            }
            byte[] ASCbyte = new byte[result.Length / 2];

            int index = 0;
            for (int i = 0; i < result.Length; i += 2)
            {
                ASCbyte[index] = Convert.ToByte(result.Substring(i, 2), 16);
                ++index;
            }

            byte[] tempbyte = new byte[ASCbyte.Length];
            for (int i = 0; i < ASCbyte.Length; i++)
            {
                if (i % 2 == 0)
                {
                    tempbyte[i + 1] = ASCbyte[i];
                }
                else
                {
                    tempbyte[i - 1] = ASCbyte[i];
                }
            }

            result = Encoding.Default.GetString(tempbyte);

            return result;
        }
        /// <summary>读取uint32(无符号)数组 标签
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public uint[] ReadArrayUInt(string tag, MemAreaEIP area = MemAreaEIP.EIP)//2023 0327 已测，可用
        {
            object rcv = GetReadWord(tag,  OMRonEIPDataType.DwordArr, area);
            uint[] readArray = (uint[])rcv;
            return readArray;
        }
        /// <summary>读取float数组 标签
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public float[] ReadArrayFloat(string tag, MemAreaEIP area = MemAreaEIP.EIP)//2023 0327 已测，可用
        {
            object rcv = GetReadWord(tag,  OMRonEIPDataType.RealArr, area);
            float[] readArray = (float[])rcv;
            return readArray;
        }
        /// <summary>读取float数组 标签
        /// 
        /// </summary>
        /// <param name="tag">数组</param>
        /// <param name="time">最后一次刷新时间</param>
        /// <param name="area"></param>
        /// <returns></returns>
        public float[] ReadArrayFloat(string tag, out uint time, MemAreaEIP area = MemAreaEIP.EIP)//2023 0915 已测，可用
        {
            object rcv = GetReadWord(tag,  OMRonEIPDataType.RealArr, area);
            float[] readArray = (float[])rcv;
            time = GetReadWordTimeStamp(tag);
            return readArray;
        }

        //Read PLC ArrayData Wait----------

        /// <summary>即时任务，读取Int16位(有符号)数组。注：数组长度限制在100以内
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="Quality"></param>
        /// <param name="readCount"></param>
        /// <param name="unMilliseconds"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public short[] ReadArrayShortIntWait(string tag, out bool Quality, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)
        {
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
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

            object rcv = GetReadWord(tag,  OMRonEIPDataType.IntArr, area);
            short[] readArray = (short[])rcv;
            return readArray;
        }
        /// <summary>即时任务，读取Int32位(有符号)数组。注：数组长度限制在50以内
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="Quality"></param>
        /// <param name="readCount"></param>
        /// <param name="unMilliseconds"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public int[] ReadArrayDIntWait(string tag, out bool Quality, short readCount = 1, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)
        {
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
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

            object rcv = GetReadWord(tag,  OMRonEIPDataType.DIntArr, area);
            int[] readArray = (int[])rcv;
            return readArray;
        }

        /// <summary>即时任务，读取Float数组。注：数组长度限制在50以内
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="Quality"></param>
        /// <param name="readCount"></param>
        /// <param name="unMilliseconds"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public float[] ReadArrayFloatWait(string tag, out bool Quality, short readCount = 1, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)
        {
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
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

            object rcv = GetReadWord(tag,  OMRonEIPDataType.RealArr, area);
            float[] readArray = (float[])rcv;
            return readArray;
        }
        #endregion
        #region  写入
        // Write PLC Data----------

        public bool WritePLCBit(string tag, uint unBit, short wValue, MemAreaEIP area = MemAreaEIP.EIP)//2023 0327 无法按位写位bit，用于只能将8位中的第一位更改true false,不实用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType =  OMRonEIPDataType.Bool;//Bit;
            task.m_unBit = unBit;//Bit;
            if (Convert.ToBoolean(wValue))
            {
                task.m_wWriteObjValue = "1";
            }
            else
            {
                task.m_wWriteObjValue = "0";
            }
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        public bool WritePLCBool(string tag, bool wValue, MemAreaEIP area = MemAreaEIP.EIP)//写PLC 的Bool类型变量，2023 0327 新增 测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType =  OMRonEIPDataType.Bool;//Bit;
            // task.m_unBit = unBit;//Bit;
            if (wValue)
            {
                task.m_wWriteObjValue = "1";
            }
            else
            {
                task.m_wWriteObjValue = "0";
            }
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="value"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public bool WritePLCShortInt(string tag, short value, MemAreaEIP area = MemAreaEIP.EIP)//写PLC16位数据(有符号)，测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.Int;//short;
            task.m_wWriteObjValue = value.ToString();
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
           public bool WritePLCUShortInt(string tag, ushort value, MemAreaEIP area = MemAreaEIP.EIP)//写PLC16位数据(无符号)，2023 0327修改 测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.UInt;//ushort;
            task.m_wWriteObjValue = value.ToString();
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        public bool WritePLCWord(string tag, ushort value, MemAreaEIP area = MemAreaEIP.EIP)//写PLC16位数据(无符号) ，2023 0327新增（原来的WritePLCUShortInt方法）
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.Word;//ushort;
            task.m_wWriteObjValue = value.ToString();
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="value"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public bool WritePLCDInt(string tag, int value, MemAreaEIP area = MemAreaEIP.EIP)//写PLC32位数据(有符号)，测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.DInt;//int32;
            task.m_wWriteObjValue = value.ToString();
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        public bool WritePLCUDInt(string tag, uint value, MemAreaEIP area = MemAreaEIP.EIP)//写PLC32位数据(无符号),2023 0327修改，测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.UDInt;//uint32;
            task.m_wWriteObjValue = value.ToString();
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        public bool WritePLCDword(string tag, uint value, MemAreaEIP area = MemAreaEIP.EIP)//写PLC32位数据(无符号) ，2023 0327新增（原来的WritePLCUInt方法）
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.Dword;//uint32;
            task.m_wWriteObjValue = value.ToString();
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        public bool WritePLCFloat(string tag, float value, MemAreaEIP area = MemAreaEIP.EIP)//写PLC 64位数据(real:float+double)，测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.Real;//real:float、double;实数
            task.m_wWriteObjValue = value.ToString();
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        public bool WritePLCString(string tag, string value, MemAreaEIP area = MemAreaEIP.EIP)//写PLC字符串数据，测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.String;//string
            task.m_wWriteObjValue = value;
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        //Write PLC Data with wait----------
        public bool WritePLCBitWait(string tag, uint unBit, short wValue, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)//2023 0328 无法按位写位bit，用于只能将8位中的第一位更改true false,不实用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.Bool;//Bit;
            task.m_unBit = unBit;//Bit;
            if (Convert.ToBoolean(wValue))
            {
                task.m_wWriteObjValue = "1";
            }
            else
            {
                task.m_wWriteObjValue = "0";
            }
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return quality;
        }
        public bool WritePLCBoolWait(string tag, bool wValue, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)//写PLC 的Bool类型变量，2023 0327 新增 测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.Bool;//Bit;
            // task.m_unBit = unBit;//Bit;
            if (wValue)
            {
                task.m_wWriteObjValue = "1";
            }
            else
            {
                task.m_wWriteObjValue = "0";
            }
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return quality;
        }
        public bool WritePLCShortIntWait(string tag, short value, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)//写PLC16位数据(有符号)，测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.Int;//short;
            task.m_wWriteObjValue = value.ToString();
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return quality;
        }
        public bool WritePLCUShortIntWait(string tag, ushort value, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)//写PLC16位数据(无符号)，2023 0328修改，测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.UInt;//ushort;
            task.m_wWriteObjValue = value.ToString();
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return quality;
        }
        public bool WritePLCWordWait(string tag, ushort value, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)//写PLC16位数据(无符号) ，2023 0328新增（原来的WritePLCUShortIntWait方法）
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.Word;//ushort;
            task.m_wWriteObjValue = value.ToString();
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return quality;
        }
        public bool WritePLCDIntWait(string tag, int value, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)//写PLC32位数据(有符号)，测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task. 
            TaskStructEIP task = new TaskStructEIP();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.DInt;//real:float/double
            task.m_wWriteObjValue = value.ToString();
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return quality;
        }
        public bool WritePLCUDIntWait(string tag, uint value, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)//写PLC32位数据(无符号),2023 0328修改，测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.UDInt;//uint;
            task.m_wWriteObjValue = value.ToString();
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return quality;
        }
        public bool WritePLCDWordWait(string tag, uint value, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)//写PLC32位数据(无符号) ，2023 0328新增（原来的WritePLCUIntWait方法）
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.Dword;//uint;
            task.m_wWriteObjValue = value.ToString();
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return quality;
        }
        public bool WritePLCFloatWait(string tag, float value, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)//写PLC32位数据(浮点数)，测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.Real;//real:float/double;
            task.m_wWriteObjValue = value.ToString();
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return quality;
        }
        public bool WritePLCStringWait(string tag, string value, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)//写PLC字符串，测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.String;//string
            task.m_wWriteObjValue = value;
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return quality;
        }
         public bool WritePLCMutilBool(string tag, bool[] strValue, MemAreaEIP area = MemAreaEIP.EIP)//写BOOL位数组--2023 0419 可以使用，但写入的bool[]长度必须为偶数
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType =OMRonEIPDataType.BoolArr;//MutilShort;
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_wWriteObjValue = strValue;
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        /// <summary>写C# bool[] 加是否超时判断 可以使用，但写入的bool[]长度必须为偶数；
        /// 原驱动类写入的数组长度必须与PLC数组实际长度一致，标签必须带有下标入  如TestArry[0]
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="strValue"></param>
        /// <param name="unMilliseconds"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public bool WritePLCMutilBoolWait(string tag, bool[] strValue, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)//写BOOL位数组--2023 0426 可以使用，但写入的bool[]长度必须为偶数
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType =OMRonEIPDataType.BoolArr;//MutilShort;
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_wWriteObjValue = strValue;
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return quality;
        }
        // Write Array :C# INT----------
        /// <summary>写c# int[]；
        /// 原驱动类写入的数组长度必须与PLC数组实际长度一致，新驱动可以写最大长度为50，但标签必须带有下标入  如TestArry[0]
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="strValue"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public bool WritePLCMutilDDInt(string tag, int[] strValue, MemAreaEIP area = MemAreaEIP.EIP)//写PLC32位数组--,2023 0328修改，测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.DIntArr;//MutilShort;
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_wWriteObjValue = strValue;
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        /// <summary>写c# int[] 加是否超时判断；
        /// 原驱动类写入的数组长度必须与PLC数组实际长度一致，新驱动可以写最大长度为50，但标签必须带有下标入  如TestArry[0]
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="strValue"></param>
        /// <param name="unMilliseconds"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public bool WritePLCMutilDDIntWait(string tag, int[] strValue, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)//写PLC32位数组--,2023 0328修改，测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.DIntArr;//MutilShort;
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_wWriteObjValue = strValue;
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return quality;
        }
        // Write Array :C# Short----------
        /// <summary> 写C# short[] ；
        ///  原驱动类写入的数组长度必须与PLC数组实际长度一致，新驱动可以写最大长度为100，但标签必须带有下标入  如TestArry[0]
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="strValue"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public bool WritePLCMutilShortInt(string tag, short[] strValue, MemAreaEIP area = MemAreaEIP.EIP)//写PLC16位数组，测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.IntArr;//MutilShort;
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_wWriteObjValue = strValue;
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        /// <summary>写C# short[]  加是否超时判断；
        /// 原驱动类写入的数组长度必须与PLC数组实际长度一致，新驱动可以写最大长度为100，但标签必须带有下标入  如TestArry[0]
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="strValue"></param>
        /// <param name="unMilliseconds"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public bool WritePLCMutilShortIntWait(string tag, short[] strValue, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)//写PLC16位数组，测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.IntArr;//MutilShort;
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_wWriteObjValue = strValue;
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return quality;
        }
        // Write Array :C# Float----------
        /// <summary> 写C# Flaot [] 
        /// 原驱动类写入的数组长度必须与PLC数组实际长度一致，新驱动可以写最大长度为50，但标签必须带有下标入  如TestArry[0]
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="strValue"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public bool WritePLCMutilFloat(string tag, float[] strValue, MemAreaEIP area = MemAreaEIP.EIP)//写PLC32位数组--,2023 0328修改，测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.RealArr;//MutilShort;
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_wWriteObjValue = strValue;
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        /// <summary> 写C# Flaot [] 加超时判断；
        /// 原驱动类写入的数组长度必须与PLC数组实际长度一致，新驱动可以写最大长度为50，但标签必须带有下标入  如TestArry[0]
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="strValue"></param>
        /// <param name="unMilliseconds"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public bool WritePLCMutilFloatWait(string tag, float[] strValue, int unMilliseconds = 1000, MemAreaEIP area = MemAreaEIP.EIP)//写PLC32位数组--,2023 0328修改，测试可用
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            TaskStructEIP task = new TaskStructEIP();
            task.m_nReadOrWrite = 1; // Write
            task.m_nDataType = OMRonEIPDataType.RealArr;//MutilShort;
            task.m_PLCArea = area;
            task.m_strTag = tag;
            task.m_wWriteObjValue = strValue;
            task.GenStrCmd();
            AddImmeTask(task);
            bool quality = task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            return quality;
        }

        /// <summary>浮点数数组写入，循环一条条数据进行写入；
        ///标签不带下标，如“TextArr”√，“TextArr[0]”×
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="strValue"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        public bool WritePLCArrFloat(string tag, float[] strValue, MemAreaEIP area = MemAreaEIP.EIP)//写PLC32位数组--。 2023 0328 这是种写法特殊情况下才使用
        {
            int count = strValue.Length;
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    // We do not real Read or Write Comm here and just product
                    // the task, add it to the list, and signal the event so that
                    // the serve thread can process the task.
                    TaskStructEIP task = new TaskStructEIP();
                    task.m_PLCArea = area;
                    task.m_strTag = tag + string.Format("[{0}]", i);
                    task.m_nReadOrWrite = 1; // Write
                    task.m_nDataType = OMRonEIPDataType.Real;//real:float、double;实数
                    task.m_wWriteObjValue = strValue[i].ToString();
                    task.GenStrCmd();
                    AddImmeTask(task);

                }
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <param name="wUpdate"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        private   bool UpdateReadWord(TaskStructEIP pTask, object wUpdate, MemAreaEIP area = MemAreaEIP.EIP)
        {
            string tag = pTask.m_strTag;
            int iArea = 0;

            #region eip特有

            if (pTask.isComArr)
            {
                int tempIndex = pTask.m_strTag.IndexOf("[");
                tag = pTask.m_strTag.Substring(0, tempIndex);
            }

            #endregion

            lock (m_critUpdateReadList)              // CriticalSect
            {
                if (m_mapReadPLCMem[iArea].ContainsKey(tag) == false)
                {
                    m_mapReadPLCMem[iArea].Add(tag, wUpdate);
                    m_mapReadPLCMemTimeStamp[iArea].Add(tag, GetTickCount());
                }
                else
                {
                    if (pTask.isComArr)
                    {
                        #region Eip特有
                        switch (pTask.m_nDataType)
                        {
                            case OMRonEIPDataType.IntArr://short[]
                                short[] tempShortWArr = (short[])wUpdate;
                                short[] tempShortRArr = (short[])m_mapReadPLCMem[iArea][tag];
                                Array.Copy(tempShortWArr, 0, tempShortRArr, pTask.m_ArrayIndex, pTask.m_ReadCount);
                                m_mapReadPLCMem[iArea][tag] = tempShortRArr;
                                if (pTask.m_ArrayCount == pTask.m_ArrayIndex + pTask.m_ReadCount)//当更新数组的最后一部分时，就算数组完整地刷新一次
                                {
                                    m_mapReadPLCMemTimeStamp[iArea][tag] = GetTickCount();
                                }
                                break;
                            case OMRonEIPDataType.WordArr://ushort[]
                                ushort[] tempUshotWArr = (ushort[])wUpdate;
                                ushort[] tempUshotRArr = (ushort[])m_mapReadPLCMem[iArea][tag];
                                Array.Copy(tempUshotWArr, 0, tempUshotRArr, pTask.m_ArrayIndex, pTask.m_ReadCount);
                                m_mapReadPLCMem[iArea][tag] = tempUshotRArr;
                                if (pTask.m_ArrayCount == pTask.m_ArrayIndex + pTask.m_ReadCount)//当更新数组的最后一部分时，就算数组完整地刷新一次
                                {
                                    m_mapReadPLCMemTimeStamp[iArea][tag] = GetTickCount();
                                }
                                break;
                            case OMRonEIPDataType.DIntArr://int[]
                                int[] tempIntWArr = (int[])wUpdate;
                                int[] tempIntRArr = (int[])m_mapReadPLCMem[iArea][tag];
                                Array.Copy(tempIntWArr, 0, tempIntRArr, pTask.m_ArrayIndex, pTask.m_ReadCount);
                                m_mapReadPLCMem[iArea][tag] = tempIntRArr;
                                if (pTask.m_ArrayCount == pTask.m_ArrayIndex + pTask.m_ReadCount)//当更新数组的最后一部分时，就算数组完整地刷新一次
                                {
                                    m_mapReadPLCMemTimeStamp[iArea][tag] = GetTickCount();
                                }
                                break;
                            case OMRonEIPDataType.RealArr://float[]
                                float[] tempFloatWArr = (float[])wUpdate;
                                float[] tempFloatRArr = (float[])m_mapReadPLCMem[iArea][tag];
                                Array.Copy(tempFloatWArr, 0, tempFloatRArr, pTask.m_ArrayIndex, pTask.m_ReadCount);
                                m_mapReadPLCMem[iArea][tag] = tempFloatRArr;
                                if (pTask.m_ArrayCount == pTask.m_ArrayIndex + pTask.m_ReadCount)//当更新数组的最后一部分时，就算数组完整地刷新一次
                                {
                                    m_mapReadPLCMemTimeStamp[iArea][tag] = GetTickCount();
                                }
                                break;

                            default:
                                m_mapReadPLCMem[iArea][tag] = wUpdate;
                                m_mapReadPLCMemTimeStamp[iArea][tag] = GetTickCount();
                                break;
                        }
                        #endregion
                    }
                    else
                    {
                        m_mapReadPLCMem[iArea][tag] = wUpdate;
                        m_mapReadPLCMemTimeStamp[iArea][tag] = GetTickCount();
                    }
                }
            }
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="mDataType"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        private object GetReadWord(string tag, OMRonEIPDataType mDataType, MemAreaEIP area = MemAreaEIP.EIP)
        {
            object wUpdate1 = new object();
            lock (m_critUpdateReadList)// CriticalSect
            {
                var p = m_mapReadPLCMem[(int)area];
                var keys =p.Keys;
                if (p .ContainsKey(tag) == true)
                {
                    wUpdate1 = m_mapReadPLCMem[(int)area][tag];
                    if (m_mapReadPLCMem[(int)area][tag].Equals((object)0))//防止PLC灌入程序后闪退
                    {
                        #region 默认数组数值为0
                        switch (mDataType)
                        {
                            case OMRonEIPDataType.WordArr:
                                short[] shortArr = new short[500];
                                wUpdate1 = shortArr;
                                break;
                            case OMRonEIPDataType.DwordArr:
                                UInt32[] uintArr = new UInt32[500];
                                wUpdate1 = uintArr;
                                break;
                            case OMRonEIPDataType.IntArr:
                                Int16[] int16Arr = new Int16[500];
                                wUpdate1 = int16Arr;
                                break;
                            case OMRonEIPDataType.DIntArr:
                                Int32[] int32Arr = new Int32[500];
                                wUpdate1 = int32Arr;
                                break;
                            case OMRonEIPDataType.RealArr:
                                float[] realArr = new float[500];
                                wUpdate1 = realArr;
                                break;
                            case OMRonEIPDataType.StringArr:
                                //string[] strArr = new string[100];
                                //wUpdate1 = strArr;
                                string strArr = "";
                                wUpdate1 = strArr;
                                break;
                            case OMRonEIPDataType.BoolArr:
                                bool[] boolArr = new bool[500];
                                wUpdate1 = boolArr;
                                break;
                            case OMRonEIPDataType.ByteArr:
                                byte[] byteArr = new byte[500];
                                wUpdate1 = byteArr;
                                break;

                            default:
                                break;
                        }
                        #endregion
                    }
                }
                else
                {
                    #region 默认数组数值为0
                    switch (mDataType)
                    {
                        case OMRonEIPDataType.WordArr:
                            short[] shortArr = new short[500];
                            wUpdate1 = shortArr;
                            break;
                        case OMRonEIPDataType.DwordArr:
                            UInt32[] uintArr = new UInt32[500];
                            wUpdate1 = uintArr;
                            break;
                        case OMRonEIPDataType.IntArr:
                            Int16[] int16Arr = new Int16[500];
                            wUpdate1 = int16Arr;
                            break;
                        case OMRonEIPDataType.DIntArr:
                            Int32[] int32Arr = new Int32[500];
                            wUpdate1 = int32Arr;
                            break;
                        case OMRonEIPDataType.RealArr:
                            float[] realArr = new float[500];
                            wUpdate1 = realArr;
                            break;
                        case OMRonEIPDataType.StringArr:
                            string strArr = "";
                            wUpdate1 = strArr;
                            break;
                        case OMRonEIPDataType.BoolArr:
                            bool[] boolArr = new bool[500];
                            wUpdate1 = boolArr;
                            break;
                        case OMRonEIPDataType.ByteArr:
                            byte[] byteArr = new byte[500];
                            wUpdate1 = byteArr;
                            break;
                        case OMRonEIPDataType.Bool:
                            bool Bool = false;
                            wUpdate1 = Bool;
                            break;
                        default:
                            break;
                    }
                    #endregion
                }
            }

            return wUpdate1;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tag"></param>
        /// <param name="area"></param>
        /// <returns></returns>
        private uint GetReadWordTimeStamp(string tag, MemAreaEIP area = MemAreaEIP.EIP)
        {
            uint TimeStamp1 = 0;
            lock (m_critUpdateReadList)// CriticalSect
            {

                if (m_mapReadPLCMemTimeStamp[(int)area].ContainsKey(tag) == true)
                {
                    TimeStamp1 = m_mapReadPLCMemTimeStamp[(int)area][tag];
                }
            }
            return TimeStamp1;
        }
        #endregion
}
