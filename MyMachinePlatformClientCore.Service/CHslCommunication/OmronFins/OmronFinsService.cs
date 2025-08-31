using HslCommunication.Profinet.Omron;
using MyMachinePlatformClientCore.IService.CHslCommunication;
using MyMachinePlatformClientCore.Log.MyLogs;

namespace MyMachinePlatformClientCore.Service.CHslCommunication;

public class OmronFinsService:IPLCService,IDisposable
{
    private OmronFinsNet omronFinsNet;
    private string ipaddress;
    private int port;
    private int timeOut;
    
    private bool isConnect = false;
    public bool IsConnect
    {
        get { return isConnect; }
    }
    public Action<LogMessage> LogMessageCallBack { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ipaddress"></param>
    /// <param name="port"></param>
    /// <param name="timeOut"></param>
    /// <param name="logMessageCallBack"></param>
    public OmronFinsService(string ipaddress,int port,int timeOut,Action<LogMessage> logMessageCallBack=null)
    {
        this.ipaddress = ipaddress;
        this.port = port;
        this.timeOut = timeOut;
        this.LogMessageCallBack = logMessageCallBack;
        omronFinsNet = new OmronFinsNet()
        {
            IpAddress = ipaddress,
            Port = port,
            ConnectTimeOut = timeOut,    // 连接超时时间

        };
        LogMessageCallBack = logMessageCallBack;
    }
    ///
    /// <summary>
    /// 启动服务的方法
    /// </summary>
    /// <returns>返回一个Task<bool>表示异步操作的结果，true表示服务启动成功，false表示失败</returns>
    public async  Task<bool> StartService()
    {
        try
        {
             if(IsConnect)
             {
                 return true;
             }
             var result= await omronFinsNet.ConnectServerAsync();
             if(result.IsSuccess)
             {
                 isConnect = true;
                 LogMessageCallBack(LogMessage.SetMessage(LogType.INFO, "链接OmronFins成功，IP地址为："+ipaddress+",端口为："+port+",超时时间为："+timeOut));
             }
             else
             {
                 LogMessageCallBack(LogMessage.SetMessage(LogType.ERROR, "链接OmronFins失败，IP地址为："+ipaddress+",端口为："+port+",超时时间为："+timeOut));
             }
             return isConnect;
        }
        catch (Exception e)
        {
            // 捕获并打印异常信息
           LogMessageCallBack(LogMessage.SetMessage(LogType.ERROR, "链接OmronFins异常，异常信息为："+e.Message));
            // 重新抛出异常，以便上层调用者可以处理
            throw;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public  async  Task<bool>StopService()
    {
        if(IsConnect)
        {
            var result  =  await omronFinsNet.ConnectCloseAsync();
            if (result.IsSuccess)
            {
                isConnect = false;
                LogMessageCallBack(LogMessage.SetMessage(LogType.INFO, "断开OmronFins连接"));
                return true;
            }
            else {
                LogMessageCallBack(LogMessage.SetMessage(LogType.ERROR, "断开OmronFins连接失败"));
                return false;
            }
        }

        return false;
    }

    #region 读取

   /// <summary>
   /// 
   /// </summary>
   /// <param name="address"></param>
   /// <param name="length"></param>
   /// <returns></returns>
    public async Task<byte[]> ReadByte(string address, ushort length)
    {
        if (omronFinsNet == null || !isConnect) return null;
        var result = await omronFinsNet.ReadAsync(address, length);
        if (result.IsSuccess)
        {
            return result.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在读取欧姆龙Fins时，发生异常，异常信息为;" + result.Message));
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
        if (omronFinsNet == null || !isConnect) return 0;
        var result = await omronFinsNet.ReadInt16Async(address);
        if (result.IsSuccess)
        {
            return result.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在读取欧姆龙Fins时，发生异常，异常信息为;" + result.Message));
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
        if (omronFinsNet == null || !isConnect) return 0;
        var result = await omronFinsNet.ReadUInt16Async(address);
        if (result.IsSuccess)
        {
            return result.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在读取欧姆龙Fins时，发生异常，异常信息为;" + result.Message));
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
        if (omronFinsNet == null || !isConnect) return 0;
        var result = await omronFinsNet.ReadInt32Async(address);
        if (result.IsSuccess)
        {
            return result.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在读取欧姆龙Fins时，发生异常，异常信息为;" + result.Message));
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
        if (omronFinsNet == null || !isConnect) return 0;
        var result = await omronFinsNet.ReadUInt32Async(address);
        if (result.IsSuccess)
        {
            return result.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在读取欧姆龙Fins时，发生异常，异常信息为;" + result.Message));
            return 0;
        }
    }
    /// <summary>
    /// 读取字符串
    /// </summary>
    /// <param name="address"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    public async Task<string> ReadString(string address, ushort length)
    {
        if (omronFinsNet == null || !isConnect) return null;
        var result = await omronFinsNet.ReadStringAsync(address, length);
        if (result.IsSuccess)
        {
            return result.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在读取欧姆龙Fins时，发生异常，异常信息为;" + result.Message));
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
        if (omronFinsNet == null || !isConnect) return 0;
        var result = await omronFinsNet.ReadFloatAsync(address);
        if (result.IsSuccess)
        {
            return result.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在读取欧姆龙Fins时，发生异常，异常信息为;" + result.Message));
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
        if (omronFinsNet == null || !isConnect) return 0;
        var result = await omronFinsNet.ReadDoubleAsync(address);
        if (result.IsSuccess)
        {
            return result.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在读取欧姆龙Fins时，发生异常，异常信息为;" + result.Message));
            return 0;
        }
    }
    /// <summary>
    /// 读取bool
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public async Task<ulong> ReadUInt64(string address)
    {
        if (omronFinsNet == null || !isConnect) return 0;
        var result = await omronFinsNet.ReadUInt64Async(address);
        if (result.IsSuccess)
        {
            return result.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在读取欧姆龙Fins时，发生异常，异常信息为;" + result.Message));
            return 0;
        }
    }
    /// <summary>
    /// 读取int64
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    public async Task<long> ReadInt64(string address)
    {
        if (omronFinsNet == null || !isConnect) return 0;
        var result = await omronFinsNet.ReadInt64Async(address);
        if (result.IsSuccess)
        {
            return result.Content;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在读取欧姆龙Fins时，发生异常，异常信息为;" + result.Message));
            return 0;
        }
    }
    
    #endregion
    
    #region 写入
    /// <summary>
    /// 写入ushort
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<bool> Write(string address, ushort value)
    {
        if (omronFinsNet == null || !isConnect) return false;
        var result = await omronFinsNet.WriteAsync(address, value);
        if (result.IsSuccess)
        {
            return true;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在写入欧姆龙Fins时，发生异常，异常信息为;" + result.Message));
            return false;
        }
    }
    /// <summary>
    /// 写入int
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>

    public async Task<bool> Write(string address, int value)
    {
        if (omronFinsNet == null || !isConnect) return false;
        var result = await omronFinsNet.WriteAsync(address, value);
        if (result.IsSuccess)
        {
            return true;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在写入欧姆龙Fins时，发生异常，异常信息为;" + result.Message));
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
        if (omronFinsNet == null || !isConnect) return false;
        var result = await omronFinsNet.WriteAsync(address, value);
        if (result.IsSuccess)
        {
            return true;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在写入欧姆龙Fins时，发生异常，异常信息为;" + result.Message));
            return false;
        }
    }
    /// <summary>
    /// 写入ushort数组
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<bool> Write(string address, short[] value)
    {
        if (omronFinsNet == null || !isConnect) return false;
        var result = await omronFinsNet.WriteAsync(address, value);
        if (result.IsSuccess)
        {
            return true;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在写入欧姆龙Fins时，发生异常，异常信息为;" + result.Message));
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
        if (omronFinsNet == null || !isConnect) return false;
        var result = await omronFinsNet.WriteAsync(address, value);
        if (result.IsSuccess)
        {
            return true;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在写入欧姆龙Fins时，发生异常，异常信息为;" + result.Message));
            return false;
        }
    }
    /// <summary>
    /// 写入double
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<bool> Write(string address, double value)
    {
        if (omronFinsNet == null || !isConnect) return false;
        var result = await omronFinsNet.WriteAsync(address, value);
        if (result.IsSuccess)
        {
            return true;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在写入欧姆龙Fins时，发生异常，异常信息为;" + result.Message));
            return false;
        }
    }
    /// <summary>
    /// 写入uint
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>

    public async Task<bool> Write(string address, uint value)
    {
        if (omronFinsNet == null || !isConnect) return false;
        var result = await omronFinsNet.WriteAsync(address, value);
        if (result.IsSuccess)
        {
            return true;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在写入欧姆龙Fins时，发生异常，异常信息为;" + result.Message));
            return false;
        }
    }
    /// <summary>
    /// /
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<bool> Write(string address, byte[] value)
    {
        if (omronFinsNet == null || !isConnect) return false;
        var result = await omronFinsNet.WriteAsync(address, value);
        if (result.IsSuccess)
        {
            return true;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在写入欧姆龙Fins时，发生异常，异常信息为;" + result.Message));
            return false;
        }
    }
    /// <summary>
    /// /
    /// </summary>
    /// <param name="address"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public async Task<bool> Write(string address, ulong value)
    {
        if (omronFinsNet == null || !isConnect) return false;
        var result = await omronFinsNet.WriteAsync(address, value);
        if (result.IsSuccess)
        {
            return true;
        }
        else
        {
            LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "在写入欧姆龙Fins时，发生异常，异常信息为;" + result.Message));
            return false;
        }
    }
    #endregion 
    
    

   
    public void Dispose()
    {
        StopService();
    }
}