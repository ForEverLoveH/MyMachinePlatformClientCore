using System.IO.Ports;
using MyMachinePlatformClientCore.Log.MyLogs;
using MyMachinePlatformClientCore.Summer.Common;
using MyMachinePlatformClientCore.Summer.Options;
using NModbus;

namespace MyMachinePlatformClientCore.Summer;


    public class ModbusRTUMasterOption : SerialPortOption
    {
        public Parity Parity { get; set; }
        public StopBits StopBits { get; set; }
        public int WriteTimeout { get; set; }
        public int ReadTimeout { get; set; }
    }

     
    /// <summary>
    /// 
    /// </summary>
    public class ModbusRTUMaster  : Automatic, IModbusMaster,IHasOption<ModbusRTUMasterOption>
    {
        private SerialPort SPort;

        /// <summary>
        /// 
        /// </summary>
        private IModbusSerialMaster rtuMaster;

        private bool isConnected;

        /// <summary>
        /// 
        /// </summary>
        public bool IsConnected
        {
            get => isConnected;
        }

        /// <summary>
        /// 
        /// </summary>

        public event Action<IDevice>? IsConnectedChanged;

        /// <summary>
        /// 
        /// </summary>

        private ModbusRTUMasterOption option;

        /// <summary>
        /// 
        /// </summary>
        public ModbusRTUMasterOption Option
        {
            get => option;
            set => option = value;

        }

        /// <summary>
        /// 
        /// </summary>
        public event Action<LogMessage>? LogMessageDataCallBack;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="option"></param>
        public ModbusRTUMaster(ModbusRTUMasterOption option)
        {
            this.option = option;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Connect()
        {
            try
            {
                SPort = new SerialPort();
                SPort.PortName = this.Option.PortName;
                SPort.BaudRate = this.Option.BaudRate;
                SPort.DataBits = this.Option.DataBits;
                SPort.Parity = this.Option.Parity;
                SPort.StopBits = this.Option.StopBits;
                SPort.WriteTimeout = this.Option.WriteTimeout;
                SPort.ReadTimeout = this.Option.ReadTimeout;
                SPort.Open();
               
                var factory = new ModbusFactory();
                rtuMaster = factory.CreateRtuMaster((NModbus.IO.IStreamResource)SPort);
                isConnected = true;
                LogMessageDataCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO, $"串口{this.Option.PortName}连接成功"));
                IsConnectedChanged?.Invoke(this);
            }
            catch (Exception ex)
            {
                isConnected = false;
                LogMessageDataCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                    $"串口{this.Option.PortName}连接失败,异常信息为{ex.Message}"));

            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Disconnect()
        {
            if (SPort == null)  return;
            SPort.Close();
            if (rtuMaster is null) return;
            rtuMaster.Dispose();
            isConnected= false;
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
                LogMessageDataCallBack ?.Invoke(LogMessage.SetMessage(LogType.ERROR, $"往串口{Option.PortName }写入数据失败,异常信息为{e.Message}"));
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
                LogMessageDataCallBack ?.Invoke(LogMessage.SetMessage(LogType.ERROR, $"从串口{Option.PortName}读取数据失败,异常信息为{e.Message}"));
            }
            return (coilsBuffer, registerBuffer);
        }
    


    
    }