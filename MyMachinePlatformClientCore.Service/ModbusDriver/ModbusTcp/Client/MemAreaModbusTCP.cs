using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Service.ModbusDriver.ModbusServer
{
    public enum MemAreaModbusTCP { ReadCoil, ReadDiscreteInputs, ReadHoldingRegister, ReadInputRegister, WriteSingleCoil, WriteMultipleCoils, WriteSingleRegister, WriteMultipleRegister }

    public enum AreaReadModbusTCPTpye
    {
        ReadCoil = (int)MemAreaModbusTCP.ReadCoil,
        ReadDiscreteInputs = (int)MemAreaModbusTCP.ReadDiscreteInputs,
        ReadHoldingRegister = (int)MemAreaModbusTCP.ReadHoldingRegister,
        ReadInputRegister = (int)MemAreaModbusTCP.ReadInputRegister
    }
    public enum AreaWriteModbusRTUDTCPTpye
    {
        WriteSingleCoil = (int)MemAreaModbusTCP.WriteSingleCoil,
        WriteMultipleCoils = (int)MemAreaModbusTCP.WriteMultipleCoils,
        WriteSingleRegister = (int)MemAreaModbusTCP.WriteSingleRegister,
        WriteMultipleRegister = (int)MemAreaModbusTCP.WriteMultipleRegister
    }
    public enum DataTypeTCP : byte
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
    public enum ModBusExceptionCode    //错误代码
    {
        IllegalFunction = 01,
        IllegalDataAddress,
        IllegalDataValue,
        SlaveDeviceFailure,
        Acknowledge,
        SlaveDeviceBusy,
        GatewayPathUnavailable,
        GatewayTargetDeviceFailed2Respond,
    }
}
