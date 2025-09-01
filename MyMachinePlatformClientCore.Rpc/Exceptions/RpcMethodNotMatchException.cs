namespace MyMachinePlatformClientCore.Rpc.Exceptions;

public class RpcMethodNotMatchException : RpcException
{
    public RpcMethodNotMatchException()
        : base(404, "The method parameters do not match.")
    {
    }
}