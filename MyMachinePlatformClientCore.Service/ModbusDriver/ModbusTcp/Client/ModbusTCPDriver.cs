using System.IO.Ports;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace MyMachinePlatformClientCore.Service.ModbusDriver.ModbusServer;

/// <summary>
/// modbus tcp 客户端 驱动
/// </summary>
public class ModbusTCPDriver
{
        [DllImport("kernel32", CharSet = CharSet.Auto)]
        private static extern uint GetTickCount();
        private object m_critSecPeriodList = new object();
        private object m_critSecImmeList = new object();
        private object m_critUpdateReadList = new object();
        private static System.Threading.Thread m_pSocketThreadProc;
        private ManualResetEvent m_hThreadExitEvent = null;
        // event handles to synchronize threads 
        private AutoResetEvent m_hImmeTaskEvent = null;

        private WaitHandle GetThreadExitHandle() { return m_hThreadExitEvent; }

        private WaitHandle GetImmeTaskEvtHandle() { return m_hImmeTaskEvent; }

        private SerialPort _serialPort;

        private string _lastError = "";

        private string _port;

        private int _timeOut;
        #region tcp
        private int _timeout = 50;
        private Ping _ping = null;
        private IPEndPoint _endPoint = null;
        internal Socket ModbusTcpSocket = null;

        #endregion


        private List<StructModbusTCP> m_periodTaskList = new List<StructModbusTCP>();

        private Queue<StructModbusTCP> m_immeTaskList = new Queue<StructModbusTCP>();

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
        /// <summary>
        /// return last error detected
        /// </summary>
        public string LastError
        {
            get { return _lastError; }
        }

        public bool IsOpen
        {
            get { return _serialPort.IsOpen == true; }
        }
        public int TimeOut
        {
            get { return _timeOut; }
            set { _timeOut = value; }
        }

        private bool initConnect = false;
        /// <summary>
        /// returns the connection status
        /// </summary>
        public bool Connected
        {
            get { return (ModbusTcpSocket == null) ? false : ModbusTcpSocket.Connected; }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public ModbusTCPDriver()
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
                if (ModbusTcpSocket != null)
                {
                    ModbusTcpSocket.Dispose();
                }
                ModbusTcpSocket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                ModbusTcpSocket.SendTimeout = _timeout;
                ModbusTcpSocket.ReceiveTimeout = _timeout;
                ModbusTcpSocket.Connect(this._endPoint);
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
                if (m_pSocketThreadProc == null)
                {
                    return false;
                }
                m_pSocketThreadProc.Start();
                return true;
            }
            catch (System.Exception ex)//SocketException error
            {
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
                if (ModbusTcpSocket == null) return;
                if (Connected)
                {
                    ModbusTcpSocket.Disconnect(false);
                    ModbusTcpSocket.Close();
                }
                ModbusTcpSocket.Dispose();
                ModbusTcpSocket = null;
            }
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
             
            int bytesSent = ModbusTcpSocket.Send(command, cmdLen, SocketFlags.None);

             
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
            int bytesRecv = ModbusTcpSocket.Receive(response, respLen, SocketFlags.None);
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
                dwRes = WaitHandle.WaitAny(hArray, 100);
                switch (dwRes)
                {
                    case WaitHandle.WaitTimeout:
                        ProcessPeriodTask();
                        break;
                    case 0:
                        // Process the immediately task
                        ////m_hImmeTaskEvent                    
                        ProcessImmeTask();
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
       /// in overlapped read function (ReadPLCWord) without wait.
       /// </summary>
       /// <param name="area"></param>
       /// <param name="unBeginWord"></param>
       /// <param name="unWordsCount"></param>
       /// <returns></returns>
        public bool AddReadArea(AreaReadModbusTCPTpye area, int unBeginWord, ushort unWordsCount)
        {
            StructModbusTCP task = new StructModbusTCP();
            task.Area = (MemAreaModbusTCP)area;
            task.m_PLCArea = area;
            task.m_unBeginWord = unBeginWord;
            task.m_unWordsCount = unWordsCount;
            task.GenStrCmd();
            lock (m_critSecPeriodList)
            {
                m_periodTaskList.Add(task);
            }
            return true;
        }

        /// <summary>
        /// Clear all the added PLC memory.
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

        private short GetReadWord(AreaReadModbusTCPTpye area, uint unWordAddress)
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

        // PLC memory map operate
        private bool UpdateReadWord(AreaReadModbusTCPTpye area, UInt32 unWordAddress, short wUpdate)
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

        private void AddImmeTask(StructModbusTCP task)
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

        private void ProcessPeriodTask()
        {
            if (m_periodTaskList == null)
            {
                _lastError = "m_periodTaskList is Null";
                return;
            }
            if (m_periodTaskList.Count == 0)
            {
                //_lastError = "m_periodTaskList.Count is 0";
                return;
            }
            bool bRes;
            foreach (StructModbusTCP task in m_periodTaskList)
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
                    // break;
                }
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
            StructModbusTCP task = new StructModbusTCP();
            while (m_immeTaskList.Count != 0)
            {
                lock (m_critSecImmeList)
                {
                    task = m_immeTaskList.Dequeue();//移除并返回
                }
                res = SendAndGetRes(task);
                if (task.m_hEventFinish != null && res != false)
                {
                    try
                    {
                        // Tell the call thread (main thread of the program) that// immediate task has finished.                   
                        task.m_hEventFinish.Set();
                    }
                    catch (System.Exception ex)
                    {
                        _lastError = "task.m_hEventFinish.Set() 异常" + ex.Message;
                    }
                }
            }
        }

