namespace MyMachinePlatformClientCore.Rpc.Tcp;

/// <summary>
/// Provides data for tcp MessageReceived event.
/// </summary>
public class MessageReceivedEventArgs : EventArgs
{
    /// <summary>
    /// Create an instance of MessageReceivedEventArgs class.
    /// </summary>
    /// <param name="clientId">The client's id</param>
    /// <param name="error">The error occurred during the transfer.</param>
    public MessageReceivedEventArgs(long clientId, Exception error)
    {
        ClientID = clientId;
        Error = error;
    }

    /// <summary>
    /// Create an instance of MessageReceivedEventArgs class.
    /// </summary>
    /// <param name="clientId">The client's id</param>
    /// <param name="messageRawData">The received message in raw data</param>
    public MessageReceivedEventArgs(long clientId, ArraySegment<byte> messageRawData)
    {
        ClientID = clientId;
        MessageRawData = messageRawData;
    }
    /// <summary>
    /// Gets or sets the id of the client where the message from.
    /// </summary>
    public long ClientID { get; }

    /// <summary>
    /// Gets or sets the message raw data that received from the network.The raw data storage may use outside buffer.
    /// </summary>
    /// <returns>The message data received from the network.</returns>
    public ArraySegment<byte> MessageRawData { get; }

    /// <summary>
    /// Gets or sets the error of the message.
    /// </summary>
    public Exception Error { get; }
}