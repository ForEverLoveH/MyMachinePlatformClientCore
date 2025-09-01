namespace MyMachinePlatformClientCore.Rpc.Rpc;

public interface IRpcTransfer
{
    //
    // 摘要:
    //     The event will be triggered when RPC return data is received.
    event EventHandler<RpcReturnDataEventArgs> RpcReturnDataReceived;

    //
    // 摘要:
    //     Send serialized invocation data to server.
    //
    // 参数:
    //   context:
    //     The invocation context that will be used during sending.
    void SendInvocation(SendInvocationContext context);
}
//
// 摘要:
//     The interface that provides functions to send and recv RPC data.
public interface IRpcClient: IRpcTransfer
{
   
    object GetRemoteService(Type serviceType);
    TService GetRemoteService<TService>();
}