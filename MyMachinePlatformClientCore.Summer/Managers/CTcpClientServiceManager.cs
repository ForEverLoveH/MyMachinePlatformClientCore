using Google.Protobuf;
using MyMachinePlatformClientCore.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Summer.Managers
{
    /// <summary>
    /// tcp client 服务管理
    /// </summary>
    public class CTcpClientServiceManager
    {
        private CTcpClient tcpClient;
        /// <summary>
        /// 
        /// </summary>
        private string ipaddress;
        /// <summary>
        /// 
        /// </summary>
        private int port;
        /// <summary>
        /// 是否是json数据
        /// </summary>
        private bool isJson=false;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <param name="port"></param>
        /// <param name="isJson"></param>
        public CTcpClientServiceManager(string ipaddress, int port,bool isJson=false):this(IPAddress.Parse(ipaddress), port,isJson)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipaddress"></param>
        /// <param name="port"></param>
        /// <param name="isJson"></param>
        public CTcpClientServiceManager(IPAddress ipaddress, int port,bool isJson=false)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        public void StartService()
        {
            tcpClient = new CTcpClient(ipaddress, port, isJson);
            tcpClient.Connect();
        }
        /// <summary>
        /// 
        /// </summary>
        public void StopService()
        {
            tcpClient?.Disconnect();
        }
        /// <summary>
        /// 重连
        /// </summary>
        public void Reconnect()
        {
            tcpClient?.Reconnect();
        }
        #region json数据发送
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="message"></param>
        public void CSendJsonData<T>(T message) where T : class =>tcpClient?.CSendJsonData(message);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="json"></param>
        public void CSendJsonData(string json) => tcpClient?.CSendJsonData(json);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void CSendJsonData(byte[] data) => tcpClient?.CSendJsonData(data);


        #endregion

        #region protobuf 发送
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="data"></param>
        public void CSendProtobufData<T>(T data) where T : IMessage<T> => tcpClient?.CSendProtobufData(data);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data"></param>
        public void CSendProtobufData(byte[] data) => tcpClient?.CSendProtobufData(data);



        #endregion

    }
}
