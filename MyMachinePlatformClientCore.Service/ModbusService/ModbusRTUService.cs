using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports; 
using NModbus;
using MyMachinePlatformClientCore.Log.MyLogs;


namespace MyMachinePlatformClientCore.Service.ModbusService
{
    /// <summary>
    /// 
    /// </summary>
    public class ModbusRTUService
    {

        /// <summary>
        /// 
        /// </summary>
        public bool IsConnection = false;
        /// <summary>
        /// 串口操作对象
        /// </summary>
        private SerialPort? SPort;
        /// <summary>
        /// 
        /// </summary>
        private IModbusSerialMaster rtuMaster;

        /// <summary>
        ///  RTU连接
        /// </summary>
        /// <param name="PortName"></param>
        /// <param name="BaudRate">波特率</param>
        /// <param name="DataBits">数据位</param>
        /// <param name="StopBits">停止位</param>
        /// <param name="Parity">校验位</param>
        /// <param name="WriteTimeout"></param>
        /// <param name="ReadTimeout"></param>
        /// <returns></returns>
        public virtual bool Connection(string PortName, int BaudRate, int DataBits,
          System.IO.Ports.StopBits StopBits, System.IO.Ports.Parity Parity, int WriteTimeout = 200, int ReadTimeout = 200)
        {
            try
            {
                SPort = new SerialPort();
                SPort.PortName = PortName;
                SPort.BaudRate = BaudRate;
                SPort.DataBits = DataBits;
                SPort.Parity = Parity;
                SPort.StopBits = StopBits;
                SPort.WriteTimeout = WriteTimeout;
                SPort.ReadTimeout = ReadTimeout;
                SPort.Open();
                //SPort.DataReceived += SPort_DataReceived;
                var factory = new ModbusFactory();
                rtuMaster = factory.CreateRtuMaster((NModbus.IO.IStreamResource)SPort);
                IsConnection = true;
                return true;
            }
            catch (Exception ex)
            {
                IsConnection = false;
                return false;
            }
        }
        /// <summary>
        /// 关闭
        /// </summary>
        public virtual void StopService()
        {
            if (SPort == null)
                return;
            SPort.Close();
            IsConnection = false;
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
                if (SPort == null)
                    return;
                if (SPort.IsOpen == false)
                {
                    SPort.Open();
                }
                switch (functionCode)
                {
                    case FunctionCode.WriteSingleCoilAsync:
                        if (coilsBuffer == null)
                            return;
                        await rtuMaster.WriteSingleCoilAsync(slaveAddress, startAddress, coilsBuffer[0]);
                        break;
                    case FunctionCode.WriteSingleRegisterAsync:
                        if (registerBuffer == null)
                            return;
                        await rtuMaster.WriteSingleRegisterAsync(slaveAddress, startAddress, registerBuffer[0]);
                        break;
                    case FunctionCode.WriteMultipleCoilsAsync:
                        if (coilsBuffer == null)
                            return;
                        await rtuMaster.WriteMultipleCoilsAsync(slaveAddress, startAddress, coilsBuffer);
                        break;
                    case FunctionCode.WriteMultipleRegistersAsync:
                        if (registerBuffer == null)
                            return;
                        await rtuMaster.WriteMultipleRegistersAsync(slaveAddress, startAddress, registerBuffer);
                        break;
                     
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                MyLogTool.ColorLog(MyLogColor.Red,"modbus服务写入异常，信息为："+e.Message+"\n");
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
        public virtual  async Task<(bool[]?, ushort[]?)> ReadData(FunctionCode functionCode, byte slaveAddress, ushort startAddress, ushort numberOfPoints)
        {
            bool[]? coilsBuffer = null;//线圈数据
            ushort[]? registerBuffer = null;//寄存器数据
            try
            {
                if (rtuMaster == null)
                    return (coilsBuffer, registerBuffer);
                if (SPort == null)
                    return (coilsBuffer, registerBuffer);
                if (SPort.IsOpen == false)
                {
                    SPort.Open();
                }
                switch (functionCode)
                {
                    case FunctionCode.ReadCoils:
                        coilsBuffer = await rtuMaster.ReadCoilsAsync(slaveAddress, startAddress, numberOfPoints);
                        break;
                    case FunctionCode.ReadHoldingRegisters:
                        registerBuffer = await rtuMaster.ReadHoldingRegistersAsync(slaveAddress, startAddress, numberOfPoints);
                        break;
                    case FunctionCode.ReadInputs:
                        coilsBuffer = await rtuMaster.ReadInputsAsync(slaveAddress, startAddress, numberOfPoints);
                        break;
                    case FunctionCode.ReadInputRegisters:
                        registerBuffer = await rtuMaster.ReadInputRegistersAsync(slaveAddress, startAddress, numberOfPoints);
                        break;
                    default:
                        break;
                }
                return (coilsBuffer, registerBuffer);
            }
            catch (Exception e)
            {
                MyLogTool.ColorLog(MyLogColor.Cyan,"ModbusRTUService.ReadData", e);
            }
            return (coilsBuffer, registerBuffer);
        }
    }
}
