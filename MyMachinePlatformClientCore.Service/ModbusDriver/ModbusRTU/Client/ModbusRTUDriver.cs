using System.IO.Ports;
using System.Runtime.InteropServices;
using System.Text;

namespace MyMachinePlatformClientCore.Service.ModbusDriver.ModbusRTU;
public enum MemAreaModbusRTUD 
{
    ReadCoil, 
    ReadDiscreteInputs,
    ReadHoldingRegister, 
    ReadInputRegister, 
    WriteSingleCoil, 
    WriteMultipleCoils, 
    WriteSingleRegister,
    WriteMultipleRegister 
}
public enum DataType : byte
{
    NONE = 0,
    BOOL = 1,
    BYTE = 3,
    SHORT = 4,
    WORD = 5,
    TIME = 6,
    INT = 7,
    FLOAT = 8,
    SYS = 9,
    STR = 11
}
public enum AreaReadModbusRTUDTpye
{
    ReadCoil = (int)MemAreaModbusRTUD.ReadCoil,
    ReadDiscreteInputs = (int)MemAreaModbusRTUD.ReadDiscreteInputs,
    ReadHoldingRegister = (int)MemAreaModbusRTUD.ReadHoldingRegister,
    ReadInputRegister = (int)MemAreaModbusRTUD.ReadInputRegister
}
public enum AreaWriteModbusRTUDTpye
{
    WriteSingleCoil = (int)MemAreaModbusRTUD.WriteSingleCoil,
    WriteMultipleCoils = (int)MemAreaModbusRTUD.WriteMultipleCoils,
    WriteSingleRegister = (int)MemAreaModbusRTUD.WriteSingleRegister,
    WriteMultipleRegister = (int)MemAreaModbusRTUD.WriteMultipleRegister
}
/// <summary>
/// 
/// </summary>
public class ModbusRTUDriver
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

        private List<StructModbusRTUD> m_periodTaskList = new List<StructModbusRTUD>();

        private Queue<StructModbusRTUD> m_immeTaskList = new Queue<StructModbusRTUD>();

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

        // Open and init the serial port
        public bool OpenComm(int timeOut = 1000, string port = "COM1", string baudRate = "9600")
        {
            _port = port;
            _serialPort = new SerialPort(port);
            _timeOut = timeOut;
            _serialPort.ReadTimeout = _timeOut;
            _serialPort.WriteTimeout = _timeOut;
            _serialPort.BaudRate = int.Parse(baudRate);
            _serialPort.DataBits = 8;
            _serialPort.Parity = Parity.None;
            _serialPort.StopBits = StopBits.One;
            try
            {
                _serialPort.Open();           
            }
            catch (Exception error)
            {

                return false;
            }
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
            m_pSocketThreadProc = new System.Threading.Thread(SocketThreadProc);
            if (m_pSocketThreadProc == null)
            {
                return false;
            }
            m_pSocketThreadProc.Start();
            return true;
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

        // in overlapped read function (ReadPLCWord) without wait.
        public bool AddReadArea(AreaReadModbusRTUDTpye area, int unBeginWord, ushort unWordsCount)
        {
            StructModbusRTUD task = new StructModbusRTUD();
            task.Area = (MemAreaModbusRTUD)area;
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

        // Clear all the added PLC memory.
        public bool ClearReadArea()
        {
            if (m_periodTaskList.Count != -1)
            {
                m_periodTaskList.Clear();
            }
            return true;
        }

        private short GetReadWord(AreaReadModbusRTUDTpye area, uint unWordAddress)
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
        private bool UpdateReadWord(AreaReadModbusRTUDTpye area, UInt32 unWordAddress, short wUpdate)
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

        private void AddImmeTask(StructModbusRTUD task)
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
                _lastError = "m_periodTaskList.Count is 0";
                return;
            }
            bool bRes;
            foreach (StructModbusRTUD task in m_periodTaskList)
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
            StructModbusRTUD task = new StructModbusRTUD();
            while (m_immeTaskList.Count != 0)
            {
                lock (m_critSecImmeList)
                {
                    task = m_immeTaskList.Dequeue();//移除并返回
                }
                res = SendAndGetRes(task);
                if (task.m_hEventFinish != null && res==true)
                {
                    // Tell the call thread (main thread of the program) that// immediate task has finished.                   
                    task.m_hEventFinish.Set();
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="pTask"></param>
        /// <returns></returns>
        private bool SendAndGetRes(StructModbusRTUD pTask)
        {         
            try
            {
                _serialPort.Write(pTask.m_strCmd, 0, pTask.m_strCmd.Length);
                byte[] frameBytes = null;
                int size = 0;
                switch (pTask.Area)
                {
                    case MemAreaModbusRTUD.ReadCoil:
                    case MemAreaModbusRTUD.ReadDiscreteInputs:
                        if (pTask.m_unWordsCount % 8 == 0)
                        {
                            frameBytes = new byte[pTask.m_unWordsCount / 8 + 5];
                            size = pTask.m_unWordsCount / 8 + 5;
                        }
                        else
                        {
                            frameBytes = new byte[pTask.m_unWordsCount / 8 + 5 + 1];
                            size = pTask.m_unWordsCount / 8 + 5 + 1;
                        }                           
                        break;
                    case MemAreaModbusRTUD.ReadHoldingRegister:
                    case MemAreaModbusRTUD.ReadInputRegister:
                        frameBytes = new byte[pTask.m_unWordsCount * 2 + 5];
                        size = pTask.m_unWordsCount * 2 + 5;
                        break;
                    case MemAreaModbusRTUD.WriteMultipleCoils://写多个线圈寄存器——响应
                        frameBytes = new byte[9];
                        size = 9;
                        break;
                    case MemAreaModbusRTUD.WriteMultipleRegister://写多个保持寄存器——响应
                        frameBytes = new byte[8];
                        size = 8;
                        break;
                    case MemAreaModbusRTUD.WriteSingleCoil://强制单个线圈——响应
                        frameBytes = new byte[8];
                        size = 8;
                        break;
                    case MemAreaModbusRTUD.WriteSingleRegister://写单个保持寄存器——响应
                        frameBytes = new byte[8];
                        size = 8;
                        break;
                    default:
                        return false;                       
                 } 
                 byte[] data = null;                                        
                int numBytesRead = 0;
                while (numBytesRead != size)
                {
                    numBytesRead += _serialPort.Read(frameBytes, numBytesRead, size - numBytesRead);
                }
                if (Utility.CheckSumCRC(frameBytes))
                {
                     if (pTask.Area == MemAreaModbusRTUD.ReadCoil || pTask.Area == MemAreaModbusRTUD.ReadDiscreteInputs || pTask.Area == MemAreaModbusRTUD.ReadHoldingRegister || pTask.Area == MemAreaModbusRTUD.ReadInputRegister)
                     {
                         data = new byte[size - 5];//读数据
                         Array.Copy(frameBytes, 3, data, 0, data.Length);
                         if (data == null)
                         {
                             return false;
                         }
                     }                                  
                     switch (pTask.Area)
                     {
                         case MemAreaModbusRTUD.ReadCoil:                       
                             for (ushort i = 0; i < data.Length; i++)
                             {
                                 for (ushort j = 0; j < 8; j++)
                                 {
                                     byte bit = data[i];
                                     if (Convert.ToBoolean((bit >> j) & 0x1))//把其它bit设为0,再把当前位与0X1与计算，判断是否为0,或不为0
                                         UpdateReadWord(pTask.m_PLCArea, (uint)(pTask.m_unBeginWord + i * 8 + j), 1);
                                     else
                                         UpdateReadWord(pTask.m_PLCArea, (uint)(pTask.m_unBeginWord + i * 8 + j), 0);
                                 }
                             }                                              
                             break;
                         case MemAreaModbusRTUD.ReadDiscreteInputs:
                             for (ushort i = 0; i < data.Length; i++)
                             {
                                 for (ushort j = 0; j < 8; j++)
                                 {
                                     byte bit = data[i];
                                     if (Convert.ToBoolean((bit >> j) & 0x1))//把其它bit设为0,再把当前位与0X1与计算，判断是否为0,或不为0
                                         UpdateReadWord(pTask.m_PLCArea, (uint)(pTask.m_unBeginWord + i * 8 + j), 1);
                                     else
                                         UpdateReadWord(pTask.m_PLCArea, (uint)(pTask.m_unBeginWord  + i * 8 + j), 0);
                                 }
                             }       
                             break;
                         case MemAreaModbusRTUD.ReadHoldingRegister:
                             if (data.Length < 2)
                             {
                                 return false;                              
                             }
                             if (data.Length % 2 == 1)
                             {
                                 return false;                            
                             }
                             for (ushort i = 0; i < data.Length; i++, i++)
                             {
                                 UpdateReadWord(pTask.m_PLCArea, (uint)(pTask.m_unBeginWord + i / 2), (short)(data[i + 1] + data[i] * 256));
                                 short a = (short)(data[i + 1] + data[i] * 256);
                             }
                             break;                  
                         case MemAreaModbusRTUD.ReadInputRegister:
                             if (data.Length<2)
                             {
                                 return false;
                             }
                             if (data.Length % 2 == 1)
                             {
                                 return false;                             
                             }                          
                             for (ushort i = 0; i < data.Length; i++, i++)
                             {
                                 UpdateReadWord(pTask.m_PLCArea, (uint)(pTask.m_unBeginWord + i / 2), (short)(data[i+1] + data[i] * 256));
                             }
                             break;      
                         default:
                             break;
                     }
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {            
                return false;
            }
            return true;
        }

        #region 读取数据

        

        
        //读线圈
        public bool ReadPLCBitCoil(AreaReadModbusRTUDTpye area, uint unBeginWord)
        {
            if (area != AreaReadModbusRTUDTpye.ReadCoil )
            {
                if (area != AreaReadModbusRTUDTpye.ReadDiscreteInputs)
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
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool ReadPLCBitCoilWait(AreaReadModbusRTUDTpye area, uint unBeginWord ,int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusRTUDTpye.ReadCoil)
            {
                if (area != AreaReadModbusRTUDTpye.ReadDiscreteInputs)
                {
                    return false;
                }
            }
            StructModbusRTUD task = new StructModbusRTUD();
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord;
            task.m_PLCArea = area;
            task.m_unWordsCount = 1;
            task.Area = (MemAreaModbusRTUD)area;         
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);//超时 
            task.m_hEventFinish.Close();
             
            short result = 0;
            result = GetReadWord(area, unBeginWord);
            if (Convert.ToBoolean(result))//把其它bit设为0,再把当前位与0X1与计算，判断是否为0,或不为0
                return true;
            else
                return false;
        }

       /// <summary>
       /// 读取寄存器
       /// </summary>
       /// <param name="area"></param>
       /// <param name="unBeginWord"></param>
       /// <param name="bit"></param>
       /// <returns></returns>
        public bool ReadPLCBitRegister(AreaReadModbusRTUDTpye area, uint unBeginWord, int bit)
        {
            if (area != AreaReadModbusRTUDTpye.ReadHoldingRegister )
            {         
                if ( area != AreaReadModbusRTUDTpye.ReadInputRegister)
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
        /// <summary>
        /// 读取寄存器
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="bit"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public bool ReadPLCBitWaitRegister(AreaReadModbusRTUDTpye area, uint unBeginWord, int bit, int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusRTUDTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusRTUDTpye.ReadInputRegister)
                {
                    return false;
                }
            }
            StructModbusRTUD task = new StructModbusRTUD();
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord; //开始地址          
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusRTUD)area;
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
        /// <summary>
        /// 读取PLC寄存器
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <returns></returns>
        public short ReadPLCShortIntRegister(AreaReadModbusRTUDTpye area, uint unBeginWord)
        {
            if (area != AreaReadModbusRTUDTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusRTUDTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            return GetReadWord(area, unBeginWord);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public short ReadPLCShortIntRegisterWait(AreaReadModbusRTUDTpye area, uint unBeginWord, int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusRTUDTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusRTUDTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            StructModbusRTUD task = new StructModbusRTUD();        
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord;//开始地址   
            task.m_unWordsCount = 1;//字数量
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusRTUD)area;              
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            
            return GetReadWord(area, unBeginWord);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public ushort ReadPLCUShortIntRegisterWait(AreaReadModbusRTUDTpye area, uint unBeginWord, int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusRTUDTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusRTUDTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            StructModbusRTUD task = new StructModbusRTUD();
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord;//开始地址   
            task.m_unWordsCount = 1;//字数量
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusRTUD)area;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            short temp = GetReadWord(area, unBeginWord);
            return Convert.ToUInt16(temp & 0xFFFF);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <returns></returns>
        public int ReadPLCIntRegister(AreaReadModbusRTUDTpye area, uint unBeginWord)
        {
            if (area != AreaReadModbusRTUDTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusRTUDTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            int a = GetReadWord(area, unBeginWord + 1) * 256 * 256;
            int b = GetReadWord(area, unBeginWord);
            int c = GetReadWord(area, unBeginWord + 1);

            return (int)(GetReadWord(area, unBeginWord + 1) * 256 * 256 + GetReadWord(area, unBeginWord));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public int ReadPLCIntRegisterWait(AreaReadModbusRTUDTpye area, uint unBeginWord, int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusRTUDTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusRTUDTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            StructModbusRTUD task = new StructModbusRTUD();        
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord;//开始地址   
            task.m_unWordsCount = 2;//字数量
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusRTUD)area;    
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
           
            int getDWord;
            Byte[] resp = new Byte[4];
            resp[2] = (Byte)(GetReadWord(area, unBeginWord + 1) & 0xFFFF);
            resp[3] = (Byte)((GetReadWord(area, unBeginWord + 1) >> 8) & 0xFF);
            resp[0] = (Byte)(GetReadWord(area, unBeginWord) & 0xFFFF);
            resp[1] = (Byte)((GetReadWord(area, unBeginWord) >> 8) & 0xFF);
            getDWord = BitConverter.ToInt32(resp, 0);
            return getDWord;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public uint ReadPLCUIntRegisterWait(AreaReadModbusRTUDTpye area, uint unBeginWord, int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusRTUDTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusRTUDTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            StructModbusRTUD task = new StructModbusRTUD();
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord;//开始地址   
            task.m_unWordsCount = 2;//字数量
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusRTUD)area;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            
            uint getDWord;
            Byte[] resp = new Byte[4];
            resp[2] = (Byte)(GetReadWord(area, unBeginWord + 1) & 0xFFFF);
            resp[3] = (Byte)((GetReadWord(area, unBeginWord + 1) >> 8) & 0xFF);
            resp[0] = (Byte)(GetReadWord(area, unBeginWord) & 0xFFFF);
            resp[1] = (Byte)((GetReadWord(area, unBeginWord) >> 8) & 0xFF);
            getDWord = BitConverter.ToUInt32(resp, 0);
            return getDWord;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public long ReadPLCLongRegisterWait(AreaReadModbusRTUDTpye area, uint unBeginWord, int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusRTUDTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusRTUDTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            StructModbusRTUD task = new StructModbusRTUD();
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord;//开始地址   
            task.m_unWordsCount = 4;//字数量
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusRTUD)area;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
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
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <returns></returns>
        public float ReadPLCFloatRegister(AreaReadModbusRTUDTpye area, uint unBeginWord)
        {
            if (area != AreaReadModbusRTUDTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusRTUDTpye.ReadInputRegister)
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public float ReadPLCFloatRegisterWait(AreaReadModbusRTUDTpye area, uint unBeginWord, int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusRTUDTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusRTUDTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            StructModbusRTUD task = new StructModbusRTUD();
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord;//开始地址   
            task.m_unWordsCount = 2;//字数量
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusRTUD)area;    
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <returns></returns>
        public double ReadPLCDoubleRegister(AreaReadModbusRTUDTpye area, uint unBeginWord)
        {
            if (area != AreaReadModbusRTUDTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusRTUDTpye.ReadInputRegister)
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public double ReadPLCDoubleRegisterWait(AreaReadModbusRTUDTpye area, uint unBeginWord, int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusRTUDTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusRTUDTpye.ReadInputRegister)
                {
                    return 0;
                }
            }
            StructModbusRTUD task = new StructModbusRTUD();
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord;//开始地址   
            task.m_unWordsCount = 4;//字数量
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusRTUD)area;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="unByteCount"></param>
        /// <returns></returns>
        public string ReadPLCStringRegister(AreaReadModbusRTUDTpye area, uint unBeginWord, short unByteCount)
        {
            if (area != AreaReadModbusRTUDTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusRTUDTpye.ReadInputRegister)
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="unByteCount"></param>
        /// <param name="unMilliseconds"></param>
        /// <returns></returns>
        public string ReadPLCStringRegisterWait(AreaReadModbusRTUDTpye area, uint unBeginWord, short unByteCount, int unMilliseconds = 1000)
        {
            if (area != AreaReadModbusRTUDTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusRTUDTpye.ReadInputRegister)
                {
                    return null;
                }
            }
            StructModbusRTUD task = new StructModbusRTUD();
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord;//开始地址   
            task.m_unWordsCount = unByteCount % 2 > 0 ? (ushort)(unByteCount / 2 + 1) : (ushort)(unByteCount / 2);//字数量
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusRTUD)area;
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unBeginWord"></param>
        /// <param name="unMilliseconds"></param>
        /// <param name="registerNum"></param>
        /// <returns></returns>
        public ushort[] ReadMultiRegisterWait(AreaReadModbusRTUDTpye area, uint unBeginWord, int unMilliseconds = 1000,ushort registerNum = 8)
        {
            ushort[] resp = new ushort[registerNum];
            if (area != AreaReadModbusRTUDTpye.ReadHoldingRegister)
            {
                if (area != AreaReadModbusRTUDTpye.ReadInputRegister)
                {
                    return resp;
                }
            }
            StructModbusRTUD task = new StructModbusRTUD();
            task.m_hEventFinish.Reset();
            task.m_unBeginWord = (int)unBeginWord;//开始地址   
            task.m_unWordsCount = registerNum;//寄存器数量
            task.m_PLCArea = area;
            task.Area = (MemAreaModbusRTUD)area;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            float getDWord;
            for (uint i = 0; i < registerNum; i++)
            {
                resp[i]=Convert.ToUInt16(GetReadWord(area, unBeginWord + i) & 0xFFFF);
            }
            return resp;
        }
        #endregion

        #region  写入

        

       
        /// Write PLC bit in overlapped way
        //代码	中文名称	寄存器PLC地址	位操作/字操作	操作数量
        //05	写单个线圈	00001-09999	位操作	单个
        public bool WritePLCBitCoil(uint unBeginWord, bool bValue)
        {        
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.         
            StructModbusRTUD task = new StructModbusRTUD();
            task.Area = MemAreaModbusRTUD.WriteSingleCoil;
            task.m_unBeginWord = (int)unBeginWord;
            task.m_bBitWrite = bValue;        
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        /// <summary>
        ///  //06	写单个保持寄存器	40001-49999	字操作	单个
        /// //写PLC16位数据(有符号)
        /// </summary>
        /// <param name="unBeginWord"></param>
        /// <param name="shortintValue"></param>
        /// <returns></returns>
        
        public bool WritePLCShortIntRegister(uint unBeginWord, short shortintValue) 
        {       
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            StructModbusRTUD task = new StructModbusRTUD();
            task.Area = MemAreaModbusRTUD.WriteSingleRegister;
            task.m_unBeginWord = (int)unBeginWord;
            task.values = BitConverter.GetBytes(shortintValue);
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unBeginWord"></param>
        /// <param name="shortintValue"></param>
        /// <returns></returns>
        public bool WritePLCUShortIntRegister(uint unBeginWord, ushort shortintValue)//写PLC16位数据(有符号)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            StructModbusRTUD task = new StructModbusRTUD();
            task.Area = MemAreaModbusRTUD.WriteSingleRegister;
            task.m_unBeginWord = (int)unBeginWord;
            task.values = BitConverter.GetBytes(shortintValue);
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unBeginWord"></param>
        /// <param name="nValue"></param>
        /// <returns></returns>
        public bool WritePLCIntRegister(uint unBeginWord, int nValue)//写PLC16位数据(有符号)
        {         
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            StructModbusRTUD task = new StructModbusRTUD();
            task.Area = MemAreaModbusRTUD.WriteMultipleRegister;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unBeginWord"></param>
        /// <param name="nValue"></param>
        /// <returns></returns>
        public bool WritePLCUIntRegister(uint unBeginWord, uint nValue)//写PLC16位数据(有符号)
        {
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            StructModbusRTUD task = new StructModbusRTUD();
            task.Area = MemAreaModbusRTUD.WriteMultipleRegister;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unBeginWord"></param>
        /// <param name="fValue"></param>
        /// <returns></returns>
        public bool WritePLCFloatRegister(uint unBeginWord, float fValue)//写PLC32位数据(浮点数)
        {       
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            StructModbusRTUD task = new StructModbusRTUD();
            task.Area = MemAreaModbusRTUD.WriteMultipleRegister;
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unBeginWord"></param>
        /// <param name="fValue"></param>
        /// <returns></returns>
       public bool WritePLCDoubleRegister(uint unBeginWord, double fValue)//写PLC32位数据(浮点数)
       {
           // We do not real Read or Write Comm here and just product
           // the task, add it to the list, and signal the event so that
           // the serve thread can process the task.
           StructModbusRTUD task = new StructModbusRTUD();
           task.Area = MemAreaModbusRTUD.WriteMultipleRegister;
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
       /// <summary>
       /// 
       /// </summary>
       /// <param name="unBeginWord"></param>
       /// <param name="nValue"></param>
       /// <returns></returns>
       public bool WritePLCLongRegister(uint unBeginWord, long nValue)//写PLC16位数据(有符号)
       {
           // We do not real Read or Write Comm here and just product
           // the task, add it to the list, and signal the event so that
           // the serve thread can process the task.
           StructModbusRTUD task = new StructModbusRTUD();
           task.Area = MemAreaModbusRTUD.WriteMultipleRegister;
           task.m_unBeginWord = (int)unBeginWord;
           task.m_unWordsCount = 4;
           Byte[] resp = new Byte[8];
           resp = BitConverter.GetBytes(nValue);
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
       /// <summary>
       /// 
       /// </summary>
       /// <param name="unBeginWord"></param>
       /// <param name="strValue"></param>
       /// <returns></returns>
        public bool WritePLCStringRegister(uint unBeginWord, string strValue)//写PLC32位数据(浮点数)
        {       
            // We do not real Read or Write Comm here and just product
            // the task, add it to the list, and signal the event so that
            // the serve thread can process the task.
            StructModbusRTUD task = new StructModbusRTUD();
            task.Area = MemAreaModbusRTUD.WriteMultipleRegister;
            task.m_unBeginWord = (int)unBeginWord;           
            task.values = Encoding.ASCII.GetBytes(strValue);
            task.values = ArrayExpandToLengthEven(task.values);
            task.GenStrCmd();
            AddImmeTask(task);
            return true;
        }
        #endregion
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
}