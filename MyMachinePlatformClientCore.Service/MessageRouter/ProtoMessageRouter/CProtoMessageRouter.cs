using Google.Protobuf;
using MyMachinePlatformClientCore.Common.TcpService.Client;
using MyMachinePlatformClientCore.Log.MyLogs;
using MyMachinePlatformClientCore.Service.MessageRouter.ProtoMessage;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MyMachinePlatformClientCore.Service.MessageRouter
{
    /// <summary>
    /// protobuf 消息路由
    /// </summary>
    public class CProtoMessageRouter
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
        private Queue<ClientProtoMessage> messageQueue = new Queue<ClientProtoMessage>();
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="protoMessage"></param>
        public delegate void MessageHandler<T>(ClientProtoMessage protoMessage);
        /// <summary>
        /// 消息处理字典
        /// </summary>
        private ConcurrentDictionary<string, Delegate> currentMessageHandlers = new ConcurrentDictionary<string, Delegate>();
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
        private Action<LogMessage> _logMessageCallBack;
        public CProtoMessageRouter(Action<LogMessage> logMessageCallBack)
        {
            _logMessageCallBack = logMessageCallBack;
        }

        /// <summary>
        /// 添加消息到队列中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tcpClient"></param>
        /// <param name="message"></param>
        public void AddMessageDataToQueue<T>(TcpClient tcpClient, T message) where T : IMessage
        {
            lock (messageQueue)
            {
                messageQueue.Enqueue(new ClientProtoMessage() { message = message, tcpClient = tcpClient });
            }
            if (messageQueue.Count > 0)
            {
                resetEvent.Set();
            }

        }
        #region 订阅消息处理
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        public void OnMessage<T>(MessageHandler<T> handler) where T : IMessage
        {
            try
            {
                string key = typeof(T).FullName;
                if (!currentMessageHandlers.ContainsKey(key))
                {
                    currentMessageHandlers[key] = null;
                }

                currentMessageHandlers[key] = (currentMessageHandlers[key] as MessageHandler<T>) + handler;
                Console.Write(currentMessageHandlers[key].GetInvocationList().Length);
            }
            catch (Exception ex)
            {
                _logMessageCallBack?.Invoke(  LogMessage.SetMessage(LogType.ERROR, "订阅消息发生异常，异常信息为：" + ex.Message));
                return;
            }
        }
        #endregion
        #region 退订
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        public void OffMessage<T>(MessageHandler<T> handler) where T : IMessage
        {
            try
            {
                string key = typeof(T).FullName;
                if (!currentMessageHandlers.ContainsKey(key))
                {
                    currentMessageHandlers[key] = null;
                }
                currentMessageHandlers[key] = currentMessageHandlers[key] as MessageHandler<T> - handler;
            }
            catch (Exception ex)
            {
                _logMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "退订消息发生异常，异常信息为：" + ex.Message));
                return;
            }
        }

        #endregion 退订
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
            this.maxThreadCount = threadCount;
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
        public void StopService()
        {
            isRunning = false;
            messageQueue.Clear();
            while (messageQueue .Count> 0)
            {
               resetEvent.Set();
            }
            Thread.Sleep(50);//考虑多线程，数据不
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
                    ClientProtoMessage message = null;
                    lock (messageQueue)
                    {
                        if (messageQueue.Count > 0)
                        {
                            message = messageQueue.Dequeue();
                        }
                        else continue;
                    }
                    if (message != null)
                    {
                        var mes = message.message;
                        if (mes != null)
                        {
                            ExcuteLoopMessage(mes, message.tcpClient);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                _logMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "消息处理发生异常，异常信息为：" + ex.Message));
            }
            finally
            {
                currentThreadCount = Interlocked.Decrement(ref currentThreadCount);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mes"></param>
        /// <param name="tcpClient"></param>
        private void ExcuteLoopMessage(IMessage message, TcpClient tcpClient)
        {
            var fireMethod = this.GetType().GetMethod("FireMessageData", BindingFlags.NonPublic | BindingFlags.Instance);
            var met = fireMethod.MakeGenericMethod(message.GetType());
            met.Invoke(this, new object[] { tcpClient, message });
            var t = message.GetType();
            foreach (var p in t.GetProperties())
            {
                // Log.Information($"{p.Name}");
                if (p.Name == "Parser" || p.Name == "Descriptor")
                    continue;
                //只要发现消息就可以订阅 递归思路实现
                var value = p.GetValue(message);
                if (value != null)
                {
                    //发现消息是否需要进一步递归 触发订阅
                    if (typeof(IMessage).IsAssignableFrom(value.GetType()))
                    {
                        //发现消息是否需要进一步递归 触发订阅
                        //继续递归
                        ExcuteLoopMessage((IMessage)value, tcpClient);
                    }
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="tcpClient"></param>
        /// <param name="messageData"></param>
        private void FireMessageData<T>(CTcpClient tcpClient, T messageData) where  T : IMessage
        {
            string type = typeof(T).FullName;
            if (currentMessageHandlers.ContainsKey(type))
            {
                MessageHandler<T> handler = (MessageHandler<T>)currentMessageHandlers[type];
                try
                {
                    ClientProtoMessage currentMessage = new ClientProtoMessage()
                    {
                        message = messageData,
                        tcpClient = tcpClient
                    };
                    handler?.Invoke(currentMessage);
                }
                catch (Exception ex)
                {
                    _logMessageCallBack?.Invoke(LogMessage.SetMessage(LogType.ERROR, "消息处理发生异常，异常信息为：" + ex.Message));
                    return;   
                }
            }
        }

    }
}
