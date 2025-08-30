using Google.Protobuf;
using MyMachinePlatformClientCore.Common.TcpService.Client;
using MyMachinePlatformClientCore.Log.MyLogs;
using MyMachinePlatformClientCore.Service.MessageRouter.ProtoMessage;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MyMachinePlatformClientCore.Service.message_router;
namespace MyMachinePlatformClientCore.Service.MessageRouter.JsonMessageRouter
{
    /// <summary>
    /// 客户端消息处理路由器
    /// </summary>
    public class ClientJsonMessageRouter
    {
        /// <summary>
        /// 最大线程数
        /// </summary>
        private int maxThreadCount = 10;
        /// <summary>
        /// 当前线程数
        /// </summary>

        private int currentThreadCount = 0;
        /// <summary>
        /// 
        /// </summary>
        private AutoResetEvent resetEvent = new AutoResetEvent(true);
        /// <summary>
        /// 
        /// </summary>
        private Queue<ClientJsonMessage> messageQueue = new Queue<ClientJsonMessage>();
         
        /// <summary>
        /// 消息处理字典
        /// </summary>
        private ConcurrentDictionary<string, Action<TcpClient,object>> currentMessageHandlers = new ConcurrentDictionary<string, Action<TcpClient, object>>();
        /// <summary>
        /// 
        /// </summary>
        private bool isRunning = false;
        /// <summary>
        /// 
        /// </summary>
        public bool Running
        {
            get { return isRunning; }
        }
        private Action<LogMessage> LogMessageCallBack;
        public ClientJsonMessageRouter(Action<LogMessage> logMessageCallBack)
        {
            this.LogMessageCallBack = logMessageCallBack;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tcpClient"></param>
        /// <param name="json"></param>
        public void AddMessageDataToQueue( TcpClient tcpClient, string json)  
        {
            if (string.IsNullOrEmpty(json)) return;
            CJsonMessage message = JsonConvert.DeserializeObject<CJsonMessage>(json);
            lock (messageQueue)
            {
                messageQueue.Enqueue(new ClientJsonMessage() { jsonMessage = message, tcpClient = tcpClient });
            }
            if (messageQueue.Count > 0)
            {
                resetEvent.Set();
            }
        }
         
        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageType"></param>
        /// <param name="action"></param>
        public void AddNewMessageHandlersToDictionary<T>(Action<TcpClient,object> action) where T : class
        {
            if(action == null) return;
            string name = typeof(T).FullName;
            if (currentMessageHandlers.ContainsKey(name)) currentMessageHandlers[name] = null;
            currentMessageHandlers.TryAdd(name, action);
            
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="threadCount"></param>
        public void StartService(int threadCount)
        {
            if (isRunning)
            {
                return;
            }

            isRunning = true;
            this.maxThreadCount = Math.Min(Math.Max(threadCount, 1), 200);
            for (int i = 0; i < this.maxThreadCount; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(MessageWork));
            }
            while (currentThreadCount < this.maxThreadCount)
            {
                Thread.Sleep(100);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="state"></param>
        private void MessageWork(object? state)
        {
            try
            {
                currentThreadCount = Interlocked.Increment(ref currentThreadCount);
                while (isRunning)
                {
                    if (messageQueue.Count == 0)
                    {
                        resetEvent.WaitOne();
                        continue;
                    }
                    ClientJsonMessage mes = null;
                    lock (messageQueue)
                    {
                        if (messageQueue.Count > 0) continue;
                        mes = messageQueue.Dequeue();

                    }
                    if(mes == null) continue;
                    ExcuteLoopMessage(mes.jsonMessage, mes.tcpClient);
                }
            }
            catch (Exception ex)
            {
              LogMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "消息处理发生异常，异常信息为：" + ex.Message));
                
            }
            finally
            {
                currentThreadCount = Interlocked.Decrement(ref currentThreadCount);
            }
        }
        /// <summary>
        /// 递归获取消息数据必须要满足在消息数据中的枚举字段名与消息数据中的数据类名一致
        /// </summary>
        /// <param name="jsonMessage"></param>
        /// <param name="tcpClient"></param>
        private void ExcuteLoopMessage(CJsonMessage jsonMessage, TcpClient tcpClient)
        {
            var messageType= jsonMessage.messageType;
            string messageName= messageType.GetType().GetEnumName(messageType);// 消息数据名字
            var infactMessage = jsonMessage.message;
            var matchMessage =  FindMatchMessage(infactMessage, messageName);
            if (matchMessage != null)
            {
                if(matchMessage.IsClass)
                {
                    string name= matchMessage.Name;
                    if (currentMessageHandlers.ContainsKey(name))
                    {
                        var handler = currentMessageHandlers[name];
                        handler?.Invoke(tcpClient,matchMessage);
                        
                    }
                }
            }
             
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="messageName"></param>
        /// <returns></returns>
        private Type FindMatchMessage(object obj, string? messageName)
        {
            if (obj== null) return null;
            Type type = obj.GetType();
            if (type.Name == messageName)
            {
                return type;
            }
            // 检查属性
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                object propertyValue = property.GetValue(obj);
                Type matchedType = FindMatchMessage(propertyValue, messageName);
                if (matchedType != null)
                {
                    return matchedType;
                }
            }
            // 检查字段
            foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                object fieldValue = field.GetValue(FindMatchMessage);
                Type matchedType = FindMatchMessage(fieldValue, messageName);
                if (matchedType != null)
                {
                    return matchedType;
                }
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        public void StopService()
        {
            isRunning = false;
            messageQueue.Clear();
            while (messageQueue.Count > 0)
            {
                resetEvent.Set();
            }
            Thread.Sleep(50);//考虑多线程，数据不
        }

    }
}
