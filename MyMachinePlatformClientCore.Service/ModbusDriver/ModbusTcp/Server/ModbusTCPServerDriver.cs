using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MyMachinePlatformClientCore.Service.ModbusDriver.ModbusServer.Server;

namespace MyMachinePlatformClientCore.Service.ModbusDriver.ModbusServer
{
    public class ModbusTCPServerDriver
    {
        [DllImport("kernel32", CharSet = CharSet.Auto)]
        private static extern uint GetTickCount();
        private object m_critSecPeriodList = new object();
        private object m_critSecImmeList = new object();
        private object m_critUpdateReadList = new object();
        private static System.Threading.Thread m_pSocketThreadProc;
        private static System.Threading.Thread m_pSocketConnectThreadProc;

        private string _lastError = "";

        private string _port;
        #region tcp

        private Ping _ping = null;
        private IPEndPoint _endPoint = null;
        internal Socket ModbusTcpServerSocket = null;

        #endregion

        public static Dictionary<uint, short>[] m_mapReadPLCMem = new Dictionary<uint, short>[6]
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

        private bool initConnect = false;
        private int backlog;
        /// <summary>
        /// returns the connection status
        /// </summary>
        public bool Connected
        {
            get { return (ModbusTcpServerSocket == null) ? false : ModbusTcpServerSocket.Connected; }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public ModbusTCPServerDriver()
        {
            // used to ping the PLC
            //
            this._ping = new Ping();

            // EndPoint parametres
            //
            this._endPoint = new IPEndPoint(0, 0);
        }

        public void SetTCPParams(IPAddress ip, int port)
        {
            this._endPoint.Address = ip;
            this._endPoint.Port = port;
        }

        private byte[] exceptionFrame;

        public class SocketInfo
        {
            public Socket socket = null;
            public byte[] buffer = null;
            public byte[] msgBuffer = null;
            public SocketInfo()
            {
                buffer = new byte[1024 * 4];
            }
        }

        private bool TCPConnect()
        {
            try
            {
                if (ModbusTcpServerSocket != null)
                {
                    ModbusTcpServerSocket.Dispose();
                }
                backlog = 0;
                ModbusTcpServerSocket = new Socket(_endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                ModbusTcpServerSocket.Bind(_endPoint);
                ModbusTcpServerSocket.Listen(backlog);
                //接下来是开通线程
                m_pSocketConnectThreadProc = new System.Threading.Thread(SocketConnectThreadProc);
                if (m_pSocketConnectThreadProc == null)
                {
                    this.initConnect = false;
                    return false;
                }
                m_pSocketConnectThreadProc.Start();
                this.initConnect = true;
                return true;
            }
            catch (System.Exception ex)//SocketException error
            {
                _lastError = "listenSocket exception:" + ex.Message;
                this.initConnect = false;
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
                initConnect = false;
                Close(ModbusTcpServerSocket);
                this._lastError = ex.Message;
                return false;
            }
        }

        public void Close(object obj)
        {
            Socket sock = obj as Socket;

            if (sock == null) return;
            if (sock.Connected == true)
            {
                sock.Shutdown(SocketShutdown.Both);
                sock.Disconnect(false);
                sock.Close();
            }
            sock.Dispose();
            sock = null;
        }

        private void SocketConnectThreadProc()
        {
            int recv;
            while (true)
            {
                Socket client = ModbusTcpServerSocket.Accept();
                SocketInfo clientInfo = new SocketInfo();
                clientInfo.socket = client;
                EndPoint ep = client.RemoteEndPoint;
                _lastError = "客户端：" + ep.ToString() + "上线";
                while (true)
                {
                    try
                    {
                        recv = client.ReceiveFrom(clientInfo.buffer, ref ep);
                        if (recv == 0)
                        {
                            clientInfo.socket.Shutdown(SocketShutdown.Both);
                            clientInfo.socket.Close();
                            _lastError = "客户端：" + ep.ToString() + "下线";
                            break;
                        }
                        else
                        {
                            if (recv < clientInfo.buffer.Length)
                            {
                                byte[] newBuffer = new byte[recv];
                                Buffer.BlockCopy(clientInfo.buffer, 0, newBuffer, 0, recv);
                                clientInfo.msgBuffer = newBuffer;
                            }
                            else
                            {
                                clientInfo.msgBuffer = clientInfo.buffer;
                            }
                            if (!isModBusException(clientInfo))
                            {
                                SendGetRes(clientInfo);
                            }
                        }
                        Thread.Sleep(100);
                    }
                    catch (Exception ex)
                    {
                        if (clientInfo.socket.Connected)
                        {
                            clientInfo.socket.Shutdown(SocketShutdown.Both);
                            clientInfo.socket.Close();
                        }
                        _lastError = "ReceiveMsg() Exception:" + ex.Message + "客户端：" + ep.ToString() + " 异常下线";
                        break; ;
                    }
                }
                Thread.Sleep(100);
            }
        }

        public void SendMsg(byte[] frame, SocketInfo info)
        {
            try
            {
                info.socket.Send(frame);
            }
            catch (System.Exception ex)
            {
                _lastError = "SendMsg():endPoint" + info.socket.RemoteEndPoint.ToString() + "Exception:" + ex.Message;
            }
        }

        private bool isModBusException(SocketInfo info)
        {
            int DataNum = 0;//读写数据个数
            int bitValue = 0;//写单个线圈或寄存器值
            int frameLength = 12;//命令帧最小长度
            bool isIllegalData = false;
            if (info.msgBuffer.Length < frameLength)
            {
                _lastError = string.Format("SendGetRes()端口[{0}]命令长度:{1}小于12", info.socket.RemoteEndPoint.ToString(), info.msgBuffer.Length.ToString());
                return true;
            }
            //Exception Code: 03 illegal Data Value
            if (info.msgBuffer[7] == 0x05 || info.msgBuffer[7] == 0x06)//写单个线圈  或单个寄存器时：
            {
                if (info.msgBuffer[7] == 0x05)//写单个线圈时判断值是否为0xFF 或0x00 
                {
                    bitValue = info.msgBuffer[10];
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
                DataNum = info.msgBuffer[10] * 256 + info.msgBuffer[11];
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
            if (isIllegalData)
            {
                exceptionFrame = new byte[9];
                Buffer.BlockCopy(info.msgBuffer, 0, exceptionFrame, 0, 4);
                exceptionFrame[4] = 0x00;                   //后面数据长度高八位
                exceptionFrame[5] = 0x03;                   //后面数据长度底八位
                exceptionFrame[6] = info.msgBuffer[6];                //站号
                exceptionFrame[7] = (byte)(info.msgBuffer[7] | 0x80); //异常返回功能码高位为1
                exceptionFrame[8] = 0x03;                   //错误代码：illegal Data Value
                SendMsg(exceptionFrame, info);
                return true;
            }
            return false;
        }

        private void SendGetRes(SocketInfo info)
        {
            //1、收到命令帧后 判断功能码 读 写 
            //2、读：读数据的格式，地址，数量
            //3、从MAP里面找到地址的数据
            //4、返回读到的信息 功能码+字节数+数据

            //2、写：读取数据的格式 写入的起始地址，（写入数量），数值
            //3、在MAP里面更新数据

            byte[] data = new byte[info.msgBuffer.Length];
            byte[] writeData;
            Buffer.BlockCopy(info.msgBuffer, 0, data, 0, data.Length);
            int funcCode = data[7];//modbus 功能码
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
                    startAddress = (uint)data[8] * 256 + data[9];   //起始地址
                    bitDataNum = data[10] * 256 + data[11];            //位数据长度
                    dataNum = bitDataNum % 8 == 0 ? dataNum = bitDataNum / 8 : dataNum = bitDataNum / 8 + 1;
                    sendData = new byte[dataNum];                 //发送数据帧
                    sendFrame = new byte[9 + dataNum];         //组成发送帧 =数据头（功能码+字节数）+数据帧
                    Buffer.BlockCopy(data, 0, sendFrame, 0, 4);
                    sendFrame[4] = Convert.ToByte((dataNum + 3) >> 8);  //后面数据长度高八位
                    sendFrame[5] = Convert.ToByte((dataNum + 3) & 0XFF);//后面数据长度底八位
                    sendFrame[6] = data[6];                            //站号
                    sendFrame[7] = data[7];                                 //功能码
                    sendFrame[8] = Convert.ToByte((dataNum) & 0XFF);//寄存器数量
                    for (uint i = 0; i < dataNum; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            temp1 = GetReadWord(AreaReadModbusTCPServerTpye.ReadCoil, (UInt32)(startAddress + i * 8 + j), out isAddrExsit);
                            if (!isAddrExsit)
                            {
                                _lastError = "SendGetRes() ReadCoil address [" + (startAddress + i * 8 + j).ToString() + "] illegal Data Address! ";
                                //异常返回帧构建：
                                exceptionFrame = new byte[9];
                                Buffer.BlockCopy(data, 0, exceptionFrame, 0, 4);
                                exceptionFrame[4] = 0x00;                   //后面数据长度高八位
                                exceptionFrame[5] = 0x03;                   //后面数据长度底八位
                                exceptionFrame[6] = data[6];                //站号
                                exceptionFrame[7] = (byte)(data[7] | 0x80); //异常返回功能码高位为1
                                exceptionFrame[8] = 0x02;                   //错误代码：illegal Data Address
                                SendMsg(exceptionFrame, info);
                                return;
                            }
                            temp += (short)(temp1 << j);
                        }
                        sendData[i] = Convert.ToByte(temp & 0XFF);
                        sendFrame[9 + i] = sendData[i];
                    }
                    //发送帧
                    SendMsg(sendFrame, info);
                    break;
                case 0x02://读离散输入状态10001-19999位操作(单个或多个)****功能码02H
                    startAddress = (uint)data[8] * 256 + data[9];   //起始地址
                    bitDataNum = data[10] * 256 + data[11];            //位数据长度
                    dataNum = bitDataNum % 8 == 0 ? dataNum = bitDataNum / 8 : dataNum = bitDataNum / 8 + 1;
                    sendData = new byte[dataNum];                 //发送数据帧
                    sendFrame = new byte[9 + dataNum];         //组成发送帧 =数据头（功能码+字节数）+数据帧
                    Buffer.BlockCopy(data, 0, sendFrame, 0, 4);
                    sendFrame[4] = Convert.ToByte((dataNum + 3) >> 8);  //后面数据长度高八位
                    sendFrame[5] = Convert.ToByte((dataNum + 3) & 0XFF);//后面数据长度底八位
                    sendFrame[6] = data[6];                            //站号
                    sendFrame[7] = data[7];                                 //功能码
                    sendFrame[8] = Convert.ToByte((dataNum) & 0XFF);//寄存器数量
                    for (uint i = 0; i < dataNum; i++)
                    {
                        for (int j = 0; j < 8; j++)
                        {
                            temp1 = GetReadWord(AreaReadModbusTCPServerTpye.ReadDiscreteInputs, (UInt32)(startAddress + i * 8 + j), out isAddrExsit);
                            if (!isAddrExsit)
                            {
                                _lastError = "SendGetRes() ReadDiscreteInputs address [" + (startAddress + i * 8 + j).ToString() + "] is illegal Data Address! ";
                                //异常返回帧构建：
                                exceptionFrame = new byte[9];
                                Buffer.BlockCopy(data, 0, exceptionFrame, 0, 4);
                                exceptionFrame[4] = 0x00;                   //后面数据长度高八位
                                exceptionFrame[5] = 0x03;                   //后面数据长度底八位
                                exceptionFrame[6] = data[6];                //站号
                                exceptionFrame[7] = (byte)(data[7] | 0x80); //异常返回功能码高位为1
                                exceptionFrame[8] = 0x02;                   //错误代码：illegal Data Address
                                SendMsg(exceptionFrame, info);
                                return;
                            }
                            temp += (short)(temp1 << j);
                        }
                        sendData[i] = Convert.ToByte(temp & 0XFF);
                        sendFrame[9 + i] = sendData[i];
                    }
                    //发送帧
                    SendMsg(sendFrame, info);
                    break;
                case 0x03://读保持寄存器40001-49999字操作(单个或多个)****功能码03H
                    startAddress = (uint)data[8] * 256 + data[9];   //起始地址
                    dataNum = data[10] * 256 + data[11];            //数据长度
                    sendData = new byte[dataNum * 2];                 //发送数据帧
                    sendFrame = new byte[9 + dataNum * 2];         //组成发送帧 =数据头（功能码+字节数）+数据帧
                    Buffer.BlockCopy(data, 0, sendFrame, 0, 4);
                    sendFrame[4] = Convert.ToByte((dataNum * 2 + 3) >> 8);  //后面数据长度高八位
                    sendFrame[5] = Convert.ToByte((dataNum * 2 + 3) & 0XFF);//后面数据长度底八位
                    sendFrame[6] = data[6];                            //站号
                    sendFrame[7] = data[7];                                 //功能码
                    sendFrame[8] = Convert.ToByte((dataNum * 2) & 0XFF);//寄存器数量

                    for (uint i = 0; i < dataNum; i++)
                    {
                        temp = GetReadWord(AreaReadModbusTCPServerTpye.ReadHoldingRegister, startAddress + i, out isAddrExsit);
                        if (!isAddrExsit)
                        {
                            _lastError = "SendGetRes() ReadHoldingRegister address [" + (startAddress + i).ToString() + "] is illegal Data Address! ";
                            //异常返回帧构建：
                            exceptionFrame = new byte[9];
                            Buffer.BlockCopy(data, 0, exceptionFrame, 0, 4);
                            exceptionFrame[4] = 0x00;                   //后面数据长度高八位
                            exceptionFrame[5] = 0x03;                   //后面数据长度底八位
                            exceptionFrame[6] = data[6];                //站号
                            exceptionFrame[7] = (byte)(data[7] | 0x80); //异常返回功能码高位为1
                            exceptionFrame[8] = 0x02;                   //错误代码：illegal Data Address
                            SendMsg(exceptionFrame, info);
                            return;
                        }
                        sendData[2 * i + 1] = Convert.ToByte(temp & 0XFF);
                        sendData[2 * i] = Convert.ToByte((temp >> 8) & 0XFF);
                        sendFrame[9 + 2 * i] = sendData[2 * i];
                        sendFrame[9 + 2 * i + 1] = sendData[2 * i + 1];
                    }
                    //发送帧
                    SendMsg(sendFrame, info);
                    break;
                case 0x04://读输入寄存器30001-39999字操作(单个或多个)****功能码04H
                    startAddress = (uint)data[8] * 256 + data[9];   //起始地址
                    dataNum = data[10] * 256 + data[11];            //数据长度
                    sendData = new byte[dataNum * 2];                 //发送数据帧
                    sendFrame = new byte[9 + dataNum * 2];         //组成发送帧 =数据头（功能码+字节数）+数据帧
                    Buffer.BlockCopy(data, 0, sendFrame, 0, 4);
                    sendFrame[4] = Convert.ToByte((dataNum * 2 + 3) >> 8);  //后面数据长度高八位
                    sendFrame[5] = Convert.ToByte((dataNum * 2 + 3) & 0XFF);//后面数据长度底八位
                    sendFrame[6] = data[6];                            //站号
                    sendFrame[7] = data[7];                                 //功能码
                    sendFrame[8] = Convert.ToByte((dataNum * 2) & 0XFF);//寄存器数量
                    for (uint i = 0; i < dataNum; i++)
                    {
                        temp = GetReadWord(AreaReadModbusTCPServerTpye.ReadInputRegister, startAddress + i, out isAddrExsit);
                        if (!isAddrExsit)
                        {
                            _lastError = "SendGetRes() ReadInputRegister address [" + (startAddress + i).ToString() + "] is illegal Data Address! ";
                            //异常返回帧构建：
                            exceptionFrame = new byte[9];
                            Buffer.BlockCopy(data, 0, exceptionFrame, 0, 4);
                            exceptionFrame[4] = 0x00;                   //后面数据长度高八位
                            exceptionFrame[5] = 0x03;                   //后面数据长度底八位
                            exceptionFrame[6] = data[6];                //站号
                            exceptionFrame[7] = (byte)(data[7] | 0x80); //异常返回功能码高位为1
                            exceptionFrame[8] = 0x02;                   //错误代码：illegal Data Address
                            SendMsg(exceptionFrame, info);
                            return;
                        }
                        sendData[2 * i + 1] = Convert.ToByte(temp & 0XFF);
                        sendData[2 * i] = Convert.ToByte((temp >> 8) & 0XFF);
                        sendFrame[9 + 2 * i] = sendData[2 * i];
                        sendFrame[9 + 2 * i + 1] = sendData[2 * i + 1];
                    }
                    //发送帧
                    SendMsg(sendFrame, info);
                    break;
                case 0x05://写单个线圈00001-09999位操作(单个)****功能码05H
                    startAddress = (uint)data[8] * 256 + data[9];   //起始地址
                    updateData = data[10] == 0xFF ? 1 : 0;
                    UpdateReadWord(AreaReadModbusTCPServerTpye.ReadCoil, startAddress, (short)updateData);
                    SendMsg(data, info);
                    break;
                   
                case 0x06://写单个保持寄存器40001-49999字操作(单个)****功能码06H
                    startAddress = (uint)data[8] * 256 + data[9];   //起始地址
                    updateData = data[10] * 256 + data[11];             //数据
                    UpdateReadWord(AreaReadModbusTCPServerTpye.ReadHoldingRegister, startAddress, (short)updateData);
                    SendMsg(data, info);
                    break;
                case 0x0F://写多个线圈00001-09999位操作(多个)****功能码0FH   
                    sendFrame = new byte[12];
                    startAddress = (uint)data[8] * 256 + data[9];   //起始地址
                    dataNum = data[10] * 256 + data[11];            //寄存器数量
                    byteMun = data[12];//字节数
                    for (int i = 0; i < byteMun; i++)
                    {
                        updateData = data[13 + i];
                        for (int j = 0; j < 8; j++)
                        {
                            int res = 0;
                            res = (updateData >> j) & 0x01;
                            UpdateReadWord(AreaReadModbusTCPServerTpye.ReadCoil, (UInt32)(startAddress + i * 8 + j), (short)temp);
                            if (i * 8 + j == dataNum)
                            {
                                break;
                            }
                        }
                    }
                    Buffer.BlockCopy(data, 0, sendFrame, 0, 12);
                    sendFrame[5] = 0x06;
                    SendMsg(sendFrame, info);
                    break;
                case 0x10://写多个保持寄存器40001-49999字操作(多个)****功能码10H 
                    sendFrame = new byte[12];
                    startAddress = (uint)data[8] * 256 + data[9];   //起始地址
                    dataNum = data[10] * 256 + data[11];            //寄存器数量
                    byteMun = data[12];//字节数
                    for (uint i = 0; i < dataNum; i++)
                    {
                        updateData = data[13 + i * 2] * 256 + data[13 + i * 2 + 1];
                        UpdateReadWord(AreaReadModbusTCPServerTpye.ReadHoldingRegister, startAddress + i, (short)updateData);
                    }
                    Buffer.BlockCopy(data, 0, sendFrame, 0, 12);
                    sendFrame[5] = 0x06;
                    SendMsg(sendFrame, info);
                    break;
                    break;
                default:
                    _lastError = "SendGetRes() illegal Function Code:" + data[7].ToString();
                    //异常返回帧构建：
                    exceptionFrame = new byte[9];
                    Buffer.BlockCopy(data, 0, exceptionFrame, 0, 4);
                    exceptionFrame[4] = 0x00;                   //后面数据长度高八位
                    exceptionFrame[5] = 0x03;                   //后面数据长度底八位
                    exceptionFrame[6] = data[6];                //站号
                    exceptionFrame[7] = (byte)(data[7] | 0x80); //异常返回功能码高位为1
                    exceptionFrame[8] = 0x01;                   //错误代码：0x01 illegal function
                    SendMsg(exceptionFrame, info);
                    break;
            }
        }

        private short GetReadWord(AreaReadModbusTCPServerTpye area, uint unWordAddress, out bool isExist)
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
        // PLC memory map operate
        private bool UpdateReadWord(AreaReadModbusTCPServerTpye area, UInt32 unWordAddress, short wUpdate)
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
    }

}