        private bool SendAndGetRes(StructModbusTCP pTask)
        {
            try
            {
                Send(pTask.m_strCmd, pTask.m_strCmd.Length);
                byte[] RecvData = null;
                int size = 0;
                switch (pTask.Area)
                {
                    case MemAreaModbusTCP.ReadCoil:
                    case MemAreaModbusTCP.ReadDiscreteInputs:
                        if (pTask.m_unWordsCount % 8 == 0)
                        {
                            RecvData = new byte[pTask.m_unWordsCount / 8 + 9];
                            size = pTask.m_unWordsCount / 8 + 9;
                        }
                        else
                        {
                            RecvData = new byte[pTask.m_unWordsCount / 8 + 9 + 1];
                            size = pTask.m_unWordsCount / 8 + 9 + 1;
                        }
                        break;
                    case MemAreaModbusTCP.ReadHoldingRegister:
                    case MemAreaModbusTCP.ReadInputRegister:
                        RecvData = new byte[pTask.m_unWordsCount * 2 + 9];
                        size = pTask.m_unWordsCount * 2 + 9;
                        break;
                    case MemAreaModbusTCP.WriteMultipleCoils://写多个线圈寄存器——响应
                        RecvData = new byte[12];
                        size = 12;
                        break;
                    case MemAreaModbusTCP.WriteMultipleRegister://写多个保持寄存器——响应
                        RecvData = new byte[12];
                        size = 12;
                        break;
                    case MemAreaModbusTCP.WriteSingleCoil://强制单个线圈——响应
                        RecvData = new byte[12];
                        size = 12;
                        break;
                    case MemAreaModbusTCP.WriteSingleRegister://写单个保持寄存器——响应
                        RecvData = new byte[12];
                        size = 12;
                        break;
                    default:
                        return false;
                }
                byte[] data = null;
                int m_Count = 0;
                short vartmp = 0;
                Receive(ref RecvData, RecvData.Length);
                switch (pTask.Area)
                {
                    case MemAreaModbusTCP.ReadCoil:
                        if ((RecvData[0] == Convert.ToByte(pTask.id >> 8) && RecvData[1] == Convert.ToByte(pTask.id)))
                        {
                            if (RecvData[7] == 0x01)//读线圈寄存器功能码
                            {
                                if (pTask.m_unWordsCount % 8 == 0)//求
                                {
                                    m_Count = pTask.m_unWordsCount / 8;
                                }
                                else
                                {
                                    m_Count = pTask.m_unWordsCount / 8 + 1;
                                }
                                if (RecvData[8] == m_Count)//(数据长度)寄存器个数*2
                                {
                                    for (int i = 0; i < m_Count; i++)
                                    {
                                        for (int j = 0; j < 8; j++)
                                        {
                                            byte bit = RecvData[i + 9];
                                            if (Convert.ToBoolean((bit >> j) & 0x1))
                                            {
                                                UpdateReadWord(pTask.m_PLCArea, (UInt32)(pTask.m_unBeginWord + i * 8 + j), 1);
                                            }
                                            else
                                            {
                                                UpdateReadWord(pTask.m_PLCArea, (UInt32)(pTask.m_unBeginWord + i * 8 + j), 0);
                                            }
                                        }
                                    }
                                    return true;
                                }
                                else
                                {
                                    _lastError = string.Format("SendAndGetRes():ReadCoil [receive data length error]" + RecvData[8].ToString());
                                    break;
                                }
                            }
                            else
                            {
                                switch (RecvData[8])
                                {
                                    case (byte)ModBusExceptionCode.IllegalDataAddress:
                                        _lastError = string.Format("SendAndGetRes():ReadCoil [IllegalDataAddress]");
                                        break;
                                    case (byte)ModBusExceptionCode.IllegalDataValue:
                                        _lastError = string.Format("SendAndGetRes():ReadCoil [IllegalDataValue]");
                                        break;
                                    case (byte)ModBusExceptionCode.IllegalFunction:
                                        _lastError = string.Format("SendAndGetRes():ReadCoil [IllegalFunction]");
                                        break;
                                    default:
                                        _lastError = string.Format("SendAndGetRes():ReadCoil [FunctionException]");
                                        break;
                                }
                            }
                        }
                        break;
                    case MemAreaModbusTCP.ReadDiscreteInputs:
                        if ((RecvData[0] == Convert.ToByte(pTask.id >> 8) && RecvData[1] == Convert.ToByte(pTask.id)))
                        {
                            if (RecvData[7] == 0x02)//读线圈寄存器功能码
                            {
                                if (pTask.m_unWordsCount % 8 == 0)//
                                {
                                    m_Count = pTask.m_unWordsCount / 8;
                                }
                                else
                                {
                                    m_Count = pTask.m_unWordsCount / 8 + 1;
                                }
                                if (RecvData[8] == m_Count)//(数据长度)寄存器个数*2
                                {
                                    for (int i = 0; i < m_Count; i++)
                                    {
                                        for (int j = 0; j < 8; j++)
                                        {
                                            byte bit = RecvData[i + 9];
                                            if (Convert.ToBoolean((bit >> j) & 0x1))
                                            {
                                                UpdateReadWord(pTask.m_PLCArea, (UInt32)(pTask.m_unBeginWord + i * 8 + j), 1);
                                            }
                                            else
                                            {
                                                UpdateReadWord(pTask.m_PLCArea, (UInt32)(pTask.m_unBeginWord + i * 8 + j), 0);
                                            }
                                        }
                                    }
                                    return true;
                                }
                                else
                                {
                                    _lastError = string.Format("SendAndGetRes():ReadDiscreteInputs [receive data length error]" + RecvData[8].ToString());
                                    break;
                                }
                            }
                            else
                            {
                                switch (RecvData[8])
                                {
                                    case (byte)ModBusExceptionCode.IllegalDataAddress:
                                        _lastError = string.Format("SendAndGetRes():ReadDiscreteInputs [IllegalDataAddress]");
                                        break;
                                    case (byte)ModBusExceptionCode.IllegalDataValue:
                                        _lastError = string.Format("SendAndGetRes():ReadDiscreteInputs [IllegalDataValue]");
                                        break;
                                    case (byte)ModBusExceptionCode.IllegalFunction:
                                        _lastError = string.Format("SendAndGetRes():ReadDiscreteInputs [IllegalFunction]");
                                        break;
                                    default:
                                        _lastError = string.Format("SendAndGetRes():ReadDiscreteInputs [FunctionException]");
                                        break;
                                }
                            }
                        }
                        break;
                    case MemAreaModbusTCP.ReadHoldingRegister:
                        if ((RecvData[0] == Convert.ToByte(pTask.id >> 8) && RecvData[1] == Convert.ToByte(pTask.id)))
                        {
                            if (RecvData[7] == 0x03)//读线圈寄存器功能码
                            {
                                for (int i = 0; i < Convert.ToByte(pTask.m_unWordsCount); i++)
                                {
                                    vartmp = RecvData[9 + (2 * i)];
                                    vartmp = (short)(vartmp << 8);
                                    vartmp = (short)((vartmp & 0xFF00) | (RecvData[9 + ((2 * i) + 1)] & 0x00FF));
                                    //EnterCriticalSection(&m_critUpdateReadList);
                                    UpdateReadWord(pTask.m_PLCArea, (UInt32)(pTask.m_unBeginWord + i), vartmp);
                                    //LeaveCriticalSection(&m_critUpdateReadList);
                                }
                                return true;
                            }
                            else
                            {
                                switch (RecvData[8])
                                {
                                    case (byte)ModBusExceptionCode.IllegalDataAddress:
                                        _lastError = string.Format("SendAndGetRes():ReadHoldingRegister [IllegalDataAddress]");
                                        break;
                                    case (byte)ModBusExceptionCode.IllegalDataValue:
                                        _lastError = string.Format("SendAndGetRes():ReadHoldingRegister [IllegalDataValue]");
                                        break;
                                    case (byte)ModBusExceptionCode.IllegalFunction:
                                        _lastError = string.Format("SendAndGetRes():ReadHoldingRegister [IllegalFunction]");
                                        break;
                                    default:
                                        _lastError = string.Format("SendAndGetRes():ReadHoldingRegister [FunctionException]");
                                        break;
                                }
                            }
                        }
                        break;
                    case MemAreaModbusTCP.ReadInputRegister:
                        if ((RecvData[0] == Convert.ToByte(pTask.id >> 8) && RecvData[1] == Convert.ToByte(pTask.id)))
                        {
                            if (RecvData[7] == 0x04)//读线圈寄存器功能码
                            {
                                for (int i = 0; i < Convert.ToByte(pTask.m_unWordsCount); i++)
                                {
                                    vartmp = RecvData[9 + (2 * i)];
                                    vartmp = (short)(vartmp << 8);
                                    vartmp = (short)((vartmp & 0xFF00) | (RecvData[9 + ((2 * i) + 1)] & 0x00FF));
                                    //EnterCriticalSection(&m_critUpdateReadList);
                                    UpdateReadWord(pTask.m_PLCArea, (UInt32)(pTask.m_unBeginWord + i), vartmp);
                                    //LeaveCriticalSection(&m_critUpdateReadList);
                                }
                                return true;
                            }
                            else
                            {
                                switch (RecvData[8])
                                {
                                    case (byte)ModBusExceptionCode.IllegalDataAddress:
                                        _lastError = string.Format("SendAndGetRes():ReadInputRegister [IllegalDataAddress]");
                                        break;
                                    case (byte)ModBusExceptionCode.IllegalDataValue:
                                        _lastError = string.Format("SendAndGetRes():ReadInputRegister [IllegalDataValue]");
                                        break;
                                    case (byte)ModBusExceptionCode.IllegalFunction:
                                        _lastError = string.Format("SendAndGetRes():ReadInputRegister [IllegalFunction]");
                                        break;
                                    default:
                                        _lastError = string.Format("SendAndGetRes():ReadInputRegister [FunctionException]");
                                        break;
                                }
                            }
                        }
                        break;
                    case MemAreaModbusTCP.WriteSingleCoil://强制单个线圈——响应 fctWriteSingleCoil = 5,
                        if ((RecvData[0] == Convert.ToByte(pTask.id >> 8) && RecvData[1] == Convert.ToByte(pTask.id)))
                        {
                            if (RecvData[7] == 0x05)//读线圈寄存器功能码 
                            {
                                return true;//写成功
                            }
                            else
                            {
                                switch (RecvData[8])
                                {
                                    case (byte)ModBusExceptionCode.IllegalDataAddress:
                                        _lastError = string.Format("SendAndGetRes():WriteSingleCoil [IllegalDataAddress]");
                                        break;
                                    case (byte)ModBusExceptionCode.IllegalDataValue:
                                        _lastError = string.Format("SendAndGetRes():WriteSingleCoil [IllegalDataValue]");
                                        break;
                                    case (byte)ModBusExceptionCode.IllegalFunction:
                                        _lastError = string.Format("SendAndGetRes():WriteSingleCoil [IllegalFunction]");
                                        break;
                                    default:
                                        _lastError = string.Format("SendAndGetRes():WriteSingleCoil [FunctionException]");
                                        break;
                                }
                            }
                        }
                        break;
                    case MemAreaModbusTCP.WriteSingleRegister://写单个保持寄存器——响应 fctWriteSingleRegister = 6,
                        if ((RecvData[0] == Convert.ToByte(pTask.id >> 8) && RecvData[1] == Convert.ToByte(pTask.id)))
                        {
                            if (RecvData[7] == 0x06)//读线圈寄存器功能码 
                            {
                                return true;//写成功
                            }
                            else
                            {
                                switch (RecvData[8])
                                {
                                    case (byte)ModBusExceptionCode.IllegalDataAddress:
                                        _lastError = string.Format("SendAndGetRes():WriteSingleRegister [IllegalDataAddress]");
                                        break;
                                    case (byte)ModBusExceptionCode.IllegalDataValue:
                                        _lastError = string.Format("SendAndGetRes():WriteSingleRegister [IllegalDataValue]");
                                        break;
                                    case (byte)ModBusExceptionCode.IllegalFunction:
                                        _lastError = string.Format("SendAndGetRes():WriteSingleRegister [IllegalFunction]");
                                        break;
                                    default:
                                        _lastError = string.Format("SendAndGetRes():WriteSingleRegister [FunctionException]");
                                        break;
                                }
                            }
                        }
                        break;
                    case MemAreaModbusTCP.WriteMultipleCoils://写多个线圈寄存器——响应 fctWriteMultipleCoils = 15,
                        if ((RecvData[0] == Convert.ToByte(pTask.id >> 8) && RecvData[1] == Convert.ToByte(pTask.id)))
                        {
                            if (RecvData[7] == 0x0F)//读线圈寄存器功能码 
                            {
                                return true;//写成功
                            }
                            else
                            {
                                switch (RecvData[8])
                                {
                                    case (byte)ModBusExceptionCode.IllegalDataAddress:
                                        _lastError = string.Format("SendAndGetRes():WriteMultipleCoils [IllegalDataAddress]");
                                        break;
                                    case (byte)ModBusExceptionCode.IllegalDataValue:
                                        _lastError = string.Format("SendAndGetRes():WriteMultipleCoils [IllegalDataValue]");
                                        break;
                                    case (byte)ModBusExceptionCode.IllegalFunction:
                                        _lastError = string.Format("SendAndGetRes():WriteMultipleCoils [IllegalFunction]");
                                        break;
                                    default:
                                        _lastError = string.Format("SendAndGetRes():WriteMultipleCoils [FunctionException]");
                                        break;
                                }
                            }
                        }
                        break;
                    case MemAreaModbusTCP.WriteMultipleRegister://写多个保持寄存器——响应 fctWriteMultipleRegister = 16,
                        if ((RecvData[0] == Convert.ToByte(pTask.id >> 8) && RecvData[1] == Convert.ToByte(pTask.id)))
                        {
                            if (RecvData[7] == 0x10)//功能码
                            {
                                return true;//写成功
                            }
                            else
                            {
                                switch (RecvData[8])
                                {
                                    case (byte)ModBusExceptionCode.IllegalDataAddress:
                                        _lastError = string.Format("SendAndGetRes():WriteMultipleRegister [IllegalDataAddress]");
                                        break;
                                    case (byte)ModBusExceptionCode.IllegalDataValue:
                                        _lastError = string.Format("SendAndGetRes():WriteMultipleRegister [IllegalDataValue]");
                                        break;
                                    case (byte)ModBusExceptionCode.IllegalFunction:
                                        _lastError = string.Format("SendAndGetRes():WriteMultipleRegister [IllegalFunction]");
                                        break;
                                    default:
                                        _lastError = string.Format("SendAndGetRes():WriteMultipleRegister [FunctionException]");
                                        break;
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                return false;
            }
            return true;
        }

       #region 读取线圈
    //读线圈
    public bool ReadPLCBitCoil(AreaReadModbusTCPTpye area, uint unBeginWord)
        {
            if (area != AreaReadModbusTCPTpye.ReadCoil)
            {
                if (area != AreaReadModbusTCPTpye.ReadDiscreteInputs)
                {
                    return false;
                }
            }
            short result = 0;
            result = GetReadWord(area, unBeginWord);
            if (Convert.ToBoolean(result))//把其它bit设为0,再把当前位与0X1与计算，判断是否为0,或不为0
                return true;
            else
                return false;
        }
        /// <summary>
        /// 读单线圈
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool ReadPLCBitCoilWait(AreaReadModbusTCPTpye area, uint unBeginWord, int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusTCPTpye.ReadCoil)
            {
                if (area != AreaReadModbusTCPTpye.ReadDiscreteInputs)
                {
                    return false;
                }
            }
            StructModbusTCP task = new StructModbusTCP();
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord;
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusTCP)area;
            task.m_unWordsCount = 1;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);//超时 
            task.m_hEventFinish.Close();
            // Now we add an immediate task, when the task is processed the read value 
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            short result = 0;
            result = GetReadWord(area, unBeginWord);
            if (Convert.ToBoolean(result))//把其它bit设为0,再把当前位与0X1与计算，判断是否为0,或不为0
                return true;
            else
                return false;
        }

        // Read PLC word in nonoverlapped way
        //读寄存器
        public bool ReadPLCBitRegister(AreaReadModbusTCPTpye area, uint unBeginWord, int bit)
        {
            if (area != AreaReadModbusTCPTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusTCPTpye.ReadInputRegister)
                {
                    return false;
                }
            }
            short result = 0;
            result = GetReadWord(area, unBeginWord);
            if (Convert.ToBoolean((result >> bit) & 0x1))//把其它bit设为0,再把当前位与0X1与计算，判断是否为0,或不为0
                return true;
            else
                return false;
        }

