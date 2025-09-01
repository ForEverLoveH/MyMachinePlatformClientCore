using System.Collections.Concurrent;
using Castle.DynamicProxy;
using Newtonsoft.Json;

namespace MyMachinePlatformClientCore.Rpc.Rpc;

internal static class RpcInvocationSerializerSettings
{
    internal static readonly JsonSerializerSettings Default;

    static RpcInvocationSerializerSettings()
    {
        Default = new JsonSerializerSettings();
    }
}
//
// 摘要:
//     Generate service proxy for client RPC calls.
public class ServiceProxyGenerator : IDisposable
{
    private IRpcTransfer _rpcTransfer;

    private ConcurrentDictionary<Type, ClientServiceProxyInfo> _clientProxyDict = new ConcurrentDictionary<Type, ClientServiceProxyInfo>();

    private ProxyGenerator _proxyGenerator = new ProxyGenerator();

    //
    // 摘要:
    //     Gets or sets the RPC call timeout, in millseconds.The default timeout is 30000ms.
    public int RpcTimeout { get; set; } = 60_000;


    //
    // 摘要:
    //     Create an instance of ServiceProxyGenerator
    //
    // 参数:
    //   rpcTransfer:
    //     A class instance which implemented IRpcClient interface.
    public ServiceProxyGenerator(IRpcTransfer rpcTransfer)
    {
        _rpcTransfer = rpcTransfer;
    }

    //
    // 摘要:
    //     Register the service proxy type to the channel.
    //
    // 参数:
    //   serviceToken:
    //     The token of the register service.
    //
    // 类型参数:
    //   TService:
    //     The service proxy type that will be called by user.
    public void RegisterServiceProxy<TService>(object serviceToken)
    {
        Type typeFromHandle = typeof(TService);
        ClientServiceProxyInfo value = new ClientServiceProxyInfo(typeFromHandle, serviceToken);
        if (_clientProxyDict.TryGetValue(typeFromHandle, out var value2))
        {
            value2.Dispose();
        }

        _clientProxyDict[typeFromHandle] = value;
    }

    //
    // 摘要:
    //     Register the service proxy type to the channel.
    //
    // 参数:
    //   serviceType:
    //     The service proxy type that will be called by user.
    //
    //   serviceToken:
    //     The token of the register service.
    public void RegisterServiceProxy(Type serviceType, object serviceToken)
    {
        ClientServiceProxyInfo value = new ClientServiceProxyInfo(serviceType, serviceToken);
        if (_clientProxyDict.TryGetValue(serviceType, out var value2))
        {
            value2.Dispose();
        }

        _clientProxyDict[serviceType] = value;
    }

    //
    // 摘要:
    //     UnRegister the service proxy type in the channel.
    //
    // 参数:
    //   serviceType:
    //     The service proxy type that will be called by user.
    public void UnRegisterServiceProxy(Type serviceType)
    {
        _clientProxyDict.TryRemove(serviceType, out var _);
    }

    //
    // 摘要:
    //     UnRegister the service proxy type in the channel.
    //
    // 类型参数:
    //   TService:
    //     The service proxy type that will be called by user.
    public void UnRegisterServiceProxy<TService>()
    {
        Type typeFromHandle = typeof(TService);
        _clientProxyDict.TryRemove(typeFromHandle, out var _);
    }

    //
    // 摘要:
    //     Get the service proxy from the channel.The user can use the service proxy to
    //     call RPC service.
    //
    // 参数:
    //   serviceType:
    //     The service proxy type that will be called by user.
    //
    // 返回结果:
    //     The instance of the service proxy.
    public object GetServiceProxy(Type serviceType)
    {
        if (!_clientProxyDict.TryGetValue(serviceType, out var value))
        {
            throw new Exception("The service has not registered.");
        }

        if (value.ServiceProxy == null)
        {
            value.Interceptor = new RpcInterceptor(_rpcTransfer, RpcTimeout, value.ServiceToken);
            value.ServiceProxy = _proxyGenerator.CreateInterfaceProxyWithoutTarget(value.ServiceType, value.Interceptor);
        }

        return value.ServiceProxy;
    }

    //
    // 摘要:
    //     Get the service proxy from the channel.The user can use the service proxy to
    //     call RPC service.
    //
    // 类型参数:
    //   TService:
    //     The service proxy type that will be called by user.
    //
    // 返回结果:
    //     The instance of the service proxy.
    public TService GetServiceProxy<TService>()
    {
        Type typeFromHandle = typeof(TService);
        if (!_clientProxyDict.TryGetValue(typeFromHandle, out var value))
        {
            throw new Exception("The service has not registered.");
        }

        if (value.ServiceProxy == null)
        {
            value.Interceptor = new RpcInterceptor(_rpcTransfer, RpcTimeout, value.ServiceToken);
            value.ServiceProxy = _proxyGenerator.CreateInterfaceProxyWithoutTarget(value.ServiceType, value.Interceptor);
        }

        return (TService)value.ServiceProxy;
    }

    //
    // 摘要:
    //     Release all resources.
    public void Dispose()
    {
        foreach (ClientServiceProxyInfo value in _clientProxyDict.Values)
        {
            value.Dispose();
        }

        _clientProxyDict.Clear();
    }
}