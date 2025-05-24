using System.Configuration;
using System.Data;
using System.Windows;
using MyMachinePlatformClientCore.Managers;
using MyMachinePlatformClientCore.Summer.MyIOCContainer;
using MyMachinePlatformClientCore.Views;

namespace MyMachinePlatformClientCore;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : PrismApplication
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="shell"></param>
    protected override void InitializeShell(Window shell)
    {
      //var login = Container.Resolve<LoginWindow>(); 
      //var isLogin = login.ShowDialog();
      // if (isLogin == true)
      //{
      //      base.InitializeShell(shell);
      //}
      //else 
      //{
      //      Current.Shutdown();
      //}
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="containerRegistry"></param>
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        MyIOCContainerManagers managers = new MyIOCContainerManagers(containerRegistry);
        managers.RegisterInstance();
        managers.RegisterSingleton();
        managers.RegisterType();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override Window CreateShell()
    {
        return Container.Resolve<MainWindow>();
    }
}