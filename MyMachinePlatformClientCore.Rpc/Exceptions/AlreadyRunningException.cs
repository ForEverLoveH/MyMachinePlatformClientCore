namespace MyMachinePlatformClientCore.Rpc.Exceptions;

/// <summary>
/// The exception that is thrown when calling a starting function on already running task.
/// </summary>
public class AlreadyRunningException : Exception
{
    public AlreadyRunningException(string message) : base(message)
    {

    }
    public AlreadyRunningException()
    {

    }
}