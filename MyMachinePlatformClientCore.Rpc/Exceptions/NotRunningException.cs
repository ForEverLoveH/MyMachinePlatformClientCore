namespace MyMachinePlatformClientCore.Rpc.Exceptions;

/// <summary>
/// The exception that is thrown when doing some opertions on not running task.
/// </summary>
public class NotRunningException : Exception
{
    public NotRunningException(string message) : base(message)
    {

    }
    public NotRunningException()
    {

    }
}