using MyMachinePlatformClientCore.Log.MyLogs;
using MyMachinePlatformClientCore.Summer.Common;
using MyMachinePlatformClientCore.Summer.Options;
using NModbus;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer
{
    public class ModubsTcpMasterOption :TcpOption
    {

    }
     
    /// <summary>
    /// 
    /// </summary>
    public class ModubsTcpMaster : Automatic, Summer. IModbusMaster,IHasOption<ModubsTcpMasterOption>
    {

        /// <summary>
        /// 
        /// </summary>
        private System.Net.Sockets.TcpClient _tcpClient;
        /// <summary>
        /// 
        /// </summary>
        private  NModbus. IModbusMaster tcpMaster;
        /// <summary>
        /// 
        /// </summary>

        private bool isConnected;
        /// <summary>
        /// 
        /// </summary>
        public bool IsConnected
        {
            get=> isConnected;
        }
        /// <summary>
        /// 
        /// </summary>
        private ModubsTcpMasterOption option;

        public ModubsTcpMasterOption Option { get =>option; set => option = value; }
        /// <summary>
        /// 
        /// </summary>
        public event Action<LogMessage> LogMessageDataCallBack;

        /// <summary>
        /// 
        /// </summary>
        public event Action<IDevice> IsConnectedChanged;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="option"></param>
        public ModubsTcpMaster(ModubsTcpMasterOption option)
        {
            this.option = option;
        }
        /// <summary>
        /// 
        /// </summary>
        public virtual void Connect()
        {
            _tcpClient = new System.Net.Sockets.TcpClient();
            _tcpClient.Connect(IPAddress.Parse(Option.IpAddress),Option.Port );
            var factory = new ModbusFactory();
            tcpMaster = factory.CreateMaster(_tcpClient);
            isConnected = true;
            string message = $"Modbus Tcp 服务连接成功，IP地址：{Option.IpAddress}，端口号：{Option.Port}";
            LogMessageDataCallBack?.Invoke(LogMessage.SetMessage(  LogType.INFO, message));
            IsConnectedChanged?.Invoke(this);
        }
        /// <summary>
        /// 
        /// </summary>

        public void Disconnect()
        {
            try
            {
                if (tcpMaster != null) tcpMaster.Dispose();
                if (_tcpClient != null)
                {
                    _tcpClient.Close();
                    _tcpClient.Dispose();
                }
                isConnected = false;
            }
            catch (Exception ex)
            {
                isConnected = false;
                return;
            }
            IsConnectedChanged?.Invoke(this);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="functionCode"></param>
        /// <param name="slaveAddress"></param>
        /// <param name="startAddress"></param>
        /// <param name="coilsBuffer"></param>
        /// <param name="registerBuffer"></param>
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
               LogMessageDataCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,"Modbus Tcp 服务写入数据失败，异常信息为：" + e.Message));
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
                LogMessageDataCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,"Modbus Tcp 服务读取数据失败，异常信息为：" + e.Message)); 
            }
            return (coilsBuffer, registerBuffer);
        }
    }
}
