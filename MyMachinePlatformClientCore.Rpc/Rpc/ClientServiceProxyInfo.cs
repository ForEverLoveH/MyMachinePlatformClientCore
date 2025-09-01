namespace MyMachinePlatformClientCore.Rpc.Rpc;

internal class ClientServiceProxyInfo : IDisposable
{
    public Type ServiceType { get; }

    public object ServiceToken { get; }

    public object ServiceProxy { get; set; }

    public RpcInterceptor Interceptor { get; set; }

    internal ClientServiceProxyInfo(Type serviceType, object serviceToken)
    {
        ServiceToken = serviceToken;
        ServiceType = serviceType;
    }

    public void Dispose()
    {
        Interceptor?.Dispose();
    }
}