namespace MyMachinePlatformClientCore.Rpc.Rpc;

public class RpcReturnDataEventArgs : EventArgs
{
    //
    // 摘要:
    //     Gets the data responsed from RPC server..
    public byte[] Data { get; }

    //
    // 摘要:
    //     Create an instance of RpcReturnDataEventArgs class.
    //
    // 参数:
    //   data:
    //     The data responsed from RPC server.
    public RpcReturnDataEventArgs(byte[] data)
    {
        Data = data;
    }
}