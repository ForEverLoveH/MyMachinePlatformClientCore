using System.Net.Sockets;
using System.Text;
using Google.Protobuf;
using MyMachinePlatformClientCore.Common.SSLService.Server;
using MyMachinePlatformClientCore.Log.MyLogs;
using MyMachinePlatformClientCore.Service.ProtobufService;

namespace MyMachinePlatformClientCore.Service.SSLService.Server;

public class CSslSession:SslSession
{
    public CSslSession(SslServer server, bool isJson=false) : base(server)
    {
        this.isJson = isJson;
        this.sslServer = server;    
    }
    protected override void OnConnected()
    {
        Console.WriteLine($"Chat SSL session with Id {Id} connected!");
    }
    private SslServer sslServer;
    bool isJson;
    protected override void OnHandshaked()
    {
        Console.WriteLine($"Chat SSL session with Id {Id} handshaked!");

        // Send invite message
        string message = "Hello from SSL chat! Please send a message or '!' to disconnect the client!";
        //Send(message);
    }

    protected override void OnDisconnected()
    {
        Console.WriteLine($"Chat SSL session with Id {Id} disconnected!");
    }

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        if (isJson) // json 发送数据 格式 4字节长度+ (数据+ 流水码)+ 2 个字节的 crc16检验码
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
        Console.WriteLine($"Chat SSL session caught an error with code {error}");
    }


    

}