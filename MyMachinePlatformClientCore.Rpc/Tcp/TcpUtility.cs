using System.Net.Sockets;

namespace MyMachinePlatformClientCore.Rpc.Tcp;

/// <summary>
/// Provides common definitions and methods for tcp communication
/// </summary>
public static class TcpUtility
{
    /// <summary>
    /// A 32-bit unsigned integer that represents join group mark.
    /// </summary>
    public const UInt32 JOIN_GROUP_MARK = 0xFA9FCB89;

    /// <summary>
    /// A 32-bit unsigned integer that represents transmiting message to specific group except the sender.
    /// </summary>
    public const UInt32 GROUP_TRANSMIT_MSG_MARK = 0xBCA2BAD4;

    /// <summary>
    /// A 32-bit unsigned integer that represents transmiting message to specific group.
    /// </summary>
    public const UInt32 GROUP_TRANSMIT_MSG_LOOP_BACK_MARK = 0xECA2BAD3;

    /// <summary>
    /// Max group description length in join group messge.
    /// </summary>
    public const int MaxGroupDesLength = 1024;

    /// <summary>
    /// Set socket parameters for keep alive.
    /// </summary>
    /// <param name="socket">The socket to set keep alive parameter.</param>
    /// <param name="keepAliveTime">The keep alive time.</param>
    /// <param name="keepAliveInterval">The keep alive interval.</param>
    public static void SetKeepAlive(Socket socket, uint keepAliveTime, uint keepAliveInterval)
    {
        //the following code is not supported in linux ,so try catch is used here.
        try
        {
            byte[] inValue = new byte[12];
            Buffer.BlockCopy(BitConverter.GetBytes((int)1), 0, inValue, 0, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(keepAliveTime), 0, inValue, 4, 4);
            Buffer.BlockCopy(BitConverter.GetBytes(keepAliveInterval), 0, inValue, 8, 4);
            socket.IOControl(IOControlCode.KeepAliveValues, inValue, null);
        }
        catch
        {
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        }
    }
}