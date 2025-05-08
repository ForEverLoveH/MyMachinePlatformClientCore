using MyMachinePlatformClientCore.Service.ModbusService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyMachinePlatformClientCore.Log.MyLogs;

namespace MyMachinePlatformClientCore.Service.Managers
{
    /// <summary>
    /// 
    /// </summary>
    public class CMyModbusServiceManagers
    {
        private ModbusRTUService _ModbusRTUServer;
        /// <summary>
        /// 
        /// </summary>
        private ModbusTcpService _ModbusTcpServer;
        /// <summary>
        /// 
        /// </summary>
        private Action<LogMessage> _logDataCallBack;

        public CMyModbusServiceManagers(Action<LogMessage> logDataCallBack= null)
        {
            this._logDataCallBack = logDataCallBack;
        }
        /// <summary>
        /// 链接类型 0 表示rtu 链接 1 表示 tcp链接
        /// </summary>
        private int type = -1;

        #region TCP
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        public void StartMyModbusTcpService(string ipAddress, int port)
        {
            type = 1;
            _ModbusTcpServer = new ModbusTcpService(ipAddress, port,_logDataCallBack);
            _ModbusTcpServer.StartService();
        }
        /// <summary>
        /// 
        /// </summary>
        private void StopMyModbusTcpService() => _ModbusTcpServer.StopService();
        #endregion
        #region RTU
        /// <summary>
        /// 
        /// </summary>
        /// <param name="PortName"></param>
        /// <param name="BaudRate"></param>
        /// <param name="DataBits"></param>
        /// <param name="StopBits"></param>
        /// <param name="Parity"></param>
        /// <param name="WriteTimeout"></param>
        /// <param name="ReadTimeout"></param>
        public void StartMyModbusRTUService(string PortName, int BaudRate, int DataBits,
          System.IO.Ports.StopBits StopBits, System.IO.Ports.Parity Parity, int WriteTimeout = 200, int ReadTimeout = 200)
        {
            _ModbusRTUServer = new ModbusRTUService(_logDataCallBack);
            type = 0;
            _ModbusRTUServer.Connection(PortName, BaudRate, DataBits, StopBits, Parity, WriteTimeout, ReadTimeout);
        }
        /// <summary>
        /// 
        /// </summary>
        private void StopMyModbusRTUService() => _ModbusRTUServer?.StopService();

        #endregion
        /// <summary>
        /// 
        /// </summary>
        public bool IsConnection => type == 0 ? _ModbusRTUServer.IsConnection : _ModbusTcpServer.IsConnection;
        /// <summary>
        /// 写入
        /// </summary>
        /// <param name="functionCode">功能码类型</param>
        /// <param name="slaveAddress">站地址</param>
        /// <param name="startAddress">位置,从0开始</param>
        /// <param name="coilsBuffer">线圈数据</param>
        /// <param name="registerBuffer">寄存器数据</param> 
        public async Task WriteData(FunctionCode functionCode, byte slaveAddress, ushort startAddress, bool[]? coilsBuffer = null, ushort[]? registerBuffer = null)
        {
            if (type == 0)await _ModbusRTUServer?.WriteData(functionCode, slaveAddress, startAddress, coilsBuffer, registerBuffer);
            else await _ModbusTcpServer?.WriteData(functionCode, slaveAddress, startAddress, coilsBuffer, registerBuffer);
        }
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="functionCode">功能码类型</param>
        /// <param name="slaveAddress">站地址</param>
        /// <param name="startAddress">位置,从0开始</param>
        /// <param name="numberOfPoints">长度</param>
        public async Task<(bool[]?, ushort[]?)> ReadData(FunctionCode functionCode, byte slaveAddress, ushort startAddress, ushort numberOfPoints)
        {
            if (type == 0) return await _ModbusRTUServer.ReadData(functionCode, slaveAddress, startAddress, numberOfPoints);
            else return await _ModbusTcpServer.ReadData(functionCode, slaveAddress, startAddress, numberOfPoints);
        }
        /// <summary>
        /// 
        /// </summary>
        public void StopService() 
        {
            if (type == 0) _ModbusRTUServer?.StopService();
            else _ModbusTcpServer?.StopService();
        }
    }
}