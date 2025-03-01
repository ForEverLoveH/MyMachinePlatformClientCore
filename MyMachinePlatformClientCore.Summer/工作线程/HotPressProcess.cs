using MyMachinePlatformClientCore.Summer.Common;

namespace MyMachinePlatformClientCore.Summer ;

public class HotPressProcess:CRun
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="taskName"></param>
    public HotPressProcess(string taskName) : base(taskName)
    {
        
    }
    /// <summary>
    /// 
    /// </summary>
    public override void ExcuteCurrentTask()
    {
        Console.WriteLine("HotPressProcess");
    }
    
}