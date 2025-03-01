namespace MyMachinePlatformClientCore.Summer.Common;

public class CRun
{
    /// <summary>
    /// 
    /// </summary>
    private CancellationTokenSource _cts;
    /// <summary>
    /// 
    /// </summary>
    private  object _lock = new object();
    /// <summary>
    /// 
    /// </summary>
    private Task _task;

    private string taskName;
    
    private  bool isSupend = false;
    
    public bool IsSupend
    {
        get => isSupend;
        set => isSupend = value;
    }
    
    private   Guid _guid;
    /// <summary>
    /// 
    /// </summary>
    private bool isStarted;
    /// <summary>
    /// 
    /// </summary>
    public bool IsStarted
    {
        get
        {
            return isStarted;
        }
        set
        {
            isStarted = value;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void ExcuteCurrentTask()
    {

    }
    
    /// <summary>
    /// /
    /// </summary>
    /// <param name="taskName"></param>
    public CRun(string taskName)
    {
        this.taskName = taskName;
       
        _cts = new CancellationTokenSource();
        _guid = Guid.NewGuid();
    }
    /// <summary>
    /// 
    /// </summary>
    public virtual void Start()
    {
        lock (_lock)
        {
            if (_task == null || _task.IsCompleted)
            {

                _task = Task.Run(async () =>
                {
                    if (IsStarted)
                    {
                        while (!_cts.IsCancellationRequested)
                        {
                            if (!IsSupend)
                            {
                                ExcuteCurrentTask();
                            }

                            await Task.Delay(100);
                        }
                    }
                }, _cts.Token);
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public virtual void Stop()
    {
        if (_task != null)
        {
            if (_cts != null)
            {
                if (_cts.IsCancellationRequested && _cts.IsCancellationRequested)
                {
                    _cts.Cancel();
                    _cts.Dispose();
                }
            }
            _task.Dispose();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public virtual void Suspend()
    {
        isSupend = true;
    }




}