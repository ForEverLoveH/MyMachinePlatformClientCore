using Microsoft.Win32;
using MyMachinePlatformClientCore.IService.IFreeSqlService;
using MyMachinePlatformClientCore.IService.ISqlSugarService;
using MyMachinePlatformClientCore.Service.FreeSqlService;
using MyMachinePlatformClientCore.Service.SqlSugarService;
using MyMachinePlatformClientCore.Summer.MyIOCContainer;
namespace MyMachinePlatformClientCore.Managers;

public class MyIOCContainerManagers
{
    private IContainerRegistry _registry;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="registry"></param>
    public MyIOCContainerManagers(IContainerRegistry registry)
    {
        this._registry = registry;
    }

    public void RegisterType()
    {
        _registry.MyRegisterType<ISqlSugarService, SqlSugarService>();
        _registry.MyRegisterType<IFreesqlService, FreeSqlService>();
    }

    public void RegisterSingleton()
    {
        _registry.MyRegisterSingleton<CMachineManager>();
    }
    public void RegisterInstance()
    {
        
    }
}