using MyMachinePlatformClientCore.Summer.MyIOCContainer;

namespace MyMachinePlatformClientCore.Summer.Managers;

public class MyIOCContainerManagers
{

    private IContainerRegistry _registry;

    public MyIOCContainerManagers(IContainerRegistry registry)
    {
        this._registry = registry;
    }

    public void RegisterType()
    {
        
    }

    public void RegisterSingleton()
    {
        this._registry.MyRegisterSingleton<CMachineManager>();
    }
    public void RegisterInstance()
    {
        
    }
}