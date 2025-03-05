using System.Net;
using System.Net.Sockets;
using System.Text;
using Google.Protobuf;
using MyMachinePlatformClientCore.Common.UDPService.Client;
using MyMachinePlatformClientCore.Log.MyLogs;
using MyMachinePlatformClientCore.Service.ProtobufService;
using Newtonsoft.Json;

namespace MyMachinePlatformClientCore.Service.UDPService.Client;

public class CUdpClient:UDPClient
{
    private bool isJson = false;
    public CUdpClient(IPAddress address, int port, bool isJson = false) : base(address, port)
    {
        this.isJson = isJson;
    }

    public CUdpClient(string address, int port, bool isJson = false) : base(address, port)
    {
        this.isJson = isJson;
    }

    public CUdpClient(DnsEndPoint endpoint, bool isJson = false) : base(endpoint)
    {
        this.isJson = isJson;
    }

    public CUdpClient(IPEndPoint endpoint, bool isJson = false) : base(endpoint)
    {
        this.isJson = isJson;
    }
    protected override void OnConnected()
    {
        Console.WriteLine($"Echo UDP client connected a new session with Id {Id}");

        // Start receive datagrams
        ReceiveAsync();
    }

    protected override void OnDisconnected()
    {
        Console.WriteLine($"Echo UDP client disconnected a session with Id {Id}");

        // Wait for a while...
        Thread.Sleep(1000);

        // Try to connect again
        if (!_stop)
            Connect();
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
        if (!isJson)
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
                        MyLogTool.ColorLog(MyLogColor.Blue, string.Format("{0}:{1}", "收到服务端json 数据", mess));
                    }
                }


            }
        }
    }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Echo UDP client caught an error with code {error}");
    }
    #region 发送
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="message"></param>
    public void CSend<T>(T message) where T:class 
    {
        if(message == null)return;
        string json= JsonConvert.SerializeObject(message);
        CSend(json);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public void CSend(string message)
    {
        if(string.IsNullOrEmpty(message))return;
        byte[] data= Encoding.UTF8.GetBytes(message);
        CSend(data);

    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public void CSend(byte[] message)
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
            SendAsync(result);   
        }
    }
    #endregion

    #region protobuf 发送
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
            SendAsync(result);
        }
    }
    #endregion

    private bool _stop;
}