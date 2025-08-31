using MyMachinePlatformClientCore.Log.MyLogs;

namespace MyMachinePlatformClientCore.IService.CHslCommunication;

public interface IPLCService
{
    bool IsConnect { get; }
    Action<LogMessage> LogMessageCallBack { get; }
    Task<bool> StartService();
    Task<bool> StopService();

    #region  读取

    

    
    Task<byte[]> ReadByte(string address, ushort length);
    Task<short> ReadInt16(string address);
    Task<ushort> ReadUInt16(string address);
    Task<int> ReadInt32(string address);
    Task<uint> ReadUInt32(string address);
    Task<string> ReadString(string address, ushort length);
    Task<float> ReadFloat(string address);
    Task<double> ReadDouble(string address);
    Task<ulong> ReadUInt64(string address);
    Task<long> ReadInt64(string address);
    

    #endregion

    #region 写入

    Task<bool> Write(string address, ushort value);
    Task<bool> Write(string address, int value);
    Task<bool> Write(string address, short value);
    Task<bool> Write(string address, short[] value);
    Task<bool> Write(string address, float value);
    Task<bool> Write(string address, double value);
    Task<bool> Write(string address, uint value);
    Task<bool> Write(string address, byte[] value);
    Task<bool> Write(string address, ulong value);

    #endregion
}