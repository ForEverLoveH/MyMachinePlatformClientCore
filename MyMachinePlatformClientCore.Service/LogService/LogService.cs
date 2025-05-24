using System.Collections.Concurrent;
using MyMachinePlatformClientCore.Log.MyLogs;

namespace MyMachinePlatformClientCore.Service.LogService;

public class LogService
{

    private Queue<LogMessage> _logQueues = new Queue<LogMessage>();
    
    private  int  threadCount=0;
    
    private  int currentThreadCount=0;
    /// <summary>
    /// 
    /// </summary>
    private AutoResetEvent _autoResetEvent= new AutoResetEvent(true);
    /// <summary>
    /// 
    /// </summary>

    private bool isRunning = false;
    /// <summary>
    /// 
    /// </summary>
    
    private Action<LogMessage> HandleCurrentLogMessage;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="threadCount"></param>
    /// <param name="handleCurrentLogMessage"></param>
    public LogService(int threadCount, Action<LogMessage> handleCurrentLogMessage= null)
    {
        this.threadCount =Math.Min(Math.Max(threadCount,1),10);
        this.HandleCurrentLogMessage = handleCurrentLogMessage;
    }
    
    /// <summary>
    /// 
    /// </summary>

    public void StartLogService()
    {
        if (isRunning) return;
        isRunning = true;
        if (threadCount > 0)
        {
            for (int i = 0; i < threadCount; i++)
            {
                ThreadPool.QueueUserWorkItem(new WaitCallback(_ThreadWorkHandleCallBack));
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="state"></param>
    private void _ThreadWorkHandleCallBack(object? state)
    {
        try
        {
            currentThreadCount = Interlocked.Increment(ref currentThreadCount);
            while (isRunning)
            {
                if (_logQueues.Count > 0)
                {
                    LogMessage message;
                    lock (_logQueues)
                    {
                        if (_logQueues.Count > 0)
                        {
                            message = _logQueues.Dequeue();
                        }
                        else
                        {
                            continue;
                        }
                    }

                    if (message != null)
                    {
                        HandleCurrentLogMessage?.Invoke(message);
                    }
                }
                else
                {
                    _autoResetEvent.WaitOne(); ///休眠等待，可使用set唤醒 存在前一个线程已经拿走了你当前的消息数据(也就是当前线程要俩消息数）
                    continue;
                }
            }
        }
        finally
        {
           currentThreadCount = Interlocked.Decrement(ref currentThreadCount);
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="message"></param>

    public void AddLogMessageToQueues(LogMessage message)
    {
        lock (_logQueues)
        {
            _logQueues.Enqueue(message);
        }

        if (_logQueues.Count > 0)
        {
            _autoResetEvent.Set();
        }
    }
}