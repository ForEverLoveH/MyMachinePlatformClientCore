using System.Net;
using MyMachinePlatformClientCore.Rpc.Common;

namespace MyMachinePlatformClientCore.Rpc.Tcp;

/// <summary>
/// Provides data for tcp ClientStatusChanged event.
/// </summary>
public class ClientStatusChangedEventArgs : EventArgs
{
    /// <summary>
    /// Create an instance of ClientStatusChangedEventArgs class.
    /// </summary>
    /// <param name="clientId">The client's id.</param>
    /// <param name="ipEndPoint">The end point where the message is from.</param>
    /// <param name="status">The client status.</param>
    public ClientStatusChangedEventArgs(long clientId, IPEndPoint ipEndPoint, ClientStatus status)
    {
        ClientID = clientId;
        IPEndPoint = ipEndPoint;
        Status = status;
    }

    /// <summary>
    /// Gets or sets the client id in long type.
    /// </summary>
    public long ClientID { get; }

    /// <summary>
    /// Gets or sets a value indicating the source IPEndPoint of the message.
    /// </summary>
    public IPEndPoint IPEndPoint { get; }

    /// <summary>
    /// Gets or sets the client status.
    /// </summary>
    /// <returns>The client status. The default is ClientStatus.Closed.</returns>
    public ClientStatus Status { get; } = ClientStatus.Closed;
}