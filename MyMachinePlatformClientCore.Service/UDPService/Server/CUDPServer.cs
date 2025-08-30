using System.Net;
using System.Net.Sockets;
using System.Text;
using Google.Protobuf;
using HslCommunication.Profinet.AllenBradley;
using MyMachinePlatformClientCore.Common.UDPService.Server;
using MyMachinePlatformClientCore.Log.MyLogs;
using MyMachinePlatformClientCore.Service.ProtobufService;
using Newtonsoft.Json;

namespace MyMachinePlatformClientCore.Service.UDPService.Server;

public class CUdpServer: UdpServer
{
    private bool isjson;
    public CUdpServer(IPAddress address, int port,bool isJson=false) : base(address, port)
    { 
        this.isjson = isJson;
    }
    private Action<LogMessage> logMessageCallBack;
    public CUdpServer(IPAddress address, int port, Action<LogMessage> logMessageCallBack,bool isJson = false) : this(address, port,isJson)
    {
        this.logMessageCallBack = logMessageCallBack;
    }

    public CUdpServer(string address,int port):base(address,port) { }

    protected override void OnStarted()
    {     
        ReceiveAsync();
    }
    protected override void OnSent(EndPoint endpoint, long sent)
    {     
        ReceiveAsync();
    }

    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="endpoint"></param>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="size"></param>
    protected override void OnReceived(EndPoint endpoint, byte[] buffer, long offset, long size)
    {
        if (isjson) // json 发送数据 格式 4字节长度+ (数据+ 流水码)+ 2 个字节的 crc16检验码
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
                ushort currentCrcCode = BitConverter.ToUInt16(crcCode, 0);
                ushort computedCrc = CRCService.ComputeChecksum(messageCode);
                if (currentCrcCode == computedCrc)
                {
                    byte[] messages = new byte[messageCode.Length - 2];
                    Array.Copy(messageCode, 0, messages, 0, messages.Length);
                    string mess = Encoding.UTF8.GetString(messages);
                    logMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO, string.Format("{0}:{1}", "收到客户端json 数据", mess)));
                }
            }
        }
        else   // protobuf 发送数据 格式 4字节长度+ (int 表示 type 数据 + 数据+ 流水码)  +2 个字节的 crc16检验码
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
    }

    
    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Echo UDP server caught an error with code {error}");
    }
    #region 发送 json数据
    public void CSendData<T>(T data) where T : class
    {
        if(data == null)return  ;
        string json = JsonConvert.SerializeObject(data);
        CSendData(json);     
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="json"></param>
    public void CSendData(string json)
    {
        if (!string.IsNullOrEmpty(json))
        {
            byte[] message = Encoding.UTF8.GetBytes(json);
            CSendData(message);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public void CSendData(byte[] message)
    {
        int length = message.Length;
        byte[] buffer = BitConverter.GetBytes(length);
        if (buffer.Length > 0)
        {
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            byte[] result = buffer.Concat(message).ToArray();
            MulticastAsync(result);
        }
    }
    #endregion

    #region protobuf 发送数据
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="data"></param>
    public void CSendProtobufData<T>(T data) where T : IMessage<T>
    {
        
        if (data == null) return;
        int code = ProtobufSession.SeqCode(data.GetType());
        byte[] typeCode = BitConverter.GetBytes(code);
        byte[] message = ProtobufSession.Serialize(data);
        byte[] mess = typeCode.Concat(message).ToArray();
        byte[] waterCode = CRCService.CreateWaterByte();
        byte[] m = mess.Concat(waterCode).ToArray();
        byte[] crc = BitConverter.GetBytes(CRCService.ComputeChecksum(m));
        byte[] result = m.Concat(crc).ToArray();
        CSendProtobufData(result);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public void CSendProtobufData(byte[] message)
    {
        int length = message.Length;
        byte[] buffer = BitConverter.GetBytes(length);
        if (buffer.Length > 0)
        {
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            byte[] result = buffer.Concat(message).ToArray();
            MulticastAsync(result);
        }
    }
    #endregion

}