        public bool ReadPLCBitWaitRegister(AreaReadModbusTCPTpye area, uint unBeginWord, int bit, int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusTCPTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusTCPTpye.ReadInputRegister)
                {
                    return false;
                }
            }
            StructModbusTCP task = new StructModbusTCP();
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord; //开始地址          
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusTCP)area;
            task.m_unWordsCount = 1;//字数量
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

        public short ReadPLCShortIntRegister(AreaReadModbusTCPTpye area, uint unBeginWord)
        {
            if (area != AreaReadModbusTCPTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusTCPTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            return GetReadWord(area, unBeginWord);
        }

        public short ReadPLCShortIntRegisterWait(AreaReadModbusTCPTpye area, uint unBeginWord, int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusTCPTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusTCPTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            StructModbusTCP task = new StructModbusTCP();
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord;//开始地址   
            task.m_unWordsCount = 1;//字数量
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusTCP)area;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            // Now we add an immediate task, when the task is processed the read value 
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            return GetReadWord(area, unBeginWord);
        }

        public ushort ReadPLCUShortIntRegisterWait(AreaReadModbusTCPTpye area, uint unBeginWord, int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusTCPTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusTCPTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            StructModbusTCP task = new StructModbusTCP();
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord;//开始地址   
            task.m_unWordsCount = 1;//字数量
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusTCP)area;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            // Now we add an immediate task, when the task is processed the read value 
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            // 
            short temp = GetReadWord(area, unBeginWord);
            return Convert.ToUInt16(temp & 0xFFFF);
        }

        public int ReadPLCIntRegister(AreaReadModbusTCPTpye area, uint unBeginWord)
        {
            if (area != AreaReadModbusTCPTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusTCPTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            int a = GetReadWord(area, unBeginWord + 1) * 256 * 256;
            int b = GetReadWord(area, unBeginWord);
            int c = GetReadWord(area, unBeginWord + 1);

            return (int)(GetReadWord(area, unBeginWord) * 256 * 256 + GetReadWord(area, unBeginWord + 1));
        }

        public int ReadPLCIntRegisterWait(AreaReadModbusTCPTpye area, uint unBeginWord, int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusTCPTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusTCPTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            StructModbusTCP task = new StructModbusTCP();
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord;//开始地址   
            task.m_unWordsCount = 2;//字数量
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusTCP)area;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            // Now we add an immediate task, when the task is processed the read value 
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            int getDWord;
            Byte[] resp = new Byte[4];
            resp[2] = (Byte)(GetReadWord(area, unBeginWord + 1) & 0xFFFF);
            resp[3] = (Byte)((GetReadWord(area, unBeginWord + 1) >> 8) & 0xFF);
            resp[0] = (Byte)(GetReadWord(area, unBeginWord) & 0xFFFF);
            resp[1] = (Byte)((GetReadWord(area, unBeginWord) >> 8) & 0xFF);
            getDWord = BitConverter.ToInt32(resp, 0);
            return getDWord;
        }

        public uint ReadPLCUIntRegisterWait(AreaReadModbusTCPTpye area, uint unBeginWord, int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusTCPTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusTCPTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            StructModbusTCP task = new StructModbusTCP();
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord;//开始地址   
            task.m_unWordsCount = 2;//字数量
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusTCP)area;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            // Now we add an immediate task, when the task is processed the read value 
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            uint getDWord;
            Byte[] resp = new Byte[4];
            resp[2] = (Byte)(GetReadWord(area, unBeginWord + 1) & 0xFFFF);
            resp[3] = (Byte)((GetReadWord(area, unBeginWord + 1) >> 8) & 0xFF);
            resp[0] = (Byte)(GetReadWord(area, unBeginWord) & 0xFFFF);
            resp[1] = (Byte)((GetReadWord(area, unBeginWord) >> 8) & 0xFF);
            getDWord = BitConverter.ToUInt32(resp, 0);
            return getDWord;
        }

        public float ReadPLCFloatRegister(AreaReadModbusTCPTpye area, uint unBeginWord)
        {
            if (area != AreaReadModbusTCPTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusTCPTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            float getDWord;
            Byte[] resp = new Byte[4];
            resp[2] = (Byte)(GetReadWord(area, unBeginWord + 1) & 0xFFFF);
            resp[3] = (Byte)((GetReadWord(area, unBeginWord + 1) >> 8) & 0xFF);
            resp[0] = (Byte)(GetReadWord(area, unBeginWord) & 0xFFFF);
            resp[1] = (Byte)((GetReadWord(area, unBeginWord) >> 8) & 0xFF);
            getDWord = BitConverter.ToSingle(resp, 0);
            return getDWord;
        }

        public float ReadPLCFloatRegisterWait(AreaReadModbusTCPTpye area, uint unBeginWord, int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusTCPTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusTCPTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            StructModbusTCP task = new StructModbusTCP();
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord;//开始地址   
            task.m_unWordsCount = 2;//字数量
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusTCP)area;
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

        public long ReadPLCLongRegister(AreaReadModbusTCPTpye area, uint unBeginWord)
        {
            if (area != AreaReadModbusTCPTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusTCPTpye.ReadInputRegister)
                {
                    return 0;
                }
            }

            return (long)(GetReadWord(area, unBeginWord + 1) * 256 * 256 * 256 * 256 * 256 * 256 * 256 * 256 + GetReadWord(area, unBeginWord) * 256 * 256 * 256 * 256 + GetReadWord(area, unBeginWord + 3) * 256 * 256 + GetReadWord(area, unBeginWord + 2));
        }

        public long ReadPLCLongRegisterWait(AreaReadModbusTCPTpye area, uint unBeginWord, int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusTCPTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusTCPTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            StructModbusTCP task = new StructModbusTCP();
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord;//开始地址   
            task.m_unWordsCount = 4;//字数量
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusTCP)area;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            // Now we add an immediate task, when the task is processed the read value 
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            long getDWord;
            Byte[] resp = new Byte[8];
            resp[2] = (Byte)(GetReadWord(area, unBeginWord + 1) & 0xFFFF);
            resp[3] = (Byte)((GetReadWord(area, unBeginWord + 1) >> 8) & 0xFF);
            resp[0] = (Byte)(GetReadWord(area, unBeginWord) & 0xFFFF);
            resp[1] = (Byte)((GetReadWord(area, unBeginWord) >> 8) & 0xFF);
            resp[6] = (Byte)(GetReadWord(area, unBeginWord + 3) & 0xFFFF);
            resp[7] = (Byte)((GetReadWord(area, unBeginWord + 3) >> 8) & 0xFF);
            resp[4] = (Byte)(GetReadWord(area, unBeginWord + 2) & 0xFFFF);
            resp[5] = (Byte)((GetReadWord(area, unBeginWord + 2) >> 8) & 0xFF);
            getDWord = BitConverter.ToInt64(resp, 0);
            return getDWord;
            //return (long)(GetReadWord(area, unBeginWord + 1) * 256 * 256 * 256 * 256 * 256 * 256 * 256 * 256 + GetReadWord(area, unBeginWord) * 256 * 256 * 256 * 256 + GetReadWord(area, unBeginWord + 3) * 256 * 256 + GetReadWord(area, unBeginWord + 2));
        }

        public double ReadPLCDoubleRegister(AreaReadModbusTCPTpye area, uint unBeginWord)
        {
            if (area != AreaReadModbusTCPTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusTCPTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            double getDWord;
            Byte[] resp = new Byte[8];
            resp[2] = (Byte)(GetReadWord(area, unBeginWord + 1) & 0xFFFF);
            resp[3] = (Byte)((GetReadWord(area, unBeginWord + 1) >> 8) & 0xFF);
            resp[0] = (Byte)(GetReadWord(area, unBeginWord) & 0xFFFF);
            resp[1] = (Byte)((GetReadWord(area, unBeginWord) >> 8) & 0xFF);
            resp[6] = (Byte)(GetReadWord(area, unBeginWord + 3) & 0xFFFF);
            resp[7] = (Byte)((GetReadWord(area, unBeginWord + 3) >> 8) & 0xFF);
            resp[4] = (Byte)(GetReadWord(area, unBeginWord + 2) & 0xFFFF);
            resp[5] = (Byte)((GetReadWord(area, unBeginWord + 2) >> 8) & 0xFF);
            getDWord = BitConverter.ToDouble(resp, 0);
            return getDWord;
        }

        public double ReadPLCDoubleRegisterWait(AreaReadModbusTCPTpye area, uint unBeginWord, int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusTCPTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusTCPTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            StructModbusTCP task = new StructModbusTCP();
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord;//开始地址   
            task.m_unWordsCount = 4;//字数量
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusTCP)area;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            double getDWord;
            Byte[] resp = new Byte[8];
            resp[2] = (Byte)(GetReadWord(area, unBeginWord + 1) & 0xFFFF);
            resp[3] = (Byte)((GetReadWord(area, unBeginWord + 1) >> 8) & 0xFF);
            resp[0] = (Byte)(GetReadWord(area, unBeginWord) & 0xFFFF);
            resp[1] = (Byte)((GetReadWord(area, unBeginWord) >> 8) & 0xFF);
            resp[6] = (Byte)(GetReadWord(area, unBeginWord + 3) & 0xFFFF);
            resp[7] = (Byte)((GetReadWord(area, unBeginWord + 3) >> 8) & 0xFF);
            resp[4] = (Byte)(GetReadWord(area, unBeginWord + 2) & 0xFFFF);
            resp[5] = (Byte)((GetReadWord(area, unBeginWord + 2) >> 8) & 0xFF);
            getDWord = BitConverter.ToDouble(resp, 0);
            return getDWord;
        }

        public string ReadPLCStringRegister(AreaReadModbusTCPTpye area, uint unBeginWord, short unByteCount)
        {
            if (area != AreaReadModbusTCPTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusTCPTpye.ReadInputRegister)
                {
                    return null;
                }
            }
            short inttemp1 = 0;
            byte[] temp = new byte[unByteCount];
            Array.Clear(temp, 0, unByteCount);
            inttemp1 = (short)(unByteCount % 2);
            for (int i = 0; i < unByteCount; i++, i++)
            {
                temp[i] = (byte)(GetReadWord(area, (uint)(unBeginWord + i / 2)) / 256);
                if ((i + 1) < unByteCount)
                {
                    temp[i + 1] = (byte)(GetReadWord(area, (uint)(unBeginWord + i / 2)) % 256);
                }
            }
            string readString = null;
            for (int i = 0; i < unByteCount; i++)
            {
                if (temp[i] != 0X00)
                {
                    readString += Encoding.ASCII.GetString(temp, i, 1);
                }
            }
            return readString;
        }

        public string ReadPLCStringRegisterWait(AreaReadModbusTCPTpye area, uint unBeginWord, short unByteCount, int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusTCPTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusTCPTpye.ReadInputRegister)
                {
                    return null;
                }
            }
            StructModbusTCP task = new StructModbusTCP();
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord;//开始地址   
            task.m_unWordsCount = unByteCount % 2 > 0 ? (ushort)(unByteCount / 2 + 1) : (ushort)(unByteCount / 2);//字数量
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusTCP)area;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            short inttemp1 = 0;
            byte[] temp = new byte[unByteCount];
            Array.Clear(temp, 0, unByteCount);
            inttemp1 = (short)(unByteCount % 2);
            for (int i = 0; i < unByteCount; i++, i++)
            {
                temp[i] = (byte)(GetReadWord(area, (uint)(unBeginWord + i / 2)) / 256);
                if ((i + 1) < unByteCount)
                {
                    temp[i + 1] = (byte)(GetReadWord(area, (uint)(unBeginWord + i / 2)) % 256);
                }
            }
            string readString = null;
            for (int i = 0; i < unByteCount; i++)
            {
                if (temp[i] != 0X00)
                {
                    readString += Encoding.ASCII.GetString(temp, i, 1);
                }
            }
            return readString;
        }
    #endregion

       #region 写入线圈
     
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unBeginWord"></param>
        /// <param name="bValue"></param>
        /// <returns></returns>
        public bool WritePLCBitCoil(uint unBeginWord, bool bValue)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.         
            StructModbusTCP task = new StructModbusTCP();
            task.Area = MemAreaModbusTCP.WriteSingleCoil;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_bBitWrite = bValue;
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        //06	写单个保持寄存器	40001-49999	字操作	单个
        public bool WritePLCShortIntRegister(uint unBeginWord, short shortintValue)//写PLC16位数据(有符号)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            StructModbusTCP task = new StructModbusTCP();
            task.Area = MemAreaModbusTCP.WriteSingleRegister;
            task.m_unBeginWord = (int)unBeginWord;
            task.values = BitConverter.GetBytes(shortintValue);
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        public bool WritePLCUShortIntRegister(uint unBeginWord, ushort shortintValue)//写PLC16位数据(有符号)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            StructModbusTCP task = new StructModbusTCP();
            task.Area = MemAreaModbusTCP.WriteSingleRegister;
            task.m_unBeginWord = (int)unBeginWord;
            task.values = BitConverter.GetBytes(shortintValue);
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        public bool WritePLCIntRegister(uint unBeginWord, int nValue)//写PLC32位数据(有符号)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            StructModbusTCP task = new StructModbusTCP();
            task.Area = MemAreaModbusTCP.WriteMultipleRegister;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_unWordsCount = 2;
            Byte[] resp = new Byte[4];
            resp = BitConverter.GetBytes(nValue);
            Byte[] resp1 = new Byte[4];
            resp1[0] = resp[1];
            resp1[1] = resp[0];
            resp1[2] = resp[3];
            resp1[3] = resp[2];
            task.values = resp1;
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        public bool WritePLCUIntRegister(uint unBeginWord, uint nValue)//写PLC32位数据(有符号)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            StructModbusTCP task = new StructModbusTCP();
            task.Area = MemAreaModbusTCP.WriteMultipleRegister;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_unWordsCount = 2;
            Byte[] resp = new Byte[4];
            resp = BitConverter.GetBytes(nValue);
            Byte[] resp1 = new Byte[4];
            resp1[0] = resp[1];
            resp1[1] = resp[0];
            resp1[2] = resp[3];
            resp1[3] = resp[2];
            task.values = resp1;
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        public bool WritePLCFloatRegister(uint unBeginWord, float fValue)//写32位数据(浮点数)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            StructModbusTCP task = new StructModbusTCP();
            task.Area = MemAreaModbusTCP.WriteMultipleRegister;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_unWordsCount = 2;
            Byte[] resp = new Byte[4];
            resp = BitConverter.GetBytes(fValue);
            Byte[] resp1 = new Byte[4];
            resp1[0] = resp[1];
            resp1[1] = resp[0];
            resp1[2] = resp[3];
            resp1[3] = resp[2];
            task.values = resp1;
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        public bool WritePLCDoubleRegister(uint unBeginWord, double fValue)//写64位数据double
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            StructModbusTCP task = new StructModbusTCP();
            task.Area = MemAreaModbusTCP.WriteMultipleRegister;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_unWordsCount = 4;
            Byte[] resp = new Byte[8];
            resp = BitConverter.GetBytes(fValue);
            Byte[] resp1 = new Byte[8];
            resp1[0] = resp[1];
            resp1[1] = resp[0];
            resp1[2] = resp[3];
            resp1[3] = resp[2];
            resp1[4] = resp[5];
            resp1[5] = resp[4];
            resp1[6] = resp[7];
            resp1[7] = resp[6];
            task.values = resp1;
            //             resp[0] = (Byte)(GetReadWord(area, unBeginWord) & 0xFFFF);
            //             resp[1] = (Byte)((GetReadWord(area, unBeginWord) >> 8) & 0xFF);
            //             resp[2] = (Byte)(GetReadWord(area, unBeginWord + 1) & 0xFFFF);
            //             resp[3] = (Byte)((GetReadWord(area, unBeginWord + 1) >> 8) & 0xFF);
            //             resp[4] = (Byte)(GetReadWord(area, unBeginWord + 2) & 0xFFFF);
            //             resp[5] = (Byte)((GetReadWord(area, unBeginWord + 2) >> 8) & 0xFF);            
            //             resp[6] = (Byte)(GetReadWord(area, unBeginWord + 3) & 0xFFFF);
            //             resp[7] = (Byte)((GetReadWord(area, unBeginWord + 3) >> 8) & 0xFF);

            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        public bool WritePLCLongRegister(uint unBeginWord, long fValue)//写64位数据double
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            StructModbusTCP task = new StructModbusTCP();
            task.Area = MemAreaModbusTCP.WriteMultipleRegister;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_unWordsCount = 4;
            Byte[] resp = new Byte[8];
            resp = BitConverter.GetBytes(fValue);
            Byte[] resp1 = new Byte[8];
            resp1[0] = resp[1];
            resp1[1] = resp[0];
            resp1[2] = resp[3];
            resp1[3] = resp[2];
            resp1[4] = resp[5];
            resp1[5] = resp[4];
            resp1[6] = resp[7];
            resp1[7] = resp[6];
            task.values = resp1;
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }

        public bool WritePLCStringRegister(uint unBeginWord, string strValue)//写string数据
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            StructModbusTCP task = new StructModbusTCP();
            task.Area = MemAreaModbusTCP.WriteMultipleRegister;
            task.m_unBeginWord = (int)unBeginWord;
            task.values = Encoding.ASCII.GetBytes(strValue);
            task.values = ArrayExpandToLengthEven(task.values);
            task.m_unWordsCount = (ushort)(task.values.Length / 2);
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        #endregion
        /// <summary>
        /// 将一个数组进行扩充到指定长度，或是缩短到指定长度
        /// </summary>
        /// <typeparam name="T">数组的类型</typeparam>
        /// <param name="data">原先数据的数据</param>
        /// <param name="length">新数组的长度</param>
        /// <returns>新数组长度信息</returns>
        public static T[] ArrayExpandToLength<T>(T[] data, int length)
        {
            if (data == null) return new T[length];

            if (data.Length == length) return data;

            T[] buffer = new T[length];

            Array.Copy(data, buffer, Math.Min(data.Length, buffer.Length));

            return buffer;
        }

        /// <summary>
        /// 将一个数组进行扩充到偶数长度
        /// </summary>
        /// <typeparam name="T">数组的类型</typeparam>
        /// <param name="data">原先数据的数据</param>
        /// <returns>新数组长度信息</returns>
        public static T[] ArrayExpandToLengthEven<T>(T[] data)
        {
            if (data == null) return new T[0];

            if (data.Length % 2 == 1)
            {
                return ArrayExpandToLength(data, data.Length + 1);
            }
            else
            {
                return data;
            }
        }

}

