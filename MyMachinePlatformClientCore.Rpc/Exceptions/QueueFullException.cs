namespace MyMachinePlatformClientCore.Rpc.Exceptions;

/// <summary>
/// The exception that is thrown when the queue is full
/// </summary>
public class QueueFullException : Exception
{
    public QueueFullException(string message) : base(message)
    {

    }
    public QueueFullException() { }
}