using Google.Protobuf;
using MyMachinePlatformClientCore.Common.TcpService.Client;
using MyMachinePlatformClientCore.Service.MessageRouter.JsonMessageRouter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using MyMachinePlatformClientCore.Common.TcpService.Server;
using MyMachinePlatformClientCore.Service.MessageRouter;
using static MyMachinePlatformClientCore.Service.MessageRouter.CProtoMessageRouter;
using TcpClient = MyMachinePlatformClientCore.Common.TcpService.Client.TcpClient;
using MyMachinePlatformClientCore.Log.MyLogs;

namespace MyMachinePlatformClientCore.Service.message_router
{
    public  class MessageRouterManager
    {
         
        /// <summary>
        /// 
        /// </summary>
        private int threadCount=10;
        /// <summary>
        /// 
        /// </summary>
        private int type= 0;
        private Action<LogMessage> LogMessageCallback;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="threadCount"></param>
        /// <param name="type"> </param>
        public MessageRouterManager( int threadCount,Action<LogMessage>logMessageCallback, int type=0)
        {
            
            this.threadCount = threadCount;
            this.type = type;
            this.LogMessageCallback = logMessageCallback;
        }
        /// <summary>
        /// 
        /// </summary>
        private ClientJsonMessageRouter clientJsonMessage;
        /// <summary>
        /// 
        /// </summary>
        private CProtoMessageRouter protoMessageRouter;
        /// <summary>
        /// 
        /// </summary>

        public void StartService()
        {
            if (type == 0)
            {
                clientJsonMessage = new ClientJsonMessageRouter(LogMessageCallback);
                clientJsonMessage. StartService(threadCount);
            }
            else
            {
                protoMessageRouter = new CProtoMessageRouter(LogMessageCallback);
                protoMessageRouter.StartService(threadCount);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void  StopService()
        {
            if (type == 0)
            {
                clientJsonMessage.StopService();
            }
            else
            {
                protoMessageRouter.StopService();
            }
        }

        #region json数据
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tcpClient"></param>
        /// <param name="json"></param>
        public void AddMessageDataToQueue( TcpClient tcpClient, string json)
        {
            if (type == 0)
            {
                clientJsonMessage.AddMessageDataToQueue(tcpClient, json);
            }else
            {
                return;
            }
             
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action"></param>
        public void AddNewMessageHandlersToDictionary<T>(Action< TcpClient, object> action) where T : class
        {
            if (type == 0)
            {
                clientJsonMessage.AddNewMessageHandlersToDictionary<T>(action);
            }
            else
            {
                return;
            }
        }

        #endregion
        #region proto数据
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tcpClient"></param>
        /// <param name="message"></param>
        public void AddMessageDataToQueue<T>(Common.TcpService.Client.TcpClient tcpClient, T message) where T : IMessage
        {
            if (type == 1)
            {
                protoMessageRouter.AddMessageDataToQueue(tcpClient, message);
            }
            else
            {
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handler"></param>
        /// <typeparam name="T"></typeparam>
        public void OnMessage<T>(MessageHandler<T> handler) where T : IMessage
        {
            if (type == 1)
            {
                protoMessageRouter.OnMessage<T>(handler);
            }
            else
            {
                return;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handler"></param>
        /// <typeparam name="T"></typeparam>
        public void OffMessage<T>(MessageHandler<T> handler) where T : IMessage
        {
            if (type == 1)
            {
                protoMessageRouter.OffMessage<T>(handler);
            }
        }

        #endregion


    }
}
