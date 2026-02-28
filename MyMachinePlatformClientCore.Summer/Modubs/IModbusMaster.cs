using MyMachinePlatformClientCore.Log.MyLogs;
using MyMachinePlatformClientCore.Summer.Common;
using MyMachinePlatformClientCore.Summer.Options;
using Twilio.Base;

namespace MyMachinePlatformClientCore.Summer
{
    

    public interface IModbusMaster : IDevice, IAutomatic
       
    {
        event Action<LogMessage>? LogMessageDataCallBack;

        Task WriteData(FunctionCode functionCode, byte slaveAddress, ushort startAddress, bool[]? coilsBuffer = null,
            ushort[]? registerBuffer = null);

        Task<(bool[]?, ushort[]?)> ReadData(FunctionCode functionCode, byte slaveAddress, ushort startAddress,
            ushort numberOfPoints);
    }
}