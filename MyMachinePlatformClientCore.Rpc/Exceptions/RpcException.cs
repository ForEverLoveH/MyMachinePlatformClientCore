namespace MyMachinePlatformClientCore.Rpc.Exceptions;

//
// 摘要:
//     The based class for RPC exception
public class RpcException : Exception
{
    //
    // 摘要:
    //     Gets the code of the exception.
    public int Code { get; }

    //
    // 摘要:
    //     Create an instance of RpcException class.
    //
    // 参数:
    //   code:
    //     The code of the exception.
    //
    //   message:
    //     The text message of the exception.
    public RpcException(int code, string message)
        : base(message)
    {
        Code = code;
    }

    //
    // 摘要:
    //     Create an instance of RpcException class with specific code.
    //
    // 参数:
    //   code:
    public RpcException(int code)
        : this(code, null)
    {
    }
}
public class RpcMethodNotFoundException : RpcException
{
    public RpcMethodNotFoundException()
        : base(404, "The method is not found.")
    {
    }
}