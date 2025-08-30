using Google.Protobuf;
using MyMachinePlatformClientCore.Common.TcpService.Client;
using MyMachinePlatformClientCore.Log.MyLogs;
using MyMachinePlatformClientCore.Service.ProtobufService;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using MyMachinePlatformClientCore.Service.LogService;

namespace MyMachinePlatformClientCore.Service
{
    /// <summary>
    /// 
    /// </summary>
    public class CTcpClient : Common.TcpService.Client.TcpClient
    {
        private bool _stop;
        private bool isJson = false;
        
        private Action<LogMessage> _logDataCallBack;
        

        public bool IsJson
        {
            get => isJson;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <param name="port"></param>
        public CTcpClient(string ipaddress, int port, bool isJson = false,Action<LogMessage>logDataCallBack=null) : base(ipaddress, port)
        {
            this.isJson = isJson;
            _logDataCallBack = logDataCallBack;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipEndPoint"></param>
        /// <param name="isJson"></param>
        /// <param name="logDataCallBack"></param>
        public CTcpClient(IPEndPoint ipEndPoint, bool isJson = false,Action<LogMessage>logDataCallBack=null) : base(ipEndPoint)
        {
            this.isJson = isJson;
            _logDataCallBack = logDataCallBack;
        }

        protected override void OnConnected()
        {
            _logDataCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,$" tcp 客户端服务器已连接，当前的guid为 {Id}"));
             
            StartHeartBeatService();
        }


        /// <summary>
        /// 
        /// </summary>
        protected override void OnDisconnected()
        {
            _logDataCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO,$" tcp 客户端服务器已断开连接，当前的guid为 {Id}"));
           
            // Wait for a while...
            Thread.Sleep(1000);
            // Try to connect again
            if (!_stop)
                ConnectAsync();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        protected override void OnReceived(byte[] buffer, long offset, long size)
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
                                //if (ClientMessageRouter.GetInstance().IsRunning)
                                //{
                                //    MyBaseClient myBaseClient = new MyBaseClient()
                                //    {
                                //        tcpClient = this,
                                //    };
                                //    ClientMessageRouter.GetInstance().AddMessageToQueue(myBaseClient, packMessage);
                                //};

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
                            _logDataCallBack?.Invoke(LogMessage.SetMessage(LogType.INFO, string.Format("{0}:{1}", "收到服务端json 数据", mess)));
                        }
                    }


                }
            }
        }
        #region 发送
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        public void CSendJsonData<T>(T message) where T : class
        {
            if (message == null) return;
            string json = JsonConvert.SerializeObject(message);
            CSendJsonData(json);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void CSendJsonData(string json)
        {
            ///封包
            if (string.IsNullOrEmpty(json)) return;
            byte[] message = Encoding.UTF8.GetBytes(json);
            message = message.Concat(CRCService.CreateWaterByte()).ToArray();
            byte[] crcCode = BitConverter.GetBytes(CRCService.ComputeChecksum(message));
            message = message.Concat(crcCode).ToArray();
            CSendJsonData(message);

        }
        /// <summary>
        ///  
        /// </summary>
        /// <param name="message"> 封包规则 4 个字节的字节长度 + (（流水码 + 消息体）+ crc码）</param>
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
                Send(result);
            }
        }
        #endregion

        #region protobuf 发送
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public bool CSendProtobufData<T>(T data) where T : IMessage<T>
        {
            //RSAService rsa = new RSAService();
            if (data == null) return  false;
            int code = ProtobufSession.SeqCode(data.GetType());
            byte[] typeCode = BitConverter.GetBytes(code);
            byte[] message = ProtobufSession.Serialize(data);
            byte[] mess = typeCode.Concat(message).ToArray();
            byte[] waterCode = CRCService.CreateWaterByte();
            byte[] m = mess.Concat(waterCode).ToArray();
            byte[] crc = BitConverter.GetBytes(CRCService.ComputeChecksum(m));
            byte[] result = m.Concat(crc).ToArray();
           return  CSendProtobufData(result);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public bool CSendProtobufData(byte[] message)
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
               return    SendAsync(result);
            }
            else
            {
                return false;
            }
        }
        #endregion

        #region 心跳
        /// <summary>
        /// 
        /// </summary>
        private Timer _heartBeatTimer;
        /// <summary>
        /// 
        /// </summary>
        private void StartHeartBeatService()
        {
            
            //ClientMessageRouter.GetInstance().OnMessage<HeartBeatResponse>(_HeartBeatResponse);
            _heartBeatTimer = new Timer(_TimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="session"></param>
        ///// <param name="message"></param>
        //private void _HeartBeatResponse(MyBaseClient session, HeartBeatResponse message)
        //{
        //    var ms = DateTime.Now - lastBeatTime;
        //    var pl = Math.Round(ms.TotalMilliseconds).ToString();
        //    int po = Math.Max(1, int.Parse(pl));
        //    string pll = $"网络延迟: {po}ms";
        //}

        private HeartBeatRequest _heartBeatRequest = new HeartBeatRequest();
        ///// <summary>
        ///
        /// </summary>
        private DateTime lastBeatTime = DateTime.MinValue;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private void _TimerCallback(object state)
        {
            if(isJson==false) CSendProtobufData(_heartBeatRequest);
            else
            {
                
            }
            
            lastBeatTime = DateTime.MinValue;
        }
        #endregion
        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        protected override void OnError(SocketError error)
        {
            _logDataCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR,$"Chat TCP client caught an error with code {error}"));
        }
    }
}
