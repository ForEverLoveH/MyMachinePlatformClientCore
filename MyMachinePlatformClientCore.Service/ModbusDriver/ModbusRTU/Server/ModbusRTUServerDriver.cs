using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Service.ModbusDriver.ModbusRTU.Server
{
    /// <summary>
    /// 
    /// </summary>
    public enum MemAreaModbusRTUServer
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
    public enum AreaReadModbusRTUServerTpye
    {
        ReadCoil = (int)MemAreaModbusRTUServer.ReadCoil,
        ReadDiscreteInputs = (int)MemAreaModbusRTUServer.ReadDiscreteInputs,
        ReadHoldingRegister = (int)MemAreaModbusRTUServer.ReadHoldingRegister,
        ReadInputRegister = (int)MemAreaModbusRTUServer.ReadInputRegister
    }
    public enum AreaWriteModbusRTUDTpye
    {
        WriteSingleCoil = (int)MemAreaModbusRTUServer.WriteSingleCoil,
        WriteMultipleCoils = (int)MemAreaModbusRTUServer.WriteMultipleCoils,
        WriteSingleRegister = (int)MemAreaModbusRTUServer.WriteSingleRegister,
        WriteMultipleRegister = (int)MemAreaModbusRTUServer.WriteMultipleRegister
    }
    /// <summary>
    /// 
    /// </summary>
    public class ModbusRTUServerDriver
    {
        [DllImport("kernel32", CharSet = CharSet.Auto)]
        private static extern uint GetTickCount();
        private object m_critSecPeriodList = new object();
        private object m_critSecImmeList = new object();
        private object m_critUpdateReadList = new object();
        private static System.Threading.Thread m_pSocketThreadProc;
        private SerialPort _serialPort;
        private string _lastError = "";
        private string _port;
        private int _timeOut;
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
        private byte[] exceptionFrame;
        public string LastError
        {
            get { return _lastError; }
        }
        /// <summary>
        /// Open and init the serial port
        /// </summary>
        /// <param name="timeOut"></param>
        /// <param name="port"></param>
        /// <param name="baudRate"></param>
        /// <returns></returns>
        public bool OpenComm(int timeOut = -1, string port = "COM1", string baudRate = "9600")
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
            }
            catch (Exception error)
            {

                return false;
            }
            //初始化句柄
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
            while (true)
            {
                Thread.Sleep(100);
                byte[] data;
                if (!isModBusException(out data))
                {
                    SendGetRes(data);
                }
            }
        }

        private short GetReadWord(AreaReadModbusRTUServerTpye area, uint unWordAddress, out bool isExist)
        {
            short wUpdate = 0;
            lock (m_critUpdateReadList)// CriticalSect
            {
                if (m_mapReadPLCMem[(int)area].ContainsKey(unWordAddress) == true)
                {
                    wUpdate = m_mapReadPLCMem[(int)area][unWordAddress];
                    isExist = true;
                }
                else
                {
                    isExist = false;
                }
            }
            return wUpdate;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="area"></param>
        /// <param name="unWordAddress"></param>
        /// <param name="wUpdate"></param>
        /// <returns></returns>
        private bool UpdateReadWord(AreaReadModbusRTUServerTpye area, UInt32 unWordAddress, short wUpdate)
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
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rcvData"></param>
        /// <returns></returns>
        private bool isModBusException(out byte[] rcvData)
        {
            byte[] crc16;
            int dataLength = 0;
            int DataNum = 0;//读写数据个数
            int bitValue = 0;//写单个线圈或寄存器值
            int frameLength = 8;//命令帧最小长度
            bool isIllegalData = false;
            string strRcvData;
            int count = 0;
            byte[] Data = new byte[100];
            while (count < 8)
            {
                count += _serialPort.Read(Data, count, 8 - count);
            }
            dataLength = count;
            if (Data[1] == 0x0F || Data[1] == 0x10)//写多线圈  或多个寄存器时：
            {
                int count1 = 0;
                DataNum = Data[6];
                dataLength = 9 + DataNum;
                while (count1 < dataLength - 8)
                {
                    count1 += _serialPort.Read(Data, count1 + 8, dataLength - 8 - count1);
                }
            }
            rcvData = new byte[dataLength];
            Buffer.BlockCopy(Data, 0, rcvData, 0, dataLength);
            if (Data[1] == 0x05 || Data[1] == 0x06)//写单个线圈  或单个寄存器时：
            {
                if (Data[1] == 0x05)//写单个线圈时判断值是否为0xFF 或0x00 
                {
                    bitValue = Data[4];
                    if (bitValue != 0xFF && bitValue != 0x00)
                    {
                        _lastError = "isModBusException() write singel Coil or Register illegal Data Value!:" + bitValue.ToString();
                        isIllegalData = true;
                    }
                    else
                    {
                        isIllegalData = false;//写单个寄存器：无处理
                    }
                }
            }
            else //其他读写时数据长度为0 返回  illegal Data Value
            {
                DataNum = Data[4] * 256 + Data[5];
                if (DataNum == 0)
                {
                    _lastError = "isModBusException() read/write Data length is 0, illegal Data Value! ";
                    isIllegalData = true;
                }
                else
                {
                    isIllegalData = false;
                }
            }
            //Exception Code: 03 illegal Data Value

            if (isIllegalData)
            {
                exceptionFrame = new byte[6];
                Buffer.BlockCopy(Data, 0, exceptionFrame, 0, 2);
                exceptionFrame[2] = (byte)(Data[1] | 0x80); //异常返回功能码高位为1 
                exceptionFrame[3] = 0x03;                //
                crc16 = CRCStuffServer.calculateCRC(ref exceptionFrame, 4);
                exceptionFrame[4] = crc16[0];// Number of data to read寄存器数量高字节
                exceptionFrame[5] = crc16[1];// Number of data to read寄存器数量低字节  
                _serialPort.Write(exceptionFrame, 0, exceptionFrame.Length);
                return true;
            }
            return false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        private void SendGetRes(byte[] data)
        {
            //1、收到命令帧后 判断功能码 读 写 
            //2、读：读数据的格式，地址，数量
            //3、从MAP里面找到地址的数据
            //4、返回读到的信息 功能码+字节数+数据

            //2、写：读取数据的格式 写入的起始地址，（写入数量），数值
            //3、在MAP里面更新数据
            byte[] writeData;
            byte[] crc16;
            int funcCode = data[1];//modbus 功能码
            uint startAddress = 0;
            int dataNum = 0;
            int updateData = 0;
            int byteMun = 0;
            int bitDataNum = 0;
            byte[] sendData;//发送的数据帧
            byte[] sendFrame;
            bool isAddrExsit = false; //判断读取的地址是否在Map中存在
            short temp = 0;
            short temp1 = 0;
            switch (funcCode)
            {
                case 0x01://读线圈状态00001-09999 位操作(单个或多个)****功能码01H
                    startAddress = (uint)data[2] * 256 + data[3];   //起始地址
                    bitDataNum = data[4] * 256 + data[5];            //位数据长度
                    dataNum = bitDataNum % 8 == 0 ? dataNum = bitDataNum / 8 : dataNum = bitDataNum / 8 + 1;
                    sendData = new byte[dataNum];                 //发送数据帧
                    sendFrame = new byte[5 + dataNum];         //组成发送帧 =数据头（功能码+字节数）+数据帧
                    Buffer.BlockCopy(data, 0, sendFrame, 0, 2);
                    sendFrame[2] = Convert.ToByte((dataNum) & 0XFF);//寄存器数量
                    for (uint i = 0; i < dataNum; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            temp1 = GetReadWord(AreaReadModbusRTUServerTpye.ReadCoil, (UInt32)(startAddress + i * 8 + j), out isAddrExsit);
                            if (!isAddrExsit)
                            {
                                _lastError = "SendGetRes() ReadCoil address [" + (startAddress + i * 8 + j).ToString() + "] illegal Data Address! ";
                                //异常返回帧构建：
                                exceptionFrame = new byte[6];
                                Buffer.BlockCopy(data, 0, exceptionFrame, 0, 2);
                                exceptionFrame[2] = (byte)(data[1] | 0x80); //异常返回功能码高位为1 
                                exceptionFrame[3] = 0x02;                //
                                crc16 = CRCStuffServer.calculateCRC(ref exceptionFrame, 4);
                                exceptionFrame[4] = crc16[0];// Number of data to read寄存器数量高字节
                                exceptionFrame[5] = crc16[1];// Number of data to read寄存器数量低字节  
                                _serialPort.Write(exceptionFrame, 0, exceptionFrame.Length);
                                return;
                            }
                            temp += (short)(temp1 << j);
                        }
                        sendData[i] = Convert.ToByte(temp & 0XFF);
                        sendFrame[3 + i] = sendData[i];
                    }
                    crc16 = CRCStuffServer.calculateCRC(ref sendFrame, 3 + dataNum);
                    sendFrame[3 + dataNum] = crc16[0];// Number of data to read寄存器数量高字节
                    sendFrame[3 + dataNum + 1] = crc16[1];// Number of data to read寄存器数量低字节  
                    //发送帧
                    _serialPort.Write(sendFrame, 0, sendFrame.Length);
                    break;
                case 0x02://读离散输入状态10001-19999位操作(单个或多个)****功能码02H
                    startAddress = (uint)data[2] * 256 + data[3];   //起始地址
                    bitDataNum = data[4] * 256 + data[5];            //位数据长度
                    dataNum = bitDataNum % 8 == 0 ? dataNum = bitDataNum / 8 : dataNum = bitDataNum / 8 + 1;
                    sendData = new byte[dataNum];                 //发送数据帧
                    sendFrame = new byte[5 + dataNum];         //组成发送帧 =数据头（功能码+字节数）+数据帧
                    Buffer.BlockCopy(data, 0, sendFrame, 0, 2);
                    sendFrame[2] = Convert.ToByte((dataNum) & 0XFF);//寄存器数量
                    for (uint i = 0; i < dataNum; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            temp1 = GetReadWord(AreaReadModbusRTUServerTpye.ReadDiscreteInputs, (UInt32)(startAddress + i * 8 + j), out isAddrExsit);
                            if (!isAddrExsit)
                            {
                                _lastError = "SendGetRes() ReadDiscreteInputs address [" + (startAddress + i * 8 + j).ToString() + "] illegal Data Address! ";
                                //异常返回帧构建：
                                exceptionFrame = new byte[6];
                                Buffer.BlockCopy(data, 0, exceptionFrame, 0, 2);
                                exceptionFrame[2] = (byte)(data[1] | 0x80); //异常返回功能码高位为1 
                                exceptionFrame[3] = 0x02;                //
                                crc16 = CRCStuffServer.calculateCRC(ref exceptionFrame, 4);
                                exceptionFrame[4] = crc16[0];// Number of data to read寄存器数量高字节
                                exceptionFrame[5] = crc16[1];// Number of data to read寄存器数量低字节  
                                _serialPort.Write(exceptionFrame, 0, exceptionFrame.Length);
                                return;
                            }
                            temp += (short)(temp1 << j);
                        }
                        sendData[i] = Convert.ToByte(temp & 0XFF);
                        sendFrame[3 + i] = sendData[i];
                    }
                    crc16 = CRCStuffServer.calculateCRC(ref sendFrame, 3 + dataNum);
                    sendFrame[3 + dataNum] = crc16[0];// Number of data to read寄存器数量高字节
                    sendFrame[3 + dataNum + 1] = crc16[1];// Number of data to read寄存器数量低字节  
                    //发送帧
                    _serialPort.Write(sendFrame, 0, sendFrame.Length);
                    break;
                case 0x03://读保持寄存器40001-49999字操作(单个或多个)****功能码03H
                    startAddress = (uint)data[2] * 256 + data[3];   //起始地址
                    dataNum = data[4] * 256 + data[5];            //位数据长度
                    //dataNum = bitDataNum % 8 == 0 ? dataNum = bitDataNum / 8 : dataNum = bitDataNum / 8 + 1;
                    sendData = new byte[dataNum * 2];                 //发送数据帧
                    sendFrame = new byte[5 + dataNum * 2];         //组成发送帧 =数据头（功能码+字节数）+数据帧
                    Buffer.BlockCopy(data, 0, sendFrame, 0, 2);
                    sendFrame[2] = Convert.ToByte((dataNum * 2) & 0XFF);//寄存器数量
                    for (uint i = 0; i < dataNum; i++)
                    {
                        temp = GetReadWord(AreaReadModbusRTUServerTpye.ReadHoldingRegister, startAddress + i, out isAddrExsit);
                        if (!isAddrExsit)
                        {
                            _lastError = "SendGetRes() ReadHoldingRegister address [" + (startAddress + i).ToString() + "] is illegal Data Address! ";
                            //异常返回帧构建：
                            exceptionFrame = new byte[6];
                            Buffer.BlockCopy(data, 0, exceptionFrame, 0, 2);
                            exceptionFrame[2] = (byte)(data[1] | 0x80); //异常返回功能码高位为1 
                            exceptionFrame[3] = 0x02;                //                              
                            crc16 = CRCStuffServer.calculateCRC(ref exceptionFrame, 4);
                            exceptionFrame[4] = crc16[0];// Number of data to read寄存器数量高字节
                            exceptionFrame[5] = crc16[1];// Number of data to read寄存器数量低字节  
                            _serialPort.Write(exceptionFrame, 0, exceptionFrame.Length);
                            return;
                        }
                        sendData[2 * i + 1] = Convert.ToByte(temp & 0XFF);
                        sendData[2 * i] = Convert.ToByte((temp >> 8) & 0XFF);
                        sendFrame[3 + 2 * i] = sendData[2 * i];
                        sendFrame[3 + 2 * i + 1] = sendData[2 * i + 1];
                    }
                    crc16 = CRCStuffServer.calculateCRC(ref sendFrame, 3 + dataNum * 2);
                    sendFrame[3 + 2 * dataNum] = crc16[0];// Number of data to read寄存器数量高字节
                    sendFrame[3 + 2 * dataNum + 1] = crc16[1];// Number of data to read寄存器数量低字节  
                    //发送帧
                    _serialPort.Write(sendFrame, 0, sendFrame.Length);
                    break;
                case 0x04://读输入寄存器30001-39999字操作(单个或多个)****功能码04H
                    startAddress = (uint)data[2] * 256 + data[3];   //起始地址
                    dataNum = data[4] * 256 + data[5];            //位数据长度
                    //dataNum = bitDataNum % 8 == 0 ? dataNum = bitDataNum / 8 : dataNum = bitDataNum / 8 + 1;
                    sendData = new byte[dataNum * 2];                 //发送数据帧
                    sendFrame = new byte[5 + dataNum * 2];         //组成发送帧 =数据头（功能码+字节数）+数据帧
                    Buffer.BlockCopy(data, 0, sendFrame, 0, 2);
                    sendFrame[2] = Convert.ToByte((dataNum * 2) & 0XFF);//寄存器数量
                    for (uint i = 0; i < dataNum; i++)
                    {
                        temp = GetReadWord(AreaReadModbusRTUServerTpye.ReadInputRegister, startAddress + i, out isAddrExsit);
                        if (!isAddrExsit)
                        {
                            _lastError = "SendGetRes() ReadinPuttingRegister address [" + (startAddress + i).ToString() + "] is illegal Data Address! ";
                            //异常返回帧构建：
                            exceptionFrame = new byte[6];
                            Buffer.BlockCopy(data, 0, exceptionFrame, 0, 2);
                            exceptionFrame[2] = (byte)(data[1] | 0x80); //异常返回功能码高位为1 
                            exceptionFrame[3] = 0x02;                //                              
                            crc16 = CRCStuffServer.calculateCRC(ref exceptionFrame, 4);
                            exceptionFrame[4] = crc16[0];// Number of data to read寄存器数量高字节
                            exceptionFrame[5] = crc16[1];// Number of data to read寄存器数量低字节  
                            _serialPort.Write(exceptionFrame, 0, exceptionFrame.Length);
                            return;
                        }
                        sendData[2 * i + 1] = Convert.ToByte(temp & 0XFF);
                        sendData[2 * i] = Convert.ToByte((temp >> 8) & 0XFF);
                        sendFrame[3 + 2 * i] = sendData[2 * i];
                        sendFrame[3 + 2 * i + 1] = sendData[2 * i + 1];
                    }
                    crc16 = CRCStuffServer.calculateCRC(ref sendFrame, 3 + dataNum * 2);
                    sendFrame[3 + 2 * dataNum] = crc16[0];// Number of data to read寄存器数量高字节
                    sendFrame[3 + 2 * dataNum + 1] = crc16[1];// Number of data to read寄存器数量低字节  
                    //发送帧
                    _serialPort.Write(sendFrame, 0, sendFrame.Length);
                    break;
                case 0x05://写单个线圈00001-09999位操作(单个)****功能码05H
                    startAddress = (uint)data[2] * 256 + data[3];   //起始地址
                    updateData = data[4] == 0xFF ? 1 : 0;
                    UpdateReadWord(AreaReadModbusRTUServerTpye.ReadCoil, startAddress, (short)updateData);
                    _serialPort.Write(data, 0, data.Length);
                    break;
                case 0x06://写单个保持寄存器40001-49999字操作(单个)****功能码06H
                    startAddress = (uint)data[2] * 256 + data[3];   //起始地址
                    updateData = data[4] * 256 + data[5];             //数据
                    UpdateReadWord(AreaReadModbusRTUServerTpye.ReadHoldingRegister, startAddress, (short)updateData);
                    _serialPort.Write(data, 0, data.Length);
                    break;
                case 0x0F://写多个线圈00001-09999位操作(多个)****功能码0FH   
                    sendFrame = new byte[8];
                    startAddress = (uint)data[2] * 256 + data[3];   //起始地址
                    dataNum = data[4] * 256 + data[5];            //寄存器数量
                    byteMun = data[6];//字节数
                    for (int i = 0; i < byteMun; i++)
                    {
                        updateData = data[7 + i];
                        for (int j = 0; j < 8; j++)
                        {
                            int res = 0;
                            res = (updateData >> j) & 0x01;
                            UpdateReadWord(AreaReadModbusRTUServerTpye.ReadCoil, (UInt32)(startAddress + i * 8 + j), (short)temp);
                            if (i * 8 + j == dataNum)
                            {
                                break;
                            }
                        }
                    }
                    Buffer.BlockCopy(data, 0, sendFrame, 0, 6);
                    crc16 = CRCStuffServer.calculateCRC(ref sendFrame, 6);
                    sendFrame[6] = crc16[0];// Number of data to read寄存器数量高字节
                    sendFrame[7] = crc16[1];// Number of data to read寄存器数量低字节 
                    _serialPort.Write(sendFrame, 0, sendFrame.Length);
                    break;
                case 0x10://写多个保持寄存器40001-49999字操作(多个)****功能码10H 
                    sendFrame = new byte[8];
                    startAddress = (uint)data[2] * 256 + data[3];   //起始地址
                    dataNum = data[4] * 256 + data[5];            //寄存器数量
                    byteMun = data[6];//字节数
                    for (uint i = 0; i < dataNum; i++)
                    {
                        updateData = data[7 + i * 2] * 256 + data[7 + i * 2 + 1];
                        UpdateReadWord(AreaReadModbusRTUServerTpye.ReadHoldingRegister, startAddress + i, (short)updateData);
                    }
                    Buffer.BlockCopy(data, 0, sendFrame, 0, 6);
                    crc16 = CRCStuffServer.calculateCRC(ref sendFrame, 6);
                    sendFrame[6] = crc16[0];// Number of data to read寄存器数量高字节
                    sendFrame[7] = crc16[1];// Number of data to read寄存器数量低字节  
                    _serialPort.Write(sendFrame, 0, sendFrame.Length);
                    break;
                    break;
                default:
                    _lastError = "SendGetRes() illegal Function Code:" + data[7].ToString();
                    //异常返回帧构建：
                    exceptionFrame = new byte[6];
                    Buffer.BlockCopy(data, 0, exceptionFrame, 0, 2);
                    exceptionFrame[2] = (byte)(data[1] | 0x80); //异常返回功能码高位为1 
                    exceptionFrame[3] = 0x02;                //                               
                    crc16 = CRCStuffServer.calculateCRC(ref exceptionFrame, 4);
                    exceptionFrame[4] = crc16[0];// Number of data to read寄存器数量高字节
                    exceptionFrame[5] = crc16[1];// Number of data to read寄存器数量低字节  
                    _serialPort.Write(exceptionFrame, 0, exceptionFrame.Length);
                    break;
            }
        }
    }
}
