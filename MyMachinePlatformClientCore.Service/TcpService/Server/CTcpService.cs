using System.Net;
using System.Net.Sockets;
using System.Text;
using Google.Protobuf;
using MyMachinePlatformClientCore.Common.TcpService.Server;
using MyMachinePlatformClientCore.Service.ProtobufService;
using Newtonsoft.Json;

namespace MyMachinePlatformClientCore.Service ;

/// <summary>
/// 
/// </summary>
public class CTcpService:TcpService
{

    /// <summary>
    /// 
    /// </summary>
    /// <param name="ipAddress"></param>
    /// <param name="port"></param>
    public CTcpService(IPAddress ipAddress, int port) : base(ipAddress, port)
    {
        
    }

    public CTcpService(string ipaddress, int port) : base(ipaddress, port)
    {
        
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="ipEndPoint"></param>
    public CTcpService(IPEndPoint ipEndPoint) : base(ipEndPoint)
    {
        
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override TcpSession CreateSession()
    {
        return new CTcpSession(this);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="error"></param>
    protected override void OnError(SocketError error)
    {
        
    }

    #region  json 发送

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <typeparam name="T"></typeparam>
    public void CSendJsonData<T>(T data) where T : class
    {
        if (data == null) return;
        string json = JsonConvert.SerializeObject(data);
        CSendJsonData(json);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="json"></param>
    public void CSendJsonData(string json)
    {
        if (!string.IsNullOrEmpty(json))
        {
            byte[] message = Encoding.UTF8.GetBytes(json);
            message=message.Concat(CRCService.CreateWaterByte()).ToArray();
            byte[] crcCode = BitConverter.GetBytes(CRCService.ComputeChecksum(message));
            message = message.Concat(crcCode).ToArray();
            CSendJsonData(message);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>
    public void CSendJsonData(byte[] message)
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
            Multicast(result);
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
    public void  CSendProtobufData(byte[] message)
    {
        int length = message.Length;
        byte[] buffer = BitConverter.GetBytes(length);
        if (buffer.Length > 0)
        {
            if (!BitConverter.IsLittleEndian) Array.Reverse(buffer);

            byte[] result = buffer.Concat(message).ToArray();
            Multicast(result);
        }
    }

    #endregion



}