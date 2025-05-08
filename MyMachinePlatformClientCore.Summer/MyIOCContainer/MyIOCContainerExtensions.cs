namespace MyMachinePlatformClientCore.Summer.MyIOCContainer;

public   static class MyIOCContainerExtensions
{

    public static void MyRegisterType<TEntityService>(this IContainerRegistry registry) where TEntityService : class
    {
        registry.Register<TEntityService>();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="registry"></param>
    /// <typeparam name="IEntityService"></typeparam>
    /// <typeparam name="TEntityService"></typeparam>
    public static void MyRegisterType<IEntityService, TEntityService>(this IContainerRegistry registry)
        where TEntityService : class, IEntityService
    {
        registry.Register<IEntityService, TEntityService>();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="registry"></param>
    /// <typeparam name="TEntityService"></typeparam>
    public static void MyRegisterSingleton<TEntityService>(this IContainerRegistry registry)
        where TEntityService : class
    {
        registry.RegisterSingleton<TEntityService>();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="registry"></param>
    /// <typeparam name="IEntityService"></typeparam>
    /// <typeparam name="TEntityService"></typeparam>
    public static void MyRegisterSingleton<IEntityService, TEntityService>(this IContainerRegistry registry)
        where TEntityService : class, IEntityService
    {
        registry.RegisterSingleton<IEntityService, TEntityService>();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="registry"></param>
    /// <param name="service"></param>
    /// <typeparam name="TEntityService"></typeparam>
    public static void MyRegisterInstance<TEntityService>(this IContainerRegistry registry,TEntityService service) where TEntityService : class
    {
        registry.RegisterInstance(service);
         
    }

}