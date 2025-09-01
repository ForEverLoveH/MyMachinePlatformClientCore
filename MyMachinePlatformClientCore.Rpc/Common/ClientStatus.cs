namespace MyMachinePlatformClientCore.Rpc.Common;

public enum ClientStatus
{
    /// <summary>
    /// Specific the client status for all kinds of clients
    /// </summary>
    
        /// <summary>
        /// Indicating a status that the client is closed.
        /// </summary>
        Closed,
        /// <summary>
        /// Indicating a status that the client is trying to connect to server.
        /// </summary>
        Connecting,
        /// <summary>
        /// Indicating a status that the client has connected to server.
        /// </summary>
        Connected
    
}