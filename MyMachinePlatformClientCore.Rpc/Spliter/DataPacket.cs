namespace MyMachinePlatformClientCore.Rpc.Spliter;

public class DataPacket
{
    /// <summary>
    /// Gets or sets the data of the packet.
    /// </summary>
    public ArraySegment<byte> Data { get; set; }

    /// <summary>
    /// Gets or sets the client id of the packet.
    /// </summary>
    public long ClientID { get; set; }
}