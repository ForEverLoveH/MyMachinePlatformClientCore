using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Service.ModbusDriver.ModbusServer.Server
{
    public enum MemAreaModbusTCPServer
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

    public enum AreaReadModbusTCPServerTpye
    {
        ReadCoil = (int)MemAreaModbusTCPServer.ReadCoil,
        ReadDiscreteInputs = (int)MemAreaModbusTCPServer.ReadDiscreteInputs,
        ReadHoldingRegister = (int)MemAreaModbusTCPServer.ReadHoldingRegister,
        ReadInputRegister = (int)MemAreaModbusTCPServer.ReadInputRegister
    }
    public enum AreaWriteModbusTCPserverTpye
    {
        WriteSingleCoil = (int)MemAreaModbusTCPServer.WriteSingleCoil,
        WriteMultipleCoils = (int)MemAreaModbusTCPServer.WriteMultipleCoils,
        WriteSingleRegister = (int)MemAreaModbusTCPServer.WriteSingleRegister,
        WriteMultipleRegister = (int)MemAreaModbusTCPServer.WriteMultipleRegister
    }
    public enum DataTypeTCPServer : byte
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

    public enum AreaModbusTCPServer : int
    {
        fctReadCoil = 1,
        fctReadDiscreteInputs = 2,
        fctReadHoldingRegister = 3,
        fctReadInputRegister = 4,
        fctWriteSingleCoil = 5,
        fctWriteSingleRegister = 6,
        fctWriteMultipleCoils = 15,
        fctWriteMultipleRegister = 16,
        fctReadWriteMultipleRegister = 23
    }
}
