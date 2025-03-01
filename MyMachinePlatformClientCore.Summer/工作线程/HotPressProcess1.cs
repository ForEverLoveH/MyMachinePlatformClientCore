using MyMachinePlatformClientCore.Summer.Common;

namespace MyMachinePlatformClientCore.Summer;

public class HotPressProcess1:CRun
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="taskName"></param>
    public HotPressProcess1(string taskName) : base(taskName)
    {
        
    }
    /// <summary>
    /// 
    /// </summary>
    public override void ExcuteCurrentTask()
    {
        Console.WriteLine("HotPressProcess1");
    }
    
}