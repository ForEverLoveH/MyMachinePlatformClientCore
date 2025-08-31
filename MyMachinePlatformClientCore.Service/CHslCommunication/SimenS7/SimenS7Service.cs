using HslCommunication;
using HslCommunication.Profinet.Siemens;
using System;
using System.Threading.Tasks;
using MyMachinePlatformClientCore.IService.CHslCommunication;
using MyMachinePlatformClientCore.Log.MyLogs;

/// <summary>
/// 西门子S7系列PLC服务类
/// </summary>
public class SimenS7Service : IPLCService, IDisposable
{
    /// <summary>
    /// 连接状态
    /// </summary>
    private bool isConnect = false;

    /// <summary>
    /// 是否已连接
    /// </summary>
    public bool IsConnect => isConnect;

    /// <summary>
    /// 日志回调函数
    /// </summary>
    public Action<LogMessage> LogMessageCallBack { get; }

    /// <summary>
    /// 西门子S7通信对象
    /// </summary>
    private SiemensS7Net SiemensS7Net;

    private int timeOut;
    private SiemensPLCS PlcType;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="ipaddress">PLC的IP地址</param>
    /// <param name="timeOut">超时时间(毫秒)</param>
    /// <param name="plcType">PLC类型</param>
    /// <param name="logMessageCallBack">日志回调函数</param>
    public SimenS7Service(string ipaddress, int timeOut, SiemensPLCS plcType,
        Action<LogMessage> logMessageCallBack = null)
    {
        this.LogMessageCallBack = logMessageCallBack;
        this.PlcType = plcType;
        SiemensS7Net = new SiemensS7Net(this.PlcType, ipaddress);
        this.timeOut = timeOut;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<bool> StartService()
    {
        try
        {
            if (IsConnect)
            {
                return true;
            }

            var result = await SiemensS7Net.ConnectServerAsync();
            if (result.IsSuccess)
            {
                isConnect = true;
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                    "链接西门子PLC成功,IP地址:" + SiemensS7Net.IpAddress + "PLC的类型为:" + PlcType));
                return true;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                    "链接西门子PLC失败,IP地址:" + SiemensS7Net.IpAddress + "PLC的类型为:" + PlcType));
                return false;
            }
        }
        catch (Exception e)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "链接西门子PLC出现异常,异常信息为:" + e.Message));
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<bool> StopService()
    {
        if (IsConnect)
        {
            var res = await SiemensS7Net.ConnectCloseAsync();
            if (res.IsSuccess)
            {


                isConnect = false;
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                    "断开西门子PLC连接,IP地址:" + SiemensS7Net.IpAddress + "PLC的类型为:" + PlcType));
                return true;
            }
            else
            {
                LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                    "断开西门子PLC连接失败,IP地址:" + SiemensS7Net.IpAddress + "PLC的类型为:" + PlcType));
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public async Task<byte[]> ReadByte(string address, ushort length)
    {
        var res = await SiemensS7Net.ReadAsync(address, length);
        if (res.IsSuccess)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                "读取西门子PLC字节数组成功,地址:" + address + "长度:" + length + "数据:" + BitConverter.ToString(res.Content)));
            return res.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "读取西门子PLC字节数组失败,地址:" + address + "长度:" + length + "异常信息:" + res.Message));
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public async Task<short> ReadInt16(string address)
    {
        var res = await SiemensS7Net.ReadInt16Async(address);
        if (res.IsSuccess)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                "读取西门子PLCInt16成功,地址:" + address + "数据:" + res.Content));
            return res.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "读取西门子PLCInt16失败,地址:" + address + "异常信息:" + res.Message));
            return 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public async Task<ushort> ReadUInt16(string address)
    {
        var res = await SiemensS7Net.ReadUInt16Async(address);
        if (res.IsSuccess)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                "读取西门子PLCUInt16成功,地址:" + address + "数据:" + res.Content));
            return res.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "读取西门子PLCUInt16失败,地址:" + address + "异常信息:" + res.Message));
            return 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public async Task<int> ReadInt32(string address)
    {
        var res = await SiemensS7Net.ReadInt32Async(address);
        if (res.IsSuccess)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                "读取西门子PLCInt32成功,地址:" + address + "数据:" + res.Content));
            return res.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "读取西门子PLCInt32失败,地址:" + address + "异常信息:" + res.Message));
            return 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public async Task<uint> ReadUInt32(string address)
    {
        var res = await SiemensS7Net.ReadUInt32Async(address);
        if (res.IsSuccess)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                "读取西门子PLCUInt32成功,地址:" + address + "数据:" + res.Content));
            return res.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "读取西门子PLCUInt32失败,地址:" + address + "异常信息:" + res.Message));
            return 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public async Task<string> ReadString(string address, ushort length)
    {
        var res = await SiemensS7Net.ReadStringAsync(address, length);
        if (res.IsSuccess)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                "读取西门子PLC字符串成功,地址:" + address + "长度:" + length + "数据:" + res.Content));
            return res.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "读取西门子PLC字符串失败,地址:" + address + "长度:" + length + "异常信息:" + res.Message));
            return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public async Task<float> ReadFloat(string address)
    {
        var res = await SiemensS7Net.ReadFloatAsync(address);
        if (res.IsSuccess)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                "读取西门子PLCFloat成功,地址:" + address + "数据:" + res.Content));
            return res.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "读取西门子PLCFloat失败,地址:" + address + "异常信息:" + res.Message));
            return 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public async Task<double> ReadDouble(string address)
    {
        var res = await SiemensS7Net.ReadDoubleAsync(address);
        if (res.IsSuccess)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                "读取西门子PLCDouble成功,地址:" + address + "数据:" + res.Content));
            return res.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "读取西门子PLCDouble失败,地址:" + address + "异常信息:" + res.Message));
            return 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public async Task<ulong> ReadUInt64(string address)
    {
        var res = await SiemensS7Net.ReadUInt64Async(address);
        if (res.IsSuccess)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                "读取西门子PLCUInt64成功,地址:" + address + "数据:" + res.Content));
            return res.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "读取西门子PLCUInt64失败,地址:" + address + "异常信息:" + res.Message));
            return 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public async Task<long> ReadInt64(string address)
    {
        var res = await SiemensS7Net.ReadInt64Async(address);
        if (res.IsSuccess)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                "读取西门子PLCInt64成功,地址:" + address + "数据:" + res.Content));
            return res.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "读取西门子PLCInt64失败,地址:" + address + "异常信息:" + res.Message));
            return 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<bool> Write(string address, ushort value)
    {
        var res = await SiemensS7Net.WriteAsync(address, value);
        if (res.IsSuccess)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                "写入西门子PLCUInt16成功,地址:" + address + "数据:" + value));
            return true;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "写入西门子PLCUInt16失败,地址:" + address + "异常信息:" + res.Message));
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<bool> Write(string address, int value)
    {
        var res = await SiemensS7Net.WriteAsync(address, value);
        if (res.IsSuccess)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                "写入西门子PLCInt32成功,地址:" + address + "数据:" + value));
            return true;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "写入西门子PLCInt32失败,地址:" + address + "异常信息:" + res.Message));
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<bool> Write(string address, short value)
    {
        var res = await SiemensS7Net.WriteAsync(address, value);
        if (res.IsSuccess)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                "写入西门子PLCInt16成功,地址:" + address + "数据:" + value));
            return true;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "写入西门子PLCInt16失败,地址:" + address + "异常信息:" + res.Message));
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<bool> Write(string address, short[] value)
    {
        var res = await SiemensS7Net.WriteAsync(address, value);
        if (res.IsSuccess)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                "写入西门子PLCInt16数组成功,地址:" + address + "数据:" + value));
            return true;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "写入西门子PLCInt16数组失败,地址:" + address + "异常信息:" + res.Message));
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<bool> Write(string address, float value)
    {
        var res = await SiemensS7Net.WriteAsync(address, value);
        if (res.IsSuccess)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                "写入西门子PLCFloat成功,地址:" + address + "数据:" + value));
            return true;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "写入西门子PLCFloat失败,地址:" + address + "异常信息:" + res.Message));
            return false;
        }
    }

    /// <summary>
    /// 写入西门子PLCDouble
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<bool> Write(string address, double value)
    {
        var res = await SiemensS7Net.WriteAsync(address, value);
        if (res.IsSuccess)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                "写入西门子PLCDouble成功,地址:" + address + "数据:" + value));
            return true;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "写入西门子PLCDouble失败,地址:" + address + "异常信息:" + res.Message));
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<bool> Write(string address, uint value)
    {
        var res = await SiemensS7Net.WriteAsync(address, value);
        if (res.IsSuccess)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                "写入西门子PLCUInt32成功,地址:" + address + "数据:" + value));
            return true;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "写入西门子PLCUInt32失败,地址:" + address + "异常信息:" + res.Message));
            return false;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<bool> Write(string address, byte[] value)
    {
        var res = SiemensS7Net.Write(address, value);
        if (res.IsSuccess)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                "写入西门子PLCByte数组成功,地址:" + address + "数据:" + value));
            return true;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "写入西门子PLCByte数组失败,地址:" + address + "异常信息:" + res.Message));
            return false;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<bool> Write(string address, ulong value)
    {
        var res = await SiemensS7Net.WriteAsync(address, value);
        if (res.IsSuccess)
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,
                "写入西门子PLCUInt64成功,地址:" + address + "数据:" + value));
            return true;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,
                "写入西门子PLCUInt64失败,地址:" + address + "异常信息:" + res.Message));
            return false;
        }
    }

    public void Dispose()
    {
        StopService();
    }
}

     
        
         
        

     