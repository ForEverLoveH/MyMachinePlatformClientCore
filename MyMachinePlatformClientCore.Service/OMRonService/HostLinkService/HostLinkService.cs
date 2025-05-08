using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Service.OMRonService
{
    /// <summary>
    /// 主要应用为RS232，由于站号都设置为0，所以RS485必须得更改站号
    /// </summary>
    public class HostLinkService
    {
        [DllImport("kernel32", CharSet = CharSet.Auto)]
        private static extern uint GetTickCount();
        object m_critSecPeriodList = new object();
        object m_critSecImmeList = new object();
        object m_critUpdateReadList = new object();
        private static System.Threading.Thread m_pSocketThreadProc;
        private ManualResetEvent m_hThreadExitEvent = null;
        // event handles to synchronize threads 
        private AutoResetEvent m_hImmeTaskEvent = null;
        private WaitHandle GetThreadExitHandle() { return m_hThreadExitEvent; }
        private WaitHandle GetImmeTaskEvtHandle() { return m_hImmeTaskEvent; }
        private SerialPort _serialPort;
        private string _lastError = "";
        string _port;
        private int _timeOut;
        List<OmronStructHostLink> m_periodTaskList = new List<OmronStructHostLink>();
        Queue<OmronStructHostLink> m_immeTaskList = new Queue<OmronStructHostLink>();
        Dictionary<uint, short>[] m_mapReadPLCMem = new Dictionary<uint, short>[6]
        {
            new Dictionary<uint, short>(),
            new Dictionary<uint, short>(),
            new Dictionary<uint, short>(),
            new Dictionary<uint, short>(),
            new Dictionary<uint, short>(),
            new Dictionary<uint, short>(),
        };
        Dictionary<uint, uint>[] m_mapReadPLCMemTimeStamp = new Dictionary<uint, uint>[6]
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
       /// <summary>
       /// 
       /// </summary>
       /// <param name="timeOut"></param>
       /// <param name="port"></param>
       /// <param name="baudRate"></param>
       /// <returns></returns>
        public bool OpenComm(int timeOut = 1000, string port = "COM1", string baudRate = "9600")
        {
            _port = port;
            _serialPort = new SerialPort(port);
            _timeOut = timeOut;
            _serialPort.ReadTimeout = _timeOut;
            _serialPort.WriteTimeout = _timeOut;
            _serialPort.BaudRate = int.Parse(baudRate);
            _serialPort.DataBits = 8;
            _serialPort.Parity = Parity.Even;
            _serialPort.StopBits = StopBits.One;
            try
            {
                _serialPort.Open();
                //return true;
            }
            catch (IOException error)
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
        public bool AddReadArea(MemAreaHostLink area, uint unBeginWord, uint unWordsCount)
        {
            OmronStructHostLink task = new OmronStructHostLink();
            task.m_nReadOrWrite = 0;
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

        private short GetReadWord(MemAreaHostLink area, uint unWordAddress)
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
        private bool UpdateReadWord(MemAreaHostLink area, UInt32 unWordAddress, short wUpdate)
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

        private void AddImmeTask(OmronStructHostLink task)
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
            foreach (OmronStructHostLink task in m_periodTaskList)
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
            OmronStructHostLink task = new OmronStructHostLink();
            while (m_immeTaskList.Count != 0)
            {
                lock (m_critSecImmeList)
                {
                    task = m_immeTaskList.Dequeue();//移除并返回
                }
                res = SendAndGetRes(task);
                if (task.m_hEventFinish != null && res == true)
                {
                    // Tell the call thread (main thread of the program) that// immediate task has finished.                   
                    task.m_hEventFinish.Set();
                }
            }
        }

        public bool SendAndGetRes(OmronStructHostLink pTask)
        {
            Byte[] strRes = new Byte[1024];
            int nStrResLen = 0;
            int nCount = 0;
            string strTotalRes;
            _serialPort.Write(pTask.m_strCmd, 0, pTask.DataLen);
            try
            {
                while (true)
                {
                    nCount += _serialPort.Read(strRes, nCount, 1024);//偏移
                    nStrResLen = nCount;
                    if (strRes[nStrResLen - 1] == '\r' && strRes[nStrResLen - 2] == '*')
                    {
                        strTotalRes = strRes.ToString(); // finish receive接收完成
                        break;
                    }
                    else
                    {
                        continue;
                    }
                }
            }
            catch (Exception e)
            {
                return false;
            }
            // process the result 
            string strValue;
            int nOffset = 0;
            if (pTask.m_nReadOrWrite == 0 /*Read*/)
            {
                // handle error //FIXME
                if (pTask.m_PLCArea == MemAreaHostLink.WR)
                    nOffset = 23;
                else
                    nOffset = 7;
                if (strTotalRes.Length < (nOffset + 4 * pTask.m_unWordsCount + 4))
                {
                    _lastError = "Response error, not enogh response, cannot generate result.";
                    return false;
                }
                int i = 0;
                for (i = 0; i < (int)pTask.m_unWordsCount; i++)
                {
                    strValue = strTotalRes.Substring(nOffset + i * 4, 4);
                    string a;
                    a = strValue.Trim();
                    short wValue = 0;
                    if (!string.IsNullOrEmpty(a))
                    {
                        short.TryParse(strValue, out wValue);
                    }
                    UpdateReadWord(pTask.m_PLCArea, (uint)(pTask.m_unBeginWord + i), wValue);
                }
            }
            else
            { /*Write*/  //写地址
                // handle error //FIXME
                // just neglect
            }
            return true;
        }

        // Read PLC word in nonoverlapped way
        public bool ReadPLCBit(MemAreaHostLink area, uint unBeginWord, int bit)
        {
            short result = 0;
            result = GetReadWord(area, unBeginWord);
            if (Convert.ToBoolean((result >> bit) & 0x1))//把其它bit设为0,再把当前位与0X1与计算，判断是否为0,或不为0
                return true;
            else
                return false;
        }

        public bool ReadPLCBitWait(MemAreaHostLink area, uint unBeginWord, int bit, int unMilliseconds = 1000)
        {
            OmronStructHostLink task = new OmronStructHostLink();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = unBeginWord;
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

        public short ReadPLCShortInt(MemAreaHostLink area, uint unBeginWord)
        {
            return GetReadWord(area, unBeginWord);
        }

        public short ReadPLCShortIntWait(MemAreaHostLink area, uint unBeginWord, int unMilliseconds = 1000)
        {
            OmronStructHostLink task = new OmronStructHostLink();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = unBeginWord;
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

        public int ReadPLCDDInt(MemAreaHostLink area, uint unBeginWord)
        {
            return (int)(GetReadWord(area, unBeginWord + 1) * 256 * 256 + GetReadWord(area, unBeginWord));
        }

        public int ReadPLCDDIntWait(MemAreaHostLink area, uint unBeginWord, int unMilliseconds = 1000)
        {
            OmronStructHostLink task = new OmronStructHostLink();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = unBeginWord;
            task.m_unWordsCount = 2;
            task.m_nWordOrDWord = 0;
            task.GenStrCmd();
            AddImmeTask(task);
            task.m_hEventFinish.WaitOne(unMilliseconds);
            task.m_hEventFinish.Close();
            // Now we add an immediate task, when the task is processed the read value 
            // is save in the read area, which is the same to the period task.
            // We just get it, regardless if the period task has update value...
            return (int)(GetReadWord(area, unBeginWord + 1) * 256 * 256 + GetReadWord(area, unBeginWord));
        }

        public float ReadPLCFloat(MemAreaHostLink area, uint unBeginWord)
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

        public float ReadPLCFloatWait(MemAreaHostLink area, uint unBeginWord, int unMilliseconds = 1000)
        {
            OmronStructHostLink task = new OmronStructHostLink();
            task.m_nReadOrWrite = 0;
            task.m_hEventFinish.Reset();
            task.m_PLCArea = area;
            task.m_unBeginWord = unBeginWord;
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

        public string ReadPLCString(MemAreaHostLink area, uint unBeginWord, short unStringCount)
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
    }
}
