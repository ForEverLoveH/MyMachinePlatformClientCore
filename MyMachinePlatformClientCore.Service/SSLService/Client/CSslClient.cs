using System.Net.Sockets;
using System.Text;
using Google.Protobuf;
using MyMachinePlatformClientCore.Common.Commo;
using MyMachinePlatformClientCore.Common.SSLService.Client;
using MyMachinePlatformClientCore.Log.MyLogs;
using MyMachinePlatformClientCore.Service.ProtobufService;

namespace MyMachinePlatformClientCore.Service.SSLService.Client;

public class CSslClient:SslClient
{
    /// <summary>
    /// 
    /// </summary>
    private bool _isjson = false;
    private bool _stop = false;
    /// <summary>
    /// 
    /// </summary>
    public bool Stop
    {
        
        get { return _stop; }
        set { _stop = value; }
        
    }
    /// <summary>
    /// 
    /// </summary>
    private Action<LogMessage>_logDataCallback;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="context"></param>
    /// <param name="address"></param>
    /// <param name="port"></param>
    /// <param name="isjson"></param>
    /// <param name="logDataCallback"></param>
    public CSslClient(SslContext context, string address, int port, bool isjson = false,Action<LogMessage>logDataCallback=null) : base(context, address, port)
    {
        this._isjson = isjson;
        this._logDataCallback = logDataCallback;
        
    }
    /// <summary>
    /// 
    /// </summary>
    public void DisconnectAndStop()
    {
        _stop = true;
        DisconnectAsync();
        while (IsConnected)
            Thread.Yield();
    }
    /// <summary>
    /// 
    /// </summary>
    protected override void OnConnected()
    {
        string msg = $"Chat SSL client connected a new session with Id {Id}";
        _logDataCallback?.Invoke(LogMessage.SetMessage(LogType.Success, msg));
    }
    /// <summary>
    /// 
    /// </summary>
    protected override void OnHandshaked()
    {
        _logDataCallback?.Invoke(LogMessage.SetMessage(LogType.Info,
            $"Chat SSL client handshaked a new session with Id {Id}"));
    }

    protected override void OnDisconnected()
    {
       _logDataCallback?.Invoke(LogMessage.SetMessage(LogType.Warm,$"Chat SSL client disconnected a new session with Id {Id}")); 
       Thread.Sleep(1000);
       if (!_stop)
           ConnectAsync();
    }
    protected override void OnError(SocketError error)
    {
        _logDataCallback?.Invoke(LogMessage.SetMessage(LogType.Error,$"Chat SSL client caught an error with code {error}"));
    }
    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
         if (!_isjson)
         {
             byte[] lengthBytes = new byte[4];
             Array.Copy(buffer, 0, lengthBytes, 0, 4);
             if (!BitConverter.IsLittleEndian) Array.Reverse(lengthBytes);
             int length = BitConverter.ToInt32(lengthBytes, 0);
             if (length > 0)
             {
                 byte[] infactMessage = new byte[length];
                 Array.Copy(buffer, 4, infactMessage, 0, length);
                 byte[] messageCode = new byte[infactMessage.Length - 2];
                 byte[] crcCode = new byte[2];
                 Array.Copy(infactMessage, 0, messageCode, 0, messageCode.Length);
                 Array.Copy(infactMessage, infactMessage.Length - 2, crcCode, 0, crcCode.Length);
                 ///
                 ushort currentCrcCode = BitConverter.ToUInt16(crcCode, 0);
                 ushort computedCrc = CRCService.ComputeChecksum(messageCode);
                 if (currentCrcCode == computedCrc)
                 {
                     byte[] messages = new byte[messageCode.Length - 2];
                     Array.Copy(messageCode, 0, messages, 0, messages.Length);
                     int code = BitConverter.ToInt32(messages, 0);
                     Type type = ProtobufSession.SeqType(code);
                     if (type.IsClass && typeof(IMessage).IsAssignableFrom(type))
                     {
                         byte[] data = new byte[messages.Length - 4];
                         Array.Copy(messages, 4, data, 0, data.Length);
                         IMessage packMessage = ProtobufSession.ParseFrom(code, data, 0, data.Length);
                         if (packMessage != null)
                         {
                    

                         }
                     }
                 }
             }



         }
         else
         {
             
             // json 数据的处理还需要进一步讨论
             byte[] lengthBytes = new byte[4];
             Array.Copy(buffer, 0, lengthBytes, 0, 4);
             if (!BitConverter.IsLittleEndian) Array.Reverse(lengthBytes);
             int length = BitConverter.ToInt32(lengthBytes, 0);
             if (length > 0)
             {
                 byte[] infactMessage = new byte[length];
                 Array.Copy(buffer, 4, infactMessage, 0, length);
                 byte[] messageCode = new byte[infactMessage.Length - 2];
                 byte[] crcCode = new byte[2];
                 Array.Copy(infactMessage, 0, messageCode, 0, messageCode.Length);
                 Array.Copy(infactMessage, infactMessage.Length - 2, crcCode, 0, crcCode.Length);
                 ushort currentCrcCode = BitConverter.ToUInt16(crcCode, 0);
                 ushort computedCrc = CRCService.ComputeChecksum(messageCode);
                 if (currentCrcCode == computedCrc)
                 {
                     byte[] messages = new byte[messageCode.Length - 2];
                     Array.Copy(messageCode, 0, messages, 0, messages.Length);
                     string mess = Encoding.UTF8.GetString(messages);
                     if (!string.IsNullOrEmpty(mess))
                     {
                         string message = string.Format("{0}:{1}", "收到服务端json 数据", mess);
                         _logDataCallback?.Invoke(LogMessage.SetMessage(LogType.Info, message));
                         
                     }
                 }


             }
         }
    }
    
    
}