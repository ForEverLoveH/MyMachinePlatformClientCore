namespace MyMachinePlatformClientCore.Rpc.Exceptions;

public class InvalidPacketException : Exception
{
    public InvalidPacketException(string message) : base(message)
    {

    }
    public InvalidPacketException()
    {

    }
}