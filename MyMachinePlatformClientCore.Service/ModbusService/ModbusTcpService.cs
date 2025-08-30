using MyMachinePlatformClientCore.Common.TcpService.Client;
using MyMachinePlatformClientCore.Log.MyLogs;
using MyMachinePlatformClientCore.Service.LogService;
using NModbus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Service.ModbusService
{
    public class ModbusTcpService
    {
        /// <summary>
        /// 
        /// </summary>
        private System.Net.Sockets.TcpClient _tcpClient;
        /// <summary>
        /// 
        /// </summary>
        private IModbusMaster tcpMaster;
        /// <summary>
        /// 
        /// </summary>
        private string serverIP;
        /// <summary>
        /// //
        /// </summary>
        private int port;
        /// <summary>
        /// 
        /// </summary>
        private bool isConnection = false;
        /// <summary>
        /// 
        /// </summary>
        public bool IsConnection
        { get => isConnection; private set => isConnection = value; }
        /// <summary>
        /// 
        /// </summary>
        private Action<LogMessage> _logDataCallBack;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <param name="port"></param>
        public ModbusTcpService(string ipaddress, int port,Action<LogMessage> logDataCallBack = null)
        {
            this.serverIP = ipaddress;
            this.port = port;
            this._logDataCallBack = logDataCallBack;
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void StartService()
        {
            try
            {
                _tcpClient = new System.Net.Sockets.TcpClient();
                _tcpClient.Connect(IPAddress.Parse(serverIP), port);
                var factory = new ModbusFactory();
                tcpMaster = factory.CreateMaster(_tcpClient);
                IsConnection = true;
                _logDataCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO, $"Modbus Tcp 服务连接成功，IP地址：{serverIP}，端口号：{port}"));

            }
            catch (Exception ex)
            {
                IsConnection = false;
                return;
            }

        }
        /// <summary>
        /// 
        /// </summary>
        public void StopService()
        {
            try
            {
                if (tcpMaster != null) tcpMaster.Dispose();

                if (_tcpClient != null)
                {
                    _tcpClient.Close();
                    _tcpClient.Dispose();
                }
                IsConnection = false;
            }
            catch (Exception ex)
            {
                IsConnection = false;
                return;
            }
        }
        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="functionCode">功能码类型</param>
        /// <param name="slaveAddress">站地址</param>
        /// <param name="startAddress">位置,从0开始</param>
        /// <param name="coilsBuffer">线圈数据</param>
        /// <param name="registerBuffer">寄存器数据</param> 
        public virtual async Task WriteData(FunctionCode functionCode, byte slaveAddress, ushort startAddress, bool[]? coilsBuffer = null, ushort[]? registerBuffer = null)
        {
            try
            {
                if (_tcpClient == null)
                    return;
                if (tcpMaster == null)
                    return;
                switch (functionCode)
                {
                    case FunctionCode.WriteSingleCoilAsync:
                        if (coilsBuffer == null)
                            return;
                        await tcpMaster.WriteSingleCoilAsync(slaveAddress, startAddress, coilsBuffer[0]);
                        break;
                    case FunctionCode.WriteSingleRegisterAsync:
                        if (registerBuffer == null)
                            return;
                        await tcpMaster.WriteSingleRegisterAsync(slaveAddress, startAddress, registerBuffer[0]);
                        break;
                    case FunctionCode.WriteMultipleCoilsAsync:
                        if (coilsBuffer == null)
                            return;
                        await tcpMaster.WriteMultipleCoilsAsync(slaveAddress, startAddress, coilsBuffer);
                        break;
                    case FunctionCode.WriteMultipleRegistersAsync:
                        if (registerBuffer == null)
                            return;
                        await tcpMaster.WriteMultipleRegistersAsync(slaveAddress, startAddress, registerBuffer);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
               _logDataCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,"Modbus Tcp 服务写入数据失败，异常信息为：" + e.Message));
               return;

            }
        }
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="functionCode">功能码类型</param>
        /// <param name="slaveAddress">站地址</param>
        /// <param name="startAddress">位置,从0开始</param>
        /// <param name="numberOfPoints">长度</param>
        public virtual async  Task<(bool[]?, ushort[]?)> ReadData(FunctionCode functionCode, byte slaveAddress, ushort startAddress, ushort numberOfPoints)
        {
            bool[]? coilsBuffer = null;//线圈数据
            ushort[]? registerBuffer = null;//寄存器数据
            try
            {
                if (tcpMaster == null)
                    return (coilsBuffer, registerBuffer);
                switch (functionCode)
                {
                    case FunctionCode.ReadCoils:
                        coilsBuffer = await tcpMaster.ReadCoilsAsync(slaveAddress, startAddress, numberOfPoints);
                        break;
                    case FunctionCode.ReadHoldingRegisters:
                        registerBuffer = await tcpMaster.ReadHoldingRegistersAsync(slaveAddress, startAddress, numberOfPoints);
                        break;
                    case FunctionCode.ReadInputs:
                        coilsBuffer = await tcpMaster.ReadInputsAsync(slaveAddress, startAddress, numberOfPoints);
                        break;
                    case FunctionCode.ReadInputRegisters:
                        registerBuffer = await tcpMaster.ReadInputRegistersAsync(slaveAddress, startAddress, numberOfPoints);
                        break;
                    default:
                        break;
                }
                return (coilsBuffer, registerBuffer);
            }
            catch (Exception e)
            {
                _logDataCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,"Modbus Tcp 服务读取数据失败，异常信息为：" + e.Message)); 
            }
            return (coilsBuffer, registerBuffer);
        }
    }
}
