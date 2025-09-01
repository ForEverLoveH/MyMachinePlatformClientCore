using System.Net;

namespace MyMachinePlatformClientCore.Rpc.Rpc;

internal class RequestResponseTask : IDisposable
{
    public Guid TaskId { get; }

    public object Result { get; set; }

    public HttpStatusCode StatusCode { get; set; }

    public string ExceptionMessage { get; set; }

    public ManualResetEvent WaitEvent { get; }

    public Type ReturnType { get; }

    public RequestResponseTask(Guid taskId, Type returnType)
    {
        WaitEvent = new ManualResetEvent(initialState: false);
        TaskId = taskId;
        ReturnType = returnType;
    }

    public void Dispose()
    {
        WaitEvent?.Close();
        WaitEvent?.Dispose();
    }
}