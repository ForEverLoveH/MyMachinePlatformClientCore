namespace MyMachinePlatformClientCore.Rpc.Tcp;

public interface ITcpClientConfig:ITcpConfig
{

}
/// <summary>
/// Represents a tcp client configuration object.
/// </summary>
internal class TcpClientConfig : TcpConfig, ITcpClientConfig
{
    /// <summary>
    /// Initializes a new instance of the TcpClientConfig. 
    /// The default SocketAsyncBufferSize is 64K, in bytes.
    /// </summary>
    public TcpClientConfig()
    {
        SocketAsyncBufferSize = 64 * 1024;
    }
}