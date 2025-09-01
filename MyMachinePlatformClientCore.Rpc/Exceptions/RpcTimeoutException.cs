namespace MyMachinePlatformClientCore.Rpc.Exceptions;

//
// 摘要:
//     Represents an exception that thrown as the RPC call is timeout.
public class RpcTimeoutException : RpcException
{
    //
    // 摘要:
    //     Create an instance of RpcTimeoutException class.
    public RpcTimeoutException()
        : base(408, "Request timeout.")
    {
    }
}